using System.Globalization;
using System.IO.Compression;
using System.Xml;
using Kharbarchi.Server.Data;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Services;

public sealed class BarokCustomerImportService
{
    private const int MaxRows = 250_000;
    private const int SaveBatchSize = 1_000;
    private readonly AppDbContext _context;

    public BarokCustomerImportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerImportResultDto> ImportAsync(
        Stream customerWorkbook,
        Stream creditWorkbook,
        string userName,
        CancellationToken cancellationToken)
    {
        var customerResult = await ImportCustomerDetailsAsync(customerWorkbook, CustomerTypes.Legal, userName, cancellationToken);
        var creditResult = await ImportCreditsAsync(creditWorkbook, CustomerTypes.Legal, userName, cancellationToken);
        return new CustomerImportResultDto(
            customerResult.CustomerRows,
            creditResult.CreditRows,
            customerResult.Inserted,
            customerResult.Updated + creditResult.Updated,
            creditResult.CreditChanges,
            customerResult.Errors.Concat(creditResult.Errors).Take(200).ToArray());
    }

    public async Task<CustomerImportResultDto> ImportCustomerDetailsAsync(
        Stream customerWorkbook,
        string customerType,
        string userName,
        CancellationToken cancellationToken)
    {
        var customerRows = ReadFirstSheet(customerWorkbook);
        var errors = new List<string>();

        var isIndividual = string.Equals(customerType, CustomerTypes.Individual, StringComparison.Ordinal);
        var keyHeader = isIndividual ? "کد ملی پذیرنده" : "شناسه یکتای شرکت";
        var importedLegalIds = customerRows
            .Select(x => DigitsOnly(Get(x, keyHeader)))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        // MySql.EntityFrameworkCore cannot assign a relational type mapping to a
        // parameterized primitive collection used by Enumerable.Contains. Load
        // legal customers as tracked entities and apply the imported-ID set in
        // memory so updates remain provider-compatible and persist normally.
        var importedLegalIdSet = importedLegalIds.ToHashSet(StringComparer.Ordinal);
        var existingLegalCustomers = await _context.Customers
            .Where(x => isIndividual ? x.NationalCode != null : x.LegalEntityId != null)
            .ToListAsync(cancellationToken);
        var existing = existingLegalCustomers
            .Where(x => importedLegalIdSet.Contains(isIndividual ? x.NationalCode! : x.LegalEntityId!))
            .ToDictionary(x => isIndividual ? x.NationalCode! : x.LegalEntityId!, StringComparer.Ordinal);

        var inserted = 0;
        var updated = 0;
        var now = DateTime.UtcNow;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        foreach (var row in customerRows)
        {
            var legalId = DigitsOnly(Get(row, keyHeader));
            var firstName = isIndividual ? NormalizeText(Get(row, "نام پذیرنده")) : string.Empty;
            var lastName = isIndividual ? NormalizeText(Get(row, "نام خانوادگی پذیرنده")) : string.Empty;
            var name = isIndividual ? NormalizeText($"{firstName} {lastName}") : NormalizeText(Get(row, "نام شرکت"));
            var phone = DigitsOnly(Get(row, "شماره موبایل پذیرنده"));
            if (string.IsNullOrWhiteSpace(legalId) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                errors.Add($"ردیف مشتری «{name}» به علت نداشتن نام، شناسه شرکت یا موبایل رد شد.");
                continue;
            }

            var isNew = !existing.TryGetValue(legalId, out var customer);
            customer ??= new Customer
            {
                CreatedAtUtc = now
            };

            customer.CustomerType = isIndividual ? CustomerTypes.Individual : CustomerTypes.Legal;
            customer.IsLegalEntity = !isIndividual;
            customer.LegalEntityId = isIndividual ? null : legalId;
            customer.NationalCode = isIndividual ? legalId : null;
            customer.FirstName = isIndividual ? firstName : null;
            customer.LastName = isIndividual ? lastName : null;
            customer.FullName = name;
            customer.PhoneNumber = phone;
            customer.AddressLine = NormalizeText(Get(row, "آدرس"));
            customer.City = isIndividual ? NormalizeText(Get(row, "شهر")) : customer.City ?? string.Empty;
            customer.PostalCode = isIndividual ? DigitsOnly(Get(row, "کد پستی")) : customer.PostalCode ?? string.Empty;
            customer.StoreName = isIndividual ? NullIfEmpty(Get(row, "نام فروشگاه")) : null;
            customer.Province = isIndividual ? NullIfEmpty(Get(row, "استان")) : null;
            customer.BusinessCategory = isIndividual ? NullIfEmpty(Get(row, "صنف")) : null;
            customer.DistributionStatus = NullIfEmpty(Get(row, "توسط پخش تعریف شده"));
            customer.IsDefinedByDistribution = IsYes(Get(row, "توسط پخش تعریف شده"));
            customer.CreditReceivedAtUtc = ParseExcelDate(Get(row, "تاریخ دریافت اعتبار"));
            customer.CreditExpiresAtUtc = ParseExcelDate(Get(row, "تاریخ منقضی شدن اعتبار"));
            if (isIndividual) customer.CreditPlanTitle = NullIfEmpty(Get(row, "عنوان طرح"));
            customer.IsActive = true;
            customer.LastImportedAtUtc = now;
            customer.UpdatedAtUtc = now;

            if (isNew)
            {
                _context.Customers.Add(customer);
                existing[legalId] = customer;
                inserted++;
            }
            else
            {
                updated++;
            }

            if ((inserted + updated) % SaveBatchSize == 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CustomerImportResultDto(customerRows.Count, 0, inserted, updated, 0, errors.Take(200).ToArray());
    }

    public async Task<CustomerImportResultDto> ImportCreditsAsync(
        Stream creditWorkbook,
        string customerType,
        string userName,
        CancellationToken cancellationToken)
    {
        var creditRows = ReadFirstSheet(creditWorkbook);
        var errors = new List<string>();
        var isIndividual = string.Equals(customerType, CustomerTypes.Individual, StringComparison.Ordinal);
        var keyHeader = isIndividual ? "کد ملی پذیرنده" : "شناسه یکتای شرکت";
        var existing = (await _context.Customers
                .Where(x => isIndividual ? x.NationalCode != null : x.LegalEntityId != null)
                .ToListAsync(cancellationToken))
            .ToDictionary(x => isIndividual ? x.NationalCode! : x.LegalEntityId!, StringComparer.Ordinal);
        var updated = 0;
        var creditChanges = 0;
        var processed = 0;
        var now = DateTime.UtcNow;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        foreach (var row in creditRows)
        {
            processed++;
            var legalId = DigitsOnly(Get(row, keyHeader));
            if (string.IsNullOrWhiteSpace(legalId))
            {
                errors.Add("یک ردیف اعتبار به علت نداشتن شناسه یکتای شرکت رد شد.");
                continue;
            }
            if (!existing.TryGetValue(legalId, out var customer))
            {
                errors.Add($"اعتبار شرکت {legalId} وارد نشد؛ ابتدا باید اطلاعات این مشتری در مرحله ۱ ثبت شود.");
                continue;
            }
            var creditHeader = isIndividual ? "اعتبار کل (ریال)" : "اعتبار تخصیص داده شده";
            if (!decimal.TryParse(NormalizeSignedNumber(Get(row, creditHeader)), NumberStyles.Number, CultureInfo.InvariantCulture, out var limit) || limit < 0)
            {
                errors.Add($"اعتبار شرکت {legalId} عدد معتبر و غیرمنفی نیست.");
                continue;
            }

            var blocked = IsYes(Get(row, "آیا پذیرنده مسدود است؟"));
            if (customer.CreditLimit != limit || customer.IsCreditBlocked != blocked)
            {
                customer.CreditHistory.Add(new CustomerCreditHistory
                {
                    PreviousCreditLimit = customer.CreditLimit,
                    NewCreditLimit = limit,
                    PreviousBlocked = customer.IsCreditBlocked,
                    NewBlocked = blocked,
                    Source = "BarokCreditExcel",
                    ChangedByUserName = userName,
                    ChangedAtUtc = now
                });
                creditChanges++;
            }
            customer.CreditLimit = limit;
            if (isIndividual)
            {
                customer.SourceRemainingCredit = ParseNullableDecimal(Get(row, "مانده اعتبار (ریال)"));
                customer.AllowedSpending = ParseNullableDecimal(Get(row, "مقدار مجاز خرجکرد"));
                customer.UsedCredit = customer.SourceRemainingCredit.HasValue ? limit - customer.SourceRemainingCredit.Value : customer.UsedCredit;
            }
            customer.IsCreditBlocked = blocked;
            customer.CreditPlanTitle = NullIfEmpty(Get(row, "عنوان طرح"));
            customer.LastImportedAtUtc = now;
            customer.UpdatedAtUtc = now;
            updated++;

            if (processed % SaveBatchSize == 0)
                await _context.SaveChangesAsync(cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new CustomerImportResultDto(0, creditRows.Count, 0, updated, creditChanges, errors.Take(200).ToArray());
    }

    private static List<Dictionary<string, string>> ReadFirstSheet(Stream input)
    {
        using var archive = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: true);
        var sharedStrings = ReadSharedStrings(archive);
        var worksheet = archive.GetEntry("xl/worksheets/sheet1.xml")
            ?? throw new InvalidDataException("فایل Excel فاقد شیت اول است.");

        if (worksheet.Length > 250_000_000)
        {
            throw new InvalidDataException("اندازه داده‌های شیت بیش از حد مجاز است.");
        }

        var rawRows = new List<Dictionary<int, string>>();
        using var stream = worksheet.Open();
        using var reader = XmlReader.Create(stream, SecureXmlSettings());
        Dictionary<int, string>? currentRow = null;
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "row")
            {
                currentRow = [];
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "c" && currentRow is not null)
            {
                var reference = reader.GetAttribute("r") ?? string.Empty;
                var type = reader.GetAttribute("t");
                var column = ColumnIndex(reference);
                var value = ReadCell(reader.ReadSubtree(), type, sharedStrings);
                currentRow[column] = value;
            }
            else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "row" && currentRow is not null)
            {
                rawRows.Add(currentRow);
                if (rawRows.Count > MaxRows + 1)
                {
                    throw new InvalidDataException($"تعداد ردیف‌ها بیش از حد مجاز {MaxRows:N0} است.");
                }
                currentRow = null;
            }
        }

        if (rawRows.Count == 0)
        {
            return [];
        }

        var headers = rawRows[0].ToDictionary(x => x.Key, x => NormalizeText(x.Value));
        return rawRows.Skip(1)
            .Where(row => row.Values.Any(value => !string.IsNullOrWhiteSpace(value)))
            .Select(row => headers.ToDictionary(x => x.Value, x => row.GetValueOrDefault(x.Key, string.Empty), StringComparer.Ordinal))
            .ToList();
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return [];
        }

        var result = new List<string>();
        using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, SecureXmlSettings());
        while (reader.ReadToFollowing("si", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"))
        {
            using var subtree = reader.ReadSubtree();
            var parts = new List<string>();
            while (subtree.Read())
            {
                if (subtree.NodeType == XmlNodeType.Element && subtree.LocalName == "t")
                {
                    parts.Add(subtree.ReadElementContentAsString());
                }
            }
            result.Add(string.Concat(parts));
        }
        return result;
    }

    private static string ReadCell(XmlReader reader, string? type, IReadOnlyList<string> sharedStrings)
    {
        string value = string.Empty;
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && (reader.LocalName == "v" || reader.LocalName == "t"))
            {
                value += reader.ReadElementContentAsString();
            }
        }
        if (type == "s" && int.TryParse(value, out var index) && index >= 0 && index < sharedStrings.Count)
        {
            return sharedStrings[index];
        }
        return value;
    }

    private static XmlReaderSettings SecureXmlSettings() => new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreWhitespace = true
    };

    private static int ColumnIndex(string reference)
    {
        var result = 0;
        foreach (var character in reference.TakeWhile(char.IsLetter))
        {
            result = result * 26 + (char.ToUpperInvariant(character) - 'A' + 1);
        }
        return Math.Max(0, result - 1);
    }

    private static string Get(IReadOnlyDictionary<string, string> row, string name) => row.GetValueOrDefault(name, string.Empty);
    private static string NormalizeText(string? value) => (value ?? string.Empty).Replace('\u064a', '\u06cc').Replace('\u0643', '\u06a9').Trim();
    private static string NormalizeNumber(string? value) => DigitsOnly(value);
    private static string NormalizeSignedNumber(string? value)
    {
        var normalized = NormalizeText(value);
        var digits = DigitsOnly(normalized);
        return normalized.StartsWith('-') ? "-" + digits : digits;
    }
    private static decimal? ParseNullableDecimal(string? value)
        => decimal.TryParse(NormalizeSignedNumber(value), NumberStyles.Number, CultureInfo.InvariantCulture, out var number) ? number : null;
    private static string DigitsOnly(string? value)
    {
        var text = NormalizeText(value);
        var builder = new System.Text.StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (c is >= '0' and <= '9') builder.Append(c);
            else if (c is >= '\u06f0' and <= '\u06f9') builder.Append((char)('0' + c - '\u06f0'));
            else if (c is >= '\u0660' and <= '\u0669') builder.Append((char)('0' + c - '\u0660'));
        }
        return builder.ToString();
    }
    private static bool IsYes(string? value) => NormalizeText(value) is "بله" or "بلی" or "yes" or "Yes" or "1";
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(NormalizeText(value)) ? null : NormalizeText(value);
    private static DateTime? ParseExcelDate(string? value)
    {
        var normalized = NormalizeText(value);
        if (string.IsNullOrWhiteSpace(normalized)) return null;
        if (double.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var serial) && serial > 0)
            return DateTime.FromOADate(serial).ToUniversalTime();
        var parts = normalized.Split('/', '-', '.');
        if (parts.Length == 3 && int.TryParse(DigitsOnly(parts[0]), out var year) && int.TryParse(DigitsOnly(parts[1]), out var month) && int.TryParse(DigitsOnly(parts[2]), out var day) && year < 1700)
        {
            try { return DateTime.SpecifyKind(new PersianCalendar().ToDateTime(year, month, day, 0, 0, 0, 0), DateTimeKind.Local).ToUniversalTime(); }
            catch (ArgumentOutOfRangeException) { return null; }
        }
        return DateTime.TryParse(normalized, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date) ? date.ToUniversalTime() : null;
    }

    private sealed record CreditImportRow(decimal Limit, bool IsBlocked, string? PlanTitle);
}

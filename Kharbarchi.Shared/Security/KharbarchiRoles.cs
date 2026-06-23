namespace Kharbarchi.Shared.Security;

public static class KharbarchiRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string PricingManager = "PricingManager";
    public const string PricingEmployee = "PricingEmployee";
    public const string WarehouseEmployee = "WarehouseEmployee";
    public const string SalesManager = "SalesManager";
    public const string ShippingOrderManager = "ShippingOrderManager";
    public const string Accountant = "Accountant";
    public const string CentralSyncAgent = "CentralSyncAgent";
    public const string GatewayAdmin = "GatewayAdmin";
    public const string Customer = "Customer";

    // Backward compatibility with older code. New code should prefer SuperAdmin.
    public const string LegacyAdmin = "Admin";

    public static readonly string[] AllSystemRoles =
    [
        SuperAdmin,
        PricingManager,
        PricingEmployee,
        WarehouseEmployee,
        SalesManager,
        ShippingOrderManager,
        Accountant,
        CentralSyncAgent,
        GatewayAdmin,
        Customer,
        LegacyAdmin
    ];

    public static readonly string[] AssignableInternalRoles =
    [
        SuperAdmin,
        PricingManager,
        PricingEmployee,
        WarehouseEmployee,
        SalesManager,
        ShippingOrderManager,
        Accountant,
        CentralSyncAgent,
        GatewayAdmin
    ];

    public static readonly string[] ProductViewers =
    [
        SuperAdmin,
        LegacyAdmin,
        PricingManager,
        PricingEmployee,
        SalesManager,
        WarehouseEmployee
    ];

    public static readonly string[] OrderViewers =
    [
        SuperAdmin,
        LegacyAdmin,
        PricingManager,
        SalesManager,
        ShippingOrderManager,
        Accountant
    ];

    public static readonly string[] OrderPaymentOperators =
    [
        SuperAdmin,
        LegacyAdmin,
        ShippingOrderManager,
        Accountant
    ];

    public static readonly string[] BarookPaymentOperators =
    [
        SuperAdmin,
        LegacyAdmin,
        ShippingOrderManager
    ];

    public const string SuperAdmins = SuperAdmin + "," + LegacyAdmin;
    public const string CatalogWriters = SuperAdmin + "," + LegacyAdmin + "," + PricingManager;
    public const string PriceProposers = SuperAdmin + "," + LegacyAdmin + "," + PricingManager + "," + PricingEmployee;
    public const string PriceApprovers = SuperAdmin + "," + LegacyAdmin + "," + PricingManager;
    public const string PurchasePriceReaders = SuperAdmin + "," + LegacyAdmin + "," + PricingManager;
    public const string StockReaders = SuperAdmin + "," + LegacyAdmin + "," + PricingManager + "," + WarehouseEmployee;
    public const string InventoryProposers = SuperAdmin + "," + LegacyAdmin + "," + PricingManager + "," + WarehouseEmployee;
    public const string OrderViewersCsv = SuperAdmin + "," + LegacyAdmin + "," + PricingManager + "," + SalesManager + "," + ShippingOrderManager + "," + Accountant;
    public const string PaymentOperatorsCsv = SuperAdmin + "," + LegacyAdmin + "," + ShippingOrderManager + "," + Accountant;
    public const string BarookPaymentOperatorsCsv = SuperAdmin + "," + LegacyAdmin + "," + ShippingOrderManager;
    public const string SyncOperatorsCsv = SuperAdmin + "," + LegacyAdmin + "," + CentralSyncAgent;

    public static readonly IReadOnlyList<KharbarchiRoleDescriptor> InternalRoleCatalog =
    [
        new(SuperAdmin, "مدیر کل", "دسترسی کامل به همه بخش‌ها، ساخت کاربر، تخصیص نقش و تنظیمات اصلی."),
        new(PricingManager, "مدیر قیمت‌گذاری", "تایید قیمت، مشاهده قیمت خرید، مدیریت کاتالوگ و تایید مرحله اول موجودی."),
        new(PricingEmployee, "کارشناس قیمت‌گذاری", "ثبت پیشنهاد قیمت فروش بدون دسترسی به قیمت خرید."),
        new(WarehouseEmployee, "انباردار", "مشاهده موجودی و ثبت پیشنهاد کسری، افزایش یا اصلاح موجودی بدون مشاهده قیمت‌ها."),
        new(SalesManager, "مدیر فروش", "مشاهده سفارش‌ها، وضعیت سفارش، مشتری و محصولات قابل فروش."),
        new(ShippingOrderManager, "مسئول ارسال و سفارش", "تغییر وضعیت سفارش، ارسال لینک پرداخت باروک و آماده‌سازی ارسال."),
        new(Accountant, "حسابدار", "مشاهده سفارش‌ها، رسیدهای پرداخت و ثبت دریافت دستی."),
        new(CentralSyncAgent, "عامل همگام‌سازی مرکزی", "اجرای صف خروجی و همگام‌سازی امن با ووکامرس."),
        new(GatewayAdmin, "کاربر درگاه", "کاربر فنی برای ثبت/دریافت وضعیت پرداخت از سرویس درگاه."),
    ];

    public static string GetDisplayName(string role)
    {
        return InternalRoleCatalog.FirstOrDefault(x => string.Equals(x.Name, role, StringComparison.OrdinalIgnoreCase))?.PersianName
            ?? (string.Equals(role, Customer, StringComparison.OrdinalIgnoreCase) ? "مشتری" : role);
    }

    public static string GetDescription(string role)
    {
        return InternalRoleCatalog.FirstOrDefault(x => string.Equals(x.Name, role, StringComparison.OrdinalIgnoreCase))?.Description
            ?? string.Empty;
    }

    public static bool IsAssignableInternalRole(string role)
    {
        return AssignableInternalRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}

public sealed record KharbarchiRoleDescriptor(string Name, string PersianName, string Description);

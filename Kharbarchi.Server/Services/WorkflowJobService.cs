using Kharbarchi.Server.Data;
using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Contracts.ProductWorkflow;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Services;

public sealed class WorkflowJobService
{
    private readonly AppDbContext _context;

    public WorkflowJobService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowJobDto> CreateAsync(string type, string? createdBy, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var job = new KhbWorkflowJob
        {
            JobId = Guid.NewGuid(),
            Type = NormalizeType(type),
            Status = "pending",
            CurrentStep = "Pending",
            Message = "Job created.",
            CreatedBy = createdBy,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _context.KhbWorkflowJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(job);
    }

    public async Task<KhbWorkflowJob> RequireAsync(Guid jobId, string expectedType, CancellationToken cancellationToken)
    {
        var job = await _context.KhbWorkflowJobs.SingleOrDefaultAsync(x => x.JobId == jobId, cancellationToken)
            ?? throw new KeyNotFoundException($"Workflow job '{jobId}' was not found.");

        if (!string.Equals(job.Type, NormalizeType(expectedType), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Workflow job '{jobId}' is type '{job.Type}', not '{expectedType}'.");
        }

        return job;
    }

    public async Task StartAsync(KhbWorkflowJob job, string step, int totalItems, string? message, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        job.Status = "running";
        job.CurrentStep = step;
        job.TotalItems = Math.Max(0, totalItems);
        job.ProcessedItems = 0;
        job.SuccessCount = 0;
        job.ErrorCount = 0;
        job.DraftCount = 0;
        job.SkippedCount = 0;
        job.PendingCount = Math.Max(0, totalItems);
        job.ProgressPercent = 0;
        job.Message = message;
        job.StartedAtUtc ??= now;
        job.FinishedAtUtc = null;
        job.UpdatedAtUtc = now;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        KhbWorkflowJob job,
        string step,
        int processed,
        int total,
        int success,
        int errors,
        int drafts,
        int skipped,
        string? message,
        CancellationToken cancellationToken)
    {
        job.Status = "running";
        job.CurrentStep = step;
        job.TotalItems = Math.Max(0, total);
        job.ProcessedItems = Math.Clamp(processed, 0, Math.Max(processed, total));
        job.SuccessCount = Math.Max(0, success);
        job.ErrorCount = Math.Max(0, errors);
        job.DraftCount = Math.Max(0, drafts);
        job.SkippedCount = Math.Max(0, skipped);
        job.PendingCount = Math.Max(0, total - processed);
        job.ProgressPercent = total <= 0 ? 0 : Math.Clamp((int)Math.Round(processed * 100d / total), 0, 100);
        job.Message = message;
        job.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(KhbWorkflowJob job, string message, CancellationToken cancellationToken)
    {
        job.Status = job.ErrorCount > 0 ? "completed_with_errors" : "completed";
        job.CurrentStep = "Completed";
        job.ProcessedItems = Math.Max(job.ProcessedItems, job.TotalItems);
        job.PendingCount = 0;
        job.ProgressPercent = 100;
        job.Message = message;
        job.FinishedAtUtc = DateTime.UtcNow;
        job.UpdatedAtUtc = job.FinishedAtUtc.Value;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetPhaseAsync(
        KhbWorkflowJob job,
        string step,
        int progressPercent,
        int totalItems,
        int processedItems,
        int successCount,
        int errorCount,
        int draftCount,
        int skippedCount,
        string? message,
        CancellationToken cancellationToken)
    {
        job.Status = "running";
        job.CurrentStep = step;
        job.TotalItems = Math.Max(0, totalItems);
        job.ProcessedItems = Math.Max(0, processedItems);
        job.SuccessCount = Math.Max(0, successCount);
        job.ErrorCount = Math.Max(0, errorCount);
        job.DraftCount = Math.Max(0, draftCount);
        job.SkippedCount = Math.Max(0, skippedCount);
        job.PendingCount = Math.Max(0, totalItems - processedItems);
        job.ProgressPercent = Math.Clamp(progressPercent, 0, 100);
        job.Message = message;
        job.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task FailAsync(KhbWorkflowJob job, string step, Exception exception, CancellationToken cancellationToken)
    {
        job.Status = "failed";
        job.CurrentStep = step;
        job.ErrorCount++;
        job.Message = Trim(exception.Message, 2000);
        job.FinishedAtUtc = DateTime.UtcNow;
        job.UpdatedAtUtc = job.FinishedAtUtc.Value;
        await AddLogAsync(job, step, null, null, null, "error", exception.Message, null, null, null, cancellationToken);
    }

    public async Task AddLogAsync(
        KhbWorkflowJob job,
        string step,
        string? entityType,
        string? entityId,
        string? sku,
        string status,
        string? message,
        string? requestUrl,
        int? responseCode,
        string? responseBody,
        CancellationToken cancellationToken)
    {
        _context.KhbWorkflowJobLogs.Add(new KhbWorkflowJobLog
        {
            WorkflowJobId = job.Id,
            JobId = job.JobId,
            StepName = Trim(step, 160) ?? "Unknown",
            EntityType = Trim(entityType, 100),
            EntityId = Trim(entityId, 191),
            Sku = Trim(sku, 191),
            Status = Trim(status, 50) ?? "info",
            Message = Trim(message, 4000),
            RequestUrl = SanitizeUrl(requestUrl),
            ResponseCode = responseCode,
            ResponseBodySummary = Trim(responseBody, 4000),
            CreatedAtUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<WorkflowJobDto?> GetAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _context.KhbWorkflowJobs.AsNoTracking().SingleOrDefaultAsync(x => x.JobId == jobId, cancellationToken);
        return job is null ? null : ToDto(job);
    }

    public async Task<IReadOnlyList<WorkflowJobLogDto>> GetLogsAsync(Guid jobId, int take, CancellationToken cancellationToken)
    {
        take = Math.Clamp(take, 1, 500);
        return await _context.KhbWorkflowJobLogs
            .AsNoTracking()
            .Where(x => x.JobId == jobId)
            .OrderByDescending(x => x.Id)
            .Take(take)
            .Select(x => new WorkflowJobLogDto
            {
                Id = x.Id,
                JobId = x.JobId,
                StepName = x.StepName,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                Sku = x.Sku,
                Status = x.Status,
                Message = x.Message,
                RequestUrl = x.RequestUrl,
                ResponseCode = x.ResponseCode,
                ResponseBodySummary = x.ResponseBodySummary,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowJobDto?> GetLatestAsync(string? type, CancellationToken cancellationToken)
    {
        var normalized = string.IsNullOrWhiteSpace(type) ? null : NormalizeType(type);
        var query = _context.KhbWorkflowJobs.AsNoTracking();
        if (normalized is not null)
        {
            query = query.Where(x => x.Type == normalized);
        }

        var job = await query.OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        return job is null ? null : ToDto(job);
    }

    public static WorkflowJobDto ToDto(KhbWorkflowJob job) => new()
    {
        JobId = job.JobId,
        Type = job.Type,
        Status = job.Status,
        CurrentStep = job.CurrentStep,
        TotalItems = job.TotalItems,
        ProcessedItems = job.ProcessedItems,
        SuccessCount = job.SuccessCount,
        ErrorCount = job.ErrorCount,
        DraftCount = job.DraftCount,
        SkippedCount = job.SkippedCount,
        PendingCount = job.PendingCount,
        ProgressPercent = job.ProgressPercent,
        Message = job.Message,
        StartedAtUtc = job.StartedAtUtc,
        FinishedAtUtc = job.FinishedAtUtc,
        CreatedAtUtc = job.CreatedAtUtc,
        UpdatedAtUtc = job.UpdatedAtUtc
    };

    private static string NormalizeType(string type)
    {
        return (type ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "import" or "csv_import" => "import",
            "process" or "processing" => "process",
            "sync" or "woocommerce_sync" => "sync",
            _ => throw new ArgumentException("Job type must be import, process, or sync.", nameof(type))
        };
    }

    private static string? SanitizeUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var safe = value;
        var question = safe.IndexOf('?');
        if (question >= 0)
        {
            safe = safe[..question];
        }

        return Trim(safe, 2000);
    }

    private static string? Trim(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}

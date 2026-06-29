using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;
using Ols.ControlCenter.Shared.Pagination;

namespace Ols.ControlCenter.Application.Features.Tasks;

public sealed class TaskListRequest : PagedRequest
{
    public string? Status { get; set; }
    public long? OwnerUserId { get; set; }
    public long? OperationId { get; set; }
}

public sealed record TaskDto(
    long Id, string Title, long? OperationId, string? OperationNo, string? OwnerName, string? DepartmentName,
    string Priority, DateOnly? DueDate, string Status, string? Description, DateTimeOffset CreatedAt);

public sealed class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public long? OperationId { get; set; }
    public long? OwnerUserId { get; set; }
    public long? DepartmentId { get; set; }
    public string Priority { get; set; } = "Normal";
    public DateOnly? DueDate { get; set; }
    public string? Description { get; set; }
}

public sealed class CompleteTaskRequest
{
    public string? Note { get; set; }
}

public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetListAsync(TaskListRequest req, CancellationToken ct);
    Task<TaskDto> CreateAsync(CreateTaskRequest req, long? userId, CancellationToken ct);
    Task<bool> CompleteAsync(long id, string? note, long? userId, CancellationToken ct);
    Task<TaskDto?> CreateFromAlertAsync(long alertId, long? userId, CancellationToken ct);
}

public sealed class TaskService : ITaskService
{
    private readonly IApplicationDbContext _db;

    public TaskService(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<TaskDto>> GetListAsync(TaskListRequest req, CancellationToken ct)
    {
        var q = _db.WorkTasks.AsNoTracking();
        if (Enum.TryParse<WorkTaskStatus>(req.Status, ignoreCase: true, out var st)) q = q.Where(t => t.Status == st);
        if (req.OwnerUserId is { } owner) q = q.Where(t => t.OwnerUserId == owner);
        if (req.OperationId is { } op) q = q.Where(t => t.OperationId == op);

        var total = await q.CountAsync(ct);
        var raw = await q
            .OrderByDescending(t => t.CreatedAt)
            .Skip(req.Skip).Take(req.PageSize)
            .Select(t => new
            {
                t.Id, t.Title, t.OperationId,
                OpNo = t.Operation != null ? t.Operation.SourceOperationNo : null,
                Owner = t.Owner != null ? t.Owner.FullName : null,
                Dept = t.Department != null ? t.Department.Name : null,
                t.Priority, t.DueDate, t.Status, t.Description, t.CreatedAt,
            })
            .ToListAsync(ct);

        var items = raw.Select(t => new TaskDto(
            t.Id, t.Title, t.OperationId, t.OpNo, t.Owner, t.Dept,
            t.Priority.ToString(), t.DueDate, t.Status.ToString(), t.Description, t.CreatedAt)).ToList();
        return new PagedResult<TaskDto>(items, total, req.Page, req.PageSize);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest req, long? userId, CancellationToken ct)
    {
        var task = new WorkTask
        {
            Title = req.Title,
            OperationId = req.OperationId,
            OwnerUserId = req.OwnerUserId,
            DepartmentId = req.DepartmentId,
            Priority = Enum.TryParse<TaskPriority>(req.Priority, ignoreCase: true, out var p) ? p : TaskPriority.Normal,
            DueDate = req.DueDate,
            Description = req.Description,
            Status = WorkTaskStatus.New,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = userId,
        };
        _db.WorkTasks.Add(task);
        await _db.SaveChangesAsync(ct);
        return Map(task, null, null, null);
    }

    public async Task<bool> CompleteAsync(long id, string? note, long? userId, CancellationToken ct)
    {
        var task = await _db.WorkTasks.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (task is null) return false;
        task.Status = WorkTaskStatus.Completed;
        task.CompletionNote = note;
        task.CompletedAt = DateTimeOffset.UtcNow;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        task.UpdatedByUserId = userId;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<TaskDto?> CreateFromAlertAsync(long alertId, long? userId, CancellationToken ct)
    {
        var alert = await _db.Alerts.Include(a => a.Operation).FirstOrDefaultAsync(a => a.Id == alertId, ct);
        if (alert is null) return null;

        var priority = alert.RiskLevel switch
        {
            RiskLevel.Red or RiskLevel.Black => TaskPriority.Critical,
            RiskLevel.Orange => TaskPriority.High,
            _ => TaskPriority.Normal,
        };

        // Alert ya eski Operation modelinden ya da bir takip tablosu (board) satırından gelir.
        var label = alert.Operation is not null
            ? alert.Operation.SourceOperationNo ?? $"#{alert.OperationId}"
            : $"{alert.BoardTitle} · {alert.RecordRef}";

        var task = new WorkTask
        {
            Title = $"{label} — {alert.Description}",
            OperationId = alert.OperationId,
            OwnerUserId = alert.ResponsibleUserId,
            Priority = priority,
            Description = alert.Description,
            Status = WorkTaskStatus.New,
            SourceAlertId = alertId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = userId,
        };
        _db.WorkTasks.Add(task);
        alert.Status = AlertStatus.TaskCreated;
        await _db.SaveChangesAsync(ct);
        return Map(task, alert.Operation?.SourceOperationNo, null, null);
    }

    private static TaskDto Map(WorkTask t, string? opNo, string? owner, string? dept) => new(
        t.Id, t.Title, t.OperationId, opNo, owner, dept,
        t.Priority.ToString(), t.DueDate, t.Status.ToString(), t.Description, t.CreatedAt);
}

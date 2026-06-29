using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Domain.Enums;
using Ols.ControlCenter.Shared.Pagination;

namespace Ols.ControlCenter.Application.Features.Alerts;

public interface IAlertService
{
    Task<PagedResult<AlertDto>> GetListAsync(AlertListRequest req, CancellationToken ct);
    Task<int> CountOpenAsync(CancellationToken ct);
    Task<bool> ResolveAsync(long id, string? note, long? userId, CancellationToken ct);
}

public sealed class AlertService : IAlertService
{
    private readonly IApplicationDbContext _db;

    public AlertService(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<AlertDto>> GetListAsync(AlertListRequest req, CancellationToken ct)
    {
        var q = _db.Alerts.AsNoTracking();

        if (Enum.TryParse<AlertStatus>(req.Status, ignoreCase: true, out var st))
            q = q.Where(a => a.Status == st);
        else
            q = q.Where(a => a.Status != AlertStatus.Resolved && a.Status != AlertStatus.Dismissed);

        if (Enum.TryParse<RiskLevel>(req.Risk, ignoreCase: true, out var rl))
            q = q.Where(a => a.RiskLevel == rl);
        if (Enum.TryParse<AlertType>(req.Type, ignoreCase: true, out var at))
            q = q.Where(a => a.Type == at);
        if (!string.IsNullOrWhiteSpace(req.Group))
            q = q.Where(a => a.Group == req.Group);

        var total = await q.CountAsync(ct);

        var raw = await q
            .OrderByDescending(a => a.LastTriggeredAt)
            .Skip(req.Skip).Take(req.PageSize)
            .Select(a => new
            {
                a.Id, a.OperationId,
                OpNo = a.Operation != null ? a.Operation.SourceOperationNo : null,
                Customer = a.Operation != null ? a.Operation.CustomerName : null,
                a.BoardKey, a.BoardTitle, a.Group, a.RecordRef,
                a.Type, a.RiskLevel, a.RuleCode, a.Description, a.Status, a.Deadline,
                Resp = a.ResponsibleUser != null ? a.ResponsibleUser.FullName : null,
                a.LastTriggeredAt, a.TriggerCount,
            })
            .ToListAsync(ct);

        var items = raw.Select(a => new AlertDto(
            a.Id, a.OperationId, a.OpNo, a.Customer, a.BoardKey, a.BoardTitle, a.Group, a.RecordRef,
            a.Type.ToString(), a.RiskLevel.ToString(),
            a.RuleCode, a.Description, a.Status.ToString(), a.Deadline, a.Resp, a.LastTriggeredAt, a.TriggerCount)).ToList();

        return new PagedResult<AlertDto>(items, total, req.Page, req.PageSize);
    }

    public Task<int> CountOpenAsync(CancellationToken ct)
        => _db.Alerts.CountAsync(a => a.Status != AlertStatus.Resolved && a.Status != AlertStatus.Dismissed, ct);

    public async Task<bool> ResolveAsync(long id, string? note, long? userId, CancellationToken ct)
    {
        var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (alert is null) return false;
        alert.Status = AlertStatus.Resolved;
        alert.ResolvedAt = DateTimeOffset.UtcNow;
        alert.ResolvedByUserId = userId;
        alert.ResolutionNote = note;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

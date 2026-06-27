using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Dashboard;

public interface IDashboardQueryService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct);
}

public sealed class DashboardQueryService : IDashboardQueryService
{
    private readonly IApplicationDbContext _db;

    public DashboardQueryService(IApplicationDbContext db) => _db = db;

    private sealed record Row(
        OperationStatus Status, RiskLevel Risk, TransportType Transport, DocumentStatus DocStatus,
        DateOnly? LoadingDate, DateOnly? PlannedDeliveryDate, DateOnly? DeliveryDate,
        int DelayDays, DateTimeOffset CreatedAt, string DepartmentName, bool CustomerCritical);

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rows = await _db.Operations.AsNoTracking()
            .Select(o => new Row(
                o.Status, o.RiskLevel, o.TransportType, o.DocumentStatus,
                o.LoadingDate, o.PlannedDeliveryDate, o.DeliveryDate, o.DelayDays, o.CreatedAt,
                o.Department != null ? o.Department.Name : "Atanmamış",
                o.Customer != null && o.Customer.IsCritical))
            .ToListAsync(ct);

        static bool Active(Row r) => r.Status != OperationStatus.Completed && r.Status != OperationStatus.Cancelled;

        var kpis = new DashboardKpis(
            TotalActive: rows.Count(Active),
            TodayLoading: rows.Count(r => r.LoadingDate == today),
            TodayDelivery: rows.Count(r => r.PlannedDeliveryDate == today || r.DeliveryDate == today),
            Delayed: rows.Count(r => r.DelayDays > 0),
            Risky: rows.Count(r => r.Risk >= RiskLevel.Orange),
            MissingDocuments: rows.Count(r => r.DocStatus == DocumentStatus.Missing || r.Status == OperationStatus.MissingDocuments),
            Completed: rows.Count(r => r.Status == OperationStatus.Completed),
            New24h: rows.Count(r => r.CreatedAt >= now.AddHours(-24)),
            AvgDelayDays: Math.Round(rows.Where(r => r.DelayDays > 0).Select(r => (double)r.DelayDays).DefaultIfEmpty(0).Average(), 1),
            CriticalCustomerOps: rows.Count(r => r.CustomerCritical && Active(r)),
            TotalOperations: rows.Count);

        var status = rows.GroupBy(r => r.Status)
            .Select(g => new NameValue(g.Key.ToString(), g.Count())).OrderByDescending(x => x.Value).ToList();
        var transport = rows.GroupBy(r => r.Transport)
            .Select(g => new NameValue(g.Key.ToString(), g.Count())).ToList();
        var risk = rows.GroupBy(r => r.Risk)
            .Select(g => new NameValue(g.Key.ToString(), g.Count())).OrderBy(x => x.Name).ToList();
        var dept = rows.Where(Active).GroupBy(r => r.DepartmentName)
            .Select(g => new NameValue(g.Key, g.Count())).OrderByDescending(x => x.Value).ToList();

        var recentRaw = await _db.Operations.AsNoTracking()
            .OrderByDescending(o => o.CreatedAt).Take(10)
            .Select(o => new { o.Id, o.SourceOperationNo, o.CustomerName, o.Status, o.CreatedAt })
            .ToListAsync(ct);
        var recent = recentRaw
            .Select(o => new RecentActivityDto(o.Id, o.SourceOperationNo, o.CustomerName, o.Status.ToString(), o.CreatedAt))
            .ToList();

        return new DashboardSummaryDto(kpis, status, transport, risk, dept, recent);
    }
}

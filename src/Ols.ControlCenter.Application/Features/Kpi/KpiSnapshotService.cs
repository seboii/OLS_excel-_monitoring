using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Kpi;

public sealed record KpiTrendPoint(
    DateOnly Date, int Total, int Current, int Completed, int Delayed, int Risky, int Critical,
    double AvgDelay, int OpenAlerts);

public interface IKpiSnapshotService
{
    /// <summary>Bugünün global KPI anlık görüntüsünü hesaplar ve <c>scope=global</c> için upsert eder (idempotent).</summary>
    Task CaptureGlobalAsync(CancellationToken ct);

    /// <summary>Son <paramref name="days"/> günün global trend serisini (eskiden yeniye) döner.</summary>
    Task<IReadOnlyList<KpiTrendPoint>> GetTrendAsync(int days, CancellationToken ct);
}

/// <summary>
/// Dashboard "anlık" olduğu için geçmiş/trend yoktu; bu servis günlük global KPI snapshot'ı alıp
/// (<see cref="KpiSnapshot"/>) zaman serisi üretir. Her senkron sonrası + günlük Hangfire job ile
/// çağrılır; aynı güne tekrar yazım upsert'tir (gün içi en güncel değeri tutar).
/// </summary>
public sealed class KpiSnapshotService : IKpiSnapshotService
{
    private const string GlobalScope = "global";
    private readonly IApplicationDbContext _db;
    private readonly ITrackingMetricsService _metrics;

    public KpiSnapshotService(IApplicationDbContext db, ITrackingMetricsService metrics)
    {
        _db = db;
        _metrics = metrics;
    }

    public async Task CaptureGlobalAsync(CancellationToken ct)
    {
        var all = await _metrics.LoadRowsAsync(ct);
        var rows = all.Where(r => BoardCatalog.OperationalGroups.Contains(r.Group)).ToList();

        var notArchived = rows.Where(r => !r.Archived).ToList();
        var current = notArchived.Where(r => !TrackingPhase.IsCompleted(r.Status)).ToList();
        var completed = notArchived.Count - current.Count;
        var archived = rows.Count - notArchived.Count;

        var openAlerts = await _db.Alerts.CountAsync(
            a => a.Status != AlertStatus.Resolved && a.Status != AlertStatus.Dismissed, ct);

        var metrics = new Dictionary<string, double>
        {
            ["total"] = rows.Count,
            ["current"] = current.Count,
            ["completed"] = completed,
            ["archived"] = archived,
            ["delayed"] = current.Count(r => r.Delay > 0),
            ["risky"] = current.Count(r => r.Risk >= RiskLevel.Orange),
            ["critical"] = current.Count(r => r.Risk >= RiskLevel.Red),
            ["avg_delay"] = Math.Round(current.Where(r => r.Delay > 0).Select(r => (double)r.Delay).DefaultIfEmpty(0).Average(), 1),
            ["open_alerts"] = openAlerts,
        };

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = DateTimeOffset.UtcNow;

        var existing = await _db.KpiSnapshots
            .FirstOrDefaultAsync(s => s.Scope == GlobalScope && s.ScopeId == null && s.Period == today, ct);

        if (existing is null)
        {
            _db.KpiSnapshots.Add(new KpiSnapshot
            {
                Scope = GlobalScope,
                ScopeId = null,
                Period = today,
                Metrics = metrics,
                ComputedAt = now,
            });
        }
        else
        {
            existing.Metrics = metrics;
            existing.ComputedAt = now;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<KpiTrendPoint>> GetTrendAsync(int days, CancellationToken ct)
    {
        var span = Math.Clamp(days, 1, 365);
        var from = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-(span - 1));

        var snapshots = await _db.KpiSnapshots.AsNoTracking()
            .Where(s => s.Scope == GlobalScope && s.ScopeId == null && s.Period >= from)
            .OrderBy(s => s.Period)
            .ToListAsync(ct);

        return snapshots.Select(s => new KpiTrendPoint(
            s.Period,
            G(s.Metrics, "total"), G(s.Metrics, "current"), G(s.Metrics, "completed"),
            G(s.Metrics, "delayed"), G(s.Metrics, "risky"), G(s.Metrics, "critical"),
            s.Metrics.TryGetValue("avg_delay", out var avg) ? avg : 0,
            G(s.Metrics, "open_alerts"))).ToList();
    }

    private static int G(Dictionary<string, double> m, string key)
        => m.TryGetValue(key, out var v) ? (int)Math.Round(v) : 0;
}

using Ols.ControlCenter.Application.Features.Kpi;

namespace Ols.ControlCenter.Worker.Jobs;

/// <summary>
/// Hangfire günlük işi: global KPI anlık görüntüsünü (<see cref="KpiSnapshot"/>) kaydeder ki dashboard
/// trend grafiği boş günlerde de bir veri noktasına sahip olsun. Senkron sonrası da yakalanır
/// (<see cref="PeriodicSyncJob"/>), ikisi de aynı güne idempotent upsert yapar.
/// </summary>
public sealed class DailyKpiSnapshotJob
{
    private readonly IKpiSnapshotService _snapshots;
    private readonly ILogger<DailyKpiSnapshotJob> _logger;

    public DailyKpiSnapshotJob(IKpiSnapshotService snapshots, ILogger<DailyKpiSnapshotJob> logger)
    {
        _snapshots = snapshots;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        await _snapshots.CaptureGlobalAsync(ct);
        _logger.LogInformation("Günlük KPI snapshot alındı.");
    }
}

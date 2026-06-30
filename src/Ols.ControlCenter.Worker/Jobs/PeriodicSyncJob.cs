using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Abstractions.Realtime;
using Ols.ControlCenter.Application.Features.Kpi;
using Ols.ControlCenter.Application.Features.Risk;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Worker.Jobs;

/// <summary>
/// Hangfire periyodik işi: zamanı gelen aktif veri kaynaklarını (URL'li, manuel olmayan)
/// otomatik senkronize eder, ardından risk motorunu çalıştırır. Sonuçlar Redis köprüsü
/// üzerinden API'ye, oradan SignalR ile dashboard'a canlı yansıtılır.
/// Her dakika tetiklenir; bir kaynak yalnızca <c>SyncIntervalMinutes</c> dolduğunda işlenir.
/// </summary>
public sealed class PeriodicSyncJob
{
    private readonly IApplicationDbContext _db;
    private readonly IDataImportService _import;
    private readonly IRiskEngine _risk;
    private readonly IKpiSnapshotService _snapshots;
    private readonly IRealtimeNotifier _realtime;
    private readonly ILogger<PeriodicSyncJob> _logger;

    public PeriodicSyncJob(
        IApplicationDbContext db,
        IDataImportService import,
        IRiskEngine risk,
        IKpiSnapshotService snapshots,
        IRealtimeNotifier realtime,
        ILogger<PeriodicSyncJob> logger)
    {
        _db = db;
        _import = import;
        _risk = risk;
        _snapshots = snapshots;
        _realtime = realtime;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var candidates = await _db.DataSources
            .Where(d => d.IsActive
                        && d.AccessType != DataSourceAccessType.Upload
                        && d.Url != null && d.Url != "")
            .ToListAsync(ct);

        var due = candidates
            .Where(d => d.LastSyncAt is null
                        || d.LastSyncAt.Value.AddMinutes(Math.Max(1, d.SyncIntervalMinutes)) <= now)
            .ToList();

        if (due.Count == 0)
            return;

        _logger.LogInformation("Otomatik sync: {Count} kaynak zamanı geldi.", due.Count);

        var syncedAny = false;
        foreach (var source in due)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var result = await _import.SyncSourceAsync(source.Id, userId: null, ct);
                syncedAny = true;
                _logger.LogInformation("Otomatik sync tamam: '{Name}' (#{Id}) — {Upserted} satır.",
                    source.Name, source.Id, result.RowsUpserted);
                await _realtime.NotifyAsync(RealtimeEvents.DataSynced,
                    new { dataSourceId = source.Id, result.RowsUpserted }, ct);
            }
            catch (Exception ex)
            {
                // İndirme/parse hatası: başarısız denemeyi kaydet ki tekrar deneme interval kadar ertelensin
                // (her dakika başarısız URL'yi indirmeye çalışmayı önler).
                source.LastSyncAt = now;
                source.LastSyncStatus = SyncStatus.Failed;
                source.LastSyncError = ex.Message;
                await _db.SaveChangesAsync(ct);
                _logger.LogWarning(ex, "Otomatik sync hatası: '{Name}' (#{Id}).", source.Name, source.Id);
            }
        }

        if (syncedAny)
        {
            var triggered = await _risk.EvaluateAllAsync(ct);
            await _realtime.NotifyAsync(RealtimeEvents.AlertsChanged, new { triggered }, ct);
            await _realtime.NotifyAsync(RealtimeEvents.NotificationsChanged, ct: ct);
            _logger.LogInformation("Otomatik sync sonrası risk motoru çalıştı: {Triggered} kural tetiklendi.", triggered);

            // Senkron sonrası bugünün KPI snapshot'ını tazele (trend grafiği için).
            await _snapshots.CaptureGlobalAsync(ct);
        }
    }
}

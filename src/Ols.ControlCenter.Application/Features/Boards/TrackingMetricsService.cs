using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Domain.Entities.Tracking;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Boards;

/// <summary>Tüm takip tablolarındaki tek satırın metrik için sadeleştirilmiş görünümü.</summary>
public sealed record TrackingMetricRow(
    string BoardKey, string BoardTitle, string Group,
    RiskLevel Risk, int Delay, bool Archived, string Ref, string? Status, DateTimeOffset ImportedAt);

/// <summary>
/// 9 sayfa-başına takip tablosunu tek bir metrik akışında birleştirir. Dashboard, KPI ve risk/dikkat
/// görünümleri buradan beslenir — böylece her tüketici kendi agregasyonunu yapar, tablo dağılımı tek yerde.
/// </summary>
public interface ITrackingMetricsService
{
    Task<IReadOnlyList<TrackingMetricRow>> LoadRowsAsync(CancellationToken ct);

    /// <summary>Bir board'un satırlarını TAM entity olarak (tüm kolonlar) döner — örn. Excel export için.</summary>
    Task<IReadOnlyList<TrackingRecordBase>> LoadBoardEntitiesAsync(TrackingBoardType board, CancellationToken ct);
}

public sealed class TrackingMetricsService : ITrackingMetricsService
{
    private readonly IApplicationDbContext _db;

    public TrackingMetricsService(IApplicationDbContext db) => _db = db;

    private sealed record Proj(RiskLevel Risk, int Delay, bool Archived, string Ref, string? Status, DateTimeOffset ImportedAt);

    public async Task<IReadOnlyList<TrackingMetricRow>> LoadRowsAsync(CancellationToken ct)
    {
        var all = new List<TrackingMetricRow>();
        foreach (var meta in BoardCatalog.All)
        {
            var rows = await LoadBoardAsync(meta.Board, ct);
            foreach (var r in rows)
                all.Add(new TrackingMetricRow(
                    meta.Key, meta.Title, meta.Group, r.Risk, r.Delay, r.Archived, r.Ref, r.Status, r.ImportedAt));
        }
        return all;
    }

    private Task<List<Proj>> LoadBoardAsync(TrackingBoardType board, CancellationToken ct) => board switch
    {
        TrackingBoardType.SeaTransit => ProjAsync(_db.SeaTransitRecords, ct),
        TrackingBoardType.SeaImport => ProjAsync(_db.SeaImportRecords, ct),
        TrackingBoardType.SeaExport => ProjAsync(_db.SeaExportRecords, ct),
        TrackingBoardType.RoadTransit => ProjAsync(_db.RoadTransitRecords, ct),
        TrackingBoardType.RoadLoad => ProjAsync(_db.RoadLoadRecords, ct),
        TrackingBoardType.RoadArchive => ProjAsync(_db.RoadArchiveRecords, ct),
        TrackingBoardType.Alabora => ProjAsync(_db.AlaboraFinanceRecords, ct),
        TrackingBoardType.Air => ProjAsync(_db.AirOperationRecords, ct),
        TrackingBoardType.AirDaily => ProjAsync(_db.AirDailyRecords, ct),
        _ => Task.FromResult(new List<Proj>()),
    };

    private static Task<List<Proj>> ProjAsync<T>(DbSet<T> set, CancellationToken ct) where T : TrackingRecordBase
        => set.AsNoTracking()
            .Select(x => new Proj(x.RiskLevel, x.DelayDays, x.IsArchived, x.SourceRowKey, x.StatusText, x.ImportedAt))
            .ToListAsync(ct);

    public Task<IReadOnlyList<TrackingRecordBase>> LoadBoardEntitiesAsync(TrackingBoardType board, CancellationToken ct) => board switch
    {
        TrackingBoardType.SeaTransit => Cast(_db.SeaTransitRecords, ct),
        TrackingBoardType.SeaImport => Cast(_db.SeaImportRecords, ct),
        TrackingBoardType.SeaExport => Cast(_db.SeaExportRecords, ct),
        TrackingBoardType.RoadTransit => Cast(_db.RoadTransitRecords, ct),
        TrackingBoardType.RoadLoad => Cast(_db.RoadLoadRecords, ct),
        TrackingBoardType.RoadArchive => Cast(_db.RoadArchiveRecords, ct),
        TrackingBoardType.Alabora => Cast(_db.AlaboraFinanceRecords, ct),
        TrackingBoardType.Air => Cast(_db.AirOperationRecords, ct),
        TrackingBoardType.AirDaily => Cast(_db.AirDailyRecords, ct),
        _ => Task.FromResult<IReadOnlyList<TrackingRecordBase>>(Array.Empty<TrackingRecordBase>()),
    };

    private static async Task<IReadOnlyList<TrackingRecordBase>> Cast<T>(DbSet<T> set, CancellationToken ct) where T : TrackingRecordBase
        => (await set.AsNoTracking().OrderBy(x => x.RowIndex).ToListAsync(ct)).Cast<TrackingRecordBase>().ToList();
}

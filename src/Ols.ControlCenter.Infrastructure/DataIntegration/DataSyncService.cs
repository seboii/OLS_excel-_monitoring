using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Features.DataSources;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>
/// Senkronizasyon orkestratörü: kaynak ayarlarını + kolon eşleştirmelerini yükler, upsert'i
/// <see cref="IOperationUpsertService"/>'e, log/son-senkron kaydını <see cref="IDataSyncLogService"/>'e
/// devreder ve sonucu tek transaction içinde kalıcılaştırır (atomik).
/// </summary>
public sealed class DataSyncService : IDataSyncService
{
    private readonly IApplicationDbContext _db;
    private readonly ISourceFileParser _parser;
    private readonly IOperationUpsertService _upsert;
    private readonly ITrackingImportService _tracking;
    private readonly IDataSyncLogService _logService;

    public DataSyncService(
        IApplicationDbContext db, ISourceFileParser parser,
        IOperationUpsertService upsert, ITrackingImportService tracking, IDataSyncLogService logService)
    {
        _db = db;
        _parser = parser;
        _upsert = upsert;
        _tracking = tracking;
        _logService = logService;
    }

    public async Task<SyncResult> ImportFileAsync(long dataSourceId, Stream fileStream, string fileName, long? userId, CancellationToken ct = default)
    {
        var ds = await _db.DataSources.FirstOrDefaultAsync(d => d.Id == dataSourceId, ct)
            ?? throw new InvalidOperationException("Veri kaynağı bulunamadı.");
        var rows = _parser.Parse(fileStream, fileName, ds.SheetName, ds.HeaderRowIndex);
        return await SyncRowsAsync(dataSourceId, rows, userId, fileName, ds.SheetName, ct);
    }

    public async Task<SyncResult> SyncRowsAsync(
        long dataSourceId, IReadOnlyList<IReadOnlyDictionary<string, string?>> rows, long? userId,
        string? fileName = null, string? sheetName = null, CancellationToken ct = default)
    {
        var started = DateTimeOffset.UtcNow;

        var ds = await _db.DataSources.FirstOrDefaultAsync(d => d.Id == dataSourceId, ct)
            ?? throw new InvalidOperationException("Veri kaynağı bulunamadı.");

        // Hedef bir takip tablosu (sayfa-başına tablo) varsa: satırları tip-güvenli sayfa tablosuna yaz.
        if (ds.TargetBoard != TrackingBoardType.None)
            return await SyncTrackingAsync(ds, rows, fileName, sheetName, started, ct);

        var mappings = await _db.DataSourceColumnMappings
            .Where(m => m.DataSourceId == dataSourceId).ToListAsync(ct);
        var statusMap = await BuildStatusMapAsync(dataSourceId, ct);

        // Satır → operasyon upsert'ini ayrı motora devret (DbContext'e ekler, kaydetmez).
        var summary = await _upsert.UpsertAsync(ds, rows, mappings, statusMap, userId, ct);

        var status = summary.Failed == 0
            ? SyncStatus.Success
            : (summary.Upserted > 0 ? SyncStatus.PartialSuccess : SyncStatus.Failed);

        // Son-senkron durumu + log kaydını hazırla (DbContext'e ekler, kaydetmez).
        _logService.RecordResult(ds, new SyncRunResult(
            started, DateTimeOffset.UtcNow, status,
            rows.Count, summary.Upserted, summary.Failed, summary.Errors, fileName, sheetName));

        if (summary.FailedRows.Count > 0) _db.ImportedRawRows.AddRange(summary.FailedRows);

        // Operasyonlar + log + ham satırlar tek transaction'da atomik kaydedilir.
        await _db.SaveChangesAsync(ct);
        return new SyncResult(rows.Count, summary.Upserted, summary.Failed, summary.Errors);
    }

    /// <summary>
    /// Sayfa-başına takip tablosuna senkron: satırları tip-güvenli sayfa tablosuna yazar,
    /// log + son-senkron durumunu kaydeder ve tek transaction'da kalıcılaştırır.
    /// </summary>
    private async Task<SyncResult> SyncTrackingAsync(
        Ols.ControlCenter.Domain.Entities.DataSource ds,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> rows,
        string? fileName, string? sheetName, DateTimeOffset started, CancellationToken ct)
    {
        var summary = await _tracking.ImportAsync(ds, rows, ct);

        var status = summary.Errors.Count == 0
            ? SyncStatus.Success
            : (summary.Imported > 0 ? SyncStatus.PartialSuccess : SyncStatus.Failed);

        _logService.RecordResult(ds, new SyncRunResult(
            started, DateTimeOffset.UtcNow, status,
            rows.Count, summary.Imported, summary.Errors.Count, summary.Errors, fileName, sheetName ?? ds.SheetName));

        await _db.SaveChangesAsync(ct);
        return new SyncResult(rows.Count, summary.Imported, summary.Errors.Count, summary.Errors);
    }

    private async Task<Dictionary<string, OperationStatus>> BuildStatusMapAsync(long dataSourceId, CancellationToken ct)
    {
        var maps = await _db.StatusMappings
            .Where(s => s.DataSourceId == null || s.DataSourceId == dataSourceId).ToListAsync(ct);

        var dict = new Dictionary<string, OperationStatus>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in maps.Where(x => x.DataSourceId == null)) dict[m.SourceStatus] = m.TargetStatus;
        foreach (var m in maps.Where(x => x.DataSourceId == dataSourceId)) dict[m.SourceStatus] = m.TargetStatus;
        return dict;
    }
}

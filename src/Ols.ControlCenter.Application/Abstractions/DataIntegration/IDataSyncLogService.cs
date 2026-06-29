using Ols.ControlCenter.Application.Features.DataSources;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Abstractions.DataIntegration;

/// <summary>Bir senkron çalışmasının kaydedilecek özeti.</summary>
public sealed record SyncRunResult(
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    SyncStatus Status,
    int RowsRead,
    int RowsUpserted,
    int RowsFailed,
    IReadOnlyList<string> Errors,
    string? FileName,
    string? SheetName);

/// <summary>
/// Senkronizasyon loglarını okur ve her çalışma için <see cref="DataSyncLog"/> kaydı üretir;
/// ayrıca veri kaynağının son-senkron durum alanlarını (<c>LastSyncAt</c> vb.) günceller.
/// </summary>
public interface IDataSyncLogService
{
    /// <summary>Bir veri kaynağının son 50 senkron logunu (yeniden eskiye) döner.</summary>
    Task<IReadOnlyList<SyncLogDto>> GetLogsAsync(long dataSourceId, CancellationToken ct);

    /// <summary>
    /// Sonucu kaynağın durum alanlarına işler ve DbContext'e bir <see cref="DataSyncLog"/> ekler.
    /// <b>SaveChanges çağırmaz</b> — orkestratör tek transaction içinde kaydeder.
    /// </summary>
    void RecordResult(DataSource source, SyncRunResult result);
}

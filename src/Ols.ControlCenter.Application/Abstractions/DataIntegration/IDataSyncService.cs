using Ols.ControlCenter.Application.Features.DataSources;

namespace Ols.ControlCenter.Application.Abstractions.DataIntegration;

/// <summary>
/// Veri kaynağından gelen satırları kolon eşleştirme + normalizasyon ile standart
/// operasyon modeline upsert eden senkronizasyon servisi.
/// </summary>
public interface IDataSyncService
{
    /// <summary>Yüklenen dosyayı (xlsx/csv) parse edip senkronize eder.</summary>
    Task<SyncResult> ImportFileAsync(long dataSourceId, Stream fileStream, string fileName, long? userId, CancellationToken ct = default);

    /// <summary>Önceden parse edilmiş satırları senkronize eder.</summary>
    Task<SyncResult> SyncRowsAsync(
        long dataSourceId, IReadOnlyList<IReadOnlyDictionary<string, string?>> rows, long? userId,
        string? fileName = null, string? sheetName = null, CancellationToken ct = default);
}

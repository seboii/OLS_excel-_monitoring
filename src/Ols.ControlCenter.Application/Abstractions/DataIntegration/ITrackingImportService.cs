using Ols.ControlCenter.Domain.Entities;

namespace Ols.ControlCenter.Application.Abstractions.DataIntegration;

/// <summary>
/// Bir veri kaynağının satırlarını, kaynağın hedef takip tablosuna (sayfa-başına tablo) upsert eder.
/// DbContext'e ekler/siler ama kaydetmez — çağıran (DataSyncService) tek transaction'da kalıcılaştırır.
/// </summary>
public interface ITrackingImportService
{
    Task<TrackingImportSummary> ImportAsync(
        DataSource source,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> rows,
        CancellationToken ct);
}

/// <summary>Takip içe-aktarma özeti.</summary>
public sealed record TrackingImportSummary(int Imported, int Skipped, IReadOnlyList<string> Errors);

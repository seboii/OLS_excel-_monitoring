using Ols.ControlCenter.Application.Features.DataSources;

namespace Ols.ControlCenter.Application.Abstractions.DataIntegration;

public sealed record ImportPreview(
    IReadOnlyList<string> SheetNames,
    SheetPreview Sheet,
    IReadOnlyList<MappingSuggestion> Suggestions);

public sealed record ConnectionTestResult(bool Ok, string FileName, long SizeBytes, IReadOnlyList<string> SheetNames);

/// <summary>İndir → oku → (önizle / eşleştir) → Operations'a aktar orkestratörü.</summary>
public interface IDataImportService
{
    Task<ConnectionTestResult> TestConnectionAsync(long dataSourceId, CancellationToken ct);
    Task<ImportPreview> PreviewSourceAsync(long dataSourceId, string? sheetName, int? headerRowIndex, CancellationToken ct);
    Task<SyncResult> SyncSourceAsync(long dataSourceId, long? userId, CancellationToken ct);
    Task<ImportPreview> PreviewUploadAsync(byte[] content, string fileName, string? sheetName, int? headerRowIndex, CancellationToken ct);
    Task<SyncResult> ImportUploadAsync(long dataSourceId, byte[] content, string fileName, string? sheetName, int? headerRowIndex, long? userId, CancellationToken ct);
}

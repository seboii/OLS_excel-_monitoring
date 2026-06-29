using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Abstractions.DataIntegration;

/// <summary>Bir upsert partisinin sonucu (sayaçlar + hatalar + başarısız ham satırlar).</summary>
public sealed record UpsertSummary(
    int Upserted,
    int Failed,
    IReadOnlyList<string> Errors,
    IReadOnlyList<ImportedRawRow> FailedRows);

/// <summary>
/// Kaynak satırlarını kolon eşleştirme + normalizasyon ile standart <see cref="Operation"/>
/// modeline upsert eder. Tekilleştirme (SourceId + SourceOperationNo) ikilisiyle yapılır;
/// operasyon numarası boş satırlar atlanır, hatalı satırlar <see cref="ImportedRawRow"/> olarak döner.
/// DbContext'e ekleme/güncelleme yapar fakat <b>SaveChanges çağırmaz</b> — kalıcılaştırmayı
/// orkestratör (DataSyncService) tek transaction içinde yapar.
/// </summary>
public interface IOperationUpsertService
{
    Task<UpsertSummary> UpsertAsync(
        DataSource source,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> rows,
        IReadOnlyList<DataSourceColumnMapping> mappings,
        IReadOnlyDictionary<string, OperationStatus> statusMap,
        long? userId,
        CancellationToken ct);
}

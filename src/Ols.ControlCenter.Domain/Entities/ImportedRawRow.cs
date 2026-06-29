using Ols.ControlCenter.Domain.Common;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// İçe aktarma sırasında ham/başarısız satırların izlenmesi. Eşleşmeyen veya hatalı satırlar
/// burada saklanır; böylece hiçbir veri kaybolmaz ve sebebi görülebilir.
/// </summary>
public class ImportedRawRow : BaseEntity
{
    public long DataSourceId { get; set; }
    public long? DataSyncLogId { get; set; }

    public int RowIndex { get; set; }

    /// <summary>Satırın ham hali (kolon → değer) JSON olarak.</summary>
    public string RawJson { get; set; } = "{}";

    public bool IsImported { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

using Ols.ControlCenter.Domain.Common;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// Kaynak dosyadaki bir kolonu sistemdeki standart alana eşler.
/// Örn: "Client" → "CustomerName", "ETA" → "Eta", "Status" → "Status".
/// </summary>
public class DataSourceColumnMapping : BaseEntity
{
    public long DataSourceId { get; set; }
    public DataSource DataSource { get; set; } = null!;

    /// <summary>Kaynak dosyadaki kolon başlığı.</summary>
    public string SourceColumn { get; set; } = string.Empty;

    /// <summary>Operations modelindeki hedef alan adı.</summary>
    public string TargetField { get; set; } = string.Empty;

    /// <summary>İsteğe bağlı dönüşüm ipucu (tarih formatı, "status-map" vb.).</summary>
    public string? Transform { get; set; }

    public bool IsRequired { get; set; }
}

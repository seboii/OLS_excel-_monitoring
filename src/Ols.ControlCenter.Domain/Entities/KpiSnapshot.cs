using Ols.ControlCenter.Domain.Common;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Hesaplanmış KPI anlık görüntüsü (departman/kişi/global · dönem bazlı). Metrikler JSONB.</summary>
public class KpiSnapshot : BaseEntity
{
    /// <summary>"global" | "department" | "user".</summary>
    public string Scope { get; set; } = "global";
    public long? ScopeId { get; set; }

    public DateOnly Period { get; set; }

    /// <summary>Metrik adı → değer (örn. {"delayed_ratio": 0.12}). JSONB.</summary>
    public Dictionary<string, double> Metrics { get; set; } = new();

    public DateTimeOffset ComputedAt { get; set; }
}

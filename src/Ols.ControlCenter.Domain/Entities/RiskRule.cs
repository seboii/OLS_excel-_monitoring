using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// Risk motoru kuralının yapılandırması. Kurallar tablodan açılıp kapanabilir ve eşik
/// parametreleri (<see cref="Parameters"/>, JSONB) ayarlanabilir.
/// </summary>
public class RiskRule : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public RiskLevel Severity { get; set; }
    public AlertType AlertType { get; set; }

    /// <summary>Eşik parametreleri (örn. {"days":"2"}). JSONB olarak saklanır.</summary>
    public Dictionary<string, string> Parameters { get; set; } = new();

    /// <summary>Yalnızca belirli bir taşıma tipine uygulanıyorsa.</summary>
    public TransportType? AppliesToTransportType { get; set; }
}

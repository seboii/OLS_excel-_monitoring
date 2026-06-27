using Ols.ControlCenter.Domain.Common;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Müşteri. Kritik müşteri ve kredi limiti risk kurallarını besler.</summary>
public class Customer : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Kritik müşteri listesinde mi? (gecikmede kırmızı uyarı tetikler)</summary>
    public bool IsCritical { get; set; }

    public decimal? CreditLimit { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<Operation> Operations { get; set; } = new List<Operation>();
}

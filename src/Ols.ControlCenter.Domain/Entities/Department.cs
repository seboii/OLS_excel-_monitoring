using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Operasyon departmanı (Karayolu, Deniz, Hava, Gümrük, Finans, Müşteri, Pricing, Parsiyel).</summary>
public class Department : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TransportType? DefaultTransportType { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Operation> Operations { get; set; } = new List<Operation>();
}

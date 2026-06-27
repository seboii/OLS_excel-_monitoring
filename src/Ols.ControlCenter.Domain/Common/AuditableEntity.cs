namespace Ols.ControlCenter.Domain.Common;

/// <summary>Oluşturma/güncelleme zaman damgası ve kullanıcı izini tutan temel sınıf.</summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public long? CreatedByUserId { get; set; }
    public long? UpdatedByUserId { get; set; }
}

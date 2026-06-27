using Ols.ControlCenter.Domain.Common;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Denetim günlüğü: kim, neyi, ne zaman değiştirdi (öncesi/sonrası JSONB).</summary>
public class AuditLog : BaseEntity
{
    public long? UserId { get; set; }
    public string? UserName { get; set; }

    /// <summary>İşlem türü (Create, Update, Delete, Login, Sync...).</summary>
    public string Action { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }

    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }

    public string? IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

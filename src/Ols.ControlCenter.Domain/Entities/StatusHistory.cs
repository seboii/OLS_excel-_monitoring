using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Operasyon statü değişikliği günlüğü (senkron veya manuel kaynaklı).</summary>
public class StatusHistory : BaseEntity
{
    public long OperationId { get; set; }
    public Operation Operation { get; set; } = null!;

    public OperationStatus? FromStatus { get; set; }
    public OperationStatus ToStatus { get; set; }

    public long? ChangedByUserId { get; set; }

    /// <summary>"sync" veya "manual".</summary>
    public string Source { get; set; } = "manual";

    public DateTimeOffset ChangedAt { get; set; }
    public string? Note { get; set; }
}

using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// Risk motorunun ürettiği (veya manuel) uyarı. <see cref="DedupeKey"/> aynı operasyon+kural için
/// mükerrer uyarı oluşmasını engeller; tekrar tetiklenince güncellenir.
/// </summary>
public class Alert : AuditableEntity
{
    public long OperationId { get; set; }
    public Operation Operation { get; set; } = null!;

    public AlertType Type { get; set; }
    public RiskLevel RiskLevel { get; set; }

    /// <summary>Kuralın kodu (DELAY, PAYMENT_RISK, SEA_DEMURRAGE...). Manuel uyarıda "MANUAL".</summary>
    public string RuleCode { get; set; } = string.Empty;

    /// <summary>Mükerrer engelleme anahtarı — genelde "{OperationId}:{RuleCode}".</summary>
    public string DedupeKey { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public long? ResponsibleUserId { get; set; }
    public User? ResponsibleUser { get; set; }

    public DateTimeOffset? Deadline { get; set; }
    public AlertStatus Status { get; set; } = AlertStatus.Open;

    public string? ResolutionNote { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public long? ResolvedByUserId { get; set; }

    public DateTimeOffset FirstTriggeredAt { get; set; }
    public DateTimeOffset LastTriggeredAt { get; set; }
    public int TriggerCount { get; set; } = 1;
}

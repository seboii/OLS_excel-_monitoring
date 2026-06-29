using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// Risk motorunun ürettiği (veya manuel) uyarı. <see cref="DedupeKey"/> aynı kaynak+kural için
/// mükerrer uyarı oluşmasını engeller; tekrar tetiklenince güncellenir.
/// İki kaynaktan biri doludur: eski <see cref="Operation"/> modeli (demo veri) VEYA bir takip
/// tablosu satırı (<see cref="BoardKey"/>+<see cref="RecordRef"/> — gerçek operasyon verisi).
/// </summary>
public class Alert : AuditableEntity
{
    public long? OperationId { get; set; }
    public Operation? Operation { get; set; }

    /// <summary>Takip tablosu sekme anahtarı (örn. "deniz-transit"). Board-bound uyarılarda dolu.</summary>
    public string? BoardKey { get; set; }
    public string? BoardTitle { get; set; }

    /// <summary>Taşıma grubu (Deniz/Kara/Hava/Finans). Board-bound uyarılarda dolu.</summary>
    public string? Group { get; set; }

    /// <summary>Kaynak satırın dosya/ref numarası. Board-bound uyarılarda dolu.</summary>
    public string? RecordRef { get; set; }

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

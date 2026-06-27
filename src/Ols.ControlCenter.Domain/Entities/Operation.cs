using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// Sistemin merkezindeki standartlaştırılmış operasyon kaydı. Tüm taşıma tipleri (kara/deniz/hava/gümrük)
/// bu modele normalize edilir. Mod'a özel alanlar <see cref="Detail"/> içinde tutulur.
/// </summary>
public class Operation : AuditableEntity, ISoftDelete
{
    // --- Kaynak ---
    public long? SourceId { get; set; }
    public DataSource? Source { get; set; }

    /// <summary>Dış sistem/Excel'deki operasyon numarası (SourceId ile birlikte tekil).</summary>
    public string? SourceOperationNo { get; set; }

    // --- Sınıflandırma ---
    public TransportType TransportType { get; set; }
    public ServiceType ServiceType { get; set; }
    public TradeDirection TradeDirection { get; set; } = TradeDirection.None;

    public long? DepartmentId { get; set; }
    public Department? Department { get; set; }

    // --- Müşteri / taraflar ---
    public long? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>Denormalize müşteri adı (kaynak henüz müşteri kaydına eşleşmemiş olabilir).</summary>
    public string CustomerName { get; set; } = string.Empty;
    public string? Shipper { get; set; }
    public string? Consignee { get; set; }

    // --- Rota ---
    public string? OriginCountry { get; set; }
    public string? OriginCity { get; set; }
    public string? DestinationCountry { get; set; }
    public string? DestinationCity { get; set; }

    // --- Tarihler ---
    public DateOnly? LoadingDate { get; set; }
    public DateTimeOffset? Etd { get; set; }
    public DateTimeOffset? Eta { get; set; }
    public DateTimeOffset? ActualArrivalDate { get; set; }
    public DateOnly? PlannedDeliveryDate { get; set; }
    public DateOnly? DeliveryDate { get; set; }

    // --- Durum & risk ---
    public OperationStatus Status { get; set; } = OperationStatus.New;
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Green;

    // --- Sorumluluk ---
    public long? ResponsibleUserId { get; set; }
    public User? ResponsibleUser { get; set; }
    public long? SalesOwnerId { get; set; }
    public User? SalesOwner { get; set; }

    // --- Finans & evrak özeti ---
    public FinanceStatus FinanceStatus { get; set; } = FinanceStatus.Pending;
    public DocumentStatus DocumentStatus { get; set; } = DocumentStatus.Pending;

    // --- İzleme zaman damgaları ---
    public DateTimeOffset? LastCustomerUpdateDate { get; set; }
    public DateTimeOffset? LastInternalCommentDate { get; set; }

    // --- Sonraki aksiyon ---
    public DateOnly? NextActionDate { get; set; }
    public string? NextActionDescription { get; set; }
    public long? NextActionOwnerId { get; set; }
    public User? NextActionOwner { get; set; }

    // --- Gecikme ---
    public int DelayDays { get; set; }
    public DelayReason DelayReason { get; set; } = DelayReason.None;

    // --- Finansal tutarlar ---
    public decimal? RevenueAmount { get; set; }
    public decimal? CostAmount { get; set; }
    public decimal? GrossProfit { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateOnly? PaymentDueDate { get; set; }
    public DateOnly? PaymentReceivedDate { get; set; }

    // --- Soft delete ---
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // --- İlişkiler ---
    public OperationDetail? Detail { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<StatusHistory> StatusHistory { get; set; } = new List<StatusHistory>();

    /// <summary>Tamamlanmamış ve iptal edilmemiş operasyon aktiftir.</summary>
    public bool IsActiveOperation =>
        Status is not (OperationStatus.Completed or OperationStatus.Cancelled);

    /// <summary>
    /// Türetilmiş alanları (brüt kâr, gecikme günü) verilen referans tarihe göre yeniden hesaplar.
    /// Tüm tarih mantığı merkezi olsun diye buradadır.
    /// </summary>
    public void RecomputeDerived(DateOnly today)
    {
        if (RevenueAmount.HasValue && CostAmount.HasValue)
            GrossProfit = RevenueAmount.Value - CostAmount.Value;

        if (PlannedDeliveryDate is { } planned && IsActiveOperation)
        {
            // Aktif ve teslim edilmemiş: bugüne göre gecikme
            var reference = DeliveryDate ?? today;
            var diff = reference.DayNumber - planned.DayNumber;
            DelayDays = diff > 0 ? diff : 0;
        }
        else if (PlannedDeliveryDate is { } p && DeliveryDate is { } delivered)
        {
            // Tamamlanmış: gerçekleşen teslime göre nihai gecikme
            var diff = delivered.DayNumber - p.DayNumber;
            DelayDays = diff > 0 ? diff : 0;
        }
        else
        {
            DelayDays = 0;
        }
    }
}

using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Tahsilat/fatura kaydı. Operasyona bağlı olabilir; finans risk kurallarını besler.</summary>
public class Payment : AuditableEntity, ISoftDelete
{
    public long? OperationId { get; set; }
    public Operation? Operation { get; set; }

    public string InvoiceNo { get; set; } = string.Empty;

    public long? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? CustomerName { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";

    public DateOnly? DueDate { get; set; }
    public DateOnly? ReceivedDate { get; set; }
    public FinanceStatus Status { get; set; } = FinanceStatus.Pending;
    public int DelayDays { get; set; }

    public string? Bank { get; set; }
    public long? FinanceUserId { get; set; }
    public User? FinanceUser { get; set; }

    public RiskLevel RiskLevel { get; set; } = RiskLevel.Green;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

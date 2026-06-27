namespace Ols.ControlCenter.Domain.Enums;

/// <summary>Otomatik veya manuel oluşturulan uyarı tipleri.</summary>
public enum AlertType
{
    Delay,
    MissingDocuments,
    PaymentRisk,
    OperationalDeviation,
    CustomerInfoGap,
    EtaChange,
    FreeTimeDemurrageRisk,
    Rework,
    CriticalCustomer,
    ManagementApproval,
    NextActionMissing
}

public enum AlertStatus
{
    Open,
    InReview,
    TaskCreated,
    Resolved,
    Dismissed
}

/// <summary>Görev statüsü. (System.Threading.Tasks.Task ile çakışmaması için "WorkTask" adlandırması.)</summary>
public enum WorkTaskStatus
{
    New,
    InProgress,
    OnHold,
    Completed,
    Cancelled,
    Escalated
}

public enum TaskPriority
{
    Low,
    Normal,
    High,
    Critical
}

/// <summary>Operasyon yorumu tipi.</summary>
public enum CommentType
{
    General,
    CustomerInfo,
    Operation,
    Finance,
    Customs,
    Risk,
    Management,
    Carrier
}

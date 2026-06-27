namespace Ols.ControlCenter.Domain.Enums;

/// <summary>Veri kaynağı tipi — hangi connector'ın kullanılacağını belirler.</summary>
public enum DataSourceType
{
    SharePointExcel,
    OneDriveExcel,
    GoogleSheets,
    YandexDiskExcel,
    ManualExcel,
    Api,
    Csv
}

/// <summary>Bir senkronizasyon çalışmasının sonucu.</summary>
public enum SyncStatus
{
    Running,
    Success,
    PartialSuccess,
    Failed
}

/// <summary>Bildirim önem seviyesi.</summary>
public enum NotificationLevel
{
    Info,
    Warning,
    Critical,
    ManagementIntervention
}

/// <summary>Bildirim tipi.</summary>
public enum NotificationType
{
    NewTask,
    TaskOverdue,
    CriticalOperation,
    PaymentOverdue,
    MissingDocuments,
    EtaChanged,
    CustomerUpdateOverdue,
    ManagerComment,
    DataSourceError,
    SyncFailed
}

using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// Bir veri kaynağı (SharePoint/OneDrive/Google Sheets/Yandex/Manuel Excel/CSV/API).
/// Bağlantı bilgisi şifreli saklanır; senkronizasyon bu tanıma göre yürür.
/// </summary>
public class DataSource : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public DataSourceType Type { get; set; }

    public long? DepartmentId { get; set; }
    public Department? Department { get; set; }

    /// <summary>Bu kaynaktan gelen operasyonlar için varsayılan taşıma tipi.</summary>
    public TransportType? DefaultTransportType { get; set; }

    /// <summary>AES ile şifrelenmiş bağlantı bilgisi (link, token, sheet id...). Asla düz metin saklanmaz.</summary>
    public string ConnectionConfigEncrypted { get; set; } = string.Empty;

    public string? SheetName { get; set; }
    public int HeaderRowIndex { get; set; } = 1;
    public int SyncIntervalMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastSyncAt { get; set; }
    public SyncStatus? LastSyncStatus { get; set; }
    public string? LastSyncError { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<DataSourceColumnMapping> ColumnMappings { get; set; } = new List<DataSourceColumnMapping>();
    public ICollection<DataSyncLog> SyncLogs { get; set; } = new List<DataSyncLog>();
    public ICollection<StatusMapping> StatusMappings { get; set; } = new List<StatusMapping>();
    public ICollection<Operation> Operations { get; set; } = new List<Operation>();
}

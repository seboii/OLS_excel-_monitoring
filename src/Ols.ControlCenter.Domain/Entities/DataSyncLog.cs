using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Bir senkronizasyon çalışmasının kaydı (kaç satır okundu/yazıldı, hata var mı).</summary>
public class DataSyncLog : BaseEntity
{
    public long DataSourceId { get; set; }
    public DataSource DataSource { get; set; } = null!;

    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public SyncStatus Status { get; set; }

    public int RowsRead { get; set; }
    public int RowsUpserted { get; set; }
    public int RowsFailed { get; set; }

    public string? Message { get; set; }
    public long? DurationMs { get; set; }
}

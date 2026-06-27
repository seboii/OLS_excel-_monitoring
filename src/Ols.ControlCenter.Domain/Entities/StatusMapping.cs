using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// Kaynak statü metnini standart <see cref="OperationStatus"/>'a çevirir.
/// DataSourceId null ise global eşleştirmedir. Örn: "On Road" → InTransit.
/// </summary>
public class StatusMapping : BaseEntity
{
    public long? DataSourceId { get; set; }
    public DataSource? DataSource { get; set; }

    public string SourceStatus { get; set; } = string.Empty;
    public OperationStatus TargetStatus { get; set; }
}

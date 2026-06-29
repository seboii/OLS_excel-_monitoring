using Ols.ControlCenter.Shared.Pagination;

namespace Ols.ControlCenter.Application.Features.Alerts;

public sealed class AlertListRequest : PagedRequest
{
    public string? Status { get; set; }
    public string? Risk { get; set; }
    public string? Type { get; set; }
    public string? Group { get; set; }
}

public sealed record AlertDto(
    long Id,
    long? OperationId,
    string? OperationNo,
    string? CustomerName,
    string? BoardKey,
    string? BoardTitle,
    string? Group,
    string? RecordRef,
    string Type,
    string RiskLevel,
    string RuleCode,
    string Description,
    string Status,
    DateTimeOffset? Deadline,
    string? ResponsibleUserName,
    DateTimeOffset LastTriggeredAt,
    int TriggerCount);

public sealed class ResolveAlertRequest
{
    public string? Note { get; set; }
}

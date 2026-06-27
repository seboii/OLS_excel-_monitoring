namespace Ols.ControlCenter.Application.Features.Dashboard;

public sealed record NameValue(string Name, int Value);

public sealed record DashboardKpis(
    int TotalActive,
    int TodayLoading,
    int TodayDelivery,
    int Delayed,
    int Risky,
    int MissingDocuments,
    int Completed,
    int New24h,
    double AvgDelayDays,
    int CriticalCustomerOps,
    int TotalOperations);

public sealed record RecentActivityDto(
    long OperationId,
    string? OperationNo,
    string CustomerName,
    string Status,
    DateTimeOffset At);

public sealed record DashboardSummaryDto(
    DashboardKpis Kpis,
    IReadOnlyList<NameValue> StatusDistribution,
    IReadOnlyList<NameValue> TransportDistribution,
    IReadOnlyList<NameValue> RiskDistribution,
    IReadOnlyList<NameValue> DepartmentLoad,
    IReadOnlyList<RecentActivityDto> Recent);

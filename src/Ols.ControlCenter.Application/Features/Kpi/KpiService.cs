using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Kpi;

public sealed record BoardKpiDto(
    string Key, string Title, string Group,
    int Total, int Active, int Archived, int Delayed, int Risky, int Critical,
    double AvgDelayDays, double RiskRatio);

public sealed record GroupKpiDto(
    string Group, int Boards, int Total, int Active, int Delayed, int Risky, int Critical, double AvgDelayDays);

public interface IKpiService
{
    Task<IReadOnlyList<BoardKpiDto>> GetBoardsAsync(CancellationToken ct);
    Task<IReadOnlyList<GroupKpiDto>> GetGroupsAsync(CancellationToken ct);
}

/// <summary>
/// KPI'lar 9 sayfa-başına takip tablosundan toplanır. Kaynak veride departman/kullanıcı ataması
/// olmadığından kırılım <b>sekme (board)</b> ve <b>grup (Deniz/Kara/Hava)</b> bazındadır.
/// "Aktif" = arşiv olmayan satır.
/// </summary>
public sealed class KpiService : IKpiService
{
    private readonly ITrackingMetricsService _metrics;

    public KpiService(ITrackingMetricsService metrics) => _metrics = metrics;

    public async Task<IReadOnlyList<BoardKpiDto>> GetBoardsAsync(CancellationToken ct)
    {
        var rows = await _metrics.LoadRowsAsync(ct);
        var result = new List<BoardKpiDto>();
        foreach (var meta in BoardCatalog.All)
        {
            var b = rows.Where(r => r.BoardKey == meta.Key).ToList();
            var active = b.Where(r => !r.Archived).ToList();
            int risky = active.Count(r => r.Risk >= RiskLevel.Orange);
            result.Add(new BoardKpiDto(
                meta.Key, meta.Title, meta.Group,
                Total: b.Count,
                Active: active.Count,
                Archived: b.Count(r => r.Archived),
                Delayed: active.Count(r => r.Delay > 0),
                Risky: risky,
                Critical: active.Count(r => r.Risk >= RiskLevel.Red),
                AvgDelayDays: Math.Round(active.Where(r => r.Delay > 0).Select(r => (double)r.Delay).DefaultIfEmpty(0).Average(), 1),
                RiskRatio: active.Count == 0 ? 0 : Math.Round((double)risky / active.Count, 2)));
        }
        return result;
    }

    public async Task<IReadOnlyList<GroupKpiDto>> GetGroupsAsync(CancellationToken ct)
    {
        var rows = await _metrics.LoadRowsAsync(ct);
        var result = new List<GroupKpiDto>();
        foreach (var group in BoardCatalog.Groups)
        {
            var g = rows.Where(r => r.Group == group).ToList();
            var active = g.Where(r => !r.Archived).ToList();
            result.Add(new GroupKpiDto(
                Group: group,
                Boards: BoardCatalog.All.Count(m => m.Group == group),
                Total: g.Count,
                Active: active.Count,
                Delayed: active.Count(r => r.Delay > 0),
                Risky: active.Count(r => r.Risk >= RiskLevel.Orange),
                Critical: active.Count(r => r.Risk >= RiskLevel.Red),
                AvgDelayDays: Math.Round(active.Where(r => r.Delay > 0).Select(r => (double)r.Delay).DefaultIfEmpty(0).Average(), 1)));
        }
        return result;
    }
}

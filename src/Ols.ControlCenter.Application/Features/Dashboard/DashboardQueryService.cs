using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Dashboard;

public interface IDashboardQueryService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct);
}

/// <summary>
/// Dashboard özetini operasyonel takip tablolarından (Deniz/Kara/Hava — Finans/Alabora hariç, o
/// kendi tahsilat sayfasında ayrı metriklerle gösterilir) toplar. "Güncel" = arşiv olmayan VE durum
/// metni teslim/tamamlanma içermeyen satır (<see cref="TrackingPhase.IsCompleted"/>); "Tamamlanan"
/// = arşiv olmayan ama teslim/tamamlanmış; "Arşiv" = kaynağın kendi arşiv sayfasından gelen satırlar.
/// Risk/gecikme yalnızca güncel satırlardan hesaplanır (tamamlanmış işin riski dashboard'u şişirmez).
/// </summary>
public sealed class DashboardQueryService : IDashboardQueryService
{
    private readonly ITrackingMetricsService _metrics;

    public DashboardQueryService(ITrackingMetricsService metrics) => _metrics = metrics;

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct)
    {
        var all = await _metrics.LoadRowsAsync(ct);
        var rows = all.Where(r => BoardCatalog.OperationalGroups.Contains(r.Group)).ToList();

        var notArchived = rows.Where(r => !r.Archived).ToList();
        var current = notArchived.Where(r => !TrackingPhase.IsCompleted(r.Status)).ToList();
        var completed = notArchived.Where(r => TrackingPhase.IsCompleted(r.Status)).ToList();
        var archived = rows.Where(r => r.Archived).ToList();

        var kpis = new DashboardKpis(
            TotalRecords: rows.Count,
            Current: current.Count,
            Completed: completed.Count,
            Archived: archived.Count,
            Delayed: current.Count(r => r.Delay > 0),
            Risky: current.Count(r => r.Risk >= RiskLevel.Orange),
            Critical: current.Count(r => r.Risk >= RiskLevel.Red),
            AvgDelayDays: Math.Round(current.Where(r => r.Delay > 0).Select(r => (double)r.Delay).DefaultIfEmpty(0).Average(), 1),
            Boards: BoardCatalog.All.Count(b => BoardCatalog.OperationalGroups.Contains(b.Group)));

        var risk = current.GroupBy(r => r.Risk)
            .OrderBy(g => g.Key)
            .Select(g => new NameValue(g.Key.ToString(), g.Count())).ToList();

        var group = BoardCatalog.OperationalGroups
            .Select(gr => new NameValue(gr, current.Count(r => r.Group == gr)))
            .ToList();

        var boardLoad = current.GroupBy(r => r.BoardTitle)
            .Select(g => new NameValue(g.Key, g.Count()))
            .OrderByDescending(x => x.Value).ToList();

        var attention = current
            .Where(r => r.Risk >= RiskLevel.Yellow || r.Delay > 0)
            .OrderByDescending(r => (int)r.Risk).ThenByDescending(r => r.Delay)
            .Take(15)
            .Select(r => new AttentionItemDto(
                r.BoardKey, r.BoardTitle, r.Group, r.Ref, r.Risk.ToString(), r.Delay, r.Status))
            .ToList();

        return new DashboardSummaryDto(kpis, risk, group, boardLoad, attention);
    }
}

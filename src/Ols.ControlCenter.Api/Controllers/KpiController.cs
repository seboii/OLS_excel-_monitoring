using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Features.Kpi;
using Ols.ControlCenter.Shared.Api;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/kpi")]
public sealed class KpiController : ControllerBase
{
    private readonly IKpiService _kpi;
    private readonly IKpiSnapshotService _snapshots;

    public KpiController(IKpiService kpi, IKpiSnapshotService snapshots)
    {
        _kpi = kpi;
        _snapshots = snapshots;
    }

    [HttpGet("boards")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BoardKpiDto>>>> Boards(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BoardKpiDto>>.Ok(await _kpi.GetBoardsAsync(ct)));

    [HttpGet("groups")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupKpiDto>>>> Groups(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<GroupKpiDto>>.Ok(await _kpi.GetGroupsAsync(ct)));

    /// <summary>Son N günün global KPI trend serisi (geçmiş snapshot'lardan).</summary>
    [HttpGet("trends")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<KpiTrendPoint>>>> Trends(
        [FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<KpiTrendPoint>>.Ok(await _snapshots.GetTrendAsync(days, ct)));
}

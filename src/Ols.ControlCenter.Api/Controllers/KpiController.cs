using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Features.Kpi;
using Ols.ControlCenter.Shared.Api;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/kpi")]
public sealed class KpiController : ControllerBase
{
    private readonly IKpiService _kpi;

    public KpiController(IKpiService kpi) => _kpi = kpi;

    [HttpGet("boards")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BoardKpiDto>>>> Boards(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BoardKpiDto>>.Ok(await _kpi.GetBoardsAsync(ct)));

    [HttpGet("groups")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupKpiDto>>>> Groups(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<GroupKpiDto>>.Ok(await _kpi.GetGroupsAsync(ct)));
}

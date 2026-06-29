using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Features.Ai;
using Ols.ControlCenter.Application.Features.Dashboard;
using Ols.ControlCenter.Shared.Api;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardQueryService _dashboard;
    private readonly IAiSummaryService _ai;

    public DashboardController(IDashboardQueryService dashboard, IAiSummaryService ai)
    {
        _dashboard = dashboard;
        _ai = ai;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> Summary(CancellationToken ct)
        => Ok(ApiResponse<DashboardSummaryDto>.Ok(await _dashboard.GetSummaryAsync(ct)));

    [HttpGet("ai-summary")]
    public async Task<ActionResult<ApiResponse<AiSummaryDto>>> AiSummary(CancellationToken ct)
        => Ok(ApiResponse<AiSummaryDto>.Ok(await _ai.GenerateAsync(ct)));
}

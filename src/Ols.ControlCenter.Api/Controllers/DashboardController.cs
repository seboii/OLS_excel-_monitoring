using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Features.Dashboard;
using Ols.ControlCenter.Shared.Api;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardQueryService _dashboard;

    public DashboardController(IDashboardQueryService dashboard) => _dashboard = dashboard;

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> Summary(CancellationToken ct)
        => Ok(ApiResponse<DashboardSummaryDto>.Ok(await _dashboard.GetSummaryAsync(ct)));
}

using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Features.Finance;
using Ols.ControlCenter.Shared.Api;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/finance")]
public sealed class FinanceController : ControllerBase
{
    private readonly IFinanceSummaryService _finance;

    public FinanceController(IFinanceSummaryService finance) => _finance = finance;

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<FinanceSummaryDto>>> Summary(CancellationToken ct)
        => Ok(ApiResponse<FinanceSummaryDto>.Ok(await _finance.GetSummaryAsync(ct)));
}

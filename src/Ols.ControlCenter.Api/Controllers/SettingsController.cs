using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Features.Risk;
using Ols.ControlCenter.Shared.Api;
using Ols.ControlCenter.Shared.Authorization;

namespace Ols.ControlCenter.Api.Controllers;

/// <summary>
/// Risk eşikleri gibi çalışma-zamanı ayarları — kod değişikliği/redeploy gerektirmeden DB'den okunur/yazılır.
/// </summary>
[ApiController]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly IRiskThresholdService _thresholds;
    private readonly IRiskEngine _riskEngine;

    public SettingsController(IRiskThresholdService thresholds, IRiskEngine riskEngine)
    {
        _thresholds = thresholds;
        _riskEngine = riskEngine;
    }

    [HttpGet("risk-thresholds")]
    public async Task<ActionResult<ApiResponse<RiskThresholdsDto>>> GetRiskThresholds(CancellationToken ct)
        => Ok(ApiResponse<RiskThresholdsDto>.Ok(await _thresholds.GetAsync(ct)));

    /// <summary>
    /// Eşikleri günceller ve risk motorunu hemen yeniden çalıştırır (Alabora tahsilat uyarıları anında
    /// yansır; board satırlarının gecikme→risk seviyesi ise bir sonraki senkronizasyonda güncellenir).
    /// </summary>
    [HttpPut("risk-thresholds")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.DepartmentManager)]
    public async Task<ActionResult<ApiResponse<RiskThresholdsDto>>> UpdateRiskThresholds(
        [FromBody] RiskThresholdsDto dto, CancellationToken ct)
    {
        var result = await _thresholds.UpdateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<RiskThresholdsDto>.Fail(result.Error.Message));

        await _riskEngine.EvaluateAllAsync(ct);
        return Ok(ApiResponse<RiskThresholdsDto>.Ok(dto, "Eşikler güncellendi; risk motoru yeniden çalıştı."));
    }
}

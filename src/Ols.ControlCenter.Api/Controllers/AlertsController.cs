using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Abstractions.Realtime;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Application.Features.Alerts;
using Ols.ControlCenter.Application.Features.Risk;
using Ols.ControlCenter.Shared.Api;
using Ols.ControlCenter.Shared.Authorization;
using Ols.ControlCenter.Shared.Pagination;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public sealed class AlertsController : ControllerBase
{
    private readonly IAlertService _alerts;
    private readonly IRiskEngine _riskEngine;
    private readonly ICurrentUser _current;
    private readonly IRealtimeNotifier _realtime;

    public AlertsController(IAlertService alerts, IRiskEngine riskEngine, ICurrentUser current, IRealtimeNotifier realtime)
    {
        _alerts = alerts;
        _riskEngine = riskEngine;
        _current = current;
        _realtime = realtime;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AlertDto>>>> GetList([FromQuery] AlertListRequest req, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<AlertDto>>.Ok(await _alerts.GetListAsync(req, ct)));

    [HttpPut("{id:long}/resolve")]
    public async Task<ActionResult<ApiResponse>> Resolve(long id, [FromBody] ResolveAlertRequest req, CancellationToken ct)
    {
        if (!await _alerts.ResolveAsync(id, req.Note, _current.UserId, ct))
            return NotFound(ApiResponse.Fail("Uyarı bulunamadı."));
        await _realtime.NotifyAsync(RealtimeEvents.AlertsChanged, ct: ct);
        return Ok(ApiResponse.Ok("Uyarı çözüldü."));
    }

    /// <summary>Risk motorunu tüm aktif operasyonlar için çalıştırır (uyarıları üretir/günceller).</summary>
    [HttpPost("evaluate")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.DepartmentManager)]
    public async Task<ActionResult<ApiResponse<object>>> Evaluate(CancellationToken ct)
    {
        var count = await _riskEngine.EvaluateAllAsync(ct);
        await _realtime.NotifyAsync(RealtimeEvents.AlertsChanged, new { triggered = count }, ct);
        await _realtime.NotifyAsync(RealtimeEvents.NotificationsChanged, ct: ct);
        return Ok(ApiResponse<object>.Ok(new { triggered = count }, $"Risk motoru çalıştı, {count} kural tetiklendi."));
    }
}

using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Abstractions.Realtime;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Application.Features.Notifications;
using Ols.ControlCenter.Shared.Api;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;
    private readonly ICurrentUser _current;
    private readonly IRealtimeNotifier _realtime;

    public NotificationsController(INotificationService notifications, ICurrentUser current, IRealtimeNotifier realtime)
    {
        _notifications = notifications;
        _current = current;
        _realtime = realtime;
    }

    /// <summary>Oturum açan kullanıcının bildirimleri (en yeni önce) + okunmamış sayısı.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<NotificationListDto>>> Get(
        [FromQuery] bool unreadOnly = false, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        if (_current.UserId is not { } userId)
            return Unauthorized(ApiResponse<NotificationListDto>.Fail("Oturum bulunamadı."));
        return Ok(ApiResponse<NotificationListDto>.Ok(await _notifications.GetAsync(userId, unreadOnly, take, ct)));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> UnreadCount(CancellationToken ct)
    {
        if (_current.UserId is not { } userId)
            return Unauthorized(ApiResponse<int>.Fail("Oturum bulunamadı."));
        return Ok(ApiResponse<int>.Ok(await _notifications.GetUnreadCountAsync(userId, ct)));
    }

    [HttpPost("{id:long}/read")]
    public async Task<ActionResult<ApiResponse>> MarkRead(long id, CancellationToken ct)
    {
        if (_current.UserId is not { } userId)
            return Unauthorized(ApiResponse.Fail("Oturum bulunamadı."));
        if (!await _notifications.MarkReadAsync(userId, id, ct))
            return NotFound(ApiResponse.Fail("Bildirim bulunamadı."));
        await _realtime.NotifyAsync(RealtimeEvents.NotificationsChanged, ct: ct);
        return Ok(ApiResponse.Ok("Bildirim okundu işaretlendi."));
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllRead(CancellationToken ct)
    {
        if (_current.UserId is not { } userId)
            return Unauthorized(ApiResponse<object>.Fail("Oturum bulunamadı."));
        var count = await _notifications.MarkAllReadAsync(userId, ct);
        await _realtime.NotifyAsync(RealtimeEvents.NotificationsChanged, ct: ct);
        return Ok(ApiResponse<object>.Ok(new { marked = count }, $"{count} bildirim okundu işaretlendi."));
    }
}

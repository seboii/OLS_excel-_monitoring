using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ols.ControlCenter.Api.Realtime;

/// <summary>
/// Dashboard canlı güncelleme hub'ı. İstemciler yalnızca dinler; sunucu
/// <c>serverEvent</c> mesajıyla (olay adı + veri) yayın yapar. JWT zorunlu.
/// </summary>
[Authorize]
public sealed class DashboardHub : Hub
{
}

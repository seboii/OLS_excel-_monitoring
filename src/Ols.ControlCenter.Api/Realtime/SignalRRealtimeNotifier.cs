using Microsoft.AspNetCore.SignalR;
using Ols.ControlCenter.Application.Abstractions.Realtime;

namespace Ols.ControlCenter.Api.Realtime;

/// <summary>
/// <see cref="IRealtimeNotifier"/>'ın SignalR implementasyonu. Tüm bağlı istemcilere
/// <c>serverEvent</c> metoduyla (olay adı, veri) yayın yapar. Yayın hatası loglanır,
/// fakat çağıran iş akışını bozmaz.
/// </summary>
public sealed class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<DashboardHub> _hub;
    private readonly ILogger<SignalRRealtimeNotifier> _logger;

    public SignalRRealtimeNotifier(IHubContext<DashboardHub> hub, ILogger<SignalRRealtimeNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task NotifyAsync(string @event, object? payload = null, CancellationToken ct = default)
    {
        try
        {
            await _hub.Clients.All.SendAsync("serverEvent", @event, payload, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Canlı bildirim yayınlanamadı: {Event}", @event);
        }
    }
}

using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Ols.ControlCenter.Infrastructure.Realtime;
using StackExchange.Redis;

namespace Ols.ControlCenter.Api.Realtime;

/// <summary>
/// Worker (Hangfire) süreci, canlı olayları Redis pub/sub'a basar; bu köprü onları dinler
/// ve SignalR hub'ından bağlı istemcilere <c>serverEvent</c> olarak iletir. Böylece arka
/// planda yapılan otomatik sync'ler de dashboard'a anında yansır.
/// </summary>
public sealed class RedisRealtimeBridge : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IConnectionMultiplexer _redis;
    private readonly IHubContext<DashboardHub> _hub;
    private readonly ILogger<RedisRealtimeBridge> _logger;

    public RedisRealtimeBridge(IConnectionMultiplexer redis, IHubContext<DashboardHub> hub, ILogger<RedisRealtimeBridge> logger)
    {
        _redis = redis;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.SubscribeAsync(RedisChannel.Literal(RealtimeRedis.Channel), async (_, value) =>
            {
                try
                {
                    var msg = JsonSerializer.Deserialize<RealtimeMessage>(value!, JsonOptions);
                    if (msg is null || string.IsNullOrEmpty(msg.Event)) return;
                    object? payload = msg.Payload.HasValue ? msg.Payload.Value : null;
                    await _hub.Clients.All.SendAsync("serverEvent", msg.Event, payload, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Redis canlı olayı işlenemedi.");
                }
            });

            _logger.LogInformation("Redis canlı bildirim köprüsü dinlemede: {Channel}", RealtimeRedis.Channel);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis canlı bildirim köprüsü başlatılamadı (Redis erişilemez olabilir).");
        }
    }
}

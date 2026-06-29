using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ols.ControlCenter.Application.Abstractions.Realtime;
using StackExchange.Redis;

namespace Ols.ControlCenter.Infrastructure.Realtime;

/// <summary>Süreçler arası canlı olay köprüsünün ortak sabitleri.</summary>
public static class RealtimeRedis
{
    /// <summary>Worker'ın yayınladığı, API bridge'inin dinlediği pub/sub kanalı.</summary>
    public const string Channel = "ols:realtime";
}

/// <summary>Köprü üzerinden taşınan olay zarfı (JSON).</summary>
public sealed record RealtimeMessage(string Event, JsonElement? Payload);

/// <summary>
/// <see cref="IRealtimeNotifier"/>'ın Redis pub/sub implementasyonu — API süreci dışından
/// (Worker) yayın yapmak için. Mesajı <see cref="RealtimeRedis.Channel"/> kanalına basar;
/// API'deki köprü servisi dinleyip SignalR hub'ından istemcilere iletir.
/// Redis erişilemezse hata yutulur (iş akışı bozulmaz).
/// </summary>
public sealed class RedisRealtimeNotifier : IRealtimeNotifier
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRealtimeNotifier> _logger;

    public RedisRealtimeNotifier(IConnectionMultiplexer redis, ILogger<RedisRealtimeNotifier> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task NotifyAsync(string @event, object? payload = null, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { @event, payload });
            await _redis.GetSubscriber().PublishAsync(RedisChannel.Literal(RealtimeRedis.Channel), json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis canlı bildirim yayınlanamadı: {Event}", @event);
        }
    }
}

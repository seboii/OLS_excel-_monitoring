namespace Ols.ControlCenter.Application.Abstractions.Realtime;

/// <summary>
/// Canlı (SignalR) bildirim soyutlaması. Application/Infrastructure katmanları bu arayüz
/// üzerinden push yayını yapar; somut SignalR implementasyonu Api katmanındadır.
/// Yayın hatası iş akışını <b>asla</b> bozmamalıdır (implementasyon yutar).
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>Tüm bağlı istemcilere adlandırılmış bir olay yayınlar.</summary>
    /// <param name="event">Olay adı — bkz. <see cref="RealtimeEvents"/>.</param>
    /// <param name="payload">İsteğe bağlı veri (ör. operasyon no, etkilenen sayı).</param>
    Task NotifyAsync(string @event, object? payload = null, CancellationToken ct = default);
}

/// <summary>Canlı olay adları. Frontend bu adlara göre TanStack Query önbelleğini geçersiz kılar.</summary>
public static class RealtimeEvents
{
    /// <summary>Uyarı/risk durumu değişti (üretildi, çözüldü, güncellendi).</summary>
    public const string AlertsChanged = "alerts-changed";

    /// <summary>Görev oluşturuldu/güncellendi.</summary>
    public const string TasksChanged = "tasks-changed";

    /// <summary>Operasyon yorumu eklendi/iptal edildi.</summary>
    public const string CommentsChanged = "comments-changed";

    /// <summary>Veri kaynağından senkron tamamlandı (operasyon/KPI verileri tazelenmeli).</summary>
    public const string DataSynced = "data-synced";

    /// <summary>Kullanıcı bildirimi oluştu/okundu (header zil rozeti ve liste tazelenmeli).</summary>
    public const string NotificationsChanged = "notifications-changed";
}

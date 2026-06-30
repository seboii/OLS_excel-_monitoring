using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Notifications;

public sealed record NotificationDto(
    long Id, string Type, string Level, string Title, string Body,
    bool IsRead, string? RelatedEntityType, long? RelatedEntityId, DateTimeOffset CreatedAt);

public sealed record NotificationListDto(IReadOnlyList<NotificationDto> Items, int UnreadCount);

public interface INotificationService
{
    Task<NotificationListDto> GetAsync(long userId, bool unreadOnly, int take, CancellationToken ct);
    Task<int> GetUnreadCountAsync(long userId, CancellationToken ct);
    Task<bool> MarkReadAsync(long userId, long id, CancellationToken ct);
    Task<int> MarkAllReadAsync(long userId, CancellationToken ct);

    /// <summary>
    /// Tüm aktif kullanıcılara bir bildirim ekler — <b>SaveChanges çağırmaz</b>; çağıran kendi
    /// transaction'ında kaydeder (ör. <c>RiskEngine</c> değerlendirme sonunda tek seferde).
    /// </summary>
    Task EnqueueForAllActiveUsersAsync(
        NotificationType type, NotificationLevel level, string title, string body,
        string? relatedEntityType, long? relatedEntityId, CancellationToken ct);
}

/// <summary>
/// Uygulama içi bildirim merkezi. Risk motoru yeni kritik/uyarı üretince digest bildirim oluşturur;
/// header'daki zil bunları okur. Bildirimler kullanıcı-başına saklanır (<see cref="Notification.UserId"/>).
/// </summary>
public sealed class NotificationService : INotificationService
{
    private const int MaxBodyLength = 2000;
    private readonly IApplicationDbContext _db;

    public NotificationService(IApplicationDbContext db) => _db = db;

    public async Task<NotificationListDto> GetAsync(long userId, bool unreadOnly, int take, CancellationToken ct)
    {
        var q = _db.Notifications.AsNoTracking().Where(n => n.UserId == userId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);

        var items = await q
            .OrderByDescending(n => n.CreatedAt)
            .Take(Math.Clamp(take, 1, 100))
            .Select(n => new NotificationDto(
                n.Id, n.Type.ToString(), n.Level.ToString(), n.Title, n.Body,
                n.IsRead, n.RelatedEntityType, n.RelatedEntityId, n.CreatedAt))
            .ToListAsync(ct);

        var unread = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);
        return new NotificationListDto(items, unread);
    }

    public Task<int> GetUnreadCountAsync(long userId, CancellationToken ct)
        => _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task<bool> MarkReadAsync(long userId, long id, CancellationToken ct)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (n is null) return false;
        if (!n.IsRead)
        {
            n.IsRead = true;
            n.ReadAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        return true;
    }

    public async Task<int> MarkAllReadAsync(long userId, CancellationToken ct)
    {
        var unread = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        foreach (var n in unread) { n.IsRead = true; n.ReadAt = now; }
        if (unread.Count > 0) await _db.SaveChangesAsync(ct);
        return unread.Count;
    }

    public async Task EnqueueForAllActiveUsersAsync(
        NotificationType type, NotificationLevel level, string title, string body,
        string? relatedEntityType, long? relatedEntityId, CancellationToken ct)
    {
        var userIds = await _db.Users.Where(u => u.IsActive).Select(u => u.Id).ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var trimmedBody = body.Length > MaxBodyLength ? body[..MaxBodyLength] : body;

        foreach (var uid in userIds)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = uid,
                Type = type,
                Level = level,
                Title = title.Length > 200 ? title[..200] : title,
                Body = trimmedBody,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                CreatedAt = now,
            });
        }
    }
}

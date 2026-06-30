using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Features.Notifications;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.UnitTests;

/// <summary>
/// Bildirim merkezi: digest yalnızca aktif kullanıcılara eklenmeli; okundu/okunmamış sayacı doğru olmalı.
/// </summary>
public class NotificationServiceTests
{
    [Fact]
    public async Task Enqueue_OnlyActiveUsers_AndReadFlow()
    {
        var db = InMemoryDb.Create();
        var now = DateTimeOffset.UtcNow;
        db.Users.AddRange(
            new User { FullName = "A", Email = "a@x", PasswordHash = "x", IsActive = true, CreatedAt = now },
            new User { FullName = "B", Email = "b@x", PasswordHash = "x", IsActive = true, CreatedAt = now },
            new User { FullName = "C", Email = "c@x", PasswordHash = "x", IsActive = false, CreatedAt = now });
        await db.SaveChangesAsync();

        var svc = new NotificationService(db);
        await svc.EnqueueForAllActiveUsersAsync(
            NotificationType.CriticalOperation, NotificationLevel.Critical, "Başlık", "Gövde", "Alert", null, default);
        await db.SaveChangesAsync(); // motor normalde tek transaction'da kaydeder

        // 2 aktif kullanıcı → 2 bildirim (pasif C hariç)
        Assert.Equal(2, await db.Notifications.CountAsync());

        var aId = (await db.Users.FirstAsync(u => u.Email == "a@x")).Id;
        Assert.Equal(1, await svc.GetUnreadCountAsync(aId, default));

        var list = await svc.GetAsync(aId, unreadOnly: false, take: 20, default);
        Assert.Single(list.Items);
        Assert.Equal(1, list.UnreadCount);

        Assert.True(await svc.MarkReadAsync(aId, list.Items[0].Id, default));
        Assert.Equal(0, await svc.GetUnreadCountAsync(aId, default));
    }

    [Fact]
    public async Task MarkAllRead_ClearsUnread()
    {
        var db = InMemoryDb.Create();
        db.Users.Add(new User { FullName = "A", Email = "a@x", PasswordHash = "x", IsActive = true, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        var uid = (await db.Users.FirstAsync()).Id;

        var svc = new NotificationService(db);
        await svc.EnqueueForAllActiveUsersAsync(NotificationType.SyncFailed, NotificationLevel.Warning, "t", "b", null, null, default);
        await svc.EnqueueForAllActiveUsersAsync(NotificationType.SyncFailed, NotificationLevel.Warning, "t2", "b2", null, null, default);
        await db.SaveChangesAsync();

        Assert.Equal(2, await svc.GetUnreadCountAsync(uid, default));
        var marked = await svc.MarkAllReadAsync(uid, default);
        Assert.Equal(2, marked);
        Assert.Equal(0, await svc.GetUnreadCountAsync(uid, default));
    }
}

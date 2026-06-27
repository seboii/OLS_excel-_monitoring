using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Kullanıcıya gönderilen uygulama içi bildirim.</summary>
public class Notification : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public NotificationType Type { get; set; }
    public NotificationLevel Level { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    public string? RelatedEntityType { get; set; }
    public long? RelatedEntityId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

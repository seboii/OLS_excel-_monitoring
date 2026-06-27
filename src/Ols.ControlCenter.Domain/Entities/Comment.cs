using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// Operasyon yorumu. Fiziksel olarak silinmez; yalnızca "iptal edildi" işaretlenir
/// (operasyon geçmişi korunsun diye).
/// </summary>
public class Comment : AuditableEntity
{
    public long OperationId { get; set; }
    public Operation Operation { get; set; } = null!;

    public long AuthorUserId { get; set; }
    public User Author { get; set; } = null!;

    public CommentType Type { get; set; } = CommentType.General;
    public string Body { get; set; } = string.Empty;

    /// <summary>Etiketlenen kişiler/departmanlar (@Finans, @Deniz, @Muhammet...).</summary>
    public List<string> Mentions { get; set; } = new();

    public bool IsCancelled { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public long? CancelledByUserId { get; set; }
}

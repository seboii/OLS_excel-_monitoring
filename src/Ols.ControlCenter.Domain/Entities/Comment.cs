using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>
/// Operasyon veya takip tablosu (board) satırı yorumu. Fiziksel olarak silinmez; yalnızca
/// "iptal edildi" işaretlenir (geçmiş korunsun diye). İki kaynaktan biri doludur: eski
/// <see cref="Operation"/> modeli VEYA bir board satırı (<see cref="BoardKey"/>+<see cref="RecordRef"/>) —
/// bkz. <see cref="Alert"/>'teki aynı polimorfik desen.
/// </summary>
public class Comment : AuditableEntity
{
    public long? OperationId { get; set; }
    public Operation? Operation { get; set; }

    /// <summary>Takip tablosu sekme anahtarı (örn. "deniz-transit"). Board-bound yorumlarda dolu.</summary>
    public string? BoardKey { get; set; }
    public string? BoardTitle { get; set; }
    public string? Group { get; set; }

    /// <summary>Kaynak satırın dosya/ref numarası. Board-bound yorumlarda dolu.</summary>
    public string? RecordRef { get; set; }

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

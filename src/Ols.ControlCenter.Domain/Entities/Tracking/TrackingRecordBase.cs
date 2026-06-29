using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Domain.Entities.Tracking;

/// <summary>
/// Sayfa-başına takip tablolarının ortak temel sınıfı. Her somut tip kendi DB tablosuna eşlenir
/// (EF kalıtım hiyerarşisi yok); ortak senkron/risk alanları buradan miras alınır.
/// Kaynak sayfadaki tüm kolonlar somut tipte birebir tutulur (veri kaybı yok); ayrıca
/// <see cref="RawJson"/> eşlenmemiş kolonlar dahil ham satırı saklar.
/// </summary>
public abstract class TrackingRecordBase : BaseEntity
{
    /// <summary>Bu satırın geldiği veri kaynağı (sayfa).</summary>
    public long DataSourceId { get; set; }

    /// <summary>Kaynak satır anahtarı (DOSYA NO / REF NO). Boşsa "#satırIndeksi" üretilir.</summary>
    public string SourceRowKey { get; set; } = string.Empty;

    /// <summary>Kaynak sayfadaki 0-tabanlı satır sırası.</summary>
    public int RowIndex { get; set; }

    /// <summary>Serbest metinden (NOT / ARAÇ KONUMU) türetilen okunur durum.</summary>
    public string? StatusText { get; set; }

    /// <summary>Risk motoru çıktısı (gecikme/anahtar kelime). Green &lt; Yellow &lt; Orange &lt; Red &lt; Black.</summary>
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Green;

    /// <summary>Birincil ETA'ya göre gecikme günü (yoksa 0).</summary>
    public int DelayDays { get; set; }

    /// <summary>Arşiv sayfasından gelen geçmiş kayıtlar için true.</summary>
    public bool IsArchived { get; set; }

    /// <summary>Satırın ham hali (başlık → değer) JSON — eşlenmeyen kolonlar dahil, kayıpsız.</summary>
    public string RawJson { get; set; } = "{}";

    public DateTimeOffset ImportedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

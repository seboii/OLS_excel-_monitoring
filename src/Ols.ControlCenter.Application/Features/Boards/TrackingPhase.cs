namespace Ols.ControlCenter.Application.Features.Boards;

/// <summary>
/// Bir takip satırının "geçmiş" (tamamlanmış/teslim olmuş) mı yoksa "güncel" (devam eden) mi olduğunu
/// serbest durum metninden tahmin eder. Dashboard güncel/geçmiş ayrımı buradan beslenir.
/// </summary>
public static class TrackingPhase
{
    // Teslim / boşaltma / tamamlanma sinyalleri (TR + RU). Kaynak metin çoğunlukla büyük harf.
    private static readonly string[] CompletedKeywords =
    {
        "TESLİM", "TESLIM", "BOŞALD", "BOSALD", "TAHLİYE", "TAHLIYE", "İNDİRİLD", "INDIRILD",
        "VARDI", "GELDİ", "GELDI", "TAMAMLAND", "ÇIKTI", "CIKTI",
        "ВЫГРУЖ", "ЗАВЕРШ", "ДОСТАВЛ", "ПРИБЫ",
    };

    public static bool IsCompleted(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return false;
        var u = status.ToUpperInvariant();
        return CompletedKeywords.Any(k => u.Contains(k, StringComparison.Ordinal));
    }
}

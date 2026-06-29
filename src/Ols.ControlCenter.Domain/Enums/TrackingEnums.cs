namespace Ols.ControlCenter.Domain.Enums;

/// <summary>
/// Bir veri kaynağının hangi takip tablosunu beslediğini belirler.
/// Her değer ayrı bir DB tablosuna ve frontend sekmesine karşılık gelir.
/// <see cref="None"/> = klasik standart <c>Operations</c> modeline yazılır (eski akış korunur).
/// </summary>
public enum TrackingBoardType
{
    None = 0,
    SeaTransit,   // DENİZYOLU TRANSİT
    SeaImport,    // İTHALAT
    SeaExport,    // İHRACAT
    RoadTransit,  // KARAYOLU TRANSİT (deniz takip dosyası içinde)
    RoadLoad,     // YOLDAKİ YÜKLER (Avrupa ithalat karayolu)
    RoadArchive,  // MURATBEY KERRY & MİRLOG VARIŞ (arşiv)
    Alabora,      // Alabora — СЧЕТА-ПЛАТЕЖИ (fatura/tahsilat)
    Air,          // Hava — OPERASYON BİLGİLERİ (temiz)
    AirDaily,     // Hava — GÜNLÜK LİSTE (kart yapısı, best-effort)
}

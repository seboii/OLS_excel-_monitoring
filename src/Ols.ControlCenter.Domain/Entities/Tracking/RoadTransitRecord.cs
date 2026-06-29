namespace Ols.ControlCenter.Domain.Entities.Tracking;

/// <summary>
/// "KARAYOLU TRANSİT" sayfası (deniz takip dosyası içinde) — limana karayoluyla giden,
/// konteyner/booking bazlı transit yükler.
/// </summary>
public class RoadTransitRecord : TrackingRecordBase
{
    public DateOnly? Date { get; set; }                  // TARİH
    public string? Shipper { get; set; }                 // SHIPPER
    public string? Consignee { get; set; }               // (CONSIGNEE)
    public string? OriginCountry { get; set; }           // ÇIKIŞ ÜLKESİ
    public string? Plate { get; set; }                   // PLAKA
    public string? Term { get; set; }                    // TERM
    public string? Line { get; set; }                    // LINE
    public string? Booking { get; set; }                 // BOOKING
    public string? ContainerNo { get; set; }             // CONTAINER NO
    public string? EmptyContainerTransfer { get; set; }  // BOŞ AKTARILACAK KONT.
    public string? Pol { get; set; }                     // POL
    public string? Pod { get; set; }                     // POD
    public DateOnly? Eta { get; set; }                   // ETA
    public DateOnly? CutOff { get; set; }                // CUT OFF
    public string? Invoice { get; set; }                 // FATURA
    public string? Note { get; set; }                    // NOT
    public string? Note2 { get; set; }                   // NOT 2
}

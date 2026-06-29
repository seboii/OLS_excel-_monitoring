namespace Ols.ControlCenter.Domain.Entities.Tracking;

/// <summary>
/// "DENİZYOLU TRANSİT" sayfası — ithalat + ihracat bacaklı transit deniz taşımaları.
/// ETD/ETA iki kez geçer: ithalat bacağı (geliş) ve ihracat bacağı (sevk).
/// </summary>
public class SeaTransitRecord : TrackingRecordBase
{
    public string? Shipper { get; set; }                 // SHIPPER
    public string? Consignee { get; set; }               // (CONSIGNEE)
    public string? Line { get; set; }                    // LINE
    public string? Term { get; set; }                    // TERM (incoterm)
    public string? Agent { get; set; }                   // ACENTE
    public string? AgentRef { get; set; }                // ACENTE REF.
    public string? ContainerKind { get; set; }           // KONT.TIP
    public string? ImportBooking { get; set; }           // İHALAT BOOKING (kaynak yazımı korunur)
    public string? ContainerNo { get; set; }             // CONTAINER NO
    public string? IncomingContainer { get; set; }       // GELEN KONTEYNER
    public string? Pol { get; set; }                     // POL
    public string? Pod { get; set; }                     // POD
    public DateOnly? ImportEtd { get; set; }             // ETD
    public DateOnly? ImportEta { get; set; }             // ETA
    public string? TransferPoint { get; set; }           // TO (aktarma noktası/ofisi)
    public string? ExportBooking { get; set; }           // İHRACAT BOOKING
    public string? EmptyContainerTransfer { get; set; }  // BOŞ AKTARILACAK KONT.
    public DateOnly? CutOff { get; set; }                // CUT OFF
    public DateOnly? ExportEtd { get; set; }             // ETD (2. bacak)
    public DateOnly? ExportEta { get; set; }             // ETA (2. bacak)
    public string? Invoice { get; set; }                 // FATURA
    public string? Note { get; set; }                    // NOT
    public string? Note2 { get; set; }                   // NOT 2
}

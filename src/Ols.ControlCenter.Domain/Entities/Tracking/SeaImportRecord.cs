namespace Ols.ControlCenter.Domain.Entities.Tracking;

/// <summary>"İTHALAT" sayfası — deniz ithalat dosyaları.</summary>
public class SeaImportRecord : TrackingRecordBase
{
    public string? Shipper { get; set; }         // SHIPPER
    public string? Consignee { get; set; }       // (CONSIGNEE)
    public string? Line { get; set; }            // LINE
    public string? Term { get; set; }            // TERM
    public string? Agent { get; set; }           // ACENTE
    public string? AgentRef { get; set; }        // ACENTE REF.
    public string? ContainerKind { get; set; }   // KONT.TIP
    public string? Booking { get; set; }         // BOOKING
    public string? ContainerNo { get; set; }     // CONTAINER NO
    public string? Pol { get; set; }             // POL
    public string? Pod { get; set; }             // POD
    public DateOnly? Etd { get; set; }           // ETD
    public DateOnly? Eta { get; set; }           // ETA
    public string? Invoice { get; set; }         // FATURA
    public string? Note { get; set; }            // NOT
    public string? Note2 { get; set; }           // NOT 2
}

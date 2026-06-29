namespace Ols.ControlCenter.Domain.Entities.Tracking;

/// <summary>Hava "OPERASYON BİLGİLERİ" sayfası — temiz hava operasyon takibi. Anahtar: Referance Number.</summary>
public class AirOperationRecord : TrackingRecordBase
{
    public string? Sender { get; set; }          // Gönderici
    public string? Flight { get; set; }          // Uçuş
    public DateOnly? WarehouseEntry { get; set; }// Ambar Girişi
    public DateOnly? OptionDate { get; set; }    // Opsiyon Tarihi
    public string? OptionTime { get; set; }      // Opsiyon Saati
    public string? Airline { get; set; }         // Airline
    public string? Warehouse { get; set; }       // Ambar
    public string? ReferenceNumber { get; set; } // Referance Number
    public string? Sn { get; set; }              // SN
    public string? Archive { get; set; }         // ARŞİV
    public string? ColA { get; set; }            // A (örn. GİRİLDİ)
    public string? ColS { get; set; }            // S (örn. KESİLDİ)
    public string? Notes { get; set; }           // NOTLAR
}

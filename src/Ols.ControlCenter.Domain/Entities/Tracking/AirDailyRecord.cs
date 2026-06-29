namespace Ols.ControlCenter.Domain.Entities.Tracking;

/// <summary>
/// Hava "GÜNLÜK LİSTE" sayfası — zengin kolonlar (MAWB/HAWB/Statü/Incoterm), ancak kaynak kart yapısında
/// olduğundan best-effort parse edilir (anahtarı/MAWB'i olmayan kart satırları atlanır). Anahtar: Referance Number.
/// </summary>
public class AirDailyRecord : TrackingRecordBase
{
    public string? UserId { get; set; }          // User ID
    public string? MawbNo { get; set; }          // MAWB No
    public string? HawbNo { get; set; }          // HAWB No
    public string? Airport { get; set; }         // Havalimanı
    public string? Destination { get; set; }     // Varış
    public string? PieceCount { get; set; }      // Parça Sayısı
    public string? Kgs { get; set; }             // KGS
    public string? Status { get; set; }          // Statü
    public string? Incoterm { get; set; }        // Incoterm
    public string? Sender { get; set; }          // Gönderici
    public string? Flight { get; set; }          // Uçuş
    public DateOnly? WarehouseEntry { get; set; }// Ambar Girişi
    public DateOnly? OptionDate { get; set; }    // Opsiyon Tarihi
    public string? OptionTime { get; set; }      // Opsiyon Saati
    public string? Airline { get; set; }         // Airline
    public string? Warehouse { get; set; }       // Ambar
    public string? ReferenceNumber { get; set; } // Referance Number
    public string? Carrier { get; set; }         // Havayolu
    public string? Flag { get; set; }            // Bayrak
    public string? WarehouseCode { get; set; }   // Ambar Code
    public string? Authorized { get; set; }      // Yetkili
    public string? Address { get; set; }         // ADRES
    public string? ShipmentNumber { get; set; }  // Shipment Number
}

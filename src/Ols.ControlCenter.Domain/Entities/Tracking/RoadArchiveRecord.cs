namespace Ols.ControlCenter.Domain.Entities.Tracking;

/// <summary>
/// "MURATBEY KERRY &amp; MİRLOG VARIŞ" sayfası — karayolu varış arşivi (geçmiş kayıtlar).
/// ÇIKIŞ/ORDİNO TARİHİ alanları kaynakta tarih veya serbest not olabildiğinden metin tutulur.
/// </summary>
public class RoadArchiveRecord : TrackingRecordBase
{
    public string? DepartureDate { get; set; }      // ÇIKIŞ TARİHİ (serbest metin olabilir)
    public string? ImportCountry { get; set; }      // İTH ÜLKESİ
    public string? Sender { get; set; }             // GÖNDERİCİ
    public string? Receiver { get; set; }           // ALICI
    public string? Plate { get; set; }              // PLAKA
    public string? ProductType { get; set; }        // ÜRÜN CİNSİ
    public string? PackageCount { get; set; }       // KAP ADEDİ
    public string? Weight { get; set; }             // KG
    public string? Stackable { get; set; }          // İSTİFLENEBİLİR ?
    public string? OrderDate { get; set; }          // ORDİNO TARİHİ (serbest metin olabilir)
    public string? ArrivalWarehouse { get; set; }   // VARIŞ ANTR
    public string? PurchaseFreight { get; set; }    // ALIŞ NAVLUNU
    public string? YdgIncluded { get; set; }        // YDG DAHİL
    public string? Supplier { get; set; }           // TEDARİKÇİ
}

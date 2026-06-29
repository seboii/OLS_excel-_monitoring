namespace Ols.ControlCenter.Domain.Entities.Tracking;

/// <summary>
/// "YOLDAKİ YÜKLER" sayfası — Avrupa ithalat karayolu, yolda olan aktif yükler.
/// ÇIKIŞ TARİHİ alanı kaynakta hem tarih hem serbest metin içerebildiğinden metin tutulur.
/// </summary>
public class RoadLoadRecord : TrackingRecordBase
{
    public string? CustomerRep { get; set; }       // MÜŞTERİ TEMSİLCİSİ
    public string? DepartureDate { get; set; }      // ÇIKIŞ TARİHİ (serbest metin olabilir)
    public string? VehicleLocation { get; set; }    // ARAÇ KONUMU (durum)
    public string? ImportCountry { get; set; }      // İTH ÜLKESİ
    public string? Sender { get; set; }             // GÖNDERİCİ
    public string? Receiver { get; set; }           // ALICI
    public string? Plate { get; set; }              // PLAKA
    public string? ProductType { get; set; }        // ÜRÜN CİNSİ
    public string? PackageCount { get; set; }       // KAP ADEDİ
    public string? Weight { get; set; }             // KG
    public string? Stackable { get; set; }          // İSTİFLENEBİLİR ?
    public string? ArrivalWarehouse { get; set; }   // VARIŞ ANTR
    public string? Freight { get; set; }            // NAVLUN
    public string? Ydg { get; set; }                // YDG
    public string? Supplier { get; set; }           // TEDARİKÇİ
}

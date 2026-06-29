using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Boards;

/// <summary>Bir takip tablosundaki tek kolonun frontend metadata'sı.</summary>
public sealed record BoardColumn(string Key, string Label, string Type); // type: text | date | number

/// <summary>Bir takip tablosunun (sayfa = sekme) tanımı: anahtar, başlık, grup (Deniz/Kara/Hava) ve kolonlar.</summary>
public sealed record BoardMeta(
    string Key, string Title, string Group, TrackingBoardType Board, IReadOnlyList<BoardColumn> Columns);

/// <summary>
/// Sayfa-başına takip tablolarının tek kaynaklı kataloğu: sekme anahtarı, başlık, grup ve kolon metadata'sı.
/// Hem API (satır projeksiyonu) hem de metrik servisi (grup eşlemesi) buradan beslenir.
/// Kolon <c>Key</c>'leri ilgili entity'nin C# property adıyla birebir aynıdır (yansımayla okunur).
/// </summary>
public static class BoardCatalog
{
    private static BoardColumn T(string key, string label) => new(key, label, "text");
    private static BoardColumn D(string key, string label) => new(key, label, "date");

    public static readonly IReadOnlyList<BoardMeta> All = new List<BoardMeta>
    {
        new("deniz-transit", "Denizyolu Transit", "Deniz", TrackingBoardType.SeaTransit, new[]
        {
            T("Shipper","Shipper"), T("Consignee","Consignee"), T("Line","Line"), T("Term","Term"),
            T("Agent","Acente"), T("AgentRef","Acente Ref."), T("ContainerKind","Kont. Tip"),
            T("ImportBooking","İthalat Booking"), T("ContainerNo","Konteyner No"), T("IncomingContainer","Gelen Konteyner"),
            T("Pol","POL"), T("Pod","POD"), D("ImportEtd","ETD (İth.)"), D("ImportEta","ETA (İth.)"),
            T("TransferPoint","TO (Aktarma)"), T("ExportBooking","İhracat Booking"),
            T("EmptyContainerTransfer","Boş Aktarılacak Kont."), D("CutOff","Cut Off"),
            D("ExportEtd","ETD (İhr.)"), D("ExportEta","ETA (İhr.)"),
            T("Invoice","Fatura"), T("Note","Not"), T("Note2","Not 2"),
        }),
        new("deniz-ithalat", "İthalat", "Deniz", TrackingBoardType.SeaImport, new[]
        {
            T("Shipper","Shipper"), T("Consignee","Consignee"), T("Line","Line"), T("Term","Term"),
            T("Agent","Acente"), T("AgentRef","Acente Ref."), T("ContainerKind","Kont. Tip"),
            T("Booking","Booking"), T("ContainerNo","Konteyner No"), T("Pol","POL"), T("Pod","POD"),
            D("Etd","ETD"), D("Eta","ETA"), T("Invoice","Fatura"), T("Note","Not"), T("Note2","Not 2"),
        }),
        new("deniz-ihracat", "İhracat", "Deniz", TrackingBoardType.SeaExport, new[]
        {
            T("Shipper","Shipper"), T("Consignee","Consignee"), T("Line","Line"), T("Term","Term"),
            T("ContainerKind","Kont. Tip"), T("Booking","Booking"), T("ContainerNo","Konteyner No"),
            T("Pol","POL"), T("Pod","POD"), D("CutOff","Cut Off"), D("Etd","ETD"), D("Eta","ETA"),
            T("Invoice","Fatura"), T("Note","Not"), T("Note2","Not 2"),
        }),
        new("deniz-karayolu-transit", "Karayolu Transit", "Deniz", TrackingBoardType.RoadTransit, new[]
        {
            D("Date","Tarih"), T("Shipper","Shipper"), T("Consignee","Consignee"),
            T("OriginCountry","Çıkış Ülkesi"), T("Plate","Plaka"), T("Term","Term"), T("Line","Line"),
            T("Booking","Booking"), T("ContainerNo","Konteyner No"), T("EmptyContainerTransfer","Boş Aktarılacak Kont."),
            T("Pol","POL"), T("Pod","POD"), D("Eta","ETA"), D("CutOff","Cut Off"),
            T("Invoice","Fatura"), T("Note","Not"), T("Note2","Not 2"),
        }),
        new("finans-alabora", "Alabora (Tahsilat)", "Finans", TrackingBoardType.Alabora, new[]
        {
            T("No","№"), D("FtDate","FT. Tarih"), T("CompanyTitle","Firma Ünvanı"), T("Voyage","Sefer"),
            T("Amount","Tutar"), T("Currency","Döviz"), T("CargoStatus","Yük/Boşaltma Durumu"),
            T("DocsReadiness","Belge Hazırlığı"), T("InvoiceMarked","İşaretli Fatura"),
            T("TransportDocs","Taşıma Belgeleri"), T("OrderContract","Poruçeniye/Sözleşme"),
            T("CommentOsh","Yorum (ОШ)"), T("CommentOls","Yorum (ОЛС)"), T("IncomingPayments","Gelen Ödemeler"),
            T("Collection","Tahsilat"), D("PaymentDate","Ödeme Tarihi"), T("Rub","RUB"),
            T("LoadingType","Yükleme Cinsi"), T("LoadingDetails","Yükleme Detayı"),
            T("CustomerRep","Müşteri Temsilcisi"), T("RateNet","Rate Net"),
        }),

        new("kara-yoldaki", "Yoldaki Yükler", "Kara", TrackingBoardType.RoadLoad, new[]
        {
            T("CustomerRep","Müşteri Temsilcisi"), T("DepartureDate","Çıkış Tarihi"), T("VehicleLocation","Araç Konumu"),
            T("ImportCountry","İth. Ülkesi"), T("Sender","Gönderici"), T("Receiver","Alıcı"), T("Plate","Plaka"),
            T("ProductType","Ürün Cinsi"), T("PackageCount","Kap Adedi"), T("Weight","KG"),
            T("Stackable","İstiflenebilir?"), T("ArrivalWarehouse","Varış Antr."), T("Freight","Navlun"),
            T("Ydg","YDG"), T("Supplier","Tedarikçi"),
        }),
        new("kara-arsiv", "Arşiv (Muratbey/Kerry/Mirlog)", "Kara", TrackingBoardType.RoadArchive, new[]
        {
            T("DepartureDate","Çıkış Tarihi"), T("ImportCountry","İth. Ülkesi"), T("Sender","Gönderici"),
            T("Receiver","Alıcı"), T("Plate","Plaka"), T("ProductType","Ürün Cinsi"), T("PackageCount","Kap Adedi"),
            T("Weight","KG"), T("Stackable","İstiflenebilir?"), T("OrderDate","Ordino Tarihi"),
            T("ArrivalWarehouse","Varış Antr."), T("PurchaseFreight","Alış Navlunu"), T("YdgIncluded","YDG Dahil"),
            T("Supplier","Tedarikçi"),
        }),

        new("hava-operasyon", "Operasyon Bilgileri", "Hava", TrackingBoardType.Air, new[]
        {
            T("Sender","Gönderici"), T("Flight","Uçuş"), D("WarehouseEntry","Ambar Girişi"),
            D("OptionDate","Opsiyon Tarihi"), T("OptionTime","Opsiyon Saati"), T("Airline","Airline"),
            T("Warehouse","Ambar"), T("ReferenceNumber","Referans No"), T("Sn","SN"),
            T("Archive","Arşiv"), T("ColA","A"), T("ColS","S"), T("Notes","Notlar"),
        }),
        new("hava-gunluk", "Günlük Liste", "Hava", TrackingBoardType.AirDaily, new[]
        {
            T("UserId","User ID"), T("MawbNo","MAWB No"), T("HawbNo","HAWB No"), T("Airport","Havalimanı"),
            T("Destination","Varış"), T("PieceCount","Parça Sayısı"), T("Kgs","KGS"), T("Status","Statü"),
            T("Incoterm","Incoterm"), T("Sender","Gönderici"), T("Flight","Uçuş"), D("WarehouseEntry","Ambar Girişi"),
            D("OptionDate","Opsiyon Tarihi"), T("OptionTime","Opsiyon Saati"), T("Airline","Airline"),
            T("Warehouse","Ambar"), T("ReferenceNumber","Referans No"), T("Carrier","Havayolu"),
            T("Flag","Bayrak"), T("WarehouseCode","Ambar Code"), T("Authorized","Yetkili"),
            T("Address","Adres"), T("ShipmentNumber","Shipment No"),
        }),
    };

    public static BoardMeta? Find(string key) => All.FirstOrDefault(b => b.Key == key);

    public static BoardMeta? ForBoard(TrackingBoardType board) => All.FirstOrDefault(b => b.Board == board);

    /// <summary>Operasyonel taşıma grupları — dashboard/KPI bunları toplar (Finans hariç).</summary>
    public static readonly IReadOnlyList<string> OperationalGroups = new[] { "Deniz", "Kara", "Hava" };

    /// <summary>Tüm frontend grupları (navigasyon için).</summary>
    public static readonly IReadOnlyList<string> Groups = new[] { "Deniz", "Kara", "Hava", "Finans" };
}

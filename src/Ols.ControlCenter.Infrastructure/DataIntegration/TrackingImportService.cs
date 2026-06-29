using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Features.Risk;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Entities.Tracking;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>
/// Kaynak satırlarını, kaynağın <see cref="DataSource.TargetBoard"/> değerine göre ilgili sayfa-tablosuna
/// yazar. Strateji: kaynak için mevcut satırları sil + tazele (canlı ayna). Başlık eşleştirme normalize
/// edilmiş (NBSP/boşluk/büyük-küçük harf toleranslı). Eşlenmeyen kolonlar dahil ham satır
/// <see cref="TrackingRecordBase.RawJson"/>'a kayıpsız yazılır. Gecikme→risk eşikleri
/// <see cref="IRiskThresholdService"/> üzerinden DB'den okunur (UI'dan ayarlanabilir, kod sabiti değil).
/// </summary>
public sealed class TrackingImportService : ITrackingImportService
{
    private readonly IApplicationDbContext _db;
    private readonly IRiskThresholdService _thresholds;

    public TrackingImportService(IApplicationDbContext db, IRiskThresholdService thresholds)
    {
        _db = db;
        _thresholds = thresholds;
    }

    public Task<TrackingImportSummary> ImportAsync(
        DataSource source, IReadOnlyList<IReadOnlyDictionary<string, string?>> rows, CancellationToken ct)
        => source.TargetBoard switch
        {
            TrackingBoardType.SeaTransit =>
                ReplaceAsync(_db.SeaTransitRecords, source.Id, false, rows, MapSeaTransit, e => e.ImportEta, ct),
            TrackingBoardType.SeaImport =>
                ReplaceAsync(_db.SeaImportRecords, source.Id, false, rows, MapSeaImport, e => e.Eta, ct),
            TrackingBoardType.SeaExport =>
                ReplaceAsync(_db.SeaExportRecords, source.Id, false, rows, MapSeaExport, e => e.Eta, ct),
            TrackingBoardType.RoadTransit =>
                ReplaceAsync(_db.RoadTransitRecords, source.Id, false, rows, MapRoadTransit, e => e.Eta, ct),
            TrackingBoardType.RoadLoad =>
                ReplaceAsync(_db.RoadLoadRecords, source.Id, false, rows, MapRoadLoad, _ => null, ct),
            TrackingBoardType.RoadArchive =>
                ReplaceAsync(_db.RoadArchiveRecords, source.Id, true, rows, MapRoadArchive, _ => null, ct),
            TrackingBoardType.Alabora =>
                ReplaceAsync(_db.AlaboraFinanceRecords, source.Id, false, rows, MapAlabora, _ => null, ct),
            TrackingBoardType.Air =>
                ReplaceAsync(_db.AirOperationRecords, source.Id, false, rows, MapAirOperation, _ => null, ct),
            TrackingBoardType.AirDaily =>
                ReplaceAsync(_db.AirDailyRecords, source.Id, false, rows, MapAirDaily, _ => null, ct),
            _ => Task.FromResult(new TrackingImportSummary(0, rows.Count,
                new[] { "Bu kaynak için tanımlı bir takip tablosu yok." })),
        };

    private async Task<TrackingImportSummary> ReplaceAsync<T>(
        DbSet<T> set, long sourceId, bool archived,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> rows,
        Func<Row, T?> map, Func<T, DateOnly?> primaryEta, CancellationToken ct)
        where T : TrackingRecordBase
    {
        var thresholds = await _thresholds.GetAsync(ct);
        var existing = await set.Where(x => x.DataSourceId == sourceId).ToListAsync(ct);
        if (existing.Count > 0) set.RemoveRange(existing);

        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int imported = 0, skipped = 0;
        var errors = new List<string>();

        for (int i = 0; i < rows.Count; i++)
        {
            var raw = rows[i];
            var row = new Row(raw);
            if (row.IsEmpty) { skipped++; continue; }
            try
            {
                var e = map(row);
                if (e is null) { skipped++; continue; }
                e.DataSourceId = sourceId;
                e.RowIndex = i;
                e.IsArchived = archived;
                e.ImportedAt = now;
                e.RawJson = JsonSerializer.Serialize(raw);
                if (string.IsNullOrWhiteSpace(e.SourceRowKey)) e.SourceRowKey = $"#{i + 1}";

                var (level, delay) = EvaluateRisk(e.StatusText, primaryEta(e), today, archived, thresholds.DelayOrangeDays, thresholds.DelayRedDays);
                e.RiskLevel = level;
                e.DelayDays = delay;

                set.Add(e);
                imported++;
            }
            catch (Exception ex)
            {
                skipped++;
                if (errors.Count < 20) errors.Add($"Satır {i + 1}: {ex.Message}");
            }
        }

        return new TrackingImportSummary(imported, skipped, errors);
    }

    // --- Sayfa eşleyicileri (başlık → tip-güvenli alan) ---

    private static SeaTransitRecord MapSeaTransit(Row r) => new()
    {
        SourceRowKey = r.S("DOSYA NO") ?? "",
        Shipper = r.S("SHIPPER"),
        Consignee = r.S("(CONSIGNEE)"),
        Line = r.S("LINE"),
        Term = r.S("TERM"),
        Agent = r.S("ACENTE"),
        AgentRef = r.S("ACENTE REF."),
        ContainerKind = r.S("KONT.TIP"),
        ImportBooking = r.S("İHALAT BOOKING"),
        ContainerNo = r.S("CONTAINER NO"),
        IncomingContainer = r.S("GELEN KONTEYNER"),
        Pol = r.S("POL"),
        Pod = r.S("POD"),
        ImportEtd = r.D("ETD"),
        ImportEta = r.D("ETA"),
        TransferPoint = r.S("TO"),
        ExportBooking = r.S("İHRACAT BOOKING"),
        EmptyContainerTransfer = r.S("BOŞ AKTARILACAK KONT."),
        CutOff = r.D("CUT OFF"),
        ExportEtd = r.D("ETD_2"),
        ExportEta = r.D("ETA_2"),
        Invoice = r.S("FATURA"),
        Note = r.S("NOT"),
        Note2 = r.S("NOT 2"),
        StatusText = r.S("NOT"),
    };

    private static SeaImportRecord MapSeaImport(Row r) => new()
    {
        SourceRowKey = r.S("DOSYA NO") ?? "",
        Shipper = r.S("SHIPPER"),
        Consignee = r.S("(CONSIGNEE)"),
        Line = r.S("LINE"),
        Term = r.S("TERM"),
        Agent = r.S("ACENTE"),
        AgentRef = r.S("ACENTE REF."),
        ContainerKind = r.S("KONT.TIP"),
        Booking = r.S("BOOKING"),
        ContainerNo = r.S("CONTAINER NO"),
        Pol = r.S("POL"),
        Pod = r.S("POD"),
        Etd = r.D("ETD"),
        Eta = r.D("ETA"),
        Invoice = r.S("FATURA"),
        Note = r.S("NOT"),
        Note2 = r.S("NOT 2"),
        StatusText = r.S("NOT"),
    };

    private static SeaExportRecord MapSeaExport(Row r) => new()
    {
        SourceRowKey = r.S("DOSYA NO") ?? "",
        Shipper = r.S("SHIPPER"),
        Consignee = r.S("(CONSIGNEE)"),
        Line = r.S("LINE"),
        Term = r.S("TERM"),
        ContainerKind = r.S("KONT.TIP"),
        Booking = r.S("BOOKING"),
        ContainerNo = r.S("CONTAINER NO"),
        Pol = r.S("POL"),
        Pod = r.S("POD"),
        CutOff = r.D("CUT OFF"),
        Etd = r.D("ETD"),
        Eta = r.D("ETA"),
        Invoice = r.S("FATURA"),
        Note = r.S("NOT"),
        Note2 = r.S("NOT 2"),
        StatusText = r.S("NOT"),
    };

    private static RoadTransitRecord MapRoadTransit(Row r) => new()
    {
        SourceRowKey = r.S("DOSYA NO") ?? "",
        Date = r.D("TARİH"),
        Shipper = r.S("SHIPPER"),
        Consignee = r.S("(CONSIGNEE)"),
        OriginCountry = r.S("ÇIKIŞ ÜLKESİ"),
        Plate = r.S("PLAKA"),
        Term = r.S("TERM"),
        Line = r.S("LINE"),
        Booking = r.S("BOOKING"),
        ContainerNo = r.S("CONTAINER NO"),
        EmptyContainerTransfer = r.S("BOŞ AKTARILACAK KONT."),
        Pol = r.S("POL"),
        Pod = r.S("POD"),
        Eta = r.D("ETA"),
        CutOff = r.D("CUT OFF"),
        Invoice = r.S("FATURA"),
        Note = r.S("NOT"),
        Note2 = r.S("NOT 2"),
        StatusText = r.S("NOT"),
    };

    private static RoadLoadRecord MapRoadLoad(Row r) => new()
    {
        SourceRowKey = r.S("REF NO") ?? "",
        CustomerRep = r.S("MÜŞTERİ TEMSİLCİSİ"),
        DepartureDate = r.S("ÇIKIŞ TARİHİ"),
        VehicleLocation = r.S("ARAÇ KONUMU"),
        ImportCountry = r.S("İTH ÜLKESİ"),
        Sender = r.S("GÖNDERİCİ"),
        Receiver = r.S("ALICI"),
        Plate = r.S("PLAKA"),
        ProductType = r.S("ÜRÜN CİNSİ"),
        PackageCount = r.S("KAP ADEDİ"),
        Weight = r.S("KG"),
        Stackable = r.S("İSTİFLENEBİLİR ?"),
        ArrivalWarehouse = r.S("VARIŞ ANTR"),
        Freight = r.S("NAVLUN"),
        Ydg = r.S("YDG"),
        Supplier = r.S("TEDARİKÇİ"),
        StatusText = r.S("ARAÇ KONUMU"),
    };

    private static RoadArchiveRecord MapRoadArchive(Row r) => new()
    {
        SourceRowKey = r.S("TAKİP REF NO") ?? "",
        DepartureDate = r.S("ÇIKIŞ TARİHİ"),
        ImportCountry = r.S("İTH ÜLKESİ"),
        Sender = r.S("GÖNDERİCİ"),
        Receiver = r.S("ALICI"),
        Plate = r.S("PLAKA"),
        ProductType = r.S("ÜRÜN CİNSİ"),
        PackageCount = r.S("KAP ADEDİ"),
        Weight = r.S("KG"),
        Stackable = r.S("İSTİFLENEBİLİR ?"),
        OrderDate = r.S("ORDİNO TARİHİ"),
        ArrivalWarehouse = r.S("VARIŞ ANTR"),
        PurchaseFreight = r.S("ALIŞ NAVLUNU"),
        YdgIncluded = r.S("YDG DAHİL"),
        Supplier = r.S("TEDARİKÇİ"),
    };

    private static AlaboraFinanceRecord? MapAlabora(Row r)
    {
        var yukNo = r.S("YÜK NO");
        var no = r.S("№");
        var ftDate = r.D("FT. TARİH");
        var company = r.S("FİRMA ÜNVANI");
        // Kaynakta başlık 2 satıra yayılmış (örn. "SEFER" + " NUMARASI"); ilk veri satırı gibi görünen
        // ikinci başlık satırı (YÜK NO/№/FT.TARİH/FİRMA hepsi boş) gerçek veri değildir — atlanır.
        if (yukNo is null && no is null && ftDate is null && company is null) return null;

        return new AlaboraFinanceRecord
        {
            SourceRowKey = yukNo ?? "",
            No = no,
            FtDate = ftDate,
            CompanyTitle = company,
            Voyage = r.S("SEFER"),
            Amount = r.S("TUTAR"),
            Currency = r.S("DÖVİZ CİNSİ"),
            CargoStatus = r.S("статус груза / выгрузка"),
            DocsReadiness = r.S("Готовность пакета документов"),
            InvoiceMarked = r.S("счет с отметкой"),
            TransportDocs = r.S("Транспортные документы"),
            OrderContract = r.S("поручение / заявка / договор"),
            CommentOsh = r.S("Комментарии ОШ"),
            CommentOls = r.S("Комментарии ОЛС"),
            IncomingPayments = r.S("GELEN ÖDEMELER"),
            Collection = r.S("TAHSİLAT"),
            PaymentDate = r.D("payment date"),
            Rub = r.S("rub"),
            LoadingType = r.S("YÜKLEME CİNSİ"),
            LoadingDetails = r.S("YÜKLEME DETAY BİLGİLERİ"),
            CustomerRep = r.S("MÜŞTERİ"),
            RateNet = r.S("rate net"),
            StatusText = r.S("статус груза / выгрузка"),
        };
    }

    private static AirOperationRecord MapAirOperation(Row r) => new()
    {
        SourceRowKey = r.S("Referance Number") ?? "",
        Sender = r.S("Gönderici"),
        Flight = r.S("Uçuş"),
        WarehouseEntry = r.D("Ambar Girişi"),
        OptionDate = r.D("Opsiyon Tarihi"),
        OptionTime = r.S("Opsiyon Saati"),
        Airline = r.S("Airline"),
        Warehouse = r.S("Ambar"),
        ReferenceNumber = r.S("Referance Number"),
        Sn = r.S("SN"),
        Archive = r.S("ARŞİV"),
        ColA = r.S("A"),
        ColS = r.S("S"),
        Notes = r.S("NOTLAR"),
        StatusText = r.S("NOTLAR"),
    };

    private static AirDailyRecord? MapAirDaily(Row r)
    {
        var refNo = r.S("Referance Number");
        var mawb = r.S("MAWB No");
        // Kaynak kart yapısında; gerçek sevkiyat satırı değilse (ref ve MAWB yoksa) atla.
        if (refNo is null && mawb is null) return null;
        return new AirDailyRecord
        {
            SourceRowKey = refNo ?? r.S("Shipment Number") ?? "",
            UserId = r.S("User ID"),
            MawbNo = mawb,
            HawbNo = r.S("HAWB No"),
            Airport = r.S("Havalimanı"),
            Destination = r.S("Varış"),
            PieceCount = r.S("Parça Sayısı"),
            Kgs = r.S("KGS"),
            Status = r.S("Statü"),
            Incoterm = r.S("Incoterm"),
            Sender = r.S("Gönderici"),
            Flight = r.S("Uçuş"),
            WarehouseEntry = r.D("Ambar Girişi"),
            OptionDate = r.D("Opsiyon Tarihi"),
            OptionTime = r.S("Opsiyon Saati"),
            Airline = r.S("Airline"),
            Warehouse = r.S("Ambar"),
            ReferenceNumber = refNo,
            Carrier = r.S("Havayolu"),
            Flag = r.S("Bayrak"),
            WarehouseCode = r.S("Ambar Code"),
            Authorized = r.S("Yetkili"),
            Address = r.S("ADRES"),
            ShipmentNumber = r.S("Shipment Number"),
            StatusText = r.S("Statü"),
        };
    }

    // --- Risk türetme ---

    private static readonly string[] RiskKeywords =
        { "BEKLEN", "GÜMRÜK", "ASKI", "REVIZE", "REVİZE", "SORUN", "PROBLEM", "GECİK", "İPTAL", "EKSİK", "ÖDEME", "HAZIRLAYAMADI" };
    private static readonly string[] DeliveredKeywords =
        { "TESLİM", "BOŞALDI", "TAHLİYE", "VARDI", "GELDİ", "TAMAMLAND" };

    private static (RiskLevel level, int delay) EvaluateRisk(
        string? statusText, DateOnly? eta, DateOnly today, bool archived, int orangeDays, int redDays)
    {
        if (archived) return (RiskLevel.Green, 0);

        var upper = statusText?.ToUpperInvariant();
        int delay = 0;
        var level = RiskLevel.Green;

        bool delivered = upper is not null && DeliveredKeywords.Any(k => upper.Contains(k, StringComparison.Ordinal));
        if (eta is { } e && e < today && !delivered)
        {
            delay = today.DayNumber - e.DayNumber;
            level = delay > redDays ? RiskLevel.Red : delay > orangeDays ? RiskLevel.Orange : RiskLevel.Yellow;
        }

        if (!delivered && upper is not null && RiskKeywords.Any(k => upper.Contains(k, StringComparison.Ordinal))
            && level < RiskLevel.Yellow)
            level = RiskLevel.Yellow;

        return (level, delay);
    }

    /// <summary>Tek satırın normalize edilmiş (başlık → değer) görünümü; toleranslı arama sağlar.</summary>
    private sealed class Row
    {
        private readonly Dictionary<string, string?> _cells;

        public Row(IReadOnlyDictionary<string, string?> raw)
        {
            _cells = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var kv in raw)
            {
                var v = Clean(kv.Value);
                if (v is not null) _cells[Norm(kv.Key)] = v;
            }
        }

        public bool IsEmpty => _cells.Count == 0;

        public string? S(string key) => _cells.TryGetValue(Norm(key), out var v) ? v : null;

        public DateOnly? D(string key) => ParseDateCell(S(key));

        private static string Norm(string s)
        {
            var sb = new StringBuilder(s.Length);
            bool prevSpace = false;
            foreach (var ch0 in s)
            {
                var ch = ch0 == ' ' ? ' ' : ch0;
                if (char.IsWhiteSpace(ch))
                {
                    if (!prevSpace) { sb.Append(' '); prevSpace = true; }
                }
                else { sb.Append(ch); prevSpace = false; }
            }
            return sb.ToString().Trim().ToUpperInvariant();
        }

        private static string? Clean(string? v)
        {
            if (v is null) return null;
            var s = v.Replace(' ', ' ').Trim();
            return s.Length == 0 ? null : s;
        }

        private static DateOnly? ParseDateCell(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var d = DataNormalizer.ParseDate(s);
            if (d is not null) return d;
            // Excel seri tarih (örn. 46192) — ClosedXML sayı olarak döndürdüyse
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var serial)
                && serial is > 20000 and < 80000)
                return DateOnly.FromDateTime(new DateTime(1899, 12, 30).AddDays(serial));
            return null;
        }
    }
}

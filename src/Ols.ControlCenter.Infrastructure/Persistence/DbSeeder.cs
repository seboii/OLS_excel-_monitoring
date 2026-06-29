using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;
using Ols.ControlCenter.Shared.Authorization;

namespace Ols.ControlCenter.Infrastructure.Persistence;

/// <summary>
/// Idempotent başlangıç verisi: departmanlar, roller, admin kullanıcı, risk kuralları,
/// statü eşleştirmeleri ve demo operasyonlar. Her blok yalnızca tablo boşsa çalışır.
/// </summary>
public static class DbSeeder
{
    /// <summary>Yalnızca <c>Seed:AdminEmail</c>/<c>Seed:AdminPassword</c> hiç ayarlanmamışsa kullanılan varsayılan.</summary>
    public const string DefaultAdminEmail = "admin@ols.local";
    public const string DefaultAdminPassword = "Admin123!";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, IConfiguration config, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var adminEmail = config["Seed:AdminEmail"];
        var adminPassword = config["Seed:AdminPassword"];

        var departments = await SeedDepartmentsAsync(db, now, ct);
        var roles = await SeedRolesAsync(db, now, ct);
        await SeedAdminAsync(db, hasher, roles, now,
            string.IsNullOrWhiteSpace(adminEmail) ? DefaultAdminEmail : adminEmail,
            string.IsNullOrWhiteSpace(adminPassword) ? DefaultAdminPassword : adminPassword,
            ct);
        await SeedRiskRulesAsync(db, now, ct);
        await SeedStatusMappingsAsync(db, ct);
        await SeedDataSourcesAsync(db, departments, now, ct);
        await SeedDemoOperationsAsync(db, departments, now, today, ct);
    }

    /// <summary>Verilen public linkleri veri kaynağı olarak hazırlar (ada göre idempotent).</summary>
    private static async Task SeedDataSourcesAsync(AppDbContext db, Dictionary<string, Department> dept, DateTimeOffset now, CancellationToken ct)
    {
        async Task EnsureAsync(
            string name, DataSourceType type, DataSourceAccessType access, string? url,
            string? sheetName, TrackingBoardType board, TransportType? transport, string? deptCode,
            int intervalMinutes = 15, int headerRow = 1)
        {
            if (await db.DataSources.AnyAsync(d => d.Name == name, ct)) return;
            db.DataSources.Add(new DataSource
            {
                Name = name,
                Type = type,
                AccessType = access,
                Url = url,
                SheetName = sheetName,
                HeaderRowIndex = headerRow,
                TargetBoard = board,
                DefaultTransportType = transport,
                DepartmentId = deptCode != null && dept.TryGetValue(deptCode, out var d) ? d.Id : null,
                ConnectionConfigEncrypted = string.Empty,
                IsActive = true,
                SyncIntervalMinutes = intervalMinutes,
                CreatedAt = now,
            });
        }

        const string denizUrl = "https://disk.yandex.com.tr/i/dZnHlx9gfUfRFw";
        const string karayoluUrl = "https://disk.yandex.ru/i/H8GeygmiLVjlcw";

        // DENİZ OPERASYON TAKİP RAPORU.xlsx — 4 sayfa = 4 takip tablosu
        await EnsureAsync("Deniz · Denizyolu Transit", DataSourceType.YandexDiskExcel, DataSourceAccessType.Public,
            denizUrl, "DENİZYOLU TRANSİT", TrackingBoardType.SeaTransit, TransportType.Sea, "SEA");
        await EnsureAsync("Deniz · İthalat", DataSourceType.YandexDiskExcel, DataSourceAccessType.Public,
            denizUrl, "İTHALAT", TrackingBoardType.SeaImport, TransportType.Sea, "SEA");
        await EnsureAsync("Deniz · İhracat", DataSourceType.YandexDiskExcel, DataSourceAccessType.Public,
            denizUrl, "İHRACAT", TrackingBoardType.SeaExport, TransportType.Sea, "SEA");
        await EnsureAsync("Deniz · Karayolu Transit", DataSourceType.YandexDiskExcel, DataSourceAccessType.Public,
            denizUrl, "KARAYOLU TRANSİT", TrackingBoardType.RoadTransit, TransportType.Road, "SEA");

        // YOLDAKİ AVRUPA İTHALAT KARAYOLU YÜKLEMELERİ.xlsx — 2 sayfa = 2 takip tablosu
        await EnsureAsync("Karayolu · Yoldaki Yükler", DataSourceType.YandexDiskExcel, DataSourceAccessType.Public,
            karayoluUrl, "YOLDAKİ YÜKLER", TrackingBoardType.RoadLoad, TransportType.Road, "ROAD");
        await EnsureAsync("Karayolu · Arşiv (Muratbey/Kerry/Mirlog)", DataSourceType.YandexDiskExcel, DataSourceAccessType.Public,
            karayoluUrl, "MURATBEY KERRY & MİRLOG VARIŞ", TrackingBoardType.RoadArchive, TransportType.Road, "ROAD");

        // ALABORA.xlsx — СЧЕТА-ПЛАТЕЖИ (fatura/tahsilat), başlık 4. satırda
        const string alaboraUrl = "https://disk.yandex.com.tr/i/_moBFNX9Q_8kZA";
        await EnsureAsync("Alabora · Tahsilat", DataSourceType.YandexDiskExcel, DataSourceAccessType.Public,
            alaboraUrl, "СЧЕТА-ПЛАТЕЖИ", TrackingBoardType.Alabora, TransportType.Sea, "SEA", headerRow: 4);

        // HAVA.xlsx — iki sayfa = iki takip tablosu
        const string havaUrl = "https://disk.yandex.com.tr/i/h93uJsdMO5Bhgw";
        await EnsureAsync("Hava · Operasyon Bilgileri", DataSourceType.YandexDiskExcel, DataSourceAccessType.Public,
            havaUrl, "OPERASYON BİLGİLERİ", TrackingBoardType.Air, TransportType.Air, "AIR");
        await EnsureAsync("Hava · Günlük Liste", DataSourceType.YandexDiskExcel, DataSourceAccessType.Public,
            havaUrl, "GÜNLÜK LİSTE", TrackingBoardType.AirDaily, TransportType.Air, "AIR", headerRow: 1);

        await db.SaveChangesAsync(ct);
    }

    private static async Task<Dictionary<string, Department>> SeedDepartmentsAsync(
        AppDbContext db, DateTimeOffset now, CancellationToken ct)
    {
        if (await db.Departments.AnyAsync(ct))
            return await db.Departments.ToDictionaryAsync(d => d.Code, ct);

        var list = new List<Department>
        {
            new() { Name = "Karayolu Operasyonları", Code = "ROAD", DefaultTransportType = TransportType.Road, CreatedAt = now },
            new() { Name = "Deniz Operasyonları", Code = "SEA", DefaultTransportType = TransportType.Sea, CreatedAt = now },
            new() { Name = "Hava Operasyonları", Code = "AIR", DefaultTransportType = TransportType.Air, CreatedAt = now },
            new() { Name = "Gümrük Operasyonları", Code = "CUSTOMS", DefaultTransportType = TransportType.Customs, CreatedAt = now },
            new() { Name = "Finans ve Tahsilatlar", Code = "FINANCE", CreatedAt = now },
            new() { Name = "Müşteri Departmanı", Code = "CUSTOMER", CreatedAt = now },
            new() { Name = "Pricing / Teklif", Code = "PRICING", CreatedAt = now },
            new() { Name = "Parsiyel / Konsolide", Code = "PARCEL", CreatedAt = now },
        };
        db.Departments.AddRange(list);
        await db.SaveChangesAsync(ct);
        return list.ToDictionary(d => d.Code);
    }

    private static async Task<Dictionary<string, Role>> SeedRolesAsync(
        AppDbContext db, DateTimeOffset now, CancellationToken ct)
    {
        if (await db.Roles.AnyAsync(ct))
            return await db.Roles.ToDictionaryAsync(r => r.Code, ct);

        var list = new List<Role>
        {
            new() { Code = AppRoles.Admin, Name = "Yönetici", Description = "Tüm operasyonları görür ve yönetir.", CreatedAt = now },
            new() { Code = AppRoles.DepartmentManager, Name = "Departman Müdürü", Description = "Kendi departmanını yönetir.", CreatedAt = now },
            new() { Code = AppRoles.OperationSpecialist, Name = "Operasyon Uzmanı", Description = "Kendi işlerini günceller.", CreatedAt = now },
            new() { Code = AppRoles.Finance, Name = "Finans Kullanıcısı", Description = "Tahsilat ve finansal riski yönetir.", CreatedAt = now },
            new() { Code = AppRoles.ReadOnly, Name = "Salt Okuma", Description = "Yalnızca görüntüler.", CreatedAt = now },
        };
        db.Roles.AddRange(list);
        await db.SaveChangesAsync(ct);
        return list.ToDictionary(r => r.Code);
    }

    private static async Task SeedAdminAsync(
        AppDbContext db, IPasswordHasher hasher, Dictionary<string, Role> roles, DateTimeOffset now,
        string adminEmail, string adminPassword, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(ct))
            return;

        var admin = new User
        {
            FullName = "Sistem Yöneticisi",
            Email = adminEmail,
            PasswordHash = hasher.Hash(adminPassword),
            IsActive = true,
            CreatedAt = now,
            UserRoles = new List<UserRole> { new() { RoleId = roles[AppRoles.Admin].Id } }
        };
        db.Users.Add(admin);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedRiskRulesAsync(AppDbContext db, DateTimeOffset now, CancellationToken ct)
    {
        if (await db.RiskRules.AnyAsync(ct))
            return;

        var rules = new List<RiskRule>
        {
            new() { Code = "DELAY", Name = "Teslim tarihi geçti", Severity = RiskLevel.Red, AlertType = AlertType.Delay,
                Description = "Planlanan teslim tarihi geçmiş ve teslim edilmemiş.", CreatedAt = now },
            new() { Code = "PAYMENT_RISK", Name = "Teslimat yakın, tahsilat yok", Severity = RiskLevel.Red, AlertType = AlertType.PaymentRisk,
                Description = "Teslimata az kaldı ama tahsilat bekleniyor.", Parameters = new() { ["daysBeforeDelivery"] = "2" }, CreatedAt = now },
            new() { Code = "CUST_INFO_24H", Name = "Müşteri bilgilendirme eksik", Severity = RiskLevel.Orange, AlertType = AlertType.CustomerInfoGap,
                Description = "Aktif operasyonda son müşteri güncellemesi 24 saati geçti.", Parameters = new() { ["hours"] = "24" }, CreatedAt = now },
            new() { Code = "DOC_MISSING", Name = "Varış öncesi evrak eksik", Severity = RiskLevel.Red, AlertType = AlertType.MissingDocuments,
                Description = "ETA/teslime az kaldı ve evrak durumu eksik.", Parameters = new() { ["daysBeforeEta"] = "2" }, CreatedAt = now },
            new() { Code = "SEA_DEMURRAGE", Name = "Demuraj riski", Severity = RiskLevel.Orange, AlertType = AlertType.FreeTimeDemurrageRisk,
                Description = "Free time bitişine az kaldı.", Parameters = new() { ["warnDays"] = "3" }, AppliesToTransportType = TransportType.Sea, CreatedAt = now },
            new() { Code = "NEXT_ACTION_MISSING", Name = "Sonraki aksiyon tanımsız", Severity = RiskLevel.Yellow, AlertType = AlertType.NextActionMissing,
                Description = "Aktif operasyonda sonraki aksiyon tanımlanmamış.", CreatedAt = now },
            new() { Code = "CRITICAL_CUSTOMER", Name = "Kritik müşteride gecikme", Severity = RiskLevel.Red, AlertType = AlertType.CriticalCustomer,
                Description = "Kritik müşteri listesindeki müşteride gecikme var.", CreatedAt = now },
        };
        db.RiskRules.AddRange(rules);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedStatusMappingsAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.StatusMappings.AnyAsync(ct))
            return;

        void Map(string source, OperationStatus target) =>
            db.StatusMappings.Add(new StatusMapping { SourceStatus = source, TargetStatus = target });

        Map("New", OperationStatus.New); Map("Yeni", OperationStatus.New);
        Map("Preparing", OperationStatus.Preparing); Map("Hazırlıkta", OperationStatus.Preparing);
        Map("On Road", OperationStatus.InTransit); Map("Transit", OperationStatus.InTransit);
        Map("Sevkte", OperationStatus.InTransit); Map("Yolda", OperationStatus.InTransit);
        Map("At Port", OperationStatus.AtPort); Map("Limanda", OperationStatus.AtPort);
        Map("In Customs", OperationStatus.InCustoms); Map("Gümrükte", OperationStatus.InCustoms);
        Map("Delivered", OperationStatus.Completed); Map("Teslim", OperationStatus.Completed);
        Map("Tamam", OperationStatus.Completed); Map("Completed", OperationStatus.Completed);
        Map("Waiting Docs", OperationStatus.MissingDocuments); Map("Evrak Bekliyor", OperationStatus.MissingDocuments);
        Map("Payment Hold", OperationStatus.FinancialHold); Map("Finans Bekliyor", OperationStatus.FinancialHold);
        Map("Cancelled", OperationStatus.Cancelled); Map("İptal", OperationStatus.Cancelled);

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedDemoOperationsAsync(
        AppDbContext db, Dictionary<string, Department> dept, DateTimeOffset now, DateOnly today, CancellationToken ct)
    {
        if (await db.Operations.AnyAsync(ct))
            return;

        var bosch = new Customer { Name = "Bosch San. ve Tic. A.Ş.", IsCritical = true, Currency = "EUR", CreatedAt = now };
        var arcelik = new Customer { Name = "Arçelik A.Ş.", Currency = "EUR", CreatedAt = now };
        var ford = new Customer { Name = "Ford Otosan", Currency = "USD", CreatedAt = now };
        db.Customers.AddRange(bosch, arcelik, ford);

        var manualSource = new DataSource { Name = "Manuel Excel — Karayolu", Type = DataSourceType.ManualExcel, DefaultTransportType = TransportType.Road, ConnectionConfigEncrypted = "", IsActive = true, CreatedAt = now };
        db.DataSources.Add(manualSource);

        var ops = new List<Operation>
        {
            new()
            {
                SourceOperationNo = "TR-123456", TransportType = TransportType.Road, ServiceType = ServiceType.Ftl,
                TradeDirection = TradeDirection.Import, Department = dept["ROAD"], Customer = bosch, CustomerName = bosch.Name,
                OriginCountry = "Almanya", OriginCity = "Stuttgart", DestinationCountry = "Türkiye", DestinationCity = "Bursa",
                Status = OperationStatus.InTransit, RiskLevel = RiskLevel.Yellow, FinanceStatus = FinanceStatus.Pending,
                DocumentStatus = DocumentStatus.Complete, LoadingDate = today.AddDays(-3), PlannedDeliveryDate = today.AddDays(2),
                Eta = now.AddDays(2), RevenueAmount = 4200m, CostAmount = 3100m, Currency = "EUR", CreatedAt = now,
                Detail = new OperationDetail { VehiclePlate = "34 ABC 123", DriverName = "M. Yılmaz", BorderCrossing = "Kapıkule", FillRate = 0.95m }
            },
            new()
            {
                SourceOperationNo = "TR-123457", TransportType = TransportType.Road, ServiceType = ServiceType.Ltl,
                TradeDirection = TradeDirection.Export, Department = dept["ROAD"], Customer = arcelik, CustomerName = arcelik.Name,
                OriginCountry = "Türkiye", OriginCity = "İstanbul", DestinationCountry = "Fransa", DestinationCity = "Lyon",
                Status = OperationStatus.Delayed, RiskLevel = RiskLevel.Red, FinanceStatus = FinanceStatus.Overdue,
                DocumentStatus = DocumentStatus.Missing, LoadingDate = today.AddDays(-8), PlannedDeliveryDate = today.AddDays(-2),
                DelayReason = DelayReason.BorderCongestion, RevenueAmount = 1800m, CostAmount = 1500m, Currency = "EUR", CreatedAt = now,
                Detail = new OperationDetail { VehiclePlate = "34 XYZ 789", DriverName = "A. Demir", BorderCrossing = "Kapıkule", FillRate = 0.40m }
            },
            new()
            {
                SourceOperationNo = "OLSU24567", TransportType = TransportType.Sea, ServiceType = ServiceType.Fcl,
                TradeDirection = TradeDirection.Import, Department = dept["SEA"], Customer = ford, CustomerName = ford.Name,
                OriginCountry = "Çin", OriginCity = "Shanghai", DestinationCountry = "Türkiye", DestinationCity = "İzmit",
                Status = OperationStatus.AtPort, RiskLevel = RiskLevel.Orange, FinanceStatus = FinanceStatus.Pending,
                DocumentStatus = DocumentStatus.Pending, Etd = now.AddDays(-25), Eta = now.AddDays(-1),
                RevenueAmount = 9500m, CostAmount = 7200m, Currency = "USD", CreatedAt = now,
                Detail = new OperationDetail
                {
                    BlNo = "MEDU1234567", ContainerNo = "MSCU7654321", ContainerType = "40HC", ShippingLine = "MSC",
                    VesselName = "MSC Gülsün", Pol = "Shanghai", Pod = "Kocaeli", OrdinoStatus = "Bekliyor",
                    FreeTimeEndDate = today.AddDays(2), DemurrageStartDate = today.AddDays(3)
                }
            },
            new()
            {
                SourceOperationNo = "OLSU24570", TransportType = TransportType.Sea, ServiceType = ServiceType.Lcl,
                TradeDirection = TradeDirection.Import, Department = dept["SEA"], Customer = arcelik, CustomerName = arcelik.Name,
                OriginCountry = "İtalya", OriginCity = "Genova", DestinationCountry = "Türkiye", DestinationCity = "İstanbul",
                Status = OperationStatus.InTransit, RiskLevel = RiskLevel.Green, FinanceStatus = FinanceStatus.Collected,
                DocumentStatus = DocumentStatus.Complete, Etd = now.AddDays(-6), Eta = now.AddDays(4),
                RevenueAmount = 2100m, CostAmount = 1400m, Currency = "EUR", CreatedAt = now,
                Detail = new OperationDetail { BlNo = "GENU9988776", ShippingLine = "Arkas", Pol = "Genova", Pod = "Ambarlı" }
            },
            new()
            {
                SourceOperationNo = "HAWB-5567", TransportType = TransportType.Air, ServiceType = ServiceType.AirCargo,
                TradeDirection = TradeDirection.Import, Department = dept["AIR"], Customer = bosch, CustomerName = bosch.Name,
                OriginCountry = "Almanya", OriginCity = "Frankfurt", DestinationCountry = "Türkiye", DestinationCity = "İstanbul",
                Status = OperationStatus.InCustoms, RiskLevel = RiskLevel.Yellow, FinanceStatus = FinanceStatus.Pending,
                DocumentStatus = DocumentStatus.Complete, Etd = now.AddDays(-1), Eta = now.AddHours(-6),
                RevenueAmount = 3300m, CostAmount = 2500m, Currency = "EUR", CreatedAt = now,
                Detail = new OperationDetail { HawbNo = "HAWB-5567", MawbNo = "020-12345675", Airline = "Lufthansa Cargo", FlightNo = "LH8200", DepartureAirport = "FRA", ArrivalAirport = "IST", Pieces = 12, GrossWeightKg = 540m }
            },
            new()
            {
                SourceOperationNo = "GUM-2026-114", TransportType = TransportType.Customs, ServiceType = ServiceType.Customs,
                TradeDirection = TradeDirection.Import, Department = dept["CUSTOMS"], Customer = ford, CustomerName = ford.Name,
                OriginCountry = "Türkiye", OriginCity = "İstanbul", DestinationCountry = "Türkiye", DestinationCity = "İstanbul",
                Status = OperationStatus.MissingDocuments, RiskLevel = RiskLevel.Red, FinanceStatus = FinanceStatus.Pending,
                DocumentStatus = DocumentStatus.Missing, CreatedAt = now,
                Detail = new OperationDetail()
            },
        };

        foreach (var op in ops)
            op.RecomputeDerived(today);

        db.Operations.AddRange(ops);
        await db.SaveChangesAsync(ct);
    }
}

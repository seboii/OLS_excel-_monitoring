using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ols.ControlCenter.Domain.Enums;
using Ols.ControlCenter.Infrastructure.Persistence;
using Ols.ControlCenter.Infrastructure.Security;

namespace Ols.ControlCenter.IntegrationTests;

/// <summary>
/// Migration zinciri + <see cref="DbSeeder"/>'ı gerçek bir PostgreSQL'e karşı çalıştırır.
/// Bağlantı <c>OLS_DB_CONNECTION</c> ortam değişkeninden gelir (CI'da test amaçlı bir Postgres
/// container'ı); yoksa yerel `docker compose up -d` ile ayağa kalkan varsayılan altyapıya düşer.
/// Idempotent seed mantığı burada doğrulanır — bu proje boyunca en çok kırılan yer burasıydı
/// (örn. admin parolası `.env`'den değil kod sabitinden okunuyordu, sessizce).
/// </summary>
public sealed class DbSeedTests : IAsyncLifetime
{
    private AppDbContext _db = null!;
    private IConfiguration _config = null!;

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("OLS_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=ols_control_center;Username=ols;Password=ols_dev_excel3232";

        _config = new ConfigurationBuilder().AddInMemoryCollection().Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention()
            .Options;

        _db = new AppDbContext(options);
        await _db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    [Fact]
    public async Task SeedAsync_CreatesAdminUserAndCoreReferenceData()
    {
        await DbSeeder.SeedAsync(_db, new BcryptPasswordHasher(), _config);

        Assert.True(await _db.Users.AnyAsync(), "Seed sonrası en az bir kullanıcı (admin) olmalı.");
        Assert.True(await _db.Departments.AnyAsync(), "Departmanlar seed edilmeli.");
        Assert.True(await _db.Roles.AnyAsync(), "Roller seed edilmeli.");
        Assert.True(await _db.RiskRules.AnyAsync(), "Risk kuralları seed edilmeli.");

        var boardSourceCount = await _db.DataSources.CountAsync(d => d.TargetBoard != TrackingBoardType.None);
        Assert.Equal(9, boardSourceCount);
    }

    [Fact]
    public async Task SeedAsync_CalledTwice_DoesNotDuplicateData()
    {
        await DbSeeder.SeedAsync(_db, new BcryptPasswordHasher(), _config);
        var userCountAfterFirst = await _db.Users.CountAsync();
        var dataSourceCountAfterFirst = await _db.DataSources.CountAsync();

        await DbSeeder.SeedAsync(_db, new BcryptPasswordHasher(), _config);
        var userCountAfterSecond = await _db.Users.CountAsync();
        var dataSourceCountAfterSecond = await _db.DataSources.CountAsync();

        Assert.Equal(userCountAfterFirst, userCountAfterSecond);
        Assert.Equal(dataSourceCountAfterFirst, dataSourceCountAfterSecond);
    }
}

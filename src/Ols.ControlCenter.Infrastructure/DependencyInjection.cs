using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Abstractions.Reports;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Infrastructure.DataIntegration;
using Ols.ControlCenter.Infrastructure.Persistence;
using Ols.ControlCenter.Infrastructure.Reports;
using Ols.ControlCenter.Infrastructure.Security;
using StackExchange.Redis;

namespace Ols.ControlCenter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default tanımlı değil.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                       npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                   .UseSnakeCaseNamingConvention()
                   .ConfigureWarnings(w =>
                       w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ISecretProtector, AesSecretProtector>();

        // JWT
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtOptions>>().Value);
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Veri entegrasyon motoru
        services.AddScoped<ISourceFileParser, ExcelCsvParser>();
        services.AddScoped<IOperationUpsertService, OperationUpsertService>();
        services.AddScoped<ITrackingImportService, TrackingImportService>();
        services.AddScoped<IDataSyncLogService, DataSyncLogService>();
        services.AddScoped<IDataSyncService, DataSyncService>();

        // İndiriciler (public Yandex / SharePoint) + Excel okuyucu + import
        services.AddHttpClient<IYandexPublicExcelDownloader, YandexPublicExcelDownloader>(ConfigureDownloadClient);
        services.AddHttpClient<ISharePointPublicExcelDownloader, SharePointPublicExcelDownloader>(ConfigureDownloadClient);
        services.AddScoped<IDataSourceDownloader, DataSourceDownloader>();
        services.AddScoped<IExcelReaderService, ExcelReaderService>();
        services.AddScoped<IDataImportService, DataImportService>();

        // Raporlama
        services.AddScoped<IReportService, ExcelReportService>();

        return services;
    }

    /// <summary>
    /// Redis bağlantısını (IConnectionMultiplexer) singleton kaydeder. Hata toleranslıdır:
    /// Redis başlangıçta erişilemese bile uygulama çöker yerine bağlanmayı sürdürür.
    /// Canlı bildirim köprüsü (Worker → API) bu bağlantıyı kullanır.
    /// </summary>
    public static IServiceCollection AddRedisConnection(this IServiceCollection services, IConfiguration config)
    {
        var connection = config.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(connection);
            options.AbortOnConnectFail = false;
            options.ConnectRetry = 5;
            return ConnectionMultiplexer.Connect(options);
        });
        return services;
    }

    private static void ConfigureDownloadClient(HttpClient client)
    {
        client.Timeout = TimeSpan.FromSeconds(60);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("OLS-ControlCenter/1.0");
    }
}

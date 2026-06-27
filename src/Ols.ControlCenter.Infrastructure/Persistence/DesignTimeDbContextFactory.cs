using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ols.ControlCenter.Infrastructure.Persistence;

/// <summary>
/// `dotnet ef migrations` / `database update` komutlarının DbContext'i oluşturabilmesi için
/// design-time factory. Bağlantı dizesini ortam değişkeninden okur, yoksa yerel docker
/// varsayılanını kullanır (canlıya alınırken ortam değişkeni zorunludur).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("OLS_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=ols_control_center;Username=ols;Password=ols_dev_password";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(w =>
                w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning))
            .Options;

        return new AppDbContext(options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ols.ControlCenter.Infrastructure.Persistence;

namespace Ols.ControlCenter.UnitTests;

/// <summary>
/// DB-bağlı servisleri gerçek Postgres olmadan test etmek için EF Core InMemory bağlamı.
/// Her çağrı izole bir veritabanı verir. InMemory transaction desteklemediğinden ilgili uyarı yutulur.
/// </summary>
internal static class InMemoryDb
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }
}

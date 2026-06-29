using Hangfire;
using Hangfire.MemoryStorage;
using Ols.ControlCenter.Application;
using Ols.ControlCenter.Application.Abstractions.Realtime;
using Ols.ControlCenter.Infrastructure;
using Ols.ControlCenter.Infrastructure.Realtime;
using Ols.ControlCenter.Worker.Jobs;

var builder = Host.CreateApplicationBuilder(args);

// Uygulama + altyapı servisleri (DbContext, veri entegrasyon motoru, risk motoru...).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRedisConnection(builder.Configuration);

// Worker, canlı olayları Redis'e yayınlar; API'deki köprü SignalR ile dashboard'a iletir.
builder.Services.AddSingleton<IRealtimeNotifier, RedisRealtimeNotifier>();
builder.Services.AddScoped<PeriodicSyncJob>();

// Hangfire — bellek tabanlı depolama (recurring job her başlangıçta yeniden kaydedilir,
// kalıcılık gerekmez; Postgres şemasını kirletmeden çalışır).
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());
builder.Services.AddHangfireServer();

var host = builder.Build();

// Periyodik iş: her dakika tetiklenir; her kaynak yalnızca SyncIntervalMinutes dolunca işlenir.
var recurringJobs = host.Services.GetRequiredService<IRecurringJobManager>();
recurringJobs.AddOrUpdate<PeriodicSyncJob>(
    "periodic-data-sync",
    job => job.RunAsync(CancellationToken.None),
    Cron.Minutely);

host.Run();

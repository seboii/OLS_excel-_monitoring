using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Infrastructure;
using Ols.ControlCenter.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console());

// --- Servisler ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "OLS Operasyon Kontrol Merkezi API", Version = "v1" });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

var app = builder.Build();

// --- Veritabanı migrasyon + seed ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var hasher = services.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(db, hasher);

    Log.Information("Veritabanı migrasyonu ve seed tamamlandı.");
}

// --- HTTP pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.DocumentTitle = "OLS Operasyon Kontrol Merkezi API");
}

app.UseSerilogRequestLogging();
app.UseCors("frontend");
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "OLS Operasyon Kontrol Merkezi",
    time = DateTimeOffset.UtcNow
}));

app.Run();

/// <summary>Integration testlerinin WebApplicationFactory ile erişebilmesi için.</summary>
public partial class Program { }

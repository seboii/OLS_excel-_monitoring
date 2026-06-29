using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ols.ControlCenter.Api.Realtime;
using Ols.ControlCenter.Api.Security;
using Ols.ControlCenter.Application;
using Ols.ControlCenter.Application.Abstractions.Realtime;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Infrastructure;
using Ols.ControlCenter.Infrastructure.Persistence;
using Ols.ControlCenter.Shared.Authorization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console());

// --- Servisler ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "OLS Operasyon Kontrol Merkezi API", Version = "v1" });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Canlı bildirim (SignalR) ---
builder.Services.AddSignalR();
builder.Services.AddSingleton<IRealtimeNotifier, SignalRRealtimeNotifier>();

// Redis köprüsü: Worker'ın (Hangfire) yayınladığı olayları SignalR'a ilet.
builder.Services.AddRedisConnection(builder.Configuration);
builder.Services.AddHostedService<RedisRealtimeBridge>();

// --- Kimlik doğrulama (JWT) + yetkilendirme ---
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = ClaimTypes.Role,
        };

        // SignalR/WebSocket: tarayıcı Authorization header gönderemediği için
        // token'ı query string'ten (?access_token=...) al.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    foreach (var role in AppRoles.All)
        options.AddPolicy(role, policy => policy.RequireRole(role));
});

builder.Services.AddCors(options =>
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// IP başına dakikada 5 deneme — brute-force/parola tahmin denemelerini sınırlar (login endpoint'i).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = 5,
            QueueLimit = 0,
        }));
});

var app = builder.Build();

// --- Veritabanı migrasyon + seed ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var hasher = services.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(db, hasher, app.Configuration);

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
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<DashboardHub>("/api/hubs/dashboard");

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "OLS Operasyon Kontrol Merkezi",
    time = DateTimeOffset.UtcNow
})).AllowAnonymous();

app.Run();

/// <summary>Integration testlerinin WebApplicationFactory ile erişebilmesi için.</summary>
public partial class Program { }

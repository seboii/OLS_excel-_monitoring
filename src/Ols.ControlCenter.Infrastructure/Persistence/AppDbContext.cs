using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Domain.Common;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Infrastructure.Persistence;

/// <summary>
/// Uygulamanın EF Core veritabanı bağlamı. Enum'lar string olarak saklanır (okunabilirlik),
/// soft-delete entity'lere global sorgu filtresi uygulanır, JSONB alanlar yapılandırılır.
/// </summary>
public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<DataSource> DataSources => Set<DataSource>();
    public DbSet<DataSourceColumnMapping> DataSourceColumnMappings => Set<DataSourceColumnMapping>();
    public DbSet<DataSyncLog> DataSyncLogs => Set<DataSyncLog>();
    public DbSet<StatusMapping> StatusMappings => Set<StatusMapping>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<OperationDetail> OperationDetails => Set<OperationDetail>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();
    public DbSet<RiskRule> RiskRules => Set<RiskRule>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<KpiSnapshot> KpiSnapshots => Set<KpiSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // ISoftDelete uygulayan tüm entity'lere global "silinmemiş" filtresi
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                typeof(AppDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(null, new object[] { modelBuilder });
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder)
        where TEntity : class, ISoftDelete
        => builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);

    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        // Para/ondalık alanlar için varsayılan hassasiyet
        b.Properties<decimal>().HavePrecision(18, 2);

        // Tüm enum'lar string olarak saklanır (DB okunabilir, sıralama değişikliğine dayanıklı)
        b.Properties<TransportType>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<ServiceType>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<TradeDirection>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<OperationStatus>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<RiskLevel>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<DelayReason>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<FinanceStatus>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<DocumentStatus>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<DocumentType>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<AlertType>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<AlertStatus>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<WorkTaskStatus>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<TaskPriority>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<CommentType>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<DataSourceType>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<SyncStatus>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<NotificationLevel>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<NotificationType>().HaveConversion<string>().HaveMaxLength(40);
    }
}

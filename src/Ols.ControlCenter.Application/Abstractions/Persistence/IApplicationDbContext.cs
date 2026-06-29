using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Entities.Tracking;

namespace Ols.ControlCenter.Application.Abstractions.Persistence;

/// <summary>
/// Application katmanının veritabanına eriştiği soyutlama. AppDbContext bunu uygular;
/// böylece use-case'ler Infrastructure'a doğrudan bağımlı olmaz.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Department> Departments { get; }
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Customer> Customers { get; }
    DbSet<DataSource> DataSources { get; }
    DbSet<DataSourceColumnMapping> DataSourceColumnMappings { get; }
    DbSet<DataSyncLog> DataSyncLogs { get; }
    DbSet<StatusMapping> StatusMappings { get; }
    DbSet<Operation> Operations { get; }
    DbSet<OperationDetail> OperationDetails { get; }
    DbSet<Comment> Comments { get; }
    DbSet<WorkTask> WorkTasks { get; }
    DbSet<Alert> Alerts { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Document> Documents { get; }
    DbSet<StatusHistory> StatusHistories { get; }
    DbSet<RiskRule> RiskRules { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<KpiSnapshot> KpiSnapshots { get; }
    DbSet<ImportedRawRow> ImportedRawRows { get; }

    // --- Takip tabloları (sayfa-başına tablo) ---
    DbSet<SeaTransitRecord> SeaTransitRecords { get; }
    DbSet<SeaImportRecord> SeaImportRecords { get; }
    DbSet<SeaExportRecord> SeaExportRecords { get; }
    DbSet<RoadTransitRecord> RoadTransitRecords { get; }
    DbSet<RoadLoadRecord> RoadLoadRecords { get; }
    DbSet<RoadArchiveRecord> RoadArchiveRecords { get; }
    DbSet<AlaboraFinanceRecord> AlaboraFinanceRecords { get; }
    DbSet<AirOperationRecord> AirOperationRecords { get; }
    DbSet<AirDailyRecord> AirDailyRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

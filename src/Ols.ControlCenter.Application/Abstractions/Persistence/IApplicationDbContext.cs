using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Domain.Entities;

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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

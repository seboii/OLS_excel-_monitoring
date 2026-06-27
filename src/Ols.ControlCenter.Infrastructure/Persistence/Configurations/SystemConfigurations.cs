using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ols.ControlCenter.Domain.Entities;

namespace Ols.ControlCenter.Infrastructure.Persistence.Configurations;

public sealed class RiskRuleConfiguration : IEntityTypeConfiguration<RiskRule>
{
    public void Configure(EntityTypeBuilder<RiskRule> b)
    {
        b.Property(x => x.Code).IsRequired().HasMaxLength(40);
        b.Property(x => x.Name).IsRequired().HasMaxLength(160);
        b.Property(x => x.Description).HasMaxLength(500);
        b.HasIndex(x => x.Code).IsUnique();

        b.Property(x => x.Parameters)
            .HasColumnType("jsonb")
            .HasConversion(
                JsonConversions.Converter<Dictionary<string, string>>(),
                JsonConversions.JsonComparer<Dictionary<string, string>>());
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Body).IsRequired().HasMaxLength(2000);
        b.Property(x => x.RelatedEntityType).HasMaxLength(60);

        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.Property(x => x.Action).IsRequired().HasMaxLength(60);
        b.Property(x => x.EntityType).IsRequired().HasMaxLength(80);
        b.Property(x => x.EntityId).HasMaxLength(60);
        b.Property(x => x.UserName).HasMaxLength(160);
        b.Property(x => x.IpAddress).HasMaxLength(60);

        b.HasIndex(x => new { x.EntityType, x.EntityId });
        b.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}

public sealed class KpiSnapshotConfiguration : IEntityTypeConfiguration<KpiSnapshot>
{
    public void Configure(EntityTypeBuilder<KpiSnapshot> b)
    {
        b.Property(x => x.Scope).IsRequired().HasMaxLength(20);

        b.Property(x => x.Metrics)
            .HasColumnType("jsonb")
            .HasConversion(
                JsonConversions.Converter<Dictionary<string, double>>(),
                JsonConversions.JsonComparer<Dictionary<string, double>>());

        b.HasIndex(x => new { x.Scope, x.ScopeId, x.Period }).IsUnique();
    }
}

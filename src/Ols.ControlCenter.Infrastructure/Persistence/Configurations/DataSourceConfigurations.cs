using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ols.ControlCenter.Domain.Entities;

namespace Ols.ControlCenter.Infrastructure.Persistence.Configurations;

public sealed class DataSourceConfiguration : IEntityTypeConfiguration<DataSource>
{
    public void Configure(EntityTypeBuilder<DataSource> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(160);
        b.Property(x => x.ConnectionConfigEncrypted).IsRequired();
        b.Property(x => x.SheetName).HasMaxLength(120);

        b.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => x.IsActive);
    }
}

public sealed class DataSourceColumnMappingConfiguration : IEntityTypeConfiguration<DataSourceColumnMapping>
{
    public void Configure(EntityTypeBuilder<DataSourceColumnMapping> b)
    {
        b.Property(x => x.SourceColumn).IsRequired().HasMaxLength(160);
        b.Property(x => x.TargetField).IsRequired().HasMaxLength(80);
        b.Property(x => x.Transform).HasMaxLength(120);

        b.HasOne(x => x.DataSource)
            .WithMany(d => d.ColumnMappings)
            .HasForeignKey(x => x.DataSourceId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.DataSourceId, x.SourceColumn }).IsUnique();
    }
}

public sealed class DataSyncLogConfiguration : IEntityTypeConfiguration<DataSyncLog>
{
    public void Configure(EntityTypeBuilder<DataSyncLog> b)
    {
        b.Property(x => x.Message).HasMaxLength(2000);

        b.HasOne(x => x.DataSource)
            .WithMany(d => d.SyncLogs)
            .HasForeignKey(x => x.DataSourceId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.DataSourceId, x.StartedAt });
    }
}

public sealed class StatusMappingConfiguration : IEntityTypeConfiguration<StatusMapping>
{
    public void Configure(EntityTypeBuilder<StatusMapping> b)
    {
        b.Property(x => x.SourceStatus).IsRequired().HasMaxLength(120);

        b.HasOne(x => x.DataSource)
            .WithMany(d => d.StatusMappings)
            .HasForeignKey(x => x.DataSourceId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.DataSourceId, x.SourceStatus });
    }
}

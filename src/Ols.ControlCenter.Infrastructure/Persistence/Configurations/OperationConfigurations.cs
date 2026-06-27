using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ols.ControlCenter.Domain.Entities;

namespace Ols.ControlCenter.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.Currency).HasMaxLength(8);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.HasIndex(x => x.Name);
        b.HasIndex(x => x.IsCritical);
    }
}

public sealed class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> b)
    {
        b.Property(x => x.SourceOperationNo).HasMaxLength(80);
        b.Property(x => x.CustomerName).IsRequired().HasMaxLength(200).HasDefaultValue(string.Empty);
        b.Property(x => x.Shipper).HasMaxLength(200);
        b.Property(x => x.Consignee).HasMaxLength(200);
        b.Property(x => x.OriginCountry).HasMaxLength(80);
        b.Property(x => x.OriginCity).HasMaxLength(120);
        b.Property(x => x.DestinationCountry).HasMaxLength(80);
        b.Property(x => x.DestinationCity).HasMaxLength(120);
        b.Property(x => x.NextActionDescription).HasMaxLength(500);
        b.Property(x => x.Currency).IsRequired().HasMaxLength(8).HasDefaultValue("EUR");

        // İlişkiler
        b.HasOne(x => x.Source)
            .WithMany(s => s.Operations)
            .HasForeignKey(x => x.SourceId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Department)
            .WithMany(d => d.Operations)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Customer)
            .WithMany(c => c.Operations)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.ResponsibleUser)
            .WithMany()
            .HasForeignKey(x => x.ResponsibleUserId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.SalesOwner)
            .WithMany()
            .HasForeignKey(x => x.SalesOwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.NextActionOwner)
            .WithMany()
            .HasForeignKey(x => x.NextActionOwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Detail)
            .WithOne(d => d.Operation)
            .HasForeignKey<OperationDetail>(d => d.OperationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexler — dashboard filtreleri ve tekil kaynak anahtarı
        b.HasIndex(x => new { x.SourceId, x.SourceOperationNo }).IsUnique();
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.RiskLevel);
        b.HasIndex(x => x.TransportType);
        b.HasIndex(x => x.Eta);
        b.HasIndex(x => x.DepartmentId);
        b.HasIndex(x => x.ResponsibleUserId);
        b.HasIndex(x => x.CustomerId);
    }
}

public sealed class OperationDetailConfiguration : IEntityTypeConfiguration<OperationDetail>
{
    public void Configure(EntityTypeBuilder<OperationDetail> b)
    {
        b.Property(x => x.BlNo).HasMaxLength(80);
        b.Property(x => x.ContainerNo).HasMaxLength(80);
        b.Property(x => x.VesselName).HasMaxLength(120);
        b.Property(x => x.ShippingLine).HasMaxLength(120);
        b.Property(x => x.HawbNo).HasMaxLength(80);
        b.Property(x => x.MawbNo).HasMaxLength(80);
        b.Property(x => x.FlightNo).HasMaxLength(40);
        b.Property(x => x.VehiclePlate).HasMaxLength(40);
        b.Property(x => x.DriverName).HasMaxLength(120);
        b.Property(x => x.BorderCrossing).HasMaxLength(120);

        b.Property(x => x.ExtraAttributes)
            .HasColumnType("jsonb")
            .HasConversion(
                JsonConversions.Converter<Dictionary<string, string>>(),
                JsonConversions.JsonComparer<Dictionary<string, string>>());

        b.HasIndex(x => x.ContainerNo);
        b.HasIndex(x => x.BlNo);
    }
}

public sealed class StatusHistoryConfiguration : IEntityTypeConfiguration<StatusHistory>
{
    public void Configure(EntityTypeBuilder<StatusHistory> b)
    {
        b.Property(x => x.Source).IsRequired().HasMaxLength(20);
        b.Property(x => x.Note).HasMaxLength(500);

        b.HasOne(x => x.Operation)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(x => x.OperationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.OperationId, x.ChangedAt });
    }
}

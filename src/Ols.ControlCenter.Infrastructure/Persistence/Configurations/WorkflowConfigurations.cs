using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ols.ControlCenter.Domain.Entities;

namespace Ols.ControlCenter.Infrastructure.Persistence.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> b)
    {
        b.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        b.Property(x => x.BoardKey).HasMaxLength(60);
        b.Property(x => x.BoardTitle).HasMaxLength(120);
        b.Property(x => x.Group).HasMaxLength(40);
        b.Property(x => x.RecordRef).HasMaxLength(120);

        b.Property(x => x.Mentions)
            .HasColumnType("jsonb")
            .HasConversion(JsonConversions.Converter<List<string>>(), JsonConversions.StringListComparer);

        b.HasOne(x => x.Operation)
            .WithMany(o => o.Comments)
            .HasForeignKey(x => x.OperationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Author)
            .WithMany()
            .HasForeignKey(x => x.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.OperationId, x.CreatedAt });
        b.HasIndex(x => new { x.BoardKey, x.RecordRef, x.CreatedAt });
    }
}

public sealed class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> b)
    {
        b.Property(x => x.Title).IsRequired().HasMaxLength(240);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.CompletionNote).HasMaxLength(2000);

        b.HasOne(x => x.Operation)
            .WithMany(o => o.Tasks)
            .HasForeignKey(x => x.OperationId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => new { x.Status, x.DueDate });
        b.HasIndex(x => x.OwnerUserId);
    }
}

public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> b)
    {
        b.Property(x => x.Description).IsRequired().HasMaxLength(1000);
        b.Property(x => x.RuleCode).IsRequired().HasMaxLength(40);
        b.Property(x => x.DedupeKey).IsRequired().HasMaxLength(120);
        b.Property(x => x.ResolutionNote).HasMaxLength(1000);
        b.Property(x => x.BoardKey).HasMaxLength(60);
        b.Property(x => x.BoardTitle).HasMaxLength(120);
        b.Property(x => x.Group).HasMaxLength(40);
        b.Property(x => x.RecordRef).HasMaxLength(120);

        b.HasOne(x => x.Operation)
            .WithMany(o => o.Alerts)
            .HasForeignKey(x => x.OperationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.ResponsibleUser)
            .WithMany()
            .HasForeignKey(x => x.ResponsibleUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Aynı kaynak (operasyon veya board satırı) + kural için tek satır (tekrar tetiklenince güncellenir)
        b.HasIndex(x => x.DedupeKey).IsUnique();
        b.HasIndex(x => new { x.Status, x.RiskLevel });
        b.HasIndex(x => x.Type);
        b.HasIndex(x => x.BoardKey);
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.Property(x => x.InvoiceNo).IsRequired().HasMaxLength(80);
        b.Property(x => x.Bank).HasMaxLength(120);
        b.Property(x => x.Currency).IsRequired().HasMaxLength(8).HasDefaultValue("EUR");
        b.Property(x => x.CustomerName).HasMaxLength(200);

        b.HasOne(x => x.Operation)
            .WithMany(o => o.Payments)
            .HasForeignKey(x => x.OperationId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.FinanceUser)
            .WithMany()
            .HasForeignKey(x => x.FinanceUserId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.DueDate);
        b.HasIndex(x => x.InvoiceNo);
    }
}

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> b)
    {
        b.Property(x => x.Note).HasMaxLength(500);

        b.HasOne(x => x.Operation)
            .WithMany(o => o.Documents)
            .HasForeignKey(x => x.OperationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.OperationId, x.DocType });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ols.ControlCenter.Domain.Entities.Tracking;

namespace Ols.ControlCenter.Infrastructure.Persistence.Configurations;

/// <summary>
/// Tüm sayfa-başına takip tablolarının ortak EF yapılandırması: kaynak satır anahtarı indeksleri
/// ve ham JSON sütununun jsonb olarak saklanması. Somut tipler kendi tablolarına eşlenir.
/// </summary>
public abstract class TrackingRecordConfiguration<T> : IEntityTypeConfiguration<T>
    where T : TrackingRecordBase
{
    public virtual void Configure(EntityTypeBuilder<T> b)
    {
        b.HasIndex(x => new { x.DataSourceId, x.SourceRowKey });
        b.HasIndex(x => x.DataSourceId);
        b.Property(x => x.SourceRowKey).HasMaxLength(120);
        b.Property(x => x.RawJson).HasColumnType("jsonb");
    }
}

public sealed class SeaTransitRecordConfiguration : TrackingRecordConfiguration<SeaTransitRecord> { }
public sealed class SeaImportRecordConfiguration : TrackingRecordConfiguration<SeaImportRecord> { }
public sealed class SeaExportRecordConfiguration : TrackingRecordConfiguration<SeaExportRecord> { }
public sealed class RoadTransitRecordConfiguration : TrackingRecordConfiguration<RoadTransitRecord> { }
public sealed class RoadLoadRecordConfiguration : TrackingRecordConfiguration<RoadLoadRecord> { }
public sealed class RoadArchiveRecordConfiguration : TrackingRecordConfiguration<RoadArchiveRecord> { }
public sealed class AlaboraFinanceRecordConfiguration : TrackingRecordConfiguration<AlaboraFinanceRecord> { }
public sealed class AirOperationRecordConfiguration : TrackingRecordConfiguration<AirOperationRecord> { }
public sealed class AirDailyRecordConfiguration : TrackingRecordConfiguration<AirDailyRecord> { }

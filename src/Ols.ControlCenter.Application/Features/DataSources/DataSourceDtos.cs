using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.DataSources;

public sealed record DataSourceDto(
    long Id,
    string Name,
    string Type,
    string AccessType,
    string? Url,
    string? DefaultTransportType,
    long? DepartmentId,
    string? DepartmentName,
    string? SheetName,
    int HeaderRowIndex,
    int SyncIntervalMinutes,
    bool IsActive,
    DateTimeOffset? LastSyncAt,
    DateTimeOffset? LastSuccessSyncAt,
    string? LastSyncStatus,
    string? LastSyncError,
    int MappingCount);

public sealed class CreateDataSourceRequest
{
    public string Name { get; set; } = string.Empty;
    public DataSourceType Type { get; set; }
    public DataSourceAccessType AccessType { get; set; } = DataSourceAccessType.Public;

    /// <summary>Public kaynak linki (Yandex/SharePoint).</summary>
    public string? Url { get; set; }

    public TransportType? DefaultTransportType { get; set; }
    public long? DepartmentId { get; set; }
    public string? SheetName { get; set; }
    public int HeaderRowIndex { get; set; } = 1;
    public int SyncIntervalMinutes { get; set; } = 15;

    /// <summary>Private token vb. — sunucuda şifreli saklanır.</summary>
    public string? ConnectionConfig { get; set; }
}

public sealed class UpdateDataSourceRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DataSourceAccessType AccessType { get; set; } = DataSourceAccessType.Public;
    public string? Url { get; set; }
    public TransportType? DefaultTransportType { get; set; }
    public long? DepartmentId { get; set; }
    public string? SheetName { get; set; }
    public int HeaderRowIndex { get; set; } = 1;
    public int SyncIntervalMinutes { get; set; } = 15;
    public string? ConnectionConfig { get; set; }
}

public sealed record ColumnMappingDto(
    long Id, string SourceColumn, int? SourceColumnIndex, string TargetField,
    string? TransformType, string? DefaultValue, bool IsRequired);

public sealed class ColumnMappingInput
{
    public string SourceColumn { get; set; } = string.Empty;
    public int? SourceColumnIndex { get; set; }
    public string TargetField { get; set; } = string.Empty;
    public string? TransformType { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
}

public sealed class PreviewRequest
{
    public string? SheetName { get; set; }
    public int? HeaderRowIndex { get; set; }
}

public sealed record SyncLogDto(
    long Id,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string Status,
    int RowsRead,
    int RowsUpserted,
    int RowsFailed,
    string? Message,
    long? DurationMs,
    string? FileName,
    string? SheetName);

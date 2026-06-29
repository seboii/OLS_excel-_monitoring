using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Domain.Entities;

namespace Ols.ControlCenter.Application.Features.DataSources;

public interface IDataSourceService
{
    Task<IReadOnlyList<DataSourceDto>> GetListAsync(CancellationToken ct);
    Task<DataSourceDto> CreateAsync(CreateDataSourceRequest req, long? userId, CancellationToken ct);
    Task<bool> UpdateAsync(long id, UpdateDataSourceRequest req, long? userId, CancellationToken ct);
    Task<bool> DeleteAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<ColumnMappingDto>> GetMappingsAsync(long id, CancellationToken ct);
    Task<bool> ReplaceMappingsAsync(long id, IReadOnlyList<ColumnMappingInput> mappings, CancellationToken ct);
    Task<IReadOnlyList<SyncLogDto>> GetSyncLogsAsync(long id, CancellationToken ct);
}

public sealed class DataSourceService : IDataSourceService
{
    private readonly IApplicationDbContext _db;
    private readonly ISecretProtector _protector;
    private readonly IDataSyncLogService _syncLogs;

    public DataSourceService(IApplicationDbContext db, ISecretProtector protector, IDataSyncLogService syncLogs)
    {
        _db = db;
        _protector = protector;
        _syncLogs = syncLogs;
    }

    public async Task<IReadOnlyList<DataSourceDto>> GetListAsync(CancellationToken ct)
    {
        var raw = await _db.DataSources.AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new
            {
                d.Id, d.Name, d.Type, d.AccessType, d.Url, d.DefaultTransportType, d.DepartmentId,
                DeptName = d.Department != null ? d.Department.Name : null,
                d.SheetName, d.HeaderRowIndex, d.SyncIntervalMinutes, d.IsActive,
                d.LastSyncAt, d.LastSuccessSyncAt, d.LastSyncStatus, d.LastSyncError,
                MappingCount = d.ColumnMappings.Count,
            })
            .ToListAsync(ct);

        return raw.Select(d => new DataSourceDto(
            d.Id, d.Name, d.Type.ToString(), d.AccessType.ToString(), d.Url,
            d.DefaultTransportType?.ToString(), d.DepartmentId, d.DeptName,
            d.SheetName, d.HeaderRowIndex, d.SyncIntervalMinutes, d.IsActive,
            d.LastSyncAt, d.LastSuccessSyncAt, d.LastSyncStatus?.ToString(), d.LastSyncError, d.MappingCount)).ToList();
    }

    public async Task<DataSourceDto> CreateAsync(CreateDataSourceRequest req, long? userId, CancellationToken ct)
    {
        var entity = new DataSource
        {
            Name = req.Name,
            Type = req.Type,
            AccessType = req.AccessType,
            Url = string.IsNullOrWhiteSpace(req.Url) ? null : req.Url.Trim(),
            DefaultTransportType = req.DefaultTransportType,
            DepartmentId = req.DepartmentId,
            SheetName = req.SheetName,
            HeaderRowIndex = req.HeaderRowIndex <= 0 ? 1 : req.HeaderRowIndex,
            SyncIntervalMinutes = req.SyncIntervalMinutes <= 0 ? 15 : req.SyncIntervalMinutes,
            ConnectionConfigEncrypted = _protector.Protect(req.ConnectionConfig ?? string.Empty),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = userId,
        };
        _db.DataSources.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new DataSourceDto(entity.Id, entity.Name, entity.Type.ToString(), entity.AccessType.ToString(), entity.Url,
            entity.DefaultTransportType?.ToString(), entity.DepartmentId, null,
            entity.SheetName, entity.HeaderRowIndex, entity.SyncIntervalMinutes, entity.IsActive,
            null, null, null, null, 0);
    }

    public async Task<bool> UpdateAsync(long id, UpdateDataSourceRequest req, long? userId, CancellationToken ct)
    {
        var entity = await _db.DataSources.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (entity is null) return false;

        entity.Name = req.Name;
        entity.IsActive = req.IsActive;
        entity.AccessType = req.AccessType;
        entity.Url = string.IsNullOrWhiteSpace(req.Url) ? null : req.Url.Trim();
        entity.DefaultTransportType = req.DefaultTransportType;
        entity.DepartmentId = req.DepartmentId;
        entity.SheetName = req.SheetName;
        entity.HeaderRowIndex = req.HeaderRowIndex <= 0 ? 1 : req.HeaderRowIndex;
        entity.SyncIntervalMinutes = req.SyncIntervalMinutes <= 0 ? 15 : req.SyncIntervalMinutes;
        if (!string.IsNullOrEmpty(req.ConnectionConfig))
            entity.ConnectionConfigEncrypted = _protector.Protect(req.ConnectionConfig);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedByUserId = userId;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        var entity = await _db.DataSources.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (entity is null) return false;
        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<ColumnMappingDto>> GetMappingsAsync(long id, CancellationToken ct)
        => await _db.DataSourceColumnMappings.AsNoTracking()
            .Where(m => m.DataSourceId == id)
            .OrderBy(m => m.Id)
            .Select(m => new ColumnMappingDto(m.Id, m.SourceColumn, m.SourceColumnIndex, m.TargetField, m.TransformType, m.DefaultValue, m.IsRequired))
            .ToListAsync(ct);

    public async Task<bool> ReplaceMappingsAsync(long id, IReadOnlyList<ColumnMappingInput> mappings, CancellationToken ct)
    {
        var exists = await _db.DataSources.AnyAsync(d => d.Id == id, ct);
        if (!exists) return false;

        var existing = await _db.DataSourceColumnMappings.Where(m => m.DataSourceId == id).ToListAsync(ct);
        _db.DataSourceColumnMappings.RemoveRange(existing);

        foreach (var m in mappings.Where(x => !string.IsNullOrWhiteSpace(x.SourceColumn) && !string.IsNullOrWhiteSpace(x.TargetField)))
        {
            _db.DataSourceColumnMappings.Add(new DataSourceColumnMapping
            {
                DataSourceId = id,
                SourceColumn = m.SourceColumn.Trim(),
                SourceColumnIndex = m.SourceColumnIndex,
                TargetField = m.TargetField.Trim(),
                TransformType = m.TransformType,
                DefaultValue = m.DefaultValue,
                IsRequired = m.IsRequired,
            });
        }
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<IReadOnlyList<SyncLogDto>> GetSyncLogsAsync(long id, CancellationToken ct)
        => _syncLogs.GetLogsAsync(id, ct);
}

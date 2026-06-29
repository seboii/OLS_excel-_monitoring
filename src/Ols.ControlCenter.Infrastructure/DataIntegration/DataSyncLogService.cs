using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Features.DataSources;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>
/// Senkron loglarını okur ve her çalışmanın sonucunu kalıcılaştırmak üzere hazırlar
/// (kaynağın son-senkron alanları + <see cref="DataSyncLog"/>). Bkz. <see cref="IDataSyncLogService"/>.
/// </summary>
public sealed class DataSyncLogService : IDataSyncLogService
{
    private readonly IApplicationDbContext _db;

    public DataSyncLogService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<SyncLogDto>> GetLogsAsync(long dataSourceId, CancellationToken ct)
    {
        var raw = await _db.DataSyncLogs.AsNoTracking()
            .Where(l => l.DataSourceId == dataSourceId)
            .OrderByDescending(l => l.StartedAt)
            .Take(50)
            .Select(l => new { l.Id, l.StartedAt, l.FinishedAt, l.Status, l.RowsRead, l.RowsUpserted, l.RowsFailed, l.Message, l.DurationMs, l.FileName, l.SheetName })
            .ToListAsync(ct);

        return raw.Select(l => new SyncLogDto(
            l.Id, l.StartedAt, l.FinishedAt, l.Status.ToString(),
            l.RowsRead, l.RowsUpserted, l.RowsFailed, l.Message, l.DurationMs, l.FileName, l.SheetName)).ToList();
    }

    public void RecordResult(DataSource source, SyncRunResult result)
    {
        source.LastSyncAt = result.FinishedAt;
        source.LastSyncStatus = result.Status;
        source.LastSyncError = result.Errors.Count > 0 ? string.Join(" | ", result.Errors.Take(5)) : null;
        if (result.RowsUpserted > 0) source.LastSuccessSyncAt = result.FinishedAt;

        _db.DataSyncLogs.Add(new DataSyncLog
        {
            DataSourceId = source.Id,
            StartedAt = result.StartedAt,
            FinishedAt = result.FinishedAt,
            Status = result.Status,
            RowsRead = result.RowsRead,
            RowsUpserted = result.RowsUpserted,
            RowsFailed = result.RowsFailed,
            Message = result.Errors.Count > 0 ? string.Join(" | ", result.Errors.Take(10)) : null,
            DurationMs = (long)(result.FinishedAt - result.StartedAt).TotalMilliseconds,
            FileName = result.FileName,
            SheetName = result.SheetName,
        });
    }
}

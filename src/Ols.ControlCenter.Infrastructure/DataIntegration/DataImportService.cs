using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Features.DataSources;
using Ols.ControlCenter.Domain.Entities;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>İndir → oku → (önizle/eşleştir) → Operations'a aktar orkestratörü. Upsert işini DataSyncService'e devreder.</summary>
public sealed class DataImportService : IDataImportService
{
    private readonly IDataSourceDownloader _downloader;
    private readonly IExcelReaderService _reader;
    private readonly IColumnMappingService _mapping;
    private readonly IDataSyncService _sync;
    private readonly IApplicationDbContext _db;

    public DataImportService(
        IDataSourceDownloader downloader, IExcelReaderService reader,
        IColumnMappingService mapping, IDataSyncService sync, IApplicationDbContext db)
    {
        _downloader = downloader;
        _reader = reader;
        _mapping = mapping;
        _sync = sync;
        _db = db;
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(long dataSourceId, CancellationToken ct)
    {
        var ds = await GetSourceAsync(dataSourceId, ct);
        var file = await _downloader.DownloadAsync(ds, ct);
        var sheets = _reader.GetSheetNames(file.Content);
        return new ConnectionTestResult(true, file.FileName, file.Content.LongLength, sheets);
    }

    public async Task<ImportPreview> PreviewSourceAsync(long dataSourceId, string? sheetName, int? headerRowIndex, CancellationToken ct)
    {
        var ds = await GetSourceAsync(dataSourceId, ct);
        var file = await _downloader.DownloadAsync(ds, ct);
        return BuildPreview(file.Content, sheetName ?? ds.SheetName, headerRowIndex);
    }

    public async Task<SyncResult> SyncSourceAsync(long dataSourceId, long? userId, CancellationToken ct)
    {
        var ds = await GetSourceAsync(dataSourceId, ct);
        var file = await _downloader.DownloadAsync(ds, ct);
        // Takip tabloları için başlık satırı kaynakta sabittir (çoğu 1, Alabora 4). Otomatik algı yerine
        // tanımlı HeaderRowIndex kullanılır; böylece düzensiz sayfalarda veri satırı başlık sanılmaz.
        var header = ds.HeaderRowIndex >= 1 ? ds.HeaderRowIndex : (int?)null;
        var sheet = _reader.ReadSheet(file.Content, ds.SheetName, header, int.MaxValue);
        return await _sync.SyncRowsAsync(dataSourceId, sheet.Rows, userId, file.FileName, sheet.SheetName, ct);
    }

    public Task<ImportPreview> PreviewUploadAsync(byte[] content, string fileName, string? sheetName, int? headerRowIndex, CancellationToken ct)
        => Task.FromResult(BuildPreview(content, sheetName, headerRowIndex));

    public async Task<SyncResult> ImportUploadAsync(long dataSourceId, byte[] content, string fileName, string? sheetName, int? headerRowIndex, long? userId, CancellationToken ct)
    {
        await GetSourceAsync(dataSourceId, ct);
        var header = headerRowIndex is > 0 ? headerRowIndex : null;
        var sheet = _reader.ReadSheet(content, sheetName, header, int.MaxValue);
        return await _sync.SyncRowsAsync(dataSourceId, sheet.Rows, userId, fileName, sheet.SheetName, ct);
    }

    private ImportPreview BuildPreview(byte[] content, string? sheetName, int? headerRowIndex)
    {
        var sheets = _reader.GetSheetNames(content);
        var sheet = _reader.ReadSheet(content, sheetName, headerRowIndex, 50);
        var suggestions = _mapping.Suggest(sheet.Columns);
        return new ImportPreview(sheets, sheet, suggestions);
    }

    private async Task<DataSource> GetSourceAsync(long id, CancellationToken ct)
        => await _db.DataSources.FirstOrDefaultAsync(d => d.Id == id, ct)
           ?? throw new DataSourceException("Veri kaynağı bulunamadı.");
}

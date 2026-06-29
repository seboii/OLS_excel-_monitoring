using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Application.Abstractions.Realtime;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Application.Features.DataSources;
using Ols.ControlCenter.Shared.Api;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/data-sources")]
public sealed class DataSourcesController : ControllerBase
{
    private readonly IDataSourceService _service;
    private readonly IDataSyncService _sync;
    private readonly IDataImportService _import;
    private readonly ICurrentUser _current;
    private readonly IRealtimeNotifier _realtime;

    public DataSourcesController(IDataSourceService service, IDataSyncService sync, IDataImportService import, ICurrentUser current, IRealtimeNotifier realtime)
    {
        _service = service;
        _sync = sync;
        _import = import;
        _current = current;
        _realtime = realtime;
    }

    // ───────────── CRUD ─────────────
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DataSourceDto>>>> GetList(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<DataSourceDto>>.Ok(await _service.GetListAsync(ct)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DataSourceDto>>> Create([FromBody] CreateDataSourceRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(ApiResponse<DataSourceDto>.Fail("Kaynak adı zorunludur."));
        var dto = await _service.CreateAsync(req, _current.UserId, ct);
        return Ok(ApiResponse<DataSourceDto>.Ok(dto, "Veri kaynağı oluşturuldu."));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse>> Update(long id, [FromBody] UpdateDataSourceRequest req, CancellationToken ct)
        => await _service.UpdateAsync(id, req, _current.UserId, ct)
            ? Ok(ApiResponse.Ok("Güncellendi."))
            : NotFound(ApiResponse.Fail("Veri kaynağı bulunamadı."));

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken ct)
        => await _service.DeleteAsync(id, ct)
            ? Ok(ApiResponse.Ok("Silindi."))
            : NotFound(ApiResponse.Fail("Veri kaynağı bulunamadı."));

    // ───────────── Kolon eşleştirme + log ─────────────
    [HttpGet("{id:long}/column-mappings")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ColumnMappingDto>>>> GetMappings(long id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ColumnMappingDto>>.Ok(await _service.GetMappingsAsync(id, ct)));

    [HttpPut("{id:long}/column-mappings")]
    public async Task<ActionResult<ApiResponse>> ReplaceMappings(long id, [FromBody] List<ColumnMappingInput> mappings, CancellationToken ct)
        => await _service.ReplaceMappingsAsync(id, mappings, ct)
            ? Ok(ApiResponse.Ok($"{mappings.Count} eşleştirme kaydedildi."))
            : NotFound(ApiResponse.Fail("Veri kaynağı bulunamadı."));

    [HttpGet("{id:long}/sync-logs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SyncLogDto>>>> GetSyncLogs(long id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SyncLogDto>>.Ok(await _service.GetSyncLogsAsync(id, ct)));

    // ───────────── Bağlantı testi / önizleme / senkron (public link) ─────────────
    [HttpPost("{id:long}/test-connection")]
    public async Task<ActionResult<ApiResponse<ConnectionTestResult>>> TestConnection(long id, CancellationToken ct)
    {
        try
        {
            var result = await _import.TestConnectionAsync(id, ct);
            return Ok(ApiResponse<ConnectionTestResult>.Ok(result, "Bağlantı başarılı, dosya indirildi."));
        }
        catch (DataSourceException ex) { return BadRequest(ApiResponse<ConnectionTestResult>.Fail(ex.Message)); }
    }

    [HttpPost("{id:long}/download-preview")]
    public async Task<ActionResult<ApiResponse<ImportPreview>>> DownloadPreview(long id, [FromBody] PreviewRequest? req, CancellationToken ct)
    {
        try
        {
            var preview = await _import.PreviewSourceAsync(id, req?.SheetName, req?.HeaderRowIndex, ct);
            return Ok(ApiResponse<ImportPreview>.Ok(preview));
        }
        catch (DataSourceException ex) { return BadRequest(ApiResponse<ImportPreview>.Fail(ex.Message)); }
    }

    [HttpPost("{id:long}/sync")]
    public async Task<ActionResult<ApiResponse<SyncResult>>> Sync(long id, CancellationToken ct)
    {
        try
        {
            var result = await _import.SyncSourceAsync(id, _current.UserId, ct);
            await _realtime.NotifyAsync(RealtimeEvents.DataSynced, new { dataSourceId = id, result.RowsUpserted }, ct);
            return Ok(ApiResponse<SyncResult>.Ok(result, $"{result.RowsUpserted} satır işlendi, {result.RowsFailed} hata."));
        }
        catch (DataSourceException ex) { return BadRequest(ApiResponse<SyncResult>.Fail(ex.Message)); }
    }

    // ───────────── Manuel Excel yükleme ─────────────
    [HttpPost("manual-upload/preview")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ApiResponse<ImportPreview>>> ManualPreview(
        IFormFile file, [FromForm] string? sheetName, [FromForm] int? headerRowIndex, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<ImportPreview>.Fail("Dosya boş veya seçilmedi."));
        try
        {
            var bytes = await ToBytesAsync(file, ct);
            var preview = await _import.PreviewUploadAsync(bytes, file.FileName, sheetName, headerRowIndex, ct);
            return Ok(ApiResponse<ImportPreview>.Ok(preview));
        }
        catch (DataSourceException ex) { return BadRequest(ApiResponse<ImportPreview>.Fail(ex.Message)); }
    }

    [HttpPost("manual-upload/import")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ApiResponse<SyncResult>>> ManualImport(
        [FromForm] long dataSourceId, IFormFile file, [FromForm] string? sheetName, [FromForm] int? headerRowIndex, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<SyncResult>.Fail("Dosya boş veya seçilmedi."));
        try
        {
            var bytes = await ToBytesAsync(file, ct);
            var result = await _import.ImportUploadAsync(dataSourceId, bytes, file.FileName, sheetName, headerRowIndex, _current.UserId, ct);
            await _realtime.NotifyAsync(RealtimeEvents.DataSynced, new { dataSourceId, result.RowsUpserted }, ct);
            return Ok(ApiResponse<SyncResult>.Ok(result, $"{result.RowsUpserted} satır işlendi, {result.RowsFailed} hata."));
        }
        catch (DataSourceException ex) { return BadRequest(ApiResponse<SyncResult>.Fail(ex.Message)); }
    }

    private static async Task<byte[]> ToBytesAsync(IFormFile file, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        return ms.ToArray();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Domain.Entities.Tracking;
using Ols.ControlCenter.Domain.Enums;
using Ols.ControlCenter.Shared.Api;

namespace Ols.ControlCenter.Api.Controllers;

/// <summary>
/// Sayfa-başına takip tablolarını (sekmeleri) frontend'e sunar: sekme listesi (özet + risk sayıları)
/// ve sekme detayı (kolon metadata + sayfalı satırlar). Satırlar, kataloğun kolon tanımlarına göre
/// yansımayla projekte edilir; böylece tek bir genel tablo bileşeni tüm sekmeleri render edebilir.
/// </summary>
[ApiController]
[Route("api/boards")]
public sealed class BoardsController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly ITrackingMetricsService _metrics;

    public BoardsController(IApplicationDbContext db, ITrackingMetricsService metrics)
    {
        _db = db;
        _metrics = metrics;
    }

    /// <summary>
    /// Tüm sekmelerde (Deniz+Kara+Hava+Finans) tek seferde arama — header'daki global arama kutusu için.
    /// Dosya/ref no, durum ve her görünür kolonu tarar; hangi kolonda eşleştiğini de döner.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BoardSearchResultDto>>>> Search(
        [FromQuery] string? q, [FromQuery] int take = 30, CancellationToken ct = default)
    {
        var term = q?.Trim() ?? "";
        if (term.Length < 2)
            return Ok(ApiResponse<IReadOnlyList<BoardSearchResultDto>>.Ok(Array.Empty<BoardSearchResultDto>()));

        var results = new List<BoardSearchResultDto>();
        foreach (var meta in BoardCatalog.All)
        {
            var entities = await _metrics.LoadBoardEntitiesAsync(meta.Board, ct);
            var type = entities.Count > 0 ? entities[0].GetType() : null;
            var props = type is null
                ? Array.Empty<KeyValuePair<BoardColumn, System.Reflection.PropertyInfo?>>()
                : meta.Columns.Select(c => new KeyValuePair<BoardColumn, System.Reflection.PropertyInfo?>(c, type.GetProperty(c.Key))).ToArray();

            int matchedInBoard = 0;
            foreach (var e in entities)
            {
                if (matchedInBoard >= 8) break; // bir board, sonuçları boğmasın

                var (matchedLabel, matchedValue) = FindMatch(e, props, term);
                if (matchedLabel is null) continue;

                results.Add(new BoardSearchResultDto(
                    meta.Key, meta.Title, meta.Group, e.SourceRowKey, e.RiskLevel.ToString(), e.DelayDays,
                    matchedLabel, matchedValue!));
                matchedInBoard++;
            }
        }

        var ordered = results
            .OrderByDescending(r => string.Equals(r.Ref, term, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(r => r.Risk)
            .Take(Math.Clamp(take, 1, 100))
            .ToList();

        return Ok(ApiResponse<IReadOnlyList<BoardSearchResultDto>>.Ok(ordered));
    }

    private static (string? Label, string? Value) FindMatch(
        TrackingRecordBase e, IReadOnlyList<KeyValuePair<BoardColumn, System.Reflection.PropertyInfo?>> props, string term)
    {
        if (Has(e.SourceRowKey, term)) return ("Dosya/Ref No", e.SourceRowKey);
        if (Has(e.StatusText, term)) return ("Durum", e.StatusText);
        foreach (var (col, prop) in props)
        {
            if (prop?.GetValue(e) is string val && Has(val, term)) return (col.Label, val);
        }
        return (null, null);
    }

    private static bool Has(string? haystack, string term)
        => !string.IsNullOrEmpty(haystack) && haystack.Contains(term, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Tüm sekmelerdeki dikkat gerektiren (riskli/geciken) aktif kayıtlar — Risk Haritası için.
    /// İsteğe bağlı grup ve minimum risk filtresi.
    /// </summary>
    [HttpGet("attention")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BoardAttentionDto>>>> Attention(
        [FromQuery] string? group,
        [FromQuery] string? minRisk,
        [FromQuery] int take = 200,
        CancellationToken ct = default)
    {
        var min = Enum.TryParse<RiskLevel>(minRisk, ignoreCase: true, out var lvl) ? lvl : RiskLevel.Yellow;
        var rows = await _metrics.LoadRowsAsync(ct);

        var list = rows
            .Where(r => !r.Archived)
            .Where(r => string.IsNullOrWhiteSpace(group) || r.Group == group)
            .Where(r => r.Risk >= min || r.Delay > 0)
            .OrderByDescending(r => (int)r.Risk).ThenByDescending(r => r.Delay)
            .Take(Math.Clamp(take, 1, 1000))
            .Select(r => new BoardAttentionDto(
                r.BoardKey, r.BoardTitle, r.Group, r.Ref, r.Risk.ToString(), r.Delay, r.Status))
            .ToList();

        return Ok(ApiResponse<IReadOnlyList<BoardAttentionDto>>.Ok(list));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BoardSummaryDto>>>> List(CancellationToken ct)
    {
        var sources = await _db.DataSources
            .Where(d => d.TargetBoard != TrackingBoardType.None)
            .Select(d => new { d.Id, d.TargetBoard, d.LastSyncAt })
            .ToListAsync(ct);
        var byBoard = sources
            .GroupBy(s => s.TargetBoard)
            .ToDictionary(g => g.Key, g => g.First());

        var result = new List<BoardSummaryDto>();
        foreach (var meta in BoardCatalog.All)
        {
            byBoard.TryGetValue(meta.Board, out var src);
            var (total, risk) = src is null
                ? (0, EmptyRisk())
                : await CountAsync(meta.Board, src.Id, ct);
            result.Add(new BoardSummaryDto(meta.Key, meta.Title, meta.Group, src?.Id, src?.LastSyncAt, total, risk));
        }

        return Ok(ApiResponse<IReadOnlyList<BoardSummaryDto>>.Ok(result));
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<ApiResponse<BoardDetailDto>>> Detail(
        string key,
        [FromQuery] string? q,
        [FromQuery] string? risk,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var meta = BoardCatalog.Find(key);
        if (meta is null)
            return NotFound(ApiResponse<BoardDetailDto>.Fail("Sekme bulunamadı."));

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 5000);

        var src = await _db.DataSources
            .Where(d => d.TargetBoard == meta.Board)
            .Select(d => new { d.Id, d.LastSyncAt })
            .FirstOrDefaultAsync(ct);

        if (src is null)
            return Ok(ApiResponse<BoardDetailDto>.Ok(new BoardDetailDto(
                meta.Key, meta.Title, meta.Group, null, meta.Columns, Array.Empty<BoardRowDto>(), 0, null)));

        var (rows, total) = await LoadRowsAsync(meta.Board, src.Id, q, risk, page, pageSize, ct);
        var dto = new BoardDetailDto(
            meta.Key, meta.Title, meta.Group, src.Id, meta.Columns,
            rows.Select(e => ToRowDto(e, meta)).ToList(), total, src.LastSyncAt);

        return Ok(ApiResponse<BoardDetailDto>.Ok(dto));
    }

    // ───────────── Sorgu yardımcıları (board → doğru DbSet) ─────────────

    private Task<(int, Dictionary<string, int>)> CountAsync(TrackingBoardType board, long sourceId, CancellationToken ct)
        => board switch
        {
            TrackingBoardType.SeaTransit => CountSet(_db.SeaTransitRecords, sourceId, ct),
            TrackingBoardType.SeaImport => CountSet(_db.SeaImportRecords, sourceId, ct),
            TrackingBoardType.SeaExport => CountSet(_db.SeaExportRecords, sourceId, ct),
            TrackingBoardType.RoadTransit => CountSet(_db.RoadTransitRecords, sourceId, ct),
            TrackingBoardType.RoadLoad => CountSet(_db.RoadLoadRecords, sourceId, ct),
            TrackingBoardType.RoadArchive => CountSet(_db.RoadArchiveRecords, sourceId, ct),
            TrackingBoardType.Alabora => CountSet(_db.AlaboraFinanceRecords, sourceId, ct),
            TrackingBoardType.Air => CountSet(_db.AirOperationRecords, sourceId, ct),
            TrackingBoardType.AirDaily => CountSet(_db.AirDailyRecords, sourceId, ct),
            _ => Task.FromResult((0, EmptyRisk())),
        };

    private Task<(List<TrackingRecordBase>, int)> LoadRowsAsync(
        TrackingBoardType board, long sourceId, string? q, string? risk, int page, int pageSize, CancellationToken ct)
        => board switch
        {
            TrackingBoardType.SeaTransit => PageSet(_db.SeaTransitRecords, sourceId, q, risk, page, pageSize, ct),
            TrackingBoardType.SeaImport => PageSet(_db.SeaImportRecords, sourceId, q, risk, page, pageSize, ct),
            TrackingBoardType.SeaExport => PageSet(_db.SeaExportRecords, sourceId, q, risk, page, pageSize, ct),
            TrackingBoardType.RoadTransit => PageSet(_db.RoadTransitRecords, sourceId, q, risk, page, pageSize, ct),
            TrackingBoardType.RoadLoad => PageSet(_db.RoadLoadRecords, sourceId, q, risk, page, pageSize, ct),
            TrackingBoardType.RoadArchive => PageSet(_db.RoadArchiveRecords, sourceId, q, risk, page, pageSize, ct),
            TrackingBoardType.Alabora => PageSet(_db.AlaboraFinanceRecords, sourceId, q, risk, page, pageSize, ct),
            TrackingBoardType.Air => PageSet(_db.AirOperationRecords, sourceId, q, risk, page, pageSize, ct),
            TrackingBoardType.AirDaily => PageSet(_db.AirDailyRecords, sourceId, q, risk, page, pageSize, ct),
            _ => Task.FromResult((new List<TrackingRecordBase>(), 0)),
        };

    private static async Task<(int, Dictionary<string, int>)> CountSet<T>(
        DbSet<T> set, long sourceId, CancellationToken ct) where T : TrackingRecordBase
    {
        var q = set.Where(x => x.DataSourceId == sourceId);
        var total = await q.CountAsync(ct);
        var grouped = await q.GroupBy(x => x.RiskLevel)
            .Select(g => new { Level = g.Key, Count = g.Count() }).ToListAsync(ct);
        var risk = EmptyRisk();
        foreach (var g in grouped) risk[g.Level.ToString()] = g.Count;
        return (total, risk);
    }

    private static async Task<(List<TrackingRecordBase>, int)> PageSet<T>(
        DbSet<T> set, long sourceId, string? q, string? risk, int page, int pageSize, CancellationToken ct)
        where T : TrackingRecordBase
    {
        IQueryable<T> query = set.Where(x => x.DataSourceId == sourceId);

        if (!string.IsNullOrWhiteSpace(risk) && Enum.TryParse<RiskLevel>(risk, ignoreCase: true, out var lvl))
            query = query.Where(x => x.RiskLevel == lvl);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.SourceRowKey, $"%{s}%") ||
                (x.StatusText != null && EF.Functions.ILike(x.StatusText, $"%{s}%")));
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.RowIndex)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items.Cast<TrackingRecordBase>().ToList(), total);
    }

    // ───────────── Projeksiyon ─────────────

    private static BoardRowDto ToRowDto(TrackingRecordBase e, BoardMeta meta)
    {
        var type = e.GetType();
        var cells = new Dictionary<string, string?>(meta.Columns.Count);
        foreach (var c in meta.Columns)
            cells[c.Key] = Format(type.GetProperty(c.Key)?.GetValue(e));

        return new BoardRowDto(
            e.Id, e.SourceRowKey, e.StatusText, e.RiskLevel.ToString(), e.DelayDays, e.IsArchived, cells);
    }

    private static string? Format(object? v) => v switch
    {
        null => null,
        DateOnly d => d.ToString("yyyy-MM-dd"),
        _ => v.ToString(),
    };

    private static Dictionary<string, int> EmptyRisk() => new()
    {
        ["Green"] = 0, ["Yellow"] = 0, ["Orange"] = 0, ["Red"] = 0, ["Black"] = 0,
    };
}

public sealed record BoardRowDto(
    long Id, string Ref, string? Status, string Risk, int DelayDays, bool Archived, Dictionary<string, string?> Cells);

public sealed record BoardSummaryDto(
    string Key, string Title, string Group, long? DataSourceId, DateTimeOffset? LastSyncAt,
    int RowCount, Dictionary<string, int> RiskCounts);

public sealed record BoardAttentionDto(
    string BoardKey, string BoardTitle, string Group, string Ref, string Risk, int DelayDays, string? Status);

public sealed record BoardSearchResultDto(
    string BoardKey, string BoardTitle, string Group, string Ref, string Risk, int DelayDays,
    string MatchedField, string MatchedValue);

public sealed record BoardDetailDto(
    string Key, string Title, string Group, long? DataSourceId,
    IReadOnlyList<BoardColumn> Columns, IReadOnlyList<BoardRowDto> Rows, int Total, DateTimeOffset? LastSyncAt);

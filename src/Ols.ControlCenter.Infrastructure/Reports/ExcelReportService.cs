using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Abstractions.Reports;
using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Domain.Entities.Tracking;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Infrastructure.Reports;

/// <summary>
/// Excel rapor üretimi — takip tablolarından (gerçek operasyon verisi) beslenir. Her sekme (board)
/// kendi çalışma sayfasına, kaynak kolonlarıyla birebir yazılır (<see cref="BoardCatalog"/> tek kaynaktır).
/// </summary>
public sealed class ExcelReportService : IReportService
{
    private const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IApplicationDbContext _db;
    private readonly ITrackingMetricsService _metrics;

    public ExcelReportService(IApplicationDbContext db, ITrackingMetricsService metrics)
    {
        _db = db;
        _metrics = metrics;
    }

    public async Task<ReportFile> BoardsExcelAsync(string? group, CancellationToken ct)
    {
        var boards = string.IsNullOrWhiteSpace(group)
            ? BoardCatalog.All
            : BoardCatalog.All.Where(b => string.Equals(b.Group, group, StringComparison.OrdinalIgnoreCase)).ToList();

        using var wb = new XLWorkbook();
        foreach (var meta in boards)
        {
            var entities = await _metrics.LoadBoardEntitiesAsync(meta.Board, ct);
            var ws = wb.Worksheets.Add(SheetName(meta.Key));
            WriteBoardSheet(ws, meta, entities);
        }
        if (wb.Worksheets.Count == 0)
            wb.Worksheets.Add("Veri"); // ClosedXML en az 1 sayfa ister (boş/bilinmeyen grup durumunda)

        var suffix = string.IsNullOrWhiteSpace(group) ? "tum-sekmeler" : group.ToLowerInvariant();
        return Save(wb, $"operasyonlar_{suffix}_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<ReportFile> AlertsExcelAsync(CancellationToken ct)
    {
        var alerts = await _db.Alerts.AsNoTracking()
            .Where(a => a.Status != AlertStatus.Resolved && a.Status != AlertStatus.Dismissed)
            .OrderByDescending(a => a.LastTriggeredAt)
            .Select(a => new
            {
                OpNo = a.Operation != null ? a.Operation.SourceOperationNo : null,
                Customer = a.Operation != null ? a.Operation.CustomerName : null,
                a.BoardTitle, a.Group, a.RecordRef,
                a.Type, a.RiskLevel, a.RuleCode, a.Description, a.Status, a.LastTriggeredAt,
            })
            .ToListAsync(ct);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Açık Uyarılar");
        string[] headers = { "Kaynak", "Grup", "Uyarı Tipi", "Risk", "Kural", "Açıklama", "Durum", "Son Tetik" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
        StyleHeader(ws.Row(1));

        var r = 2;
        foreach (var a in alerts)
        {
            var source = a.BoardTitle is not null ? $"{a.BoardTitle} · {a.RecordRef}" : a.OpNo ?? a.Customer ?? "";
            ws.Cell(r, 1).Value = source;
            ws.Cell(r, 2).Value = a.Group ?? "";
            ws.Cell(r, 3).Value = a.Type.ToString();
            ws.Cell(r, 4).Value = a.RiskLevel.ToString();
            ws.Cell(r, 5).Value = a.RuleCode;
            ws.Cell(r, 6).Value = a.Description;
            ws.Cell(r, 7).Value = a.Status.ToString();
            ws.Cell(r, 8).Value = a.LastTriggeredAt.DateTime;
            r++;
        }
        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);

        return Save(wb, $"acik_uyarilar_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx");
    }

    private static void WriteBoardSheet(IXLWorksheet ws, BoardMeta meta, IReadOnlyList<TrackingRecordBase> entities)
    {
        var headers = new List<string> { "Dosya / Ref No", "Risk", "Gecikme (gün)", "Durum", "Arşiv" };
        headers.AddRange(meta.Columns.Select(c => c.Label));
        for (int i = 0; i < headers.Count; i++) ws.Cell(1, i + 1).Value = headers[i];
        StyleHeader(ws.Row(1));

        var entityType = entities.Count > 0 ? entities[0].GetType() : null;
        var props = meta.Columns.Select(c => entityType?.GetProperty(c.Key)).ToArray();

        int r = 2;
        foreach (var e in entities)
        {
            ws.Cell(r, 1).Value = e.SourceRowKey;
            ws.Cell(r, 2).Value = e.RiskLevel.ToString();
            ws.Cell(r, 3).Value = e.DelayDays;
            ws.Cell(r, 4).Value = e.StatusText ?? "";
            ws.Cell(r, 5).Value = e.IsArchived ? "Evet" : "Hayır";

            for (int i = 0; i < props.Length; i++)
            {
                var value = props[i]?.GetValue(e);
                var cell = ws.Cell(r, 6 + i);
                if (value is DateOnly d) cell.Value = d.ToDateTime(TimeOnly.MinValue);
                else cell.Value = value?.ToString() ?? "";
            }
            r++;
        }
        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);
    }

    private static void StyleHeader(IXLRow row)
    {
        row.Style.Font.Bold = true;
        row.Style.Fill.BackgroundColor = XLColor.FromHtml("#0b1830");
        row.Style.Font.FontColor = XLColor.White;
    }

    /// <summary>Excel sayfa adları ≤31 karakter olmalı; board key'leri (kebab-case) zaten kısa ve tekildir.</summary>
    private static string SheetName(string key) => key.Length > 31 ? key[..31] : key;

    private static ReportFile Save(XLWorkbook wb, string fileName)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return new ReportFile(ms.ToArray(), fileName, Xlsx);
    }
}

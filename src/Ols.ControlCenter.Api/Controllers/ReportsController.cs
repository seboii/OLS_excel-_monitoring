using Microsoft.AspNetCore.Mvc;
using Ols.ControlCenter.Application.Abstractions.Reports;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    /// <summary>Takip tablolarını (sekmeleri) Excel olarak dışa aktarır. group boşsa tüm sekmeler (9 sayfa).</summary>
    [HttpGet("boards/excel")]
    public async Task<IActionResult> BoardsExcel([FromQuery] string? group, CancellationToken ct)
    {
        var report = await _reports.BoardsExcelAsync(group, ct);
        return File(report.Content, report.ContentType, report.FileName);
    }

    [HttpGet("alerts/excel")]
    public async Task<IActionResult> AlertsExcel(CancellationToken ct)
    {
        var report = await _reports.AlertsExcelAsync(ct);
        return File(report.Content, report.ContentType, report.FileName);
    }
}

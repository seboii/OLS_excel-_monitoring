namespace Ols.ControlCenter.Application.Abstractions.Reports;

public sealed record ReportFile(byte[] Content, string FileName, string ContentType);

/// <summary>Excel rapor üretimi — takip tablolarından (gerçek operasyon verisi) beslenir.</summary>
public interface IReportService
{
    /// <summary>
    /// Bir grubun (Deniz/Kara/Hava/Finans) veya (null ise) tüm grupların sekmelerini, sekme-başına
    /// bir Excel çalışma sayfası olarak dışa aktarır.
    /// </summary>
    Task<ReportFile> BoardsExcelAsync(string? group, CancellationToken ct);

    /// <summary>Açık (çözülmemiş) tüm uyarıları — operasyon ve board kaynaklı — Excel olarak dışa aktarır.</summary>
    Task<ReportFile> AlertsExcelAsync(CancellationToken ct);
}

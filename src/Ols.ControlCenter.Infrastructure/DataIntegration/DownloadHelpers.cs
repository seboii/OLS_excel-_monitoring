using Ols.ControlCenter.Application.Abstractions.DataIntegration;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

internal static class DownloadHelpers
{
    public const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const long MaxBytes = 25_000_000; // 25 MB

    /// <summary>İndirilen baytların gerçekten bir xlsx (ZIP/PK imzası) olduğunu doğrular.</summary>
    public static void EnsureExcel(byte[]? bytes, string notExcelMessage)
    {
        if (bytes is null || bytes.Length == 0)
            throw new DataSourceException("Dosya indirilemedi.");
        if (bytes.Length > MaxBytes)
            throw new DataSourceException("Dosya boyutu limiti aşıldı (25 MB).");

        // .xlsx aslında bir ZIP'tir → 'PK' (0x50 0x4B) ile başlar
        var isZip = bytes.Length >= 2 && bytes[0] == 0x50 && bytes[1] == 0x4B;
        if (!isZip)
            throw new DataSourceException(notExcelMessage);
    }
}

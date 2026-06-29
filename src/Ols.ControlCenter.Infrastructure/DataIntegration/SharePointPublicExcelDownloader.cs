using Ols.ControlCenter.Application.Abstractions.DataIntegration;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>
/// SharePoint public Excel indirici. İlk aşamada Graph istemez: linke download=1 eklenip indirilir.
/// HTML/login/403 gelirse Microsoft Graph yetkisi gerektiğini belirten Türkçe hata fırlatır.
/// </summary>
public sealed class SharePointPublicExcelDownloader : ISharePointPublicExcelDownloader
{
    private const string GraphMessage =
        "Bu SharePoint linki public dosya olarak indirilemedi. Microsoft Graph yetkisi gerekebilir.";

    private readonly HttpClient _http;

    public SharePointPublicExcelDownloader(HttpClient http) => _http = http;

    public async Task<DownloadedFile> DownloadAsync(string publicUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
            throw new DataSourceException("Link boş olamaz.");

        var trimmed = publicUrl.Trim();
        var url = trimmed.Contains('?') ? $"{trimmed}&download=1" : $"{trimmed}?download=1";

        byte[] bytes;
        try
        {
            using var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
                throw new DataSourceException(GraphMessage);
            bytes = await resp.Content.ReadAsByteArrayAsync(ct);
        }
        catch (DataSourceException) { throw; }
        catch { throw new DataSourceException("Dosya indirilemedi."); }

        DownloadHelpers.EnsureExcel(bytes, GraphMessage);
        return new DownloadedFile(bytes, "sharepoint.xlsx", DownloadHelpers.Xlsx);
    }
}

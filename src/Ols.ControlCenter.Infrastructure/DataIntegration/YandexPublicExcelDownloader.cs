using System.Text.Json;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>
/// Yandex Disk public Excel indirici. OAuth istemez:
/// cloud-api download endpoint'inden gerçek href alınır, dosya indirilir ve xlsx doğrulanır.
/// </summary>
public sealed class YandexPublicExcelDownloader : IYandexPublicExcelDownloader
{
    private readonly HttpClient _http;

    public YandexPublicExcelDownloader(HttpClient http) => _http = http;

    public async Task<DownloadedFile> DownloadAsync(string publicUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
            throw new DataSourceException("Link boş olamaz.");

        var api = "https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key="
                  + Uri.EscapeDataString(publicUrl.Trim());

        string? href;
        try
        {
            using var resp = await _http.GetAsync(api, ct);
            if (!resp.IsSuccessStatusCode)
                throw new DataSourceException("Yandex indirme linki alınamadı.");
            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            href = doc.RootElement.TryGetProperty("href", out var h) ? h.GetString() : null;
        }
        catch (DataSourceException) { throw; }
        catch { throw new DataSourceException("Yandex indirme linki alınamadı."); }

        if (string.IsNullOrWhiteSpace(href))
            throw new DataSourceException("Yandex indirme linki alınamadı.");

        byte[] bytes;
        try { bytes = await _http.GetByteArrayAsync(href, ct); }
        catch { throw new DataSourceException("Dosya indirilemedi."); }

        DownloadHelpers.EnsureExcel(bytes, "İndirilen içerik Excel dosyası değil.");
        return new DownloadedFile(bytes, "yandex.xlsx", DownloadHelpers.Xlsx);
    }
}

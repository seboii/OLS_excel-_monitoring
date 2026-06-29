using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>Kaynak tipine göre doğru public indiriciyi seçer.</summary>
public sealed class DataSourceDownloader : IDataSourceDownloader
{
    private readonly IYandexPublicExcelDownloader _yandex;
    private readonly ISharePointPublicExcelDownloader _sharepoint;

    public DataSourceDownloader(IYandexPublicExcelDownloader yandex, ISharePointPublicExcelDownloader sharepoint)
    {
        _yandex = yandex;
        _sharepoint = sharepoint;
    }

    public Task<DownloadedFile> DownloadAsync(DataSource source, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(source.Url))
            throw new DataSourceException("Link boş olamaz.");

        return source.Type switch
        {
            DataSourceType.YandexDiskExcel => _yandex.DownloadAsync(source.Url, ct),
            DataSourceType.SharePointExcel => _sharepoint.DownloadAsync(source.Url, ct),
            _ => throw new DataSourceException(
                "Bu kaynak tipi otomatik indirmeyi desteklemiyor (yalnızca Yandex/SharePoint public veya manuel yükleme)."),
        };
    }
}

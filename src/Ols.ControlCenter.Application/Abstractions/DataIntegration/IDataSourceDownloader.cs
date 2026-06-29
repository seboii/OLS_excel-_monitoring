using Ols.ControlCenter.Domain.Entities;

namespace Ols.ControlCenter.Application.Abstractions.DataIntegration;

public sealed record DownloadedFile(byte[] Content, string FileName, string ContentType);

/// <summary>Kullanıcıya Türkçe gösterilen veri kaynağı hatası.</summary>
public sealed class DataSourceException : Exception
{
    public DataSourceException(string message) : base(message) { }
}

/// <summary>Veri kaynağı tipine göre doğru indiriciyi seçen yönlendirici.</summary>
public interface IDataSourceDownloader
{
    Task<DownloadedFile> DownloadAsync(DataSource source, CancellationToken ct);
}

public interface IYandexPublicExcelDownloader
{
    Task<DownloadedFile> DownloadAsync(string publicUrl, CancellationToken ct);
}

public interface ISharePointPublicExcelDownloader
{
    Task<DownloadedFile> DownloadAsync(string publicUrl, CancellationToken ct);
}

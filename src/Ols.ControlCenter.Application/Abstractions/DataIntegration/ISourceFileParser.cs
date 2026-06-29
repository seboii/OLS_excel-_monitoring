namespace Ols.ControlCenter.Application.Abstractions.DataIntegration;

/// <summary>Excel (.xlsx) veya CSV akışını "başlık → hücre" satır sözlüklerine çevirir.</summary>
public interface ISourceFileParser
{
    IReadOnlyList<IReadOnlyDictionary<string, string?>> Parse(
        Stream stream, string fileName, string? sheetName, int headerRowIndex);
}

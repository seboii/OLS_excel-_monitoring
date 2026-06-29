namespace Ols.ControlCenter.Application.Abstractions.DataIntegration;

public sealed record SheetColumn(int Index, string Name);

public sealed record SheetPreview(
    string SheetName,
    int HeaderRowIndex,
    IReadOnlyList<SheetColumn> Columns,
    IReadOnlyList<IReadOnlyDictionary<string, string?>> Rows,
    int TotalDataRows);

/// <summary>Excel (.xlsx) içeriğini sheet/kolon/satır olarak okur; header'ı otomatik tahmin eder.</summary>
public interface IExcelReaderService
{
    IReadOnlyList<string> GetSheetNames(byte[] content);

    /// <summary>maxRows kadar veri satırı döner (önizleme için 50, import için int.MaxValue).</summary>
    SheetPreview ReadSheet(byte[] content, string? sheetName, int? headerRowIndex, int maxRows);
}

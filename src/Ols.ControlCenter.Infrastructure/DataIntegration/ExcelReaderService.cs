using ClosedXML.Excel;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

public sealed class ExcelReaderService : IExcelReaderService
{
    public IReadOnlyList<string> GetSheetNames(byte[] content)
    {
        try
        {
            using var wb = Open(content);
            return wb.Worksheets.Select(w => w.Name).ToList();
        }
        catch
        {
            // ClosedXML açamadı (örn. Yandex web editörü çıktısı) → namespace-agnostik ham okuyucu
            return RawXlsxReader.GetSheetNames(content);
        }
    }

    public SheetPreview ReadSheet(byte[] content, string? sheetName, int? headerRowIndex, int maxRows)
    {
        try
        {
            return ReadSheetXl(content, sheetName, headerRowIndex, maxRows);
        }
        catch
        {
            // ClosedXML açamadı (standart-dışı OOXML) → ham OOXML okuyucuya düş
            return RawXlsxReader.ReadSheet(content, sheetName, headerRowIndex, maxRows);
        }
    }

    private SheetPreview ReadSheetXl(byte[] content, string? sheetName, int? headerRowIndex, int maxRows)
    {
        using var wb = Open(content);
        var ws = PickSheet(wb, sheetName);

        var used = ws.RangeUsed();
        if (used is null)
            return new SheetPreview(ws.Name, 1, Array.Empty<SheetColumn>(),
                Array.Empty<IReadOnlyDictionary<string, string?>>(), 0);

        int firstRow = used.FirstRow().RowNumber(), lastRow = used.LastRow().RowNumber();
        int firstCol = used.FirstColumn().ColumnNumber(), lastCol = used.LastColumn().ColumnNumber();

        int headerRow = headerRowIndex is > 0 ? headerRowIndex.Value
            : DetectHeaderRow(ws, firstRow, lastRow, firstCol, lastCol);
        if (headerRow < firstRow) headerRow = firstRow;

        // Kolonlar (boş başlık → Column{n}, tekrarlar → _2)
        var columns = new List<SheetColumn>();
        var colMap = new List<(int Col, string Name)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int pos = 0;
        for (int c = firstCol; c <= lastCol; c++)
        {
            var name = (GetCellText(ws.Cell(headerRow, c)) ?? string.Empty).Trim();
            if (name.Length == 0) name = $"Column{c - firstCol + 1}";
            var unique = name;
            int k = 2;
            while (!seen.Add(unique)) unique = $"{name}_{k++}";
            columns.Add(new SheetColumn(pos++, unique));
            colMap.Add((c, unique));
        }

        // Satırlar
        var rows = new List<IReadOnlyDictionary<string, string?>>();
        int dataCount = 0;
        for (int r = headerRow + 1; r <= lastRow; r++)
        {
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            bool any = false;
            foreach (var (col, name) in colMap)
            {
                var text = GetCellText(ws.Cell(r, col));
                dict[name] = text;
                if (text is not null) any = true;
            }
            if (!any) continue;
            dataCount++;
            if (rows.Count < maxRows) rows.Add(dict);
        }

        return new SheetPreview(ws.Name, headerRow, columns, rows, dataCount);
    }

    private static XLWorkbook Open(byte[] content)
    {
        try { return new XLWorkbook(new MemoryStream(content)); }
        catch { throw new DataSourceException("İndirilen içerik Excel dosyası değil veya açılamadı."); }
    }

    private static IXLWorksheet PickSheet(XLWorkbook wb, string? sheetName)
    {
        if (!string.IsNullOrWhiteSpace(sheetName) && wb.Worksheets.TryGetWorksheet(sheetName, out var ws))
            return ws;
        return wb.Worksheets.FirstOrDefault(w => w.RangeUsed() is not null) ?? wb.Worksheets.First();
    }

    private static int DetectHeaderRow(IXLWorksheet ws, int firstRow, int lastRow, int firstCol, int lastCol)
    {
        int best = firstRow, bestCount = -1;
        var limit = Math.Min(firstRow + 9, lastRow);
        for (int r = firstRow; r <= limit; r++)
        {
            int count = 0;
            for (int c = firstCol; c <= lastCol; c++)
                if (!string.IsNullOrWhiteSpace(ws.Cell(r, c).GetString())) count++;
            if (count > bestCount) { bestCount = count; best = r; }
        }
        return best;
    }

    private static string? GetCellText(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;
        if (cell.DataType == XLDataType.DateTime)
            return cell.GetDateTime().ToString("yyyy-MM-dd");
        var s = cell.GetString();
        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}

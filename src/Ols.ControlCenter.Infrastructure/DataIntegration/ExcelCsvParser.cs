using System.Text;
using ClosedXML.Excel;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>.xlsx (ClosedXML) ve .csv akışlarını "başlık → hücre" satır sözlüklerine çevirir.</summary>
public sealed class ExcelCsvParser : ISourceFileParser
{
    public IReadOnlyList<IReadOnlyDictionary<string, string?>> Parse(
        Stream stream, string fileName, string? sheetName, int headerRowIndex)
    {
        if (headerRowIndex <= 0) headerRowIndex = 1;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext == ".csv"
            ? ParseCsv(stream, headerRowIndex)
            : ParseXlsx(stream, sheetName, headerRowIndex);
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, string?>> ParseXlsx(
        Stream stream, string? sheetName, int headerRowIndex)
    {
        using var workbook = new XLWorkbook(stream);
        var ws = !string.IsNullOrWhiteSpace(sheetName) && workbook.Worksheets.Contains(sheetName)
            ? workbook.Worksheet(sheetName)
            : workbook.Worksheets.First();

        var used = ws.RangeUsed();
        if (used is null) return Array.Empty<IReadOnlyDictionary<string, string?>>();

        int firstCol = used.FirstColumn().ColumnNumber();
        int lastCol = used.LastColumn().ColumnNumber();
        int lastRow = used.LastRow().RowNumber();

        var headers = new Dictionary<int, string>();
        var headerRow = ws.Row(headerRowIndex);
        for (int c = firstCol; c <= lastCol; c++)
        {
            var name = headerRow.Cell(c).GetString().Trim();
            if (!string.IsNullOrEmpty(name)) headers[c] = name;
        }

        var rows = new List<IReadOnlyDictionary<string, string?>>();
        for (int r = headerRowIndex + 1; r <= lastRow; r++)
        {
            var row = ws.Row(r);
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            bool any = false;
            foreach (var (col, header) in headers)
            {
                var text = GetCellText(row.Cell(col));
                dict[header] = text;
                if (text is not null) any = true;
            }
            if (any) rows.Add(dict);
        }
        return rows;
    }

    private static string? GetCellText(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;
        if (cell.DataType == XLDataType.DateTime)
            return cell.GetDateTime().ToString("yyyy-MM-dd");
        var s = cell.GetString();
        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, string?>> ParseCsv(Stream stream, int headerRowIndex)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var lines = new List<string>();
        string? line;
        while ((line = reader.ReadLine()) is not null) lines.Add(line);

        if (lines.Count < headerRowIndex) return Array.Empty<IReadOnlyDictionary<string, string?>>();

        var headerLine = lines[headerRowIndex - 1];
        char delimiter = headerLine.Count(c => c == ';') > headerLine.Count(c => c == ',') ? ';' : ',';
        var headers = SplitCsv(headerLine, delimiter);

        var rows = new List<IReadOnlyDictionary<string, string?>>();
        for (int i = headerRowIndex; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cells = SplitCsv(lines[i], delimiter);
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < headers.Count; c++)
            {
                var key = headers[c].Trim();
                if (string.IsNullOrEmpty(key)) continue;
                var val = c < cells.Count ? cells[c].Trim() : null;
                dict[key] = string.IsNullOrWhiteSpace(val) ? null : val;
            }
            rows.Add(dict);
        }
        return rows;
    }

    private static List<string> SplitCsv(string line, char delimiter)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else inQuotes = !inQuotes;
            }
            else if (ch == delimiter && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else sb.Append(ch);
        }
        result.Add(sb.ToString());
        return result;
    }
}

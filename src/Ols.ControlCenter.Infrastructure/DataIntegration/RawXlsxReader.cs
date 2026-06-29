using System.IO.Compression;
using System.Xml.Linq;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>
/// Namespace-agnostik ham OOXML (.xlsx) okuyucu. ClosedXML'in açamadığı standart-dışı dosyalar için
/// fallback'tir — örn. Yandex web editöründen kaydedilen, prefix'li namespace (<c>s:</c>, <c>vyd:</c>)
/// kullanan dosyalar. Hücreler metin olarak döner; tarih serileri (örn. 46192) içe-aktarmada çözülür.
/// Başlık algılama + tekrar/boş başlık adlandırması <see cref="ExcelReaderService"/> ile birebir aynıdır.
/// </summary>
internal static class RawXlsxReader
{
    public static IReadOnlyList<string> GetSheetNames(byte[] content)
    {
        using var zip = OpenZip(content);
        return ReadSheetMap(zip).Select(s => s.Name).ToList();
    }

    public static SheetPreview ReadSheet(byte[] content, string? sheetName, int? headerRowIndex, int maxRows)
    {
        using var zip = OpenZip(content);
        var sheets = ReadSheetMap(zip);
        if (sheets.Count == 0)
            throw new DataSourceException("İndirilen içerik Excel dosyası değil veya açılamadı.");

        var sheet = (!string.IsNullOrWhiteSpace(sheetName)
                        ? sheets.FirstOrDefault(s => string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                        : null)
                    ?? sheets[0];

        var shared = ReadSharedStrings(zip);
        var grid = ReadCells(zip, sheet.Path, shared); // (row1Based -> (col0Based -> text))

        if (grid.Count == 0)
            return new SheetPreview(sheet.Name, 1, Array.Empty<SheetColumn>(),
                Array.Empty<IReadOnlyDictionary<string, string?>>(), 0);

        int firstRow = grid.Keys.Min(), lastRow = grid.Keys.Max();
        int firstCol = grid.Values.SelectMany(r => r.Keys).Min();
        int lastCol = grid.Values.SelectMany(r => r.Keys).Max();

        int headerRow = headerRowIndex is > 0 ? headerRowIndex.Value
            : DetectHeaderRow(grid, firstRow, lastRow, firstCol, lastCol);
        if (headerRow < firstRow) headerRow = firstRow;

        // Kolon adları (boş → Column{n}, tekrar → _2) — ExcelReaderService ile aynı kurallar.
        var columns = new List<SheetColumn>();
        var colMap = new List<(int Col, string Name)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int pos = 0;
        grid.TryGetValue(headerRow, out var headerCells);
        for (int c = firstCol; c <= lastCol; c++)
        {
            var name = (headerCells != null && headerCells.TryGetValue(c, out var h) ? h : null)?.Trim() ?? string.Empty;
            if (name.Length == 0) name = $"Column{c - firstCol + 1}";
            var unique = name;
            int k = 2;
            while (!seen.Add(unique)) unique = $"{name}_{k++}";
            columns.Add(new SheetColumn(pos++, unique));
            colMap.Add((c, unique));
        }

        var rows = new List<IReadOnlyDictionary<string, string?>>();
        int dataCount = 0;
        for (int r = headerRow + 1; r <= lastRow; r++)
        {
            if (!grid.TryGetValue(r, out var cells)) continue;
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            bool any = false;
            foreach (var (col, name) in colMap)
            {
                var text = cells.TryGetValue(col, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : null;
                dict[name] = text;
                if (text is not null) any = true;
            }
            if (!any) continue;
            dataCount++;
            if (rows.Count < maxRows) rows.Add(dict);
        }

        return new SheetPreview(sheet.Name, headerRow, columns, rows, dataCount);
    }

    // ───────────── İç yardımcılar ─────────────

    private static ZipArchive OpenZip(byte[] content)
    {
        try { return new ZipArchive(new MemoryStream(content), ZipArchiveMode.Read); }
        catch { throw new DataSourceException("İndirilen içerik Excel dosyası değil veya açılamadı."); }
    }

    private static XDocument? Load(ZipArchive zip, string path)
    {
        var entry = zip.GetEntry(path);
        if (entry is null) return null;
        using var s = entry.Open();
        return XDocument.Load(s);
    }

    private static IEnumerable<XElement> Kids(XElement? el, string localName)
        => el?.Elements().Where(e => e.Name.LocalName == localName) ?? Enumerable.Empty<XElement>();

    private static string? Attr(XElement el, string localName)
        => el.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value;

    private sealed record SheetRef(string Name, string Path);

    private static List<SheetRef> ReadSheetMap(ZipArchive zip)
    {
        var wb = Load(zip, "xl/workbook.xml");
        var rels = Load(zip, "xl/_rels/workbook.xml.rels");
        if (wb is null) return new List<SheetRef>();

        var relMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (rels is not null)
            foreach (var rel in rels.Root!.Elements().Where(e => e.Name.LocalName == "Relationship"))
            {
                var id = Attr(rel, "Id"); var target = Attr(rel, "Target");
                if (id is not null && target is not null) relMap[id] = target;
            }

        var result = new List<SheetRef>();
        var sheetsEl = Kids(wb.Root, "sheets").FirstOrDefault();
        foreach (var s in Kids(sheetsEl, "sheet"))
        {
            var name = Attr(s, "name") ?? "Sheet";
            var rid = Attr(s, "id"); // r:id (relationships namespace) → local name "id"
            string path;
            if (rid is not null && relMap.TryGetValue(rid, out var tgt))
                path = tgt.StartsWith("/") ? tgt.TrimStart('/') : (tgt.StartsWith("xl/") ? tgt : "xl/" + tgt);
            else
                path = $"xl/worksheets/sheet{result.Count + 1}.xml";
            result.Add(new SheetRef(name, path));
        }
        return result;
    }

    private static List<string> ReadSharedStrings(ZipArchive zip)
    {
        var doc = Load(zip, "xl/sharedStrings.xml");
        var list = new List<string>();
        if (doc is null) return list;
        foreach (var si in doc.Root!.Elements().Where(e => e.Name.LocalName == "si"))
            list.Add(string.Concat(si.Descendants().Where(d => d.Name.LocalName == "t").Select(t => t.Value)));
        return list;
    }

    private static Dictionary<int, Dictionary<int, string>> ReadCells(ZipArchive zip, string path, List<string> shared)
    {
        var doc = Load(zip, path) ?? throw new DataSourceException("İndirilen içerik Excel dosyası değil veya açılamadı.");
        var grid = new Dictionary<int, Dictionary<int, string>>();
        var sheetData = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "sheetData");
        if (sheetData is null) return grid;

        foreach (var row in sheetData.Elements().Where(e => e.Name.LocalName == "row"))
        {
            foreach (var c in row.Elements().Where(e => e.Name.LocalName == "c"))
            {
                var refAttr = Attr(c, "r");
                if (refAttr is null) continue;
                var (col, rowNum) = ParseRef(refAttr);
                if (col < 0) continue;

                var t = Attr(c, "t");
                string? val = null;
                if (t == "inlineStr")
                {
                    var isEl = c.Elements().FirstOrDefault(e => e.Name.LocalName == "is");
                    val = isEl is null ? null : string.Concat(isEl.Descendants().Where(d => d.Name.LocalName == "t").Select(x => x.Value));
                }
                else
                {
                    var v = c.Elements().FirstOrDefault(e => e.Name.LocalName == "v")?.Value;
                    if (v is not null)
                        val = t == "s" && int.TryParse(v, out var idx) && idx >= 0 && idx < shared.Count ? shared[idx] : v;
                }

                if (string.IsNullOrEmpty(val)) continue;
                if (!grid.TryGetValue(rowNum, out var cells)) grid[rowNum] = cells = new Dictionary<int, string>();
                cells[col] = val;
            }
        }
        return grid;
    }

    private static (int col, int row) ParseRef(string cellRef)
    {
        int i = 0, col = 0;
        while (i < cellRef.Length && char.IsLetter(cellRef[i]))
        {
            col = col * 26 + (char.ToUpperInvariant(cellRef[i]) - 'A' + 1);
            i++;
        }
        if (col == 0 || i >= cellRef.Length || !int.TryParse(cellRef.AsSpan(i), out var row)) return (-1, -1);
        return (col - 1, row);
    }

    private static int DetectHeaderRow(Dictionary<int, Dictionary<int, string>> grid, int firstRow, int lastRow, int firstCol, int lastCol)
    {
        int best = firstRow, bestCount = -1;
        var limit = Math.Min(firstRow + 9, lastRow);
        for (int r = firstRow; r <= limit; r++)
        {
            int count = grid.TryGetValue(r, out var cells)
                ? cells.Count(kv => kv.Key >= firstCol && kv.Key <= lastCol && !string.IsNullOrWhiteSpace(kv.Value))
                : 0;
            if (count > bestCount) { bestCount = count; best = r; }
        }
        return best;
    }
}

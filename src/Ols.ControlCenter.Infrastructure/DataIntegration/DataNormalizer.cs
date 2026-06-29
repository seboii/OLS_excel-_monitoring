using System.Globalization;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Infrastructure.DataIntegration;

/// <summary>Kaynak metin değerlerini standart tip/enum/tarih/para değerlerine normalize eder.</summary>
public static class DataNormalizer
{
    private static readonly string[] DateFormats =
    {
        "yyyy-MM-dd", "dd.MM.yyyy", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd",
        "d.M.yyyy", "dd-MM-yyyy", "yyyy.MM.dd", "dd.MM.yy",
    };

    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    public static DateOnly? ParseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Trim();
        if (DateOnly.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;
        if (DateTime.TryParse(s, Tr, DateTimeStyles.None, out var dt)) return DateOnly.FromDateTime(dt);
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2)) return DateOnly.FromDateTime(dt2);
        return null;
    }

    public static DateTimeOffset? ParseDateTime(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Trim();
        const DateTimeStyles styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, styles, out var dt))
            return new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
        if (DateTime.TryParse(s, Tr, styles, out var dt2))
            return new DateTimeOffset(DateTime.SpecifyKind(dt2, DateTimeKind.Utc));
        var date = ParseDate(s);
        return date is { } d ? new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)) : null;
    }

    public static decimal? ParseDecimal(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var t = s.Trim().Replace(" ", "").Replace("€", "").Replace("$", "").Replace("₺", "").Replace("TL", "", StringComparison.OrdinalIgnoreCase);
        bool hasComma = t.Contains(','), hasDot = t.Contains('.');
        if (hasComma && hasDot) t = t.Replace(".", "").Replace(",", ".");   // tr: nokta binlik, virgül ondalık
        else if (hasComma) t = t.Replace(",", ".");
        return decimal.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    public static OperationStatus NormalizeStatus(string? s, IReadOnlyDictionary<string, OperationStatus> map)
    {
        if (string.IsNullOrWhiteSpace(s)) return OperationStatus.New;
        s = s.Trim();
        if (map.TryGetValue(s, out var mapped)) return mapped;
        return Enum.TryParse<OperationStatus>(s, ignoreCase: true, out var parsed) ? parsed : OperationStatus.New;
    }

    public static T ParseEnum<T>(string? s, T fallback) where T : struct, Enum
        => !string.IsNullOrWhiteSpace(s) && Enum.TryParse<T>(s.Trim(), ignoreCase: true, out var e) ? e : fallback;
}

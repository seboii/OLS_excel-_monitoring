using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Finance;

public sealed record FinanceCurrencyTotal(string Currency, int Count, decimal Total);

public sealed record FinanceSummaryDto(
    int TotalFiles,
    int Delivered,         // статус "выгружен" (boşaltılmış/teslim)
    int InTransit,         // durum dolu ama teslim değil (örn. "в пути", "сборка N")
    int Unknown,           // durum boş
    int PaymentReceived,   // ödeme tarihi VEYA gelen ödeme tutarı var
    int PaymentPending,
    int DocsComplete,      // fatura + taşıma belgesi + sözleşme/poruçeniye hepsi "есть"
    int DocsIncomplete,
    IReadOnlyList<FinanceCurrencyTotal> ByCurrency,
    DateTimeOffset? LastSyncAt);

public interface IFinanceSummaryService
{
    Task<FinanceSummaryDto> GetSummaryAsync(CancellationToken ct);
}

/// <summary>
/// Alabora (СЧЕТА-ПЛАТЕЖИ) tahsilat sayfasına özel finans metrikleri. Genel <c>TrackingMetricsService</c>
/// yalnızca risk/gecikme taşır; tutar/döviz/ödeme/belge alanları burada ayrıca okunur çünkü bunlar
/// diğer 8 board'da yok — Alabora'ya özgü bir finansal görünüm gerektiriyor.
/// </summary>
public sealed class FinanceSummaryService : IFinanceSummaryService
{
    private readonly IApplicationDbContext _db;

    public FinanceSummaryService(IApplicationDbContext db) => _db = db;

    public async Task<FinanceSummaryDto> GetSummaryAsync(CancellationToken ct)
    {
        var rows = await _db.AlaboraFinanceRecords.AsNoTracking().ToListAsync(ct);

        int delivered = rows.Count(r => ContainsOrdinal(r.CargoStatus, "ВЫГРУЖ"));
        int inTransit = rows.Count(r => !string.IsNullOrWhiteSpace(r.CargoStatus) && !ContainsOrdinal(r.CargoStatus, "ВЫГРУЖ"));
        int unknown = rows.Count - delivered - inTransit;

        int received = rows.Count(r => r.PaymentDate.HasValue || ParseAmount(r.IncomingPayments) is > 0);
        int pending = rows.Count - received;

        int docsComplete = rows.Count(r => IsYes(r.InvoiceMarked) && IsYes(r.TransportDocs) && IsYes(r.OrderContract));
        int docsIncomplete = rows.Count - docsComplete;

        var byCurrency = rows
            .Select(r => new { Currency = NormalizeCurrency(r.Currency), Value = ParseAmount(r.Amount) ?? ParseAmount(r.IncomingPayments) ?? 0m })
            .GroupBy(x => x.Currency)
            .Select(g => new FinanceCurrencyTotal(g.Key, g.Count(), Math.Round(g.Sum(x => x.Value), 2)))
            .OrderByDescending(x => x.Count)
            .ToList();

        var lastSync = await _db.DataSources
            .Where(d => d.TargetBoard == TrackingBoardType.Alabora)
            .Select(d => d.LastSyncAt)
            .FirstOrDefaultAsync(ct);

        return new FinanceSummaryDto(
            rows.Count, delivered, inTransit, unknown, received, pending, docsComplete, docsIncomplete, byCurrency, lastSync);
    }

    private static bool ContainsOrdinal(string? s, string token)
        => !string.IsNullOrEmpty(s) && s.Contains(token, StringComparison.OrdinalIgnoreCase);

    private static bool IsYes(string? s)
        => !string.IsNullOrWhiteSpace(s) && s.Trim().Equals("есть", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeCurrency(string? c)
    {
        if (string.IsNullOrWhiteSpace(c)) return "Belirsiz";
        var t = c.Trim().ToUpperInvariant();
        return t is "USD" or "EUR" or "RUB" or "TRY" ? t : "Belirsiz";
    }

    private static decimal? ParseAmount(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var t = s.Trim().Replace(" ", "");
        bool hasComma = t.Contains(','), hasDot = t.Contains('.');
        if (hasComma && hasDot) t = t.Replace(".", "").Replace(",", ".");
        else if (hasComma) t = t.Replace(",", ".");
        return decimal.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }
}

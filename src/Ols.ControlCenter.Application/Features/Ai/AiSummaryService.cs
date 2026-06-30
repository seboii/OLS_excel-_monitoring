using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Ai;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Domain.Enums;

[assembly: InternalsVisibleTo("Ols.ControlCenter.UnitTests")]

namespace Ols.ControlCenter.Application.Features.Ai;

public sealed record AiSummarySection(string Title, string Body);
public sealed record AiSummaryDto(IReadOnlyList<AiSummarySection> Sections, DateTimeOffset GeneratedAt, bool AiGenerated);

public interface IAiSummaryService
{
    Task<AiSummaryDto> GenerateAsync(CancellationToken ct);
}

/// <summary>
/// Yönetim özeti — 9 takip tablosundan (gerçek operasyon verisi) ve risk motorunun ürettiği
/// <c>Alert</c> kayıtlarından metrikleri toplar, ardından <see cref="IAiClient"/> (Claude) ile doğal
/// dil özeti üretir. AI anahtarı yoksa veya çağrı başarısızsa aynı metriklerden <b>kural-tabanlı</b>
/// özete zarifçe geri düşer (<see cref="AiSummaryDto.AiGenerated"/> hangisinin kullanıldığını bildirir).
/// Kesin karar vermez; öneri/özet sunar.
/// </summary>
public sealed class AiSummaryService : IAiSummaryService
{
    private readonly IApplicationDbContext _db;
    private readonly ITrackingMetricsService _metrics;
    private readonly IAiClient _ai;

    public AiSummaryService(IApplicationDbContext db, ITrackingMetricsService metrics, IAiClient ai)
    {
        _db = db;
        _metrics = metrics;
        _ai = ai;
    }

    private sealed record Facts(
        int Total, int Current, int Delayed, int Risky, int OpenAlerts,
        int PaymentRisk, int Demurrage, int DocAlerts, string? TopRiskGroup, int TopRiskGroupCount,
        IReadOnlyList<string> Actions);

    public async Task<AiSummaryDto> GenerateAsync(CancellationToken ct)
    {
        var facts = await ComputeFactsAsync(ct);

        if (_ai.IsConfigured)
        {
            var aiText = await _ai.GenerateAsync(SystemPrompt, BuildUserPrompt(facts), ct);
            var sections = ParseSections(aiText);
            if (sections.Count > 0)
                return new AiSummaryDto(sections, DateTimeOffset.UtcNow, AiGenerated: true);
        }

        return new AiSummaryDto(BuildRuleBasedSections(facts), DateTimeOffset.UtcNow, AiGenerated: false);
    }

    // ───────────── Metrik toplama ─────────────

    private async Task<Facts> ComputeFactsAsync(CancellationToken ct)
    {
        var rows = (await _metrics.LoadRowsAsync(ct))
            .Where(r => BoardCatalog.OperationalGroups.Contains(r.Group)).ToList();
        var current = rows.Where(r => !r.Archived && !TrackingPhase.IsCompleted(r.Status)).ToList();

        var delayed = current.Count(r => r.Delay > 0);
        var risky = current.Count(r => r.Risk >= RiskLevel.Orange);

        var openAlerts = await _db.Alerts.CountAsync(a => a.Status != AlertStatus.Resolved && a.Status != AlertStatus.Dismissed, ct);
        var paymentRisk = await _db.Alerts.CountAsync(a => a.Type == AlertType.PaymentRisk && a.Status != AlertStatus.Resolved && a.Status != AlertStatus.Dismissed, ct);
        var demurrage = await _db.Alerts.CountAsync(a => a.Type == AlertType.FreeTimeDemurrageRisk && a.Status != AlertStatus.Resolved && a.Status != AlertStatus.Dismissed, ct);
        var docAlerts = await _db.Alerts.CountAsync(a => a.Type == AlertType.MissingDocuments && a.Status != AlertStatus.Resolved && a.Status != AlertStatus.Dismissed, ct);

        var topRiskGroup = current
            .GroupBy(r => r.Group)
            .Select(g => new { Group = g.Key, Risky = g.Count(r => r.Risk >= RiskLevel.Orange) })
            .Where(g => g.Risky > 0)
            .OrderByDescending(g => g.Risky)
            .FirstOrDefault();

        var actions = new List<string>();
        if (delayed > 0) actions.Add($"Geciken {delayed} dosya için müşteri/acente bilgilendirmesi ve sorumlu takibi yapılmalı.");
        if (paymentRisk > 0) actions.Add($"{paymentRisk} dosyada tahsilat riski var; finans ekibi önceliklendirmeli.");
        if (docAlerts > 0) actions.Add($"{docAlerts} dosyada belge paketi eksik; kapanış öncesi tamamlanmalı.");
        if (topRiskGroup is not null) actions.Add($"{topRiskGroup.Group} grubunda risk yoğunlaşıyor ({topRiskGroup.Risky} dosya); öncelik verilmeli.");
        if (actions.Count == 0) actions.Add("Acil aksiyon gerektiren kritik bir durum görünmüyor; rutin takip yeterli.");

        return new Facts(rows.Count, current.Count, delayed, risky, openAlerts, paymentRisk, demurrage, docAlerts,
            topRiskGroup?.Group, topRiskGroup?.Risky ?? 0, actions);
    }

    // ───────────── AI istemi ─────────────

    private const string SystemPrompt =
        "Sen OLS Dış Ticaret firmasının Operasyon Kontrol Merkezi'nde çalışan deneyimli bir operasyon " +
        "direktörüsün. Görevin, sana verilen GÜNCEL operasyon metriklerine dayanarak yöneticiye kısa, net ve " +
        "aksiyon-odaklı bir Türkçe özet sunmak. Kurallar: (1) Yalnızca verilen sayılara dayan, veri uydurma. " +
        "(2) Kesin karar verme; öncelik ve öneri sun. (3) Kısa ve profesyonel yaz. " +
        "Çıktını TAM OLARAK şu formatta ver — her bölüm '## ' ile başlayan bir başlık satırı, ardından 1-3 cümlelik " +
        "gövde olsun, başka açıklama ekleme:\n" +
        "## Bugünkü Durum\n## Kritik Riskler\n## Finansal Risk\n## Önerilen Aksiyonlar";

    private static string BuildUserPrompt(Facts f)
    {
        var top = f.TopRiskGroup is null ? "yok" : $"{f.TopRiskGroup} ({f.TopRiskGroupCount} riskli dosya)";
        return
            $"Deniz/Kara/Hava operasyonel metrikleri (Finans hariç):\n" +
            $"- Toplam kayıt: {f.Total}\n" +
            $"- Güncel (açık) dosya: {f.Current}\n" +
            $"- Geciken dosya: {f.Delayed}\n" +
            $"- Riskli dosya (turuncu+): {f.Risky}\n" +
            $"- Açık uyarı (toplam): {f.OpenAlerts}\n" +
            $"- Tahsilat riski uyarısı: {f.PaymentRisk}\n" +
            $"- Demuraj riski uyarısı: {f.Demurrage}\n" +
            $"- Belge eksikliği uyarısı: {f.DocAlerts}\n" +
            $"- En riskli grup: {top}\n\n" +
            $"Sistemin kural-tabanlı aksiyon önerileri (referans):\n" +
            string.Join("\n", f.Actions.Select(a => $"- {a}"));
    }

    /// <summary>'## Başlık' bloklarını bölümlere ayırır. Başlık yoksa tüm metni tek bölüm yapar.</summary>
    internal static IReadOnlyList<AiSummarySection> ParseSections(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<AiSummarySection>();

        var sections = new List<AiSummarySection>();
        string? title = null;
        var body = new System.Text.StringBuilder();

        void Flush()
        {
            if (title is not null)
                sections.Add(new AiSummarySection(title, body.ToString().Trim()));
            body.Clear();
        }

        foreach (var rawLine in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.TrimEnd();
            if (line.StartsWith("## ", StringComparison.Ordinal) || line.StartsWith("##\t", StringComparison.Ordinal))
            {
                Flush();
                title = line[2..].Trim();
            }
            else if (title is not null)
            {
                if (body.Length > 0) body.Append('\n');
                body.Append(line);
            }
        }
        Flush();

        if (sections.Count == 0)
            sections.Add(new AiSummarySection("Yönetici Özeti", text.Trim()));

        return sections;
    }

    // ───────────── Kural-tabanlı yedek ─────────────

    private static IReadOnlyList<AiSummarySection> BuildRuleBasedSections(Facts f)
    {
        return new List<AiSummarySection>
        {
            new("Bugünkü Özet",
                $"Deniz/Kara/Hava genelinde {f.Total} kayıttan {f.Current} tanesi güncel (açık) dosya. " +
                $"{f.Delayed} gecikme, {f.Risky} riskli dosya var. Sistemde toplam {f.OpenAlerts} açık uyarı bulunuyor."),
            new("Kritik Riskler",
                f.Risky > 0
                    ? $"{f.Risky} dosya turuncu/kırmızı risk seviyesinde." +
                      (f.TopRiskGroup is not null ? $" En yoğun grup: {f.TopRiskGroup} ({f.TopRiskGroupCount} dosya)." : "")
                    : "Şu an kritik (turuncu/kırmızı) risk seviyesinde dosya yok."),
            new("Finansal Risk",
                f.PaymentRisk > 0
                    ? $"{f.PaymentRisk} dosyada tahsilat riski tespit edildi (Alabora tahsilat takibi dahil); bazıları 45 günü geçmiş olabilir."
                    : "Tahsilat kaynaklı acil finansal risk görünmüyor."),
            new("Operasyonel Tıkanma",
                f.Demurrage > 0 || f.DocAlerts > 0
                    ? $"{f.Demurrage} dosyada demuraj riski, {f.DocAlerts} dosyada belge eksikliği operasyonu tıkayabilir."
                    : "Belirgin operasyonel tıkanma sinyali yok."),
            new("Önerilen Aksiyonlar", string.Join("\n", f.Actions.Select(a => $"• {a}"))),
        };
    }
}

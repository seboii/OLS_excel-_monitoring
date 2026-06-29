using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Ai;

public sealed record AiSummarySection(string Title, string Body);
public sealed record AiSummaryDto(IReadOnlyList<AiSummarySection> Sections, DateTimeOffset GeneratedAt);

public interface IAiSummaryService
{
    Task<AiSummaryDto> GenerateAsync(CancellationToken ct);
}

/// <summary>
/// Kural tabanlı yönetim özeti — 9 takip tablosundan (gerçek operasyon verisi) ve risk motorunun
/// ürettiği <c>Alert</c> kayıtlarından (operasyon+board birleşik) üretilir. Kesin karar vermez,
/// öneri/özet sunar.
/// </summary>
public sealed class AiSummaryService : IAiSummaryService
{
    private readonly IApplicationDbContext _db;
    private readonly ITrackingMetricsService _metrics;

    public AiSummaryService(IApplicationDbContext db, ITrackingMetricsService metrics)
    {
        _db = db;
        _metrics = metrics;
    }

    public async Task<AiSummaryDto> GenerateAsync(CancellationToken ct)
    {
        var rows = (await _metrics.LoadRowsAsync(ct))
            .Where(r => BoardCatalog.OperationalGroups.Contains(r.Group)).ToList();
        var notArchived = rows.Where(r => !r.Archived).ToList();
        var current = notArchived.Where(r => !TrackingPhase.IsCompleted(r.Status)).ToList();

        var total = rows.Count;
        var currentCount = current.Count;
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
        if (delayed > 0) actions.Add($"• Geciken {delayed} dosya için müşteri/acente bilgilendirmesi ve sorumlu takibi yapılmalı.");
        if (paymentRisk > 0) actions.Add($"• {paymentRisk} dosyada tahsilat riski var; finans ekibi önceliklendirmeli.");
        if (docAlerts > 0) actions.Add($"• {docAlerts} dosyada belge paketi eksik; kapanış öncesi tamamlanmalı.");
        if (topRiskGroup is not null) actions.Add($"• {topRiskGroup.Group} grubunda risk yoğunlaşıyor ({topRiskGroup.Risky} dosya); öncelik verilmeli.");
        if (actions.Count == 0) actions.Add("• Acil aksiyon gerektiren kritik bir durum görünmüyor; rutin takip yeterli.");

        var sections = new List<AiSummarySection>
        {
            new("Bugünkü Özet",
                $"Deniz/Kara/Hava genelinde {total} kayıttan {currentCount} tanesi güncel (açık) dosya. " +
                $"{delayed} gecikme, {risky} riskli dosya var. Sistemde toplam {openAlerts} açık uyarı bulunuyor."),
            new("Kritik Riskler",
                risky > 0
                    ? $"{risky} dosya turuncu/kırmızı risk seviyesinde." +
                      (topRiskGroup is not null ? $" En yoğun grup: {topRiskGroup.Group} ({topRiskGroup.Risky} dosya)." : "")
                    : "Şu an kritik (turuncu/kırmızı) risk seviyesinde dosya yok."),
            new("Finansal Risk",
                paymentRisk > 0
                    ? $"{paymentRisk} dosyada tahsilat riski tespit edildi (Alabora tahsilat takibi dahil); bazıları 45 günü geçmiş olabilir."
                    : "Tahsilat kaynaklı acil finansal risk görünmüyor."),
            new("Operasyonel Tıkanma",
                demurrage > 0 || docAlerts > 0
                    ? $"{demurrage} dosyada demuraj riski, {docAlerts} dosyada belge eksikliği operasyonu tıkayabilir."
                    : "Belirgin operasyonel tıkanma sinyali yok."),
            new("Önerilen Aksiyonlar", string.Join("\n", actions)),
        };

        return new AiSummaryDto(sections, DateTimeOffset.UtcNow);
    }
}

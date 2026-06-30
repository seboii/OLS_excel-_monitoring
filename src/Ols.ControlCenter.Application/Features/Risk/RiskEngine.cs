using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Abstractions.Risk;
using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Application.Features.Notifications;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Risk;

public interface IRiskEngine
{
    /// <summary>
    /// Tüm aktif operasyonlara VE takip tablosu (board) satırlarına kuralları uygular;
    /// alert'leri upsert eder ve risk seviyesini günceller.
    /// </summary>
    Task<int> EvaluateAllAsync(CancellationToken ct);
}

/// <summary>
/// İki geçişli risk motoru: (A) eski <c>Operation</c> modeli için 7 yapılandırılabilir kural
/// (<see cref="IRiskRule"/>) — demo veri; (B) 9 takip tablosu (board) satırı için gecikme/risk-anahtar-kelime
/// + Alabora'ya özgü tahsilat/belge kuralları — gerçek operasyon verisi. İkisi de aynı <see cref="Alert"/>
/// tablosuna, aynı dedupe/upsert/otomatik-çözme mantığıyla yazar (bkz. <see cref="Alert.BoardKey"/>).
/// </summary>
public sealed class RiskEngine : IRiskEngine
{
    private static readonly IReadOnlyDictionary<string, string> EmptyParams = new Dictionary<string, string>();

    private readonly IApplicationDbContext _db;
    private readonly IReadOnlyList<IRiskRule> _rules;
    private readonly ITrackingMetricsService _metrics;
    private readonly IRiskThresholdService _thresholds;
    private readonly INotificationService _notifications;

    /// <summary>Bu değerlendirme turunda <b>ilk kez</b> oluşan uyarılar — tur sonunda digest bildirimi üretir.</summary>
    private readonly List<(RiskLevel Level, string Description)> _newAlerts = new();

    public RiskEngine(IApplicationDbContext db, IEnumerable<IRiskRule> rules, ITrackingMetricsService metrics,
        IRiskThresholdService thresholds, INotificationService notifications)
    {
        _db = db;
        _rules = rules.ToList();
        _metrics = metrics;
        _thresholds = thresholds;
        _notifications = notifications;
    }

    public async Task<int> EvaluateAllAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        _newAlerts.Clear();

        int triggeredCount = await EvaluateOperationsAsync(now, today, ct);
        triggeredCount += await EvaluateBoardsAsync(now, today, ct);

        await PublishDigestAsync(ct);

        await _db.SaveChangesAsync(ct);
        return triggeredCount;
    }

    /// <summary>
    /// Tur boyunca ilk kez oluşan turuncu+ uyarılar varsa, tüm aktif kullanıcılara <b>tek</b> bir özet
    /// bildirim ekler (satır-başına değil — sel önlenir). SaveChanges motorun tur sonundaki çağrısıyla yapılır.
    /// </summary>
    private async Task PublishDigestAsync(CancellationToken ct)
    {
        var relevant = _newAlerts.Where(a => a.Level >= RiskLevel.Orange).ToList();
        if (relevant.Count == 0) return;

        var critical = relevant.Count(a => a.Level >= RiskLevel.Red);
        var warning = relevant.Count - critical;

        var title = critical > 0
            ? $"{critical} kritik uyarı oluştu"
            : $"{warning} yeni risk uyarısı oluştu";

        var parts = new List<string>();
        if (critical > 0) parts.Add($"{critical} kritik (kırmızı/siyah)");
        if (warning > 0) parts.Add($"{warning} turuncu");
        var header = $"Risk taraması: {string.Join(", ", parts)} uyarı tespit edildi.";

        var samples = relevant
            .OrderByDescending(a => a.Level)
            .Take(5)
            .Select(a => $"• {a.Description}");
        var body = header + "\n" + string.Join("\n", samples);

        var level = critical > 0 ? NotificationLevel.Critical : NotificationLevel.Warning;
        await _notifications.EnqueueForAllActiveUsersAsync(
            NotificationType.CriticalOperation, level, title, body, "Alert", null, ct);
    }

    // ───────────── Pass A: eski Operation modeli (demo veri) ─────────────

    private async Task<int> EvaluateOperationsAsync(DateTimeOffset now, DateOnly today, CancellationToken ct)
    {
        var ruleConfigs = await _db.RiskRules.ToDictionaryAsync(r => r.Code, ct);

        var ops = await _db.Operations
            .Include(o => o.Detail)
            .Include(o => o.Customer)
            .Where(o => o.Status != OperationStatus.Completed && o.Status != OperationStatus.Cancelled)
            .ToListAsync(ct);

        var opIds = ops.Select(o => o.Id).ToList();
        var alerts = await _db.Alerts.Where(a => a.OperationId != null && opIds.Contains(a.OperationId.Value)).ToListAsync(ct);
        var alertByKey = alerts.ToDictionary(a => a.DedupeKey);

        int triggeredCount = 0;

        foreach (var op in ops)
        {
            var baseContext = new RiskContext(op, op.Detail, op.Customer?.IsCritical ?? false, today, now, EmptyParams);
            var maxLevel = RiskLevel.Green;

            foreach (var rule in _rules)
            {
                ruleConfigs.TryGetValue(rule.Code, out var cfg);
                if (cfg is { IsActive: false }) continue;

                var ctx = baseContext with { Params = cfg?.Parameters ?? EmptyParams };
                var result = rule.Evaluate(ctx);
                var key = $"{op.Id}:{rule.Code}";

                if (!result.Triggered)
                {
                    if (alertByKey.TryGetValue(key, out var open) && open.Status != AlertStatus.Resolved && open.Status != AlertStatus.Dismissed)
                    {
                        open.Status = AlertStatus.Resolved;
                        open.ResolvedAt = now;
                        open.ResolutionNote = "Koşul ortadan kalktı (otomatik).";
                    }
                    continue;
                }

                triggeredCount++;
                if (result.Level > maxLevel) maxLevel = result.Level;

                if (alertByKey.TryGetValue(key, out var existing))
                {
                    existing.Type = result.Type;
                    existing.RiskLevel = result.Level;
                    existing.Description = result.Message;
                    existing.Deadline = result.Deadline;
                    existing.LastTriggeredAt = now;
                    existing.TriggerCount++;
                    if (existing.Status is AlertStatus.Resolved or AlertStatus.Dismissed)
                    {
                        existing.Status = AlertStatus.Open;
                        existing.ResolvedAt = null;
                        existing.ResolutionNote = null;
                    }
                }
                else
                {
                    var created = new Alert
                    {
                        OperationId = op.Id,
                        Type = result.Type,
                        RiskLevel = result.Level,
                        RuleCode = rule.Code,
                        DedupeKey = key,
                        Description = result.Message,
                        Status = AlertStatus.Open,
                        ResponsibleUserId = op.ResponsibleUserId,
                        Deadline = result.Deadline,
                        FirstTriggeredAt = now,
                        LastTriggeredAt = now,
                        TriggerCount = 1,
                        CreatedAt = now,
                    };
                    _db.Alerts.Add(created);
                    alertByKey[key] = created;
                    _newAlerts.Add((result.Level, result.Message));
                }
            }

            op.RiskLevel = maxLevel;
            op.UpdatedAt = now;
        }

        return triggeredCount;
    }

    // ───────────── Pass B: takip tabloları (gerçek operasyon verisi) ─────────────

    private async Task<int> EvaluateBoardsAsync(DateTimeOffset now, DateOnly today, CancellationToken ct)
    {
        var existing = await _db.Alerts.Where(a => a.BoardKey != null).ToListAsync(ct);
        var byKey = existing.ToDictionary(a => a.DedupeKey);
        var stillTriggered = new HashSet<string>();
        int triggeredCount = 0;

        void Upsert(string boardKey, string boardTitle, string group, string recordRef, string ruleCode, AlertType type, RiskLevel level, string message)
        {
            var key = $"board:{boardKey}:{recordRef}:{ruleCode}";
            stillTriggered.Add(key);
            triggeredCount++;

            if (byKey.TryGetValue(key, out var alert))
            {
                alert.Type = type;
                alert.RiskLevel = level;
                alert.Description = message;
                alert.LastTriggeredAt = now;
                alert.TriggerCount++;
                if (alert.Status is AlertStatus.Resolved or AlertStatus.Dismissed)
                {
                    alert.Status = AlertStatus.Open;
                    alert.ResolvedAt = null;
                    alert.ResolutionNote = null;
                }
            }
            else
            {
                alert = new Alert
                {
                    BoardKey = boardKey,
                    BoardTitle = boardTitle,
                    Group = group,
                    RecordRef = recordRef,
                    Type = type,
                    RiskLevel = level,
                    RuleCode = ruleCode,
                    DedupeKey = key,
                    Description = message,
                    Status = AlertStatus.Open,
                    FirstTriggeredAt = now,
                    LastTriggeredAt = now,
                    TriggerCount = 1,
                    CreatedAt = now,
                };
                _db.Alerts.Add(alert);
                byKey[key] = alert;
                _newAlerts.Add((level, message));
            }
        }

        // Genel kurallar (9 board, gecikme + not-anahtar-kelime riski) — dashboard'daki "Dikkat Listesi" ile aynı evren.
        var rows = await _metrics.LoadRowsAsync(ct);
        foreach (var row in rows.Where(r => !r.Archived))
        {
            if (row.Delay > 0)
                Upsert(row.BoardKey, row.BoardTitle, row.Group, row.Ref, "BOARD_DELAY", AlertType.Delay, row.Risk,
                    row.Status is { Length: > 0 } ? $"{row.Delay} gün gecikme — {row.Status}" : $"{row.Delay} gün gecikme.");
            else if (row.Risk >= RiskLevel.Yellow)
                Upsert(row.BoardKey, row.BoardTitle, row.Group, row.Ref, "BOARD_RISK_NOTE", AlertType.OperationalDeviation, row.Risk,
                    row.Status ?? "Not'ta risk ibaresi tespit edildi.");
        }

        // Alabora'ya özgü finans kuralları (tahsilat/belge) — generic metrik akışında olmayan alanlar gerektirir.
        var alaboraMeta = BoardCatalog.ForBoard(TrackingBoardType.Alabora);
        if (alaboraMeta is not null)
        {
            var thresholds = await _thresholds.GetAsync(ct);
            var finance = await _db.AlaboraFinanceRecords.AsNoTracking().Where(r => !r.IsArchived).ToListAsync(ct);
            foreach (var r in finance)
            {
                var pending = r.PaymentDate is null && string.IsNullOrWhiteSpace(r.IncomingPayments);
                if (pending && r.FtDate is { } ft)
                {
                    var ageDays = today.DayNumber - ft.DayNumber;
                    if (ageDays > thresholds.FinanceOverdueRedDays)
                        Upsert(alaboraMeta.Key, alaboraMeta.Title, alaboraMeta.Group, r.SourceRowKey, "FINANCE_PAYMENT_OVERDUE",
                            AlertType.PaymentRisk, RiskLevel.Red, $"{ageDays} gündür tahsilat yapılmadı (FT. Tarih: {ft:yyyy-MM-dd}).");
                    else if (ageDays > thresholds.FinanceOverdueOrangeDays)
                        Upsert(alaboraMeta.Key, alaboraMeta.Title, alaboraMeta.Group, r.SourceRowKey, "FINANCE_PAYMENT_OVERDUE",
                            AlertType.PaymentRisk, RiskLevel.Orange, $"{ageDays} gündür tahsilat yapılmadı (FT. Tarih: {ft:yyyy-MM-dd}).");
                }

                var delivered = !string.IsNullOrEmpty(r.CargoStatus) && r.CargoStatus.Contains("ВЫГРУЖ", StringComparison.OrdinalIgnoreCase);
                var docsComplete = IsYes(r.InvoiceMarked) && IsYes(r.TransportDocs) && IsYes(r.OrderContract);
                if (delivered && !docsComplete)
                    Upsert(alaboraMeta.Key, alaboraMeta.Title, alaboraMeta.Group, r.SourceRowKey, "FINANCE_DOCS_INCOMPLETE",
                        AlertType.MissingDocuments, RiskLevel.Yellow, "Yük boşaltıldı ama belge paketi (fatura/taşıma/sözleşme) tamamlanmadı.");
            }
        }

        foreach (var alert in existing)
        {
            if (!stillTriggered.Contains(alert.DedupeKey) && alert.Status is not (AlertStatus.Resolved or AlertStatus.Dismissed))
            {
                alert.Status = AlertStatus.Resolved;
                alert.ResolvedAt = now;
                alert.ResolutionNote = "Koşul ortadan kalktı (otomatik).";
            }
        }

        return triggeredCount;
    }

    private static bool IsYes(string? s)
        => !string.IsNullOrWhiteSpace(s) && s.Trim().Equals("есть", StringComparison.OrdinalIgnoreCase);
}

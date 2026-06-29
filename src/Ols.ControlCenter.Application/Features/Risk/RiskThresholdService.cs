using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;
using Ols.ControlCenter.Shared.Results;

namespace Ols.ControlCenter.Application.Features.Risk;

public sealed record RiskThresholdsDto(
    int DelayOrangeDays, int DelayRedDays,
    int FinanceOverdueOrangeDays, int FinanceOverdueRedDays);

public interface IRiskThresholdService
{
    Task<RiskThresholdsDto> GetAsync(CancellationToken ct);
    Task<Result> UpdateAsync(RiskThresholdsDto dto, CancellationToken ct);
}

/// <summary>
/// Board risk eşiklerini (gecikme bucket'ları, Alabora tahsilat gecikme eşiği) mevcut
/// <see cref="RiskRule"/> tablosu üzerinden (Code+Parameters jsonb) okur/yazar — yeni bir tablo
/// gerekmez. Satır yoksa kod-içi varsayılana düşülür (<see cref="DefaultDelayOrange"/> vb.), böylece
/// hiç ayarlanmamış olması da güvenli bir durumdur. Infrastructure'daki TrackingImportService (sync
/// sırasında satır risk seviyesi) ve <see cref="IRiskEngine"/> (Alabora tahsilat kuralı) buradan okur — UI'dan
/// değer değiştirmek kod değişikliği/redeploy gerektirmez; bir sonraki sync/risk değerlendirmesinde etkili olur.
/// </summary>
public sealed class RiskThresholdService : IRiskThresholdService
{
    public const string DelayBucketsCode = "BOARD_DELAY_BUCKETS";
    public const string FinanceOverdueCode = "FINANCE_PAYMENT_OVERDUE";

    private const int DefaultDelayOrange = 7, DefaultDelayRed = 15;
    private const int DefaultFinanceOrange = 21, DefaultFinanceRed = 45;

    private readonly IApplicationDbContext _db;

    public RiskThresholdService(IApplicationDbContext db) => _db = db;

    public async Task<RiskThresholdsDto> GetAsync(CancellationToken ct)
    {
        var rules = await _db.RiskRules
            .Where(r => r.Code == DelayBucketsCode || r.Code == FinanceOverdueCode)
            .ToDictionaryAsync(r => r.Code, ct);

        rules.TryGetValue(DelayBucketsCode, out var delayRule);
        rules.TryGetValue(FinanceOverdueCode, out var financeRule);

        return new RiskThresholdsDto(
            GetInt(delayRule, "orangeDays", DefaultDelayOrange),
            GetInt(delayRule, "redDays", DefaultDelayRed),
            GetInt(financeRule, "orangeDays", DefaultFinanceOrange),
            GetInt(financeRule, "redDays", DefaultFinanceRed));
    }

    public async Task<Result> UpdateAsync(RiskThresholdsDto dto, CancellationToken ct)
    {
        if (dto.DelayOrangeDays < 1 || dto.DelayRedDays <= dto.DelayOrangeDays)
            return Result.Failure(Error.Validation("Gecikme eşiklerinde Turuncu ≥ 1 gün ve Kırmızı, Turuncu'dan büyük olmalı."));
        if (dto.FinanceOverdueOrangeDays < 1 || dto.FinanceOverdueRedDays <= dto.FinanceOverdueOrangeDays)
            return Result.Failure(Error.Validation("Tahsilat eşiklerinde Turuncu ≥ 1 gün ve Kırmızı, Turuncu'dan büyük olmalı."));

        await UpsertAsync(DelayBucketsCode, "Gecikme risk eşikleri (gün)", AlertType.Delay,
            new Dictionary<string, string> { ["orangeDays"] = dto.DelayOrangeDays.ToString(), ["redDays"] = dto.DelayRedDays.ToString() }, ct);
        await UpsertAsync(FinanceOverdueCode, "Alabora tahsilat gecikme eşikleri (gün)", AlertType.PaymentRisk,
            new Dictionary<string, string> { ["orangeDays"] = dto.FinanceOverdueOrangeDays.ToString(), ["redDays"] = dto.FinanceOverdueRedDays.ToString() }, ct);

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task UpsertAsync(string code, string name, AlertType type, Dictionary<string, string> parameters, CancellationToken ct)
    {
        var rule = await _db.RiskRules.FirstOrDefaultAsync(r => r.Code == code, ct);
        if (rule is null)
        {
            rule = new RiskRule { Code = code, Name = name, AlertType = type, Severity = RiskLevel.Orange, IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
            _db.RiskRules.Add(rule);
        }
        rule.Parameters = parameters;
        rule.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static int GetInt(RiskRule? rule, string key, int fallback)
        => rule is not null && rule.Parameters.TryGetValue(key, out var v) && int.TryParse(v, out var n) ? n : fallback;
}

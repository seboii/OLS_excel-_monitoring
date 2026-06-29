using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Abstractions.Risk;

/// <summary>Bir kuralın değerlendirme bağlamı (operasyon + parametreler + referans zaman).</summary>
public sealed record RiskContext(
    Operation Operation,
    OperationDetail? Detail,
    bool CustomerCritical,
    DateOnly Today,
    DateTimeOffset Now,
    IReadOnlyDictionary<string, string> Params)
{
    public int IntParam(string key, int fallback)
        => Params.TryGetValue(key, out var v) && int.TryParse(v, out var i) ? i : fallback;
}

/// <summary>Bir kuralın sonucu.</summary>
public sealed record RiskRuleResult(bool Triggered, RiskLevel Level, AlertType Type, string Message, DateTimeOffset? Deadline = null)
{
    public static readonly RiskRuleResult None = new(false, RiskLevel.Green, AlertType.OperationalDeviation, string.Empty);

    public static RiskRuleResult Trigger(RiskLevel level, AlertType type, string message, DateTimeOffset? deadline = null)
        => new(true, level, type, message, deadline);
}

/// <summary>Tek bir risk kuralı. Her kural bağımsız ve test edilebilir.</summary>
public interface IRiskRule
{
    string Code { get; }
    RiskRuleResult Evaluate(RiskContext context);
}

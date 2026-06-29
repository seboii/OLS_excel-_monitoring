using Ols.ControlCenter.Application.Abstractions.Risk;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.Application.Features.Risk;

/// <summary>Kural 1: Planlanan teslim geçmiş ve teslim edilmemiş.</summary>
public sealed class DelayRule : IRiskRule
{
    public string Code => "DELAY";
    public RiskRuleResult Evaluate(RiskContext c)
    {
        var op = c.Operation;
        if (op.IsActiveOperation && op.PlannedDeliveryDate is { } pd && op.DeliveryDate is null && pd < c.Today)
            return RiskRuleResult.Trigger(RiskLevel.Red, AlertType.Delay, $"Teslim tarihi {op.DelayDays} gün geçti.");
        return RiskRuleResult.None;
    }
}

/// <summary>Kural 2: Teslimat yakın ama tahsilat alınmamış.</summary>
public sealed class PaymentRiskRule : IRiskRule
{
    public string Code => "PAYMENT_RISK";
    public RiskRuleResult Evaluate(RiskContext c)
    {
        var op = c.Operation;
        if (!op.IsActiveOperation) return RiskRuleResult.None;
        var days = c.IntParam("daysBeforeDelivery", 2);
        var pending = op.FinanceStatus is FinanceStatus.Pending or FinanceStatus.Overdue or FinanceStatus.PartiallyCollected;
        var soon = (op.PlannedDeliveryDate is { } pd && pd <= c.Today.AddDays(days))
                   || (op.Eta is { } eta && eta <= c.Now.AddDays(days));
        if (pending && soon)
            return RiskRuleResult.Trigger(RiskLevel.Red, AlertType.PaymentRisk, "Teslimat yaklaşıyor, tahsilat alınmamış.");
        return RiskRuleResult.None;
    }
}

/// <summary>Kural 3: Aktif operasyonda müşteri bilgilendirmesi N saati geçti.</summary>
public sealed class CustomerInfoRule : IRiskRule
{
    public string Code => "CUST_INFO_24H";
    public RiskRuleResult Evaluate(RiskContext c)
    {
        var op = c.Operation;
        if (!op.IsActiveOperation) return RiskRuleResult.None;
        var hours = c.IntParam("hours", 24);
        var last = op.LastCustomerUpdateDate ?? op.CreatedAt;
        if (c.Now - last > TimeSpan.FromHours(hours))
            return RiskRuleResult.Trigger(RiskLevel.Orange, AlertType.CustomerInfoGap, "Müşteriye güncel statü verilmemiş.");
        return RiskRuleResult.None;
    }
}

/// <summary>Kural 4: Varış/teslim yakın ve evrak eksik.</summary>
public sealed class DocumentMissingRule : IRiskRule
{
    public string Code => "DOC_MISSING";
    public RiskRuleResult Evaluate(RiskContext c)
    {
        var op = c.Operation;
        if (!op.IsActiveOperation) return RiskRuleResult.None;
        var days = c.IntParam("daysBeforeEta", 2);
        var missing = op.DocumentStatus == DocumentStatus.Missing || op.Status == OperationStatus.MissingDocuments;
        var arriving = (op.Eta is { } eta && eta <= c.Now.AddDays(days))
                       || (op.PlannedDeliveryDate is { } pd && pd <= c.Today.AddDays(days));
        if (missing && arriving)
            return RiskRuleResult.Trigger(RiskLevel.Red, AlertType.MissingDocuments, "Varış/teslim öncesi evrak eksik.");
        return RiskRuleResult.None;
    }
}

/// <summary>Kural 5: Deniz — free time bitti veya bitmek üzere (demuraj riski).</summary>
public sealed class SeaDemurrageRule : IRiskRule
{
    public string Code => "SEA_DEMURRAGE";
    public RiskRuleResult Evaluate(RiskContext c)
    {
        var op = c.Operation;
        if (op.TransportType != TransportType.Sea || !op.IsActiveOperation) return RiskRuleResult.None;
        var warn = c.IntParam("warnDays", 3);
        if (c.Detail?.FreeTimeEndDate is { } fte)
        {
            if (fte < c.Today)
                return RiskRuleResult.Trigger(RiskLevel.Red, AlertType.FreeTimeDemurrageRisk, "Free time bitti, demuraj başlamış olabilir.");
            if (fte <= c.Today.AddDays(warn))
                return RiskRuleResult.Trigger(RiskLevel.Orange, AlertType.FreeTimeDemurrageRisk, $"Demuraj riski yaklaşıyor (free time {fte:dd.MM}).");
        }
        return RiskRuleResult.None;
    }
}

/// <summary>Kural 6: Aktif operasyonda sonraki aksiyon tanımsız.</summary>
public sealed class NextActionMissingRule : IRiskRule
{
    public string Code => "NEXT_ACTION_MISSING";
    public RiskRuleResult Evaluate(RiskContext c)
    {
        var op = c.Operation;
        if (op.IsActiveOperation && op.NextActionDate is null && string.IsNullOrWhiteSpace(op.NextActionDescription))
            return RiskRuleResult.Trigger(RiskLevel.Yellow, AlertType.NextActionMissing, "Sonraki aksiyon tanımlanmamış.");
        return RiskRuleResult.None;
    }
}

/// <summary>Kural 7: Kritik müşteride gecikme.</summary>
public sealed class CriticalCustomerRule : IRiskRule
{
    public string Code => "CRITICAL_CUSTOMER";
    public RiskRuleResult Evaluate(RiskContext c)
    {
        var op = c.Operation;
        if (c.CustomerCritical && op.IsActiveOperation && op.DelayDays > 0)
            return RiskRuleResult.Trigger(RiskLevel.Red, AlertType.CriticalCustomer, "Kritik müşteride gecikme var.");
        return RiskRuleResult.None;
    }
}

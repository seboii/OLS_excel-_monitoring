namespace Ols.ControlCenter.Application.Features.Dashboard;

public sealed record NameValue(string Name, int Value);

/// <summary>
/// Dashboard üst-bant KPI'ları — operasyonel takip tablolarından (Deniz/Kara/Hava) toplanır.
/// "Güncel" = arşiv olmayan + durum metni teslim/tamamlanma içermeyen satır. "Geçmiş" = tamamlanan + arşiv.
/// </summary>
public sealed record DashboardKpis(
    int TotalRecords,   // operasyonel tüm satırlar (arşiv dahil)
    int Current,        // güncel iş (aktif & tamamlanmamış)
    int Completed,      // tamamlanan/teslim (arşiv olmayan)
    int Archived,       // arşiv satırları
    int Delayed,        // gecikme > 0 (güncel)
    int Risky,          // risk >= Orange (güncel)
    int Critical,       // risk >= Red (güncel)
    double AvgDelayDays,
    int Boards);        // operasyonel sekme sayısı

/// <summary>Dikkat gerektiren tek satır (riskli/geciken) — dashboard listesi ve TV modu için.</summary>
public sealed record AttentionItemDto(
    string BoardKey, string BoardTitle, string Group,
    string Ref, string Risk, int DelayDays, string? Status);

public sealed record DashboardSummaryDto(
    DashboardKpis Kpis,
    IReadOnlyList<NameValue> RiskDistribution,    // Green..Black (aktif)
    IReadOnlyList<NameValue> GroupDistribution,   // Deniz/Kara/Hava (aktif)
    IReadOnlyList<NameValue> BoardLoad,           // her sekme: aktif satır sayısı
    IReadOnlyList<AttentionItemDto> Attention);   // en riskli/geciken ilk N

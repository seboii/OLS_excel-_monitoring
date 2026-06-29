namespace Ols.ControlCenter.Application.Features.DataSources;

/// <summary>Bir senkronizasyon/içe-aktarma çalışmasının sonucu.</summary>
public sealed record SyncResult(
    int RowsRead,
    int RowsUpserted,
    int RowsFailed,
    IReadOnlyList<string> Errors)
{
    public static SyncResult Empty { get; } = new(0, 0, 0, Array.Empty<string>());
}

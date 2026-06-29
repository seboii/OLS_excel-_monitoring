namespace Ols.ControlCenter.Application.Abstractions.DataIntegration;

public sealed record MappingSuggestion(string SourceColumn, int SourceColumnIndex, string? SuggestedTargetField);

/// <summary>Kaynak kolon adlarından sistem alanı (otomatik) önerisi üretir.</summary>
public interface IColumnMappingService
{
    IReadOnlyList<MappingSuggestion> Suggest(IReadOnlyList<SheetColumn> columns);
}

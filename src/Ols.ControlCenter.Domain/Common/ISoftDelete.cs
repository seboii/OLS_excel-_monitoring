namespace Ols.ControlCenter.Domain.Common;

/// <summary>
/// Soft-delete işaretleyici. Bu arayüzü uygulayan entity'ler fiziksel olarak silinmez;
/// global sorgu filtresiyle gizlenir. Operasyon geçmişi korunur.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}

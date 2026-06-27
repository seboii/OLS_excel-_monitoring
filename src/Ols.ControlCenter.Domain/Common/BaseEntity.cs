namespace Ols.ControlCenter.Domain.Common;

/// <summary>Tüm entity'lerin ortak temel sınıfı. Anahtar bigint (long) kimliktir.</summary>
public abstract class BaseEntity
{
    public long Id { get; set; }
}

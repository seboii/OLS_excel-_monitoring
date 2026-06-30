namespace Ols.ControlCenter.Application.Abstractions.Ai;

/// <summary>
/// AI özet ayarları (<c>Ai</c> bölümü / <c>.env</c> <c>AI__*</c>). Anahtar yoksa AI devre dışı
/// sayılır ve servisler kural-tabanlı özete düşer.
/// </summary>
public sealed class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>AI özeti açık mı (anahtar dolu olsa bile kapatılabilir).</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Anthropic API anahtarı. Boşsa <c>ANTHROPIC_API_KEY</c> ortam değişkenine düşülür.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Kullanılacak model (varsayılan en yetenekli Claude).</summary>
    public string Model { get; set; } = "claude-opus-4-8";

    /// <summary>Yanıt için maksimum token.</summary>
    public int MaxTokens { get; set; } = 1200;
}

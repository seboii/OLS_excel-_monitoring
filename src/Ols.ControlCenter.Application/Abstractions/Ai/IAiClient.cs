namespace Ols.ControlCenter.Application.Abstractions.Ai;

/// <summary>
/// LLM (Claude) tamamlama soyutlaması. Application katmanı bu arayüz üzerinden çağırır;
/// somut Anthropic SDK implementasyonu Infrastructure'dadır. Anahtar yoksa veya çağrı
/// başarısız olursa servisler kural-tabanlı çıktıya zarifçe geri düşebilsin diye
/// <see cref="GenerateAsync"/> <c>null</c> dönebilir.
/// </summary>
public interface IAiClient
{
    /// <summary>AI çağrısı yapılabilir mi (anahtar tanımlı ve etkin mi).</summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Verilen sistem + kullanıcı istemiyle tek seferlik tamamlama üretir.
    /// Başarısızlık/anahtar yokluğunda <c>null</c> döner (istisna fırlatmaz).
    /// </summary>
    Task<string?> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct);
}

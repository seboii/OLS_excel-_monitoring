using System.Text;
using Anthropic;
using Anthropic.Core;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ols.ControlCenter.Application.Abstractions.Ai;

namespace Ols.ControlCenter.Infrastructure.Ai;

/// <summary>
/// <see cref="IAiClient"/>'in resmi Anthropic C# SDK (Claude Messages API) implementasyonu.
/// Anahtar yoksa devre dışıdır; çağrı hatalarını yutar ve <c>null</c> döner ki çağıran servis
/// kural-tabanlı çıktıya geri düşebilsin. Tek seferlik tamamlama (thinking varsayılan/kapalı) —
/// yönetici özeti için yeterli ve düşük gecikmeli.
/// </summary>
public sealed class AnthropicAiClient : IAiClient, IDisposable
{
    private readonly AiOptions _options;
    private readonly ILogger<AnthropicAiClient> _logger;
    private readonly AnthropicClient? _client;

    public AnthropicAiClient(IOptions<AiOptions> options, ILogger<AnthropicAiClient> logger)
    {
        _options = options.Value;
        _logger = logger;

        var apiKey = string.IsNullOrWhiteSpace(_options.ApiKey)
            ? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
            : _options.ApiKey;

        IsConfigured = _options.Enabled && !string.IsNullOrWhiteSpace(apiKey);
        if (IsConfigured)
            _client = new AnthropicClient(new ClientOptions { ApiKey = apiKey });
    }

    public bool IsConfigured { get; }

    public async Task<string?> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        if (_client is null) return null;

        try
        {
            var message = await _client.Messages.Create(new MessageCreateParams
            {
                Model = _options.Model,
                MaxTokens = _options.MaxTokens,
                System = systemPrompt,
                Messages = new List<MessageParam>
                {
                    new() { Role = Role.User, Content = userPrompt },
                },
            }, ct);

            var sb = new StringBuilder();
            foreach (var block in message.Content)
                if (block.TryPickText(out var text))
                    sb.Append(text.Text);

            var result = sb.ToString().Trim();
            return result.Length == 0 ? null : result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Claude AI özet çağrısı başarısız; kural-tabanlı özete düşülüyor.");
            return null;
        }
    }

    public void Dispose() => _client?.Dispose();
}

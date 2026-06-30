using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ols.ControlCenter.Application.Abstractions.Ai;
using Ols.ControlCenter.Application.Features.Ai;
using Ols.ControlCenter.Infrastructure.Ai;

namespace Ols.ControlCenter.UnitTests;

/// <summary>
/// AI özet katmanı: Claude çıktısının '## Başlık' ayrıştırması ve anahtar yokken kural-tabanlıya
/// güvenli geri düşüş (IsConfigured=false → null).
/// </summary>
public class AiSummaryTests
{
    [Fact]
    public void ParseSections_SplitsHeaderBlocks()
    {
        var text = "## Bugünkü Durum\nGenel tablo iyi.\n## Kritik Riskler\n3 dosya riskli.";
        var sections = AiSummaryService.ParseSections(text);

        Assert.Equal(2, sections.Count);
        Assert.Equal("Bugünkü Durum", sections[0].Title);
        Assert.Equal("Genel tablo iyi.", sections[0].Body);
        Assert.Equal("Kritik Riskler", sections[1].Title);
        Assert.Equal("3 dosya riskli.", sections[1].Body);
    }

    [Fact]
    public void ParseSections_NoHeaders_WrapsAsSingleSection()
    {
        var sections = AiSummaryService.ParseSections("Sadece düz bir metin.");
        Assert.Single(sections);
        Assert.Equal("Yönetici Özeti", sections[0].Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseSections_Empty_ReturnsEmpty(string? text)
    {
        Assert.Empty(AiSummaryService.ParseSections(text));
    }

    [Fact]
    public async Task AnthropicClient_Disabled_IsNotConfigured_ReturnsNull()
    {
        var options = Options.Create(new AiOptions { Enabled = false, ApiKey = "dummy-key" });
        var client = new AnthropicAiClient(options, NullLogger<AnthropicAiClient>.Instance);

        Assert.False(client.IsConfigured);
        Assert.Null(await client.GenerateAsync("sistem", "kullanıcı", default));
    }
}

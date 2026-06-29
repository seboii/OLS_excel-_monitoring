using Ols.ControlCenter.Domain.Enums;
using Ols.ControlCenter.Infrastructure.DataIntegration;

namespace Ols.ControlCenter.UnitTests;

/// <summary>
/// Kaynak Excel hücrelerini standart tiplere çeviren <see cref="DataNormalizer"/> için testler.
/// Bu sınıf hatalı/sessiz veri kaybına en açık yer olduğundan (tarih/sayı formatları, takip
/// tablolarının senkron sırasındaki risk hesabı buradan beslenir) kapsamlı test edilir.
/// </summary>
public class DataNormalizerTests
{
    [Theory]
    [InlineData("2026-06-27", 2026, 6, 27)]
    [InlineData("27.06.2026", 2026, 6, 27)]
    [InlineData("27/06/2026", 2026, 6, 27)]
    [InlineData("06/27/2026", 2026, 6, 27)]
    public void ParseDate_KnownFormats_ParsesCorrectly(string input, int year, int month, int day)
    {
        var result = DataNormalizer.ParseDate(input);

        Assert.Equal(new DateOnly(year, month, day), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("bilinmeyen tarih")]
    public void ParseDate_InvalidOrEmpty_ReturnsNull(string? input)
    {
        Assert.Null(DataNormalizer.ParseDate(input));
    }

    [Theory]
    [InlineData("1234.56", 1234.56)]
    [InlineData("1.234,56", 1234.56)]   // tr: nokta binlik, virgül ondalık
    [InlineData("1234,56", 1234.56)]
    [InlineData("€1.234,56", 1234.56)]
    [InlineData("5.200,00 €", 5200.00)]
    [InlineData("100", 100)]
    public void ParseDecimal_VariousFormats_ParsesCorrectly(string input, double expected)
    {
        var result = DataNormalizer.ParseDecimal(input);

        Assert.NotNull(result);
        Assert.Equal((decimal)expected, result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sayı değil")]
    public void ParseDecimal_InvalidOrEmpty_ReturnsNull(string? input)
    {
        Assert.Null(DataNormalizer.ParseDecimal(input));
    }

    [Fact]
    public void ParseEnum_ValidValue_IgnoresCase()
    {
        var result = DataNormalizer.ParseEnum("sea", TransportType.Other);

        Assert.Equal(TransportType.Sea, result);
    }

    [Fact]
    public void ParseEnum_InvalidValue_ReturnsFallback()
    {
        var result = DataNormalizer.ParseEnum("gecersiz-deger", TransportType.Other);

        Assert.Equal(TransportType.Other, result);
    }

    [Fact]
    public void ParseEnum_NullOrEmpty_ReturnsFallback()
    {
        Assert.Equal(TransportType.Road, DataNormalizer.ParseEnum(null, TransportType.Road));
        Assert.Equal(TransportType.Road, DataNormalizer.ParseEnum("", TransportType.Road));
    }
}

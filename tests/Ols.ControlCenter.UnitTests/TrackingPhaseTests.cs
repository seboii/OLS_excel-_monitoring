using Ols.ControlCenter.Application.Features.Boards;

namespace Ols.ControlCenter.UnitTests;

/// <summary>
/// <see cref="TrackingPhase.IsCompleted"/> dashboard'un Güncel/Tamamlanan ayrımını besler;
/// kaynak durum notları Türkçe ve Rusça karışık olduğundan (Alabora) her ikisi de test edilir.
/// </summary>
public class TrackingPhaseTests
{
    [Theory]
    [InlineData("TESLİM EDİLDİ")]
    [InlineData("17.07.2026 TESLIM EDILDI")]
    [InlineData("BOŞALDI")]
    [InlineData("TAHLİYE EDİLDİ")]
    [InlineData("MÜŞTERİYE VARDI")]
    [InlineData("GELDİ")]
    [InlineData("İŞLEM TAMAMLANDI")]
    [InlineData("выгружен")]
    [InlineData("ВЫГРУЖЕН 16.05")]
    public void IsCompleted_DeliveredKeywords_ReturnsTrue(string status)
    {
        Assert.True(TrackingPhase.IsCompleted(status));
    }

    [Theory]
    [InlineData("YÜKLENDİ ÇIKIŞ BEKLENİYOR")]
    [InlineData("7.7 CUT OFF GEMİ BOOKİNG ALINDI")]
    [InlineData("GÜMRÜKTE BEKLİYOR")]
    [InlineData("в пути")]
    [InlineData(null)]
    [InlineData("")]
    public void IsCompleted_PendingOrEmpty_ReturnsFalse(string? status)
    {
        Assert.False(TrackingPhase.IsCompleted(status));
    }
}

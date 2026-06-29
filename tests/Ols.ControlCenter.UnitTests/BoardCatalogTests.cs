using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Domain.Enums;

namespace Ols.ControlCenter.UnitTests;

/// <summary>
/// <see cref="BoardCatalog"/> tek kaynak — API projeksiyonu, metrikler ve raporlama hep buradan
/// okur. Burada bir tutarsızlık (yinelenen anahtar, eksik grup) sessizce tüm akışları bozabilir.
/// </summary>
public class BoardCatalogTests
{
    [Fact]
    public void All_HasExactlyNineBoards()
    {
        Assert.Equal(9, BoardCatalog.All.Count);
    }

    [Fact]
    public void All_BoardKeys_AreUnique()
    {
        var keys = BoardCatalog.All.Select(b => b.Key).ToList();

        Assert.Equal(keys.Distinct().Count(), keys.Count);
    }

    [Fact]
    public void All_EveryBoard_BelongsToAKnownGroup()
    {
        Assert.All(BoardCatalog.All, b => Assert.Contains(b.Group, BoardCatalog.Groups));
    }

    [Fact]
    public void Find_ExistingKey_ReturnsMeta()
    {
        var meta = BoardCatalog.Find("deniz-transit");

        Assert.NotNull(meta);
        Assert.Equal("Deniz", meta!.Group);
        Assert.Equal(TrackingBoardType.SeaTransit, meta.Board);
    }

    [Fact]
    public void Find_UnknownKey_ReturnsNull()
    {
        Assert.Null(BoardCatalog.Find("olmayan-board"));
    }

    [Fact]
    public void Alabora_BelongsToFinansGroup_NotOperational()
    {
        var meta = BoardCatalog.ForBoard(TrackingBoardType.Alabora);

        Assert.NotNull(meta);
        Assert.Equal("Finans", meta!.Group);
        Assert.DoesNotContain("Finans", BoardCatalog.OperationalGroups);
    }

    [Fact]
    public void OperationalGroups_ContainsOnlyDenizKaraHava()
    {
        Assert.Equal(new[] { "Deniz", "Kara", "Hava" }, BoardCatalog.OperationalGroups);
    }
}

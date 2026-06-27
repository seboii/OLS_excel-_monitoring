namespace Ols.ControlCenter.Shared.Pagination;

/// <summary>Liste uçları için ortak sayfalama + arama + sıralama girdisi.</summary>
public class PagedRequest
{
    public const int MaxPageSize = 200;
    public const int DefaultPageSize = 25;

    private int _pageSize = DefaultPageSize;
    private int _page = 1;

    /// <summary>1 tabanlı sayfa numarası.</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Sayfa boyutu (1..<see cref="MaxPageSize"/> aralığına sıkıştırılır).</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > MaxPageSize ? DefaultPageSize : value;
    }

    /// <summary>Serbest metin araması (uca özel alanlarda aranır).</summary>
    public string? Search { get; set; }

    /// <summary>Sıralanacak alan adı (uca özel beyaz listeyle doğrulanır).</summary>
    public string? SortBy { get; set; }

    /// <summary>true ise azalan sıralama.</summary>
    public bool SortDesc { get; set; }

    public int Skip => (Page - 1) * PageSize;
}

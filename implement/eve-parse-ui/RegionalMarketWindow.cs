using static eve_parse_ui.UITreeNodeWithDisplayRegion;

namespace eve_parse_ui
{
  public record RegionalMarketWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }

    public required UITreeNodeWithDisplayRegion BrowseButton { get; init; }
    public required UITreeNodeWithDisplayRegion QuickbarButton { get; init; }
    public required UITreeNodeWithDisplayRegion DetailsButton { get; init; }
    public required UITreeNodeWithDisplayRegion GroupsButton { get; init; }
    public required UITreeNodeWithDisplayRegion MarketOrdersButton { get; init; }
    public required UITreeNodeWithDisplayRegion MultibuyButton { get; init; }
    public UITreeNodeWithDisplayRegion? ResultsFilterButton { get; init; }
    public UITreeNodeWithDisplayRegion? MarketDataTab { get; init; }
    public UITreeNodeWithDisplayRegion? PriceHistoryTab { get; init; }
    public UITreeNodeWithDisplayRegion? PlaceBuyOrderButton { get; init; }
    public UITreeNodeWithDisplayRegion? FindInContractsButton { get; init; }
    public UITreeNodeWithDisplayRegion? ExportToFileButton { get; init; }
    public UITreeNodeWithDisplayRegion? SearchField { get; init; }
    public IEnumerable<DisplayTextWithRegion>? SearchResults { get; init; }
    public ScrollingPanel? SearchResultsPanel { get; init; }
    public string? SelectedItemName { get; init; }
    public IEnumerable<MarketOrder>? Sellers { get; init; }
    public ScrollingPanel? SellersPanel { get; init; }
    public IEnumerable<MarketOrder>? Buyers { get; init; }
    public ScrollingPanel? BuyersPanel { get; init; }
  }

  public record MarketOrder
  {
    public required string Jumps;
    public required int Quantity;
    public required Double Price;
    public required string Location;
    public string? Range;
    public int? MinVolume;
    public required string ExpiresIn;
    public required bool InRange;
  }
}
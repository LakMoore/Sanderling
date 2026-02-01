namespace eve_parse_ui
{
  // ==== MarketOrders ====
  public record MarketOrdersWindow
  {
    public enum Tab
    {
      MyOrders,
      CorpOrders,
      OrdersHistory
    }

    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required Tab CurrentTab { get; init; }
    public required UITreeNodeWithDisplayRegion MyOrdersTab { get; init; }
    public required UITreeNodeWithDisplayRegion CorpOrdersTab { get; init; }
    public required UITreeNodeWithDisplayRegion OrdersHistoryTab { get; init; }
    public required UITreeNodeWithDisplayRegion SellingPanel { get; init; }
    public ScrollingPanel? SellingScroller { get; init; }
    public required IEnumerable<OpenMarketOrder> SellOrders { get; init; }
    public required UITreeNodeWithDisplayRegion BuyingPanel { get; init; }
    public ScrollingPanel? BuyingScroller { get; init; }
    public required IEnumerable<OpenMarketOrder> BuyOrders { get; init; }
    public int? OrdersRemaining { get; init; }
  }

  public record OpenMarketOrder
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string Type;
    public required int QuantityRemaining;
    public required int Quantity;
    public required double Price;
    public required string Station;
    public required string Region;
    public string? Range;
    public int? MinVolume;
    public required string ExpiresIn;
    public string? IssuedBy;
    public string? WalletDivision;
  }
}

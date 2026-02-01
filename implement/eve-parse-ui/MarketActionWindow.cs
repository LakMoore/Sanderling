namespace eve_parse_ui
{
  public abstract record MarketActionWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }

    public required string TypeName;
    public required UITreeNodeWithDisplayRegion CancelButton;
  }

  public enum Mode
  {
    Simple,
    Advanced,
  }

  public record BuyMarketActionWindow : MarketActionWindow
  {
    public required Mode mode;
    public required UITreeNodeWithDisplayRegion? LocationLink;
    public required UITreeNodeWithDisplayRegion Quantity;
    public required UITreeNodeWithDisplayRegion? BidPrice;
    public required UITreeNodeWithDisplayRegion? Duration;
    public required UITreeNodeWithDisplayRegion? Range;
    public required UITreeNodeWithDisplayRegion UseCorpAccount;
    public required UITreeNodeWithDisplayRegion? RememberSettings;
    public required UITreeNodeWithDisplayRegion BuyButton;
    public required UITreeNodeWithDisplayRegion ToggleModeButton;
  }

  public record ModifyMarketActionWindow : MarketActionWindow
  {
    public required UITreeNodeWithDisplayRegion? NewBuyPrice;
    public required UITreeNodeWithDisplayRegion OKButton;
  }
}
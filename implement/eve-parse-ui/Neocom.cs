namespace eve_parse_ui
{
  // ==== Neocom ====
  public record Neocom
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? EveMenuButton { get; init; }

    public UITreeNodeWithDisplayRegion? InventoryButton { get; init; }
    public UITreeNodeWithDisplayRegion? AssetsButton { get; init; }
    public UITreeNodeWithDisplayRegion? MarketButton { get; init; }
    public UITreeNodeWithDisplayRegion? PlanetaryIndustryButton { get; init; }
    public IReadOnlyList<NeocomPanelItem>? PanelGroups { get; init; }
    public IReadOnlyList<NeocomPanelItem>? PanelCommands { get; init; }
    public NeocomClock? Clock { get; init; }

    // Additional properties from AI conversion
    public IReadOnlyList<NeocomButton> Buttons { get; init; } = new List<NeocomButton>();
    public string? SystemName { get; init; }
    public string? SystemSecurityStatus { get; init; }
  }

  public record NeocomButton
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Name { get; init; }
    public bool IsActive { get; init; }
    public bool IsHighlighted { get; init; }
    public bool IsBlinking { get; init; }
  }

  public record NeocomPanelItem
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string Text { get; init; }
  }

  public record NeocomClock
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string Text { get; init; }
    public ParsedTime? ParsedText { get; init; }
  }

  public record ParsedTime
  {
    public required int Hour { get; init; }
    public required int Minute { get; init; }
  }
}

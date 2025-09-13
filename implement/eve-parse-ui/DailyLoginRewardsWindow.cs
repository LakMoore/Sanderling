namespace eve_parse_ui
{
  public record DailyLoginRewardsWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? ClaimButton { get; init; }
    public UITreeNodeWithDisplayRegion? CloseButton { get; init; }
  }
}

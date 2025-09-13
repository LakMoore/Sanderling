namespace eve_parse_ui
{
  public record ExpandedUtilMenu
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? SearchBox { get; init; }
    public UITreeNodeWithDisplayRegion? SearchButton { get; init; }
  }
}
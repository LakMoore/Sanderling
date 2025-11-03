namespace eve_parse_ui
{
  public class AssetsWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required UITreeNodeWithDisplayRegion TabAll { get; init; }
    public required UITreeNodeWithDisplayRegion TabSearch { get; init; }  
    public required UITreeNodeWithDisplayRegion TabSafety { get; init; }
    public required IEnumerable<AssetLocation> AssetLocations { get; init; }
  }

  public class AssetLocation
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required float SecurityStatus { get; init; }
    public required string Name { get; init; }
    public required int Items { get; init; }
    public required int Jumps { get; init; }
  }
}
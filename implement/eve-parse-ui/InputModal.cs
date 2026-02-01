namespace eve_parse_ui
{
  public record InputModal
  {
    public enum Type
    {
      Numeric,
      Text
    }

    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required Type InputType { get; init; }
    public required UITreeNodeWithDisplayRegion Textbox { get; init; }
    public required string Title { get; init; }
    public required UITreeNodeWithDisplayRegion OkButton { get; init; }
    public required UITreeNodeWithDisplayRegion CancelButton { get; init; }
  }
}
namespace eve_parse_ui
{
    public record ExpandedUtilMenu
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required UITreeNodeWithDisplayRegion SearchBox { get; init; }
        public required UITreeNodeWithDisplayRegion SearchButton { get; init; }
    }
}
namespace eve_parse_ui
{
    public record ListWindow
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required UITreeNodeWithDisplayRegion CollapseAllButton { get; init; }
        public required IReadOnlyList<ListGroup> ListGroups { get; init; }
        public required UITreeNodeWithDisplayRegion CloseButton { get; init; }
        public ScrollingPanel? ScrollingPanel { get; init; }
    }

    public record ListGroup
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required string Name { get; init; }
        public required bool IsCollapsed { get; init; }
        public required IReadOnlyList<ListItem> ListItems { get; init; }
    }

    public record ListItem
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required string Name { get; init; }
    }
}
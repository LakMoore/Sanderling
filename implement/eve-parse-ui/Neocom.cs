namespace eve_parse_ui
{
    // ==== Neocom ====
    public record Neocom
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public UITreeNodeWithDisplayRegion? EveMenuButton { get; init; }

        public UITreeNodeWithDisplayRegion? InventoryButton { get; init; }
        public UITreeNodeWithDisplayRegion? PlanetaryIndustryButton { get; init; }
        public IReadOnlyList<NeocomPanelItem>? PanelGroups { get; init; }
        public IReadOnlyList<NeocomPanelItem>? PanelCommands { get; init; }
        public NeocomClock? Clock { get; init; }
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

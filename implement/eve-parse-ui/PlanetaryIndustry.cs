namespace eve_parse_ui
{
    // ==== Planet Window ====
    public record PlanetsWindow
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required IReadOnlyList<Colony> Colonies { get; init; }
        public required ScrollingPanel? ScrollingPanel { get; init; }
        public UITreeNodeWithDisplayRegion? ViewButton { get; init; }
        public UITreeNodeWithDisplayRegion? WarpToButton { get; init; }
        public UITreeNodeWithDisplayRegion? AccessButton { get; init; }
    }

    public record Colony
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required string Name { get; init; }
        public required UITreeNodeWithDisplayRegion MainIcon { get; init; }
        public required bool IsSelected { get; init; }
        public UITreeNodeWithDisplayRegion? RestartExtractionButton { get; init; }
        public required bool RequiresAttention { get; init; }
        public required IReadOnlyList<PIItem> PIItems { get; init; }
    }

    public record PIItem
    {
        public required int TypeId { get; init; }
        public required int TierIndex { get; init; }
    }

    public record PlanetaryImportExportUI
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required string Name { get; init; }
        public required CustomsOfficeList CustomsList { get; init; }
        public required CustomsOfficeList SpaceportList { get; init; }
        public required int TransferCost { get; init; }
        public required UITreeNodeWithDisplayRegion TransferButton { get; init; }
    }

    public record CustomsOfficeList
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required IReadOnlyList<CustomsEntry> Entries { get; init; }
    }

    public record CustomsEntry
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required int Quantity { get; init; }
        public required string CommodityName { get; init; }
        public required string Tier { get; init; }
        public required bool IsPending { get; init; }
    }
}

namespace eve_parse_ui
{
    public record CharacterSelectionScreen
    {
        public required IReadOnlyList<CharacterSlot> CharacterSlots { get; init; }
        public required bool AccountIsAlpha { get; init; }
    }

    public record CharacterSlot
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string? Name { get; init; }
        public string? CharacterId { get; init; }
        public string? ShipTypeName { get; init; }
        public string? SystemName { get; init; }
        public float? SystemSecStatus { get; init; }
        public bool? IsUndocked { get; init; }
    }
}
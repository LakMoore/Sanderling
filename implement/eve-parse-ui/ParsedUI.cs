using read_memory_64_bit;

namespace eve_parse_ui
{
    public record ParsedUserInterface
    {
        public required UITreeNodeNoDisplayRegion UiTree { get; init; }
        public required IReadOnlyList<OverviewWindow> OverviewWindows { get; init; }
        public InfoPanelContainer? InfoPanelContainer { get; init; }
        public StationWindow? StationWindow { get; init; }
        public ShipUI? ShipUI { get; init; }
        public ProbeScannerWindow? ProbeScanner { get; init; }
        public LayerAboveMain? LayerAboveMain { get; init; }
        public required IReadOnlyList<MessageBox> MessageBoxes { get; init; }
    }

    public record DisplayRegion(int X, int Y, int Width, int Height)
    {
        public bool Contains(Location2d point) =>
            point.X >= X && point.X <= X + Width &&
            point.Y >= Y && point.Y <= Y + Height;

        public int? AreaFromDisplayRegion()
        {
            if (this.Width < 0 || this.Height < 0)
            {
                return null;
            }
            else
            {
                return this.Width * this.Height;
            }
        }
    }

    public record Location2d(int X, int Y);

    // ==== Overview ====

    public record OverviewWindow
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required string OverviewTabName { get; init; }
        public required IReadOnlyList<string> Tabs { get; init; }
        public required IReadOnlyList<(string, UITreeNodeWithDisplayRegion)> EntriesHeader { get; init; }
        public required IReadOnlyList<OverviewWindowEntry> Entries { get; init; }
        public required ScrollControls? ScrollControls { get; init; }
    }

    public record ScrollControls
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public UITreeNodeWithDisplayRegion? ScrollHandle { get; set; }
    }

    public record OverviewWindowEntry
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required IDictionary<string, string> CellsTexts { get; init; }
        public required string[] TextsLeftToRight { get; init; }
        public string? ObjectDistance { get; init; }
        public int? ObjectDistanceInMeters { get; init; }
        public string? ObjectName { get; init; }
        public string? ObjectType { get; init; }
        public string? ObjectCorporation { get; init; }
        public string? ObjectAlliance { get; init; }
        public int? ObjectVelocity { get; init; }
        public ColorComponents? IconSpriteColorPercent { get; init; }
        public IReadOnlyList<string>? NamesUnderSpaceObjectIcon { get; init; } = new List<string>();
        public IReadOnlyList<ColorComponents>? BgColorFillsPercent { get; init; } = new List<ColorComponents>();
        public IReadOnlyList<string> RightAlignedIconsHints { get; init; } = new List<string>();
        public required OverviewWindowEntryCommonIndications CommonIndications { get; init; }
        public int OpacityPercent { get; init; }
    }

    public record OverviewWindowEntryCommonIndications
    {
        public bool Targeting { get; init; }
        public bool TargetedByMe { get; init; }
        public bool IsJammingMe { get; init; }
        public bool IsWarpDisruptingMe { get; init; }
    }

    public record ColorComponents
    {
        public int A { get; set; } = 100;
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }

    // ==== InfoPanel ====

    public record InfoPanelContainer
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public InfoPanelIcons? Icons { get; set; }
        public InfoPanelLocationInfo? InfoPanelLocationInfo { get; set; }
        public InfoPanelRoute? InfoPanelRoute { get; set; }
        public InfoPanelAgentMissions? InfoPanelAgentMissions { get; set; }
    }

    public record InfoPanelIcons
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public UITreeNodeWithDisplayRegion? Search { get; set; }
        public UITreeNodeWithDisplayRegion? LocationInfo { get; set; }
        public UITreeNodeWithDisplayRegion? Route { get; set; }
        public UITreeNodeWithDisplayRegion? AgentMissions { get; set; }
        public UITreeNodeWithDisplayRegion? DailyChallenge { get; set; }
    }

    public record InfoPanelRoute
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public required List<InfoPanelRouteRouteElementMarker> RouteElementMarkers { get; set; }
    }

    public record InfoPanelRouteRouteElementMarker
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
    }

    public record InfoPanelLocationInfo
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public required UITreeNodeWithDisplayRegion ListSurroundingsButton { get; set; }
        public string? CurrentSolarSystemName { get; set; }
        public int? SecurityStatusPercent { get; set; }
        public string? SecurityStatusColor { get; set; }
        public InfoPanelLocationInfoExpandedContent? ExpandedContent { get; set; }
    }

    public record InfoPanelLocationInfoExpandedContent
    {
        public string? CurrentStationName { get; set; }
    }

    public record InfoPanelAgentMissions
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public required List<InfoPanelAgentMissionsEntry> Entries { get; set; }
    }

    public record InfoPanelAgentMissionsEntry
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
    }

    public record StationWindow
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public UITreeNodeWithDisplayRegion? UndockButton { get; set; }
        public UITreeNodeWithDisplayRegion? AbortUndockButton { get; set; }
    }

    // ==== Inventory ====
    public record InventoryWindow
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required IReadOnlyList<InventoryWindowLeftTreeEntry> LeftTreeEntries { get; init; }
        public string? SubCaptionLabelText { get; init; }
        public InventoryWindowCapacityGauge? SelectedContainerCapacityGauge { get; init; }
        public Inventory? SelectedContainerInventory { get; init; }
        public UITreeNodeWithDisplayRegion? ButtonToSwitchToListView { get; init; }
        public UITreeNodeWithDisplayRegion? ButtonToStackAll { get; init; }
    }

    public record Inventory
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public InventoryItemsView? ItemsView { get; init; }
        public ScrollControls? ScrollControls { get; init; }
    }

    public abstract record InventoryItemsView;
    public record InventoryItemsListView(IReadOnlyList<InventoryItemsListViewEntry> Items) : InventoryItemsView;
    public record InventoryItemsNotListView(IReadOnlyList<UITreeNodeWithDisplayRegion> Items) : InventoryItemsView;

    public record InventoryWindowLeftTreeEntry
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public UITreeNodeWithDisplayRegion? ToggleBtn { get; init; }
        public UITreeNodeWithDisplayRegion? SelectRegion { get; init; }
        public required string Text { get; init; }
        public required IReadOnlyList<InventoryWindowLeftTreeEntryChild> Children { get; init; }
    }

    public record InventoryWindowLeftTreeEntryChild(InventoryWindowLeftTreeEntry Entry);

    public record InventoryWindowCapacityGauge
    {
        public required int Used { get; init; }
        public int? Maximum { get; init; }
        public int? Selected { get; init; }
    }

    public record InventoryItemsListViewEntry
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required IReadOnlyDictionary<string, string> CellsTexts { get; init; }
    }

    // ==== Ship UI ====
    public record ShipUI
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public required ShipUICapacitor Capacitor { get; set; }
        public required Hitpoints HitpointsPercent { get; set; }
        public ShipUIIndication? Indication { get; set; }
        public required List<ShipUIModuleButton> ModuleButtons { get; set; }
        public required ModuleButtonsRows ModuleButtonsRows { get; set; }
        public required List<OffensiveBuffButton> OffensiveBuffButtons { get; set; }
        public SquadronsUI? SquadronsUI { get; set; }
        public UITreeNodeWithDisplayRegion? StopButton { get; set; }
        public UITreeNodeWithDisplayRegion? MaxSpeedButton { get; set; }
        public ShipUIHeatGauges? HeatGauges { get; set; }
    }

    public record ShipUIIndication
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public ShipManeuverType? ManeuverType { get; set; }
    }

    public record ShipUIModuleButton
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required UITreeNodeWithDisplayRegion SlotUINode { get; init; }
        public bool? IsActive { get; init; }
        public bool IsHiliteVisible { get; init; }
        public bool IsBusy { get; init; }
        public int? RampRotationMilli { get; init; }
        public int? TypeID { get; init; }
        public int? Blinking { get; init; }
        public bool? Online { get; init; }
        public int? Quantity { get; init; }
        public bool? IsDeactivating { get; init; }
        public string? SlotName { get; init; }
    }

    public record ShipUICapacitor
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public required List<ShipUICapacitorPmark> PMarks { get; set; }
        public int? LevelFromPMarksPercent { get; set; }
    }

    public record ShipUICapacitorPmark
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public required ColorComponents ColorPercent { get; set; }
    }

    public record ShipUIHeatGauges
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public required List<ShipUIHeatGauge> Gauges { get; set; }
    }

    public record ShipUIHeatGauge
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public int? RotationPercent { get; set; }
        public int? HeatPercent { get; set; }
    }

    public record Hitpoints
    {
        public int Structure { get; set; }
        public int Armor { get; set; }
        public int Shield { get; set; }
    }

    public enum ShipManeuverType
    {
        ManeuverWarp,
        ManeuverJump,
        ManeuverOrbit,
        ManeuverApproach
    }

    public record SquadronsUI
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public required List<SquadronUI> Squadrons { get; set; }
    }

    public record SquadronUI
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public required List<SquadronAbilityIcon> Abilities { get; set; }
        public required UITreeNodeWithDisplayRegion ActionLabel { get; set; }
    }

    public record SquadronAbilityIcon
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public int? Quantity { get; set; }
        public bool? RampActive { get; set; }
    }

    public record ModuleButtonsRows
    {
        public required List<ShipUIModuleButton> Top { get; set; }
        public required List<ShipUIModuleButton> Middle { get; set; }
        public required List<ShipUIModuleButton> Bottom { get; set; }
    }

    public record OffensiveBuffButton
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; set; }
        public required string Name { get; set; }
    }

    // ==== Probe Scanner ====
    public record ProbeScannerWindow
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required List<ProbeScanResult> ScanResults { get; init; }
    }

    public record ProbeScanResult
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required List<string>? TextsLeftToRight { get; init; }
        public required Dictionary<string, string>? CellsTexts { get; init; }
        public UITreeNodeWithDisplayRegion? WarpButton { get; init; }
    }

    // ==== MessageBox ====
    public class MessageBox
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string? TextHeadline { get; init; }
        public string? TextBody { get; init; }
        public UITreeNodeWithDisplayRegion? ButtonGroup { get; init; }
        public required List<MessageBoxButton> Buttons { get; init; }
    }

    public class MessageBoxButton
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string? MainText { get; init; }
    }

    // ==== LayerAboveMain ====
    public record LayerAboveMain
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public QuickMessage? QuickMessage { get; init; }
    }

    public record QuickMessage
    {
        public required UITreeNodeWithDisplayRegion UiNode { get; init; }
        public required string Text { get; init; }
    }


}

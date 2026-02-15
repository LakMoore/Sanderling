using System.Runtime.Intrinsics.X86;
using static eve_parse_ui.UITreeNodeWithDisplayRegion;

namespace eve_parse_ui
{
  public record ParsedUserInterface
  {
    public required UITreeNodeWithDisplayRegion UiTree { get; init; }
    public required Lazy<IEnumerable<ContextMenu>> ContextMenus { get; init; }
    public required Lazy<ShipUI?> ShipUI { get; init; }
    public required Lazy<IEnumerable<Target>> Targets { get; init; }
    public required Lazy<InfoPanelContainer?> InfoPanelContainer { get; init; }
    public required Lazy<IEnumerable<OverviewWindow>> OverviewWindows { get; init; }
    public required Lazy<SelectedItemWindow?> SelectedItemWindow { get; init; }
    public required Lazy<DronesWindow?> DronesWindow { get; init; }
    public required Lazy<FittingWindow?> FittingWindow { get; init; }
    public required Lazy<ProbeScannerWindow?> ProbeScannerWindow { get; init; }
    public required Lazy<DirectionalScannerWindow?> DirectionalScannerWindow { get; init; }
    public required Lazy<StationWindow?> StationWindow { get; init; }
    public required Lazy<IEnumerable<InventoryWindow>> InventoryWindows { get; init; }
    public required Lazy<IEnumerable<WindowStack>> WindowStacks { get; init; }
    public required Lazy<IEnumerable<ChatWindow>> ChatWindows { get; init; }
    public required Lazy<IEnumerable<AgentConversationWindow>> AgentConversationWindows { get; init; }
    public required Lazy<SurveyScanWindow?> SurveyScanWindow { get; init; }
    public required Lazy<BookmarkLocationWindow?> BookmarkLocationWindow { get; init; }
    public required Lazy<RepairShopWindow?> RepairShopWindow { get; init; }
    public required Lazy<CharacterSheetWindow?> CharacterSheetWindow { get; init; }
    public required Lazy<FleetWindow?> FleetWindow { get; init; }
    public required Lazy<LocationsWindow?> LocationsWindow { get; init; }
    public required Lazy<WatchListPanel?> WatchListPanel { get; init; }
    public required Lazy<StandaloneBookmarkWindow?> StandaloneBookmarkWindow { get; init; }
    public required Lazy<ModuleButtonTooltip?> ModuleButtonTooltip { get; init; }
    public required Lazy<HeatStatusTooltip?> HeatStatusTooltip { get; init; }
    public required Lazy<Neocom?> Neocom { get; init; }
    public required Lazy<IEnumerable<MessageBox>> MessageBoxes { get; init; }
    public required Lazy<LayerAboveMain?> LayerAboveMain { get; init; }
    public required Lazy<KeyActivationWindow?> KeyActivationWindow { get; init; }
    public required Lazy<CompressionWindow?> CompressionWindow { get; init; }
    public required Lazy<PlanetsWindow?> PlanetsWindow { get; init; }
    public required Lazy<PlanetaryImportExportUI?> PlanetaryImportExportUI { get; init; }
    public required Lazy<SessionTimeIndicator?> SessionTimeIndicator { get; init; }
    public required Lazy<InputModal?> InputModal { get; init; }
    public required Lazy<ExpandedUtilMenu?> ExpandedUtilMenu { get; init; }
    public required Lazy<IEnumerable<ListWindow>> ListWindows { get; init; }
    public required Lazy<CharacterSelectionScreen?> CharacterSelectionScreen { get; init; }
    public required Lazy<DailyLoginRewardsWindow?> DailyLoginRewardsWindow { get; init; }
    public required Lazy<IEnumerable<InfoWindow>> InfoWindows { get; init; }
    public required Lazy<AssetsWindow?> AssetsWindow { get; init; }
    public required Lazy<RegionalMarketWindow?> RegionalMarketWindow { get; init; }
    public required Lazy<BuyMarketActionWindow?> BuyMarketActionWindow { get; init; }
    public required Lazy<ModifyMarketActionWindow?> ModifyMarketActionWindow { get; init; }
    public required Lazy<SelectStationWindow?> SelectStationWindow { get; init; }
    public required Lazy<MarketOrdersWindow?> MarketOrdersWindow { get; init; }

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

  // ==== Context Menu ====
  public record ContextMenu
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required IReadOnlyList<ContextMenuEntry> Entries { get; init; }
  }

  public record ContextMenuEntry
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string Text { get; init; }
  }

  // ==== Overview ====
  public record OverviewWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string OverviewTabName { get; init; }
    public UITreeNodeWithDisplayRegion? MinimiseButton { get; init; }
    public required IReadOnlyList<string> Tabs { get; init; }
    public required IReadOnlyList<DisplayTextWithRegion> EntriesHeader { get; init; }
    public required IEnumerable<OverviewWindowEntry> Entries { get; init; }
    public required ScrollingPanel? ScrollingPanel { get; init; }
  }

  public record ScrollingPanel
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
    public UITreeNodeWithDisplayRegion? SearchBox { get; set; }
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
    public required bool IsExpanded { get; set; }
    public UITreeNodeWithDisplayRegion? ExpandRouteButton { get; set; }
    public UITreeNodeWithDisplayRegion? AutopilotMenuButton { get; set; }
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

  // ==== Target ====
  public record Target
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? BarAndImageCont { get; init; }
    public required IReadOnlyList<string> TextsTopToBottom { get; init; }
    public required bool IsActiveTarget { get; init; }
    public UITreeNodeWithDisplayRegion? AssignedContainerNode { get; init; }
    public required IReadOnlyList<UITreeNodeWithDisplayRegion> AssignedIcons { get; init; }
    public bool IsHighlighted { get; init; }
    public bool IsSelected { get; init; }
    public bool IsExpanded { get; init; }
    public bool IsWarpTo { get; init; }
    public bool IsJamming { get; init; }
    public bool IsTargeting { get; init; }
    public bool IsTargetingMe { get; init; }
    public bool IsWarpDisrupted { get; init; }
    public bool IsWarpScrambled { get; init; }
    public bool IsWebified { get; init; }
    public bool IsBeingTargeted { get; init; }
    public bool IsBeingTargetedByMe { get; init; }
    public bool IsBeingTargetedByMeOnly { get; init; }
    public int? BarPercent { get; init; }
    public ColorComponents? BarColor { get; init; }
    public IReadOnlyList<string> Icons { get; init; } = new List<string>();
    public IReadOnlyList<string> IconsWithHints { get; init; } = new List<string>();
  }

  // ==== StationWindow ====
  public record StationWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; set; }
    public UITreeNodeWithDisplayRegion? UndockButton { get; set; }
    public UITreeNodeWithDisplayRegion? AbortUndockButton { get; set; }
    public UITreeNodeWithDisplayRegion? DockedModeButton { get; set; }
  }

  // ==== Inventory ====
  public record InventoryWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string WindowCaption { get; init; }
    public required LeftTreePanel LeftTreePanel { get; init; }
    public string? SubCaptionLabelText { get; init; }
    public InventoryWindowCapacityGauge? SelectedContainerCapacityGauge { get; init; }
    public required Inventory SelectedInventory { get; init; }
    public UITreeNodeWithDisplayRegion? ButtonToSwitchToListView { get; init; }
    public UITreeNodeWithDisplayRegion? ButtonToStackAll { get; init; }
    public required UITreeNodeWithDisplayRegion FilterTextBox { get; init; }
  }

  public record LeftTreePanel
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required IEnumerable<InventoryWindowLeftTreeEntry> Entries { get; init; }
    public ScrollingPanel? ScrollingPanel { get; init; }
  }

  public record Inventory
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required IReadOnlyList<InventoryItem> Items { get; init; }
    public ScrollingPanel? ScrollingPanel { get; init; }
  }

  public record InventoryWindowLeftTreeEntry
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? ToggleBtn { get; init; }
    public UITreeNodeWithDisplayRegion? SelectRegion { get; init; }
    public required string Text { get; init; }
    public required bool IsSelected { get; init; }
    public required IReadOnlyList<InventoryWindowLeftTreeEntry> Children { get; init; }
  }

  public record InventoryWindowCapacityGauge
  {
    public required int Used { get; init; }
    public int? Maximum { get; init; }
    public int? Selected { get; init; }
  }

  public record InventoryItem
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string Name { get; init; }
    public int? Quantity { get; init; }
  }

  public record WindowStack
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required IEnumerable<WindowStackTab> Tabs { get; init; }
  }

  public record WindowStackTab
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; init; }
  }

  public record ChatWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Name { get; init; }
    public required ChatWindowUserlist Userlist { get; init; }
  }

  public record ChatWindowUserlist
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required IEnumerable<ChatUserEntry> VisibleUsers { get; init; }
    public ScrollingPanel? ScrollingPanel { get; init; }
  }

  public record ChatUserEntry
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string Name { get; init; }
    public string? CharacterID { get; init; }
    public string? StandingIconHint { get; init; }
  }

  // ==== Module Button Tooltip ====
  public record ModuleButtonTooltip
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public ModuleButtonTooltipShortcut? Shortcut { get; init; }
    public ModuleButtonTooltipOptimalRange? OptimalRange { get; init; }
    public string? ModuleName { get; init; }
    public string? GroupName { get; init; }
    public string? ActivationEffect { get; init; }
    public string? DeactivationEffect { get; init; }
    public string? CycleTime { get; init; }
    public string? FalloffRange { get; init; }
    public string? Duration { get; init; }
    public string? ActivationCost { get; init; }
    public string? ActivationTime { get; init; }
    public string? DeactivationTime { get; init; }
    public string? HeatDamage { get; init; }
    public string? HeatDamageBonus { get; init; }
    public string? OverloadBonus { get; init; }
    public string? OverloadSelfBonus { get; init; }
    public string? OverloadDurationBonus { get; init; }
  }

  public record ModuleButtonTooltipShortcut
  {
    public required string Text { get; init; }
    public List<int>? ParseResult { get; init; }
  }

  public record ModuleButtonTooltipOptimalRange
  {
    public required string AsString { get; init; }
    public int? InMeters { get; init; }
  }

  public record HeatStatusTooltip
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public int? LowPercent { get; init; }
    public int? MediumPercent { get; init; }
    public int? HighPercent { get; init; }
    public int? HeatLevel { get; init; }
    public string? HeatLevelText { get; init; }
    public string? HeatDamage { get; init; }
    public string? HeatDamageBonus { get; init; }
    public string? OverloadBonus { get; init; }
    public string? OverloadSelfBonus { get; init; }
    public string? OverloadDurationBonus { get; init; }
  }

  // ==== Agent Conversation ====
  public record AgentConversationWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? AgentName { get; init; }
    public string? AgentCorporation { get; init; }
    public required IReadOnlyList<AgentConversationMessage> Messages { get; init; }
    public required IReadOnlyList<AgentConversationResponse> Responses { get; init; }
    public UITreeNodeWithDisplayRegion? InputBox { get; init; }
  }

  public record AgentConversationMessage
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Text { get; init; }
    public bool IsFromAgent { get; init; }
    public string? Timestamp { get; init; }
  }

  public record AgentConversationResponse
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Text { get; init; }
    public bool IsHighlighted { get; init; }
  }

  // ==== Bookmark Location ====
  public record BookmarkLocationWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? NameInput { get; init; }
    public UITreeNodeWithDisplayRegion? NotesInput { get; init; }
    public UITreeNodeWithDisplayRegion? CreateButton { get; init; }
    public UITreeNodeWithDisplayRegion? CancelButton { get; init; }
    public UITreeNodeWithDisplayRegion? SubmitButton { get; init; }
  }

  // ==== Fleet ====
  public record FleetWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public IReadOnlyList<FleetMember> Members { get; init; } = new List<FleetMember>();
    public string? FleetName { get; init; }
    public bool IsCommander { get; init; }
    public UITreeNodeWithDisplayRegion? InviteButton { get; init; }
    public UITreeNodeWithDisplayRegion? LeaveButton { get; init; }
  }

  public record FleetMember
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Name { get; init; }
    public string? ShipType { get; init; }
    public string? SystemName { get; init; }
    public int? SolarSystemId { get; init; }
    public int? Distance { get; init; }
    public bool IsWarping { get; init; }
    public bool IsInFleetHangar { get; init; }
    public bool IsInFleetHangarAccessAllowed { get; init; }
  }

  // ==== Watch List ====
  public record WatchListPanel
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public IReadOnlyList<WatchListEntry> Entries { get; init; } = new List<WatchListEntry>();
  }

  public record WatchListEntry
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Name { get; init; }
    public string? TypeName { get; init; }
    public string? Distance { get; init; }
    public bool IsSelected { get; init; }
  }

  // ==== Standalone Bookmark ====
  public record StandaloneBookmarkWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required UITreeNodeWithDisplayRegion? SearchTextbox { get; init; }
    public required IReadOnlyList<UITreeNodeWithDisplayRegion> Entries { get; init; }
    public ScrollingPanel? ScrollingPanel { get; init; }
  }

  // ==== Ship UI ====
  public record ShipUI
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required ShipUICapacitor Capacitor { get; init; }
    public required Hitpoints HitpointsPercent { get; init; }
    public ShipUIIndication? Indication { get; init; }
    public required List<ShipUIModuleButton> ModuleButtons { get; init; }
    public required ModuleButtonsRows ModuleButtonsRows { get; init; }
    public required List<OffensiveBuffButton> OffensiveBuffButtons { get; init; }
    public required List<string> DefensiveBuffs { get; init; }
    public SquadronsUI? SquadronsUI { get; init; }
    public UITreeNodeWithDisplayRegion? StopButton { get; init; }
    public int CurrentSpeed { get; init; }
    public UITreeNodeWithDisplayRegion? MaxSpeedButton { get; init; }
    public ShipUIHeatGauges? HeatGauges { get; init; }
    public required bool IsInvulnerable { get; init; }
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
    public required bool IsOnCooldown { get; init; }
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
    ManeuverUnknown,
    ManeuverWarp,
    ManeuverJump,
    ManeuverOrbit,
    ManeuverAlign,
    ManeuverApproach,
    ManeuverDock
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
    public required IReadOnlyList<ProbeScanResult> ScanResults { get; init; }
    public ScrollingPanel? ScrollingPanel { get; init; }
    public UITreeNodeWithDisplayRegion? ScanButton { get; init; }
    public UITreeNodeWithDisplayRegion? StopButton { get; init; }
    public bool IsScanning { get; init; }
  }

  public record ProbeScanResult
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string Distance { get; init; }
    public required string ID { get; init; }
    public required string Name { get; init; }
    public string? Group { get; init; }
    public string? Signal { get; init; }
    public UITreeNodeWithDisplayRegion? WarpButton { get; init; }
    public string? TypeName { get; init; }
    public int? SignalStrength { get; init; }
    public float? DistanceInMeters { get; init; }
    public string? DistanceUnit { get; init; }
    public bool IsSelected { get; init; }
    public bool IsHighlighted { get; init; }
  }

  // ==== Directional Scanner ====
  public record DirectionalScannerWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? ScrollNode { get; init; }
    public required IReadOnlyList<DirectionalScanResult> ScanResults { get; init; }
    public UITreeNodeWithDisplayRegion? ScanButton { get; init; }
    public UITreeNodeWithDisplayRegion? RangeDropdown { get; init; }
    public UITreeNodeWithDisplayRegion? RangeText { get; init; }
  }

  public record DirectionalScanResult
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? TypeName { get; init; }
    public string? Name { get; init; }
    public int? Distance { get; init; }
    public bool IsAsteroid { get; init; }
    public bool IsShip { get; init; }
    public bool IsWreck { get; init; }
    public bool IsContainer { get; init; }
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

  public record KeyActivationWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? ActivateButton { get; init; }
    public string? ActivationCode { get; init; }
    public UITreeNodeWithDisplayRegion? InputField { get; init; }
    public UITreeNodeWithDisplayRegion? CancelButton { get; init; }
  }

  public record CompressionWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? CompressButton { get; init; }
    public WindowControls? WindowControls { get; init; }
    public IReadOnlyList<CompressionItem> Items { get; init; } = new List<CompressionItem>();
    public bool CanCompress { get; init; }
  }

  public record CompressionItem
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Name { get; init; }
    public int? Quantity { get; init; }
    public bool IsSelected { get; init; }
  }

  public record LocationsWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public IReadOnlyList<LocationFolder> Folders { get; init; } = new List<LocationFolder>();
    public IReadOnlyList<LocationBookmark> Bookmarks { get; init; } = new List<LocationBookmark>();
    public IReadOnlyList<LocationsWindowPlaceEntry> PlaceEntries { get; init; } = new List<LocationsWindowPlaceEntry>();
  }

  public record LocationFolder
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Name { get; init; }
    public bool IsExpanded { get; init; }
    public IReadOnlyList<LocationFolder> SubFolders { get; init; } = new List<LocationFolder>();
    public IReadOnlyList<LocationBookmark> Bookmarks { get; init; } = new List<LocationBookmark>();
  }

  public record LocationBookmark
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Name { get; init; }
    public string? Location { get; init; }
    public string? Notes { get; init; }
    public DateTime? Created { get; init; }
    public bool IsSelected { get; init; }
  }

  public record LocationsWindowPlaceEntry
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required string MainText { get; init; }
  }

  public record WindowControls
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? MinimizeButton { get; init; }
    public UITreeNodeWithDisplayRegion? CloseButton { get; init; }
  }

  // ==== SelectedItem ====
  public record SelectedItemWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public UITreeNodeWithDisplayRegion? OrbitButton { get; init; }
  }

  // ==== Fitting ====
  public record FittingWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public IReadOnlyList<FittingWindowFitting> Fittings { get; init; } = new List<FittingWindowFitting>();
    public UITreeNodeWithDisplayRegion? SaveButton { get; init; }
    public UITreeNodeWithDisplayRegion? SaveAsButton { get; init; }
    public UITreeNodeWithDisplayRegion? DeleteButton { get; init; }
    public UITreeNodeWithDisplayRegion? ImportButton { get; init; }
    public UITreeNodeWithDisplayRegion? ExportButton { get; init; }
  }

  public record FittingWindowFitting
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Name { get; init; }
    public bool IsSelected { get; init; }
    public bool IsHighlighted { get; init; }
    public bool IsExpanded { get; init; }
    public IReadOnlyList<FittingWindowFitting> Children { get; init; } = new List<FittingWindowFitting>();
  }

  // ==== Survey Scan ====
  public record SurveyScanWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required IReadOnlyList<SurveyScanResult> ScanEntries { get; init; }
  }

  public record SurveyScanResult
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? TypeName { get; init; }
    public int? Quantity { get; init; }
    public float? Distance { get; init; }
    public string? DistanceUnit { get; init; }
  }

  // ==== Repair Shop ====
  public record RepairShopWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public IReadOnlyList<RepairItem> Items { get; init; } = new List<RepairItem>();
    public UITreeNodeWithDisplayRegion? RepairAllButton { get; init; }
    public UITreeNodeWithDisplayRegion? ButtonGroup { get; init; }
    public IReadOnlyList<RepairShopWindowButton> Buttons { get; init; } = new List<RepairShopWindowButton>();
  }

  public record RepairItem
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Name { get; init; }
    public int? DamagePercent { get; init; }
    public decimal? RepairCost { get; init; }
    public bool IsSelected { get; init; }
  }

  public record RepairShopWindowButton
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? MainText { get; init; }
  }

  // ==== Character Sheet ====
  public record CharacterSheetWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? CharacterName { get; init; }
    public string? CorporationName { get; init; }
    public string? AllianceName { get; init; }
    public IReadOnlyList<CharacterSkill> Skills { get; init; } = new List<CharacterSkill>();
    public IReadOnlyList<CharacterAttribute> Attributes { get; init; } = new List<CharacterAttribute>();
    public IReadOnlyList<SkillGroupGauge> SkillGroups { get; init; } = new List<SkillGroupGauge>();
  }

  public record CharacterSkill
  {
    public string? Name { get; init; }
    public int Level { get; init; }
    public int? TrainedLevel { get; init; }
    public int? Points { get; init; }
    public int? PointsToNextLevel { get; init; }
    public TimeSpan? TimeToNextLevel { get; init; }
    public bool IsTraining { get; init; }
  }

  public record CharacterAttribute
  {
    public string? Name { get; init; }
    public int BaseValue { get; init; }
    public int ImplantBonus { get; init; }
  }

  public record SkillGroupGauge
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Name { get; init; }
    public int? TrainedSkillPoints { get; init; }
    public int? TotalSkillPoints { get; init; }
  }

  // ==== Drones ====
  public record DronesWindow
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public required DronesWindowDroneGroupHeader BayHeader { get; init; }
    public required IReadOnlyList<DronesWindowDrone> DronesInBay { get; init; }
    public required IReadOnlyList<DronesWindowDroneGroupHeader> DroneGroupsInBay { get; init; }
    public required DronesWindowDroneGroupHeader SpaceHeader { get; init; }
    public required IReadOnlyList<DronesWindowDrone> DronesInSpace { get; init; }
    public required IReadOnlyList<DronesWindowDroneGroupHeader> DroneGroupsInSpace { get; init; }
  }

  public record DronesWindowDroneGroupHeader
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public bool? IsCollapsed { get; init; }
    public string? MainText { get; init; }
    public int? Quantity { get; init; }
    public int? MaxQuantity { get; init; }
    public UITreeNodeWithDisplayRegion? LaunchButton { get; init; }
    public UITreeNodeWithDisplayRegion? EngageButton { get; init; }
    public UITreeNodeWithDisplayRegion? ReturnButton { get; init; }
    public UITreeNodeWithDisplayRegion? OrbitButton { get; init; }
  }

  public record DronesWindowDrone
  {
    public required UITreeNodeWithDisplayRegion UiNode { get; init; }
    public string? Type { get; init; }
    public string? Activity { get; init; }
    public Hitpoints? HitpointsPercent { get; init; }
    public UITreeNodeWithDisplayRegion? LaunchButton { get; init; }
    public UITreeNodeWithDisplayRegion? EngageButton { get; init; }
    public UITreeNodeWithDisplayRegion? ReturnButton { get; init; }
    public UITreeNodeWithDisplayRegion? OrbitButton { get; init; }
  }
}

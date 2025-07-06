using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EveOnline
{
    //public record ParsedUserInterface
    //{
    //    public IReadOnlyList<ContextMenu> ContextMenus { get; init; } = new List<ContextMenu>();
    //    public ShipUI ShipUI { get; init; }
    //    public IReadOnlyList<Target> Targets { get; init; } = new List<Target>();
    //    public InfoPanelContainer InfoPanelContainer { get; init; }

    //    public SelectedItemWindow SelectedItemWindow { get; init; }
    //    public DronesWindow DronesWindow { get; init; }
    //    public FittingWindow FittingWindow { get; init; }
    //    public ProbeScannerWindow ProbeScannerWindow { get; init; }
    //    public DirectionalScannerWindow DirectionalScannerWindow { get; init; }
    //    public StationWindow StationWindow { get; init; }
    //    public IReadOnlyList<InventoryWindow> InventoryWindows { get; init; } = new List<InventoryWindow>();
    //    public IReadOnlyList<ChatWindowStack> ChatWindowStacks { get; init; } = new List<ChatWindowStack>();
    //    public IReadOnlyList<AgentConversationWindow> AgentConversationWindows { get; init; } = new List<AgentConversationWindow>();
    //    public MarketOrdersWindow MarketOrdersWindow { get; init; }
    //    public SurveyScanWindow SurveyScanWindow { get; init; }
    //    public BookmarkLocationWindow BookmarkLocationWindow { get; init; }
    //    public RepairShopWindow RepairShopWindow { get; init; }
    //    public CharacterSheetWindow CharacterSheetWindow { get; init; }
    //    public FleetWindow FleetWindow { get; init; }
    //    public LocationsWindow LocationsWindow { get; init; }
    //    public WatchListPanel WatchListPanel { get; init; }
    //    public StandaloneBookmarkWindow StandaloneBookmarkWindow { get; init; }
    //    public ModuleButtonTooltip ModuleButtonTooltip { get; init; }
    //    public HeatStatusTooltip HeatStatusTooltip { get; init; }
    //    public Neocom Neocom { get; init; }
    //    public IReadOnlyList<MessageBox> MessageBoxes { get; init; } = new List<MessageBox>();
    //    public LayerAbovemain LayerAbovemain { get; init; }
    //    public KeyActivationWindow KeyActivationWindow { get; init; }
    //    public CompressionWindow CompressionWindow { get; init; }
    //}

    public record ContextMenu
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<ContextMenuEntry> Entries { get; init; } = new List<ContextMenuEntry>();
    }

    public record ContextMenuEntry
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Text { get; init; }
    }

    public record ShipUI
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public ShipUICapacitor Capacitor { get; init; }
        public Hitpoints HitpointsPercent { get; init; }
        public ShipUIIndication Indication { get; init; }
        public IReadOnlyList<ShipUIModuleButton> ModuleButtons { get; init; } = new List<ShipUIModuleButton>();
        public ShipUIModuleButtonsRows ModuleButtonsRows { get; init; }
        public IReadOnlyList<OffensiveBuffButton> OffensiveBuffButtons { get; init; } = new List<OffensiveBuffButton>();
        public SquadronsUI SquadronsUI { get; init; }
        public UITreeNodeWithDisplayRegion StopButton { get; init; }
        public UITreeNodeWithDisplayRegion MaxSpeedButton { get; init; }
        public ShipUIHeatGauges HeatGauges { get; init; }
    }

    public record ShipUIIndication
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public ShipManeuverType? ManeuverType { get; init; }
    }

    public record ShipUIModuleButton
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public UITreeNodeWithDisplayRegion SlotUINode { get; init; }
        public bool? IsActive { get; init; }
        public bool IsHiliteVisible { get; init; }
        public bool IsBusy { get; init; }
        public int? RampRotationMilli { get; init; }
    }

    public record ShipUIModuleButtonsRows
    {
        public IReadOnlyList<ShipUIModuleButton> Top { get; init; } = new List<ShipUIModuleButton>();
        public IReadOnlyList<ShipUIModuleButton> Middle { get; init; } = new List<ShipUIModuleButton>();
        public IReadOnlyList<ShipUIModuleButton> Bottom { get; init; } = new List<ShipUIModuleButton>();
    }

    public record OffensiveBuffButton
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
    }

    public record ShipUICapacitor
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<ShipUICapacitorPmark> PMarks { get; init; } = new List<ShipUICapacitorPmark>();
        public int? LevelFromPMarksPercent { get; init; }
    }

    public record ShipUICapacitorPmark
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public ColorComponents ColorPercent { get; init; }
    }

    public record ColorComponents(float R, float G, float B, float A);

    public record ShipUIHeatGauges
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<ShipUIHeatGauge> Gauges { get; init; } = new List<ShipUIHeatGauge>();
    }

    public record ShipUIHeatGauge
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public int? RotationPercent { get; init; }
        public int? HeatPercent { get; init; }
    }

    public record Hitpoints(int Structure, int Armor, int Shield);

    public enum ShipManeuverType
    {
        Warp,
        Jump,
        Orbit,
        Approach
    }

    public record SquadronsUI
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<SquadronUI> Squadrons { get; init; } = new List<SquadronUI>();
    }

    public record SquadronUI
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<SquadronAbilityIcon> Abilities { get; init; } = new List<SquadronAbilityIcon>();
        public UITreeNodeWithDisplayRegion ActionLabel { get; init; }
    }

    public record SquadronAbilityIcon
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public int? Quantity { get; init; }
        public bool? RampActive { get; init; }
    }

    public record InfoPanelContainer
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public InfoPanelIcons Icons { get; init; }
        public InfoPanelLocationInfo InfoPanelLocationInfo { get; init; }
        public InfoPanelRoute InfoPanelRoute { get; init; }
    }

    public record InfoPanelIcons
    {
        public IReadOnlyList<InfoPanelIcon> Icons { get; init; } = new List<InfoPanelIcon>();
    }

    public record InfoPanelIcon
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Hint { get; init; }
        public string IconId { get; init; }
    }

    public record InfoPanelLocationInfo
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string CurrentSolarSystemName { get; init; }
        public string CurrentDockedStationName { get; init; }
        public string CurrentStationName { get; init; }
    }

    public record InfoPanelRoute
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<RouteElementMarker> RouteElementMarker { get; init; } = new List<RouteElementMarker>();
    }

    public record RouteElementMarker
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Text { get; init; }
    }

    public record Target
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public bool IsActiveTarget { get; init; }
        public bool IsHighlighted { get; init; }
        public string[] Texts { get; init; }
        public int? BarPercent { get; init; }
        public ColorComponents? BarColor { get; init; }
        public string[] Icons { get; init; }
        public string[] IconsWithHints { get; init; }
        public bool IsExpanded { get; init; }
        public bool IsSelected { get; init; }
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
    }



    public record SelectedItemWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<SelectedItemWindowEntry> Entries { get; init; } = new List<SelectedItemWindowEntry>();
    }

    public record SelectedItemWindowEntry
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Text { get; init; }
    }

    public static class Parser
    {
        //public static ParsedUserInterface ParseUserInterface(UITreeNode uiTreeRoot)
        //{
        //    if (uiTreeRoot == null)
        //        throw new ArgumentNullException(nameof(uiTreeRoot));

        //    var uiTreeRootWithDisplayRegion = AsUITreeNodeWithDisplayRegion(uiTreeRoot);

        //    return new ParsedUserInterface
        //    {
        //        UiTree = uiTreeRootWithDisplayRegion,
        //        ContextMenus = ParseContextMenusFromUITreeRoot(uiTreeRootWithDisplayRegion).ToList(),
        //        ShipUI = ParseShipUIFromUITreeRoot(uiTreeRootWithDisplayRegion),
        //        DronesWindow = ParseDronesWindowFromUITreeRoot(uiTreeRootWithDisplayRegion),
        //        OverviewWindows = ParseOverviewWindowsFromUITreeRoot(uiTreeRootWithDisplayRegion).ToList(),
        //        SelectedItemWindow = ParseSelectedItemWindowFromUITreeRoot(uiTreeRootWithDisplayRegion),
        //        CharacterSheetWindow = ParseCharacterSheetWindowFromUITreeRoot(uiTreeRootWithDisplayRegion),
        //        ModuleButtonTooltip = ParseModuleButtonTooltipFromUITreeRoot(uiTreeRootWithDisplayRegion),
        //        HeatStatusTooltip = ParseHeatStatusTooltipFromUITreeRoot(uiTreeRootWithDisplayRegion),
        //        FleetWindow = ParseFleetWindowFromUITreeRoot(uiTreeRootWithDisplayRegion),
        //        ProbeScannerWindow = ParseProbeScannerWindowFromUITreeRoot(uiTreeRootWithDisplayRegion),
        //        DirectionalScannerWindow = ParseDirectionalScannerWindowFromUITreeRoot(uiTreeRootWithDisplayRegion),
        //        StationWindow = ParseStationWindowFromUITreeRoot(uiTreeRootWithDisplayRegion),
        //        InventoryWindows = ParseInventoryWindowsFromUITreeRoot(uiTreeRootWithDisplayRegion).ToList(),
        //        ChatWindowStacks = ParseChatWindowStacksFromUITreeRoot(uiTreeRootWithDisplayRegion).ToList(),
        //        AgentConversationWindows = ParseAgentConversationWindowsFromUITreeRoot(uiTreeRootWithDisplayRegion).ToList()
        //    };
        //}


        private static DisplayRegion CalculateVisibleDisplayRegion(DisplayRegion selfRegion, IReadOnlyList<ChildOfNodeWithDisplayRegion> children)
        {
            // Simplified implementation - would need to account for clipping, scrolling, etc.
            return selfRegion;
        }



        public static IEnumerable<UITreeNodeWithDisplayRegion> ListDescendantsWithDisplayRegion(UITreeNodeWithDisplayRegion node)
        {
            if (node == null) yield break;

            foreach (var child in node.Children ?? Enumerable.Empty<ChildOfNodeWithDisplayRegion>())
            {
                if (child is ChildWithRegion cwr)
                {
                    yield return cwr.Node;

                    foreach (var descendant in ListDescendantsWithDisplayRegion(cwr.Node))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        private static IEnumerable<ContextMenu> ParseContextMenusFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            if (uiTreeRoot == null) yield break;

            var contextMenuNodes = ListDescendantsWithDisplayRegion(uiTreeRoot)
                .Where(n => n.UiNode.PythonObjectTypeName == "LayeredMenu");

            foreach (var menuNode in contextMenuNodes)
            {
                var entries = menuNode.Children?
                    .OfType<ChildWithRegion>()
                    .Select(cwr => cwr.Node)
                    .Where(n => n.UiNode.PythonObjectTypeName == "LayeredMenuEntry")
                    .Select(ParseContextMenuEntry)
                    .Where(e => e != null)
                    .ToList() ?? new List<ContextMenuEntry>();

                if (entries.Any())
                {
                    yield return new ContextMenu
                    {
                        UiNode = menuNode,
                        Entries = entries
                    };
                }
            }
        }

        private static ContextMenuEntry ParseContextMenuEntry(UITreeNodeWithDisplayRegion node)
        {
            var textNode = node.Children?
                .OfType<ChildWithRegion>()
                .Select(cwr => cwr.Node)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "EveLabelMedium");

            if (textNode == null) return null;

            var text = textNode.UiNode.DictEntriesOfInterest?.GetValueOrDefault("text") as string;
            if (string.IsNullOrEmpty(text)) return null;

            return new ContextMenuEntry
            {
                UiNode = node,
                Text = text.Trim()
            };
        }

        private static ShipUI ParseShipUIFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            var shipUINode = ListDescendantsWithDisplayRegion(uiTreeRoot)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "ShipUI");

            if (shipUINode == null) return null;

            var capacitorNode = shipUINode.Children?
                .OfType<ChildWithRegion>()
                .Select(cwr => cwr.Node)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "CapacitorContainer");

            var capacitor = capacitorNode != null ? ParseShipUICapacitor(capacitorNode) : null;

            var moduleButtons = ParseShipUIModuleButtons(shipUINode);

            return new ShipUI
            {
                UiNode = shipUINode,
                Capacitor = capacitor,
                ModuleButtons = moduleButtons,
                ModuleButtonsRows = GroupModuleButtonsByRow(moduleButtons),
                // Other properties would be parsed here
            };
        }

        private static ShipUICapacitor ParseShipUICapacitor(UITreeNodeWithDisplayRegion capacitorNode)
        {
            var pmarks = capacitorNode.Children?
                .OfType<ChildWithRegion>()
                .Select(cwr => cwr.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "CapacitorPmark")
                .Select(pm => new ShipUICapacitorPmark
                {
                    UiNode = pm,
                    ColorPercent = GetColorPercentFromDict(pm.UiNode.DictEntriesOfInterest)
                })
                .ToList() ?? new List<ShipUICapacitorPmark>();

            var levelFromPmarks = pmarks
                .Select(pm => (int?)(pm.ColorPercent?.R * 100))
                .LastOrDefault();

            return new ShipUICapacitor
            {
                UiNode = capacitorNode,
                PMarks = pmarks,
                LevelFromPMarksPercent = levelFromPmarks
            };
        }

        private static ColorComponents GetColorPercentFromDict(IReadOnlyDictionary<string, object> dict)
        {
            if (dict == null || !dict.TryGetValue("_color", out var colorObj))
                return null;

            // Parse color from dictionary (format depends on how it's stored)
            // This is a simplified implementation
            return new ColorComponents(1f, 1f, 1f, 1f);
        }

        private static IReadOnlyList<ShipUIModuleButton> ParseShipUIModuleButtons(UITreeNodeWithDisplayRegion shipUINode)
        {
            var moduleButtons = new List<ShipUIModuleButton>();

            // Find module buttons in the ship UI
            var slotNodes = ListDescendantsWithDisplayRegion(shipUINode)
                .Where(n => n.UiNode.PythonObjectTypeName == "ModuleButton");

            foreach (var slotNode in slotNodes)
            {
                var moduleButton = ParseShipUIModuleButton(slotNode);
                if (moduleButton != null)
                {
                    moduleButtons.Add(moduleButton);
                }
            }

            return moduleButtons;
        }

        private static ShipUIModuleButton ParseShipUIModuleButton(UITreeNodeWithDisplayRegion slotNode)
        {
            var buttonNode = slotNode.Children?
                .OfType<ChildWithRegion>()
                .Select(cwr => cwr.Node)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "ModuleButton");

            if (buttonNode == null) return null;

            var isActive = buttonNode.UiNode.DictEntriesOfInterest?.ContainsKey("isActive") == true;
            var isHiliteVisible = buttonNode.Children?.Any(c =>
                c is ChildWithRegion cwr &&
                cwr.Node.UiNode.PythonObjectTypeName == "Hilite" &&
                cwr.Node.SelfDisplayRegion.Width > 0 &&
                cwr.Node.SelfDisplayRegion.Height > 0) == true;

            var isBusy = buttonNode.Children?.Any(c =>
                c is ChildWithRegion cwr &&
                cwr.Node.UiNode.PythonObjectTypeName == "Busy" &&
                cwr.Node.SelfDisplayRegion.Width > 0) == true;

            return new ShipUIModuleButton
            {
                UiNode = buttonNode,
                SlotUINode = slotNode,
                IsActive = isActive,
                IsHiliteVisible = isHiliteVisible,
                IsBusy = isBusy,
                // Ramp rotation would be parsed from animation state if available
            };
        }

        private static ShipUIModuleButtonsRows GroupModuleButtonsByRow(IReadOnlyList<ShipUIModuleButton> buttons)
        {
            // Group buttons by their vertical position to determine rows
            var groups = buttons
                .GroupBy(b => b.SlotUINode.SelfDisplayRegion.Y / 10) // Group by approximate Y position
                .OrderBy(g => g.Key)
                .ToList();

            return new ShipUIModuleButtonsRows
            {
                Top = groups.Count > 0 ? groups[0].ToList() : new List<ShipUIModuleButton>(),
                Middle = groups.Count > 1 ? groups[1].ToList() : new List<ShipUIModuleButton>(),
                Bottom = groups.Count > 2 ? groups[2].ToList() : new List<ShipUIModuleButton>()
            };
        }


        private static DronesWindow ParseDronesWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            var dronesWindowNode = ListDescendantsWithDisplayRegion(uiTreeRoot)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "DronesWindow");

            if (dronesWindowNode == null) return null;

            var entries = new List<DronesWindowEntry>();

            // Parse drone groups and individual drones from the window
            var groupNodes = dronesWindowNode.Children?
                .OfType<ChildWithRegion>()
                .Select(cwr => cwr.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "DroneGroup");

            if (groupNodes != null)
            {
                foreach (var groupNode in groupNodes)
                {
                    var groupName = groupNode.UiNode.DictEntriesOfInterest?.GetValueOrDefault("name") as string;
                    var isExpanded = groupNode.UiNode.DictEntriesOfInterest?.ContainsKey("isExpanded") == true;

                    entries.Add(new DronesWindowEntryGroup
                    {
                        UiNode = groupNode,
                        GroupName = groupName ?? "Unknown Group",
                        IsExpanded = isExpanded,
                        Count = groupNode.Children?.Count ?? 0
                    });

                    // Add individual drones in this group
                    var droneNodes = groupNode.Children?
                        .OfType<ChildWithRegion>()
                        .Select(cwr => cwr.Node)
                        .Where(n => n.UiNode.PythonObjectTypeName == "DroneEntry");

                    if (droneNodes != null)
                    {
                        foreach (var droneNode in droneNodes)
                        {
                            var droneName = droneNode.UiNode.DictEntriesOfInterest?.GetValueOrDefault("name") as string;
                            var status = droneNode.UiNode.DictEntriesOfInterest?.GetValueOrDefault("status") as string;

                            entries.Add(new DronesWindowEntryDrone
                            {
                                UiNode = droneNode,
                                TypeName = droneName ?? "Unknown Drone",
                                Status = status ?? "Unknown",
                                // Parse other drone properties as needed
                            });
                        }
                    }
                }
            }

            return new DronesWindow
            {
                UiNode = dronesWindowNode,
                Entries = entries,
                // Parse other window properties as needed
            };
        }


        private static IEnumerable<InventoryItem> ParseInventoryItems(UITreeNodeWithDisplayRegion inventoryNode)
        {
            return inventoryNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "InventoryItem")
                .Select(itemNode => new InventoryItem
                {
                    UiNode = itemNode,
                    Name = GetStringFromDict(itemNode.UiNode.DictEntriesOfInterest, "name"),
                    TypeName = GetStringFromDict(itemNode.UiNode.DictEntriesOfInterest, "typeName"),
                    Quantity = GetIntFromDict(itemNode.UiNode.DictEntriesOfInterest, "quantity") ?? 1,
                    IsSelected = GetBoolFromDict(itemNode.UiNode.DictEntriesOfInterest, "isSelected") ?? false,
                    IsHighlighted = GetBoolFromDict(itemNode.UiNode.DictEntriesOfInterest, "isHighlighted") ?? false,
                    IsActive = GetBoolFromDict(itemNode.UiNode.DictEntriesOfInterest, "isActive") ?? false
                });
        }

        private static IEnumerable<InventoryWindow> ParseInventoryWindowsFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            return uiTreeRoot.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "InventoryWindow")
                .Select(ParseInventoryWindow);
        }

        private static InventoryWindow ParseInventoryWindow(UITreeNodeWithDisplayRegion windowNode)
        {
            return new InventoryWindow
            {
                UiNode = windowNode,
                SelectedContainerCapacityGauge = ParseSelectedContainerCapacityGauge(windowNode),
                LeftTreeList = ParseLeftTreeList(windowNode),
                SelectedContainer = ParseSelectedContainer(windowNode)
            };
        }

        private static UITreeNodeWithDisplayRegion ParseSelectedContainerCapacityGauge(UITreeNodeWithDisplayRegion windowNode)
        {
            return windowNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "SelectedContainerCapacityGauge");
        }

        private static UITreeNodeWithDisplayRegion ParseLeftTreeList(UITreeNodeWithDisplayRegion windowNode)
        {
            return windowNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "LeftTreeList");
        }

        private static UITreeNodeWithDisplayRegion ParseSelectedContainer(UITreeNodeWithDisplayRegion windowNode)
        {
            return windowNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "SelectedContainer");
        }

        private static IEnumerable<ChatWindowStack> ParseChatWindowStacksFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            return uiTreeRoot.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "ChatWindowStack")
                .Select(ParseChatWindowStack);
        }

        private static ChatWindowStack ParseChatWindowStack(UITreeNodeWithDisplayRegion stackNode)
        {
            var chatWindowNode = stackNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "XmppChatWindow");

            return new ChatWindowStack
            {
                UiNode = stackNode,
                ChatWindow = chatWindowNode != null ? ParseChatWindow(chatWindowNode) : null
            };
        }

        private static ChatWindow ParseChatWindow(UITreeNodeWithDisplayRegion chatWindowNode)
        {
            var userlistNode = chatWindowNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .FirstOrDefault(n =>
                    n.UiNode.DictEntriesOfInterest
                        .TryGetValue("name", out var name) &&
                    name?.ToString()?.ToLower().Contains("userlist") == true);

            return new ChatWindow
            {
                UiNode = chatWindowNode,
                Name = GetStringFromDict(chatWindowNode.UiNode.DictEntriesOfInterest, "name"),
                Userlist = userlistNode != null ? ParseChatWindowUserlist(userlistNode) : null
            };
        }

        private static ChatWindowUserlist ParseChatWindowUserlist(UITreeNodeWithDisplayRegion userlistNode)
        {
            var visibleUsers = userlistNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "XmppChatSimpleUserEntry" ||
                           n.UiNode.PythonObjectTypeName == "XmppChatUserEntry")
                .Select(ParseChatUserEntry)
                .ToList();

            var scrollControls = userlistNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName?.Contains("ScrollControls") == true);

            return new ChatWindowUserlist
            {
                UiNode = userlistNode,
                VisibleUsers = visibleUsers,
                ScrollControls = scrollControls != null ? ParseScrollControls(scrollControls) : null
            };
        }

        private static ChatUserEntry ParseChatUserEntry(UITreeNodeWithDisplayRegion userEntryNode)
        {
            var standingIconNode = userEntryNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "EveIcon");

            return new ChatUserEntry
            {
                UiNode = userEntryNode,
                Name = GetStringFromDict(userEntryNode.UiNode.DictEntriesOfInterest, "text"),
                StandingIcon = standingIconNode != null ?
                    GetStringFromDict(standingIconNode.UiNode.DictEntriesOfInterest, "spriteName") : null
            };
        }

        private static ScrollControls ParseScrollControls(UITreeNodeWithDisplayRegion scrollControlsNode)
        {
            var buttons = scrollControlsNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "Button")
                .ToList();

            return new ScrollControls
            {
                UiNode = scrollControlsNode,
                CanScrollUp = buttons.Count > 0 &&
                    (GetBoolFromDict(buttons[0].UiNode.DictEntriesOfInterest, "isEnabled") ?? false),
                CanScrollDown = buttons.Count > 1 &&
                    (GetBoolFromDict(buttons[1].UiNode.DictEntriesOfInterest, "isEnabled") ?? false)
            };
        }



        private static IEnumerable<AgentConversationWindow> ParseAgentConversationWindowsFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            return uiTreeRoot.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "AgentConversationWindow")
                .Select(ParseAgentConversationWindow);
        }

        private static AgentConversationWindow ParseAgentConversationWindow(UITreeNodeWithDisplayRegion windowNode)
        {
            return new AgentConversationWindow
            {
                UiNode = windowNode,
                AgentName = GetStringFromDict(windowNode.UiNode.DictEntriesOfInterest, "agentName"),
                AgentCorporation = GetStringFromDict(windowNode.UiNode.DictEntriesOfInterest, "agentCorporation"),
                Messages = ParseAgentConversationMessages(windowNode).ToList(),
                Responses = ParseAgentConversationResponses(windowNode).ToList()
            };
        }

        private static IEnumerable<AgentConversationMessage> ParseAgentConversationMessages(UITreeNodeWithDisplayRegion windowNode)
        {
            return windowNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "AgentConversationMessage")
                .Select(messageNode => new AgentConversationMessage
                {
                    UiNode = messageNode,
                    Text = GetStringFromDict(messageNode.UiNode.DictEntriesOfInterest, "text"),
                    IsFromAgent = GetBoolFromDict(messageNode.UiNode.DictEntriesOfInterest, "isFromAgent") ?? false,
                    Timestamp = GetStringFromDict(messageNode.UiNode.DictEntriesOfInterest, "timestamp")
                });
        }

        private static IEnumerable<AgentConversationResponse> ParseAgentConversationResponses(UITreeNodeWithDisplayRegion windowNode)
        {
            return windowNode.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "AgentConversationResponse")
                .Select(responseNode => new AgentConversationResponse
                {
                    UiNode = responseNode,
                    Text = GetStringFromDict(responseNode.UiNode.DictEntriesOfInterest, "text"),
                    IsHighlighted = GetBoolFromDict(responseNode.UiNode.DictEntriesOfInterest, "isHighlighted") ?? false
                });
        }

        private static ModuleButtonTooltip ParseModuleButtonTooltipFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            var tooltipNode = uiTreeRoot.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "ModuleButtonTooltip");

            if (tooltipNode == null)
                return null;

            return ParseModuleButtonTooltip(tooltipNode);
        }

        private static ModuleButtonTooltip ParseModuleButtonTooltip(UITreeNodeWithDisplayRegion tooltipUINode)
        {
            Vector2 UpperRightCornerFromDisplayRegion(DisplayRegion region)
            {
                return new Vector2(region.X + region.Width, region.Y);
            }

            float DistanceSquared(Vector2 a, Vector2 b)
            {
                var distanceX = a.X - b.X;
                var distanceY = a.Y - b.Y;
                return distanceX * distanceX + distanceY * distanceY;
            }

            var allTexts = GetAllContainedDisplayTextsWithRegion(tooltipUINode);

            // Find the shortcut text (closest text to upper right corner)
            var shortcutCandidates = allTexts
                .Select(t => new
                {
                    Text = t.Text,
                    DistanceSquared = DistanceSquared(
                        UpperRightCornerFromDisplayRegion(t.Region),
                        UpperRightCornerFromDisplayRegion(tooltipUINode.TotalDisplayRegion))
                })
                .OrderBy(x => x.DistanceSquared)
                .ToList();

            var shortcut = shortcutCandidates
                .FirstOrDefault(c => c.DistanceSquared < 1000)
                ?.Text;

            // Parse optimal range if present
            var optimalRangeString = GetAllContainedDisplayTexts(tooltipUINode.UiNode)
                .Select(text =>
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        text,
                        @"Optimal range \(\|within\)\s*([\d\.]+\s*[km]+)",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    return match.Success ? match.Groups[1].Value.Trim() : null;
                })
                .FirstOrDefault(x => x != null);

            var optimalRange = optimalRangeString != null ? new ModuleButtonTooltip.OptimalRange
            {
                AsString = optimalRangeString,
                InMeters = ParseOverviewEntryDistanceInMetersFromText(optimalRangeString)
            } : null;

            return new ModuleButtonTooltip
            {
                UiNode = tooltipUINode,
                Shortcut = shortcut,
                OptimalRange = optimalRange
            };
        }

        private static double? ParseOverviewEntryDistanceInMetersFromText(string distanceText)
        {
            // This is a simplified implementation. The actual implementation would need to handle
            // various distance formats like "10 km", "5,000 m", etc.
            if (string.IsNullOrWhiteSpace(distanceText))
                return null;

            // Extract the numeric part
            var match = System.Text.RegularExpressions.Regex.Match(distanceText, @"([\d\.,]+)");
            if (!match.Success)
                return null;

            if (!double.TryParse(match.Groups[1].Value, out var value))
                return null;

            // Convert to meters based on unit
            if (distanceText.IndexOf("km", StringComparison.OrdinalIgnoreCase) >= 0)
                return value * 1000; // Convert km to m

            return value; // Assume it's already in meters
        }

        private static HeatStatusTooltip ParseHeatStatusTooltipFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            var tooltipNode = uiTreeRoot.Children
                .OfType<ChildWithRegion>()
                .Select(c => c.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "TooltipPanel")
                .FirstOrDefault(n =>
                {
                    var texts = GetAllContainedDisplayTextsWithRegion(n)
                        .OrderBy(t => t.Region.Y)
                        .Select(t => t.Text)
                        .ToList();
                    return texts.Any() && texts[0].Contains("Heat Status");
                });

            if (tooltipNode == null)
                return null;

            return ParseHeatStatusTooltip(tooltipNode);
        }

        private static HeatStatusTooltip ParseHeatStatusTooltip(UITreeNodeWithDisplayRegion tooltipNode)
        {
            var allTexts = GetAllContainedDisplayTexts(tooltipNode.UiNode)
                .Select(t => t.Trim())
                .ToList();

            int? ParsePercentFromPrefix(string prefix)
            {
                return allTexts
                    .Where(t => t.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Select(t => t.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Skip(1)
                        .FirstOrDefault()
                        ?.TrimEnd('%'))
                    .FirstOrDefault()
                    ?.ParseInt();
            }

            return new HeatStatusTooltip
            {
                UiNode = tooltipNode,
                LowPercent = ParsePercentFromPrefix("low"),
                MediumPercent = ParsePercentFromPrefix("medium"),
                HighPercent = ParsePercentFromPrefix("high"),
                // Additional properties can be parsed here as needed
                HeatLevel = ParsePercentFromPrefix("heat level"),
                HeatLevelText = allTexts.FirstOrDefault(t => t.Contains("Heat Level:", StringComparison.OrdinalIgnoreCase)),
                HeatDamage = allTexts.FirstOrDefault(t => t.Contains("Heat Damage:", StringComparison.OrdinalIgnoreCase)),
                HeatDamageBonus = allTexts.FirstOrDefault(t => t.Contains("Damage Bonus:", StringComparison.OrdinalIgnoreCase)),
                OverloadBonus = allTexts.FirstOrDefault(t => t.Contains("Overload Bonus:", StringComparison.OrdinalIgnoreCase)),
                OverloadSelfBonus = allTexts.FirstOrDefault(t => t.Contains("Overload Self Bonus:", StringComparison.OrdinalIgnoreCase)),
                OverloadDurationBonus = allTexts.FirstOrDefault(t => t.Contains("Overload Duration Bonus:", StringComparison.OrdinalIgnoreCase))
            };
        }

        private static SelectedItemWindow ParseSelectedItemWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            var selectedItemNode = ListDescendantsWithDisplayRegion(uiTreeRoot)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "SelectedItemWindow");

            if (selectedItemNode == null) return null;

            var entries = selectedItemNode.Children?
                .OfType<ChildWithRegion>()
                .Select(cwr => cwr.Node)
                .Where(n => n.UiNode.PythonObjectTypeName == "SelectedItemEntry")
                .Select(e => new SelectedItemWindowEntry
                {
                    UiNode = e,
                    Text = e.UiNode.DictEntriesOfInterest?.GetValueOrDefault("text") as string ?? string.Empty
                })
                .ToList() ?? new List<SelectedItemWindowEntry>();

            return new SelectedItemWindow
            {
                UiNode = selectedItemNode,
                Entries = entries
            };
        }
    }


    public record DronesWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<DronesWindowEntry> Entries { get; init; } = new List<DronesWindowEntry>();
        public UITreeNodeWithDisplayRegion HeaderContainer { get; init; }
        public UITreeNodeWithDisplayRegion DroneGroupInLocal { get; init; }
        public UITreeNodeWithDisplayRegion DroneGroupInBay { get; init; }
        public UITreeNodeWithDisplayRegion DroneGroupInSpace { get; init; }
    }

    public abstract record DronesWindowEntry;

    public record DronesWindowEntryGroup : DronesWindowEntry
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string GroupName { get; init; }
        public bool IsExpanded { get; init; }
        public int? Count { get; init; }
    }

    public record DronesWindowEntryDrone : DronesWindowEntry
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string TypeName { get; init; }
        public string Status { get; init; }
        public int? ShieldPercent { get; init; }
        public int? ArmorPercent { get; init; }
        public int? HullPercent { get; init; }
        public int? TargetDistance { get; init; }
        public string TargetName { get; init; }
        public bool IsActive { get; init; }
        public bool IsInLocal { get; init; }
        public bool IsInBay { get; init; }
        public bool IsInSpace { get; init; }
        public bool IsEngaging { get; init; }
        public bool IsMining { get; init; }
        public bool IsEngaged { get; init; }
        public bool IsReturning { get; init; }
        public bool IsInDanger { get; init; }
    }

    public record FittingWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<FittingWindowFitting> Fittings { get; init; } = new List<FittingWindowFitting>();
        public UITreeNodeWithDisplayRegion SaveButton { get; init; }
        public UITreeNodeWithDisplayRegion SaveAsButton { get; init; }
        public UITreeNodeWithDisplayRegion DeleteButton { get; init; }
        public UITreeNodeWithDisplayRegion ImportButton { get; init; }
        public UITreeNodeWithDisplayRegion ExportButton { get; init; }
    }

    public record FittingWindowFitting
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public bool IsSelected { get; init; }
        public bool IsHighlighted { get; init; }
        public bool IsExpanded { get; init; }
        public IReadOnlyList<FittingWindowFitting> Children { get; init; } = new List<FittingWindowFitting>();
    }

    public record ProbeScannerWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<ProbeScanResult> ScanResults { get; init; } = new List<ProbeScanResult>();
        public UITreeNodeWithDisplayRegion ScanButton { get; init; }
        public UITreeNodeWithDisplayRegion StopButton { get; init; }
        public bool IsScanning { get; init; }
    }

    public record ProbeScanResult
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string TypeName { get; init; }
        public string Name { get; init; }
        public int? SignalStrength { get; init; }
        public float? Distance { get; init; }
        public string DistanceUnit { get; init; }
        public bool IsSelected { get; init; }
        public bool IsHighlighted { get; init; }
    }

    public record DirectionalScannerWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public UITreeNodeWithDisplayRegion ScanButton { get; init; }
        public UITreeNodeWithDisplayRegion RangeDropdown { get; init; }
        public UITreeNodeWithDisplayRegion RangeText { get; init; }
        public IReadOnlyList<DirectionalScanResult> ScanResults { get; init; } = new List<DirectionalScanResult>();
    }

    public record DirectionalScanResult
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string TypeName { get; init; }
        public string Name { get; init; }
        public int? Distance { get; init; }
        public bool IsAsteroid { get; init; }
        public bool IsShip { get; init; }
        public bool IsWreck { get; init; }
        public bool IsContainer { get; init; }
    }

    public record StationWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<StationServiceButton> ServiceButtons { get; init; } = new List<StationServiceButton>();
        public StationInventory Inventory { get; init; }
    }

    public record StationServiceButton
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public bool IsActive { get; init; }
    }

    public record StationInventory
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<InventoryItem> Items { get; init; } = new List<InventoryItem>();
        public string CapacityText { get; init; }
    }

    public record InventoryItem
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public int? Quantity { get; init; }
        public bool IsSelected { get; init; }
        public bool IsHighlighted { get; init; }
    }

    public record ChatWindowStack
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public ChatWindow ChatWindow { get; init; }
    }

    public record ChatWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public ChatWindowUserlist Userlist { get; init; }
    }

    public record ChatWindowUserlist
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<ChatUserEntry> VisibleUsers { get; init; } = new List<ChatUserEntry>();
        public ScrollControls ScrollControls { get; init; }
    }

    public record ChatUserEntry
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public string StandingIcon { get; init; }
    }

    public record ScrollControls
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public bool CanScrollUp { get; init; }
        public bool CanScrollDown { get; init; }
    }

    public record ChatMessage
    {
        public string Sender { get; init; }
        public string Text { get; init; }
        public DateTime Timestamp { get; init; }
        public bool IsLocal { get; init; }
        public bool IsPrivate { get; init; }
        public bool IsCorp { get; init; }
        public bool IsAlliance { get; init; }
    }

    public record AgentConversationWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string AgentName { get; init; }
        public string AgentCorporation { get; init; }
        public IReadOnlyList<AgentConversationMessage> Messages { get; init; } = new List<AgentConversationMessage>();
        public UITreeNodeWithDisplayRegion InputBox { get; init; }
        public IReadOnlyList<AgentConversationResponse> Responses { get; init; } = new List<AgentConversationResponse>();
    }

    public record AgentConversationMessage
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Text { get; init; }
        public bool IsFromAgent { get; init; }
        public string Timestamp { get; init; }
    }

    public record AgentConversationResponse
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Text { get; init; }
        public bool IsHighlighted { get; init; }
    }

    public record MarketOrdersWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<MarketOrder> Orders { get; init; } = new List<MarketOrder>();
        public string Title { get; init; }
    }

    public record MarketOrder
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string ItemName { get; init; }
        public int? Quantity { get; init; }
        public decimal? Price { get; init; }
        public string Location { get; init; }
        public string Range { get; init; }
        public string Duration { get; init; }
        public bool IsBuyOrder { get; init; }
        public bool IsSellOrder { get; init; }
    }

    public record SurveyScanWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<SurveyScanResult> Results { get; init; } = new List<SurveyScanResult>();
    }

    public record SurveyScanResult
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string TypeName { get; init; }
        public int? Quantity { get; init; }
        public float? Distance { get; init; }
        public string DistanceUnit { get; init; }
    }

    public record BookmarkLocationWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public UITreeNodeWithDisplayRegion NameInput { get; init; }
        public UITreeNodeWithDisplayRegion NotesInput { get; init; }
        public UITreeNodeWithDisplayRegion CreateButton { get; init; }
        public UITreeNodeWithDisplayRegion CancelButton { get; init; }
    }

    public record RepairShopWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<RepairItem> Items { get; init; } = new List<RepairItem>();
        public UITreeNodeWithDisplayRegion RepairAllButton { get; init; }
    }

    public record RepairItem
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public int? DamagePercent { get; init; }
        public decimal? RepairCost { get; init; }
        public bool IsSelected { get; init; }
    }

    public record SkillGroupGauge
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public int? TrainedSkillPoints { get; init; }
        public int? TotalSkillPoints { get; init; }
    }

    public record CharacterSheetWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string CharacterName { get; init; }
        public string CorporationName { get; init; }
        public string AllianceName { get; init; }
        public IReadOnlyList<CharacterSkill> Skills { get; init; } = new List<CharacterSkill>();
        public IReadOnlyList<CharacterAttribute> Attributes { get; init; } = new List<CharacterAttribute>();
        public IReadOnlyList<SkillGroupGauge> SkillGroups { get; init; } = new List<SkillGroupGauge>();
    }

    public record CharacterSkill
    {
        public string Name { get; init; }
        public int Level { get; init; }
        public int? TrainedLevel { get; init; }
        public int? Points { get; init; }
        public int? PointsToNextLevel { get; init; }
        public TimeSpan? TimeToNextLevel { get; init; }
        public bool IsTraining { get; init; }
    }

    public record CharacterAttribute
    {
        public string Name { get; init; }
        public int BaseValue { get; init; }
        public int ImplantBonus { get; init; }
    }

    public record InventoryWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<InventoryWindowLeftTreeEntry> LeftTreeEntries { get; init; } = new List<InventoryWindowLeftTreeEntry>();
        public string SubCaptionLabelText { get; init; }
        public Result<InventoryWindowCapacityGauge, string> SelectedContainerCapacityGauge { get; init; }
        public Inventory SelectedContainerInventory { get; init; }
        public UITreeNodeWithDisplayRegion ButtonToSwitchToListView { get; init; }
        public UITreeNodeWithDisplayRegion ButtonToStackAll { get; init; }
    }

    public record Inventory
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public InventoryItemsView ItemsView { get; init; }
        public ScrollControls ScrollControls { get; init; }
    }

    public abstract record InventoryItemsView;

    public record InventoryItemsListView : InventoryItemsView
    {
        public IReadOnlyList<InventoryItemsListViewEntry> Items { get; init; } = new List<InventoryItemsListViewEntry>();
    }

    public record InventoryItemsNotListView : InventoryItemsView
    {
        public IReadOnlyList<UITreeNodeWithDisplayRegion> Items { get; init; } = new List<UITreeNodeWithDisplayRegion>();
    }

    public record InventoryWindowLeftTreeEntry
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public UITreeNodeWithDisplayRegion ToggleButton { get; init; }
        public UITreeNodeWithDisplayRegion SelectRegion { get; init; }
        public string Text { get; init; }
        public IReadOnlyList<InventoryWindowLeftTreeEntryChild> Children { get; init; } = new List<InventoryWindowLeftTreeEntryChild>();
    }

    public abstract record InventoryWindowLeftTreeEntryChild;

    public record InventoryWindowLeftTreeEntryChildNode : InventoryWindowLeftTreeEntryChild
    {
        public InventoryWindowLeftTreeEntry Entry { get; init; }
    }

    public record InventoryWindowCapacityGauge
    {
        public int Used { get; init; }
        public int? Maximum { get; init; }
        public int? Selected { get; init; }
    }

    public record InventoryItemsListViewEntry
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyDictionary<string, string> CellsTexts { get; init; } = new Dictionary<string, string>();
    }

    public record FleetWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<FleetMember> Members { get; init; } = new List<FleetMember>();
        public string FleetName { get; init; }
        public bool IsCommander { get; init; }
        public UITreeNodeWithDisplayRegion InviteButton { get; init; }
        public UITreeNodeWithDisplayRegion LeaveButton { get; init; }
    }

    public record FleetMember
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public string ShipType { get; init; }
        public string SystemName { get; init; }
        public int? SolarSystemId { get; init; }
        public int? Distance { get; init; }
        public bool IsWarping { get; init; }
        public bool IsInFleetHangar { get; init; }
        public bool IsInFleetHangarAccessAllowed { get; init; }
    }

    public record LocationsWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<LocationFolder> Folders { get; init; } = new List<LocationFolder>();
        public IReadOnlyList<LocationBookmark> Bookmarks { get; init; } = new List<LocationBookmark>();
    }

    public record LocationFolder
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public bool IsExpanded { get; init; }
        public IReadOnlyList<LocationFolder> SubFolders { get; init; } = new List<LocationFolder>();
        public IReadOnlyList<LocationBookmark> Bookmarks { get; init; } = new List<LocationBookmark>();
    }

    public record LocationBookmark
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public string Location { get; init; }
        public string Notes { get; init; }
        public DateTime Created { get; init; }
        public bool IsSelected { get; init; }
    }

    public record WatchListPanel
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<WatchListEntry> Entries { get; init; } = new List<WatchListEntry>();
    }

    public record WatchListEntry
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public string TypeName { get; init; }
        public string Distance { get; init; }
        public bool IsSelected { get; init; }
    }

    public record StandaloneBookmarkWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public string Notes { get; init; }
        public string Location { get; init; }
        public UITreeNodeWithDisplayRegion SaveButton { get; init; }
        public UITreeNodeWithDisplayRegion CancelButton { get; init; }
    }

    public record ModuleButtonTooltip
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string ModuleName { get; init; }
        public string GroupName { get; init; }
        public string ActivationEffect { get; init; }
        public string DeactivationEffect { get; init; }
        public string CycleTime { get; init; }
        public string OptimalRange { get; init; }
        public string FalloffRange { get; init; }
        public string Duration { get; init; }
        public string ActivationCost { get; init; }
        public string ActivationTime { get; init; }
        public string DeactivationTime { get; init; }
        public string HeatDamage { get; init; }
        public string HeatDamageBonus { get; init; }
        public string OverloadBonus { get; init; }
        public string OverloadSelfBonus { get; init; }
        public string OverloadDurationBonus { get; init; }
    }

    public record HeatStatusTooltip
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public int? HeatLevel { get; init; }
        public string HeatLevelText { get; init; }
        public string HeatDamage { get; init; }
        public string HeatDamageBonus { get; init; }
        public string OverloadBonus { get; init; }
        public string OverloadSelfBonus { get; init; }
        public string OverloadDurationBonus { get; init; }
    }

    public record Neocom
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<NeocomButton> Buttons { get; init; } = new List<NeocomButton>();
        public UITreeNodeWithDisplayRegion Clock { get; init; }
        public string Time { get; init; }
        public string SystemName { get; init; }
        public string SystemSecurityStatus { get; init; }
    }

    public record NeocomButton
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public bool IsActive { get; init; }
        public bool IsHighlighted { get; init; }
        public bool IsBlinking { get; init; }
    }

    public record MessageBox
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Title { get; init; }
        public string Message { get; init; }
        public IReadOnlyList<MessageBoxButton> Buttons { get; init; } = new List<MessageBoxButton>();
    }

    public record MessageBoxButton
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Text { get; init; }
        public bool IsDefault { get; init; }
        public bool IsCancel { get; init; }
    }

    public record LayerAbovemain
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<LayerAbovemainWindow> Windows { get; init; } = new List<LayerAbovemainWindow>();
    }

    public record LayerAbovemainWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public bool IsModal { get; init; }
        public bool IsMoving { get; init; }
        public bool IsResizing { get; init; }
    }

    public record KeyActivationWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string ActivationCode { get; init; }
        public UITreeNodeWithDisplayRegion InputField { get; init; }
        public UITreeNodeWithDisplayRegion ActivateButton { get; init; }
        public UITreeNodeWithDisplayRegion CancelButton { get; init; }
    }

    public record CompressionWindow
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public IReadOnlyList<CompressionItem> Items { get; init; } = new List<CompressionItem>();
        public UITreeNodeWithDisplayRegion CompressButton { get; init; }
        public bool CanCompress { get; init; }
    }

    public record CompressionItem
    {
        public UITreeNodeWithDisplayRegion UiNode { get; init; }
        public string Name { get; init; }
        public int? Quantity { get; init; }
        public bool IsSelected { get; init; }
    }

    private static CharacterSheetWindow ParseCharacterSheetWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            var windowNode = Parser.ListDescendantsWithDisplayRegion(uiTreeRoot)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "CharacterSheetWindow");

            if (windowNode == null) return null;

            return ParseCharacterSheetWindow(windowNode);
        }

        private static InventoryWindowLeftTreeEntry ParseInventoryWindowLeftTreeEntry(UITreeNodeWithDisplayRegion entryNode)
        {
            var toggleButton = Parser.ListDescendantsWithDisplayRegion(entryNode)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "TreeViewToggleButton");

            var selectRegion = Parser.ListDescendantsWithDisplayRegion(entryNode)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "TreeViewSelect");

            var text = Parser.ListDescendantsWithDisplayRegion(entryNode)
                .SelectMany(n => GetAllContainedDisplayTexts(n.UiNode))
                .FirstOrDefault();

            var childEntries = Parser.ListDescendantsWithDisplayRegion(entryNode)
                .Where(n => n.UiNode.PythonObjectTypeName == "TreeViewEntry")
                .Select(ParseInventoryWindowLeftTreeEntry)
                .Select(e => new InventoryWindowLeftTreeEntryChildNode { Entry = e } as InventoryWindowLeftTreeEntryChild)
                .ToList();

            return new InventoryWindowLeftTreeEntry
            {
                UiNode = entryNode,
                ToggleButton = toggleButton,
                SelectRegion = selectRegion,
                Text = text,
                Children = childEntries
            };
        }

        private static IReadOnlyList<UITreeNodeWithDisplayRegion> GetContainedTreeViewEntryRootNodes(UITreeNodeWithDisplayRegion parentNode)
        {
            return Parser.ListDescendantsWithDisplayRegion(parentNode)
                .Where(n => n.UiNode.PythonObjectTypeName == "TreeViewEntry")
                .Where(n => !Parser.ListDescendantsWithDisplayRegion(parentNode)
                    .Any(p => p != n &&
                             Parser.ListDescendantsWithDisplayRegion(p)
                                 .Any(c => c == n)))
                .ToList();
        }

        private static Result<InventoryWindowCapacityGauge, string> ParseInventoryWindowCapacityGauge(UITreeNodeWithDisplayRegion gaugeNode)
        {
            try
            {
                var gaugeTexts = Parser.ListDescendantsWithDisplayRegion(gaugeNode)
                    .Select(n => n.UiNode.DisplayTextFromDict())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .OrderByDescending(t => t.Length)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(gaugeTexts))
                    return Result<InventoryWindowCapacityGauge, string>.Err("No gauge text found");

                return Result<InventoryWindowCapacityGauge, string>.Ok(ParseInventoryCapacityGaugeText(gaugeTexts));
            }
            catch (Exception ex)
            {
                return Result<InventoryWindowCapacityGauge, string>.Err($"Failed to parse capacity gauge: {ex.Message}");
            }
        }

        private static InventoryWindowCapacityGauge ParseInventoryCapacityGaugeText(string gaugeText)
        {
            // Example: "1,234 / 5,000 m³ (25%)"
            var match = System.Text.RegularExpressions.Regex.Match(gaugeText, @"([\d,]+)\s*/\s*([\d,]+)\s*[^\d]*\(?(\d+)%?\)?");

            if (!match.Success)
                return null;

            int ParseNumber(string numStr) => int.Parse(numStr.Replace(",", ""));

            return new InventoryWindowCapacityGauge
            {
                Used = ParseNumber(match.Groups[1].Value),
                Maximum = match.Groups[2].Success ? ParseNumber(match.Groups[2].Value) : (int?)null,
                Selected = match.Groups[3].Success ? ParseNumber(match.Groups[3].Value) : (int?)null
            };
        }

        private static Inventory ParseInventory(UITreeNodeWithDisplayRegion inventoryNode)
        {
            var listViewItemNodes = Parser.ListDescendantsWithDisplayRegion(inventoryNode)
                .Where(n => n.UiNode.PythonObjectTypeName == "Item")
                .ToList();

            var scrollNode = Parser.ListDescendantsWithDisplayRegion(inventoryNode)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName.ToLower().Contains("scroll"));

            var scrollControls = scrollNode != null
                ? Parser.ListDescendantsWithDisplayRegion(scrollNode)
                    .FirstOrDefault(n => n.UiNode.PythonObjectTypeName.Contains("ScrollControls"))
                : null;

            var headersContainerNode = Parser.ListDescendantsWithDisplayRegion(inventoryNode)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName.ToLower().Contains("headers") ||
                                   (n.UiNode.NameFromDictEntries?.ToLower().Contains("headers") ?? false));

            var entriesHeaders = headersContainerNode != null
                ? GetAllContainedDisplayTextsWithRegion(headersContainerNode)
                      .GroupBy(x => x.Text)
                      .Select(g => g.First())
                      .ToList()
                : new List<(string Text, UITreeNodeWithDisplayRegion Node)>();

            var notListViewItemNodes = Parser.ListDescendantsWithDisplayRegion(inventoryNode)
                .Where(n => n.UiNode.PythonObjectTypeName == "Item")
                .Where(n => !listViewItemNodes.Contains(n))
                .Select(n => new InventoryItem
                {
                    UiNode = n,
                    Name = n.UiNode.DisplayTextFromDict() ?? ""
                })
                .ToList();

            InventoryItemsView itemsView = null;

            if (listViewItemNodes.Any())
            {
                itemsView = new InventoryItemsListView
                {
                    Items = listViewItemNodes
                        .Select(itemNode => new InventoryItemsListViewEntry
                        {
                            UiNode = itemNode,
                            CellsTexts = ParseListViewEntry(entriesHeaders, itemNode).CellsTexts
                        })
                        .ToList()
                };
            }
            else if (notListViewItemNodes.Any())
            {
                itemsView = new InventoryItemsNotListView
                {
                    Items = notListViewItemNodes
                };
            }

            return new Inventory
            {
                UiNode = inventoryNode,
                ItemsView = itemsView,
                ScrollControls = scrollControls != null ? ParseScrollControls(scrollControls) : null
            };
        }


        private static (Dictionary<string, string> CellsTexts, List<string> TextsLeftToRight) ParseListViewEntry(
            List<(string Text, UITreeNodeWithDisplayRegion Node)> entriesHeaders,
            UITreeNodeWithDisplayRegion listViewEntryNode)
        {
            var textsWithRegions = GetAllContainedDisplayTextsWithRegion(listViewEntryNode);
            var textsLeftToRight = textsWithRegions
                .OrderBy(t => t.Node.TotalDisplayRegion.X)
                .Select(t => t.Text)
                .ToList();

            var cellsTexts = new Dictionary<string, string>();

            foreach (var (headerText, headerNode) in entriesHeaders)
            {
                var headerRegion = headerNode.TotalDisplayRegion;
                var cellText = textsWithRegions
                    .Where(t => Math.Abs(t.Node.TotalDisplayRegion.Y - headerRegion.Y) < 5) // Same row
                    .Where(t => t.Node.TotalDisplayRegion.X >= headerRegion.X - 3 &&
                               t.Node.TotalDisplayRegion.X <= headerRegion.X + headerRegion.Width + 3)
                    .OrderBy(t => t.Node.TotalDisplayRegion.X)
                    .Select(t => t.Text)
                    .FirstOrDefault();

                if (cellText != null)
                {
                    cellsTexts[headerText] = cellText;
                }
            }

            return (cellsTexts, textsLeftToRight);
        }

        private static IReadOnlyList<InventoryWindow> ParseInventoryWindowsFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            var inventoryWindowNodes = Parser.ListDescendantsWithDisplayRegion(uiTreeRoot)
                .Where(n => n.UiNode.PythonObjectTypeName == "InventoryPrimary" || n.UiNode.PythonObjectTypeName == "ActiveShipCargo")
                .ToList();

            return inventoryWindowNodes.Select(ParseInventoryWindow).ToList();
        }

        private static InventoryWindow ParseInventoryWindow(UITreeNodeWithDisplayRegion windowUiNode)
        {
            var selectedContainerCapacityGaugeNode = Parser.ListDescendantsWithDisplayRegion(windowUiNode)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName.Contains("CapacityGauge"));

            var selectedContainerCapacityGauge = selectedContainerCapacityGaugeNode != null
                ? ParseInventoryWindowCapacityGauge(selectedContainerCapacityGaugeNode)
                : null;

            var leftTreeEntriesRootNodes = GetContainedTreeViewEntryRootNodes(windowUiNode);
            var leftTreeEntries = leftTreeEntriesRootNodes.Select(ParseInventoryWindowLeftTreeEntry).ToList();

            var rightContainerNode = Parser.ListDescendantsWithDisplayRegion(windowUiNode)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "Container" &&
                                   n.UiNode.NameFromDictEntries?.Contains("right") == true);

            var subCaptionLabelText = rightContainerNode != null
                ? Parser.ListDescendantsWithDisplayRegion(rightContainerNode)
                    .Where(n => n.UiNode.NameFromDictEntries?.StartsWith("subCaptionLabel") == true)
                    .SelectMany(n => GetAllContainedDisplayTexts(n.UiNode))
                    .FirstOrDefault()
                : null;

            var selectedContainerInventoryNode = rightContainerNode != null
                ? Parser.ListDescendantsWithDisplayRegion(rightContainerNode)
                    .FirstOrDefault(n => new[] { "ShipCargo", "ShipDroneBay", "ShipGeneralMiningHold", "StationItems", "ShipFleetHangar", "StructureItemHangar" }
                        .Contains(n.UiNode.PythonObjectTypeName))
                : null;

            var selectedContainerInventory = selectedContainerInventoryNode != null
                ? ParseInventory(selectedContainerInventoryNode)
                : null;

            var buttonToSwitchToListView = rightContainerNode != null
                ? Parser.ListDescendantsWithDisplayRegion(rightContainerNode)
                    .FirstOrDefault(n => n.UiNode.PythonObjectTypeName.Contains("ButtonIcon") &&
                                       n.UiNode.TexturePathFromDict()?.EndsWith("38_16_190.png") == true)
                : null;

            var buttonToStackAll = rightContainerNode != null
                ? Parser.ListDescendantsWithDisplayRegion(rightContainerNode)
                    .FirstOrDefault(n => n.UiNode.PythonObjectTypeName.Contains("ButtonIcon") &&
                                       n.UiNode.HintTextFromDict()?.Contains("Stack All") == true)
                : null;

            return new InventoryWindow
            {
                UiNode = windowUiNode,
                LeftTreeEntries = leftTreeEntries,
                SubCaptionLabelText = subCaptionLabelText,
                SelectedContainerCapacityGauge = selectedContainerCapacityGauge,
                SelectedContainerInventory = selectedContainerInventory,
                ButtonToSwitchToListView = buttonToSwitchToListView,
                ButtonToStackAll = buttonToStackAll
            };
        }

        private static CharacterSheetWindow ParseCharacterSheetWindow(UITreeNodeWithDisplayRegion windowNode)
        {
            var skillGroups = Parser.ListDescendantsWithDisplayRegion(windowNode)
                .Where(n => n.UiNode.PythonObjectTypeName?.Contains("SkillGroupGauge") == true)
                .Select(skillGroupNode => new SkillGroupGauge
                {
                    UiNode = skillGroupNode,
                    Name = GetAllContainedDisplayTexts(skillGroupNode).FirstOrDefault() ?? "Unknown",
                    // Parse trained and total skill points from the UI if available
                    TrainedSkillPoints = null,
                    TotalSkillPoints = null
                })
                .ToList();

            // Parse character name, corporation, and alliance from the UI
            var characterName = Parser.ListDescendantsWithDisplayRegion(windowNode)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName?.Contains("CharacterNameLabel") == true) is { } nameNode
                ? GetAllContainedDisplayTexts(nameNode).FirstOrDefault()
                : null;

            var corporationName = Parser.ListDescendantsWithDisplayRegion(windowNode)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName?.Contains("CorporationLabel") == true) is { } corpNode
                ? GetAllContainedDisplayTexts(corpNode).FirstOrDefault()
                : null;

            var allianceName = Parser.ListDescendantsWithDisplayRegion(windowNode)
                .FirstOrDefault(n => n.UiNode.PythonObjectTypeName?.Contains("AllianceLabel") == true) is { } allianceNode
                ? GetAllContainedDisplayTexts(allianceNode).FirstOrDefault()
                : null;

            // Parse skills (simplified - would need to be expanded based on actual UI structure)
            var skills = new List<CharacterSkill>();

            // Parse attributes (simplified - would need to be expanded based on actual UI structure)
            var attributes = new List<CharacterAttribute>();

            return new CharacterSheetWindow
            {
                UiNode = windowNode,
                CharacterName = characterName,
                CorporationName = corporationName,
                AllianceName = allianceName,
                Skills = skills,
                Attributes = attributes,
                SkillGroups = skillGroups
            };
        }

        private static IEnumerable<string> GetAllContainedDisplayTexts(UITreeNodeWithDisplayRegion node)
        {
            if (node == null) yield break;

            if (node.UiNode.DictEntriesOfInterest.TryGetValue("text", out var textValue) && textValue is string text)
            {
                yield return text;
            }

            if (node.Children != null)
            {
                foreach (var child in node.Children.OfType<ChildWithRegion>().Select(c => c.Node))
                {
                    foreach (var childText in GetAllContainedDisplayTexts(child))
                    {
                        yield return childText;
                    }
                }
            }
        }
    }

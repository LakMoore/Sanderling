﻿
namespace eve_parse_ui
{
    internal record ShipUIParser
    {
        public static ShipUI? ParseShipUIFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
        {
            // Find ShipUI node
            var thisUINode = uiTreeRoot
                .ListDescendantsInUITreeNode()
                .FirstOrDefault(node => node.pythonObjectTypeName == "ShipUI");

            if (thisUINode is not UITreeNodeWithDisplayRegion shipUINode)
                return null;

            // Find CapacitorContainer
            var capacitorUINode = shipUINode.GetDescendantsByType("CapacitorContainer")
                .FirstOrDefault();

            if (capacitorUINode == null) 
                return null;

            var capacitor = ParseShipUICapacitorFromUINode(capacitorUINode);

            // Parse indication
            var indicationNode = shipUINode
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(node =>
                    node.GetNameFromDictEntries()?.Contains("indicationcontainer", StringComparison.CurrentCultureIgnoreCase) == true
                );

            var indication = indicationNode != null ? ParseShipUIIndication(indicationNode) : null;

            // Parse module buttons
            var moduleButtons = shipUINode.GetDescendantsByType("ShipSlot")
                .SelectMany(slotNode =>
                    slotNode.GetDescendantsByType("ModuleButton")
                        .Select(moduleButtonNode =>
                            ParseShipUIModuleButton(slotNode, moduleButtonNode)
                        )
                )
                .ToList();

            // Helper function to get gauge value
            int? GetGaugeValue(string gaugeName) =>
                shipUINode.GetDescendantsByName(gaugeName)
                    .Select(node =>
                        node.dictEntriesOfInterest?.TryGetValue("_lastValue", out var value) == true
                            ? (float?)Convert.ToDouble(value) * 100
                            : (float?)null
                    )
                    .FirstOrDefault() is float gaugeValue ? (int?)gaugeValue : null;

            // Parse hitpoints
            var structure = GetGaugeValue("structureGauge");
            var armor = GetGaugeValue("armorGauge");
            var shield = GetGaugeValue("shieldGauge");

            if (structure == null || armor == null || shield == null)
                return null;

            var hitpointsPercent = new Hitpoints
            {
                Structure = structure.Value,
                Armor = armor.Value,
                Shield = shield.Value
            };

            // Parse offensive buff buttons
            List<OffensiveBuffButton> offensiveBuffButtons = shipUINode
                .ListDescendantsWithDisplayRegion()
                .Select(node => new { Node = node, Name = node.GetNameFromDictEntries() })
                .Where(x => x.Name != null)
                .Select(x => new OffensiveBuffButton() { UiNode = x.Node, Name = x.Name! })
                .ToList();

            // Parse squadrons UI
            var squadronsUI = shipUINode.GetDescendantsByType("SquadronsUI") is UITreeNodeWithDisplayRegion squadNode
                    ? ParseSquadronsUI(squadNode)
                    : null;

            // Parse heat gauges
            var heatGauges = shipUINode.GetDescendantsByType("HeatGauges") is UITreeNodeWithDisplayRegion heatNode
                    ? ParseShipUIHeatGaugesFromUINode(heatNode)
                    : null;

            // Get stop and max speed buttons
            var stopButton = shipUINode.GetDescendantsByType("StopButton").FirstOrDefault();
            var maxSpeedButton = shipUINode.GetDescendantsByType("MaxSpeedButton").FirstOrDefault();

            return new ShipUI
            {
                UiNode = shipUINode,
                Capacitor = capacitor,
                HitpointsPercent = hitpointsPercent,
                Indication = indication,
                ModuleButtons = moduleButtons,
                ModuleButtonsRows = GroupShipUIModulesIntoRows(capacitor, moduleButtons),
                OffensiveBuffButtons = offensiveBuffButtons,
                SquadronsUI = squadronsUI,
                StopButton = stopButton,
                MaxSpeedButton = maxSpeedButton,
                HeatGauges = heatGauges
            };
        }

        public static ShipUIModuleButton ParseShipUIModuleButton(UITreeNodeWithDisplayRegion slotNode, UITreeNodeWithDisplayRegion moduleButtonNode)
        {
            float? RotationFloatFromRampName(string rampName)
            {
                return slotNode
                    .ListDescendantsWithDisplayRegion()
                    .Where(node => node.GetNameFromDictEntries() == rampName)
                    .Select(node => UIParser.GetRotationFloatFromDictEntries(node))
                    .FirstOrDefault();
            }

            int? rampRotationMilli = null;
            var leftRampRotationFloat = RotationFloatFromRampName("leftRamp");
            var rightRampRotationFloat = RotationFloatFromRampName("rightRamp");

            if (leftRampRotationFloat.HasValue && rightRampRotationFloat.HasValue)
            {
                var left = leftRampRotationFloat.Value;
                var right = rightRampRotationFloat.Value;

                if ((left >= 0 && left <= Math.PI * 2.01) &&
                    (right >= 0 && right <= Math.PI * 2.01))
                {
                    var rotation = 1000 - ((left + right) * 500 / Math.PI);
                    rampRotationMilli = (int)Math.Max(0, Math.Min(1000, rotation));
                }
            }

            var glow = slotNode.GetDescendantsByType("Sprite")
                .Where(node => node.GetNameFromDictEntries() == "glow")
                .FirstOrDefault();

            return new ShipUIModuleButton
            {
                UiNode = moduleButtonNode,
                SlotUINode = slotNode,
                IsActive = glow != null,
                IsHiliteVisible = slotNode
                    .ListDescendantsWithDisplayRegion()
                    .Any(node =>
                        node.pythonObjectTypeName == "Sprite" &&
                        node.GetNameFromDictEntries() == "hilite"),
                IsBusy = slotNode
                    .ListDescendantsWithDisplayRegion()
                    .Any(node =>
                        node.pythonObjectTypeName == "Sprite" &&
                        node.GetNameFromDictEntries() == "busy"),
                RampRotationMilli = rampRotationMilli,
                TypeID = moduleButtonNode.GetDescendantsByType("Icon").FirstOrDefault()?.GetFromDict<int>("typeID"),
                Blinking = moduleButtonNode.GetFromDict<int>("blinking"),
                Online = moduleButtonNode.GetFromDict<bool>("online"),
                Quantity = moduleButtonNode.GetFromDict<int>("quantity"),
                IsDeactivating = moduleButtonNode.GetFromDict<bool>("isDeactivating"),
                SlotName = slotNode.GetNameFromDictEntries(),
            };
        }

        public static ShipUICapacitor ParseShipUICapacitorFromUINode(UITreeNodeWithDisplayRegion capacitorUINode)
        {
            // a list of all the capacitor cells
            var pmarks = capacitorUINode
                .ListDescendantsWithDisplayRegion()
                .Where(node =>
                    node.GetNameFromDictEntries()?.Equals("pmark", StringComparison.CurrentCultureIgnoreCase) == true
                )
                .Select(pmarkUINode =>
                {
                    ColorComponents? colorComponents = UIParser.GetColorPercentFromDictEntries(pmarkUINode);
                    if (colorComponents == null)
                        return null;
                    return new ShipUICapacitorPmark
                    {
                        UiNode = pmarkUINode,
                        ColorPercent = colorComponents
                    };
                })
                .Where(pmark => pmark != null)
                .Cast<ShipUICapacitorPmark>()
                .ToList();

            double pmarkCount = pmarks.Count;

            // count the number of cells that are filled
            double pmarkFilledCount = pmarks
                .Count(pmark => pmark?.ColorPercent.A < 20);

            int levelFromPmarksPercent = (int)(100f * pmarkFilledCount / pmarkCount);

            return new ShipUICapacitor
            {
                UiNode = capacitorUINode,
                PMarks = pmarks,
                LevelFromPMarksPercent = levelFromPmarksPercent
            };
        }

        public static ShipUIHeatGauges ParseShipUIHeatGaugesFromUINode(UITreeNodeWithDisplayRegion gaugesUINode)
        {
            var heatGaugesRotationZeroValues = new[] { -213, -108, -3 };

            int? HeatValuePercentFromRotationPercent(int? rotationPercent)
            {
                if (!rotationPercent.HasValue)
                    return null;

                return heatGaugesRotationZeroValues
                    .Select(gaugeRotationZero =>
                    {
                        if (rotationPercent <= gaugeRotationZero && gaugeRotationZero - 100 <= rotationPercent)
                            return (int?)-(rotationPercent.Value - gaugeRotationZero);
                        return null;
                    })
                    .FirstOrDefault(percent => percent.HasValue);
            }

            var gauges = gaugesUINode
                .ListDescendantsWithDisplayRegion()
                .Where(node => node.GetNameFromDictEntries()?.Equals("heatGauge", StringComparison.CurrentCultureIgnoreCase) == true)
                .Select(gaugeUiNode =>
                {
                    int? rotationPercent = UIParser.GetRotationFloatFromDictEntries(gaugeUiNode) is float rotation ?
                        (int)(rotation * 100) : null;

                    return new ShipUIHeatGauge
                    {
                        UiNode = gaugeUiNode,
                        RotationPercent = rotationPercent,
                        HeatPercent = HeatValuePercentFromRotationPercent(rotationPercent)
                    };
                })
                .ToList();

            return new ShipUIHeatGauges
            {
                UiNode = gaugesUINode,
                Gauges = gauges
            };
        }

        public static ModuleButtonsRows GroupShipUIModulesIntoRows(ShipUICapacitor capacitor, List<ShipUIModuleButton> modules)
        {
            const int verticalDistanceThreshold = 20;

            static int VerticalCenterOfUINode(UITreeNodeWithDisplayRegion uiNode) => 
                uiNode.TotalDisplayRegion.Y + uiNode.TotalDisplayRegion.Height / 2;

            var capacitorVerticalCenter = VerticalCenterOfUINode(capacitor.UiNode);
        
            return modules
                .Aggregate(
                    new ModuleButtonsRows
                    {
                        Top = [],
                        Middle = [],
                        Bottom = []
                    },
                    (previousRows, shipModule) =>
                    {
                        var moduleCenter = VerticalCenterOfUINode(shipModule.UiNode);
                    
                        if (moduleCenter < capacitorVerticalCenter - verticalDistanceThreshold)
                        {
                            previousRows.Top.Add(shipModule);
                        }
                        else if (moduleCenter > capacitorVerticalCenter + verticalDistanceThreshold)
                        {
                            previousRows.Bottom.Add(shipModule);
                        }
                        else
                        {
                            previousRows.Middle.Add(shipModule);
                        }
                    
                        return previousRows;
                    });
        }

        public static ShipUIIndication ParseShipUIIndication(UITreeNodeWithDisplayRegion indicationUINode)
        {
            var displayTexts = UIParser.GetAllContainedDisplayTexts(indicationUINode);
        
            var maneuverPatterns = new Dictionary<string, ShipManeuverType>
            {
                { "Warp", ShipManeuverType.ManeuverWarp },
                { "Jump", ShipManeuverType.ManeuverJump },
                { "Orbit", ShipManeuverType.ManeuverOrbit },
                { "Approach", ShipManeuverType.ManeuverApproach },
                { "Docking", ShipManeuverType.ManeuverDock },
                { "워프 드라이브 가동", ShipManeuverType.ManeuverWarp }, // Korean for "Warp Drive Active"
                { "점프 중", ShipManeuverType.ManeuverJump } // Korean for "Jumping"
            };

            var maneuverType = maneuverPatterns
                .Where(pair => displayTexts.Any(text => text.Contains(pair.Key, StringComparison.OrdinalIgnoreCase)))
                .Select(pair => pair.Value)
                .Cast<ShipManeuverType?>()
                .FirstOrDefault();

            return new ShipUIIndication
            {
                UiNode = indicationUINode,
                ManeuverType = maneuverType
            };
        }

        public static SquadronsUI ParseSquadronsUI(UITreeNodeWithDisplayRegion squadronsUINode)
        {
            return new SquadronsUI
            {
                UiNode = squadronsUINode,
                Squadrons = squadronsUINode
                    .ListDescendantsWithDisplayRegion()
                    .Where(node => node.pythonObjectTypeName == "SquadronUI")
                    .Select(ParseSquadronUI)
                    .ToList()
            };
        }

        public static SquadronUI ParseSquadronUI(UITreeNodeWithDisplayRegion squadronUINode)
        {
            return new SquadronUI
            {
                UiNode = squadronUINode,
                Abilities = squadronUINode
                    .ListDescendantsWithDisplayRegion()
                    .Where(node => node.pythonObjectTypeName == "AbilityIcon")
                    .Select(ParseSquadronAbilityIcon)
                    .ToList(),
                ActionLabel = squadronUINode
                    .ListDescendantsWithDisplayRegion()
                    .First(node => node.pythonObjectTypeName == "SquadronActionLabel")
            };
        }

        public static SquadronAbilityIcon ParseSquadronAbilityIcon(UITreeNodeWithDisplayRegion abilityIconUINode)
        {
            return new SquadronAbilityIcon
            {
                UiNode = abilityIconUINode,
                Quantity = abilityIconUINode
                    .ListDescendantsWithDisplayRegion()
                    .Where(node =>
                        node.GetNameFromDictEntries()?.Contains("quantity", StringComparison.CurrentCultureIgnoreCase) == true
                    )
                    .SelectMany(UIParser.GetAllContainedDisplayTexts)
                    .Select(text => int.TryParse(text.Trim(), out var result) ? (int?)result : null)
                    .FirstOrDefault(),
                RampActive = abilityIconUINode.dictEntriesOfInterest
                    .TryGetValue("ramp_active", out var rampActiveToken)
                        ? rampActiveToken as bool?
                        : null
            };
        }

    }
}

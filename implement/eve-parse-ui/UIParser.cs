using read_memory_64_bit;
using System.Diagnostics;

namespace eve_parse_ui
{
    public static class UIParser
    {
        public static ParsedUserInterface ParseUserInterface(UITreeNode uiTreeRoot)
        {
            ArgumentNullException.ThrowIfNull(uiTreeRoot);

            var uiTreeRootWithDisplayRegion = AsUITreeNodeWithDisplayRegion(uiTreeRoot);
            return new ParsedUserInterface
            {
                UiTree = uiTreeRootWithDisplayRegion,
                ShipUI = ShipUIParser.ParseShipUIFromUITreeRoot(uiTreeRootWithDisplayRegion),
                InfoPanelContainer = InfoPanelParser.ParseInfoPanelContainerFromUIRoot(uiTreeRootWithDisplayRegion),
                OverviewWindows = OverviewParser.ParseOverviewWindowsFromUITreeRoot(uiTreeRootWithDisplayRegion).ToList(),
                ProbeScanner = ProbeScannerParser.ParseProbeScannerWindowFromUITreeRoot(uiTreeRootWithDisplayRegion),
                StationWindow = StationWindowParser.ParseStationWindowFromUITreeRoot(uiTreeRootWithDisplayRegion),
                MessageBoxes = MessageBoxParser.ParseMessageBoxesFromUITreeRoot(uiTreeRootWithDisplayRegion),
                LayerAboveMain = LayerAboveMainParser.ParseLayerAbovemainFromUITreeRoot(uiTreeRootWithDisplayRegion)
            };
        }

        // only called on the root node!
        private static UITreeNodeNoDisplayRegion AsUITreeNodeWithDisplayRegion(UITreeNode node)
        {
            ArgumentNullException.ThrowIfNull(node);

            var upgradedRootNode = new UITreeNodeNoDisplayRegion(node);

            var selfDisplayRegion = GetDisplayRegionFromDictEntries(node);
            if (selfDisplayRegion == null)
            {
                // Root node has never been known to have no display region
                Debug.Fail("Root node should have display region");
                return upgradedRootNode;
            } else
            {
                var totalDisplayRegion = new DisplayRegion(0, 0, selfDisplayRegion.Width, selfDisplayRegion.Height);
                var occludedRegions = new List<DisplayRegion>();

                return AsUITreeNodeWithDisplayRegion(node, selfDisplayRegion, totalDisplayRegion, occludedRegions);
            }
        }

        private static UITreeNodeWithDisplayRegion AsUITreeNodeWithDisplayRegion(
            UITreeNode node,
            DisplayRegion selfDisplayRegion,
            DisplayRegion totalDisplayRegion,
            List<DisplayRegion> occludedRegions
            )
        {
            ArgumentNullException.ThrowIfNull(node);

            var children = new List<UITreeNodeNoDisplayRegion>();
            var currentOccludedRegions = new List<DisplayRegion>(occludedRegions);

            if (node.children != null)
            {
                // Process children in order to build up occluded regions for subsequent siblings
                foreach (var child in node.children)
                {
                    var childResult = AsChildOfNodeWithDisplayRegion(
                        child,
                        new Location2d(totalDisplayRegion.X, totalDisplayRegion.Y),
                        currentOccludedRegions);

                    if (childResult == null) continue;

                    children.Add(childResult);

                    // If this child has a region, update occluded regions for subsequent siblings
                    if (childResult is UITreeNodeWithDisplayRegion childWithRegion)
                    {
                        var occludingNodes = childWithRegion.ListDescendantsWithDisplayRegion()
                            .Where(desc => NodeOccludesFollowingNodes(desc))
                            .Select(desc => desc.TotalDisplayRegion);

                        currentOccludedRegions.AddRange(occludingNodes);
                    }
                }
            }

            var totalDisplayRegionVisible = SubtractRegionsFromRegion(totalDisplayRegion, occludedRegions)
                .OrderByDescending(region => region.Width * region.Height)
                .FirstOrDefault() ?? new DisplayRegion(-1, -1, 0, 0);

            var result = new UITreeNodeWithDisplayRegion(node)
            {
                Children = children,
                SelfDisplayRegion = selfDisplayRegion,
                TotalDisplayRegion = totalDisplayRegion,
                TotalDisplayRegionVisible = totalDisplayRegionVisible
            };

            // add this node as a parent to all its children
            result.Children?.ToList().ForEach(child => child.Parent = result);

            return result;
        }

        private static UITreeNodeNoDisplayRegion? AsChildOfNodeWithDisplayRegion(
            UITreeNode node,
            Location2d inheritedOffset, 
            List<DisplayRegion> occludedRegions
        )
        {
            if (node == null) return null;

            var selfRegion = GetDisplayRegionFromDictEntries(node);
            if (selfRegion == null)
            {
                if (node.children?.Count > 0)
                {
                    // Should a node with no display region have children?
                    //Debug.WriteLine($"{node.pythonObjectTypeName} Node with no display region has children!");
                }
                return new UITreeNodeNoDisplayRegion(node);
            }

            // Calculate the total region by applying the inherited offset
            var totalRegion = new DisplayRegion(
                inheritedOffset.X + selfRegion.X,
                inheritedOffset.Y + selfRegion.Y,
                selfRegion.Width,
                selfRegion.Height);

            // Create a new list for child occluded regions to avoid modifying the parent's list
            var childOccludedRegions = new List<DisplayRegion>(occludedRegions);

            return AsUITreeNodeWithDisplayRegion(
                node,
                selfRegion,
                totalRegion,
                childOccludedRegions);
        }

        private static DisplayRegion? GetDisplayRegionFromDictEntries(UITreeNode node)
        {
            if (node?.dictEntriesOfInterest != null)
            {
                var x = GetIntFromDict(node.dictEntriesOfInterest, "_displayX");
                var y = GetIntFromDict(node.dictEntriesOfInterest, "_displayY");
                var width = GetIntFromDict(node.dictEntriesOfInterest, "_displayWidth");
                var height = GetIntFromDict(node.dictEntriesOfInterest, "_displayHeight");

                if (x.HasValue && y.HasValue && width.HasValue && height.HasValue)
                {
                    // Ensure non-negative dimensions as per Elm implementation
                    var actualWidth = Math.Max(0, width.Value);
                    var actualHeight = Math.Max(0, height.Value);
                    return new DisplayRegion(x.Value, y.Value, actualWidth, actualHeight);
                }
            }

            return null;
        }

        internal static int? GetIntFromDict(IReadOnlyDictionary<string, object> dict, string key)
        {
            if (dict == null || !dict.TryGetValue(key, out var value))
                return null;

            if (value is int intValue)
                return intValue;

            if (value is double doubleValue)
                return (int)doubleValue;

            // Handle case where value might be a python long
            try
            {
                if (value != null)
                {
                    //var longProperty = (long)value.GetType().GetProperty("int").GetValue(value);
                    var intProperty = value.GetType().GetProperty("int_low32");

                    if (intProperty != null)
                    {
                        var i = intProperty.GetValue(value);
                        if (i != null)
                        {
                            return (int)i;
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static string? GetStringFromDict(IReadOnlyDictionary<string, object> dict, string key)
        {
            if (dict != null && dict.TryGetValue(key, out var value) && value is string strValue)
            {
                return strValue;
            }
            return null;
        }

        private static bool? GetBoolFromDict(IReadOnlyDictionary<string, object> dict, string key)
        {
            if (dict != null && dict.TryGetValue(key, out var value) && value is bool boolValue)
            {
                return boolValue;
            }
            return null;
        }

        private static string? GetDisplayText(UITreeNode? node)
        {
            if (node?.dictEntriesOfInterest == null)
                return null;

            if (node.dictEntriesOfInterest.TryGetValue("_setText", out var setTextObj) && setTextObj != null)
            {
                if (setTextObj is string displaySetText)
                {
                    return displaySetText;
                }
                /*
                   2024-05-26: Observed in info panel:
                   Property '_setText' contained not string but a python object of type 'Link', which in turn references a dictionary.
                   That dictionary contains a key '_text' with the actual text.
                 */
                if (setTextObj is UITreeNode setTextNode)
                {
                    var location = GetDisplayText(setTextNode);
                    var alt = GetStringPropertyFromDictEntries("_alt", setTextNode);
                    return $"{alt}:{location}";
                } 

            }

            if (node.dictEntriesOfInterest.TryGetValue("_text", out var textObj) &&
                textObj is string displayText)
            {
                return displayText;
            }

            return null;
        }

        public static string GetHintTextFromDictEntries(UITreeNode node)
        {
            return node?.dictEntriesOfInterest?.GetValueOrDefault("hint") as string ?? string.Empty;
        }

        public static string? GetTexturePathFromDictEntries(UITreeNode node)
        {
            return GetStringPropertyFromDictEntries("texturePath", node) ?? GetStringPropertyFromDictEntries("_texturePath", node);
        }

        public static string? GetStringPropertyFromDictEntries(string dictEntryKey, UITreeNode uiNode)
        {
            if (uiNode.dictEntriesOfInterest.TryGetValue(dictEntryKey, out object? value))
            {
                if (value is string stringValue)
                {
                    // Remove any quotes around the string
                    var trimmedJsonValue = stringValue.Trim('"');
                    return trimmedJsonValue;
                }
            }

            return null;
        }

        public static ColorComponents? GetColorPercentFromDictEntries(UITreeNode? node)
        {
            if (node?.dictEntriesOfInterest == null)
                return null;

            if (node?.dictEntriesOfInterest?.TryGetValue("_color", out var colorObj) == true && colorObj is Dictionary<string, object> colorDict)
            {
                return new ColorComponents
                {
                    A = (int)colorDict.GetValueOrDefault("aPercent", 100),
                    R = (int)colorDict.GetValueOrDefault("rPercent", 100),
                    G = (int)colorDict.GetValueOrDefault("gPercent", 100),
                    B = (int)colorDict.GetValueOrDefault("bPercent", 100)
                };
            }
            return null;
        }

        public static float? GetRotationFloatFromDictEntries(UITreeNode uiNode) =>
            uiNode?.dictEntriesOfInterest is Dictionary<string, object> dict &&
            dict.TryGetValue("_rotation", out var rotationToken) &&
            rotationToken is float floatRoatation
                ? floatRoatation
                : null;

        public static double GetOpacityFloatFromDictEntries(UITreeNode node)
        {
            if (node?.dictEntriesOfInterest?.TryGetValue("_opacity", out var opacityObj) == true)
            {
                if (opacityObj is double opacity)
                    return opacity;
                if (opacityObj is int opacityInt)
                    return opacityInt;
            }
            return 1.0;
        }

        private static bool NodeOccludesFollowingNodes(UITreeNode node)
        {
            // In Elm: nodeOccludesFollowingNodes = .pythonObjectTypeName >> (==) "EveWindow"
            return node?.pythonObjectTypeName == "EveWindow";
        }

        public static int CountDescendantsInUITreeNode(UITreeNode parent)
        {
            return parent.children?.Select(child => CountDescendantsInUITreeNode(child) + 1).Sum() ?? 0;
        }

        private static IEnumerable<DisplayRegion> SubtractRegionsFromRegion(DisplayRegion region, IEnumerable<DisplayRegion> regionsToSubtract)
        {
            if (region == null || region.Width <= 0 || region.Height <= 0)
            {
                yield break;
            }

            var result = new List<DisplayRegion> { region };

            foreach (var toSubtract in regionsToSubtract ?? Enumerable.Empty<DisplayRegion>())
            {
                if (toSubtract == null) continue;

                var newResult = new List<DisplayRegion>();

                foreach (var current in result)
                {
                    if (!RegionsOverlap(current, toSubtract))
                    {
                        newResult.Add(current);
                        continue;
                    }

                    // Split the current region into up to 4 non-overlapping regions

                    // Left segment
                    if (current.X < toSubtract.X)
                    {
                        newResult.Add(new DisplayRegion(
                            current.X,
                            current.Y,
                            toSubtract.X - current.X,
                            current.Height));
                    }

                    // Right segment
                    if (current.X + current.Width > toSubtract.X + toSubtract.Width)
                    {
                        newResult.Add(new DisplayRegion(
                            toSubtract.X + toSubtract.Width,
                            current.Y,
                            (current.X + current.Width) - (toSubtract.X + toSubtract.Width),
                            current.Height));
                    }

                    // Top segment
                    if (current.Y < toSubtract.Y)
                    {
                        var left = Math.Max(current.X, toSubtract.X);
                        var width = Math.Min(current.X + current.Width, toSubtract.X + toSubtract.Width) - left;

                        newResult.Add(new DisplayRegion(
                            left,
                            current.Y,
                            width,
                            toSubtract.Y - current.Y));
                    }

                    // Bottom segment
                    if (current.Y + current.Height > toSubtract.Y + toSubtract.Height)
                    {
                        var left = Math.Max(current.X, toSubtract.X);
                        var width = Math.Min(current.X + current.Width, toSubtract.X + toSubtract.Width) - left;

                        newResult.Add(new DisplayRegion(
                            left,
                            toSubtract.Y + toSubtract.Height,
                            width,
                            (current.Y + current.Height) - (toSubtract.Y + toSubtract.Height)));
                    }
                }

                result = newResult;
            }

            foreach (var region2 in result)
            {
                if (region2.Width > 0 && region2.Height > 0)
                {
                    yield return region2;
                }
            }
        }

        private static bool RegionsOverlap(DisplayRegion a, DisplayRegion b)
        {
            if (a == null || b == null) return false;

            return a.X < b.X + b.Width &&
                   a.X + a.Width > b.X &&
                   a.Y < b.Y + b.Height &&
                   a.Y + a.Height > b.Y;
        }

        internal static Dictionary<string, string> ParseListViewEntry(
            List<(string Text, UITreeNodeWithDisplayRegion Node)> entriesHeaders,
            UITreeNodeWithDisplayRegion listViewEntryNode
        ) {
            // Observations show two different kinds of representations of the texts in the cells in a list view:

            // +Each cell text in a dedicated UI element. (Overview entry)
            // +All cell texts in a single UI element, separated by a tab - tag(< t >)(Inventory item)

            // Following is an example of the latter case:
            // Condensed Scordite<t><right>200<t>Scordite<t><t><t><right>30 m3<t><right>2.290,00 ISK

            if (entriesHeaders.Count == 0)
                return [];

            var (leftmostText, leftmostNode) = entriesHeaders[0];
            var cellsTexts = new Dictionary<string, string>();

            var allTextsWithRegions = GetAllContainedDisplayTextsWithRegion(listViewEntryNode);

            foreach (var (cellText, cell) in allTextsWithRegions)
            {
                // Check if this text matches any header by region
                var maybeHeaderTextByCellRegion = entriesHeaders
                    .Where(header => header.Node != null && header.Text != null)
                    .Where(header =>
                    {
                        var headerRegion = header.Node.TotalDisplayRegion;
                        var cellRegion = cell.TotalDisplayRegion;
                        return (headerRegion.X < cellRegion.X + 3) &&
                               (headerRegion.X + headerRegion.Width > cellRegion.X + cellRegion.Width - 3);
                    })
                    .Select(header => header.Text)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(maybeHeaderTextByCellRegion))
                {
                    cellsTexts[maybeHeaderTextByCellRegion] = cellText;
                }
                else
                {
                    // Check if this is a tab-separated cell with multiple values
                    var distanceFromLeftmostHeader = cell.TotalDisplayRegion.X - leftmostNode.TotalDisplayRegion.X;
                    if (Math.Abs(distanceFromLeftmostHeader) >= 4)
                    {
                        var cellTexts = cellText.Split("<t>", StringSplitOptions.None)
                            .Select(t => t.Trim())
                            .ToList();

                        for (int i = 0; i < Math.Min(cellTexts.Count, entriesHeaders.Count); i++)
                        {
                            cellsTexts[entriesHeaders[i].Text] = cellTexts[i];
                        }
                    }
                }
            }

            return cellsTexts;
        }

        public static IEnumerable<string> GetAllContainedDisplayTexts(UITreeNodeNoDisplayRegion uiNode)
        {
            var allNodes = new List<UITreeNode> { uiNode };
            allNodes.AddRange(uiNode.ListDescendantsInUITreeNode());

            return allNodes
                .Select(GetDisplayText)
                .Where(text => text != null)
                .Select(text => text!); // null forgiving operator
        }

        public static List<(string Text, UITreeNodeWithDisplayRegion Node)> GetAllContainedDisplayTextsWithRegion(UITreeNodeWithDisplayRegion? uiNode)
        {
            if (uiNode == null)
                return [];

            var nodesWithText = new List<(string, UITreeNodeWithDisplayRegion)>();

            // Include the current node and all its descendants
            var allNodes = new List<UITreeNodeWithDisplayRegion> { uiNode };
            allNodes.AddRange(uiNode.ListDescendantsWithDisplayRegion());

            foreach (var node in allNodes)
            {
                var displayText = GetDisplayText(node);
                if (!string.IsNullOrEmpty(displayText))
                {
                    nodesWithText.Add((displayText, node));
                }
            }

            return nodesWithText;
        }

        public static UITreeNodeWithDisplayRegion? FindButtonInDescendantsContainingDisplayText(UITreeNodeWithDisplayRegion node, string displayText)
        {
            return node.ListDescendantsWithDisplayRegion()
                .Where(descendant => descendant.pythonObjectTypeName.Contains("Button"))
                .Where(node =>
                    UIParser.GetAllContainedDisplayTexts(node)
                    .Any(text => text.Contains(displayText, StringComparison.CurrentCultureIgnoreCase))
                )
                .OrderBy(descendant => descendant.TotalDisplayRegion.AreaFromDisplayRegion().GetValueOrDefault(0))
                .FirstOrDefault();
        }
    }
}


using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static eve_parse_ui.UITreeNodeWithDisplayRegion;

namespace eve_parse_ui
{
    public static class InventoryWindowsParser
    {
        private static readonly string[] inventoryWindowTypes = ["InventoryPrimary", "Inventory", "ActiveShipCargo"];
        private static readonly string[] shipInventories =
            [
                "ShipCargo", "ShipDroneBay", "ShipGeneralMiningHold",
                "StationItems", "ShipFleetHangar", "StructureItemHangar"
            ];

        public static IReadOnlyList<InventoryWindow> ParseInventoryWindowsFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
        {
            ArgumentNullException.ThrowIfNull(uiTreeRoot);

            return uiTreeRoot
                .ListDescendantsWithDisplayRegion()
                .Where(uiNode => inventoryWindowTypes.Contains(uiNode.pythonObjectTypeName ?? string.Empty))
                .Select(ParseInventoryWindow)
                .Where(window => window != null)
                .Cast<InventoryWindow>()
                .OrderBy(i => i.UiNode.pythonObjectTypeName.Length * -1)  // hack!!
                .ToList()
                .AsReadOnly();
        }

        public static InventoryWindow? ParseInventoryWindow(UITreeNodeWithDisplayRegion windowUiNode)
        {
            ArgumentNullException.ThrowIfNull(windowUiNode);

            var windowCaption = windowUiNode
                .GetDescendantsByType("WindowCaption")
                .SelectMany(UIParser.GetAllContainedDisplayTexts)
                .FirstOrDefault();

            var selectedContainerCapacityGaugeNode = windowUiNode
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(node => (node.pythonObjectTypeName ?? string.Empty).Contains("CapacityGauge"));

            var selectedContainerCapacityGauge = selectedContainerCapacityGaugeNode?
                .ListDescendantsInUITreeNode()
                .Select(UIParser.GetDisplayText)
                .Where(text => !string.IsNullOrEmpty(text))
                .OrderByDescending(text => text?.Length ?? 0)
                .Select(text => ParseInventoryCapacityGaugeText(text!))
                .FirstOrDefault();

            var rightContainerNode = windowUiNode
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(uiNode =>
                    uiNode.pythonObjectTypeName == "Container" &&
                    (uiNode.GetNameFromDictEntries() ?? string.Empty).Contains("right"));

            var leftContainer = windowUiNode
                .GetDescendantsByType("ScrollContainer")
                .Where(uiNode => uiNode.GetNameFromDictEntries()?.Equals("tree", StringComparison.CurrentCultureIgnoreCase) == true)
                .FirstOrDefault();

            if (rightContainerNode == null || leftContainer == null)
            {
                Debug.Fail("rightContainerNode is null");
                return null;
            }

            var subCaptionLabelText = rightContainerNode
                .ListDescendantsWithDisplayRegion()
                .Where(uiNode => (uiNode.GetNameFromDictEntries() ?? string.Empty).StartsWith("subCaptionLabel"))
                .SelectMany(UIParser.GetAllContainedDisplayTexts)
                .FirstOrDefault();
      
            var buttonToSwitchToListView = rightContainerNode?
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(uiNode =>
                    (uiNode.pythonObjectTypeName ?? string.Empty).Contains("ButtonIcon") &&
                    (uiNode.GetTexturePathFromDictEntries() ?? string.Empty).EndsWith("38_16_190.png"));

            var buttonToStackAll = rightContainerNode?
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(uiNode =>
                    (uiNode.pythonObjectTypeName ?? string.Empty).Contains("ButtonIcon") &&
                    (UIParser.GetHintTextFromDictEntries(uiNode) ?? string.Empty).Contains("Stack All"));

            return new InventoryWindow
            {
                UiNode = windowUiNode,
                WindowCaption = windowCaption ?? "Unknown window caption",
                LeftTreePanel = ParseLeftTreePanel(leftContainer),
                SubCaptionLabelText = subCaptionLabelText,
                SelectedContainerCapacityGauge = selectedContainerCapacityGauge,
                SelectedInventory = ParseInventory(rightContainerNode),
                ButtonToSwitchToListView = buttonToSwitchToListView,
                ButtonToStackAll = buttonToStackAll
            };
        }

        private static LeftTreePanel ParseLeftTreePanel(UITreeNodeWithDisplayRegion leftPanel)
        {
            var leftTreeEntriesRootNodes = GetContainedTreeViewEntryRootNodes(leftPanel);
            var leftTreeEntries = leftTreeEntriesRootNodes
                .Select(ParseInventoryWindowTreeViewEntry)
                .ToList();

            return new LeftTreePanel()
            {
                UiNode = leftPanel,
                Entries = leftTreeEntries,
                ScrollControls = UIParser.ParseScrollControls(leftPanel)
            };
        }

        private static InventoryItem ParseInventoryItemIcon(UITreeNodeWithDisplayRegion item)
        {
            var name = item
                .GetDescendantsByName("itemNameLabel")
                .SelectMany(UIParser.GetAllContainedDisplayTexts)
                .FirstOrDefault()?.Replace("<center>", "");

            var quantity = item
                .GetDescendantsByName("qtypar")
                .SelectMany(UIParser.GetAllContainedDisplayTexts)
                .FirstOrDefault() ?? "1";

            var multiplier = 1;
            if (quantity.EndsWith("M"))
            {
                multiplier = 1000000;
                quantity = quantity.Remove(quantity.Length - 1);
            }

            if (quantity.EndsWith("K"))
            {
                multiplier = 1000;
                quantity = quantity.Remove(quantity.Length - 1);
            }

            var quantityInt = (int)(float.Parse(quantity.Replace(",", "")) * multiplier);

            return new InventoryItem
            {
                UiNode = item,
                Name = name ?? "Unknown item",
                Quantity = quantityInt,
            };
        }

        private static InventoryItem? ParseInventoryItemText(DisplayTextWithRegion item)
        {
            // Name<t> <right>Quantity<t>type
            var regex = new Regex(@"(.*?)<t>\s?<right>(.*?)<t>(.*?)");
            var match = regex.Match(item.Text);
            if (match.Success && match.Groups.Count == 4)
            {
                return new InventoryItem
                {
                    UiNode = item.Region,
                    Name = match.Groups[1].Value.Trim(),
                    Quantity = int.Parse(match.Groups[2].Value.Trim()),
                };
            }

            return null;
        }

        public static Inventory ParseInventory(UITreeNodeWithDisplayRegion? inventoryNode)
        {
            ArgumentNullException.ThrowIfNull(inventoryNode);

            var items = inventoryNode
                .GetDescendantsByType("Item")
                .SelectMany(UIParser.GetAllContainedDisplayTextsWithRegion)
                .Select(ParseInventoryItemText)
                .Where(item => item != null)
                .Cast<InventoryItem>();

            var invItems = inventoryNode
                .GetDescendantsByType("InvItem")
                .Select(ParseInventoryItemIcon);

            var scrollNode = inventoryNode
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(node => (node.pythonObjectTypeName ?? string.Empty).Contains("scroll", StringComparison.CurrentCultureIgnoreCase));

            return new Inventory
            {
                UiNode = inventoryNode,
                Items = [.. items, .. invItems],
                ScrollControls = UIParser.ParseScrollControls(scrollNode)
            };
        }

        /// <summary>
        /// Returns the subsequence of items not contained in any of the other ones
        /// </summary>
        private static ReadOnlyCollection<UITreeNodeWithDisplayRegion> SubsequenceNotContainedInAnyOtherWithDisplayRegion(
            this IEnumerable<UITreeNodeWithDisplayRegion> original)
        {
            ArgumentNullException.ThrowIfNull(original);

            return original
                .Where(item => !original.Any(other =>
                    !ReferenceEquals(item, other) &&
                    NodeDescendantsContainWithDisplayRegion(other, item)))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Checks if the set of descendants of the second node contains the first node
        /// </summary>
        public static bool NodeDescendantsContainWithDisplayRegion(
            UITreeNodeNoDisplayRegion potentialAncestor,
            UITreeNodeNoDisplayRegion potentialDescendant)
        {
            var allChildren = potentialDescendant.ListDescendantsInUITreeNode();
            return allChildren.Contains(potentialAncestor);
        }

        /// <summary>
        /// Gets the most populous descendant matching the predicate
        /// </summary>
        public static UITreeNodeWithDisplayRegion? GetMostPopulousDescendantWithDisplayRegionMatchingPredicate(
            this UITreeNodeWithDisplayRegion parent,
            Func<UITreeNodeWithDisplayRegion, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(parent);
            ArgumentNullException.ThrowIfNull(predicate);

            return parent.ListDescendantsWithDisplayRegion()
                .Where(predicate)
                .OrderByDescending(CountDescendantsInUITreeNodeWithDisplayRegion)
                .FirstOrDefault();
        }

        /// <summary>
        /// Counts all descendants of a UI tree node with display region
        /// </summary>
        public static int CountDescendantsInUITreeNodeWithDisplayRegion(this UITreeNodeWithDisplayRegion parent)
        {
            ArgumentNullException.ThrowIfNull(parent);

            if (parent.Children == null)
                return 0;

            return parent.Children
                .Select(child => {
                    if (child is UITreeNodeWithDisplayRegion displayChild) {
                        return displayChild;
                    }
                    return null;
                })
                .Where(child => child != null)
                .Sum(child => CountDescendantsInUITreeNodeWithDisplayRegion(child!) + 1);
        }


        private static ListViewEntry ParseListViewEntry(
            IReadOnlyList<DisplayTextWithRegion> entriesHeaders,
            UITreeNodeWithDisplayRegion listViewEntryNode)
        {
            if (entriesHeaders.Count == 0)
                return new ListViewEntry { CellsTexts = [] };

            var leftmostHeader = entriesHeaders[0];
            var cellsTexts = new Dictionary<string, string>();
            var cellTexts = listViewEntryNode.GetAllContainedDisplayTextsWithRegion();

            foreach (var cell in cellTexts ?? [])
            {
                static bool HeaderRegionMatchesCellRegion(DisplayRegion headerRegion, DisplayRegion cellRegion)
                {
                    return (headerRegion.X < cellRegion.X + 3) &&
                           (headerRegion.X + headerRegion.Width > cellRegion.X + cellRegion.Width - 3);
                }

                var matchingHeader = entriesHeaders
                    .FirstOrDefault(header =>
                        HeaderRegionMatchesCellRegion(header.Region.TotalDisplayRegion, cell.Region.TotalDisplayRegion));

                if (matchingHeader != default)
                {
                    cellsTexts[matchingHeader.Text] = cell.Text;
                }
                else
                {
                    var distanceFromLeftmostHeader = cell.Region.TotalDisplayRegion.X - leftmostHeader.Region.TotalDisplayRegion.X;
                    if (Math.Abs(distanceFromLeftmostHeader) >= 4)
                    {
                        var cellSubTexts = cell.Text.Split(["<t>"], StringSplitOptions.None)
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrEmpty(t))
                            .ToList();

                        for (int i = 0; i < Math.Min(cellSubTexts.Count, entriesHeaders.Count); i++)
                        {
                            cellsTexts[entriesHeaders[i].Text] = cellSubTexts[i];
                        }
                    }
                }
            }

            return new ListViewEntry { CellsTexts = cellsTexts };
        }

        public record ListViewEntry
        {
            public required Dictionary<string, string> CellsTexts { get; init; }
        }

        public static List<UITreeNodeWithDisplayRegion> GetContainedTreeViewEntryRootNodes(UITreeNodeWithDisplayRegion parentNode)
        {
            var leftTreeEntriesAllNodes = parentNode
                .ListDescendantsWithDisplayRegion()
                .Where(node => (node.pythonObjectTypeName ?? string.Empty).StartsWith("TreeViewEntry"))
                .ToList();

            bool IsContainedInTreeEntry(UITreeNodeWithDisplayRegion candidate)
            {
                return leftTreeEntriesAllNodes
                    .SelectMany(node => node.ListDescendantsWithDisplayRegion())
                    .Any(descendant => descendant == candidate);
            }

            return leftTreeEntriesAllNodes
                .Where(node => !IsContainedInTreeEntry(node))
                .ToList();
        }

        public static InventoryWindowLeftTreeEntry ParseInventoryWindowTreeViewEntry(UITreeNodeWithDisplayRegion treeEntryNode)
        {
            var topContNode = treeEntryNode
                .ListDescendantsWithDisplayRegion()
                .Where(node => (node.GetNameFromDictEntries() ?? string.Empty).StartsWith("topCont_"))
                .OrderBy(node => node.TotalDisplayRegion.Y)
                .FirstOrDefault();

            if (topContNode == null)
                return null;

            var toggleBtn = topContNode
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(node => (node.GetNameFromDictEntries() ?? string.Empty) == "toggleBtn");

            var text = topContNode
                .GetAllContainedDisplayTextsWithRegion()?
                .OrderBy(x => x.Region.TotalDisplayRegion.Y)
                .Select(x => x.Text)
                .FirstOrDefault() ?? string.Empty;

            var selectedIndicator = topContNode
                .GetDescendantsByType("SelectionIndicatorLine")
                .FirstOrDefault();

            var isSelected = (UIParser.GetColorPercentFromDictEntries(selectedIndicator)?.A > 50) == true;

            var childrenNodes = GetContainedTreeViewEntryRootNodes(treeEntryNode);
            var children = childrenNodes
                .Select(ParseInventoryWindowTreeViewEntry)
                .ToList();

            return new InventoryWindowLeftTreeEntry
            {
                UiNode = treeEntryNode,
                ToggleBtn = toggleBtn,
                SelectRegion = topContNode,
                Text = text,
                IsSelected = isSelected,
                Children = children
            };
        }

        public static InventoryWindowCapacityGauge? ParseInventoryCapacityGaugeText(string capacityText)
        {
            if (string.IsNullOrWhiteSpace(capacityText))
            {
                Debug.WriteLine("Capacity text is null or empty");
                return null;
            }

            var cleanText = capacityText.Replace("m³", "", StringComparison.Ordinal);
            var parts = cleanText.Split('/');

            if (parts.Length == 1)
            {
                return ParseCapacityGaugePart(parts[0], null);
            } 
            else if (parts.Length == 2)
            {
                return ParseCapacityGaugePart(parts[0], parts[1]);
            }
            
            Debug.WriteLine($"Unexpected number of components in capacityText '{capacityText}'");
            return null;
        }

        private static InventoryWindowCapacityGauge? ParseCapacityGaugePart(string beforeSlashText, string? afterSlashText)
        {
            var beforeSlashParts = beforeSlashText.Trim().Split(')');
            var usedText = beforeSlashParts.Length > 1 ? beforeSlashParts[1] : beforeSlashParts[0];
            var maybeSelectedText = beforeSlashParts.Length > 1 ? beforeSlashParts[0].TrimStart('(') : null;

            var used = ParseNumberTruncatingAfterOptionalDecimalSeparator(usedText);

            var maybeMaximum = string.IsNullOrEmpty(afterSlashText)
                ? null
                : ParseNumberTruncatingAfterOptionalDecimalSeparator(afterSlashText);

            var maybeSelected = string.IsNullOrEmpty(maybeSelectedText)
                ? null
                : ParseNumberTruncatingAfterOptionalDecimalSeparator(maybeSelectedText);

            return new InventoryWindowCapacityGauge
            {
                Used = used ?? 0,
                Maximum = maybeMaximum,
                Selected = maybeSelected
            };
        }

        private static int? ParseNumberTruncatingAfterOptionalDecimalSeparator(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.WriteLine("Text is null or empty");
                return null;
            }

            // Find the position of the decimal separator
            var decimalIndex = text.IndexOf('.');
            if (decimalIndex == -1)
                decimalIndex = text.IndexOf(',');

            // If decimal separator found, take only the part before it
            var integerPart = decimalIndex >= 0
                ? text[..decimalIndex]
                : text;

            // Remove any non-digit characters
            var digitsOnly = new string(integerPart.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digitsOnly))
            {
                Debug.WriteLine($"No digits found in text '{text}'");
                return null;
            }

            if (int.TryParse(digitsOnly, out var result))
            {
                return result;
            }

            Debug.WriteLine($"Failed to parse '{text}' as integer");
            return null;
        }
    }
}
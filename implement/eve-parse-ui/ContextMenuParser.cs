
namespace eve_parse_ui
{
    public class ContextMenuParser
    {
        private static readonly string[] MenuTypes = [ "ContextMenu", "ContextSubMenu" ];

        public static List<ContextMenu> ParseContextMenusFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
        {
            // Find the layer menu node
            var layerMenu = uiTreeRoot.GetDescendantsByName("l_menu").FirstOrDefault();

            if (layerMenu == null)
            {
                return [];
            }

            // Find and parse all menu items
            return layerMenu
                .ListDescendantsWithDisplayRegion()
                .Where(node => MenuTypes.Contains(node.pythonObjectTypeName))
                .Select(ParseContextMenu)
                .ToList();
        }

        public static ContextMenu ParseContextMenu(UITreeNodeWithDisplayRegion contextMenuUINode)
        {
            // Get all menu entry nodes
            var entriesUINodes = contextMenuUINode
                .GetDescendantsByType("MenuEntryView")
                .ToList();

            // Process each entry
            var entries = entriesUINodes
                .Select(entryUINode =>
                {
                    // Find the display text for this entry
                    var text = UIParser.GetAllContainedDisplayTexts(entryUINode)
                        .Where(t => t != null)
                        .OrderByDescending(t => t!.Length)
                        .FirstOrDefault() ?? string.Empty;

                    return new ContextMenuEntry
                    {
                        Text = text,
                        UiNode = entryUINode
                    };
                })
                .OrderBy(entry => entry.UiNode.TotalDisplayRegion.Y)
                .ToList();

            return new ContextMenu
            {
                UiNode = contextMenuUINode,
                Entries = entries
            };
        }
    }
}
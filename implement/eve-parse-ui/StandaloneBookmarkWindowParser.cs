
namespace eve_parse_ui
{
    public class StandaloneBookmarkWindowParser
    {
        public static StandaloneBookmarkWindow? ParseStandaloneBookmarkWindowFromUITreeRoot(UITreeNodeNoDisplayRegion rootNode)
        {
            if (rootNode == null)
                return null;

            var standaloneBookmarkWindowNode = rootNode
                .GetDescendantsByType("StandaloneBookmarkWnd")
                .FirstOrDefault();

            if (standaloneBookmarkWindowNode == null)
                return null;

            var searchTextbox = standaloneBookmarkWindowNode
                .GetDescendantsByType("QuickFilterEdit")
                .SelectMany(node => node.GetDescendantsByName("_textClipper"))
                .FirstOrDefault();

            var entries = standaloneBookmarkWindowNode
                .GetDescendantsByType("PlaceEntry")
                .ToList();

            return new StandaloneBookmarkWindow()
            {
                UiNode = standaloneBookmarkWindowNode,
                SearchTextbox = searchTextbox,
                Entries = entries,
                ScrollBar = UIParser.ParseScrollBar(standaloneBookmarkWindowNode),
            };
        }
    }
}
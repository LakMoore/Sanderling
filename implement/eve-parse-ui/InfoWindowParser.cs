
namespace eve_parse_ui
{
    internal class InfoWindowParser
    {
        internal static IEnumerable<InfoWindow> ParseInfoWindowsFromUITreeRoot(UITreeNodeWithDisplayRegion rootNode)
        {
            return rootNode
                .GetDescendantsByType("InfoWindow")
                .Select(w => new InfoWindow()
                { 
                    UiNode = w 
                });
        }
    }
}

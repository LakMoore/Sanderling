
namespace eve_parse_ui
{
    internal class SelectedItemWindowParser
    {
        public static SelectedItemWindow? ParseSelectedItemWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiTreeRoot)
        {
            var windowNode = uiTreeRoot
                .GetDescendantsByType("ActiveItem")
                .FirstOrDefault();

            if (windowNode == null)
                return null;

            return ParseSelectedItemWindow(windowNode);
        }

        private static SelectedItemWindow ParseSelectedItemWindow(UITreeNodeWithDisplayRegion windowNode)
        {
            var orbitButton = ActionButtonFromTexturePathEnding(windowNode, "44_32_21.png");

            return new SelectedItemWindow
            {
                UiNode = windowNode,
                OrbitButton = orbitButton
            };
        }

        private static UITreeNodeWithDisplayRegion? ActionButtonFromTexturePathEnding(UITreeNodeWithDisplayRegion windowNode, string texturePathEnding)
        {
            var lowerTexturePathEnding = texturePathEnding.ToLower();
            return windowNode
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(node =>
                    UIParser.GetTexturePathFromDictEntries(node)?
                        .EndsWith(lowerTexturePathEnding, StringComparison.CurrentCultureIgnoreCase) == true
                );
        }
    }
}
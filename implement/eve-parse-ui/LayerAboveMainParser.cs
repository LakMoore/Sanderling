namespace eve_parse_ui
{
  internal class LayerAboveMainParser
  {
    public static LayerAboveMain? ParseLayerAbovemainFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var layerAboveMainUINode = uiTreeRoot.GetDescendantsByName("l_abovemain").FirstOrDefault();

      if (layerAboveMainUINode == null)
        return null;

      return new LayerAboveMain
      {
        UiNode = layerAboveMainUINode,
        QuickMessage = ParseQuickMessage(layerAboveMainUINode)
      };
    }

    private static QuickMessage? ParseQuickMessage(UITreeNodeWithDisplayRegion layerAboveMainUINode)
    {
      var quickMessageUINode = layerAboveMainUINode.GetDescendantsByType("QuickMessage").FirstOrDefault();

      if (quickMessageUINode == null)
        return null;

      var text = UIParser.GetAllContainedDisplayTexts(quickMessageUINode)
          .FirstOrDefault() ?? "";

      return new QuickMessage
      {
        UiNode = quickMessageUINode,
        Text = text
      };
    }
  }
}

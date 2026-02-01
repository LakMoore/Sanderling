


namespace eve_parse_ui
{
    internal class WindowStacksParser
    {
    internal static IEnumerable<WindowStack> ParseWindowStacksFromUITreeRoot(UITreeNodeWithDisplayRegion uiRoot)
    {
      if (uiRoot == null)
      {
        yield break;
      }

      var windowStackNodes = uiRoot.GetDescendantsByType("WindowStackHeader");
      foreach (var windowStackNode in windowStackNodes)
      {
        var windowStack = ParseWindowStack(windowStackNode);
        if (windowStack != null)
        {
          yield return windowStack;
        }
      }
    }

    private static WindowStack? ParseWindowStack(UITreeNodeWithDisplayRegion windowStackNode)
    {
      if (windowStackNode == null)
      {
        return null;
      }

      var tabNodes = windowStackNode.GetDescendantsByType("WindowStackTab");

      var tabs = tabNodes
        .Select(ParseWindowStackTab)
        .Where(tab => tab != null)
        .Cast<WindowStackTab>()
        .ToList();

      return new WindowStack { 
        Tabs = tabs, 
        UiNode = windowStackNode 
      };
    }

    private static WindowStackTab? ParseWindowStackTab(UITreeNodeWithDisplayRegion tabNode)
    {
      if (tabNode == null)
      {
        return null;
      }

      var label = tabNode.GetDescendantsByType("EveLabelMedium").FirstOrDefault();

      if (label == null)
      {
        return null;
      }

      return new WindowStackTab
      {
        UiNode = tabNode,
        Name = UIParser.GetDisplayText(label) ?? "Unnamed Tab",
        IsActive = UIParser.GetColorPercentFromDictEntries(label)?.A > 50
      };

    }
  }
}
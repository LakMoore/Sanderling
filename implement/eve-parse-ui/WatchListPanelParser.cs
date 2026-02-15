namespace eve_parse_ui
{
  internal record WatchListPanelParser
  {
    internal static WatchListPanel? ParseWatchListPanelFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var watchListPanelNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "WatchListPanel" ||
                              n.pythonObjectTypeName == "WatchList");

      if (watchListPanelNode == null)
        return null;

      return ParseWatchListPanel(watchListPanelNode);
    }

    private static WatchListPanel ParseWatchListPanel(UITreeNodeWithDisplayRegion panelNode)
    {
      var entryNodes = panelNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "WatchListEntry" ||
                     n.pythonObjectTypeName == "Entry" ||
                     n.pythonObjectTypeName == "ScrollEntry")
          .ToList();

      var entries = entryNodes.Select(ParseWatchListEntry).ToList();

      return new WatchListPanel
      {
        UiNode = panelNode,
        Entries = entries
      };
    }

    private static WatchListEntry ParseWatchListEntry(UITreeNodeWithDisplayRegion entryNode)
    {
      var textsLeftToRight = UIParser.GetAllContainedDisplayTextsWithRegion(entryNode)
          .OrderBy(t => t.Region.TotalDisplayRegion.X)
          .Select(t => t.Text)
          .ToList();

      var name = textsLeftToRight.ElementAtOrDefault(0);
      var typeName = textsLeftToRight.ElementAtOrDefault(1);
      var distance = textsLeftToRight.ElementAtOrDefault(2);

      var isSelected = entryNode.GetBoolFromDictEntries("isSelected") ?? false;

      return new WatchListEntry
      {
        UiNode = entryNode,
        Name = name,
        TypeName = typeName,
        Distance = distance,
        IsSelected = isSelected
      };
    }
  }
}

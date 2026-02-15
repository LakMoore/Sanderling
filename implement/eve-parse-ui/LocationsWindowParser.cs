namespace eve_parse_ui
{
  internal record LocationsWindowParser
  {
    internal static LocationsWindow? ParseLocationsWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var locationsWindowNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "LocationsWindow" ||
                              n.pythonObjectTypeName == "PlacesWindow");

      if (locationsWindowNode == null)
        return null;

      return ParseLocationsWindow(locationsWindowNode);
    }

    private static LocationsWindow ParseLocationsWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      // Parse tree structure for folders and bookmarks
      var treeViewNodes = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "TreeView" ||
                     n.pythonObjectTypeName == "ScrollContainer")
          .FirstOrDefault();

      var folderNodes = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "LocationFolder" ||
                     n.pythonObjectTypeName == "TreeViewFolder" ||
                     (n.GetNameFromDictEntries()?.Contains("folder", StringComparison.OrdinalIgnoreCase) == true))
          .ToList();

      var folders = folderNodes.Select(ParseLocationFolder).ToList();

      var bookmarkNodes = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "LocationBookmark" ||
                     n.pythonObjectTypeName == "TreeViewBookmark" ||
                     n.pythonObjectTypeName == "BookmarkEntry")
          .ToList();

      var bookmarks = bookmarkNodes.Select(ParseLocationBookmark).ToList();

      // Also parse legacy PlaceEntry nodes for backwards compatibility
      var placeEntries = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "PlaceEntry" ||
                     n.pythonObjectTypeName == "TreeViewEntry")
          .Select(entryNode => new LocationsWindowPlaceEntry
          {
            UiNode = entryNode,
            MainText = UIParser.GetAllContainedDisplayTexts(entryNode).FirstOrDefault() ?? ""
          })
          .ToList();

      return new LocationsWindow
      {
        UiNode = windowNode,
        Folders = folders,
        Bookmarks = bookmarks,
        PlaceEntries = placeEntries
      };
    }

    private static LocationFolder ParseLocationFolder(UITreeNodeWithDisplayRegion folderNode)
    {
      var name = UIParser.GetAllContainedDisplayTexts(folderNode).FirstOrDefault();
      var isExpanded = folderNode.GetBoolFromDictEntries("isExpanded") ?? false;

      // Parse subfolders recursively
      var subFolderNodes = folderNode.ListDescendantsWithDisplayRegion()
          .Where(n => n != folderNode &&
                     (n.pythonObjectTypeName == "LocationFolder" ||
                      n.pythonObjectTypeName == "TreeViewFolder"))
          .ToList();

      var subFolders = subFolderNodes.Select(ParseLocationFolder).ToList();

      // Parse bookmarks in this folder
      var bookmarkNodes = folderNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "LocationBookmark" ||
                     n.pythonObjectTypeName == "BookmarkEntry")
          .ToList();

      var bookmarks = bookmarkNodes.Select(ParseLocationBookmark).ToList();

      return new LocationFolder
      {
        UiNode = folderNode,
        Name = name,
        IsExpanded = isExpanded,
        SubFolders = subFolders,
        Bookmarks = bookmarks
      };
    }

    private static LocationBookmark ParseLocationBookmark(UITreeNodeWithDisplayRegion bookmarkNode)
    {
      var textsLeftToRight = UIParser.GetAllContainedDisplayTextsWithRegion(bookmarkNode)
          .OrderBy(t => t.Region.TotalDisplayRegion.X)
          .Select(t => t.Text)
          .ToList();

      var name = textsLeftToRight.ElementAtOrDefault(0);
      var location = textsLeftToRight.ElementAtOrDefault(1);
      var notes = bookmarkNode.GetStringFromDictEntries("notes");

      // Parse creation date if available
      DateTime? created = null;
      var createdStr = bookmarkNode.GetStringFromDictEntries("created");
      if (!string.IsNullOrEmpty(createdStr) && DateTime.TryParse(createdStr, out var date))
      {
        created = date;
      }

      var isSelected = bookmarkNode.GetBoolFromDictEntries("isSelected") ?? false;

      return new LocationBookmark
      {
        UiNode = bookmarkNode,
        Name = name,
        Location = location,
        Notes = notes,
        Created = created,
        IsSelected = isSelected
      };
    }
  }
}

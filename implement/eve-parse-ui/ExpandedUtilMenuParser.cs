namespace eve_parse_ui
{
  public static class ExpandedUtilMenuParser
  {
    public static ExpandedUtilMenu? ParseExpandedUtilMenuFromUITreeRoot(UITreeNodeNoDisplayRegion uiRoot)
    {
      var expandedUtilMenu = uiRoot
          .GetDescendantsByType("ExpandedUtilMenu")
          .FirstOrDefault();

      if (expandedUtilMenu == null)
        return null;

      var searchBox = expandedUtilMenu
          .GetDescendantsByType("SingleLineEditText")
          .Where(n => n.GetNameFromDictEntries() == "searchbox")
          .FirstOrDefault();

      var searchButton = expandedUtilMenu
          .GetDescendantsByType("Button")
          .Where(n =>
              n.GetAllContainedDisplayTextsWithRegion()
                  .Any(t => t.Text == "Search")
          )
          .FirstOrDefault();

      if (searchBox == null || searchButton == null)
      {
        // Ship fitting inventory tooltip doesn't have a searchbox
        // Debug.Fail("Found a menu but not the components!");
        // return null;
      }

      return new ExpandedUtilMenu()
      {
        UiNode = expandedUtilMenu,
        SearchBox = searchBox,
        SearchButton = searchButton
      };
    }
  }
}
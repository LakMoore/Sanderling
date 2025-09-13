namespace eve_parse_ui
{
  public record StationWindowParser
  {

    public static StationWindow? ParseStationWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var lobbyWindowNode =
          uiTreeRoot.ListDescendantsWithDisplayRegion()
          .Where(node => node.pythonObjectTypeName == "LobbyWnd")
          .FirstOrDefault();

      if (lobbyWindowNode == null)
      {
        return null;
      }

      StationWindow window = new()
      {
        UiNode = lobbyWindowNode,
        UndockButton = UIParser.FindButtonInDescendantsContainingDisplayText(lobbyWindowNode, "undock"),
        AbortUndockButton = UIParser.FindButtonInDescendantsContainingDisplayText(lobbyWindowNode, "abort undock"),
        DockedModeButton = UIParser.FindButtonInDescendantsContainingDisplayText(lobbyWindowNode, "view"),
      };

      return window;
    }
  }
}

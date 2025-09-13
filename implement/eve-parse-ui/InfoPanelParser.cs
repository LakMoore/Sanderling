using System.Text.RegularExpressions;

namespace eve_parse_ui
{
  internal record InfoPanelParser
  {
    public static InfoPanelContainer? ParseInfoPanelContainerFromUIRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var containerNode =
          uiTreeRoot.GetDescendantsByType("InfoPanelContainer")
          .OrderByDescending(n => UIParser.CountDescendantsInUITreeNode(n) * -1)
          .FirstOrDefault();

      if (containerNode == null)
        return null;

      var searchBox = containerNode
          .GetDescendantsByType("SingleLineEditText")
          .Where(n => n.GetNameFromDictEntries() == "searchEdit")
          .FirstOrDefault();

      return new InfoPanelContainer
      {
        UiNode = containerNode,
        Icons = ParseInfoPanelIconsFromInfoPanelContainer(containerNode),
        InfoPanelLocationInfo = ParseInfoPanelLocationInfoFromInfoPanelContainer(containerNode),
        InfoPanelRoute = ParseInfoPanelRouteFromInfoPanelContainer(containerNode),
        InfoPanelAgentMissions = ParseInfoPanelAgentMissionsFromInfoPanelContainer(containerNode),
        SearchBox = searchBox
      };
    }

    public static InfoPanelIcons? ParseInfoPanelIconsFromInfoPanelContainer(UITreeNodeWithDisplayRegion infoPanelContainerNode)
    {
      var iconContainerNode =
          infoPanelContainerNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.GetNameFromDictEntries() == "iconCont")
          .OrderBy(n => n.TotalDisplayRegion.Y)
          .FirstOrDefault();

      if (iconContainerNode == null)
        return null;

      Func<string, UITreeNodeWithDisplayRegion?> iconNodeFromTexturePathEnd = texturePathEnd =>
          iconContainerNode.ListDescendantsWithDisplayRegion()
              .Where(n => UIParser.GetTexturePathFromDictEntries(n)?.EndsWith(texturePathEnd) == true)
              .FirstOrDefault();

      return new InfoPanelIcons
      {
        UiNode = iconContainerNode,
        Search = iconNodeFromTexturePathEnd("search.png"),
        LocationInfo = iconNodeFromTexturePathEnd("LocationInfo.png"),
        Route = iconNodeFromTexturePathEnd("Route.png"),
        AgentMissions = iconNodeFromTexturePathEnd("Missions.png"),
        DailyChallenge = iconNodeFromTexturePathEnd("dailyChallenge.png")
      };
    }

    public static InfoPanelLocationInfo? ParseInfoPanelLocationInfoFromInfoPanelContainer(UITreeNodeWithDisplayRegion infoPanelContainerNode)
    {
      var infoPanelNode =
          infoPanelContainerNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "InfoPanelLocationInfo")
          .FirstOrDefault();

      if (infoPanelNode == null)
        return null;

      int? securityStatusPercent = null;
      string? securityStatusColor = null;
      string? currentSolarSystemName = null;

      var texts = UIParser.GetAllContainedDisplayTexts(infoPanelNode);
      foreach (var text in texts)
      {
        if (ParseSecurityStatusFromUINodeText(text) is Tuple<string?, int?> status)
        {
          securityStatusColor = status?.Item1;
          securityStatusPercent = status?.Item2;
        }

        if (ParseCurrentSolarSystemFromUINodeText(text) is string solarSystemName)
          currentSolarSystemName = solarSystemName;
      }

      var maybeListSurroundingsButton =
          infoPanelContainerNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "ListSurroundingsBtn")
          .FirstOrDefault();

      InfoPanelLocationInfoExpandedContent? expandedContent = null;
      if (infoPanelNode != null)
      {
        var expandedContainer =
            infoPanelNode.ListDescendantsWithDisplayRegion()
            .Where(n =>
                n.pythonObjectTypeName.Contains("Container")
                && n.GetNameFromDictEntries()?.Contains("mainCont") == true
            )
            .FirstOrDefault();

        if (expandedContainer != null)
        {
          var textsInExpandedContainer = UIParser.GetAllContainedDisplayTexts(expandedContainer);
          foreach (var text in textsInExpandedContainer)
          {
            if (ParseCurrentStationNameFromInfoPanelLocationInfoLabelText(text) is string stationName)
              expandedContent = new InfoPanelLocationInfoExpandedContent { CurrentStationName = stationName };
          }
        }
      }

      return maybeListSurroundingsButton != null
          ? new InfoPanelLocationInfo
          {
            UiNode = infoPanelNode!,
            ListSurroundingsButton = maybeListSurroundingsButton,
            CurrentSolarSystemName = currentSolarSystemName,
            SecurityStatusPercent = securityStatusPercent,
            SecurityStatusColor = securityStatusColor,
            ExpandedContent = expandedContent
          }
          : null;
    }

    public static Tuple<string?, int?>? ParseSecurityStatusFromUINodeText(string text)
    {
      var match = Regex.Match(text, @"hint='Security status'>(.*?)</color>");
      if (match.Success)
      {
        if (float.TryParse(match.Groups[1].Value.Trim(), out float value))
          return new(null, (int)(value * 100));
      }

      match = Regex.Match(text, @"hint=""Security status""><color=(.*?)>(.*?)</color>");
      if (match.Success)
      {
        if (float.TryParse(match.Groups[2].Value.Trim(), out float value))
          return new(match.Groups[1].Value.Trim(), (int)(value * 100));
      }

      return null;
    }

    public static string? ParseCurrentSolarSystemFromUINodeText(string text)
    {
      var match = Regex.Match(text, @"Current Solar System:(.*)");
      if (match.Success)
        return match.Groups[1].Value.Trim();

      match = Regex.Match(text, @"alt='Current Solar System'>(.*?)</color>");
      if (match.Success)
        return match.Groups[1].Value.Trim();

      match = Regex.Match(text, @"alt=""Current Solar System""><color=(.*?)>(.*?)</color>");
      if (match.Success)
        return match.Groups[2].Value.Trim();

      return null;
    }

    public static string? ParseCurrentStationNameFromInfoPanelLocationInfoLabelText(string text)
    {
      var match = Regex.Match(text, @"<url=(.*?) alt='Current Station'>(.*?)</url>");
      if (match.Success && match.Groups.Count == 3)
        return match.Groups[2].Value.Trim();

      return null;
    }

    public static InfoPanelRoute? ParseInfoPanelRouteFromInfoPanelContainer(UITreeNodeWithDisplayRegion infoPanelContainerNode)
    {
      var infoPanelRouteNode =
          infoPanelContainerNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "InfoPanelRoute")
          .FirstOrDefault();

      if (infoPanelRouteNode == null)
        return null;

      var isExpanded = infoPanelRouteNode
          .GetDescendantsByName("mainCont")
          .FirstOrDefault() != null;

      var autopilotMenuButton =
          infoPanelRouteNode.GetDescendantsByType("UtilMenu")
          .FirstOrDefault();

      var expandRouteButton =
          infoPanelRouteNode.GetDescendantsByType("ExpandButton")
          .FirstOrDefault();

      var routeElementMarkers =
          infoPanelRouteNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "AutopilotDestinationIcon")
          .Select(n => new InfoPanelRouteRouteElementMarker { UiNode = n })
          .ToList();

      return new InfoPanelRoute
      {
        UiNode = infoPanelRouteNode,
        IsExpanded = isExpanded,
        ExpandRouteButton = expandRouteButton,
        AutopilotMenuButton = autopilotMenuButton,
        RouteElementMarkers = routeElementMarkers
      };
    }

    public static InfoPanelAgentMissions? ParseInfoPanelAgentMissionsFromInfoPanelContainer(UITreeNodeWithDisplayRegion infoPanelContainerNode)
    {
      var infoPanelNode =
          infoPanelContainerNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "InfoPanelAgentMissions")
          .FirstOrDefault();

      if (infoPanelNode == null)
        return null;

      var entries =
          infoPanelNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "MissionEntry")
          .Select(n => new InfoPanelAgentMissionsEntry { UiNode = n })
          .ToList();

      return new InfoPanelAgentMissions
      {
        UiNode = infoPanelNode,
        Entries = entries
      };
    }
  }
}

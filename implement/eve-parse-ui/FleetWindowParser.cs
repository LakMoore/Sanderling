namespace eve_parse_ui
{
  internal record FleetWindowParser
  {
    internal static FleetWindow? ParseFleetWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var fleetWindowNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "FleetWindow" ||
                              n.pythonObjectTypeName == "FleetView");

      if (fleetWindowNode == null)
        return null;

      return ParseFleetWindow(fleetWindowNode);
    }

    private static FleetWindow ParseFleetWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      var memberNodes = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "FleetMember" ||
                     n.pythonObjectTypeName == "FleetEntry" ||
                     n.pythonObjectTypeName == "ScrollEntry")
          .ToList();

      var members = memberNodes.Select(ParseFleetMember).ToList();

      // Parse fleet name from header
      var fleetName = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName.Contains("Label") || n.pythonObjectTypeName.Contains("Header"))
          .Select(n => UIParser.GetDisplayText(n))
          .FirstOrDefault(t => !string.IsNullOrEmpty(t));

      // Check if user is commander (presence of certain buttons)
      var isCommander = windowNode.ListDescendantsWithDisplayRegion()
          .Any(n => UIParser.GetAllContainedDisplayTexts(n)
              .Any(t => t?.Contains("Fleet Settings", StringComparison.OrdinalIgnoreCase) == true));

      // Find action buttons
      var inviteButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => UIParser.GetAllContainedDisplayTexts(n)
              .Any(t => t?.Contains("Invite", StringComparison.OrdinalIgnoreCase) == true));

      var leaveButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => UIParser.GetAllContainedDisplayTexts(n)
              .Any(t => t?.Contains("Leave", StringComparison.OrdinalIgnoreCase) == true));

      return new FleetWindow
      {
        UiNode = windowNode,
        Members = members,
        FleetName = fleetName,
        IsCommander = isCommander,
        InviteButton = inviteButton,
        LeaveButton = leaveButton
      };
    }

    private static FleetMember ParseFleetMember(UITreeNodeWithDisplayRegion memberNode)
    {
      var textsLeftToRight = UIParser.GetAllContainedDisplayTextsWithRegion(memberNode)
          .OrderBy(t => t.Region.TotalDisplayRegion.X)
          .Select(t => t.Text)
          .ToList();

      var name = textsLeftToRight.ElementAtOrDefault(0);
      var shipType = textsLeftToRight.ElementAtOrDefault(1);
      var systemName = textsLeftToRight.ElementAtOrDefault(2);
      var distanceStr = textsLeftToRight.ElementAtOrDefault(3);

      // Parse distance
      int? distance = null;
      if (!string.IsNullOrEmpty(distanceStr))
      {
        var match = System.Text.RegularExpressions.Regex.Match(distanceStr, @"([\d,]+)");
        if (match.Success)
        {
          var valueStr = match.Groups[1].Value.Replace(",", "");
          if (int.TryParse(valueStr, out var dist))
          {
            distance = dist;
          }
        }
      }

      var isWarping = memberNode.GetBoolFromDictEntries("isWarping") ?? false;
      var isInFleetHangar = memberNode.GetBoolFromDictEntries("isInFleetHangar") ?? false;
      var isInFleetHangarAccessAllowed = memberNode.GetBoolFromDictEntries("isInFleetHangarAccessAllowed") ?? false;

      return new FleetMember
      {
        UiNode = memberNode,
        Name = name,
        ShipType = shipType,
        SystemName = systemName,
        SolarSystemId = null, // Would need additional parsing
        Distance = distance,
        IsWarping = isWarping,
        IsInFleetHangar = isInFleetHangar,
        IsInFleetHangarAccessAllowed = isInFleetHangarAccessAllowed
      };
    }
  }
}

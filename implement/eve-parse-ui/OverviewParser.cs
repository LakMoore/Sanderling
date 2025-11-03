using static eve_parse_ui.UITreeNodeWithDisplayRegion;

namespace eve_parse_ui
{
  internal record OverviewParser
  {
    private static readonly string[] overviewNames = new[] { "OverView", "OverviewWindow", "OverviewWindowOld" };
    internal static IReadOnlyList<OverviewWindow> ParseOverviewWindowsFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      return uiTreeRoot.ListDescendantsWithDisplayRegion()
          .Where(n => overviewNames.Contains(n.pythonObjectTypeName))
          .Select(ParseOverviewWindow)
          .Where(o => o != null)
          .Cast<OverviewWindow>()
          .ToList();
    }

    private static OverviewWindow? ParseOverviewWindow(UITreeNodeWithDisplayRegion overviewWindowNode)
    {
      var selectionIndicatorLine = overviewWindowNode.GetDescendantsByType("SelectionIndicatorLine").FirstOrDefault();

      var minimiseButton = overviewWindowNode
        .GetDescendantsByType("WindowControls")
        .FirstOrDefault()?
        .GetDescendantsByType("WindowControlButton")
        .FirstOrDefault()?
        .GetDescendantsByName("MinimizeButtonIcon")
        .FirstOrDefault();

      var overviewTabs = overviewWindowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName.Equals("OverviewTab", StringComparison.CurrentCultureIgnoreCase));

      var overviewTabsTexts = overviewTabs
          .SelectMany(t => UIParser.GetAllContainedDisplayTextsWithRegion(t))
          .Select(t => t.Text ?? "NoName")
          .ToList();

      var overviewTabText = overviewTabs
          .Where(t => t.SelfDisplayRegion.X == selectionIndicatorLine?.SelfDisplayRegion.X)
          .Select(UIParser.GetAllContainedDisplayTextsWithRegion)
          .FirstOrDefault()?.FirstOrDefault();

      var scrollNode = overviewWindowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName.Contains("scroll", StringComparison.CurrentCultureIgnoreCase));

      var headersContainerNode = scrollNode?.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName.Contains("headers", StringComparison.CurrentCultureIgnoreCase));

      var entriesHeader = headersContainerNode?.GetAllContainedDisplayTextsWithRegion();

      if (entriesHeader == null)
        return null;

      var entries = overviewWindowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName.Equals("OverviewScrollEntry"))
          .Select(n => ParseOverviewWindowEntry(entriesHeader, n));

      return new OverviewWindow
      {
        UiNode = overviewWindowNode,
        OverviewTabName = overviewTabText?.Text ?? "NoName",
        MinimiseButton = minimiseButton,
        EntriesHeader = entriesHeader,
        Entries = entries,
        Tabs = overviewTabsTexts,
        ScrollingPanel = UIParser.ParseScrollingPanel(overviewWindowNode)
      };
    }

    private static OverviewWindowEntry ParseOverviewWindowEntry(List<DisplayTextWithRegion> entriesHeaders, UITreeNodeWithDisplayRegion overviewEntryNode)
    {
      // Get all display texts from the node, sorted left to right
      var textsLeftToRight = UIParser.GetAllContainedDisplayTextsWithRegion(overviewEntryNode)
          .OrderBy(t => t.Region.TotalDisplayRegion.X)
          .Select(t => t.Text)
          .ToList();

      // Parse list view entry
      var listViewEntry = UIParser.ParseListViewEntry(entriesHeaders, overviewEntryNode);

      // Parse distance
      var objectDistance = listViewEntry.GetValueOrDefault("Distance");
      var objectDistanceInMeters = ParseOverviewEntryDistanceInMetersFromText(objectDistance);

      // Find space object icon
      var spaceObjectIconNode = overviewEntryNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "SpaceObjectIcon");

      // Get icon sprite color
      var iconSpriteColorPercent = spaceObjectIconNode != null
          ? UIParser.GetColorPercentFromDictEntries(
              spaceObjectIconNode.ListDescendantsWithDisplayRegion()
                  .FirstOrDefault(n => n.GetNameFromDictEntries() == "iconSprite"))
          : null;

      // Get names under space object icon
      var namesUnderSpaceObjectIcon = spaceObjectIconNode?.ListDescendantsInUITreeNode()
              .Select(n => n.GetNameFromDictEntries())
              .Select(s => s ?? string.Empty)
              .Where(s => !string.IsNullOrEmpty(s))
              .ToHashSet();

      // Get background color fills
      var bgColorFillsPercent = overviewEntryNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "Fill")
          .Where(n => n.GetNameFromDictEntries() == "bgColor")
          .Select(n => UIParser.GetColorPercentFromDictEntries(n))
          .Where(c => c != null)
          .Select(c => c!)  // null forgiving operator!  ColorComponent? --> ColorComponent
          .ToList();

      // Get right-aligned icons hints
      var rightAlignedIconsHints = overviewEntryNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.GetNameFromDictEntries() == "rightAlignedIconContainer")
          .SelectMany(n => n.ListDescendantsWithDisplayRegion())
          .Select(n => UIParser.GetHintTextFromDictEntries(n))
          .Where(hint => !string.IsNullOrEmpty(hint))
          .ToList();

      // Common indications
      var commonIndications = new OverviewWindowEntryCommonIndications
      {
        Targeting = namesUnderSpaceObjectIcon?.Contains("targeting") == true,
        TargetedByMe = namesUnderSpaceObjectIcon?.Contains("targetedByMeIndicator") == true,
        IsJammingMe = rightAlignedIconsHints.Any(hint => hint?.Contains("is jamming me", StringComparison.OrdinalIgnoreCase) == true),
        IsWarpDisruptingMe = rightAlignedIconsHints.Any(hint => hint?.Contains("is warp disrupting me", StringComparison.OrdinalIgnoreCase) == true)
      };

      // Get opacity
      int opacityPercent = (int)Math.Round(UIParser.GetOpacityFloatFromDictEntries(overviewEntryNode) * 100);

      // Get Velocity
      int velocity;
      string velocityText = listViewEntry.GetValueOrDefault("Velocity (m/s)") ?? "";
      TryParseNumberTruncatingAfterOptionalDecimalSeparator(velocityText, out velocity);

      // Create and return the entry
      return new OverviewWindowEntry
      {
        UiNode = overviewEntryNode,
        TextsLeftToRight = textsLeftToRight.ToArray(),
        CellsTexts = listViewEntry,
        ObjectDistance = objectDistance,
        ObjectDistanceInMeters = objectDistanceInMeters,
        ObjectName = listViewEntry.GetValueOrDefault("Name"),
        ObjectType = listViewEntry.GetValueOrDefault("Type"),
        ObjectCorporation = listViewEntry.GetValueOrDefault("Corporation"),
        ObjectAlliance = listViewEntry.GetValueOrDefault("Alliance"),
        ObjectVelocity = velocity,
        IconSpriteColorPercent = iconSpriteColorPercent,
        NamesUnderSpaceObjectIcon = namesUnderSpaceObjectIcon?.ToList(),
        BgColorFillsPercent = bgColorFillsPercent,
        RightAlignedIconsHints = rightAlignedIconsHints,
        CommonIndications = commonIndications,
        OpacityPercent = opacityPercent
      };
    }

    public static int ParseOverviewEntryDistanceInMetersFromText(string? distanceDisplayTextBeforeTrim)
    {
      if (string.IsNullOrEmpty(distanceDisplayTextBeforeTrim))
        return -1;

      var trimmedText = distanceDisplayTextBeforeTrim.Trim();
      var parts = trimmedText.Split(' ');
      Array.Reverse(parts);

      if (parts.Length < 2)
      {
        Console.WriteLine($"ParseOverviewEntryDistanceInMetersFromText('{distanceDisplayTextBeforeTrim}): Expecting at least one whitespace character separating number and unit.");
        return -1;
      }

      var unitText = parts[0];
      var reversedNumberTexts = parts.Skip(1).Reverse().ToArray();

      if (!TryParseDistanceUnitInMeters(unitText, out int unitInMeters))
      {
        Console.WriteLine($"Failed to parse distance unit text of '{unitText}'");
        return -1;
      }

      var numberText = string.Join(" ", reversedNumberTexts);
      if (!TryParseNumberTruncatingAfterOptionalDecimalSeparator(numberText, out int parsedNumber))
      {
        Console.WriteLine($"Failed to parse number: {numberText}");
        return -1;
      }

      return parsedNumber * unitInMeters;
    }

    private static bool TryParseDistanceUnitInMeters(string unitText, out int unitInMeters)
    {
      // implement parsing logic for distance unit here
      // for example:
      switch (unitText.ToLower())
      {
        case "m":
          unitInMeters = 1;
          return true;
        case "km":
          unitInMeters = 1000;
          return true;
        // add more cases as needed
        default:
          unitInMeters = 0;
          return false;
      }
    }

    public static bool TryParseNumberTruncatingAfterOptionalDecimalSeparator(string numberDisplayText, out int parsedNumber)
    {
      var expectedSeparators = new[] { ",", ".", "'", " ", "\u00A0", "\u202F" };

      var groupsTexts = expectedSeparators
          .Aggregate(
              new[] { numberDisplayText.Trim() },
              (texts, separator) => texts.SelectMany(text =>
                  text.Split(new[] { separator }, StringSplitOptions.None)
                  ).ToArray());

      var lastGroupIsFraction = groupsTexts.Length > 1 && groupsTexts.Last().Length < 3;

      var integerText = string.Join("", lastGroupIsFraction
          ? groupsTexts.Reverse().Skip(1).Reverse()
          : groupsTexts);

      if (Int32.TryParse(integerText, out parsedNumber))
      {
        return true;
      }

      parsedNumber = -1;
      return false;
    }

  }
}

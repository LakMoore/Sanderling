namespace eve_parse_ui
{
  internal record TargetParser
  {
    internal static IEnumerable<Target> ParseTargetsFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var targetNodes = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "Target" || 
                     n.pythonObjectTypeName == "TargetInBar" ||
                     n.pythonObjectTypeName == "SelectedTarget");

      foreach (var targetNode in targetNodes)
      {
        var target = ParseTarget(targetNode);
        if (target != null)
          yield return target;
      }
    }

    private static Target? ParseTarget(UITreeNodeWithDisplayRegion targetNode)
    {
      var barAndImageCont = targetNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.GetNameFromDictEntries()?.Contains("barAndImageCont", StringComparison.OrdinalIgnoreCase) == true);

      var textsTopToBottom = UIParser.GetAllContainedDisplayTextsWithRegion(targetNode)
          .OrderBy(t => t.Region.TotalDisplayRegion.Y)
          .Select(t => t.Text)
          .ToList();

      var isActiveTarget = targetNode.ListDescendantsWithDisplayRegion()
          .Any(n => n.GetNameFromDictEntries()?.Contains("activeTarget", StringComparison.OrdinalIgnoreCase) == true);

      var assignedContainerNode = targetNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.GetNameFromDictEntries()?.Contains("assigned", StringComparison.OrdinalIgnoreCase) == true);

      var assignedIcons = assignedContainerNode?.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName?.Contains("Icon", StringComparison.OrdinalIgnoreCase) == true)
          .ToList() ?? new List<UITreeNodeWithDisplayRegion>();

      // Parse all icon hints
      var allIconHints = targetNode.ListDescendantsWithDisplayRegion()
          .Select(n => UIParser.GetHintTextFromDictEntries(n))
          .Where(hint => !string.IsNullOrEmpty(hint))
          .ToList();

      // Parse icon names
      var allIconNames = targetNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName?.Contains("Icon", StringComparison.OrdinalIgnoreCase) == true)
          .Select(n => n.GetNameFromDictEntries())
          .Where(name => !string.IsNullOrEmpty(name))
          .ToList();

      // Parse bar percent and color from the bar container
      int? barPercent = null;
      ColorComponents? barColor = null;
      if (barAndImageCont != null)
      {
        // Look for bar nodes with fill percentage
        var barNode = barAndImageCont.ListDescendantsWithDisplayRegion()
            .FirstOrDefault(n => n.pythonObjectTypeName?.Contains("Bar", StringComparison.OrdinalIgnoreCase) == true ||
                                n.GetNameFromDictEntries()?.Contains("bar", StringComparison.OrdinalIgnoreCase) == true);

        if (barNode != null)
        {
          barPercent = barNode.GetIntFromDictEntries("_value");
          barColor = UIParser.GetColorPercentFromDictEntries(barNode);
        }
      }

      // Parse Icons - get all icon names
      var icons = targetNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName?.Contains("Icon", StringComparison.OrdinalIgnoreCase) == true)
          .Select(n => n.GetNameFromDictEntries())
          .Where(name => !string.IsNullOrEmpty(name))
          .Distinct()
          .ToList();

      // Parse IconsWithHints - combine icon names with their hints
      var iconsWithHints = targetNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName?.Contains("Icon", StringComparison.OrdinalIgnoreCase) == true)
          .Select(n => new
          {
            Name = n.GetNameFromDictEntries(),
            Hint = UIParser.GetHintTextFromDictEntries(n)
          })
          .Where(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Hint))
          .Select(x => $"{x.Name}: {x.Hint}")
          .ToList();

      // Boolean flags based on icon hints and names
      bool ContainsHintOrName(string keyword)
      {
        return allIconHints.Any(h => h?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true) ||
               allIconNames.Any(n => n?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true);
      }

      return new Target
      {
        UiNode = targetNode,
        BarAndImageCont = barAndImageCont,
        TextsTopToBottom = textsTopToBottom,
        IsActiveTarget = isActiveTarget,
        AssignedContainerNode = assignedContainerNode,
        AssignedIcons = assignedIcons,
        IsHighlighted = targetNode.GetBoolFromDictEntries("isHighlighted") ?? false,
        IsSelected = targetNode.GetBoolFromDictEntries("isSelected") ?? false,
        IsExpanded = targetNode.GetBoolFromDictEntries("isExpanded") ?? false,
        IsWarpTo = ContainsHintOrName("warp to") || ContainsHintOrName("warpTo"),
        IsJamming = ContainsHintOrName("jamming"),
        IsTargeting = ContainsHintOrName("targeting"),
        IsTargetingMe = ContainsHintOrName("targeting me") || ContainsHintOrName("targetingMe"),
        IsWarpDisrupted = ContainsHintOrName("warp disrupt") || ContainsHintOrName("warpDisrupt"),
        IsWarpScrambled = ContainsHintOrName("warp scrambl") || ContainsHintOrName("warpScrambl"),
        IsWebified = ContainsHintOrName("web") || ContainsHintOrName("stasis"),
        IsBeingTargeted = ContainsHintOrName("being targeted") || ContainsHintOrName("beingTargeted"),
        IsBeingTargetedByMe = ContainsHintOrName("targeted by me") || ContainsHintOrName("targetedByMe"),
        IsBeingTargetedByMeOnly = ContainsHintOrName("targeted by me only") || ContainsHintOrName("targetedByMeOnly"),
        BarPercent = barPercent,
        BarColor = barColor,
        Icons = icons,
        IconsWithHints = iconsWithHints
      };
    }
  }
}

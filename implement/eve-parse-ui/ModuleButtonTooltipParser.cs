using System.Numerics;
using System.Text.RegularExpressions;

namespace eve_parse_ui
{
  internal record ModuleButtonTooltipParser
  {
    internal static ModuleButtonTooltip? ParseModuleButtonTooltipFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var tooltipNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "ModuleButtonTooltip" ||
                              (n.pythonObjectTypeName == "TooltipPanel" && 
                               UIParser.GetAllContainedDisplayTexts(n).Any(t => 
                                   t?.Contains("Optimal", StringComparison.OrdinalIgnoreCase) == true)));

      if (tooltipNode == null)
        return null;

      return ParseModuleButtonTooltip(tooltipNode);
    }

    private static ModuleButtonTooltip? ParseModuleButtonTooltip(UITreeNodeWithDisplayRegion tooltipUINode)
    {
      Vector2 UpperRightCornerFromDisplayRegion(DisplayRegion region)
      {
        return new Vector2(region.X + region.Width, region.Y);
      }

      float DistanceSquared(Vector2 a, Vector2 b)
      {
        var distanceX = a.X - b.X;
        var distanceY = a.Y - b.Y;
        return distanceX * distanceX + distanceY * distanceY;
      }

      var allTexts = UIParser.GetAllContainedDisplayTextsWithRegion(tooltipUINode);

      // Find the shortcut text (closest text to upper right corner)
      var shortcutCandidates = allTexts
          .Select(t => new
          {
            Text = t.Text,
            DistanceSquared = DistanceSquared(
                UpperRightCornerFromDisplayRegion(t.Region.TotalDisplayRegion),
                UpperRightCornerFromDisplayRegion(tooltipUINode.TotalDisplayRegion))
          })
          .OrderBy(x => x.DistanceSquared)
          .ToList();

      var shortcutText = shortcutCandidates
          .FirstOrDefault(c => c.DistanceSquared < 1000)
          ?.Text;

      ModuleButtonTooltipShortcut? shortcut = null;
      if (shortcutText != null)
      {
        shortcut = new ModuleButtonTooltipShortcut
        {
          Text = shortcutText,
          ParseResult = null // Could be parsed further if needed
        };
      }

      // Parse optimal range if present
      var optimalRangeString = UIParser.GetAllContainedDisplayTexts(tooltipUINode)
          .Select(text =>
          {
            var match = Regex.Match(
                text,
                @"Optimal range \(\|within\)\s*([\d\.]+\s*[km]+)",
                RegexOptions.IgnoreCase);

            return match.Success ? match.Groups[1].Value.Trim() : null;
          })
          .FirstOrDefault(x => x != null);

      ModuleButtonTooltipOptimalRange? optimalRange = null;
      if (optimalRangeString != null)
      {
        optimalRange = new ModuleButtonTooltipOptimalRange
        {
          AsString = optimalRangeString,
          InMeters = ParseDistanceInMetersFromText(optimalRangeString)
        };
      }

      // Extract all text properties from tooltip
      var allTextsPlain = allTexts.Select(t => t.Text).ToList();

      // Helper to find text containing a specific keyword
      string? FindTextContaining(string keyword) =>
          allTextsPlain.FirstOrDefault(t => t?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true);

      // Helper to extract value after a label
      string? ExtractValueAfter(string label)
      {
        var text = FindTextContaining(label);
        if (text == null) return null;

        var parts = text.Split(':', 2);
        return parts.Length > 1 ? parts[1].Trim() : null;
      }

      // Extract module name (usually first/largest text)
      var moduleName = allTextsPlain.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

      // Extract group name (usually contains "Turret", "Launcher", etc.)
      var groupName = allTextsPlain.FirstOrDefault(t => 
          t?.Contains("Turret", StringComparison.OrdinalIgnoreCase) == true ||
          t?.Contains("Launcher", StringComparison.OrdinalIgnoreCase) == true ||
          t?.Contains("Mining", StringComparison.OrdinalIgnoreCase) == true ||
          t?.Contains("Shield", StringComparison.OrdinalIgnoreCase) == true);

      // Extract detailed properties
      var activationEffect = ExtractValueAfter("Activation effect") ?? ExtractValueAfter("Effect");
      var deactivationEffect = ExtractValueAfter("Deactivation effect");
      var cycleTime = ExtractValueAfter("Cycle time") ?? ExtractValueAfter("Duration");
      var falloffRange = ExtractValueAfter("Falloff") ?? ExtractValueAfter("Falloff range");
      var duration = ExtractValueAfter("Duration");
      var activationCost = ExtractValueAfter("Activation cost") ?? ExtractValueAfter("Capacitor");
      var activationTime = ExtractValueAfter("Activation time");
      var deactivationTime = ExtractValueAfter("Deactivation time");
      var heatDamage = ExtractValueAfter("Heat damage");
      var heatDamageBonus = ExtractValueAfter("Heat damage bonus") ?? ExtractValueAfter("Damage bonus");
      var overloadBonus = ExtractValueAfter("Overload bonus");
      var overloadSelfBonus = ExtractValueAfter("Overload self bonus");
      var overloadDurationBonus = ExtractValueAfter("Overload duration bonus");

      return new ModuleButtonTooltip
      {
        UiNode = tooltipUINode,
        Shortcut = shortcut,
        OptimalRange = optimalRange,
        ModuleName = moduleName,
        GroupName = groupName,
        ActivationEffect = activationEffect,
        DeactivationEffect = deactivationEffect,
        CycleTime = cycleTime,
        FalloffRange = falloffRange,
        Duration = duration,
        ActivationCost = activationCost,
        ActivationTime = activationTime,
        DeactivationTime = deactivationTime,
        HeatDamage = heatDamage,
        HeatDamageBonus = heatDamageBonus,
        OverloadBonus = overloadBonus,
        OverloadSelfBonus = overloadSelfBonus,
        OverloadDurationBonus = overloadDurationBonus
      };
    }

    private static int? ParseDistanceInMetersFromText(string distanceText)
    {
      if (string.IsNullOrWhiteSpace(distanceText))
        return null;

      // Extract the numeric part
      var match = Regex.Match(distanceText, @"([\d\.,]+)");
      if (!match.Success)
        return null;

      var valueString = match.Groups[1].Value.Replace(",", "");
      if (!double.TryParse(valueString, out var value))
        return null;

      // Convert to meters based on unit
      if (distanceText.IndexOf("km", StringComparison.OrdinalIgnoreCase) >= 0)
        return (int)(value * 1000); // Convert km to m

      return (int)value; // Assume it's already in meters
    }
  }
}

namespace eve_parse_ui
{
  internal record HeatStatusTooltipParser
  {
    internal static HeatStatusTooltip? ParseHeatStatusTooltipFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var tooltipNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "TooltipPanel")
          .FirstOrDefault(n =>
          {
            var texts = UIParser.GetAllContainedDisplayTextsWithRegion(n)
                .OrderBy(t => t.Region.TotalDisplayRegion.Y)
                .Select(t => t.Text)
                .ToList();
            return texts.Any() && texts[0]?.Contains("Heat Status", StringComparison.OrdinalIgnoreCase) == true;
          });

      if (tooltipNode == null)
        return null;

      return ParseHeatStatusTooltip(tooltipNode);
    }

    private static HeatStatusTooltip? ParseHeatStatusTooltip(UITreeNodeWithDisplayRegion tooltipNode)
    {
      var allTexts = UIParser.GetAllContainedDisplayTexts(tooltipNode)
          .Select(t => t?.Trim())
          .Where(t => !string.IsNullOrEmpty(t))
          .ToList();

      int? ParsePercentFromPrefix(string prefix)
      {
        var valueString = allTexts
            .Where(t => t!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(t => t!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .FirstOrDefault()
                ?.TrimEnd('%'))
            .FirstOrDefault();

        if (valueString != null && int.TryParse(valueString, out var value))
          return value;

        return null;
      }

      return new HeatStatusTooltip
      {
        UiNode = tooltipNode,
        LowPercent = ParsePercentFromPrefix("low"),
        MediumPercent = ParsePercentFromPrefix("medium"),
        HighPercent = ParsePercentFromPrefix("high"),
        HeatLevel = ParsePercentFromPrefix("heat level"),
        HeatLevelText = allTexts.FirstOrDefault(t => t?.Contains("Heat Level:", StringComparison.OrdinalIgnoreCase) == true),
        HeatDamage = allTexts.FirstOrDefault(t => t?.Contains("Heat Damage:", StringComparison.OrdinalIgnoreCase) == true),
        HeatDamageBonus = allTexts.FirstOrDefault(t => t?.Contains("Damage Bonus:", StringComparison.OrdinalIgnoreCase) == true),
        OverloadBonus = allTexts.FirstOrDefault(t => t?.Contains("Overload Bonus:", StringComparison.OrdinalIgnoreCase) == true),
        OverloadSelfBonus = allTexts.FirstOrDefault(t => t?.Contains("Overload Self Bonus:", StringComparison.OrdinalIgnoreCase) == true),
        OverloadDurationBonus = allTexts.FirstOrDefault(t => t?.Contains("Overload Duration Bonus:", StringComparison.OrdinalIgnoreCase) == true)
      };
    }
  }
}

namespace eve_parse_ui
{
  internal record SurveyScanWindowParser
  {
    internal static SurveyScanWindow? ParseSurveyScanWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var surveyScanWindowNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "SurveyScanView" ||
                              n.pythonObjectTypeName == "SurveyScanWindow");

      if (surveyScanWindowNode == null)
        return null;

      return ParseSurveyScanWindow(surveyScanWindowNode);
    }

    private static SurveyScanWindow ParseSurveyScanWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      var scanEntryNodes = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "SurveyScanEntry" ||
                     n.pythonObjectTypeName == "SurveyEntry" ||
                     n.pythonObjectTypeName == "ScrollEntry")
          .ToList();

      var scanEntries = scanEntryNodes
          .Select(ParseSurveyScanResult)
          .ToList();

      return new SurveyScanWindow
      {
        UiNode = windowNode,
        ScanEntries = scanEntries
      };
    }

    private static SurveyScanResult ParseSurveyScanResult(UITreeNodeWithDisplayRegion entryNode)
    {
      // Get texts left to right
      var textsLeftToRight = UIParser.GetAllContainedDisplayTextsWithRegion(entryNode)
          .OrderBy(t => t.Region.TotalDisplayRegion.X)
          .Select(t => t.Text)
          .ToList();

      var typeName = textsLeftToRight.ElementAtOrDefault(0);
      var quantityStr = textsLeftToRight.ElementAtOrDefault(1);
      var distanceStr = textsLeftToRight.ElementAtOrDefault(2);

      // Parse quantity
      int? quantity = null;
      if (!string.IsNullOrEmpty(quantityStr))
      {
        var match = System.Text.RegularExpressions.Regex.Match(quantityStr, @"([\d,]+)");
        if (match.Success)
        {
          var valueStr = match.Groups[1].Value.Replace(",", "");
          if (int.TryParse(valueStr, out var qty))
          {
            quantity = qty;
          }
        }
      }

      // Parse distance
      float? distance = null;
      string? distanceUnit = null;
      if (!string.IsNullOrEmpty(distanceStr))
      {
        var match = System.Text.RegularExpressions.Regex.Match(distanceStr, @"([\d,.]+)\s*([a-zA-Z]+)");
        if (match.Success)
        {
          var valueStr = match.Groups[1].Value.Replace(",", "");
          if (float.TryParse(valueStr, out var dist))
          {
            distance = dist;
            distanceUnit = match.Groups[2].Value;
          }
        }
      }

      return new SurveyScanResult
      {
        UiNode = entryNode,
        TypeName = typeName,
        Quantity = quantity,
        Distance = distance,
        DistanceUnit = distanceUnit
      };
    }
  }
}

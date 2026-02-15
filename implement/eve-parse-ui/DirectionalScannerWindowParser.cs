namespace eve_parse_ui
{
  internal record DirectionalScannerWindowParser
  {
    internal static DirectionalScannerWindow? ParseDirectionalScannerWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var dscanWindowNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "DirectionalScanner" ||
                              n.pythonObjectTypeName == "DirectionalScannerWindow");

      if (dscanWindowNode == null)
        return null;

      return ParseDirectionalScannerWindow(dscanWindowNode);
    }

    private static DirectionalScannerWindow? ParseDirectionalScannerWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      var scrollNode = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName.Contains("Scroll", StringComparison.OrdinalIgnoreCase));

      // Find scan button
      var scanButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n =>
          {
            var texts = UIParser.GetAllContainedDisplayTexts(n);
            return texts.Any(t => t?.Contains("Scan", StringComparison.OrdinalIgnoreCase) == true);
          });

      // Find range dropdown
      var rangeDropdown = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "Dropdown" ||
                              n.pythonObjectTypeName == "DropdownMenu" ||
                              n.GetNameFromDictEntries()?.Contains("range", StringComparison.OrdinalIgnoreCase) == true);

      // Find range text display
      var rangeText = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => 
          {
            var text = UIParser.GetDisplayText(n);
            return text?.Contains("AU", StringComparison.OrdinalIgnoreCase) == true ||
                   text?.Contains("km", StringComparison.OrdinalIgnoreCase) == true;
          });

      var scanResultNodes = scrollNode?.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "DirectionalScanResult" ||
                     n.pythonObjectTypeName == "ScrollEntry" ||
                     n.pythonObjectTypeName == "ScanResult")
          .ToList() ?? new List<UITreeNodeWithDisplayRegion>();

      var scanResults = scanResultNodes
          .Select(ParseDirectionalScanResult)
          .ToList();

      return new DirectionalScannerWindow
      {
        UiNode = windowNode,
        ScrollNode = scrollNode,
        ScanResults = scanResults,
        ScanButton = scanButton,
        RangeDropdown = rangeDropdown,
        RangeText = rangeText
      };
    }

    private static DirectionalScanResult ParseDirectionalScanResult(UITreeNodeWithDisplayRegion resultNode)
    {
      // Get all text elements sorted left to right
      var textsLeftToRight = UIParser.GetAllContainedDisplayTextsWithRegion(resultNode)
          .OrderBy(t => t.Region.TotalDisplayRegion.X)
          .Select(t => t.Text)
          .ToList();

      var typeName = textsLeftToRight.ElementAtOrDefault(0);
      var name = textsLeftToRight.ElementAtOrDefault(1);
      var distanceStr = textsLeftToRight.ElementAtOrDefault(2);

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

      // Determine object type from type name
      var typeNameLower = typeName?.ToLower() ?? "";
      var isAsteroid = typeNameLower.Contains("asteroid") || typeNameLower.Contains("veldspar") || 
                       typeNameLower.Contains("scordite") || typeNameLower.Contains("plagioclase");
      var isShip = typeNameLower.Contains("capsule") || typeNameLower.Contains("pod") ||
                   typeNameLower.Contains("cruiser") || typeNameLower.Contains("frigate") ||
                   typeNameLower.Contains("battleship") || typeNameLower.Contains("destroyer");
      var isWreck = typeNameLower.Contains("wreck");
      var isContainer = typeNameLower.Contains("container") || typeNameLower.Contains("cargo");

      return new DirectionalScanResult
      {
        UiNode = resultNode,
        TypeName = typeName,
        Name = name,
        Distance = distance,
        IsAsteroid = isAsteroid,
        IsShip = isShip,
        IsWreck = isWreck,
        IsContainer = isContainer
      };
    }
  }
}

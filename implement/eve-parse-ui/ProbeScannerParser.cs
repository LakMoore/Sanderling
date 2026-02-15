namespace eve_parse_ui
{
  internal class ProbeScannerParser
  {
    public static ProbeScannerWindow? ParseProbeScannerWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var windowNode = uiTreeRoot.GetDescendantsByType("ProbeScannerWindow").FirstOrDefault();

      if (windowNode == null)
        return null;

      var scanResults = windowNode
          .GetDescendantsByType("ScanResultNew")
          .Select(ParseProbeScanResult)
          .ToList();

      // Find scan button
      var scanButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n =>
          {
            var texts = UIParser.GetAllContainedDisplayTexts(n);
            return texts.Any(t => t?.Contains("Scan", StringComparison.OrdinalIgnoreCase) == true);
          });

      // Find stop button
      var stopButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n =>
          {
            var texts = UIParser.GetAllContainedDisplayTexts(n);
            return texts.Any(t => t?.Contains("Stop", StringComparison.OrdinalIgnoreCase) == true);
          });

      // Determine if currently scanning (stop button visible/enabled)
      var isScanning = stopButton?.GetBoolFromDictEntries("isEnabled") ?? false;

      return new ProbeScannerWindow
      {
        UiNode = windowNode,
        ScanResults = scanResults,
        ScrollingPanel = UIParser.ParseScrollingPanel(windowNode),
        ScanButton = scanButton,
        StopButton = stopButton,
        IsScanning = isScanning
      };
    }

    public static ProbeScanResult ParseProbeScanResult(
        UITreeNodeWithDisplayRegion scanResultNode
    )
    {
      var detailsLeftToRight = scanResultNode
          .GetDescendantsByType("EveLabelMedium")
          .ToList();

      var warpButton = scanResultNode
          .ListDescendantsWithDisplayRegion()
          .FirstOrDefault(node =>
              node.GetTexturePathFromDictEntries()?.EndsWith("44_32_18.png") == true);

      string? signal = null;
      string? group = null;
      if (warpButton == null)
      {
        signal = UIParser.GetDisplayText(detailsLeftToRight.Last());
        group = detailsLeftToRight.Count == 5 ? UIParser.GetDisplayText(detailsLeftToRight[3]) : null;
      }
      else
      {
        group = detailsLeftToRight.Count == 4 ? UIParser.GetDisplayText(detailsLeftToRight[3]) : null;

        // adjust the width of the scanResultNode to account for the warp button
        scanResultNode.TotalDisplayRegionVisible = new DisplayRegion(
            scanResultNode.TotalDisplayRegionVisible.X,
            scanResultNode.TotalDisplayRegionVisible.Y,
            scanResultNode.TotalDisplayRegionVisible.Width - warpButton.TotalDisplayRegionVisible.Width,
            scanResultNode.TotalDisplayRegionVisible.Height
        );
      }

      var distance = UIParser.GetDisplayText(detailsLeftToRight[0]);
      var id = UIParser.GetDisplayText(detailsLeftToRight[1]);
      var name = UIParser.GetDisplayText(detailsLeftToRight[2]);

      // Parse additional properties from AI conversion
      var typeName = group; // Group often contains type information
      var isSelected = scanResultNode.GetBoolFromDictEntries("isSelected") ?? false;
      var isHighlighted = scanResultNode.GetBoolFromDictEntries("isHighlighted") ?? false;

      // Parse signal strength (percentage from signal string like "100%")
      int? signalStrength = null;
      if (!string.IsNullOrEmpty(signal))
      {
        var match = System.Text.RegularExpressions.Regex.Match(signal, @"(\d+)%");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var strength))
        {
          signalStrength = strength;
        }
      }

      // Parse distance in meters
      float? distanceInMeters = null;
      string? distanceUnit = null;
      if (!string.IsNullOrEmpty(distance))
      {
        var distanceMatch = System.Text.RegularExpressions.Regex.Match(distance, @"([\d,.]+)\s*([a-zA-Z]+)");
        if (distanceMatch.Success)
        {
          var valueStr = distanceMatch.Groups[1].Value.Replace(",", "");
          if (float.TryParse(valueStr, out var distValue))
          {
            distanceUnit = distanceMatch.Groups[2].Value;
            // Convert to meters
            distanceInMeters = distanceUnit.ToLower() switch
            {
              "km" => distValue * 1000,
              "au" => distValue * 149597870700,
              "m" => distValue,
              _ => distValue
            };
          }
        }
      }

      return new ProbeScanResult
      {
        UiNode = scanResultNode,
        Distance = distance ?? "??",
        ID = id ?? "??",
        Name = name ?? "??",
        Group = group,
        Signal = signal,
        WarpButton = warpButton,
        TypeName = typeName,
        SignalStrength = signalStrength,
        DistanceInMeters = distanceInMeters,
        DistanceUnit = distanceUnit,
        IsSelected = isSelected,
        IsHighlighted = isHighlighted
      };
    }
  }
}

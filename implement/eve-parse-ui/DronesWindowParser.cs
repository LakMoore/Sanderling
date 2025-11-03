
using System.Diagnostics;

namespace eve_parse_ui
{
  internal class DronesWindowParser
  {
    public static DronesWindow? ParseDronesWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var windowNode = uiTreeRoot
          .ListDescendantsWithDisplayRegion()
          .FirstOrDefault(node =>
          {
            var typeName = node.pythonObjectTypeName;
            return typeName == "DroneView" || typeName == "DronesWindow";
          });

      if (windowNode == null)
        return null;

      var bayHeader = windowNode
          .GetDescendantsByType("DroneGroupHeaderInBay")
          .Select(ParseDronesWindowDroneGroup)
          .Where(header => header != null)
          .FirstOrDefault();

      if (bayHeader == null)
        return null;

      var spaceHeader = windowNode
          .GetDescendantsByType("DroneGroupHeaderInSpace")
          .Select(ParseDronesWindowDroneGroup)
          .Where(header => header != null)
          .FirstOrDefault();

      if (spaceHeader == null)
        return null;

      var bayDrones = windowNode
          .GetDescendantsByType("DroneInBayEntry")
          .Select(ParseDronesWindowDroneEntry)
          .ToList();

      var bayGroups = windowNode
          .GetDescendantsByType("DroneSubGroupInBay")
          .Select(ParseDronesWindowDroneGroup)
          .Where(header => header != null)
          .Cast<DronesWindowDroneGroupHeader>()
          .ToList();

      var spaceDrones = windowNode
          .GetDescendantsByType("DroneInSpaceEntry")
          .Select(ParseDronesWindowDroneEntry)
          .ToList();

      var spaceGroups = windowNode
          .GetDescendantsByType("DroneSubGroupInSpace")
          .Select(ParseDronesWindowDroneGroup)
          .Where(header => header != null)
          .Cast<DronesWindowDroneGroupHeader>()
          .ToList();

      return new DronesWindow
      {
        UiNode = windowNode,
        BayHeader = bayHeader,
        DronesInBay = bayDrones,
        DroneGroupsInBay = bayGroups,
        SpaceHeader = spaceHeader,
        DronesInSpace = spaceDrones,
        DroneGroupsInSpace = spaceGroups
      };
    }

    private static DronesWindowDroneGroupHeader? ParseDronesWindowDroneGroup(UITreeNodeWithDisplayRegion groupHeaderUiNode)
    {
      var allTexts = groupHeaderUiNode.GetAllContainedDisplayTextsWithRegion()?
          .OrderBy(x => x.Region.TotalDisplayRegion.Width * x.Region.TotalDisplayRegion.Height)
          .Select(x => x.Text)
          .FirstOrDefault();

      if (allTexts == null)
        return null;

      var quantitiesFromTitle = ParseQuantityFromDroneGroupTitleText(allTexts);

      var isCollapsed = UIParser.IsCollapsedFromGlowSprite(groupHeaderUiNode);

      return new DronesWindowDroneGroupHeader
      {
        UiNode = groupHeaderUiNode,
        IsCollapsed = isCollapsed,
        MainText = allTexts,
        Quantity = quantitiesFromTitle?.Item1,
        MaxQuantity = quantitiesFromTitle?.Item2
      };
    }

    private static Tuple<int, int?>? ParseQuantityFromDroneGroupTitleText(string droneGroupTitleText)
    {
      var parts = droneGroupTitleText.Split('(');
      if (parts.Length == 1)
      {
        Debug.WriteLine("ParseQuantityFromDroneGroupTitleText: Missing opening parens");
        return null;
      }

      var textInParens = parts[1].Split(')').FirstOrDefault();
      if (string.IsNullOrEmpty(textInParens))
      {
        Debug.WriteLine("ParseQuantityFromDroneGroupTitleText: Missing closing parens");
        return null;
      }

      var numberTexts = textInParens
          .Split('/')
          .Select(x => x.Trim())
          .ToList();

      var parsedResults = numberTexts
          .Select(text =>
          {
            if (int.TryParse(text, out var number))
              return number;
            Debug.WriteLine($"Failed to parse to integer from '{text}'");
            return null as int?;
          })
          .ToList();

      var anyFailures = parsedResults.Any(x => x == null);
      if (anyFailures)
      {
        Debug.WriteLine($"ParseQuantityFromDroneGroupTitleText: Failed to parse numbers in parentheses");
        return null;
      }

      if (parsedResults.Count == 1 && parsedResults[0] != null)
      {
        return new Tuple<int, int?>((int)parsedResults[0]!, null);
      }

      if (parsedResults.Count == 2 && parsedResults[0] != null)
      {
        return new Tuple<int, int?>((int)parsedResults[0]!, parsedResults[1]);
      }

      Debug.WriteLine("Found unexpected number of numbers in parentheses.");
      return null;
    }

    private static DronesWindowDrone ParseDronesWindowDroneEntry(UITreeNodeWithDisplayRegion droneEntryNode)
    {
      var mainText = droneEntryNode
          .GetAllContainedDisplayTextsWithRegion()?
          .OrderBy(x => x.Region.TotalDisplayRegion.Width * x.Region.TotalDisplayRegion.Height)
          .Select(x => x.Text)
          .FirstOrDefault();

      int? GaugeValuePercentFromContainerName(string containerName)
      {
        var gaugeNode = droneEntryNode
            .ListDescendantsWithDisplayRegion()
            .FirstOrDefault(node => node.GetNameFromDictEntries() == containerName);

        if (gaugeNode == null)
          return null;

        var gaugeBar = gaugeNode
            .ListDescendantsWithDisplayRegion()
            .FirstOrDefault(node => node.GetNameFromDictEntries() == "droneGaugeBar");

        var droneGaugeBarDmg = gaugeNode
            .ListDescendantsWithDisplayRegion()
            .FirstOrDefault(node => node.GetNameFromDictEntries() == "droneGaugeBarDmg");

        if (gaugeBar == null || droneGaugeBarDmg == null)
          return null;

        return (gaugeBar.TotalDisplayRegion.Width - droneGaugeBarDmg.TotalDisplayRegion.Width) * 100 /
               gaugeBar.TotalDisplayRegion.Width;
      }

      // Gauge is incorrectly spelt in the client - CCP Interns!
      var shieldPercent = GaugeValuePercentFromContainerName("shieldGauge");
      var armorPercent = GaugeValuePercentFromContainerName("armorGauge");
      var structPercent = GaugeValuePercentFromContainerName("structGauge");

      Hitpoints? hitpointsPercent = null;
      if (shieldPercent.HasValue && armorPercent.HasValue && structPercent.HasValue)
      {
        hitpointsPercent = new Hitpoints
        {
          Shield = shieldPercent.Value,
          Armor = armorPercent.Value,
          Structure = structPercent.Value
        };
      }

      return new DronesWindowDrone
      {
        UiNode = droneEntryNode,
        Type = mainText,
        HitpointsPercent = hitpointsPercent
      };
    }
  }
}
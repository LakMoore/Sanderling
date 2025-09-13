
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace eve_parse_ui
{
  public class PlanetaryIndustryParser
  {
    public static PlanetsWindow? ParsePlanetWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiRoot)
    {
      var planetWindow = uiRoot.GetDescendantsByType("PlanetWindow").FirstOrDefault();

      if (planetWindow == null)
        return null;

      var allButtons = planetWindow.GetDescendantsByType("Button").ToList();

      var viewButton = allButtons
          .FirstOrDefault(button =>
              button.GetNameFromDictEntries()?
              .Equals("View_Btn", StringComparison.CurrentCultureIgnoreCase) ?? false
          );

      var warpToButton = allButtons
          .FirstOrDefault(button =>
              button.GetNameFromDictEntries()?
              .Equals("Warp to_Btn", StringComparison.CurrentCultureIgnoreCase) ?? false
          );

      var accessButton = allButtons.
          FirstOrDefault(button =>
              button.GetNameFromDictEntries()?
              .Equals("Access_Btn", StringComparison.CurrentCultureIgnoreCase) ?? false
          );

      return new PlanetsWindow()
      {
        UiNode = planetWindow,
        Colonies = ParseColonies(planetWindow),
        ScrollingPanel = UIParser.ParseScrollingPanel(planetWindow),
        ViewButton = viewButton,
        WarpToButton = warpToButton,
        AccessButton = accessButton
      };
    }

    private static IReadOnlyList<Colony> ParseColonies(UITreeNodeWithDisplayRegion planetWindow)
    {
      return planetWindow
          .GetDescendantsByType("ColonyEntry")
          .Select(entry =>
          {
            var caption = entry
                      .GetDescendantsByName("captionCont")
                      .FirstOrDefault()?
                      .GetAllContainedDisplayTextsWithRegion()?
                      .FirstOrDefault()?.Text;

            // fallback to full value
            var name = caption;

            // <color=0xFF8D3264>-1.0%</color> J121006 II - Barren - 15 installations
            // extract the name from the caption
            var regex = Regex.Match(caption ?? "", @"<color(.*?)</color>(.*)");
            if (regex.Success && regex.Groups.Count == 3)
            {
              name = regex.Groups[2].Value.Trim();
            }

            if (name?.Contains(" - ") == true)
            {
              var splits = name.Split(" - ");
              name = splits[0];
            }

            var isSelected = entry.GetFromDict<bool>("isSelected");

            var mainIcon = entry
                      .GetDescendantsByType("Container")
                      .FirstOrDefault(sprite =>
                          sprite.GetNameFromDictEntries()?
                          .Equals("iconCont", StringComparison.CurrentCultureIgnoreCase) ?? false
                      );

            Debug.Assert(mainIcon != null);

            var restartExtractionButton = entry
                      .GetDescendantsByType("Button")
                      .FirstOrDefault(button =>
                          button.GetNameFromDictEntries()?
                          .Equals("restartExtraction", StringComparison.CurrentCultureIgnoreCase) ?? false
                      );

            var requiresAttention = entry
                      .GetDescendantsByType("Container")
                      .Select(UIParser.GetHintTextFromDictEntries)
                      .FirstOrDefault(hint =>
                          hint?.Equals("One or more facility requires attention", StringComparison.CurrentCultureIgnoreCase) == true)
                      != null;

            var piItems = entry
                      .GetDescendantsByType("ContainerAutoSize")
                      .Where(container => container.GetNameFromDictEntries()?.Equals("topRowCont", StringComparison.CurrentCultureIgnoreCase) == true)
                      .SelectMany(container => container.GetDescendantsByType("StorageIcon"))
                      .Select(icon => icon.GetFromDict<int>("typeID"))
                      .Select(typeid => new PIItem()
                      {
                        TypeId = typeid,
                        TierIndex = 0
                      })
                      .ToList() ?? [];

            return new Colony()
            {
              UiNode = entry,
              Name = name ?? "Unknown Colony",
              IsSelected = isSelected,
              MainIcon = mainIcon,
              RequiresAttention = requiresAttention,
              RestartExtractionButton = restartExtractionButton,
              PIItems = piItems
            };
          })
          .ToList();
    }

    public static PlanetaryImportExportUI? ParsePlanetaryImportExportUIFromUITreeRoot(UITreeNodeNoDisplayRegion uiRoot)
    {
      var planetaryImportExportUI = uiRoot.GetDescendantsByType("PlanetaryImportExportUI").FirstOrDefault();

      if (planetaryImportExportUI == null)
        return null;

      var name = planetaryImportExportUI
          .GetDescendantsByType("WindowCaption")
          .FirstOrDefault()?
          .GetAllContainedDisplayTextsWithRegion()?
          .FirstOrDefault()?
          .Text
          .Replace("Orbital Customs Office", "")
          .Trim();

      var customsList = planetaryImportExportUI
          .GetDescendantsByType("Container")
          .Where(container =>
              container.GetNameFromDictEntries()?
              .Equals("customsList", StringComparison.CurrentCultureIgnoreCase) == true
          )
          .Select(ParseCustomsOfficeList)
          .FirstOrDefault();

      if (customsList == null)
        return null;

      var spaceportList = planetaryImportExportUI
          .GetDescendantsByType("Container")
          .Where(container =>
              container.GetNameFromDictEntries()?
              .Equals("spaceportList", StringComparison.CurrentCultureIgnoreCase) == true
          )
          .Select(ParseCustomsOfficeList)
          .FirstOrDefault();

      if (spaceportList == null)
        return null;

      var transferButton = planetaryImportExportUI
          .GetDescendantsByType("Button")
          .FirstOrDefault(button =>
              button.GetNameFromDictEntries()?
              .Equals("Transfer_Btn", StringComparison.CurrentCultureIgnoreCase) == true
          );

      if (transferButton == null)
        return null;

      var transferCost = planetaryImportExportUI
          .GetDescendantsByType("ContainerAutosize")
          .Select(text =>
          {
            if (
                      text.GetNameFromDictEntries()?
                      .Equals("footer", StringComparison.CurrentCultureIgnoreCase) == true
                      && text.dictEntriesOfInterest.ContainsKey("_setText")
                      && text.dictEntriesOfInterest["_setText"] is string setText
                      && setText.StartsWith("Transfer Cost:", StringComparison.CurrentCultureIgnoreCase)
                  )
            {
              var isk = setText["Transfer Cost:".Length..];
              if (int.TryParse(isk.Trim(), out int iskValue))
              {
                return iskValue;
              }
            }

            return -1;
          })
          .FirstOrDefault(v => v > -1);

      return new PlanetaryImportExportUI()
      {
        UiNode = planetaryImportExportUI,
        Name = name ?? "Unknown Colony",
        CustomsList = customsList,
        SpaceportList = spaceportList,
        TransferCost = transferCost,
        TransferButton = transferButton
      };
    }

    private static CustomsOfficeList? ParseCustomsOfficeList(UITreeNodeWithDisplayRegion customsList)
    {
      var content = customsList.GetDescendantsByName("maincontainer").FirstOrDefault();

      if (content == null)
        return null;

      return new CustomsOfficeList()
      {
        UiNode = content,
        Entries = ParseCustomsEntries(content)
      };
    }

    private static List<CustomsEntry> ParseCustomsEntries(UITreeNodeWithDisplayRegion? content)
    {
      if (content == null)
        return [];

      return content
          .GetDescendantsByType("CustomsItem")
          .Select(ParseCustomsEntry)
          .Where(e => e != null)
          .Cast<CustomsEntry>()
          .ToList();
    }

    private static CustomsEntry? ParseCustomsEntry(UITreeNodeWithDisplayRegion entry)
    {
      var text = entry
          .GetAllContainedDisplayTextsWithRegion()?
          .FirstOrDefault()?.Text;

      // Qty, Name, Tier
      // <t>1665<t>Transmitter <color=gray>Tier 2</color>
      var regex = Regex.Match(text ?? "", @"<t>(.*?)<t>(.*?)<color=(.*?)>(.*?)</color>");
      if (!regex.Success || regex.Groups.Count != 5)
        return null;

      _ = int.TryParse(regex.Groups[1].Value.Trim(), out int quantity);
      var commodityName = regex.Groups[2].Value.Trim();
      var tier = regex.Groups[4].Value.Trim();

      var isPending = entry.GetDescendantsByType("Fill").Any();

      return new CustomsEntry()
      {
        UiNode = entry,
        Quantity = quantity,
        CommodityName = commodityName,
        Tier = tier,
        IsPending = isPending
      };
    }
  }
}
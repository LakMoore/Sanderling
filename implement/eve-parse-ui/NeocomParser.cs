
using System.Text.RegularExpressions;

namespace eve_parse_ui
{
  public static class NeocomParser
  {
    public static Neocom? ParseNeocomFromUITreeRoot(UITreeNodeNoDisplayRegion uiRoot)
    {
      var neocomNode = uiRoot
          .GetDescendantsByType("NeocomContainer")
          .FirstOrDefault();

      if (neocomNode == null)
        return null;

      var eveMenuButton = neocomNode
          .GetDescendantsByType("ButtonEveMenu")
          .FirstOrDefault();

      var inventory = neocomNode
          .GetDescendantsByType("ButtonInventory")
          .FirstOrDefault();

      var windows = neocomNode
          .GetDescendantsByType("ButtonWindow");

      var pi = windows
          .Where(w => w.GetNameFromDictEntries()?.Equals("planets", StringComparison.CurrentCultureIgnoreCase) == true)
          .FirstOrDefault();

      var assetsButton = windows
          .Where(w => w.GetNameFromDictEntries()?.Equals("assets", StringComparison.CurrentCultureIgnoreCase) == true)
          .FirstOrDefault();

      var marketButton = windows
          .Where(w => w.GetNameFromDictEntries()?.Equals("market", StringComparison.CurrentCultureIgnoreCase) == true)
          .FirstOrDefault();

      var clock = neocomNode
          .GetDescendantsByType("ClockButton")
          .FirstOrDefault();

      var eveMenus = uiRoot
          .GetDescendantsByType("PanelEveMenu")
          .Union(uiRoot.GetDescendantsByType("PanelGroup"));

      var panelGroups = eveMenus
          .SelectMany(em => em.GetDescendantsByType("PanelEntryGroup"))
          .Select(ParsePanelItem)
          .Where(pg => pg != null)
          .Cast<NeocomPanelItem>()
          .ToList();

      var panelCommands = eveMenus
          .SelectMany(em => em.GetDescendantsByType("PanelEntryCmd"))
          .Select(ParsePanelItem)
          .Where(pc => pc != null)
          .Cast<NeocomPanelItem>()
          .ToList();

      return new Neocom()
      {
        UiNode = neocomNode,
        EveMenuButton = eveMenuButton,
        InventoryButton = inventory,
        AssetsButton = assetsButton,
        MarketButton = marketButton,
        PlanetaryIndustryButton = pi,
        PanelGroups = panelGroups,
        PanelCommands = panelCommands,
        Clock = ParseClock(clock)
      };
    }

    private static NeocomPanelItem? ParsePanelItem(UITreeNodeWithDisplayRegion panelItem)
    {
      if (panelItem == null)
        return null;

      var text = panelItem
          .GetAllContainedDisplayTextsWithRegion()
          .Select(t => t.Text)
          .FirstOrDefault() ?? "";

      return new NeocomPanelItem()
      {
        UiNode = panelItem,
        Text = text
      };
    }


    private static NeocomClock? ParseClock(UITreeNodeWithDisplayRegion? clock)
    {

      if (clock == null)
        return null;

      var text = clock
          .GetAllContainedDisplayTextsWithRegion()
          .Select(t => t.Text)
          .Aggregate((a, b) => a + " " + b);


      // <b>hh:mm</b>
      var match = Regex.Match(text, @"<b>(\d\d):(\d\d)</b>");
      if (!match.Success || match.Groups.Count != 3)
        return null;

      var parsedText = new ParsedTime()
      {
        Hour = int.Parse(match.Groups[1].Value),
        Minute = int.Parse(match.Groups[2].Value)
      };

      return new NeocomClock()
      {
        UiNode = clock,
        Text = text,
        ParsedText = parsedText
      };

    }
  }
}


using System.Diagnostics;
using System.Text.RegularExpressions;

namespace eve_parse_ui
{
  internal class AssetsWindowParser
  {
    internal static AssetsWindow? ParseAssetsWindowFromUITreeRoot(UITreeNodeWithDisplayRegion rootNode)
    {
      if (rootNode == null)
        return null;

      var assetsWindowNode = rootNode
          .GetDescendantsByType("AssetsWindow")
          .FirstOrDefault();

      if (assetsWindowNode == null)
        return null;

      var tabGroup = assetsWindowNode
        .GetDescendantsByType("TabGroup")
        .FirstOrDefault();

      if (tabGroup == null)
        return null;

      var tabs = tabGroup
          .GetDescendantsByType("Tab");

      var tabAll = tabs
          .FirstOrDefault(t => t.GetNameFromDictEntries()?.Equals("allitems", StringComparison.CurrentCultureIgnoreCase) == true);

      var tabSearch = tabs
          .FirstOrDefault(t => t.GetNameFromDictEntries()?.Equals("search", StringComparison.CurrentCultureIgnoreCase) == true);

      var tabSafety = tabs
          .FirstOrDefault(t => t.GetNameFromDictEntries()?.Equals("safety", StringComparison.CurrentCultureIgnoreCase) == true);

      if (tabAll == null || tabSearch == null || tabSafety == null)
        return null;

      return new()
      {
        UiNode = assetsWindowNode,
        TabAll = tabAll,
        TabSearch = tabSearch,
        TabSafety = tabSafety,
        AssetLocations = ParseAssetsLocations(assetsWindowNode)
      };
    }

    private static IEnumerable<AssetLocation> ParseAssetsLocations(UITreeNodeWithDisplayRegion assetsWindowNode)
    {
      var entries = assetsWindowNode
          .GetDescendantsByType("LocationGroup");

      return entries
          .Select(e =>
          {
            var text = UIParser.GetAllContainedDisplayTexts(e).Aggregate((a, b) => a + " " + b);

            // <color=#ff3a9aeb>0.9</color> Jita IV - Moon 5 - Caldari Navy Assembly Plant - 3 Items - Route: 0 Jumps
            // Regex
            var match = Regex.Match(text ?? "", @"<color=(.*?)>(.*?)</color> (.*?) - (\d+) Item[s]? - Route: (\d+) Jump[s]?");
            if (!match.Success)
            {
              Debug.WriteLine(text);
              return null;
            }

            var secStatus = float.TryParse(match.Groups[2].Value, out float sec);

            Debug.WriteLine(match.Groups[3].Value);

            return new AssetLocation()
            {
              UiNode = e,
              SecurityStatus = secStatus ? sec : -2f,
              Name = match.Groups[3].Value,
              Items = int.Parse(match.Groups[4].Value),
              Jumps = int.Parse(match.Groups[5].Value)
            };
          })
          .Where(e => e != null)
          .Cast<AssetLocation>();
    }
  }
}
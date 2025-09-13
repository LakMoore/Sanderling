
namespace eve_parse_ui
{
  public class DailyLoginRewardsParser
  {
    public static DailyLoginRewardsWindow? ParseDailyLoginRewardsWindowFromUITreeRoot(UITreeNodeWithDisplayRegion rootNode)
    {
      var window = rootNode
          .GetDescendantsByType("DailyLoginRewardsWnd")
          .FirstOrDefault();

      if (window == null)
        return null;

      return new DailyLoginRewardsWindow()
      {
        UiNode = window,
        ClaimButton = UIParser.FindButtonInDescendantsContainingDisplayText(window, "claim"),
        CloseButton = UIParser.FindButtonInDescendantsContainingDisplayText(window, "close"),
      };
    }
  }
}
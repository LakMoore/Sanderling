
using System.ComponentModel.DataAnnotations;

namespace eve_parse_ui
{
  public class MarketActionParser
  {
    public static BuyMarketActionWindow? ParseBuyMarketActionWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiRoot)
    {
      if (uiRoot == null)
      {
        return null;
      }

      var marketActionWindow = uiRoot
        .GetDescendantsByType("MarketActionWindow")
        .FirstOrDefault(node =>
          node.GetNameFromDictEntries()?.Equals("marketbuyaction", StringComparison.CurrentCultureIgnoreCase) == true
        );
      if (marketActionWindow == null)
      {
        return null;
      }

      UITreeNodeWithDisplayRegion? buyButton = null;
      UITreeNodeWithDisplayRegion? cancelButton = null;
      UITreeNodeWithDisplayRegion? toggleModeButton = null;

      var allButtons = marketActionWindow.GetDescendantsByType("Button").ToList();
      foreach (var item in allButtons)
      {
        var itemName = item.GetNameFromDictEntries();
        if (itemName == "Buy_Btn") {
          buyButton = item;
        } 
        else if (itemName == "Cancel_Btn") {
           cancelButton = item;
        } else
        {
          toggleModeButton = item;
        }
      }

      if (toggleModeButton == null || buyButton == null || cancelButton == null)
      {
        return null;
      }

      var mode = Mode.Simple;
      if (UIParser.GetAllContainedDisplayTexts(toggleModeButton).Aggregate((a, b) => a + b) == "Simple")
      {
        mode = Mode.Advanced;
      }

      var typeName = string.Empty;
      var windowCaption = marketActionWindow.GetDescendantsByType("WindowCaption").FirstOrDefault();
      if (windowCaption != null)
      {
        typeName = UIParser.GetAllContainedDisplayTexts(windowCaption).Aggregate((a, b) => a + b);
        if (typeName.StartsWith("Buy "))
        {
          typeName = typeName[4..];
        }
      }

      var locationLink = marketActionWindow.GetDescendantsByName("marketbuyaction_location").FirstOrDefault();

      var bidPrice = marketActionWindow.GetDescendantsByName("Bid price").FirstOrDefault();
      var quantity = marketActionWindow.GetDescendantsByName("Quantity")
        .FirstOrDefault()?.GetDescendantsByName("_textClipper").FirstOrDefault();

      if (quantity == null)
      {
        return null;
      }

      var dropDowns = marketActionWindow.GetDescendantsByType("Combo").ToList();

      UITreeNodeWithDisplayRegion? duration = null;
      UITreeNodeWithDisplayRegion? range = null;
      foreach (var dd in dropDowns)
      {
        var text = UIParser.GetAllContainedDisplayTexts(dd).Aggregate((a, b) => a + b);
        if (
          text.Contains("jump", StringComparison.CurrentCultureIgnoreCase) 
          || text.Equals("region", StringComparison.CurrentCultureIgnoreCase)
          || text.Equals("solar system", StringComparison.CurrentCultureIgnoreCase)
        )
        {
          range = dd;
        }
        else
        {
          duration = dd;
        }
      }

      var checkboxes = marketActionWindow.GetDescendantsByType("Checkbox");
      var useCorp = checkboxes.FirstOrDefault(c => c.GetNameFromDictEntries() == "usecorp");
      var rememberSettings = checkboxes.FirstOrDefault(c => c.GetNameFromDictEntries() == "rememberBuySettings");

      if (useCorp == null)
      {
        return null;
      }

      return new()
      {
        UiNode = marketActionWindow,
        mode = mode,
        TypeName = typeName,
        LocationLink = locationLink,
        BidPrice = bidPrice,
        Quantity = quantity,
        Duration = duration,
        Range = range,
        UseCorpAccount = useCorp,
        RememberSettings = rememberSettings,
        BuyButton = buyButton,
        CancelButton = cancelButton,
        ToggleModeButton = toggleModeButton
      };
    }

    public static ModifyMarketActionWindow? ParseModifyMarketActionWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiRoot)
    {
      if (uiRoot == null)
      {
        return null;
      }

      var marketActionWindow = uiRoot
        .GetDescendantsByType("MarketActionWindow")
        .FirstOrDefault(node => 
          node.GetNameFromDictEntries()?.Equals("marketmodifyaction", StringComparison.CurrentCultureIgnoreCase) == true
        );
      if (marketActionWindow == null)
      {
        return null;
      }

      UITreeNodeWithDisplayRegion? okButton = null;
      UITreeNodeWithDisplayRegion? cancelButton = null;

      var allButtons = marketActionWindow.GetDescendantsByType("Button").ToList();
      foreach (var item in allButtons)
      {
        var itemName = item.GetNameFromDictEntries();
        if (itemName == "OK_Btn")
        {
          okButton = item;
        }
        else if (itemName == "Cancel_Btn")
        {
          cancelButton = item;
        }
      }

      if (okButton == null || cancelButton == null)
      {
        return null;
      }

      var typeName = string.Empty;
      var typeContainer = marketActionWindow
        .GetDescendantsByName("marketmodifyaction_textContainer")
        .FirstOrDefault(container => {
          var text = UIParser.GetAllContainedDisplayTexts(container).Aggregate((a, b) => a.Trim() + b.Trim());
          if (text.StartsWith("Type"))
          {
            typeName = text[4..];
            return true;
          }
          return false;
        });

      var newBuyPrice = marketActionWindow
        .GetDescendantsByName("New buy price").FirstOrDefault()?
        .GetDescendantsByType("EveLabelMedium").FirstOrDefault();

      return new()
      {
        UiNode = marketActionWindow,
        TypeName = typeName,
        NewBuyPrice = newBuyPrice,
        OKButton = okButton,
        CancelButton = cancelButton,
      };
    }
  }
}
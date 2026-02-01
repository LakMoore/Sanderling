using System.Text.RegularExpressions;

namespace eve_parse_ui
{
  public class MarketOrdersParser
  {
    public static MarketOrdersWindow? ParseMarketOrdersWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiRoot)
    {
      if (uiRoot == null)
      {
        return null;
      }

      var marketOrdersWindow = uiRoot.GetDescendantsByType("MarketOrdersWnd").FirstOrDefault();
      if (marketOrdersWindow == null)
      {
        return null;
      }

      var allTabs = marketOrdersWindow.GetDescendantsByType("Tab").ToList();
      UITreeNodeWithDisplayRegion? myOrdersTab = null;
      UITreeNodeWithDisplayRegion? corpOrdersTab = null;
      UITreeNodeWithDisplayRegion? ordersHistoryTab = null;
      MarketOrdersWindow.Tab tab = MarketOrdersWindow.Tab.MyOrders;

      foreach (var thisTab in allTabs)
      {
        var tabLabel = thisTab.GetDescendantsByType("EveLabelMedium").FirstOrDefault();
        var labelText = tabLabel?.GetAllContainedDisplayTexts().Aggregate((a, b) => a + b);
        var alpha = UIParser.GetColorPercentFromDictEntries(tabLabel)?.A;

        if (labelText?.Equals("My Orders", StringComparison.CurrentCultureIgnoreCase) == true)
        {
          myOrdersTab = thisTab;
          if (alpha > 50)
          {
            tab = MarketOrdersWindow.Tab.MyOrders;
          }
        }
        else if (labelText?.Equals("Corporation Orders", StringComparison.CurrentCultureIgnoreCase) == true)
        {
          corpOrdersTab = thisTab;
          if (alpha > 50)
          {
            tab = MarketOrdersWindow.Tab.CorpOrders;
          }
        }
        else if (labelText?.Equals("Orders History", StringComparison.CurrentCultureIgnoreCase) == true)
        {
          ordersHistoryTab = thisTab;
          if (alpha > 50)
          {
            tab = MarketOrdersWindow.Tab.OrdersHistory;
          }
        }
      }

      if (myOrdersTab == null || corpOrdersTab == null || ordersHistoryTab == null)
      {
        return null;
      }

      UITreeNodeWithDisplayRegion? sellingPanel;
      UITreeNodeWithDisplayRegion? buyingPanel;

      if (tab == MarketOrdersWindow.Tab.OrdersHistory)
      {
        sellingPanel = marketOrdersWindow.GetDescendantsByName("orderHistorySellScroll").FirstOrDefault();
        buyingPanel = marketOrdersWindow.GetDescendantsByName("orderHistoryBuyScroll").FirstOrDefault();
      }
      else
      {
        sellingPanel = marketOrdersWindow.GetDescendantsByName("sellscroll").FirstOrDefault();
        buyingPanel = marketOrdersWindow.GetDescendantsByName("buyscroll").FirstOrDefault();
      }

      if (sellingPanel == null || buyingPanel == null)
      {
        return null;
      }

      // The following assignments are placeholders. Replace them with actual parsing logic as needed.
      return new MarketOrdersWindow
      {
        UiNode = marketOrdersWindow,
        CurrentTab = tab,
        MyOrdersTab = myOrdersTab,
        CorpOrdersTab = corpOrdersTab,
        OrdersHistoryTab = ordersHistoryTab,
        SellingPanel = sellingPanel,
        SellingScroller = UIParser.ParseScrollingPanel(sellingPanel),
        SellOrders = ParseOrders(sellingPanel),
        BuyingPanel = buyingPanel,
        BuyingScroller = UIParser.ParseScrollingPanel(buyingPanel),
        BuyOrders = ParseOrders(buyingPanel),
      };
    }

    private static IEnumerable<OpenMarketOrder> ParseOrders(UITreeNodeWithDisplayRegion ordersPanel)
    {
      if (ordersPanel == null)
      {
        return [];
      }

      var entries = ordersPanel.GetDescendantsByType("OrderEntry");

      if (!entries.Any())
      {
        entries = ordersPanel.GetDescendantsByType("OrderHistoryEntry");
      }

      return entries
        .Select(entry =>
        {
          var regexPattern = @"^(?<Type>.+?)<t><right>(?<Remaining>[\d,]+)\/(?<Quantity>[\d,]+)<t><right><color='.+'>(?<Price>[\d,\.]+) ISK<\/color><\/right><t>(?<Location>.+?)<t>(?<Region>.+?)(<t>(?<Range>.+?)<t><right>(?<MinVolume>.+?))?<t>(?<ExpiresIn>[\d+d ?]+?[\d+h ?]+?[\d+m ?]+?[\d+s]+?)(<t>(?<IssuedBy>.+?)<t>(?<Wallet>.+?))?$";
          
          // "Centum B-Type Thermal Energized Membrane<t><right>1/10<t><right><color='0xFFFFFFFF'>20,440,000.00 ISK</color></right><t>Perimeter - Tranquility Trading Tower<t>The Forge<t>1 Jump<t><right>1<t>83d 13h 21m 49s"
          var entryText = UIParser.GetAllContainedDisplayTexts(entry).Aggregate((a, b) => a + b);

          var match = Regex.Match(entryText ?? "", regexPattern);
          if (!match.Success)
            return null;

          return new OpenMarketOrder()
          {
            UiNode = entry,
            Type = match.Groups["Type"].Value,
            QuantityRemaining = int.Parse(match.Groups["Remaining"].Value.Replace(",", "")),
            Quantity = int.Parse(match.Groups["Quantity"].Value.Replace(",", "")),
            Price = double.Parse(match.Groups["Price"].Value),
            Station = match.Groups["Location"].Value,
            Region = match.Groups["Region"].Value,
            ExpiresIn = match.Groups["ExpiresIn"].Value,
            IssuedBy = match.Groups["IssuedBy"].Value,
            WalletDivision = match.Groups["Wallet"].Value
          };
        })
        .Where(e => e != null)
        .Cast<OpenMarketOrder>();
    }
  }
}
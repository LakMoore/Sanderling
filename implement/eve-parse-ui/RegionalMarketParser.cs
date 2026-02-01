


using System.Text.RegularExpressions;

namespace eve_parse_ui
{
  internal class RegionalMarketParser
  {
    internal static RegionalMarketWindow? ParseRegionalMarketWindowFromUITreeRoot(UITreeNodeWithDisplayRegion uiRoot)
    {

      var marketWindow = uiRoot
        .GetDescendantsByType("RegionalMarket")
        .FirstOrDefault();
      
      if (marketWindow == null)
      {
        return null;
      }

      var marketbase = marketWindow.GetDescendantsByType("MarketBase").FirstOrDefault();

      if (marketbase == null)
      {
        return null;
      }

      var browseButton = marketbase.GetDescendantsByName("marketTabbrowse").FirstOrDefault();
      if (browseButton == null)
      {
        return null;
      }
      var quickbarButton = marketbase.GetDescendantsByName("marketTabquickbar").FirstOrDefault();
      if (quickbarButton == null)
      {
        return null;
      }
      var detailsButton = marketbase.GetDescendantsByName("marketTabdetails").FirstOrDefault();
      if (detailsButton == null)
      {
        return null;
      }
      var groupsButton = marketbase.GetDescendantsByName("marketTabgroups").FirstOrDefault();
      if (groupsButton == null)
      {
        return null;
      }
      var marketOrdersButton = marketbase.GetDescendantsByType("Button")
        .FirstOrDefault(button => "market orders".Equals(
          UIParser.GetAllContainedDisplayTexts(button).Aggregate((a, b) => a + b),
          StringComparison.CurrentCultureIgnoreCase
        ));
      if (marketOrdersButton == null)
      {
        return null;
      }
      var multibuyButton = marketbase.GetDescendantsByType("Button")
        .FirstOrDefault(button => "multibuy".Equals(
          UIParser.GetAllContainedDisplayTexts(button).Aggregate((a, b) => a + b),
          StringComparison.CurrentCultureIgnoreCase
        ));
      if (multibuyButton == null)
      {
        return null;
      }

      var details = marketbase.GetDescendantsByName("details").FirstOrDefault();

      var resultsFilterButton = details?.GetDescendantsByType("UtilMenu").FirstOrDefault();

      var tabParent = marketbase.GetDescendantsByName("tabparent").FirstOrDefault();
      var marketDataTab = tabParent?.GetDescendantsByName("marketDetailsTabmarketdata").FirstOrDefault();
      var priceHistoryTab = tabParent?.GetDescendantsByName("marketDetailsTabpricehistory").FirstOrDefault();

      var placeBuyOrderButton = marketWindow.GetDescendantsByName("Place Buy Order_Btn").FirstOrDefault();

      var findInContractsButton = marketbase.GetDescendantsByType("Button")
        .FirstOrDefault(button => "find in contracts".Equals(
          UIParser.GetAllContainedDisplayTexts(button).Aggregate((a, b) => a + b),
          StringComparison.CurrentCultureIgnoreCase
        ));

      var exportToFileButton = marketbase.GetDescendantsByType("Button")
        .FirstOrDefault(button => "export to file".Equals(
          UIParser.GetAllContainedDisplayTexts(button).Aggregate((a, b) => a + b),
          StringComparison.CurrentCultureIgnoreCase
        ));

      var searchField = marketbase.GetDescendantsByName("searchField").FirstOrDefault();

      var searchResults = marketbase.GetDescendantsByType("GenericMarketItem").SelectMany(UIParser.GetAllContainedDisplayTextsWithRegion);

      var resultName = marketbase
        .GetDescendantsByName("typeNameCont")
        .FirstOrDefault()?
        .GetDescendantsByName("Row0_Col0")
        .SelectMany(UIParser.GetAllContainedDisplayTexts)
        .Aggregate((a, b) => a + b) ?? string.Empty;

      var buyScroll = marketbase.GetDescendantsByName("buyscroll").FirstOrDefault();

      var sellScroll = marketbase.GetDescendantsByName("sellscroll").FirstOrDefault();

      return new()
      {
        UiNode = marketWindow,
        BrowseButton = browseButton,
        QuickbarButton = quickbarButton,
        DetailsButton = detailsButton,
        GroupsButton = groupsButton,
        MarketOrdersButton = marketOrdersButton,
        MultibuyButton = multibuyButton,
        ResultsFilterButton = resultsFilterButton,
        MarketDataTab = marketDataTab,
        PriceHistoryTab = priceHistoryTab,
        PlaceBuyOrderButton = placeBuyOrderButton,
        FindInContractsButton = findInContractsButton,
        ExportToFileButton = exportToFileButton,
        SearchField = searchField,
        SearchResults = searchResults,
        SearchResultsPanel = UIParser.ParseScrollingPanel(marketbase.GetDescendantsByName("typescroll").FirstOrDefault()),
        SelectedItemName = resultName,
        Sellers = ParseMarketOrders(buyScroll?.GetDescendantsByType("MarketOrder")),
        SellersPanel = UIParser.ParseScrollingPanel(buyScroll),
        Buyers = ParseMarketOrders(sellScroll?.GetDescendantsByType("MarketOrder")),
        BuyersPanel = UIParser.ParseScrollingPanel(sellScroll)
      };
    }

    private static IEnumerable<MarketOrder> ParseMarketOrders(IEnumerable<UITreeNodeWithDisplayRegion>? marketOrders)
    {

      if (marketOrders == null)
      {
        return [];
      }

      // Jumps<t>Quantity<t>Price<t>Location<t>TimeRemaining
      // Station<t><right>2,218<t><right><color='0xFFFFFFFF'>290,500.00 ISK</color></right><t>Jita IV - Moon 4 - Caldari<t>89d 9h 58m 50s

      // Use Regex
      var regexPattern = @"^(?<Jumps>.+?)<t><right>(?<Quantity>[\d,]+)<t><right><color='0xFFFFFFFF'>(?<Price>[\d,\.]+) ISK<\/color><\/right><t>(?<Location>.+?)(<t>(?<Range>.+?)<t><right>(?<MinVolume>.+?))?<t>(?<ExpiresIn>[\d+d ?]+?[\d+h ?]+?[\d+m ?]+?[\d+s]+?)$";

      return marketOrders
          .Select(mo => {
            var textBody = mo.GetDescendantsByType("TextBody").FirstOrDefault();
            var text = UIParser.GetDisplayText(textBody);

            var match = Regex.Match(text ?? "", regexPattern);
            if (!match.Success)
              return null;

            var inRange = mo.GetDescendantsByType("Sprite").FirstOrDefault() != null;

            return new MarketOrder
            {
              Jumps = match.Groups["Jumps"].Value,
              Quantity = int.Parse(match.Groups["Quantity"].Value.Replace(",", "")),
              Price = double.Parse(match.Groups["Price"].Value.Replace(",", "").Replace(" ISK", "")),
              Location = match.Groups["Location"].Value,
              ExpiresIn = match.Groups["ExpiresIn"].Value,
              InRange = inRange,
            };
          })
          .Where(order => order != null)!;

    }
  }
}
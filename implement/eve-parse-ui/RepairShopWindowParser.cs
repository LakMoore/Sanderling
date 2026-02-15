namespace eve_parse_ui
{
  internal record RepairShopWindowParser
  {
    internal static RepairShopWindow? ParseRepairShopWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var repairShopWindowNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "RepairShop" ||
                              n.pythonObjectTypeName == "RepairShopWindow");

      if (repairShopWindowNode == null)
        return null;

      return ParseRepairShopWindow(repairShopWindowNode);
    }

    private static RepairShopWindow ParseRepairShopWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      var itemNodes = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "RepairItem" ||
                     n.pythonObjectTypeName == "Item" ||
                     n.pythonObjectTypeName == "ScrollEntry")
          .ToList();

      var items = itemNodes.Select(ParseRepairItem).ToList();

      var repairAllButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => UIParser.GetAllContainedDisplayTexts(n)
              .Any(t => t?.Contains("Repair All", StringComparison.OrdinalIgnoreCase) == true));

      var buttonGroup = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.GetNameFromDictEntries()?.Contains("buttonGroup") == true ||
                              n.pythonObjectTypeName == "ButtonGroup");

      var buttons = buttonGroup?.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName.Contains("Button", StringComparison.OrdinalIgnoreCase))
          .Select(btnNode => new RepairShopWindowButton
          {
            UiNode = btnNode,
            MainText = UIParser.GetAllContainedDisplayTexts(btnNode).FirstOrDefault()
          })
          .ToList() ?? new List<RepairShopWindowButton>();

      return new RepairShopWindow
      {
        UiNode = windowNode,
        Items = items,
        RepairAllButton = repairAllButton,
        ButtonGroup = buttonGroup,
        Buttons = buttons
      };
    }

    private static RepairItem ParseRepairItem(UITreeNodeWithDisplayRegion itemNode)
    {
      var name = UIParser.GetAllContainedDisplayTexts(itemNode).FirstOrDefault();
      var isSelected = itemNode.GetBoolFromDictEntries("isSelected") ?? false;

      // Parse damage percent from displayed text (e.g., "25% damaged")
      int? damagePercent = null;
      var texts = UIParser.GetAllContainedDisplayTexts(itemNode);
      foreach (var text in texts)
      {
        var match = System.Text.RegularExpressions.Regex.Match(text ?? "", @"(\d+)%");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var percent))
        {
          damagePercent = percent;
          break;
        }
      }

      // Parse repair cost (e.g., "1,234 ISK")
      decimal? repairCost = null;
      foreach (var text in texts)
      {
        var match = System.Text.RegularExpressions.Regex.Match(text ?? "", @"([\d,]+)\s*ISK");
        if (match.Success)
        {
          var valueStr = match.Groups[1].Value.Replace(",", "");
          if (decimal.TryParse(valueStr, out var cost))
          {
            repairCost = cost;
            break;
          }
        }
      }

      return new RepairItem
      {
        UiNode = itemNode,
        Name = name,
        DamagePercent = damagePercent,
        RepairCost = repairCost,
        IsSelected = isSelected
      };
    }
  }
}

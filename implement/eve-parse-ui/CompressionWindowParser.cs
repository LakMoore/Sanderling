namespace eve_parse_ui
{
  internal record CompressionWindowParser
  {
    internal static CompressionWindow? ParseCompressionWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var compressionWindowNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "CompressionWindow" ||
                              (n.pythonObjectTypeName.Contains("Window") && 
                               UIParser.GetAllContainedDisplayTexts(n).Any(t => 
                                   t?.Contains("Compress", StringComparison.OrdinalIgnoreCase) == true)));

      if (compressionWindowNode == null)
        return null;

      return ParseCompressionWindow(compressionWindowNode);
    }

    private static CompressionWindow? ParseCompressionWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      // Find compress button
      var compressButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n =>
          {
            var texts = UIParser.GetAllContainedDisplayTexts(n);
            return texts.Any(t => t?.Contains("Compress", StringComparison.OrdinalIgnoreCase) == true);
          });

      // Parse window controls
      var windowControlsNode = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "WindowControls");

      WindowControls? windowControls = null;
      if (windowControlsNode != null)
      {
        var minimizeButton = windowControlsNode.GetDescendantsByName("MinimizeButton").FirstOrDefault();
        var closeButton = windowControlsNode.GetDescendantsByName("CloseButton").FirstOrDefault();

        windowControls = new WindowControls
        {
          UiNode = windowControlsNode,
          MinimizeButton = minimizeButton,
          CloseButton = closeButton
        };
      }

      // Parse compression items
      var items = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "CompressionItem" ||
                     n.pythonObjectTypeName == "Item" ||
                     n.pythonObjectTypeName == "Entry")
          .Select(itemNode =>
          {
            var name = UIParser.GetAllContainedDisplayTexts(itemNode).FirstOrDefault();
            var quantity = itemNode.GetIntFromDictEntries("quantity");
            var isSelected = itemNode.GetBoolFromDictEntries("isSelected") ?? false;

            return new CompressionItem
            {
              UiNode = itemNode,
              Name = name,
              Quantity = quantity,
              IsSelected = isSelected
            };
          })
          .ToList();

      // Determine if compression is possible (button enabled)
      var canCompress = compressButton?.GetBoolFromDictEntries("isEnabled") ?? false;

      return new CompressionWindow
      {
        UiNode = windowNode,
        CompressButton = compressButton,
        WindowControls = windowControls,
        Items = items,
        CanCompress = canCompress
      };
    }
  }
}

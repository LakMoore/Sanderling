namespace eve_parse_ui
{
  internal record FittingWindowParser
  {
    internal static FittingWindow? ParseFittingWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var fittingWindowNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "FittingWindow");

      if (fittingWindowNode == null)
        return null;

      return ParseFittingWindow(fittingWindowNode);
    }

    private static FittingWindow ParseFittingWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      // Find action buttons by text content
      var saveButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => UIParser.GetAllContainedDisplayTexts(n).Any(t => 
              t?.Equals("Save", StringComparison.OrdinalIgnoreCase) == true));

      var saveAsButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => UIParser.GetAllContainedDisplayTexts(n).Any(t => 
              t?.Contains("Save As", StringComparison.OrdinalIgnoreCase) == true));

      var deleteButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => UIParser.GetAllContainedDisplayTexts(n).Any(t => 
              t?.Equals("Delete", StringComparison.OrdinalIgnoreCase) == true));

      var importButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => UIParser.GetAllContainedDisplayTexts(n).Any(t => 
              t?.Equals("Import", StringComparison.OrdinalIgnoreCase) == true));

      var exportButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => UIParser.GetAllContainedDisplayTexts(n).Any(t => 
              t?.Equals("Export", StringComparison.OrdinalIgnoreCase) == true));

      // Parse fitting entries from tree structure
      var fittingNodes = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "FittingEntry" ||
                     n.pythonObjectTypeName == "TreeViewEntry" ||
                     (n.pythonObjectTypeName == "Container" && 
                      n.GetNameFromDictEntries()?.Contains("fitting", StringComparison.OrdinalIgnoreCase) == true))
          .ToList();

      var fittings = fittingNodes
          .Select(ParseFittingWindowFitting)
          .Where(f => f != null)
          .Cast<FittingWindowFitting>()
          .ToList();

      return new FittingWindow
      {
        UiNode = windowNode,
        Fittings = fittings,
        SaveButton = saveButton,
        SaveAsButton = saveAsButton,
        DeleteButton = deleteButton,
        ImportButton = importButton,
        ExportButton = exportButton
      };
    }

    private static FittingWindowFitting? ParseFittingWindowFitting(UITreeNodeWithDisplayRegion fittingNode)
    {
      var name = UIParser.GetAllContainedDisplayTexts(fittingNode).FirstOrDefault();
      if (string.IsNullOrEmpty(name))
        return null;

      var isSelected = fittingNode.GetBoolFromDictEntries("isSelected") ?? false;
      var isHighlighted = fittingNode.GetBoolFromDictEntries("isHighlighted") ?? false;
      var isExpanded = fittingNode.GetBoolFromDictEntries("isExpanded") ?? false;

      // Parse child fittings recursively
      var childNodes = fittingNode.ListDescendantsWithDisplayRegion()
          .Where(n => n != fittingNode && 
                     (n.pythonObjectTypeName == "FittingEntry" || n.pythonObjectTypeName == "TreeViewEntry"))
          .ToList();

      var children = childNodes
          .Select(ParseFittingWindowFitting)
          .Where(f => f != null)
          .Cast<FittingWindowFitting>()
          .ToList();

      return new FittingWindowFitting
      {
        UiNode = fittingNode,
        Name = name,
        IsSelected = isSelected,
        IsHighlighted = isHighlighted,
        IsExpanded = isExpanded,
        Children = children
      };
    }
  }
}

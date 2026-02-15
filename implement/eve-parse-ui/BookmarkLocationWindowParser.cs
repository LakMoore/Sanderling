namespace eve_parse_ui
{
  internal record BookmarkLocationWindowParser
  {
    internal static BookmarkLocationWindow? ParseBookmarkLocationWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var bookmarkWindowNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "SaveLocationWindow" ||
                              n.pythonObjectTypeName == "CreateBookmarkWindow");

      if (bookmarkWindowNode == null)
        return null;

      return ParseBookmarkLocationWindow(bookmarkWindowNode);
    }

    private static BookmarkLocationWindow? ParseBookmarkLocationWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      // Find submit button (typically labeled "Submit" or "Save")
      var submitButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n =>
          {
            var texts = UIParser.GetAllContainedDisplayTexts(n);
            return texts.Any(t => 
                t?.Contains("Submit", StringComparison.OrdinalIgnoreCase) == true ||
                t?.Contains("Save", StringComparison.OrdinalIgnoreCase) == true);
          });

      // Find cancel button
      var cancelButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n =>
          {
            var texts = UIParser.GetAllContainedDisplayTexts(n);
            return texts.Any(t => t?.Contains("Cancel", StringComparison.OrdinalIgnoreCase) == true);
          });

      return new BookmarkLocationWindow
      {
        UiNode = windowNode,
        SubmitButton = submitButton,
        CancelButton = cancelButton
      };
    }
  }
}

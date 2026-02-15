namespace eve_parse_ui
{
  internal record KeyActivationWindowParser
  {
    internal static KeyActivationWindow? ParseKeyActivationWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var keyActivationWindowNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "KeyActivationWindow" ||
                              (n.pythonObjectTypeName == "Window" && 
                               UIParser.GetAllContainedDisplayTexts(n).Any(t => 
                                   t?.Contains("Activate", StringComparison.OrdinalIgnoreCase) == true)));

      if (keyActivationWindowNode == null)
        return null;

      return ParseKeyActivationWindow(keyActivationWindowNode);
    }

    private static KeyActivationWindow? ParseKeyActivationWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      // Find activate button
      var activateButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n =>
          {
            var texts = UIParser.GetAllContainedDisplayTexts(n);
            return texts.Any(t => t?.Contains("Activate", StringComparison.OrdinalIgnoreCase) == true);
          });

      // Find cancel button
      var cancelButton = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n =>
          {
            var texts = UIParser.GetAllContainedDisplayTexts(n);
            return texts.Any(t => t?.Contains("Cancel", StringComparison.OrdinalIgnoreCase) == true);
          });

      // Find input field (usually EditPlainText or similar)
      var inputField = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "EditPlainText" ||
                              n.pythonObjectTypeName == "SinglelineEdit" ||
                              n.GetNameFromDictEntries()?.Contains("input", StringComparison.OrdinalIgnoreCase) == true);

      // Get activation code from input field if present
      var activationCode = inputField?.GetStringFromDictEntries("_setText") ??
                          inputField?.GetStringFromDictEntries("text");

      return new KeyActivationWindow
      {
        UiNode = windowNode,
        ActivateButton = activateButton,
        CancelButton = cancelButton,
        InputField = inputField,
        ActivationCode = activationCode
      };
    }
  }
}

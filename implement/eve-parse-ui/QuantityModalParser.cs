
namespace eve_parse_ui
{
  public static class QuantityModalParser
  {
    public static QuantityModal? ParseQuantityModalFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRootWithDisplayRegion)
    {
      var quantityModal = uiTreeRootWithDisplayRegion
          .GetDescendantsByType("Container")
          .Where(n => n.GetNameFromDictEntries()?.StartsWith("l_modal_") == true)
          .FirstOrDefault();

      if (quantityModal == null)
        return null;

      var textbox = quantityModal
          .GetDescendantsByType("SingleLineEditInteger")
          .FirstOrDefault();

      if (textbox == null)
        return null;

      var title = quantityModal
          .GetDescendantsByType("WindowCaption")
          .SelectMany(UIParser.GetAllContainedDisplayTexts)
          .FirstOrDefault();

      var okButton = quantityModal
          .GetDescendantsByType("Button")
          .FirstOrDefault(n => n.GetNameFromDictEntries()?.Equals("ok_dialog_button") == true);

      var cancelButton = quantityModal
          .GetDescendantsByType("Button")
          .FirstOrDefault(n => n.GetNameFromDictEntries()?.Equals("cancel_dialog_button") == true);

      if (okButton == null || cancelButton == null)
        return null;

      return new QuantityModal()
      {
        UiNode = quantityModal,
        Textbox = textbox,
        Title = title ?? "Unknown Title",
        OkButton = okButton,
        CancelButton = cancelButton
      };
    }
  }
}
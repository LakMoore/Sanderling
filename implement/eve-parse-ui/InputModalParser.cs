
namespace eve_parse_ui
{
  public static class InputModalParser
  {
    public static InputModal? ParseInputModalFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRootWithDisplayRegion)
    {
      var anyModal = uiTreeRootWithDisplayRegion
          .GetDescendantsByType("Container")
          .Where(n => n.GetNameFromDictEntries()?.StartsWith("l_modal_") == true)
          .FirstOrDefault();

      if (anyModal == null)
        return null;

      var textbox = anyModal
          .GetDescendantsByType("SingleLineEditInteger")
          .FirstOrDefault();
      var inputType = InputModal.Type.Numeric;

      if (textbox == null)
      {
        textbox = anyModal
          .GetDescendantsByType("SingleLineEditText")
          .FirstOrDefault();
        inputType = InputModal.Type.Text;

        if (textbox == null)
          return null;
      }

      var title = anyModal
          .GetDescendantsByType("WindowCaption")
          .SelectMany(UIParser.GetAllContainedDisplayTexts)
          .FirstOrDefault();

      var okButton = anyModal
          .GetDescendantsByType("Button")
          .FirstOrDefault(n => n.GetNameFromDictEntries()?.Equals("ok_dialog_button") == true);

      var cancelButton = anyModal
          .GetDescendantsByType("Button")
          .FirstOrDefault(n => n.GetNameFromDictEntries()?.Equals("cancel_dialog_button") == true);

      if (okButton == null || cancelButton == null)
        return null;

      return new InputModal()
      {
        UiNode = anyModal,
        InputType = inputType,
        Textbox = textbox,
        Title = title ?? "Unknown Title",
        OkButton = okButton,
        CancelButton = cancelButton
      };
    }
  }
}
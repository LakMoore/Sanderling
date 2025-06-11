namespace eve_parse_ui
{
    internal class MessageBoxParser
    {
        public static List<MessageBox> ParseMessageBoxesFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
        {
            var messageBoxNodes = uiTreeRoot.GetDescendantsByType("MessageBox")
                .ToList();

            var modalLayers = uiTreeRoot.GetDescendantsByType("LayerCore")
                .Where(node => node.GetNameFromDictEntries()?.Contains("modal", StringComparison.CurrentCultureIgnoreCase) ?? false)
                .ToList();

            var modalHybridWindowNodes = modalLayers
                .SelectMany(layer => layer.ListDescendantsWithDisplayRegion())
                .Where(node => node.pythonObjectTypeName == "HybridWindow")
                .ToList();

            return messageBoxNodes
                .Concat(modalHybridWindowNodes)
                .Select(ParseMessageBox)
                .ToList();
        }

        public static MessageBox ParseMessageBox(UITreeNodeWithDisplayRegion uiNode)
        {
            string? textHeadline = uiNode.GetDescendantsByType("TextHeadline")
                .FirstOrDefault()?
                .GetAllContainedDisplayTextsWithRegion()?
                .Select(tuple => tuple.Item1)
                .FirstOrDefault();

            string? textBody = uiNode.GetDescendantsByType("TextBody")
                .FirstOrDefault()?
                .GetAllContainedDisplayTextsWithRegion()?
                .Select(tuple => tuple.Item1)
                .FirstOrDefault();

            var buttonGroup = uiNode
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(node => node.pythonObjectTypeName.Contains("ButtonGroup", StringComparison.CurrentCultureIgnoreCase));

            var buttons = (buttonGroup?.ListDescendantsWithDisplayRegion()
                    ?? Enumerable.Empty<UITreeNodeWithDisplayRegion>())
                .Where(node => node.pythonObjectTypeName.Contains("Button", StringComparison.CurrentCultureIgnoreCase))
                .Select(buttonNode => new MessageBoxButton
                {
                    UiNode = buttonNode,
                    MainText = buttonNode?
                        .GetAllContainedDisplayTextsWithRegion()?
                        .OrderBy(tuple => tuple.Item2.TotalDisplayRegion.AreaFromDisplayRegion() ?? 0)
                        .Select(tuple => tuple.Item1)
                        .FirstOrDefault()
                })
                .ToList();

            return new MessageBox
            {
                UiNode = uiNode,
                TextHeadline = textHeadline,
                TextBody = textBody,
                ButtonGroup = buttonGroup,
                Buttons = buttons
            };
        }
    }
}

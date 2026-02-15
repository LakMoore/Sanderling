namespace eve_parse_ui
{
  internal record AgentConversationWindowParser
  {
    internal static IEnumerable<AgentConversationWindow> ParseAgentConversationWindowsFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var agentConversationNodes = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "AgentDialogueWindow" ||
                     n.pythonObjectTypeName == "AgentConversationWindow");

      foreach (var windowNode in agentConversationNodes)
      {
        var parsedWindow = ParseAgentConversationWindow(windowNode);
        if (parsedWindow != null)
          yield return parsedWindow;
      }
    }

    private static AgentConversationWindow? ParseAgentConversationWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      var agentName = windowNode.GetStringFromDictEntries("agentName");
      var agentCorporation = windowNode.GetStringFromDictEntries("agentCorporation");

      var messages = ParseAgentConversationMessages(windowNode).ToList();
      var responses = ParseAgentConversationResponses(windowNode).ToList();

      var inputBox = windowNode.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "EditPlainText" ||
                              n.pythonObjectTypeName == "InputField");

      return new AgentConversationWindow
      {
        UiNode = windowNode,
        AgentName = agentName,
        AgentCorporation = agentCorporation,
        Messages = messages,
        Responses = responses,
        InputBox = inputBox
      };
    }

    private static IEnumerable<AgentConversationMessage> ParseAgentConversationMessages(UITreeNodeWithDisplayRegion windowNode)
    {
      return windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "AgentConversationMessage" ||
                     n.pythonObjectTypeName == "Message")
          .Select(messageNode =>
          {
            var text = UIParser.GetAllContainedDisplayTexts(messageNode).FirstOrDefault();
            var isFromAgent = messageNode.GetBoolFromDictEntries("isFromAgent") ?? false;
            var timestamp = messageNode.GetStringFromDictEntries("timestamp");

            return new AgentConversationMessage
            {
              UiNode = messageNode,
              Text = text,
              IsFromAgent = isFromAgent,
              Timestamp = timestamp
            };
          });
    }

    private static IEnumerable<AgentConversationResponse> ParseAgentConversationResponses(UITreeNodeWithDisplayRegion windowNode)
    {
      return windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName == "AgentConversationResponse" ||
                     n.pythonObjectTypeName == "Response")
          .Select(responseNode =>
          {
            var text = UIParser.GetAllContainedDisplayTexts(responseNode).FirstOrDefault();
            var isHighlighted = responseNode.GetBoolFromDictEntries("isHighlighted") ?? false;

            return new AgentConversationResponse
            {
              UiNode = responseNode,
              Text = text,
              IsHighlighted = isHighlighted
            };
          });
    }
  }
}

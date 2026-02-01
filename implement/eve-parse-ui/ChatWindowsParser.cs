


namespace eve_parse_ui
{
  internal class ChatWindowsParser
  {
    internal static IEnumerable<ChatWindow> ParseChatWindowsFromUITreeRoot(UITreeNodeWithDisplayRegion uiRoot)
    {

      if (uiRoot == null)
      {
        yield break;
      }

      var chatWindows = uiRoot.GetDescendantsByType("XmppChatWindow");

      foreach (var chatWindowNode in chatWindows)
      {
        var userList = ParseChatWindowUserlist(chatWindowNode);
        if (userList == null)
        {
          continue;
        }

        var chatWindow = new ChatWindow
        {
          UiNode = chatWindowNode,
          Name = chatWindowNode.GetFromDict<string>("displayName"),
          Userlist = userList
        };

        yield return chatWindow;
      }
    }

    private static ChatWindowUserlist? ParseChatWindowUserlist(UITreeNodeWithDisplayRegion chatWindowNode)
    {
      if (chatWindowNode == null)
      {
        return null;
      }

      var scroll = chatWindowNode.GetDescendantsByType("BasicDynamicScroll").FirstOrDefault();

      return new()
      {
        UiNode = chatWindowNode,
        VisibleUsers = ParseUsers(chatWindowNode),
        ScrollingPanel = UIParser.ParseScrollingPanel(scroll)
      };
    }

    private static IEnumerable<ChatUserEntry> ParseUsers(UITreeNodeWithDisplayRegion chatWindowNode)
    {
      if (chatWindowNode == null)
      {
        yield break;
      }

      var userEntries = chatWindowNode.GetDescendantsByType("XmppChatUserEntry");

      foreach (var userEntry in userEntries)
      {
        var flagIconWithState = userEntry.GetDescendantsByType("FlagIconWithState").FirstOrDefault();

        var chatUser = new ChatUserEntry
        {
          UiNode = userEntry,
          Name = userEntry.GetFromDict<string>("_name") ?? "Unknown User",
          CharacterID = userEntry.GetFromDict<string>("charid"),
          StandingIconHint = flagIconWithState?.GetFromDict<string>("_hint")
        };
        yield return chatUser;
      }
    }
  }
}


using System.Diagnostics;

namespace eve_parse_ui
{
    public static class ListWindowsParser
    {
        public static IReadOnlyList<ListWindow> ParseListWindowsFromUITreeRoot(UITreeNodeNoDisplayRegion uiRoot)
        {
            return uiRoot
                .GetDescendantsByType("ListWindow")
                .Select(ParseListWindow)
                .Where(lw => lw != null)
                .Cast<ListWindow>()
                .ToList()
                .AsReadOnly();
        }

        private static ListWindow? ParseListWindow(UITreeNodeWithDisplayRegion listWindow)
        {
            if (listWindow == null)
                return null;

            var collapseAllButton = listWindow
                .GetDescendantsByType("ButtonIcon")
                .Where(n => n.GetNameFromDictEntries() == "collapse")
                .FirstOrDefault();

            var contentContainer = listWindow
                .GetDescendantsByType("Container")
                .Where(n => n.GetNameFromDictEntries() == "__content")
                .FirstOrDefault();

            if (contentContainer == null)
                return null;

            List<ListGroup> listGroups = [];
            UITreeNodeWithDisplayRegion? thisListGroup = null;
            List<UITreeNodeWithDisplayRegion> listItems = []; 
            foreach (var child in contentContainer.Children ?? [])
            {
                if (child.pythonObjectTypeName == "ListGroup")
                {
                    if (thisListGroup != null)
                    {
                        var listGroup = ParseListGroup(thisListGroup, listItems);
                        if (listGroup != null)
                            listGroups.Add(listGroup);
                    }
                    thisListGroup = (UITreeNodeWithDisplayRegion)child;
                    listItems = [];
                }
                else
                {
                    listItems.Add((UITreeNodeWithDisplayRegion)child);
                }
            }

            if (thisListGroup != null)
            {
                var listGroup = ParseListGroup(thisListGroup, listItems);
                if (listGroup != null)
                    listGroups.Add(listGroup);
            }

            var closeButton = listWindow
                .GetDescendantsByType("Button")
                .Where(n => n.GetNameFromDictEntries() == "close_dialog_button")
                .FirstOrDefault();

            if (collapseAllButton == null || listGroups == null || closeButton == null)
            {
                Debug.WriteLine("Failed to parse list window");
                return null;
            }

            return new ListWindow()
            {
                UiNode = listWindow,
                CollapseAllButton = collapseAllButton,
                ListGroups = listGroups,
                CloseButton = closeButton,
                ScrollingPanel = UIParser.ParseScrollingPanel(listWindow)
            };
        }

        private static ListGroup? ParseListGroup(UITreeNodeWithDisplayRegion listGroup, List<UITreeNodeWithDisplayRegion> listItems)
        {

            if (listGroup == null)
                return null;

            var name = UIParser.GetAllContainedDisplayTexts(listGroup)
                .FirstOrDefault() ?? "";

            var openParenIndex = name.LastIndexOf('(');
            if (openParenIndex > 0)
                name = name[..openParenIndex];

            name = name.Trim();

            var isCollapsed = UIParser.IsCollapsedFromGlowSprite(listGroup) == true;

            var items = listItems
                .Select(ParseListItem)
                .Where(li => li != null)
                .Cast<ListItem>()
                .ToList()
                .AsReadOnly();

            return new ListGroup()
            {
                UiNode = listGroup,
                Name = name,
                IsCollapsed = isCollapsed,
                ListItems = items
            };
        }

        private static ListItem? ParseListItem(UITreeNodeNoDisplayRegion listItem)
        {
            if (listItem is UITreeNodeWithDisplayRegion listWithRegion) {
                var name = UIParser.GetAllContainedDisplayTexts(listWithRegion)
                    .FirstOrDefault() ?? "Unknown item";

                return new()
                {
                    UiNode = listWithRegion,
                    Name = name
                };
            }

            return null;
        }
    }
}
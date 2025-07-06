
namespace eve_parse_ui
{
    public static class SessionTimeIndicatorParser
    {
        public static SessionTimeIndicator? ParseSessionTimeIndicatorFromUITreeRoot(UITreeNodeNoDisplayRegion rootNode)
        {
            var timeIndicator = rootNode.GetDescendantsByType("SessionTimeIndicator").FirstOrDefault();

            if (timeIndicator == null)
                return null;

            return new SessionTimeIndicator() { UiNode = timeIndicator };
        }
    }
}

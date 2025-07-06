
using System.Text.RegularExpressions;

namespace eve_parse_ui
{
    public static class NeocomParser
    {
        public static Neocom? ParseNeocomFromUITreeRoot(UITreeNodeNoDisplayRegion uiRoot)
        {
            var neocomNode = uiRoot
                .GetDescendantsByType("NeocomContainer")
                .FirstOrDefault();

            if (neocomNode == null)
                return null;

            var inventory = neocomNode
                .GetDescendantsByType("ButtonInventory")
                .FirstOrDefault();

            var windows = neocomNode
                .GetDescendantsByType("ButtonWindow");

            var pi = windows
                .Where(w => w.GetNameFromDictEntries()?.Equals("planets", StringComparison.CurrentCultureIgnoreCase) == true)
                .FirstOrDefault();

            var clock = neocomNode
                .GetDescendantsByType("ClockButton")
                .FirstOrDefault();

            return new Neocom()
            {
                UiNode = neocomNode,
                InventoryButton = inventory,
                PlanetaryIndustryButton = pi,
                Clock = ParseClock(clock)
            };
        }

        private static NeocomClock? ParseClock(UITreeNodeWithDisplayRegion? clock)
        {

            if (clock == null)
                return null;

            var text = clock
                .GetAllContainedDisplayTextsWithRegion()
                .Select(t => t.Text)
                .Aggregate((a, b) => a + " " + b);


            // <b>hh:mm</b>
            var match = Regex.Match(text, @"<b>(\d\d):(\d\d)</b>");
            if (!match.Success || match.Groups.Count != 3)
                return null;

            var parsedText = new ParsedTime()
            {
                Hour = int.Parse(match.Groups[1].Value),
                Minute = int.Parse(match.Groups[2].Value)
            };

            return new NeocomClock()
            {
                UiNode = clock,
                Text = text, 
                ParsedText = parsedText
            };

        }
    }
}


using System.Text.RegularExpressions;

namespace eve_parse_ui
{
    public class CharacterSelectionParser
    {
        public static CharacterSelectionScreen? ParseCharacterSelectionFromUITreeRoot(UITreeNodeNoDisplayRegion rootNode)
        {
            if (rootNode == null)
                return null;

            var characterSelectionNode = rootNode
                .GetDescendantsByType("CharacterSelection")
                .FirstOrDefault();

            if (characterSelectionNode == null || characterSelectionNode.Children.Count == 0)
                return null;

            var characterSlots = characterSelectionNode
                .GetDescendantsByType("SmallCharacterSlot")
                .Select(ParseCharacterSlot)
                .Where(cs => cs != null)
                .Cast<CharacterSlot>()
                .ToList();

            var gameTimeLapsed = rootNode
                .GetDescendantsByType("LapsedGametimeWarning")
                .FirstOrDefault() != null;

            return new CharacterSelectionScreen()
            {
                CharacterSlots = characterSlots,
                AccountIsAlpha = gameTimeLapsed
            };
        }

        private static CharacterSlot? ParseCharacterSlot(UITreeNodeWithDisplayRegion slotNode)
        {
            if (slotNode == null)
                return null;

            var name = slotNode
                .GetDescendantsByName("characterNameLabel")
                .SelectMany(l => l.GetAllContainedDisplayTextsWithRegion())
                .FirstOrDefault()?.Text;

            var portraitPath = slotNode.GetDescendantsByType("Sprite")
                .Where(node => node.GetNameFromDictEntries() == "portraitSprite")
                .Select(node => node.GetTexturePathFromDictEntries())
                .FirstOrDefault();

            // cache:/Pictures/Characters/1234125_512.jpg
            var characterId = portraitPath?.Split('/').LastOrDefault()?.Split('_').FirstOrDefault();

            var locationNode = slotNode
                .GetDescendantsByType("CharacterDetailsLocation")
                .FirstOrDefault();

            var shipTypeName = locationNode?
                .GetDescendantsByName("shipText")
                .SelectMany(l => l.GetAllContainedDisplayTextsWithRegion())
                .FirstOrDefault()?.Text;

            var locationTextNode = locationNode?
                .GetDescendantsByName("locationTextCont")
                .FirstOrDefault();

            var systemNameText = locationTextNode?
                .GetDescendantsByType("EveLabelLargeBold")
                .SelectMany(l => l.GetAllContainedDisplayTextsWithRegion())
                .FirstOrDefault()?.Text;

            string? systemName = null;
            float? systemSecStatus = null;

            // <color=2576167524>-1.0</color> <color=3221225471>J121006
            // regex
            var regex = Regex.Match(systemNameText ?? "", @"<color=(.*?)>(.*?)</color> <color=(.*?)>(.*)");
            if (regex.Success && regex.Groups.Count == 5)
            {
                systemName = regex.Groups[4].Value.Trim();
                systemSecStatus = float.Parse(regex.Groups[2].Value.Trim());
            }

            var dockedText = locationTextNode?
                .GetDescendantsByType("EveLabelMedium")
                .SelectMany(l => l.GetAllContainedDisplayTextsWithRegion())
                .FirstOrDefault()?.Text;

            var isUndocked = dockedText?.Contains("Undocked", StringComparison.CurrentCultureIgnoreCase) == true;

            return new CharacterSlot()
            {
                UiNode = slotNode,
                Name = name,
                CharacterId = characterId,
                ShipTypeName = shipTypeName,
                SystemName = systemName,
                SystemSecStatus = systemSecStatus,
                IsUndocked = isUndocked
            };
        }
    }
}
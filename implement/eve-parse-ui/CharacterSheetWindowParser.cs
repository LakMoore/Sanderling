namespace eve_parse_ui
{
  internal record CharacterSheetWindowParser
  {
    internal static CharacterSheetWindow? ParseCharacterSheetWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
    {
      var characterSheetWindowNode = uiTreeRoot.ListDescendantsWithDisplayRegion()
          .FirstOrDefault(n => n.pythonObjectTypeName == "CharacterSheet" ||
                              n.pythonObjectTypeName == "CharacterSheetWindow");

      if (characterSheetWindowNode == null)
        return null;

      return ParseCharacterSheetWindow(characterSheetWindowNode);
    }

    private static CharacterSheetWindow ParseCharacterSheetWindow(UITreeNodeWithDisplayRegion windowNode)
    {
      // Parse character name from header
      var characterName = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.GetNameFromDictEntries()?.Contains("characterName", StringComparison.OrdinalIgnoreCase) == true)
          .Select(n => UIParser.GetDisplayText(n))
          .FirstOrDefault();

      // Parse corporation name
      var corporationName = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.GetNameFromDictEntries()?.Contains("corporation", StringComparison.OrdinalIgnoreCase) == true)
          .Select(n => UIParser.GetDisplayText(n))
          .FirstOrDefault();

      // Parse alliance name
      var allianceName = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.GetNameFromDictEntries()?.Contains("alliance", StringComparison.OrdinalIgnoreCase) == true)
          .Select(n => UIParser.GetDisplayText(n))
          .FirstOrDefault();

      // Parse skill groups
      var skillGroupNodes = windowNode.ListDescendantsWithDisplayRegion()
          .Where(n => n.pythonObjectTypeName?.Contains("SkillGroup") == true)
          .ToList();

      var skillGroups = skillGroupNodes.Select(ParseSkillGroupGauge).ToList();

      // Parse skills (simplified - would need more detailed parsing)
      var skills = new List<CharacterSkill>();

      // Parse attributes (simplified - would need more detailed parsing)
      var attributes = new List<CharacterAttribute>();

      return new CharacterSheetWindow
      {
        UiNode = windowNode,
        CharacterName = characterName,
        CorporationName = corporationName,
        AllianceName = allianceName,
        Skills = skills,
        Attributes = attributes,
        SkillGroups = skillGroups
      };
    }

    private static SkillGroupGauge ParseSkillGroupGauge(UITreeNodeWithDisplayRegion groupNode)
    {
      var name = UIParser.GetAllContainedDisplayTexts(groupNode).FirstOrDefault();

      // Parse skill points from text (e.g., "1,234,567 / 2,000,000 SP")
      int? trainedSkillPoints = null;
      int? totalSkillPoints = null;

      var texts = UIParser.GetAllContainedDisplayTexts(groupNode);
      foreach (var text in texts)
      {
        var match = System.Text.RegularExpressions.Regex.Match(text ?? "", @"([\d,]+)\s*/\s*([\d,]+)");
        if (match.Success)
        {
          var trainedStr = match.Groups[1].Value.Replace(",", "");
          var totalStr = match.Groups[2].Value.Replace(",", "");

          if (int.TryParse(trainedStr, out var trained))
            trainedSkillPoints = trained;

          if (int.TryParse(totalStr, out var total))
            totalSkillPoints = total;

          break;
        }
      }

      return new SkillGroupGauge
      {
        UiNode = groupNode,
        Name = name,
        TrainedSkillPoints = trainedSkillPoints,
        TotalSkillPoints = totalSkillPoints
      };
    }
  }
}

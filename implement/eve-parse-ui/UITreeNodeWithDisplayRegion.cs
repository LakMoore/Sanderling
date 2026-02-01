using read_memory_64_bit;

namespace eve_parse_ui
{
  public record UITreeNodeWithDisplayRegion : UITreeNodeNoDisplayRegion
  {
    public required DisplayRegion SelfDisplayRegion { get; init; }
    public required DisplayRegion TotalDisplayRegion { get; init; }
    public required DisplayRegion TotalDisplayRegionVisible { get; set; }

    // constructor
    public UITreeNodeWithDisplayRegion(UITreeNode node) : base(node)
    {
    }

    public T? GetFromDict<T>(string key)
    {
      if (!dictEntriesOfInterest.ContainsKey(key))
        return default;

      // if T is int
      if (typeof(T) == typeof(int))
      {
        if (UIParser.GetIntFromDict(dictEntriesOfInterest, key) is T intValue)
          return intValue;
      }

      return (T)dictEntriesOfInterest[key];
    }

    public IEnumerable<string> GetAllContainedDisplayTexts()
    {
      return UIParser.GetAllContainedDisplayTexts(this);
    }

    public List<DisplayTextWithRegion> GetAllContainedDisplayTextsWithRegion()
    {
      return UIParser.GetAllContainedDisplayTextsWithRegion(this);
    }

    public record DisplayTextWithRegion
    {
      public required string Text { get; init; }
      public required UITreeNodeWithDisplayRegion Region { get; init; }
    }

    public string? GetTexturePathFromDictEntries()
    {
      return UIParser.GetTexturePathFromDictEntries(this);
    }
  }
}

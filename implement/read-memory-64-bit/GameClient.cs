namespace read_memory_64_bit;

public record GameClient
{
  public string? mainWindowTitle;
  public required int processId;
  public required long mainWindowId;
  public ulong uiRootAddress;
  public int? mainWindowZIndex;
}

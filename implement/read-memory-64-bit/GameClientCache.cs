using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace read_memory_64_bit
{
  public class GameClientCache
  {
    private static List<GameClient> _uiRootCache = [];

    public static void LoadCache(string uiRootAddressCache)
    {
      string xmlString = uiRootAddressCache;
      if (string.IsNullOrEmpty(xmlString))
      {
        return;
      }
      XmlSerializer serializer = new(typeof(List<GameClient>), []);
      using var reader = new StringReader(xmlString);
      if (serializer.Deserialize(reader) is List<GameClient> serializableDictionary)
      {
        _uiRootCache = serializableDictionary;
      }
    }

    public static string SaveCache()
    {
      XmlSerializer serializer = new(typeof(List<GameClient>), []);
      using var writer = new StringWriter();
      serializer.Serialize(writer, _uiRootCache);
      return writer.ToString();
    }

    // get a game client from the cache, or null if not found
    public static GameClient GetGameClient(int processId, long mainWindowId)
    {
      GameClient? gameClient = _uiRootCache.FirstOrDefault(x =>
          x.processId == processId && x.mainWindowId == mainWindowId
      );

      // if not found, make a new one
      if (gameClient == null)
      {
        gameClient = new GameClient() { processId = processId, mainWindowId = mainWindowId };
        _uiRootCache.Add(gameClient);
      }

      return gameClient;
    }
  }
}

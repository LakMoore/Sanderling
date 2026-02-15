using System;
using System.Collections.Generic;

namespace read_memory_64_bit;

internal class MemoryReadingCache
{
  IDictionary<ulong, string> PythonTypeNameFromPythonObjectAddress;

  IDictionary<ulong, string> PythonStringValueMaxLength4000;

  IDictionary<ulong, object> DictEntryValueRepresentation;

  public MemoryReadingCache()
  {
    PythonTypeNameFromPythonObjectAddress = new Dictionary<ulong, string>();
    PythonStringValueMaxLength4000 = new Dictionary<ulong, string>();
    DictEntryValueRepresentation = new Dictionary<ulong, object>();
  }

  public string GetPythonTypeNameFromPythonObjectAddress(ulong address, Func<ulong, string> getFresh) =>
      GetFromCacheOrUpdate(PythonTypeNameFromPythonObjectAddress, address, getFresh);

  public string GetPythonStringValueMaxLength4000(ulong address, Func<ulong, string> getFresh) =>
      GetFromCacheOrUpdate(PythonStringValueMaxLength4000, address, getFresh);

  public object GetDictEntryValueRepresentation(ulong address, Func<ulong, object> getFresh) =>
      GetFromCacheOrUpdate(DictEntryValueRepresentation, address, getFresh);

  static TValue GetFromCacheOrUpdate<TKey, TValue>(IDictionary<TKey, TValue> cache, TKey key, Func<TKey, TValue> getFresh)
  {
    if (cache.TryGetValue(key, out var fromCache))
      return fromCache;

    var fresh = getFresh(key);

    cache[key] = fresh;
    return fresh;
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static read_memory_64_bit.EveOnline64;

namespace read_memory_64_bit
{
    internal class PythonReader
    {
        IMemoryReader memoryReader;
        MemoryReadingCache cache;

        internal readonly IImmutableDictionary<string, Func<ulong, LocalMemoryReadingTools, object>> specializedReadingFromPythonType;

        // contstructor
        internal PythonReader(IMemoryReader memoryReader, MemoryReadingCache cache)
        {
            this.memoryReader = memoryReader;
            this.cache = cache;

            specializedReadingFromPythonType =
                ImmutableDictionary<string, Func<ulong, LocalMemoryReadingTools, object>>.Empty
                .Add("str", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_str))
                .Add("unicode", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_unicode))
                .Add("int", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_int))
                .Add("bool", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_bool))
                .Add("float", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_float))
                .Add("PyColor", new Func<ulong, LocalMemoryReadingTools, PyColor>(ReadingFromPythonType_PyColor))
                .Add("Bunch", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_Bunch))
                /*
                 * 2024-05-26 observed dict entry with key "_setText" pointing to a python object of type "Link".
                 * The client used that instance of "Link" to display "Current Solar System" label in the location info panel.
                 * */
                .Add("Link", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_Link))
                .Add("instance", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_instance))
                .Add("dict", new Func<ulong, LocalMemoryReadingTools, List<DictionaryEntry>>(ReadingFromPythonType_dict));
        }

        internal string getPythonTypeNameFromPythonTypeObjectAddress(ulong typeObjectAddress)
        {
            var typeObjectMemory = memoryReader.ReadBytes(typeObjectAddress, 0x20);

            if (!(typeObjectMemory?.Length == 0x20))
                return null;

            var tp_name = BitConverter.ToUInt64(typeObjectMemory.Value.Span[0x18..]);

            var nameBytes = memoryReader.ReadBytes(tp_name, 100)?.ToArray();

            if (!(nameBytes?.Contains((byte)0) ?? false))
                return null;

            return System.Text.Encoding.ASCII.GetString(nameBytes.TakeWhile(character => character != 0).ToArray());
        }

        internal string getPythonTypeNameFromPythonObjectAddress(ulong objectAddress)
        {
            return cache.GetPythonTypeNameFromPythonObjectAddress(objectAddress, objectAddress =>
            {
                var objectMemory = memoryReader.ReadBytes(objectAddress, 0x10);

                if (!(objectMemory?.Length == 0x10))
                    return null;

                return getPythonTypeNameFromPythonTypeObjectAddress(BitConverter.ToUInt64(objectMemory.Value.Span[8..]));
            });
        }

        internal IImmutableDictionary<string, ulong> GetDictionaryEntriesWithStringKeys(ulong dictionaryObjectAddress)
        {
            var dictionaryEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(dictionaryObjectAddress);

            if (dictionaryEntries == null)
                return null;

            return
                dictionaryEntries
                .Select(entry => new { key = readPythonStringValueMaxLength4000(entry.key), value = entry.value })
                .Aggregate(
                    seed: ImmutableDictionary<string, ulong>.Empty,
                    func: (dict, entry) => dict.SetItem(entry.key, entry.value));
        }

        internal string readPythonStringValueMaxLength4000(ulong strObjectAddress)
        {
            return cache.GetPythonStringValueMaxLength4000(
                strObjectAddress,
                strObjectAddress => ReadPythonStringValue(strObjectAddress, memoryReader, 4000));
        }

        internal PyDictEntry[] ReadActiveDictionaryEntriesFromDictionaryAddress(ulong dictionaryAddress)
        {
            /*
            Sources:
            https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/dictobject.h
            https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Objects/dictobject.c
            */

            var dictMemory = memoryReader.ReadBytes(dictionaryAddress, 0x30);

            //  Console.WriteLine($"dictMemory is {(dictMemory == null ? "not " : "")}ok for 0x{dictionaryAddress:X}");

            if (!(dictMemory?.Length == 0x30))
                return null;

            var dictMemoryAsLongMemory = TransformMemoryContent.AsULongMemory(dictMemory.Value);

            //  var dictTypeName = getPythonTypeNameFromObjectAddress(dictionaryAddress);

            //  Console.WriteLine($"Type name for dictionary 0x{dictionaryAddress:X} is '{dictTypeName}'.");

            //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/dictobject.h#L60-L89

            var ma_fill = dictMemoryAsLongMemory.Span[2];
            var ma_used = dictMemoryAsLongMemory.Span[3];
            var ma_mask = dictMemoryAsLongMemory.Span[4];
            var ma_table = dictMemoryAsLongMemory.Span[5];

            //  Console.WriteLine($"Details for dictionary 0x{dictionaryAddress:X}: type_name = '{dictTypeName}' ma_mask = 0x{ma_mask:X}, ma_table = 0x{ma_table:X}.");

            var numberOfSlots = (int)ma_mask + 1;

            if (numberOfSlots < 0 || 10_000 < numberOfSlots)
            {
                //  Avoid stalling the whole reading process when a single dictionary contains garbage.
                return null;
            }

            var slotsMemorySize = numberOfSlots * 8 * 3;

            var slotsMemory = memoryReader.ReadBytes(ma_table, slotsMemorySize);

            //  Console.WriteLine($"slotsMemory (0x{ma_table:X}) has length of {slotsMemory?.Length} and is {(slotsMemory?.Length == slotsMemorySize ? "" : "not ")}ok for 0x{dictionaryAddress:X}");

            if (!(slotsMemory?.Length == slotsMemorySize))
                return null;

            var slotsMemoryAsLongMemory = TransformMemoryContent.AsULongMemory(slotsMemory.Value);

            var entries = new List<PyDictEntry>();

            for (var slotIndex = 0; slotIndex < numberOfSlots; ++slotIndex)
            {
                var hash = slotsMemoryAsLongMemory.Span[slotIndex * 3];
                var key = slotsMemoryAsLongMemory.Span[slotIndex * 3 + 1];
                var value = slotsMemoryAsLongMemory.Span[slotIndex * 3 + 2];

                if (key == 0 || value == 0)
                    continue;

                entries.Add(new PyDictEntry { hash = hash, key = key, value = value });
            }

            return [.. entries];
        }

        internal object GetDictEntryValueRepresentation(ulong valueOjectAddress, LocalMemoryReadingTools localMemoryReadingTools)
        {
            return cache.GetDictEntryValueRepresentation(valueOjectAddress, valueOjectAddress =>
            {
                var value_pythonTypeName = getPythonTypeNameFromPythonObjectAddress(valueOjectAddress);

                var genericRepresentation = new UITreeNode.DictEntryValueGenericRepresentation
                (
                    address: valueOjectAddress,
                    pythonObjectTypeName: value_pythonTypeName
                );

                if (value_pythonTypeName == null)
                    return genericRepresentation;

                specializedReadingFromPythonType.TryGetValue(value_pythonTypeName, out var specializedRepresentation);

                if (specializedRepresentation == null)
                    return genericRepresentation;

                return specializedRepresentation(genericRepresentation.address, localMemoryReadingTools);
            });
        }

        internal string ReadingFromPythonType_str(ulong address, LocalMemoryReadingTools memoryReadingTools)
        {
            return ReadPythonStringValue(address, memoryReadingTools.memoryReader, 0x1000);
        }

        internal object ReadingFromPythonType_unicode(ulong address, LocalMemoryReadingTools memoryReadingTools)
        {
            var pythonObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x20);

            if (!(pythonObjectMemory?.Length == 0x20))
                return "Failed to read python object memory.";

            var unicode_string_length = BitConverter.ToUInt64(pythonObjectMemory.Value.Span[0x10..]);

            if (0x1000 < unicode_string_length)
                return "String too long.";

            var stringBytesCount = (int)unicode_string_length * 2;

            var stringBytes = memoryReadingTools.memoryReader.ReadBytes(
                BitConverter.ToUInt64(pythonObjectMemory.Value.Span[0x18..]), stringBytesCount);

            if (!(stringBytes?.Length == stringBytesCount))
                return "Failed to read string bytes.";

            return System.Text.Encoding.Unicode.GetString(stringBytes.Value.Span);
        }

        internal object ReadingFromPythonType_int(ulong address, LocalMemoryReadingTools memoryReadingTools)
        {
            var intObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x18);

            if (!(intObjectMemory?.Length == 0x18))
                return "Failed to read int object memory.";

            var value = BitConverter.ToInt64(intObjectMemory.Value.Span[0x10..]);

            var asInt32 = (int)value;

            if (asInt32 == value)
                return asInt32;

            return new
            {
                @int = value,
                int_low32 = asInt32,
            };
        }

        internal object ReadingFromPythonType_bool(ulong address, LocalMemoryReadingTools memoryReadingTools)
        {
            var pythonObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x18);

            if (!(pythonObjectMemory?.Length == 0x18))
                return "Failed to read python object memory.";

            return BitConverter.ToInt64(pythonObjectMemory.Value.Span[0x10..]) != 0;
        }

        internal object ReadingFromPythonType_float(ulong address, LocalMemoryReadingTools memoryReadingTools)
        {
            return ReadPythonFloatObjectValue(address);
        }

        internal PyColor ReadingFromPythonType_PyColor(ulong address, LocalMemoryReadingTools memoryReadingTools)
        {
            var pyColorObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x18);

            if (!(pyColorObjectMemory?.Length == 0x18))
                throw new Exception("Failed to read pyColorObjectMemory.");

            var dictionaryAddress = BitConverter.ToUInt64(pyColorObjectMemory.Value.Span[0x10..]);

            var dictionaryEntries = memoryReadingTools.getDictionaryEntriesWithStringKeys(dictionaryAddress);

            if (dictionaryEntries == null)
                throw new Exception("Failed to read dictionary entries.");

            int? readValuePercentFromDictEntryKey(string dictEntryKey)
            {
                if (!dictionaryEntries.TryGetValue(dictEntryKey, out var valueAddress))
                    return null;

                var valueAsFloat = ReadPythonFloatObjectValue(valueAddress);

                if (!valueAsFloat.HasValue)
                    return null;

                return (int)(valueAsFloat.Value * 100);
            }

            return new PyColor()
            {
                aPercent = readValuePercentFromDictEntryKey("_a") ?? 100,
                rPercent = readValuePercentFromDictEntryKey("_r") ?? 100,
                gPercent = readValuePercentFromDictEntryKey("_g") ?? 100,
                bPercent = readValuePercentFromDictEntryKey("_b") ?? 100,
            };
        }

        internal object ReadingFromPythonType_Bunch(ulong address, LocalMemoryReadingTools memoryReadingTools)
        {
            var dictionaryEntries = memoryReadingTools.getDictionaryEntriesWithStringKeys(address);

            if (dictionaryEntries == null)
                return "Failed to read dictionary entries.";

            var entriesOfInterest = new List<UITreeNode.DictEntry>();

            foreach (var entry in dictionaryEntries)
            {
                if (!DictEntriesOfInterestKeys.Contains(entry.Key))
                {
                    continue;
                }

                entriesOfInterest.Add(new UITreeNode.DictEntry
                (
                    key: entry.Key,
                    value: memoryReadingTools.GetDictEntryValueRepresentation(entry.Value, memoryReadingTools)
                ));
            }

            var entriesOfInterestJObject =
                new System.Text.Json.Nodes.JsonObject(
                    entriesOfInterest.Select(dictEntry =>
                    new KeyValuePair<string, System.Text.Json.Nodes.JsonNode?>
                        (dictEntry.key,
                        System.Text.Json.Nodes.JsonNode.Parse(SerializeMemoryReadingNodeToJson(dictEntry.value)))));

            return new UITreeNode.Bunch
            (
                entriesOfInterest: entriesOfInterestJObject
            );
        }

        internal object ReadingFromPythonType_Link(ulong address, LocalMemoryReadingTools memoryReadingTools)
        {
            var pythonObjectTypeName = memoryReadingTools.GetPythonTypeNameFromPythonObjectAddress(address);

            var linkMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x40);

            if (linkMemory is null)
                return null;

            var linkMemoryAsLongMemory = TransformMemoryContent.AsULongMemory(linkMemory.Value);

            /*
             * 2024-05-26 observed a reference to a dictionary object at offset 6 * 4 bytes.
             * */

            var firstDictReference =
                linkMemoryAsLongMemory
                .ToArray()
                .Where(reference =>
                {
                    var referencedObjectTypeName = memoryReadingTools.GetPythonTypeNameFromPythonObjectAddress(reference);

                    return referencedObjectTypeName is "dict";
                })
                .FirstOrDefault();

            if (firstDictReference is 0)
                return null;

            var dictEntries =
                memoryReadingTools.getDictionaryEntriesWithStringKeys(firstDictReference)
                ?.ToImmutableDictionary(
                    keySelector: dictEntry => dictEntry.Key,
                    elementSelector: dictEntry => memoryReadingTools.GetDictEntryValueRepresentation(dictEntry.Value, memoryReadingTools));

            return new UITreeNode(
                pythonObjectAddress: address,
                pythonObjectTypeName: pythonObjectTypeName,
                dictEntriesOfInterest: dictEntries,
                otherDictEntriesKeys: null,
                children: null);
        }

        internal List<DictionaryEntry> ReadingFromPythonType_dict(ulong address, LocalMemoryReadingTools memoryReadingTools)
        {
            var dictionaryEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(address);

            List<DictionaryEntry> dictEntries = [];

            foreach (var dictionaryEntry in dictionaryEntries)
            {
                var keyObject_type_name = getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key);

                //  Console.WriteLine($"Dict entry type name is '{keyObject_type_name}'");

                if (keyObject_type_name != "str")
                    continue;

                var keyString = readPythonStringValueMaxLength4000(dictionaryEntry.key);

                dictEntries.Add(new DictionaryEntry
                (
                    key: keyString,
                    value: GetDictEntryValueRepresentation(dictionaryEntry.value, memoryReadingTools)
                ));
            }

            return dictEntries;
        }

        internal object ReadingFromPythonType_instance(ulong address, LocalMemoryReadingTools memoryReadingTools)
        {

            var dictMemory = memoryReader.ReadBytes(address, 0x20);

            if (!(dictMemory?.Length == 0x20))
                return "Failed to read ob_dict memory.";

            //var ma_version_tag = BitConverter.ToUInt64(dictMemory.Value.Span[0x10..]);
            var ma_keys = BitConverter.ToUInt64(dictMemory.Value.Span[0x18..]);

            // ma_keys should be a dictionary of properties
            return ReadingFromPythonType_dict(ma_keys, memoryReadingTools);
        }

        internal double? ReadPythonFloatObjectValue(ulong floatObjectAddress)
        {
            //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/floatobject.h

            var pythonObjectMemory = memoryReader.ReadBytes(floatObjectAddress, 0x20);

            if (!(pythonObjectMemory?.Length == 0x20))
                return null;

            return BitConverter.ToDouble(pythonObjectMemory.Value.Span[0x10..]);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matching Python naming style")]
    internal record PyColor
    {
        public required int aPercent { get; init; }
        public required int rPercent { get; init; }
        public required int gPercent { get; init; }
        public required int bPercent { get; init; }
    }
}

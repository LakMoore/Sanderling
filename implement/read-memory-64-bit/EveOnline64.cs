using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

namespace read_memory_64_bit;


public class EveOnline64
{
    static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjectsFromProcessId(int processId)
    {
        var memoryReader = new MemoryReaderFromLiveProcess(processId);

        var (committedMemoryRegions, _) = ReadCommittedMemoryRegionsWithoutContentFromProcessId(processId);

        return EnumeratePossibleAddressesForUIRootObjects(committedMemoryRegions, memoryReader);
    }

    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries)
        ReadCommittedMemoryRegionsWithContentFromProcessId(int processId)
    {
        var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: true);

        return genericResult;
    }

    static public (IImmutableList<(ulong baseAddress, int length)> memoryRegions, IImmutableList<string> logEntries)
        ReadCommittedMemoryRegionsWithoutContentFromProcessId(int processId)
    {
        var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: false);

        var memoryRegions =
            genericResult.memoryRegions
            .Select(memoryRegion => (baseAddress: memoryRegion.baseAddress, length: (int)memoryRegion.length))
            .ToImmutableList();

        return (memoryRegions, genericResult.logEntries);
    }

    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries)
        ReadCommittedMemoryRegionsFromProcessId(
        int processId,
        bool readContent)
    {
        var logEntries = new List<string>();

        void logLine(string lineText)
        {
            logEntries.Add(lineText);
            //  Console.WriteLine(lineText);
        }

        logLine("Reading from process " + processId + ".");

        var processHandle =
            WinApi.OpenProcess(
                (int)(WinApi.ProcessAccessFlags.QueryInformation | WinApi.ProcessAccessFlags.VirtualMemoryRead),
                false,
                dwProcessId: processId);

        long address = 0;

        var committedRegions = new List<SampleMemoryRegion>();

        do
        {
            int result = WinApi.VirtualQueryEx(
                processHandle,
                (IntPtr)address,
                out WinApi.MEMORY_BASIC_INFORMATION m,
                (uint)Marshal.SizeOf<WinApi.MEMORY_BASIC_INFORMATION>());

            var regionProtection = (WinApi.MemoryInformationProtection)m.Protect;

            logLine(
                $"{m.BaseAddress}-" +
                $"{(uint)m.BaseAddress + (uint)m.RegionSize - 1} : {m.RegionSize}" +
                $" bytes result={result}, state={(WinApi.MemoryInformationState)m.State}" +
                $", type={(WinApi.MemoryInformationType)m.Type}, protection={regionProtection}");

            if (address == (long)m.BaseAddress + (long)m.RegionSize)
                break;

            address = (long)m.BaseAddress + (long)m.RegionSize;

            if (m.State != WinApi.MemoryInformationState.MEM_COMMIT)
                continue;

            var protectionFlagsToSkip =
                WinApi.MemoryInformationProtection.PAGE_GUARD |
                WinApi.MemoryInformationProtection.PAGE_NOACCESS;

            var matchingFlagsToSkip = protectionFlagsToSkip & regionProtection;

            if (matchingFlagsToSkip != 0)
            {
                logLine($"Skipping region beginning at {m.BaseAddress:X} as it has flags {matchingFlagsToSkip}.");
                continue;
            }

            var regionBaseAddress = m.BaseAddress;

            byte[] regionContent = null;

            if (readContent)
            {
                UIntPtr bytesRead = UIntPtr.Zero;
                var regionContentBuffer = new byte[(long)m.RegionSize];

                WinApi.ReadProcessMemory(
                    processHandle,
                    (ulong)regionBaseAddress,
                    regionContentBuffer,
                    (UIntPtr)regionContentBuffer.LongLength,
                    ref bytesRead);

                if (bytesRead.ToUInt64() != (ulong)regionContentBuffer.LongLength)
                {
                    throw new Exception(
                        $"Failed to ReadProcessMemory at 0x{regionBaseAddress:X}: Only read " + bytesRead + " bytes.");
                }

                regionContent = regionContentBuffer;
            }

            committedRegions.Add(new SampleMemoryRegion(
                baseAddress: (ulong)regionBaseAddress,
                length: (ulong)m.RegionSize,
                content: regionContent));

        } while (true);

        logLine(
            $"Found {committedRegions.Count} committed regions with a total size of " +
            $"{committedRegions.Select(region => (long)region.length).Sum()}.");

        return (committedRegions.ToImmutableList(), logEntries.ToImmutableList());
    }

    static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjects(
        IEnumerable<(ulong baseAddress, int length)> memoryRegions,
        IMemoryReader memoryReader)
    {
        var memoryRegionsOrderedByAddress =
            memoryRegions
            .OrderBy(memoryRegion => memoryRegion.baseAddress)
            .ToImmutableArray();

        string ReadNullTerminatedAsciiStringFromAddressUpTo255(ulong address)
        {
            var asMemory = memoryReader.ReadBytes(address, 0x100);

            if (asMemory is null)
                return null;

            var asSpan = asMemory.Value.Span;

            var length = 0;

            for (var i = 0; i < asSpan.Length; ++i)
            {
                length = i;

                if (asSpan[i] == 0)
                    break;
            }

            return System.Text.Encoding.ASCII.GetString(asSpan[..length]);
        }

        ReadOnlyMemory<ulong>? ReadMemoryRegionContentAsULongArray((ulong baseAddress, int length) memoryRegion)
        {
            var asByteArray = memoryReader.ReadBytes(memoryRegion.baseAddress, memoryRegion.length);

            if (asByteArray is null)
                return null;

            return TransformMemoryContent.AsULongMemory(asByteArray.Value);
        }

        IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectType()
        {
            IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectTypeInMemoryRegion((ulong baseAddress, int length) memoryRegion)
            {
                var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

                if (memoryRegionContentAsULongArray is null)
                    yield break;

                for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
                {
                    var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

                    var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

                    if (candidate_ob_type != candidateAddressInProcess)
                        continue;

                    var candidate_tp_name =
                        ReadNullTerminatedAsciiStringFromAddressUpTo255(
                            memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 3]);

                    if (candidate_tp_name is not "type")
                        continue;

                    yield return candidateAddressInProcess;
                }
            }

            return
                memoryRegionsOrderedByAddress
                .AsParallel()
                .WithDegreeOfParallelism(8)
                .SelectMany(EnumerateCandidatesForPythonTypeObjectTypeInMemoryRegion)
                .ToImmutableArray();
        }

        IEnumerable<(ulong address, string tp_name)> EnumerateCandidatesForPythonTypeObjects(
            IImmutableList<ulong> typeObjectCandidatesAddresses)
        {
            if (typeObjectCandidatesAddresses.Count < 1)
                yield break;

            var typeAddressMin = typeObjectCandidatesAddresses.Min();
            var typeAddressMax = typeObjectCandidatesAddresses.Max();

            foreach (var memoryRegion in memoryRegionsOrderedByAddress)
            {
                var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

                if (memoryRegionContentAsULongArray is null)
                    continue;

                for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
                {
                    var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

                    var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

                    {
                        //  This check is redundant with the following one. It just implements a specialization to optimize runtime expenses.
                        if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
                            continue;
                    }

                    if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
                        continue;

                    var candidate_tp_name =
                        ReadNullTerminatedAsciiStringFromAddressUpTo255(
                            memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 3]);

                    if (candidate_tp_name is null)
                        continue;

                    yield return (candidateAddressInProcess, candidate_tp_name);
                }
            }
        }

        IEnumerable<ulong> EnumerateCandidatesForInstancesOfPythonType(
            IImmutableList<ulong> typeObjectCandidatesAddresses)
        {
            if (typeObjectCandidatesAddresses.Count < 1)
                yield break;

            var typeAddressMin = typeObjectCandidatesAddresses.Min();
            var typeAddressMax = typeObjectCandidatesAddresses.Max();

            foreach (var memoryRegion in memoryRegionsOrderedByAddress)
            {
                var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

                if (memoryRegionContentAsULongArray is null)
                    continue;

                for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
                {
                    var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

                    var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

                    {
                        //  This check is redundant with the following one. It just implements a specialization to reduce processing time.
                        if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
                            continue;
                    }

                    if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
                        continue;

                    yield return candidateAddressInProcess;
                }
            }
        }

        var uiRootTypeObjectCandidatesAddresses =
            EnumerateCandidatesForPythonTypeObjects(EnumerateCandidatesForPythonTypeObjectType().ToImmutableList())
            .Where(typeObject => typeObject.tp_name == "UIRoot")
            .Select(typeObject => typeObject.address)
            .ToImmutableList();

        return
            EnumerateCandidatesForInstancesOfPythonType(uiRootTypeObjectCandidatesAddresses)
            .ToImmutableList();
    }

    internal struct PyDictEntry
    {
        public ulong hash;
        public ulong key;
        public ulong value;
    }

    internal static readonly IImmutableSet<string> DictEntriesOfInterestKeys = ImmutableHashSet.Create(
        "_top", "_left", "_width", "_height", "_displayX", "_displayY",
        "_displayHeight", "_displayWidth",
        "_name", "_text", "_setText",
        "children",
        "texturePath", "_bgTexturePath",
        "_hint", "_display",

        //  HPGauges
        "lastShield", "lastArmor", "lastStructure",

        //  Found in "ShipHudSpriteGauge"
        "_lastValue",

        //  Found in "ModuleButton"
        "ramp_active", "end", "endTime",

        //  Found in the Transforms contained in "ShipModuleButtonRamps"
        "_rotation",

        //  Found under OverviewEntry in Sprite named "iconSprite"
        "_color",

        //  Found in "SE_TextlineCore"
        "_sr",

        //  Found in "_sr" Bunch
        "htmlstr",

        // Module button info
        "autoreload",  // int
        "activationTimer",  // ???
        "moduleButtonHint",  // ???
        "isDeactivating",  // bool
        "autorepeat",  // long
        "online",  // bool
        "quantity",  // int

        // in Icon, might be useful for ButtonModule
        "itemID", "typeID",

        // 2023-01-03 Sample with PhotonUI: process-sample-ebdfff96e7.zip
        "_texturePath", "_opacity", "_bgColor", "isExpanded",

        // Planetary Industry
        "isSelected",

        // chat channels
        "displayName", "charid"
    );

    internal struct LocalMemoryReadingTools
    {
        public IMemoryReader memoryReader;

        public Func<ulong, IImmutableDictionary<string, ulong>> getDictionaryEntriesWithStringKeys;

        public Func<ulong, string> GetPythonTypeNameFromPythonObjectAddress;

        public Func<ulong, LocalMemoryReadingTools, object> GetDictEntryValueRepresentation;
    }

    internal static string ReadPythonStringValue(ulong stringObjectAddress, IMemoryReader memoryReader, int maxLength)
    {
        //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/stringobject.h

        var stringObjectMemory = memoryReader.ReadBytes(stringObjectAddress, 0x20);

        if (!(stringObjectMemory?.Length == 0x20))
            return "Failed to read string object memory.";

        var stringObject_ob_size = BitConverter.ToUInt64(stringObjectMemory.Value.Span[0x10..]);

        if (0 < maxLength && maxLength < (int)stringObject_ob_size || int.MaxValue < stringObject_ob_size)
            return "String too long.";

        var stringBytes = memoryReader.ReadBytes(stringObjectAddress + 8 * 4, (int)stringObject_ob_size);

        if (!(stringBytes?.Length == (int)stringObject_ob_size))
            return "Failed to read string bytes.";

        return System.Text.Encoding.ASCII.GetString(stringBytes.Value.Span);
    }

    static public UITreeNode ReadUITreeFromAddress(ulong nodeAddress, IMemoryReader memoryReader, int maxDepth) =>
        ReadUITreeFromAddress(nodeAddress, memoryReader, maxDepth, null);

    static UITreeNode ReadUITreeFromAddress(ulong nodeAddress, IMemoryReader memoryReader, int maxDepth, MemoryReadingCache cache)
    {
        cache ??= new MemoryReadingCache();

        var pythonReader = new PythonReader(memoryReader, cache);

        var uiNodeObjectMemory = memoryReader.ReadBytes(nodeAddress, 0x30);

        if (!(0x30 == uiNodeObjectMemory?.Length))
            return null;

        var localMemoryReadingTools = new LocalMemoryReadingTools
        {
            memoryReader = memoryReader,
            getDictionaryEntriesWithStringKeys = pythonReader.GetDictionaryEntriesWithStringKeys,
            GetPythonTypeNameFromPythonObjectAddress = pythonReader.getPythonTypeNameFromPythonObjectAddress,
        };

        var pythonObjectTypeName = pythonReader.getPythonTypeNameFromPythonObjectAddress(nodeAddress);

        if (!(0 < pythonObjectTypeName?.Length))
            return null;

        var dictAddress = BitConverter.ToUInt64(uiNodeObjectMemory.Value.Span[0x10..]);

        var dictionaryEntries = pythonReader.ReadActiveDictionaryEntriesFromDictionaryAddress(dictAddress);

        if (dictionaryEntries is null)
            return null;

        var dictEntriesOfInterest = new List<UITreeNode.DictEntry>();

        var otherDictEntriesKeys = new List<string>();

        localMemoryReadingTools.GetDictEntryValueRepresentation = pythonReader.GetDictEntryValueRepresentation;

        foreach (var dictionaryEntry in dictionaryEntries)
        {
            var keyObject_type_name = pythonReader.getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key);

            //  Console.WriteLine($"Dict entry type name is '{keyObject_type_name}'");

            if (keyObject_type_name is not "str")
                continue;

            var keyString = pythonReader.readPythonStringValueMaxLength4000(dictionaryEntry.key);

            if (!DictEntriesOfInterestKeys.Contains(keyString))
            {
                otherDictEntriesKeys.Add(keyString);
                continue;
            }

            dictEntriesOfInterest.Add(new UITreeNode.DictEntry
            (
                key: keyString,
                value: pythonReader.GetDictEntryValueRepresentation(dictionaryEntry.value, localMemoryReadingTools)
            ));
        }

        {
            var _displayDictEntry = dictEntriesOfInterest.FirstOrDefault(entry => entry.key == "_display");

            if (_displayDictEntry != null && (_displayDictEntry.value is bool displayAsBool))
                if (!displayAsBool)
                    return null;
        }

        UITreeNode[] ReadChildren(PythonReader pythonReader)
        {
            if (maxDepth < 1)
                return null;

            //  https://github.com/Arcitectus/Sanderling/blob/b07769fb4283e401836d050870121780f5f37910/guide/image/2015-01.eve-online-python-ui-tree-structure.png

            var childrenDictEntry = dictEntriesOfInterest.FirstOrDefault(entry => entry.key == "children");

            if (childrenDictEntry is null)
                return null;

            if (childrenDictEntry.value is UITreeNode.DictEntryValueGenericRepresentation childrenEntryObject)
            {

                var childrenEntryObjectAddress = childrenEntryObject.address;

                //  Console.WriteLine($"'children' dict entry of 0x{nodeAddress:X} points to 0x{childrenEntryObjectAddress:X}.");

                var pyChildrenListMemory = memoryReader.ReadBytes(childrenEntryObjectAddress, 0x18);

                if (!(pyChildrenListMemory?.Length == 0x18))
                    return null;

                var pyChildrenDictAddress = BitConverter.ToUInt64(pyChildrenListMemory.Value.Span[0x10..]);

                var pyChildrenDictEntries = pythonReader.ReadActiveDictionaryEntriesFromDictionaryAddress(pyChildrenDictAddress);

                //  Console.WriteLine($"Found {(pyChildrenDictEntries is null ? "no" : "some")} children dictionary entries for 0x{nodeAddress:X}");

                if (pyChildrenDictEntries is null)
                    return null;

                var childrenEntry =
                    pyChildrenDictEntries
                    .FirstOrDefault(dictionaryEntry =>
                    {
                        if (pythonReader.getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key) is not "str")
                            return false;

                        var keyString = pythonReader.readPythonStringValueMaxLength4000(dictionaryEntry.key);

                        return keyString == "_childrenObjects";
                    });

                //  Console.WriteLine($"Found {(childrenEntry.value == 0 ? "no" : "a")} dictionary entry for children of 0x{nodeAddress:X}");

                if (childrenEntry.value == 0)
                    return null;

                if (pythonReader.getPythonTypeNameFromPythonObjectAddress(childrenEntry.value).Equals("PyChildrenList"))
                {
                    pyChildrenListMemory = memoryReader.ReadBytes(childrenEntry.value, 0x18);

                    if (!(pyChildrenListMemory?.Length == 0x18))
                        return null;

                    pyChildrenDictAddress = BitConverter.ToUInt64(pyChildrenListMemory.Value.Span[0x10..]);

                    pyChildrenDictEntries = pythonReader.ReadActiveDictionaryEntriesFromDictionaryAddress(pyChildrenDictAddress);

                    if (pyChildrenDictEntries is null)
                        return null;

                    childrenEntry =
                        pyChildrenDictEntries
                        .FirstOrDefault(dictionaryEntry =>
                        {
                            if (pythonReader.getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key) is not "str")
                                return false;

                            var keyString = pythonReader.readPythonStringValueMaxLength4000(dictionaryEntry.key);

                            return keyString == "_childrenObjects";
                        });
                }

                var pythonListObjectMemory = memoryReader.ReadBytes(childrenEntry.value, 0x20);

                if (!(pythonListObjectMemory?.Length == 0x20))
                    return null;

                //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/listobject.h

                var list_ob_size = BitConverter.ToUInt64(pythonListObjectMemory.Value.Span[0x10..]);

                if (4000 < list_ob_size)
                    return null;

                var listEntriesSize = (int)list_ob_size * 8;

                var list_ob_item = BitConverter.ToUInt64(pythonListObjectMemory.Value.Span[0x18..]);

                var listEntriesMemory = memoryReader.ReadBytes(list_ob_item, listEntriesSize);

                if (!(listEntriesMemory?.Length == listEntriesSize))
                    return null;

                var listEntries = TransformMemoryContent.AsULongMemory(listEntriesMemory.Value);

                //  Console.WriteLine($"Found {listEntries.Length} children entries for 0x{nodeAddress:X}: " + String.Join(", ", listEntries.Select(childAddress => $"0x{childAddress:X}").ToArray()));

                return
                     [.. listEntries
                     .ToArray()
                     .Select(childAddress => ReadUITreeFromAddress(childAddress, memoryReader, maxDepth - 1, cache))];
            }

            // the child is not a UI tree node
            if (childrenDictEntry.value is PyColor colorValue)
            {
                // we've seen PyColor here, but we can't use it
                return null;
            }

            // left this here to see what other objects we can see
            return null;
        }

        var dictEntriesOfInterestLessNoneType =
            dictEntriesOfInterest
            .Where(entry =>
            !(entry.value is UITreeNode.DictEntryValueGenericRepresentation entryValue && entryValue.pythonObjectTypeName is "NoneType"))
            .ToArray();

        var dictEntriesOfInterestDict =
            dictEntriesOfInterestLessNoneType.Aggregate(
                seed: ImmutableDictionary<string, object>.Empty,
                func: (dict, entry) => dict.SetItem(entry.key, entry.value));

        return new UITreeNode
        (
            pythonObjectAddress: nodeAddress,
            pythonObjectTypeName: pythonObjectTypeName,
            dictEntriesOfInterest: dictEntriesOfInterestDict,
            otherDictEntriesKeys: [.. otherDictEntriesKeys],
            children: ReadChildren(pythonReader)?.Where(child => child != null)?.ToArray()
        );
    }

    static public string SerializeMemoryReadingNodeToJson(object obj) =>
        System.Text.Json.JsonSerializer.Serialize(obj, MemoryReadingJsonSerializerOptions);

    static public System.Text.Json.JsonSerializerOptions MemoryReadingJsonSerializerOptions =>
        new()
        {
            Converters =
            {
            //  Support common JSON parsers: Wrap large integers in a string to work around limitations there. (https://discourse.elm-lang.org/t/how-to-parse-a-json-object/4977)
            new JavaScript.Int64JsonConverter(),
            new JavaScript.UInt64JsonConverter()
            }
        };
}

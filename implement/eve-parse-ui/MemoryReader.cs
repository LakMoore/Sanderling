using read_memory_64_bit;
using System.Diagnostics;

namespace eve_parse_ui
{
    public record MemoryReader
    {
        public static ulong? FindUIRootAddressFromProcessId(int processId)
        {
            var candidatesAddresses =
                EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(processId);

            using (var memoryReader = new MemoryReaderFromLiveProcess(processId))
            {
                return
                    candidatesAddresses
                    .Select(candidateAddress => EveOnline64.ReadUITreeFromAddress(candidateAddress, memoryReader, 99))
                    .OrderByDescending(uiTree => uiTree?.EnumerateSelfAndDescendants().Count() ?? -1)
                    .Select(uiTree => uiTree?.pythonObjectAddress)
                    .FirstOrDefault();
            }
        }

        public static UITreeNode? ReadMemory(int processId, ulong uiRootAddress)
        {
            using (var memoryReader = new read_memory_64_bit.MemoryReaderFromLiveProcess(processId))
            {
                var uiTree = EveOnline64.ReadUITreeFromAddress(uiRootAddress, memoryReader, 99);

                if (uiTree != null)
                {
                    // if we're running from the IDE
                    if (Debugger.IsAttached)
                    {
                        return uiTree;
                    }
                    return uiTree.WithOtherDictEntriesRemoved();
                }
            }

            return null;
        }
    }
}

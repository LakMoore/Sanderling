using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace read_memory_64_bit;

class Program
{
    static string AppVersionId => "2025-04-20";

    static int Main(string[] args)
    {
        var app = new CommandLineApplication
        {
            Name = "read-memory-64-bit",
            Description = "Welcome to the Sanderling memory reading command-line interface. This tool helps you read objects from the memory of a 64-bit EVE Online client process and save it to a file. In addition to that, you have the option to save the entire memory contents of a game client process to a file.\nTo get help or report an issue, see the project website at https://github.com/Arcitectus/Sanderling",
        };

        app.HelpOption(inherited: true);

        app.VersionOption(template: "-v|--version", shortFormVersion: "version " + AppVersionId);

        app.Command("save-process-sample", saveProcessSampleCmd =>
        {
            saveProcessSampleCmd.Description = "Save a sample from a live process to a file. Use the '--pid' parameter to specify the process id.";

            var processIdParam =
            saveProcessSampleCmd.Option("--pid", "[Required] Id of the Windows process to read from.", CommandOptionType.SingleValue).IsRequired(errorMessage: "From which process should I read?");

            var delaySecondsParam =
            saveProcessSampleCmd.Option("--delay", "Timespan to wait before starting the collection of the sample, in seconds.", CommandOptionType.SingleValue);

            saveProcessSampleCmd.OnExecute(() =>
            {
                var processIdArgument = processIdParam.Value();

                var delayMilliSeconds =
                delaySecondsParam.HasValue() ?
                (int)(double.Parse(delaySecondsParam.Value()) * 1000) :
                0;

                var processId = int.Parse(processIdArgument);

                if (0 < delayMilliSeconds)
                {
                    Console.WriteLine("Delaying for " + delayMilliSeconds + " milliseconds.");
                    Task.Delay(TimeSpan.FromMilliseconds(delayMilliSeconds)).Wait();
                }

                Console.WriteLine("Starting to collect the sample...");

                var processSampleFile = GetProcessSampleFileFromProcessId(processId);

                Console.WriteLine("Completed collecting the sample.");

                var processSampleId =
                Convert.ToHexStringLower(
                    System.Security.Cryptography.SHA256.HashData(processSampleFile));

                var fileName = "process-sample-" + processSampleId[..10] + ".zip";

                System.IO.File.WriteAllBytes(fileName, processSampleFile);

                Console.WriteLine("Saved sample {0} to file '{1}'.", processSampleId, fileName);
            });
        });

        app.Command("read-memory-eve-online", readMemoryEveOnlineCmd =>
        {
            readMemoryEveOnlineCmd.Description = "Read the memory of an 64 bit EVE Online client process. You can use a live process ('--pid') or a process sample file ('--source-file') as the source.";

            var processIdParam = readMemoryEveOnlineCmd.Option("--pid", "Id of the Windows process to read from.", CommandOptionType.SingleValue);
            var rootAddressParam = readMemoryEveOnlineCmd.Option("--root-address", "Address of the UI root. If the address is not specified, the program searches the whole process memory for UI roots.", CommandOptionType.SingleValue);
            var sourceFileParam = readMemoryEveOnlineCmd.Option("--source-file", "Process sample file to read from.", CommandOptionType.SingleValue);
            var outputFileParam = readMemoryEveOnlineCmd.Option("--output-file", "File to save the memory reading result to.", CommandOptionType.SingleValue);
            var removeOtherDictEntriesParam = readMemoryEveOnlineCmd.Option("--remove-other-dict-entries", "Use this to remove the other dict entries from the UI nodes in the resulting JSON representation.", CommandOptionType.NoValue);
            var warmupIterationsParam = readMemoryEveOnlineCmd.Option("--warmup-iterations", "Only to measure execution time: Use this to perform additional warmup runs before measuring execution time.", CommandOptionType.SingleValue);

            readMemoryEveOnlineCmd.OnExecute(() =>
            {
                var processIdArgument = processIdParam.Value();
                var rootAddressArgument = rootAddressParam.Value();
                var sourceFileArgument = sourceFileParam.Value();
                var outputFileArgument = outputFileParam.Value();
                var removeOtherDictEntriesArgument = removeOtherDictEntriesParam.HasValue();
                var warmupIterationsArgument = warmupIterationsParam.Value();

                var processId =
                    0 < processIdArgument?.Length
                    ?
                    (int?)int.Parse(processIdArgument)
                    :
                    null;

                (IMemoryReader, IImmutableList<ulong>) GetMemoryReaderAndRootAddressesFromProcessSampleFile(byte[] processSampleFile)
                {
                    var processSampleId =
                    Convert.ToHexStringLower(
                        System.Security.Cryptography.SHA256.HashData(processSampleFile));

                    Console.WriteLine($"Reading from process sample {processSampleId}.");

                    var processSampleUnpacked = ProcessSample.ProcessSampleFromZipArchive(processSampleFile);

                    var memoryReader = new MemoryReaderFromProcessSample(processSampleUnpacked.memoryRegions);

                    var searchUIRootsStopwatch = System.Diagnostics.Stopwatch.StartNew();

                    var memoryRegions =
                        processSampleUnpacked.memoryRegions
                        .Select(memoryRegion => (memoryRegion.baseAddress, length: memoryRegion.content.Value.Length))
                        .ToImmutableList();

                    var uiRootCandidatesAddresses =
                        EveOnline64.EnumeratePossibleAddressesForUIRootObjects(memoryRegions, memoryReader)
                        .ToImmutableList();

                    searchUIRootsStopwatch.Stop();

                    Console.WriteLine($"Found {uiRootCandidatesAddresses.Count} candidates for UIRoot in {(int)searchUIRootsStopwatch.Elapsed.TotalSeconds} seconds: " + string.Join(",", uiRootCandidatesAddresses.Select(address => $"0x{address:X}")));

                    return (memoryReader, uiRootCandidatesAddresses);
                }

                (IMemoryReader, IImmutableList<ulong>) GetMemoryReaderAndWithSpecifiedRootFromProcessSampleFile(byte[] processSampleFile, ulong rootAddress)
                {
                    var processSampleId =
                    Convert.ToHexStringLower(
                        System.Security.Cryptography.SHA256.HashData(processSampleFile));

                    Console.WriteLine($"Reading from process sample {processSampleId}.");

                    var processSampleUnpacked = ProcessSample.ProcessSampleFromZipArchive(processSampleFile);

                    var memoryReader = new MemoryReaderFromProcessSample(processSampleUnpacked.memoryRegions);

                    Console.WriteLine($"Reading UIRoot from specified address: {rootAddress}");

                    return (memoryReader, ImmutableList<ulong>.Empty.Add(rootAddress));
                }

                (IMemoryReader, IImmutableList<ulong>) GetMemoryReaderAndRootAddresses()
                {
                    if (processId.HasValue)
                    {
                        var possibleRootAddresses = 0 < rootAddressArgument?.Length ? ImmutableList.Create(ParseULong(rootAddressArgument)) : EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(processId.Value);

                        return (new MemoryReaderFromLiveProcess(processId.Value), possibleRootAddresses);
                    }

                    if (!(0 < sourceFileArgument?.Length))
                    {
                        throw new Exception("Where should I read from?");
                    }

                    if (0 < rootAddressArgument?.Length)
                    {
                        return GetMemoryReaderAndWithSpecifiedRootFromProcessSampleFile(System.IO.File.ReadAllBytes(sourceFileArgument), ParseULong(rootAddressArgument));
                    }

                    return GetMemoryReaderAndRootAddressesFromProcessSampleFile(System.IO.File.ReadAllBytes(sourceFileArgument));
                }

                var (memoryReader, uiRootCandidatesAddresses) = GetMemoryReaderAndRootAddresses();

                IImmutableList<UITreeNode> ReadUITrees() =>
                    uiRootCandidatesAddresses
                    .Select(uiTreeRoot => EveOnline64.ReadUITreeFromAddress(uiTreeRoot, memoryReader, 99))
                    .Where(uiTree => uiTree != null)
                    .ToImmutableList();

                if (warmupIterationsArgument != null)
                {
                    var iterations = int.Parse(warmupIterationsArgument);

                    Console.WriteLine("Performing " + iterations + " warmup iterations...");

                    for (var i = 0; i < iterations; i++)
                    {
                        ReadUITrees().ToList();
                        System.Threading.Thread.Sleep(1111);
                    }
                }

                var readUiTreesStopwatch = System.Diagnostics.Stopwatch.StartNew();

                var uiTrees = ReadUITrees();

                readUiTreesStopwatch.Stop();

                var uiTreesWithStats =
                    uiTrees
                    .Select(uiTree =>
                    new
                    {
                        uiTree = uiTree,
                        nodeCount = uiTree.EnumerateSelfAndDescendants().Count()
                    })
                    .OrderByDescending(uiTreeWithStats => uiTreeWithStats.nodeCount)
                    .ToImmutableList();

                var uiTreesReport =
                    uiTreesWithStats
                    .Select(uiTreeWithStats => $"\n0x{uiTreeWithStats.uiTree.pythonObjectAddress:X}: {uiTreeWithStats.nodeCount} nodes.")
                    .ToImmutableList();

                Console.WriteLine($"Read {uiTrees.Count} UI trees in {(int)readUiTreesStopwatch.Elapsed.TotalMilliseconds} milliseconds:" + string.Join("", uiTreesReport));

                var largestUiTree =
                    uiTreesWithStats
                    .OrderByDescending(uiTreeWithStats => uiTreeWithStats.nodeCount)
                    .FirstOrDefault().uiTree;

                if (largestUiTree != null)
                {
                    var uiTreePreparedForFile = largestUiTree;

                    if (removeOtherDictEntriesArgument)
                    {
                        uiTreePreparedForFile = uiTreePreparedForFile.WithOtherDictEntriesRemoved();
                    }

                    var serializeStopwatch = System.Diagnostics.Stopwatch.StartNew();

                    var uiTreeAsJson = EveOnline64.SerializeMemoryReadingNodeToJson(uiTreePreparedForFile);

                    serializeStopwatch.Stop();

                    Console.WriteLine(
                        "Serialized largest tree to " + uiTreeAsJson.Length + " characters of JSON in " +
                        serializeStopwatch.ElapsedMilliseconds + " milliseconds.");

                    var fileContent = System.Text.Encoding.UTF8.GetBytes(uiTreeAsJson);

                    var sampleId = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(fileContent));

                    var outputFilePath = outputFileArgument;

                    if (!(0 < outputFileArgument?.Length))
                    {
                        var outputFileName = "eve-online-memory-reading-" + sampleId[..10] + ".json";

                        outputFilePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), outputFileName);

                        Console.WriteLine(
                            "I found no configuration of an output file path, so I use '" +
                            outputFilePath + "' as the default.");
                    }

                    System.IO.File.WriteAllBytes(outputFilePath, fileContent);

                    Console.WriteLine($"I saved memory reading {sampleId} from address 0x{largestUiTree.pythonObjectAddress:X} to file '{outputFilePath}'.");
                }
                else
                {
                    Console.WriteLine("No largest UI tree.");
                }
            });
        });

        app.OnExecute(() =>
        {
            Console.WriteLine("Please specify a subcommand.");
            app.ShowHelp();

            return 1;
        });

        return app.Execute(args);
    }

    static byte[] GetProcessSampleFileFromProcessId(int processId)
    {
        var process = System.Diagnostics.Process.GetProcessById(processId);

        var beginMainWindowClientAreaScreenshotBmp = BMPFileFromBitmap(GetScreenshotOfWindowClientAreaAsBitmap(process.MainWindowHandle));

        var (committedRegions, logEntries) = EveOnline64.ReadCommittedMemoryRegionsWithContentFromProcessId(processId);

        var endMainWindowClientAreaScreenshotBmp = BMPFileFromBitmap(GetScreenshotOfWindowClientAreaAsBitmap(process.MainWindowHandle));

        return ProcessSample.ZipArchiveFromProcessSample(
            committedRegions,
            logEntries,
            beginMainWindowClientAreaScreenshotBmp: beginMainWindowClientAreaScreenshotBmp,
            endMainWindowClientAreaScreenshotBmp: endMainWindowClientAreaScreenshotBmp);
    }

    //  Screenshot implementation found at https://github.com/Viir/bots/blob/225c680115328d9ba0223760cec85d56f2ea9a87/implement/templates/locate-object-in-window/src/BotEngine/VolatileHostWindowsApi.elm#L479-L557

    static public byte[] BMPFileFromBitmap(System.Drawing.Bitmap bitmap)
    {
        using var stream = new System.IO.MemoryStream();

        bitmap.Save(stream, format: System.Drawing.Imaging.ImageFormat.Bmp);
        return stream.ToArray();
    }

    static public int[][] GetScreenshotOfWindowAsPixelsValuesR8G8B8(IntPtr windowHandle)
    {
        var screenshotAsBitmap = GetScreenshotOfWindowAsBitmap(windowHandle);
        if (screenshotAsBitmap == null)
            return null;
        var bitmapData = screenshotAsBitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, screenshotAsBitmap.Width, screenshotAsBitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        int byteCount = bitmapData.Stride * screenshotAsBitmap.Height;
        byte[] pixelsArray = new byte[byteCount];
        IntPtr ptrFirstPixel = bitmapData.Scan0;
        Marshal.Copy(ptrFirstPixel, pixelsArray, 0, pixelsArray.Length);
        screenshotAsBitmap.UnlockBits(bitmapData);
        var pixels = new int[screenshotAsBitmap.Height][];
        for (var rowIndex = 0; rowIndex < screenshotAsBitmap.Height; ++rowIndex)
        {
            var rowPixelValues = new int[screenshotAsBitmap.Width];
            for (var columnIndex = 0; columnIndex < screenshotAsBitmap.Width; ++columnIndex)
            {
                var pixelBeginInArray = bitmapData.Stride * rowIndex + columnIndex * 3;
                var red = pixelsArray[pixelBeginInArray + 2];
                var green = pixelsArray[pixelBeginInArray + 1];
                var blue = pixelsArray[pixelBeginInArray + 0];
                rowPixelValues[columnIndex] = (red << 16) | (green << 8) | blue;
            }
            pixels[rowIndex] = rowPixelValues;
        }
        return pixels;
    }

    //  https://github.com/Viir/bots/blob/225c680115328d9ba0223760cec85d56f2ea9a87/implement/templates/locate-object-in-window/src/BotEngine/VolatileHostWindowsApi.elm#L535-L557
    static public System.Drawing.Bitmap GetScreenshotOfWindowAsBitmap(IntPtr windowHandle)
    {
        SetProcessDPIAware();
        var windowRect = new WinApi.Rect();
        if (WinApi.GetWindowRect(windowHandle, ref windowRect) == IntPtr.Zero)
            return null;
        int width = windowRect.right - windowRect.left;
        int height = windowRect.bottom - windowRect.top;
        var asBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        System.Drawing.Graphics.FromImage(asBitmap).CopyFromScreen(
            windowRect.left,
            windowRect.top,
            0,
            0,
            new System.Drawing.Size(width, height),
            System.Drawing.CopyPixelOperation.SourceCopy);
        return asBitmap;
    }

    static public System.Drawing.Bitmap GetScreenshotOfWindowClientAreaAsBitmap(IntPtr windowHandle)
    {
        SetProcessDPIAware();

        var clientRect = new WinApi.Rect();

        if (WinApi.GetClientRect(windowHandle, ref clientRect) == IntPtr.Zero)
            return null;

        var clientRectLeftTop = new WinApi.Point { x = clientRect.left, y = clientRect.top };
        var clientRectRightBottom = new WinApi.Point { x = clientRect.right, y = clientRect.bottom };

        WinApi.ClientToScreen(windowHandle, ref clientRectLeftTop);
        WinApi.ClientToScreen(windowHandle, ref clientRectRightBottom);

        clientRect = new WinApi.Rect
        {
            left = clientRectLeftTop.x,
            top = clientRectLeftTop.y,
            right = clientRectRightBottom.x,
            bottom = clientRectRightBottom.y
        };

        int width = clientRect.right - clientRect.left;
        int height = clientRect.bottom - clientRect.top;
        var asBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        System.Drawing.Graphics.FromImage(asBitmap).CopyFromScreen(
            clientRect.left,
            clientRect.top,
            0,
            0,
            new System.Drawing.Size(width, height),
            System.Drawing.CopyPixelOperation.SourceCopy);
        return asBitmap;
    }

    static void SetProcessDPIAware()
    {
        //  https://www.google.com/search?q=GetWindowRect+dpi
        //  https://github.com/dotnet/wpf/issues/859
        //  https://github.com/dotnet/winforms/issues/135
        WinApi.SetProcessDPIAware();
    }

    static ulong ParseULong(string asString)
    {
        if (asString.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            return ulong.Parse(asString[2..], System.Globalization.NumberStyles.HexNumber);

        return ulong.Parse(asString);
    }
}

public class EveOnline64
{
    static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjectsFromProcessId(int processId)
    {
        var memoryReader = new MemoryReaderFromLiveProcess(processId);

        var (committedMemoryRegions, _) = ReadCommittedMemoryRegionsWithoutContentFromProcessId(processId);

        return EnumeratePossibleAddressesForUIRootObjects(committedMemoryRegions, memoryReader);
    }

    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsWithContentFromProcessId(int processId)
    {
        var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: true);

        return genericResult;
    }

    static public (IImmutableList<(ulong baseAddress, int length)> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsWithoutContentFromProcessId(int processId)
    {
        var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: false);

        var memoryRegions =
            genericResult.memoryRegions
            .Select(memoryRegion => (baseAddress: memoryRegion.baseAddress, length: (int)memoryRegion.length))
            .ToImmutableList();

        return (memoryRegions, genericResult.logEntries);
    }

    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsFromProcessId(
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

        var processHandle = WinApi.OpenProcess(
            (int)(WinApi.ProcessAccessFlags.QueryInformation | WinApi.ProcessAccessFlags.VirtualMemoryRead), false, processId);

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

            logLine($"{m.BaseAddress}-{(uint)m.BaseAddress + (uint)m.RegionSize - 1} : {m.RegionSize} bytes result={result}, state={(WinApi.MemoryInformationState)m.State}, type={(WinApi.MemoryInformationType)m.Type}, protection={regionProtection}");

            if (address == (long)m.BaseAddress + (long)m.RegionSize)
                break;

            address = (long)m.BaseAddress + (long)m.RegionSize;

            if (m.State != WinApi.MemoryInformationState.MEM_COMMIT)
                continue;

            var protectionFlagsToSkip = WinApi.MemoryInformationProtection.PAGE_GUARD | WinApi.MemoryInformationProtection.PAGE_NOACCESS;

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

                WinApi.ReadProcessMemory(processHandle, (ulong)regionBaseAddress, regionContentBuffer, (UIntPtr)regionContentBuffer.LongLength, ref bytesRead);

                if (bytesRead.ToUInt64() != (ulong)regionContentBuffer.LongLength)
                    throw new Exception($"Failed to ReadProcessMemory at 0x{regionBaseAddress:X}: Only read " + bytesRead + " bytes.");

                regionContent = regionContentBuffer;
            }

            committedRegions.Add(new SampleMemoryRegion(
                baseAddress: (ulong)regionBaseAddress,
                length: (ulong)m.RegionSize,
                content: regionContent));

        } while (true);

        logLine($"Found {committedRegions.Count} committed regions with a total size of {committedRegions.Select(region => (long)region.length).Sum()}.");

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

            if (asMemory == null)
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

            if (asByteArray == null)
                return null;

            return TransformMemoryContent.AsULongMemory(asByteArray.Value);
        }

        IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectType()
        {
            IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectTypeInMemoryRegion((ulong baseAddress, int length) memoryRegion)
            {
                var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

                if (memoryRegionContentAsULongArray == null)
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

                    if (candidate_tp_name != "type")
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

                if (memoryRegionContentAsULongArray == null)
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

                    if (candidate_tp_name == null)
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

                if (memoryRegionContentAsULongArray == null)
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
        "ramp_active",

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
        "isSelected"
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

        if (dictionaryEntries == null)
            return null;

        var dictEntriesOfInterest = new List<UITreeNode.DictEntry>();

        var otherDictEntriesKeys = new List<string>();

        localMemoryReadingTools.GetDictEntryValueRepresentation = pythonReader.GetDictEntryValueRepresentation;

        foreach (var dictionaryEntry in dictionaryEntries)
        {
            var keyObject_type_name = pythonReader.getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key);

            //  Console.WriteLine($"Dict entry type name is '{keyObject_type_name}'");

            if (keyObject_type_name != "str")
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

            if (childrenDictEntry == null)
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

                //  Console.WriteLine($"Found {(pyChildrenDictEntries == null ? "no" : "some")} children dictionary entries for 0x{nodeAddress:X}");

                if (pyChildrenDictEntries == null)
                    return null;

                var childrenEntry =
                    pyChildrenDictEntries
                    .FirstOrDefault(dictionaryEntry =>
                    {
                        if (pythonReader.getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key) != "str")
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

                    if (pyChildrenDictEntries == null)
                        return null;

                    childrenEntry =
                        pyChildrenDictEntries
                        .FirstOrDefault(dictionaryEntry =>
                        {
                            if (pythonReader.getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key) != "str")
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
                     listEntries
                     .ToArray()
                     .Select(childAddress => ReadUITreeFromAddress(childAddress, memoryReader, maxDepth - 1, cache))
                     .ToArray();
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
            .Where(entry => !((entry.value as UITreeNode.DictEntryValueGenericRepresentation)?.pythonObjectTypeName == "NoneType"))
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
                new Int64JsonConverter(),
                new UInt64JsonConverter()
            }
        };
}


public interface IMemoryReader
{
    ReadOnlyMemory<byte>? ReadBytes(ulong startAddress, int length);
}

public class MemoryReaderFromProcessSample : IMemoryReader
{
    readonly IImmutableList<SampleMemoryRegion> memoryRegionsOrderedByAddress;

    public MemoryReaderFromProcessSample(IImmutableList<SampleMemoryRegion> memoryRegions)
    {
        memoryRegionsOrderedByAddress =
            memoryRegions
            .OrderBy(memoryRegion => memoryRegion.baseAddress)
            .ToImmutableList();
    }

    public ReadOnlyMemory<byte>? ReadBytes(ulong startAddress, int length)
    {
        var memoryRegion =
            memoryRegionsOrderedByAddress
            .Where(region => region.baseAddress <= startAddress)
            .LastOrDefault();

        if (memoryRegion?.content == null)
            return null;

        var start = startAddress - memoryRegion.baseAddress;

        if ((int)start < 0)
            return null;

        if (memoryRegion.content.Value.Length <= (int)start)
            return null;

        return
            memoryRegion?.content?.Slice((int)start, Math.Min(length, memoryRegion.content.Value.Length - (int)start));
    }
}


public class MemoryReaderFromLiveProcess : IMemoryReader, IDisposable
{
    readonly IntPtr processHandle;

    public MemoryReaderFromLiveProcess(int processId)
    {
        processHandle = WinApi.OpenProcess(
            (int)(WinApi.ProcessAccessFlags.QueryInformation | WinApi.ProcessAccessFlags.VirtualMemoryRead), false, processId);
    }

    public void Dispose()
    {
        if (processHandle != IntPtr.Zero)
            WinApi.CloseHandle(processHandle);
    }

    public ReadOnlyMemory<byte>? ReadBytes(ulong startAddress, int length)
    {
        var buffer = new byte[length];

        UIntPtr numberOfBytesReadAsPtr = UIntPtr.Zero;

        if (!WinApi.ReadProcessMemory(processHandle, startAddress, buffer, (UIntPtr)buffer.LongLength, ref numberOfBytesReadAsPtr))
            return null;

        var numberOfBytesRead = numberOfBytesReadAsPtr.ToUInt64();

        if (numberOfBytesRead == 0)
            return null;

        if (int.MaxValue < numberOfBytesRead)
            return null;

        if (numberOfBytesRead == (ulong)buffer.LongLength)
            return buffer;

        return buffer;
    }
}

/// <summary>
/// Offsets from https://docs.python.org/2/c-api/structures.html
/// </summary>
public class PyObject
{
    public const int Offset_ob_refcnt = 0;
    public const int Offset_ob_type = 8;
}

public record UITreeNode(
    ulong pythonObjectAddress,
    string pythonObjectTypeName,
    IReadOnlyDictionary<string, object> dictEntriesOfInterest,
    string[] otherDictEntriesKeys,
    IReadOnlyList<UITreeNode> children)
{
    public record DictEntryValueGenericRepresentation(
        ulong address,
        string pythonObjectTypeName);

    public record DictEntry(
        string key,
        object value);

    public record Bunch(
        System.Text.Json.Nodes.JsonObject entriesOfInterest);

    public IEnumerable<UITreeNode> EnumerateSelfAndDescendants() =>
        new[] { this }
        .Concat((children ?? Array.Empty<UITreeNode>()).SelectMany(child => child?.EnumerateSelfAndDescendants() ?? ImmutableList<UITreeNode>.Empty));

    public UITreeNode WithOtherDictEntriesRemoved()
    {
        return new UITreeNode
        (
            pythonObjectAddress: pythonObjectAddress,
            pythonObjectTypeName: pythonObjectTypeName,
            dictEntriesOfInterest: dictEntriesOfInterest,
            otherDictEntriesKeys: null,
            children: children?.Select(child => child?.WithOtherDictEntriesRemoved()).ToArray()
        );
    }

}

static class TransformMemoryContent
{
    static public ReadOnlyMemory<ulong> AsULongMemory(ReadOnlyMemory<byte> byteMemory) =>
        MemoryMarshal.Cast<byte, ulong>(byteMemory.Span).ToArray();
}

class ProcessSample
{
    static public byte[] ZipArchiveFromProcessSample(
        IImmutableList<SampleMemoryRegion> memoryRegions,
        IImmutableList<string> logEntries,
        byte[] beginMainWindowClientAreaScreenshotBmp,
        byte[] endMainWindowClientAreaScreenshotBmp)
    {
        var screenshotEntriesCandidates = new[]
        {
            (filePath: ImmutableList.Create("begin-main-window-client-area.bmp"), content: beginMainWindowClientAreaScreenshotBmp),
            (filePath: ImmutableList.Create("end-main-window-client-area.bmp"), content: endMainWindowClientAreaScreenshotBmp),
        };

        var screenshotEntries =
            screenshotEntriesCandidates
            .Where(filePathAndContent => filePathAndContent.content != null)
            .Select(filePathAndContent => new KeyValuePair<IImmutableList<string>, byte[]>(
                filePathAndContent.filePath, filePathAndContent.content))
            .ToArray();

        var zipArchiveEntries =
            memoryRegions.ToImmutableDictionary(
                region => (IImmutableList<string>)(["Process", "Memory", $"0x{region.baseAddress:X}"]),
                region => region.content.Value.ToArray())
            .Add(new[] { "copy-memory-log" }.ToImmutableList(), System.Text.Encoding.UTF8.GetBytes(String.Join("\n", logEntries)))
            .AddRange(screenshotEntries);

        return Pine.ZipArchive.ZipArchiveFromEntries(zipArchiveEntries);
    }

    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> copyMemoryLog) ProcessSampleFromZipArchive(byte[] sampleFile)
    {
        var files =
            Pine.ZipArchive.EntriesFromZipArchive(sampleFile);

        IEnumerable<(IImmutableList<string> filePath, byte[] fileContent)> GetFilesInDirectory(IImmutableList<string> directory)
        {
            foreach (var fileFullPathAndContent in files)
            {
                var fullPath = fileFullPathAndContent.name.Split(new[] { '/', '\\' });

                if (!fullPath.Take(directory.Count).SequenceEqual(directory))
                    continue;

                yield return (fullPath.Skip(directory.Count).ToImmutableList(), fileFullPathAndContent.content);
            }
        }

        var memoryRegions =
            GetFilesInDirectory(ImmutableList.Create("Process", "Memory"))
            .Where(fileSubpathAndContent => fileSubpathAndContent.filePath.Count == 1)
            .Select(fileSubpathAndContent =>
            {
                var baseAddressBase16 = System.Text.RegularExpressions.Regex.Match(fileSubpathAndContent.filePath.Single(), @"0x(.+)").Groups[1].Value;

                var baseAddress = ulong.Parse(baseAddressBase16, System.Globalization.NumberStyles.HexNumber);

                return new SampleMemoryRegion(
                    baseAddress,
                    length: (ulong)fileSubpathAndContent.fileContent.LongLength,
                    content: fileSubpathAndContent.fileContent);
            }).ToImmutableList();

        return (memoryRegions, null);
    }
}

public record SampleMemoryRegion(
    ulong baseAddress,
    ulong length,
    ReadOnlyMemory<byte>? content);

public class Int64JsonConverter : System.Text.Json.Serialization.JsonConverter<long>
{
    public override long Read(
        ref System.Text.Json.Utf8JsonReader reader,
        Type typeToConvert,
        System.Text.Json.JsonSerializerOptions options) =>
            long.Parse(reader.GetString()!);

    public override void Write(
        System.Text.Json.Utf8JsonWriter writer,
        long integer,
        System.Text.Json.JsonSerializerOptions options) =>
            writer.WriteStringValue(integer.ToString());
}

public class UInt64JsonConverter : System.Text.Json.Serialization.JsonConverter<ulong>
{
    public override ulong Read(
        ref System.Text.Json.Utf8JsonReader reader,
        Type typeToConvert,
        System.Text.Json.JsonSerializerOptions options) =>
            ulong.Parse(reader.GetString()!);

    public override void Write(
        System.Text.Json.Utf8JsonWriter writer,
        ulong integer,
        System.Text.Json.JsonSerializerOptions options) =>
            writer.WriteStringValue(integer.ToString());
}

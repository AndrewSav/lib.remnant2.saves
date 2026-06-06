using lib.remnant2.saves;
using System.Buffers.Binary;
using lib.remnant2.saves.Compression;
using lib.remnant2.saves.Compression.Model;
using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;
using Serilog;
using Log = lib.remnant2.saves.Log;

namespace examples;

internal class Tests
{
    public static void Run()
    {
        // This can be specified to see logging events from the library
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()  // Change this if needed
            .WriteTo.Console()
            .CreateLogger();

        int saveIndex = Utils.GetSaveIndex();          // which world save (save_<index>.sav) to test; or DEBUG_REMNANT_SAVE_INDEX env var
        string folder = Utils.GetSteamSavePath();
        string profilePath = Path.Combine(folder, "profile.sav");
        string savePath = Utils.GetWorldSavePath(saveIndex);

        DoOne(profilePath);
        DoOne(savePath);

        Log.Logger = null!;
    }

    private static void DoOne(string path)
    {
        Console.WriteLine($"Testing {path}");

        byte[] decodedBeforeParsing = ReadDecompressedIfNeeded(path);
        string outputPrefix = GetOutputPrefix(path);
        string decodedBeforeParsingPath = $"{outputPrefix}.before-parse.dec";
        File.WriteAllBytes(decodedBeforeParsingPath, decodedBeforeParsing);

        Reader reader = new(decodedBeforeParsing);
        SaveFile sf = new(reader);

        byte[] firstWrite = WriteSaveData(sf);
        string parsedSavePath = $"{outputPrefix}.parsed.sav";
        SaveFile.Write(parsedSavePath, sf);

        byte[] decodedAfterParsing = ReadDecompressedIfNeeded(parsedSavePath);
        string decodedAfterParsingPath = $"{outputPrefix}.parsed.dec";
        File.WriteAllBytes(decodedAfterParsingPath, decodedAfterParsing);

        ReportComparison(decodedBeforeParsing, decodedAfterParsing);
        ReportWriteReadWriteComparison(firstWrite, parsedSavePath, outputPrefix);

        Console.WriteLine($"Saved before-parse bytes to {decodedBeforeParsingPath}");
        Console.WriteLine($"Saved parsed save to {parsedSavePath}");
        Console.WriteLine($"Saved parsed decompressed bytes to {decodedAfterParsingPath}");
        ReportDuplicateProperties(sf);
        ReportLengthMismatchPaths(sf);
        ReportPersistenceBlobResiduals(sf, decodedBeforeParsing);

        int count = 0;
        int offsetMismatchCount = 0;
        int lengthMismatchCount = 0;
        const int maxMismatchesToPrint = 20;
        sf.VisitObjects((node, _) =>
        {
            count++;
            if (node.ReadOffset != node.WriteOffset)
            {
                if (offsetMismatchCount < maxMismatchesToPrint)
                {
                    Console.WriteLine($"ReadOffset {node.ReadOffset} is not the same as WriteOffset {node.WriteOffset}");
                }
                offsetMismatchCount++;
                //Console.WriteLine($"At {node.DisplayPath}");
            }
            if (node.ReadLength != node.WriteLength)
            {
                if (lengthMismatchCount < maxMismatchesToPrint)
                {
                    Console.WriteLine($"ReadLength {node.ReadLength} is not the same as WriteLength {node.WriteLength}");
                }
                lengthMismatchCount++;
                //Console.WriteLine($"At {node.DisplayPath}");
            }
        });
        Console.WriteLine($"Found {count} objects");
        Console.WriteLine($"Read/write offset mismatches: {offsetMismatchCount}");
        Console.WriteLine($"Read/write length mismatches: {lengthMismatchCount}");
    }

    private static void ReportWriteReadWriteComparison(byte[] firstWrite, string writtenSavePath, string outputPrefix)
    {
        string firstWritePath = $"{outputPrefix}.write-before-read.dec";
        File.WriteAllBytes(firstWritePath, firstWrite);

        SaveFile readBack = SaveFile.Read(writtenSavePath);
        byte[] secondWrite = WriteSaveData(readBack);
        string secondWritePath = $"{outputPrefix}.write-after-read.dec";
        File.WriteAllBytes(secondWritePath, secondWrite);

        Console.WriteLine(firstWrite.SequenceEqual(secondWrite)
            ? "Written SaveData bytes and read-back written SaveData bytes are the same"
            : "Written SaveData bytes and read-back written SaveData bytes are different");
        Console.WriteLine($"First write SaveData length: {firstWrite.Length}");
        Console.WriteLine($"Read-back write SaveData length: {secondWrite.Length} ({FormatDelta(secondWrite.Length - firstWrite.Length)})");

        int firstDifference = GetFirstDifference(firstWrite, secondWrite);
        if (firstDifference != -1)
        {
            Console.WriteLine($"First write/read/write differing offset: 0x{firstDifference:X8} ({firstDifference})");
        }

        Console.WriteLine($"Saved first write SaveData bytes to {firstWritePath}");
        Console.WriteLine($"Saved read-back write SaveData bytes to {secondWritePath}");
    }

    private static byte[] WriteSaveData(SaveFile saveFile)
    {
        Writer writer = new();
        saveFile.SaveData.Write(writer);
        return writer.ToArray();
    }

    private static void ReportLengthMismatchPaths(SaveFile saveFile)
    {
        Navigator navigator = new(saveFile);
        List<Node> lengthMismatches = Flatten(navigator.Root)
            .Where(node => node.Object.ReadLength != node.Object.WriteLength)
            .ToList();

        Console.WriteLine($"Length mismatch paths: {lengthMismatches.Count}");
        foreach (Node node in lengthMismatches)
        {
            ModelBase obj = node.Object;
            int delta = obj.WriteLength - obj.ReadLength;
            Console.WriteLine(
                $"{FormatDelta(delta)} read={obj.ReadLength} write={obj.WriteLength} offset={obj.ReadOffset} path={node.DisplayPath}");
        }
    }

    private static void ReportPersistenceBlobResiduals(SaveFile saveFile, byte[] originalBytes)
    {
        Navigator navigator = new(saveFile);
        List<Node> residuals = Flatten(navigator.Root)
            .Where(node => node.Object is StructProperty { Type.Name: "PersistenceBlob", Value: ModelBase value }
                && node.Object.ReadOffset + node.Object.ReadLength > value.ReadOffset + value.ReadLength)
            .ToList();

        Console.WriteLine($"PersistenceBlob trailing residuals: {residuals.Count}");
        foreach (Node node in residuals)
        {
            StructProperty property = (StructProperty)node.Object;
            ModelBase value = (ModelBase)property.Value!;
            int trailingOffset = value.ReadOffset + value.ReadLength;
            int trailingLength = property.ReadOffset + property.ReadLength - trailingOffset;
            byte[] trailingBytes = originalBytes[trailingOffset..(trailingOffset + trailingLength)];

            Console.WriteLine(
                $"{trailingLength} bytes at 0x{trailingOffset:X8} ({trailingOffset}) path={node.DisplayPath}");
            Console.WriteLine($"  nested {value.GetType().Name} readOffset={value.ReadOffset} readLength={value.ReadLength}");
            Console.WriteLine($"  bytes: {BitConverter.ToString(trailingBytes)}");
        }
    }

    private static byte[] ReadDecompressedIfNeeded(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        return IsCompressedSave(data) ? Archive.DecompressSave(data) : data;
    }

    private static bool IsCompressedSave(byte[] data)
    {
        const int firstChunkHeaderOffset = 12;
        return data.Length >= firstChunkHeaderOffset + sizeof(ulong)
            && BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(firstChunkHeaderOffset)) == ChunkHeader.ArchiveHeaderTag;
    }

    private static string GetOutputPrefix(string sourcePath)
    {
        string outputDirectory = Path.Combine(AppContext.BaseDirectory, "roundtrip-tests");
        Directory.CreateDirectory(outputDirectory);

        string fileName = Path.GetFileNameWithoutExtension(sourcePath);
        return Path.Combine(outputDirectory, fileName);
    }

    private static void ReportComparison(byte[] expected, byte[] actual)
    {
        const int fileHeaderLength = 16;
        int commonLength = Math.Min(expected.Length, actual.Length);
        int overlappingDifferences = 0;
        int overlappingDifferencesAfterHeader = 0;
        int firstDifferenceAfterHeader = -1;

        for (int i = 0; i < commonLength; i++)
        {
            if (expected[i] == actual[i]) continue;

            overlappingDifferences++;
            if (i >= fileHeaderLength)
            {
                overlappingDifferencesAfterHeader++;
                if (firstDifferenceAfterHeader == -1)
                {
                    firstDifferenceAfterHeader = i;
                }
            }
        }

        int lengthDelta = actual.Length - expected.Length;
        bool same = lengthDelta == 0 && overlappingDifferences == 0;

        Console.WriteLine(same
            ? "Original decompressed bytes and parsed save bytes are the same"
            : "Original decompressed bytes and parsed save bytes are different");
        Console.WriteLine($"Before parse length: {expected.Length}");
        Console.WriteLine($"After parse length: {actual.Length} ({FormatDelta(lengthDelta)})");
        Console.WriteLine($"Overlapping byte differences: {overlappingDifferences}");
        Console.WriteLine($"Overlapping byte differences after FileHeader: {overlappingDifferencesAfterHeader}");

        int firstDifference = GetFirstDifference(expected, actual);
        if (firstDifference != -1)
        {
            Console.WriteLine($"First differing offset: 0x{firstDifference:X8} ({firstDifference})");
        }
        else if (lengthDelta != 0)
        {
            Console.WriteLine($"First differing offset: 0x{commonLength:X8} ({commonLength})");
        }
        if (firstDifferenceAfterHeader != -1)
        {
            Console.WriteLine($"First differing offset after FileHeader: 0x{firstDifferenceAfterHeader:X8} ({firstDifferenceAfterHeader})");
        }
        else if (lengthDelta != 0 && commonLength >= fileHeaderLength)
        {
            Console.WriteLine($"First differing offset after FileHeader: 0x{commonLength:X8} ({commonLength})");
        }
    }

    private static int GetFirstDifference(byte[] expected, byte[] actual)
    {
        int commonLength = Math.Min(expected.Length, actual.Length);
        for (int i = 0; i < commonLength; i++)
        {
            if (expected[i] != actual[i])
            {
                return i;
            }
        }

        return expected.Length == actual.Length ? -1 : commonLength;
    }

    private static string FormatDelta(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private static void ReportDuplicateProperties(SaveFile saveFile)
    {
        Navigator navigator = new(saveFile);
        List<Node> bagsWithDuplicateProperties = navigator.Root.Children
            .Append(navigator.Root)
            .SelectMany(Flatten)
            .Where(node => node.Object is PropertyBag bag && bag.Properties.Count != bag.Lookup.Count)
            .ToList();

        if (bagsWithDuplicateProperties.Count == 0)
        {
            Console.WriteLine("Property bags with duplicate keys: 0");
            return;
        }

        Console.WriteLine($"Property bags with duplicate keys: {bagsWithDuplicateProperties.Count}");
        foreach (Node node in bagsWithDuplicateProperties)
        {
            PropertyBag bag = (PropertyBag)node.Object;
            Console.WriteLine($"{node.DisplayPath}");
            Console.WriteLine($"  property count: {bag.Properties.Count}, lookup count: {bag.Lookup.Count}");

            foreach (IGrouping<string, (int Index, Property Property)> duplicateGroup in bag.Properties
                .Select((pair, index) => (Index: index, Property: pair.Value))
                .GroupBy(item => item.Property.Name.Name)
                .Where(group => group.Count() > 1))
            {
                Console.WriteLine($"  duplicate key: {duplicateGroup.Key}");
                bool first = true;
                foreach ((int index, Property property) in duplicateGroup)
                {
                    string kept = first ? "kept" : "dropped by lookup write";
                    Console.WriteLine(
                        $"    [{index}] {kept}, type={property.Type?.Name}, readOffset={property.ReadOffset}, readLength={property.ReadLength}");
                    first = false;
                }
            }
        }
    }

    private static IEnumerable<Node> Flatten(Node node)
    {
        yield return node;
        foreach (Node child in node.Children)
        {
            foreach (Node descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }
}

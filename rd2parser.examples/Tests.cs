using rd2parser.Model;
using Serilog;

namespace rd2parser.examples;

internal class Tests
{
    public static void Run()
    {
        // This can be specified to see logging events from the library
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()  // Change this if needed
            .WriteTo.Console()
            .CreateLogger();

        string folder = Utils.GetSteamSavePath();
        string profilePath = Path.Combine(folder, "profile.sav");
        string savePath = Path.Combine(folder, Environment.GetEnvironmentVariable("DEBUG_REMNANT_SAVE") ?? "save_0.sav");

        DoOne(profilePath);
        DoOne(savePath);


        Log.Logger = null!;
    }

    private static void DoOne(string path)
    {
        SaveFile sf = SaveFile.Read(path);

        Writer w = new();
        sf.SaveData.Write(w);
        byte[] decoded1 = w.ToArray();

        string targetPath = Path.GetFileName(Path.ChangeExtension(path, "dec"));
        File.WriteAllBytes(targetPath, decoded1);
        SaveFile.Write("test.DoOne", sf);
        sf = SaveFile.Read("test.DoOne");

        w = new();
        sf.SaveData.Write(w);
        byte[] decoded2 = w.ToArray();

        Console.WriteLine(decoded1.SequenceEqual(decoded2)
            ? "Original and read data is the same"
            : "Original and read data is different");

        SaveFile.Write("test.DoOne", sf);

        int count = 0;
        sf.VisitObjects(node =>
        {
            count++;
            if (node.ReadOffset != node.WriteOffset)
            {
                Console.WriteLine($"ReadOffset {node.ReadOffset} is not the same as WriteOffset {node.WriteOffset}");
                //Console.WriteLine($"At {node.DisplayPath}");
            }
            if (node.ReadLength != node.WriteLength)
            {
                Console.WriteLine($"ReadLength {node.ReadLength} is not the same as WriteLength {node.WriteLength}");
                //Console.WriteLine($"At {node.DisplayPath}");
            }
        });
        Console.WriteLine($"Found {count} objects");
    }
}

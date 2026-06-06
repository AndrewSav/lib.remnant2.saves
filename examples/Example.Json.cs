using lib.remnant2.saves;
using lib.remnant2.saves.Model;

namespace examples;
internal partial class Example
{
    public static void Json()
    {
        Console.WriteLine("Json===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // which world save (save_<index>.sav) to round-trip; or DEBUG_REMNANT_SAVE_INDEX env var
        // ========================

        string folder = Utils.GetSteamSavePath();
        string profilePath = Path.Combine(folder, "profile.sav");
        string savePath = Utils.GetWorldSavePath(saveIndex);

        RoundtripJson(profilePath);
        RoundtripJson(savePath);
    }

    private static void RoundtripJson(string path)
    {
        Console.WriteLine($"Parsing {path}...");
        SaveFile sf = SaveFile.Read(path);

        string targetJsonPath = Path.GetFileName(Path.ChangeExtension(path, "json"));
        Console.WriteLine($"Writing json to {targetJsonPath}...");
        JsonReadWrite.ToJson(targetJsonPath, sf.SaveData);

        Console.WriteLine("Writing original data to memory blob...");
        Writer w = new();
        sf.SaveData.Write(w);
        byte[] original = w.ToArray();

        Console.WriteLine($"Reading json from {targetJsonPath}...");
        SaveData sd = JsonReadWrite.FromJson(targetJsonPath);

        Console.WriteLine("Writing round-tripped data to memory blob...");
        w = new();
        sd.Write(w);
        byte[] roundTripped = w.ToArray();

        Console.WriteLine(original.SequenceEqual(roundTripped)
            ? "Written and read data is the same"
            : "Written and read data is different");
    }
}

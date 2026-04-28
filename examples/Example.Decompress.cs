using lib.remnant2.saves.Compression;

namespace examples;

internal partial class Example
{
    public static void Decompress()
    {
        Console.WriteLine("Decompress===========");
        string folder = Utils.GetSteamSavePath();
        string profilePath = Path.Combine(folder, "profile.sav");
        string savePath = Path.Combine(folder, Environment.GetEnvironmentVariable("DEBUG_REMNANT_SAVE") ?? "save_0.sav");

        byte[] b = Archive.DecompressSave(savePath);
        string outPath = "output.bin";
        File.WriteAllBytes(outPath, b);
    }
}
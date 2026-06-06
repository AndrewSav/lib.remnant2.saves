using lib.remnant2.saves.Compression;

namespace examples;

internal partial class Example
{
    public static void Decompress()
    {
        Console.WriteLine("Decompress===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // character / save slot (or DEBUG_REMNANT_SAVE_INDEX env var)
        // ========================

        string savePath = Utils.GetWorldSavePath(saveIndex);

        byte[] b = Archive.DecompressSave(savePath);
        string outPath = "output.bin";
        File.WriteAllBytes(outPath, b);
    }
}
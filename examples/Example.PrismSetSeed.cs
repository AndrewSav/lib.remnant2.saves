using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

// One of the prism-editing examples. For the character-index convention, the prism profile IDs,
// the UI-name => save RowName mappings (fragments, fusions and legendaries) and the level/value conversions,
// see the reference comment in Example.PrismAddRollChance.cs.
internal partial class Example
{
    // Set the prism's roll seed (CurrentSeed). Random by default (so the next roll comes out
    // different); set a fixed value for a specific, repeatable roll.
    public static void PrismSetSeed()
    {
        Console.WriteLine("Set Prism Seed===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // character / save slot (or DEBUG_REMNANT_SAVE_INDEX env var)
        const string prismProfileId = "/Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfVoracity.PrismOfVoracity_C";
        const string targetFileName = "prism_seed_set.sav";
        // A fresh random seed gives a different roll each run. For a SPECIFIC, repeatable roll,
        // replace this with a fixed value, e.g.  int seed = 123456789;
        int seed = new Random().Next(int.MinValue, int.MaxValue);
        // ========================

        string path = Path.Combine(Utils.GetSteamSavePath(), "profile.sav");
        Console.WriteLine($"Reading {path}");
        SaveFile sf = SaveFile.Read(path);

        // Locate the prism's PrismStoneInstanceData property bag.
        Navigator navigator = new(sf);
        List<ObjectProperty> characters = navigator.GetProperty("Characters")!.GetItems<ObjectProperty>();
        Property? prismItemBp = navigator.GetProperties("ItemBP", characters[saveIndex].Object)
            .FirstOrDefault(x => x.Value!.ToString() == prismProfileId);
        if (prismItemBp == null)
        {
            Console.WriteLine($"WARNING: prism not found on save_{saveIndex}.");
            return;
        }
        PropertyBag instance = prismItemBp.GetParent<PropertyBag>(navigator)["InstanceData"].Get<ObjectProperty>().Object!.Properties!;

        if (!instance.Contains("CurrentSeed"))
        {
            Console.WriteLine("WARNING: this prism has no CurrentSeed - nothing changed.");
            return;
        }
        Console.WriteLine($"Changing CurrentSeed from {instance["CurrentSeed"].Value} to {seed}.");
        instance["CurrentSeed"].Value = seed;

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"Copy {targetFileName} over your profile.sav to apply.");
    }
}

using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

// One of the prism-editing examples. For the character-index convention, the prism profile IDs,
// the UI-name => save RowName mappings (fragments, fusions and legendaries) and the level/value conversions,
// see the reference comment in Example.PrismAddRollChance.cs.
internal partial class Example
{
    // Remove a roll chance (a fed fragment) from the prism's ROLL CHANCES list.
    // Prints a warning and does nothing else if that roll chance is not present.
    public static void PrismRemoveRollChance()
    {
        Console.WriteLine("Remove Prism Roll Chance===========");

        // ===== CHANGE THESE =====
        const int characterIndex = 0;                  // save_0 (first character)
        const string prismProfileId = "/Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfVoracity.PrismOfVoracity_C";
        const string fragmentRowName = "ModDuration";  // fed fragment to remove (single RowName)
        const string targetFileName = "prism_roll_chance_removed.sav";
        // ========================

        string path = Path.Combine(Utils.GetSteamSavePath(), "profile.sav");
        Console.WriteLine($"Reading {path}");
        SaveFile sf = SaveFile.Read(path);

        // Locate the prism's PrismStoneInstanceData property bag.
        Navigator navigator = new(sf);
        List<ObjectProperty> characters = navigator.GetProperty("Characters")!.GetItems<ObjectProperty>();
        if (characterIndex >= characters.Count || characters[characterIndex].Object == null)
        {
            Console.WriteLine($"WARNING: character index {characterIndex} (save_{characterIndex}) not found.");
            return;
        }
        Property? prismItemBp = navigator.GetProperties("ItemBP", characters[characterIndex].Object)
            .FirstOrDefault(x => x.Value!.ToString() == prismProfileId);
        if (prismItemBp == null)
        {
            Console.WriteLine($"WARNING: prism not found on save_{characterIndex}.");
            return;
        }
        PropertyBag instance = prismItemBp.GetParent<PropertyBag>(navigator)["InstanceData"].Get<ObjectProperty>().Object!.Properties!;

        // Remove the roll chance if present.
        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!instance.Contains("CurrentFeedData") || instance["CurrentFeedData"].Value is not ArrayStructProperty feed
            || !feed.Items.Cast<PropertyBag>().Any(x => x["RowName"].ToStringValue() == fragmentRowName))
        {
            Console.WriteLine($"WARNING: roll chance '{fragmentRowName}' is not in this prism - nothing removed.");
            return;
        }
        feed.Items.RemoveAll(x => ((PropertyBag)x!)["RowName"].ToStringValue() == fragmentRowName);
        Console.WriteLine($"Removed roll chance '{fragmentRowName}'.");

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"Copy {targetFileName} over your profile.sav to apply.");
    }
}

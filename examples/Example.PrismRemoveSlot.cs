using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

// One of the prism-editing examples. For the character-index convention, the prism profile IDs,
// the UI-name => save RowName mappings (fragments, fusions and legendaries) and the level/value conversions,
// see the reference comment in Example.PrismAddRollChance.cs.
internal partial class Example
{
    // Remove a slot (a leveled-up fragment or fusion) from the prism's PRISM block.
    // Prints a warning and does nothing else if that slot is not present.
    public static void PrismRemoveSlot()
    {
        Console.WriteLine("Remove Prism Slot===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // character / save slot (or DEBUG_REMNANT_SAVE_INDEX env var)
        const string prismProfileId = "/Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfVoracity.PrismOfVoracity_C";
        const string slotRowName = "ArmorPercent";     // slot to remove (single OR fusion RowName)
        const string targetFileName = "prism_slot_removed.sav";
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

        // Remove the slot if present.
        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!instance.Contains("CurrentSegments") || instance["CurrentSegments"].Value is not ArrayStructProperty segments
            || !segments.Items.Cast<PropertyBag>().Any(x => x["RowName"].ToStringValue() == slotRowName))
        {
            Console.WriteLine($"WARNING: slot '{slotRowName}' is not in this prism - nothing removed.");
            return;
        }
        segments.Items.RemoveAll(x => ((PropertyBag)x!)["RowName"].ToStringValue() == slotRowName);
        Console.WriteLine($"Removed slot '{slotRowName}'.");

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"Copy {targetFileName} over your profile.sav to apply.");
    }
}

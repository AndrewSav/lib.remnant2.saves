using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

// One of the prism-editing examples. For the character-index convention, the prism profile IDs,
// the UI-name => save RowName mappings (fragments, fusions and legendaries) and the level/value conversions,
// see the reference comment in Example.PrismAddRollChance.cs.
internal partial class Example
{
    // Set the prism's stored pending XP (the ring around the level number). Updates it in place if the
    // prism already has a PendingExperience, or creates the field from scratch on a never-fed/leveled prism.
    public static void PrismSetExperience()
    {
        Console.WriteLine("Set Prism Experience===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // character / save slot (or DEBUG_REMNANT_SAVE_INDEX env var)
        const string prismProfileId = "/Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfVoracity.PrismOfVoracity_C";
        const float experience = 600000f;              // new pending XP
        const string targetFileName = "prism_xp_set.sav";
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

        if (instance.Contains("PendingExperience"))
        {
            Console.WriteLine($"Setting PendingExperience from {instance["PendingExperience"].Value} to {experience}.");
            instance["PendingExperience"].Value = experience;
        }
        else
        {
            // Pristine prism (never fed/leveled) has no PendingExperience yet — create it from scratch.
            // The enclosing SaveData owns the names table the new property's FNames resolve/append into.
            SaveData saveData = prismItemBp.EnclosingSaveData(navigator);
            instance.Properties.Add(new("PendingExperience", new Property
            {
                Name = saveData.MakeFName("PendingExperience"),
                Type = saveData.MakeFName("FloatProperty"),
                Index = 0, Size = 4, HasPropertyGuid = 0, PropertyGuid = null,
                Value = experience
            }));
            instance.RefreshLookup();
            Console.WriteLine($"Created PendingExperience = {experience} (prism was never fed/leveled).");
        }

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"Copy {targetFileName} over your profile.sav to apply.");
    }
}
    
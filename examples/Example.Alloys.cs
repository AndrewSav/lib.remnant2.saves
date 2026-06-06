using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

internal partial class Example
{
    // Set the three DLC3 special alloys (Alien, Mythril, Ancient) to 100 on the selected character slot. If an
    // alloy is missing it is built from scratch (see AddInventoryItem in Example.InventoryHelpers.cs) -
    // no item is cloned. (Example.AddRing / Example.MaxShards show the clone approach instead.)
    public static void Alloys()
    {
        Console.WriteLine("Alloys===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // character / save slot (or DEBUG_REMNANT_SAVE_INDEX env var)
        const string alienId = "/Game/World_DLC3/Items/Special/AlienAlloy/Material_AlienAlloy.Material_AlienAlloy_C";
        const string mythrilId = "/Game/World_DLC3/Items/Special/MythrilAlloy/Material_MythrilAlloy.Material_MythrilAlloy_C";
        const string ancientId = "/Game/World_DLC3/Items/Special/AncientAlloy/Material_AncientAlloy.Material_AncientAlloy_C";
        const string targetFileName = "alloys.sav";
        // ========================

        string folder = Utils.GetSteamSavePath();
        string path = Path.Combine(folder, "profile.sav");

        Console.WriteLine("Reading profile data...");
        SaveFile sf = SaveFile.Read(path);
        Navigator navigator = new(sf);

        ObjectProperty character = navigator.GetProperty("Characters")!.GetItems<ObjectProperty>()[saveIndex];

        SetAlloy(navigator, character, alienId, 100);
        SetAlloy(navigator, character, mythrilId, 100);
        SetAlloy(navigator, character, ancientId, 100);

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"You have to copy {targetFileName} over your profile.sav in '{folder}' folder!");
    }

    // Set one alloy's stack to `value`: update the quantity if the character already has it, otherwise
    // build the item from scratch (alloys are materials, so InstanceData type "ItemInstanceData").
    private static void SetAlloy(Navigator navigator, ObjectProperty character, string alloyId, int value)
    {
        Property? alloyItem = navigator.GetProperties("ItemBP", character.Object)
            .SingleOrDefault(x => x.Value!.ToString() == alloyId);

        if (alloyItem != null)
        {
            PropertyBag instance = alloyItem.GetParent<PropertyBag>(navigator)["InstanceData"]
                .Get<ObjectProperty>().Object!.Properties!;
            Console.WriteLine($"Updating {Utils.GetShortenedAssetPath(alloyId)}: quantity {instance["Quantity"].Value} -> {value}");
            instance["Quantity"].Value = value;
        }
        else
        {
            Console.WriteLine($"Adding {Utils.GetShortenedAssetPath(alloyId)} from scratch at quantity {value}");
            AddInventoryItem(navigator, character, alloyId, "ItemInstanceData", value);
        }
    }
}

using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

internal partial class Example
{
    // From-scratch counterpart to Example.MaxShards: instead of shallow-cloning an existing material (the
    // clone version copies a Scrap item), it builds the shard's item + InstanceData objects from nothing via
    // the shared AddInventoryItem helper (see Example.InventoryHelpers.cs). Corrupted Shards are a material,
    // so the InstanceData type is "ItemInstanceData" and Quantity is a real stack count (10, the in-game
    // max). If the character already has shards we just bump the existing stack to 10.
    public static void MaxShardsFromScratch()
    {
        Console.WriteLine("Max Shards (from scratch)===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // character / save slot (or DEBUG_REMNANT_SAVE_INDEX env var)
        const string shardId = "/Game/World_Base/Items/Materials/LumeniteCrystal/Material_CorruptedShard.Material_CorruptedShard_C";
        const string targetFileName = "shards_maxed_from_scratch.sav";
        // ========================

        string folder = Utils.GetSteamSavePath();
        string path = Path.Combine(folder, "profile.sav");

        Console.WriteLine("Reading profile data...");
        SaveFile sf = SaveFile.Read(path);
        Navigator navigator = new(sf);

        Console.WriteLine($"Looking for existing shards on character slot {saveIndex}...");
        ObjectProperty character = navigator.GetProperty("Characters")!.GetItems<ObjectProperty>()[saveIndex];

        Property? shardItem = navigator.GetProperties("ItemBP", character.Object)
            .SingleOrDefault(x => x.Value!.ToString() == shardId);

        if (shardItem != null)
        {
            PropertyBag instance = shardItem.GetParent<PropertyBag>(navigator)["InstanceData"]
                .Get<ObjectProperty>().Object!.Properties!;
            Console.WriteLine($"Your character has shards, updating quantity from {instance["Quantity"].Value} to 10");
            instance["Quantity"].Value = 10;
        }
        else
        {
            Console.WriteLine("Your character does not have shards, adding from scratch...");
            AddInventoryItem(navigator, character, shardId, "ItemInstanceData", 10);
        }

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"You have to copy {targetFileName} over your profile.sav in '{folder}' folder!");
    }
}

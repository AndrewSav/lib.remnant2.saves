using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

internal partial class Example
{
    // From-scratch counterpart to Example.AddRing: instead of shallow-cloning an existing ring, it builds
    // the ring's item + InstanceData objects from nothing via the shared AddInventoryItem helper (see
    // Example.InventoryHelpers.cs). A ring is equipment, so the InstanceData type is "EquipmentInstanceData"
    // with NO Quantity - real-save equipment carries an empty property bag, so we pass null (materials carry
    // a real stack count instead - compare Example.MaxShardsFromScratch). Unlike the clone version, no
    // existing ring is needed to copy from.
    public static void AddRingFromScratch()
    {
        Console.WriteLine("Add Ring (from scratch)===========");

        const string ringId = "/Game/World_Jungle/Items/Trinkets/Rings/ArchersCrest/Ring_ArchersCrest.Ring_ArchersCrest_C";
        const string targetFileName = "ring_added_from_scratch.sav";

        string folder = Utils.GetSteamSavePath();
        string path = Path.Combine(folder, "profile.sav");

        Console.WriteLine("Reading profile data...");
        SaveFile sf = SaveFile.Read(path);
        Navigator navigator = new(sf);

        Console.WriteLine("Looking for the ring on the first character...");
        ObjectProperty? character = navigator.GetProperty("Characters")!.GetItems<ObjectProperty>().FirstOrDefault();
        if (character == null)
        {
            Console.WriteLine("Do you have any characters?");
            return;
        }

        Property? ringItem = navigator.GetProperties("ItemBP", character.Object)
            .SingleOrDefault(x => x.Value!.ToString() == ringId);
        if (ringItem != null)
        {
            Console.WriteLine("Your character already has the ring");
            return;
        }

        Console.WriteLine("Adding the ring from scratch...");
        AddInventoryItem(navigator, character, ringId, "EquipmentInstanceData", null);

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"You have to copy {targetFileName} over your profile.sav in the game save folder!");
    }
}

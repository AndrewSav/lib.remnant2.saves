using rd2parser.Model.Properties;
using rd2parser.Model;

namespace rd2parser.examples;

internal partial class Example
{
    public static void AddRing()
    {
        Console.WriteLine("Add Ring===========");

        const string ringId = "/Game/World_Base/Items/Trinkets/Rings/CompulsionLoop/Ring_CompulsionLoop.Ring_CompulsionLoop_C";
        const string targetFileName = "ring_added.sav";

        string folder = Utils.GetSteamSavePath();
        string path = Path.Combine(folder, "profile.sav");

        Console.WriteLine("Reading profile data...");
        SaveFile sf = SaveFile.Read(path);

        Console.WriteLine("Looking for the ring on the first character...");

        Property? characters = sf.GetProperty("Characters");

        ObjectProperty? character = characters!.GetItems<ObjectProperty>().FirstOrDefault();
        if (character == null)
        {
            Console.WriteLine("Do you have any characters?");
            return;
        }

        Property? ringItem = sf.GetProperties("ItemBP", $"UObject={character.ObjectIndex}")!
            .SingleOrDefault(x => x.Value!.ToString() == ringId);

        if (ringItem != null)
        {
            Console.WriteLine("Your character already has the ring");
            return;
        }

        ringItem = sf.GetProperties("ItemBP", $"UObject={character.ObjectIndex}")!
            .FirstOrDefault(x => x.Value!.ToString()!.Contains("Ring_"));

        if (ringItem == null)
        {
            Console.WriteLine("Your character does not have any rings, we need an existing ring to clone from");
            return;
        }

        UObject itemObject = (ringItem.Value as ObjectProperty)!.Object!.CopyObject();
        UObject instanceDataObject = (ringItem.Bag["InstanceData"].Value as ObjectProperty)!.Object!.CopyObject();

        itemObject.SaveData.Objects.Add(itemObject);
        itemObject.Path[^1].Index = itemObject.ObjectIndex;
        itemObject.ObjectPath = ringId;

        instanceDataObject.SaveData.Objects.Add(instanceDataObject);
        instanceDataObject.Path[^1].Index = instanceDataObject.ObjectIndex;

        var newItemBag = ringItem.Bag.CopyPropertyBag();
        (newItemBag["ItemBP"].Value as ObjectProperty)!.SetObject(itemObject);
        (newItemBag["InstanceData"].Value as ObjectProperty)!.SetObject(instanceDataObject);
        newItemBag["New"].Value = 1; // Just for in-game display
        newItemBag["EquipmentSlotIndex"].Value = -1; // In case ring we cloned from is equipped

        ArrayStructProperty inventoryArray = (ArrayStructProperty)ringItem.Bag.Parent!;
        int maxId = inventoryArray.Items.Select(x => (int)((PropertyBag)x!)["ID"].Value!).Max();
        newItemBag["ID"].Value = maxId+1;

        inventoryArray.Items.Add(newItemBag);

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"You have to copy {targetFileName} over your profile.sav in the game save folder!");
    }
}

using rd2parser.Model;
using rd2parser.Model.Properties;

namespace rd2parser.examples;

internal partial class Example
{
    public static void EditScrap()
    {
        Console.WriteLine("Edit Scrap===========");

        const string scrapId = "/Game/World_Base/Items/Materials/Scraps/Material_Scraps.Material_Scraps_C";
        const int targetScrapValue = 12345;
        const string targetFileName = "scrap_changed.sav";

        string folder = Utils.GetSteamSavePath();
        string path = Path.Combine(folder, "profile.sav");

        Console.WriteLine("Reading profile data...");
        SaveFile sf = SaveFile.Read(path);

        Console.WriteLine("Looking for scrap on the first character...");

        Property? characters = sf.GetProperty("Characters");

        ObjectProperty? character = characters!.GetItems<ObjectProperty>().FirstOrDefault();
        if (character == null)
        {
            Console.WriteLine("Do you have any characters?");
            return;
        }

        Property scrapItem = sf.GetProperties("ItemBP", $"UObject={character.ObjectIndex}")!
            .Single(x => x.Value!.ToString() == scrapId);

        int instanceDataIndex = (scrapItem.Bag["InstanceData"].Value as ObjectProperty)!.ObjectIndex;

        Property scrap = sf.GetProperty("Quantity",
            $"UObject={character.ObjectIndex},UObject=ItemInstanceData:{instanceDataIndex}")!;
        Console.WriteLine($"Current scrap value is {scrap.Value}. Changing to {targetScrapValue}...");
        scrap.Value = targetScrapValue;

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"You have to copy {targetFileName} over your profile.sav in the game save folder!");
    }
}

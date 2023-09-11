using rd2parser.Model.Properties;
using rd2parser.Model;

namespace rd2parser.examples;

internal partial class Example
{
    public static void EditScrap()
    {
        Console.WriteLine("Edit Scrap===========");

        int targetSrcapValue = 12345;
        string targetFileName = "scrap_changed.sav";

        string folder = Utils.GetSteamSavePath();
        string path = Path.Combine(folder, "profile.sav");

        Console.WriteLine("Reading profile data...");
        SaveFile sf = SaveFile.Read(path);

        Console.WriteLine("Looking for scrap on the first character...");
        KeyValuePair<string, Property> charactersProp = sf.SaveData.Objects[0].Properties!.Single(x => x.Key == "Characters");

        ArrayProperty ap = (charactersProp.Value.Value as ArrayProperty)!;
        List<ObjectProperty> op = ap.Items.Select(x => (ObjectProperty)x!).ToList();
        var character = op.FirstOrDefault(x => x.Object != null);
        if (character == null)
        {
            Console.WriteLine("Do you have any characters?");
            return;
        }

        Property characterData = character.Object!.Properties!.SingleOrDefault(x => x.Key == "CharacterData").Value;
        StructProperty sp = (StructProperty)characterData.Value!;
        SaveData inner = (SaveData)sp.Value!;
        UObject master = inner.Objects.Single(x => x.Name == "Character_Master_Player_C");
        Component inventory = master.Components!.Single(x => x.ComponentKey == "Inventory");
        Property items = inventory.Properties!.Single(x => x.Key == "Items").Value;
        ArrayStructProperty asp = (ArrayStructProperty)items.Value!;

        string scrapId = "/Game/World_Base/Items/Materials/Scraps/Material_Scraps.Material_Scraps_C";

        List<KeyValuePair<string, Property>> scrapInventory = (List<KeyValuePair<string, Property>>)asp.Items.Single(x =>
        {
            return ((List<KeyValuePair<string, Property>>)x!).Any(y => 
                y.Key == "ItemBP" 
                &&((ObjectProperty)y.Value.Value!).ClassName == scrapId
                );
        })!;

        ObjectProperty instanceData = (ObjectProperty)scrapInventory.Single(x => x.Key == "InstanceData").Value.Value!;
        Property scrap = instanceData.Object!.Properties!.Single(x=>x.Key == "Quantity").Value;

        Console.WriteLine($"Current scrap value is {scrap.Value}. Changing to {targetSrcapValue}...");
        scrap.Value = targetSrcapValue;
        
        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName,sf);
        Console.WriteLine($"You have to copy {targetFileName} over your profile.sav in the game save folder!");
    }
}

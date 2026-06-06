using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;

namespace examples;

internal partial class Example
{
    // Same as EditCurrency (update an existing currency's Quantity) but WITHOUT the Navigator - manual
    // object traversal. More verbose, but it skips building the Navigator graph, so it does less work.
    // Errors if the character doesn't have the currency. See the variant guide + currency-id list in
    // Example.EditCurrency.cs.
    public static void EditCurrencyRaw()
    {
        Console.WriteLine("Edit Currency Raw API===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // character / save slot (or DEBUG_REMNANT_SAVE_INDEX env var)
        const string currencyId = "/Game/World_Base/Items/Materials/Scraps/Material_Scraps.Material_Scraps_C";  // paste one from the list in EditCurrency
        const int quantity = 12345;                    // new amount
        const string targetFileName = "currency_changed_raw.sav";
        // ========================

        string folder = Utils.GetSteamSavePath();
        string path = Path.Combine(folder, "profile.sav");

        Console.WriteLine("Reading profile data...");
        SaveFile sf = SaveFile.Read(path);

        Console.WriteLine($"Looking for the currency on character slot {saveIndex}...");
        KeyValuePair<string, Property> charactersProp = sf.SaveData.Objects[0].Properties!.Properties.Single(x => x.Key == "Characters");

        ArrayProperty ap = (charactersProp.Value.Value as ArrayProperty)!;
        List<ObjectProperty> op = ap.Items.Select(x => (ObjectProperty)x!).ToList();
        ObjectProperty character = op[saveIndex];

        Property characterData = sf.SaveData.Objects[character.ObjectIndex].Properties!.Properties.SingleOrDefault(x => x.Key == "CharacterData").Value;
        StructProperty sp = (StructProperty)characterData.Value!;
        SaveData inner = (SaveData)sp.Value!;
        UObject master = inner.Objects.Single(x => x.Name == "Character_Master_Player_C");
        Component inventory = master.Components!.Single(x => x.ComponentKey == "Inventory");
        Property items = inventory.Properties!.Properties.Single(x => x.Key == "Items").Value;
        ArrayStructProperty asp = (ArrayStructProperty)items.Value!;

        PropertyBag currencyInventory = (PropertyBag)asp.Items.Single(x =>
        {
            return ((PropertyBag)x!).Properties.Any(y =>
                y.Key == "ItemBP"
                && inner.Objects[((ObjectProperty)y.Value.Value!).ObjectIndex].ObjectPath == currencyId
                );
        })!;

        ObjectProperty instanceData = (ObjectProperty)currencyInventory.Properties.Single(x => x.Key == "InstanceData").Value.Value!;
        Property quantityProp = inner.Objects[instanceData.ObjectIndex].Properties!.Properties.Single(x => x.Key == "Quantity").Value;

        Console.WriteLine($"Current quantity is {quantityProp.Value}. Changing to {quantity}...");
        quantityProp.Value = quantity;

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"You have to copy {targetFileName} over your profile.sav in the game save folder!");
    }
}

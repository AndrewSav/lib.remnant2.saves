using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

internal partial class Example
{
    // Set a currency's Quantity, CREATING it if the character doesn't have it - by shallow-cloning an item
    // they DO have (the donor) and repointing the copy at the target currency. Compare EditCurrencyFromScratch,
    // which builds the item from nothing (no donor needed); see Example.AddRing for the same clone technique
    // on equipment. See the variant guide + currency-id list in Example.EditCurrency.cs.
    public static void EditCurrencyClone()
    {
        Console.WriteLine("Edit Currency (clone)===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // character / save slot (or DEBUG_REMNANT_SAVE_INDEX env var)
        const string currencyId = "/Game/World_Base/Items/Materials/LumeniteCrystal/Material_CorruptedShard.Material_CorruptedShard_C";  // paste one from the list in EditCurrency
        const string donorId = "/Game/World_Base/Items/Materials/Scraps/Material_Scraps.Material_Scraps_C";  // a currency you already own, to clone from (Scrap is a safe default)
        const int quantity = 10;                       // new amount
        const string targetFileName = "currency_cloned.sav";
        // ========================

        string folder = Utils.GetSteamSavePath();
        string path = Path.Combine(folder, "profile.sav");

        Console.WriteLine("Reading profile data...");
        SaveFile sf = SaveFile.Read(path);
        Navigator navigator = new(sf);

        Console.WriteLine($"Looking for the currency on character slot {saveIndex}...");
        ObjectProperty character = navigator.GetProperty("Characters")!.GetItems<ObjectProperty>()[saveIndex];

        Property? currencyItem = navigator.GetProperties("ItemBP", character.Object)
            .SingleOrDefault(x => x.Value!.ToString() == currencyId);

        if (currencyItem != null)
        {
            Console.WriteLine("Your character has the currency");
            var f = navigator.Lookup(currencyItem);
            Property pp = navigator.GetProperty("InstanceData", f.Parent!.Object)!.Get<ObjectProperty>().Object!.Properties!["Quantity"];
            Console.WriteLine($"Updating quantity from {pp.Value} to {quantity}");
            pp.Value = quantity;
        }
        else
        {
            Console.WriteLine("Your character does not have the currency, adding by cloning the donor");

            Property donorItem = navigator.GetProperties("ItemBP", character.Object)
                .Single(x => x.Value!.ToString() == donorId);

            UObject itemObject = (donorItem.Value as ObjectProperty)!.Object!.ShallowCopyObject(navigator);
            itemObject.ObjectPath = currencyId;

            UObject instanceDataObject = (donorItem.GetParent<PropertyBag>(navigator)["InstanceData"].Value as ObjectProperty)!.Object!.ShallowCopyObject(navigator);
            instanceDataObject.LoadedData = instanceDataObject.LoadedData!.ShallowCopyLoadedData();
            instanceDataObject.Properties = new PropertyBag
            {
                Properties = instanceDataObject.Properties!.Properties
                    .Select(x => new KeyValuePair<string, Property>(x.Key, x.Value.ShallowCopyProperty())).ToList()
            };
            instanceDataObject.Properties.RefreshLookup();
            instanceDataObject.Properties["Quantity"].Value = quantity;

            PropertyBag newItemBag = new()
            {
                Properties = donorItem.GetParent<PropertyBag>(navigator).Properties
                    .Select(x => new KeyValuePair<string, Property>(x.Key, x.Value.ShallowCopyProperty())).ToList(),
            };
            newItemBag.RefreshLookup();
            newItemBag["ItemBP"].Value = new ObjectProperty { ObjectIndex = itemObject.ObjectIndex };
            newItemBag["InstanceData"].Value = new ObjectProperty { ObjectIndex = instanceDataObject.ObjectIndex };
            newItemBag["New"].Value = 1; // Just for in-game display

            ArrayStructProperty inventoryArray = donorItem.GetParent<PropertyBag>(navigator).GetParent<ArrayStructProperty>(navigator);
            newItemBag["ID"].Value = NextInventoryId(navigator, inventoryArray);

            inventoryArray.Items.Add(newItemBag);
        }

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"You have to copy {targetFileName} over your profile.sav in '{folder}' folder!");
    }
}

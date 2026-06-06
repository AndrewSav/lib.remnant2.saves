using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

internal partial class Example
{
    // The most versatile (and most complex) currency edit: set a currency's Quantity, CREATING it from
    // nothing via the shared AddInventoryItem helper (see InventoryHelpers.cs) if the character doesn't have
    // it - no donor item needed (compare EditCurrencyClone, which copies one you already own). Currencies are
    // materials, so the InstanceData type is "ItemInstanceData" and Quantity is a real stack count.
    // See the variant guide + currency-id list in Example.EditCurrency.cs.
    public static void EditCurrencyFromScratch()
    {
        Console.WriteLine("Edit Currency (from scratch)===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // character / save slot (or DEBUG_REMNANT_SAVE_INDEX env var)
        const string currencyId = "/Game/World_Base/Items/Materials/LumeniteCrystal/Material_CorruptedShard.Material_CorruptedShard_C";  // paste one from the list in EditCurrency
        const int quantity = 10;                       // new amount
        const string targetFileName = "currency_from_scratch.sav";
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
            PropertyBag instance = currencyItem.GetParent<PropertyBag>(navigator)["InstanceData"]
                .Get<ObjectProperty>().Object!.Properties!;
            Console.WriteLine($"Your character has the currency, updating quantity from {instance["Quantity"].Value} to {quantity}");
            instance["Quantity"].Value = quantity;
        }
        else
        {
            Console.WriteLine("Your character does not have the currency, adding from scratch...");
            AddInventoryItem(navigator, character, currencyId, "ItemInstanceData", quantity);
        }

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"You have to copy {targetFileName} over your profile.sav in '{folder}' folder!");
    }
}

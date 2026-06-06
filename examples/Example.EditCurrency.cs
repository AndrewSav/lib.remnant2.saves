using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

// ============================================================================================
//  CURRENCY EDITING EXAMPLES
// ============================================================================================
//  Four ways to set a currency's amount - pick by need:
//    EditCurrency             update an existing stack, via the Navigator     - simplest
//    EditCurrencyRaw          update an existing stack, no Navigator          - faster, more verbose
//    EditCurrencyClone        create it (if missing) by cloning an item       - handles a missing currency
//    EditCurrencyFromScratch  create it (if missing) by building from nothing - most versatile, most complex
//
//  EditCurrency / EditCurrencyRaw assume you already own some of the currency (they error otherwise);
//  use Clone or FromScratch if it might be absent. EditCurrencyRaw skips the Navigator, so it does less
//  work / runs faster - whether that matters is up to your use case.
//
// --------------------------------------------------------------------------------------------
//  CURRENCY IDS  (paste one into currencyId)
// --------------------------------------------------------------------------------------------
//    Scrap                       /Game/World_Base/Items/Materials/Scraps/Material_Scraps.Material_Scraps_C
//    Lumenite Crystal            /Game/World_Base/Items/Materials/LumeniteCrystal/Material_LumeniteCrystal.Material_LumeniteCrystal_C
//    Corrupted Lumenite Crystal  /Game/World_Base/Items/Materials/LumeniteCrystal/Material_LumeniteCorrupted.Material_LumeniteCorrupted_C
//    Corrupted Shard             /Game/World_Base/Items/Materials/LumeniteCrystal/Material_CorruptedShard.Material_CorruptedShard_C
//                                  (capped at 10 in inventory; setting higher may not produce a valid save)
//    Relic Dust                  /Game/World_Base/Items/Materials/GemFragments/Material_RelicDust.Material_RelicDust_C
//    Simulacrum                  /Game/World_Base/Items/Materials/Simulacrum/Material_Simulacrum.Material_Simulacrum_C
//    Iron                        /Game/World_Base/Items/Materials/Irons/01_Iron/Material_Iron.Material_Iron_C
//    Forged Iron                 /Game/World_Base/Items/Materials/Irons/02_ForgedIron/Material_ForgedIron.Material_ForgedIron_C
//    Galvanized Iron             /Game/World_Base/Items/Materials/Irons/03_GalvanizedIron/Material_GalvanizedIron.Material_GalvanizedIron_C
//    Hardened Iron               /Game/World_Base/Items/Materials/Irons/04_HardenedIron/Material_HardenedIron.Material_HardenedIron_C
//    Blood Moon Essence          /Game/World_Jungle/Items/Materials/World/BloodMoonEssence/Material_BloodMoonEssence.Material_BloodMoonEssence_C
//    Alien Alloy   (DLC3)        /Game/World_DLC3/Items/Special/AlienAlloy/Material_AlienAlloy.Material_AlienAlloy_C
//    Mythril Alloy (DLC3)        /Game/World_DLC3/Items/Special/MythrilAlloy/Material_MythrilAlloy.Material_MythrilAlloy_C
//    Ancient Alloy (DLC3)        /Game/World_DLC3/Items/Special/AncientAlloy/Material_AncientAlloy.Material_AncientAlloy_C
// ============================================================================================
internal partial class Example
{
    // Simplest currency edit: update the Quantity of a currency you ALREADY own, via the Navigator.
    // Errors if the character doesn't have the currency - use EditCurrencyClone or EditCurrencyFromScratch
    // to create it. See the variant guide + currency-id list above.
    public static void EditCurrency()
    {
        Console.WriteLine("Edit Currency===========");

        // ===== CHANGE THESE =====
        int saveIndex = Utils.GetSaveIndex();          // character / save slot (or DEBUG_REMNANT_SAVE_INDEX env var)
        const string currencyId = "/Game/World_Base/Items/Materials/Scraps/Material_Scraps.Material_Scraps_C";  // paste one from the list above
        const int quantity = 12345;                    // new amount
        const string targetFileName = "currency_changed.sav";
        // ========================

        string folder = Utils.GetSteamSavePath();
        string path = Path.Combine(folder, "profile.sav");

        Console.WriteLine("Reading profile data...");
        SaveFile sf = SaveFile.Read(path);
        Navigator navigator = new(sf);

        Console.WriteLine($"Looking for the currency on character slot {saveIndex}...");
        ObjectProperty character = navigator.GetProperty("Characters")!.GetItems<ObjectProperty>()[saveIndex];

        Property currencyItem = navigator.GetProperties("ItemBP", character.Object)
            .Single(x => x.Value is ObjectProperty { ClassName: currencyId });

        UObject instanceData = (currencyItem.GetParent<PropertyBag>(navigator)["InstanceData"].Value as ObjectProperty)!.Object!;

        Property quantityProp = navigator.GetProperty("Quantity", instanceData)!;

        Console.WriteLine($"Current quantity is {quantityProp.Value}. Changing to {quantity}...");
        quantityProp.Value = quantity;

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"You have to copy {targetFileName} over your profile.sav in the game save folder!");
    }
}

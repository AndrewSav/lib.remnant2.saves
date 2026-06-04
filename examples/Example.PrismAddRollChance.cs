using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Parts;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

//  --------------------------------------------------------------------------------------------
//  PRISM PROFILE IDS  (paste one into prismProfileId)
//  --------------------------------------------------------------------------------------------
//    Prism of Greed     /Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfGreed.PrismOfGreed_C
//    Prism of Hatred    /Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfHatred.PrismOfHatred_C
//    Prism of Jealousy  /Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfJealousy.PrismOfJealousy_C
//    Prism of Lethargy  /Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfLethargy.PrismOfLethargy_C
//    Prism of Passion   /Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfPassion.PrismOfPassion_C
//    Prism of Pride     /Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfPride.PrismOfPride_C
//    Prism of Voracity  /Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfVoracity.PrismOfVoracity_C
//
//  --------------------------------------------------------------------------------------------
//  UI NAME  =>  SAVE RowName   (single fragments)
//  Used by BOTH the PRISM block (slots) and the ROLL CHANCES (fed fragments).
//  --------------------------------------------------------------------------------------------
//    Critical Damage          CriticalDamage          Health Bonus             HealthPercent
//    Weakspot Damage          WeakspotDamage          Health                   HealthFlat
//    Ranged Damage            RangedDamage            Health Regeneration      HealthRegen
//    Melee Damage             MeleeDamage             Healing Effectiveness    HealingEfficacy
//    Mod Damage               ModDamage               Grey Health Conversion   GreyHealthRate
//    Skill Damage             SkillDamage             Stamina Bonus            StaminaPercent
//    Explosive Damage         ExplosiveDamage         Stamina                  StaminaFlat
//    Status Effect Damage     StatusDamage            Damage Reduction         DamageReduction
//    Ranged Critical Chance   RangedCriticalChance    Armor Bonus              ArmorPercent
//    Melee Critical Chance    MeleeCriticalChance     Armor                    ArmorFlat
//    Mod Critical Chance      ModCriticalChance       Shield Amount            ShieldPercent
//    Skill Critical Chance    SkillCriticalChance     Shield Duration          ShieldDuration
//    Fire Rate                RangedFireRate          Evade Speed              EvadeSpeed
//    Firearm Charge Time      FirearmChargeSpeed      Evade Distance           EvadeDistance
//    Total Melee Speed        MeleeAttackSpeed        Revive Speed             ReviveSpeed
//    Reload Speed             ReloadSpeed             Movement Speed           MovementSpeed
//    Firearm Swap Speed       SwapSpeed               Weapon Spread            WeaponSpread
//    Ammo Reserves            AmmoReserves            Weapon Ideal Range       IdealRange
//    Projectile Speed         ProjectileSpeed         Heat Reduction           HeatReduction
//    Cast Speed               CastSpeed               Mod Power Generation     ModGeneration
//    Mod Duration             ModDuration             Skill Cooldown           SkillCooldown
//    Skill Duration           SkillDuration           Use Speed                ConsumableSpeed
//    Consumable Duration      ConsumableDuration
//
//  --------------------------------------------------------------------------------------------
//  UI NAME  =>  SAVE RowName   (fusions - only ever appear as SLOTS, never as roll chances)
//  --------------------------------------------------------------------------------------------
//    Meta        WeakspotDamageCriticalDamage     Threshold    HealthPercentGreyHealthRate
//    Wizard      ModCriticalSkillCritical         Revitalize   HealthRegenSkillCooldown
//    Warrior     MeleeDamageMeleeSpeed            Athletic     MovementSpeedEvadeSpeed
//    Rogue       MeleeCriticalEvadeSpeed          Cleric       HealingEfficacyUseSpeed
//    Pugilist    MeleeSpeedStaminaFlat            Longevity    ModDurationSkillDuration
//    Gunfighter  FireRateReloadSpeed              Grip         WeaponSpreadSwapSpeed
//    Sniper      RangedDamageIdealRange           Flash        CastSpeedUseSpeed
//    Munitions   RangedCriticalAmmoReserves       Pirate       RangedDamageMeleeDamage
//    Mage        ModDamageModGeneration           Sapper       ExplosiveDamageDamageReduction
//    Hulk        HealthPercentStaminaPercent      Capacitor    FirearmChargeSpeedHeatReduction
//    Tank        DamageReductionArmorPercent      Selfless     ReviveSpeedHealingEfficacy
//    Protected   ShieldEfficacyArmorFlat
//
//  --------------------------------------------------------------------------------------------
//  UI NAME  =>  SAVE RowName   (legendaries - the +51 "Mythic" bonus; only ever appear as SLOTS)
//  --------------------------------------------------------------------------------------------
//    Allegiance          Allegiance          Altruistic          Altruistic
//    Artful Dodger       ArtfulDodger        Bodyguard           Bodyguard
//    Boundless Energy    BoundlessEnergy     Brutality           Brutality
//    Critical Situation  CriticalSituation   Dark Omen           DarkOmen
//    Defense Measures    DefensiveMeasures   Exhausted           Exhausted
//    Fleet Footed        FleetFooted         Full Hearted        FullHearted
//    Gigantic            Gigantic            God Tear            GodTear
//    Heavy Drinker       HeavyDrinker        Hyperactive         Hyperactive
//    Immovable           Immovable           Impervious          Impervious
//    Insult to Injury    InsultToInjury      Jack of all Trades  JackOfAllTrades
//    Luck of the Devil   LuckOfTheDevil      Master Killer       MasterKiller
//    Outlaw              Outlaw              Overpowered         Overpowered
//    Peak Conditioning   PeakConditioning    Physician           Physician
//    Power Fantasy       PowerFantasy        Power Trip          PowerTrip
//    Prime Time          PrimeTime           Reverberation       Reverberation
//    Sadistic            Sadistic            Sharpshooter        SharpShooter
//    Size Matters        SizeMatters         Soulmate            Soulmate
//    Spectrum            Spectrum            Speed Demon         SpeedDemon
//    Steel Plating       SteelPlating        Traitor             Traitor
//    Unbreakable         Unbreakable         Unbridled           Unbridled
//    Vaccinated          Vaccinated          Wrecking Ball       WreckingBall
//
// ============================================================================================
internal partial class Example
{
    // Add a roll chance (a fed fragment) at FedLevel 32 (the maximum). Works even on a never-fed prism:
    // it creates the CurrentFeedData array (and the HasBeenFed / PendingExperience fields) from scratch.
    // Prints a warning and does nothing else if that roll chance already exists (any value).
    public static void PrismAddRollChance()
    {
        Console.WriteLine("Add Prism Roll Chance===========");

        // ===== CHANGE THESE =====
        const int characterIndex = 0;                  // save_0 (first character)
        const string prismProfileId = "/Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfVoracity.PrismOfVoracity_C";
        const string fragmentRowName = "SkillDamage";  // fed fragment to add (single RowName)
        const int fedLevel = 32;                        // 32 = max (see conversions above)
        const string targetFileName = "prism_roll_chance_added.sav";
        // ========================

        string path = Path.Combine(Utils.GetSteamSavePath(), "profile.sav");
        Console.WriteLine($"Reading {path}");
        SaveFile sf = SaveFile.Read(path);

        // Locate the prism's PrismStoneInstanceData property bag.
        Navigator navigator = new(sf);
        List<ObjectProperty> characters = navigator.GetProperty("Characters")!.GetItems<ObjectProperty>();
        if (characterIndex >= characters.Count || characters[characterIndex].Object == null)
        {
            Console.WriteLine($"WARNING: character index {characterIndex} (save_{characterIndex}) not found.");
            return;
        }
        Property? prismItemBp = navigator.GetProperties("ItemBP", characters[characterIndex].Object)
            .FirstOrDefault(x => x.Value!.ToString() == prismProfileId);
        if (prismItemBp == null)
        {
            Console.WriteLine($"WARNING: prism not found on save_{characterIndex}.");
            return;
        }
        PropertyBag instance = prismItemBp.GetParent<PropertyBag>(navigator)["InstanceData"].Get<ObjectProperty>().Object!.Properties!;

        // The save's names table — needed to resolve/append the FName indices for any property we build
        // from scratch (a never-fed prism is missing CurrentFeedData, HasBeenFed, etc.).
        List<string> names = FindNamesTable(navigator.Lookup(prismItemBp))
                             ?? throw new InvalidOperationException("could not find the save's names table");

        // Create the CurrentFeedData array from scratch if the prism was never fed. Feeding also gives the
        // prism its HasBeenFed flag and a PendingExperience field (but NOT a Level - feeding doesn't level
        // it), so create those too; the PRISM block (CurrentSegments / Level) is left untouched.
        if (!instance.Contains("CurrentFeedData"))
        {
            ArrayStructProperty newArray = new()
            {
                HasPropertyGuid = 0,
                PropertyGuid = null,
                OuterElementType = N("StructProperty"),
                NameIndex = N("CurrentFeedData").Index,
                TypeIndex = N("StructProperty").Index,
                Size = 0,
                Index = 0,
                Count = 0,
                ElementType = N("FeedData"),   // the inner struct type for a fed-fragment entry
                Guid = default,
                InnerHasPropertyGuid = 0,
                InnerPropertyGuid = null,
                Items = []
            };
            instance.Properties.Add(new("CurrentFeedData", new Property
            {
                Name = N("CurrentFeedData"),
                Type = N("ArrayProperty"),
                Index = 0, Size = 0, HasPropertyGuid = null, PropertyGuid = null,
                Value = newArray
            }));
            if (!instance.Contains("HasBeenFed"))
                instance.Properties.Add(new("HasBeenFed", new Property
                {
                    Name = N("HasBeenFed"),
                    Type = N("BoolProperty"),
                    Index = 0, Size = 0, HasPropertyGuid = 0, PropertyGuid = null,
                    Value = (byte)1
                }));
            if (!instance.Contains("PendingExperience"))
                instance.Properties.Add(new("PendingExperience", new Property
                {
                    Name = N("PendingExperience"),
                    Type = N("FloatProperty"),
                    Index = 0, Size = 4, HasPropertyGuid = 0, PropertyGuid = null,
                    Value = 0f
                }));
            instance.RefreshLookup();
            Console.WriteLine("Created empty CurrentFeedData array (prism was never fed).");
        }
        ArrayStructProperty feed = (ArrayStructProperty)instance["CurrentFeedData"].Value!;
        if (feed.Items.Cast<PropertyBag>().Any(x => x["RowName"].ToStringValue() == fragmentRowName))
        {
            Console.WriteLine($"WARNING: roll chance '{fragmentRowName}' already exists - leaving it unchanged.");
            return;
        }

        // A FeedData entry is a struct (PropertyBag) with a RowName (NameProperty) and a FedLevel
        // (IntProperty). Both are tagged properties (HasPropertyGuid = 0); FName indices and the
        // array Count/Size are resolved/recomputed on write, so the placeholder 0s here are fine.
        PropertyBag entry = new()
        {
            Properties =
            [
                new("RowName", new Property
                {
                    Name = new FName { Index = 0, Number = null, Name = "RowName" },
                    Type = new FName { Index = 0, Number = null, Name = "NameProperty" },
                    Index = 0, Size = 0, HasPropertyGuid = 0, PropertyGuid = null,
                    Value = new FName { Index = 0, Number = null, Name = fragmentRowName }
                }),
                new("FedLevel", new Property
                {
                    Name = new FName { Index = 0, Number = null, Name = "FedLevel" },
                    Type = new FName { Index = 0, Number = null, Name = "IntProperty" },
                    Index = 0, Size = 0, HasPropertyGuid = 0, PropertyGuid = null,
                    Value = fedLevel
                }),
            ]
        };
        entry.RefreshLookup();
        feed.Items.Add(entry);
        Console.WriteLine($"Added roll chance '{fragmentRowName}' at FedLevel {fedLevel}.");

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"Copy {targetFileName} over your profile.sav to apply.");

        // Resolve `name` to an FName, appending it to the save's names table if it isn't there yet.
        FName N(string name)
        {
            int i = names.FindIndex(x => x == name);
            if (i < 0) { names.Add(name); i = names.Count - 1; }
            return new FName { Index = (ushort)i, Number = null, Name = name };
        }

        // Walk up the navigation node graph to the enclosing SaveData and return its names table.
        static List<string>? FindNamesTable(Node? node)
        {
            Node? cur = node;
            while (cur != null && cur.Object.GetType() != typeof(SaveData)) cur = cur.Parent;
            return ((SaveData?)cur?.Object)?.NamesTable;
        }
    }
}

using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Parts;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

// One of the prism-editing examples. For the character-index convention, the prism profile IDs,
// the UI-name => save RowName mappings (fragments, fusions and legendaries) and the level/value conversions,
// see the reference comment in Example.PrismAddRollChance.cs.
internal partial class Example
{
    // Set the prism's PRISM-block slots (CurrentSegments) to an exact target state: the array is grown
    // or shrunk to match, with any new slot entries built from scratch. Works even on a never-leveled
    // prism - it creates the CurrentSegments array (and the missing Level/HasBeenFed/PendingExperience
    // fields) from scratch from the values below.
    public static void PrismSetSegments()
    {
        Console.WriteLine("Set Prism Segments===========");

        // ===== CHANGE THESE =====
        const int characterIndex = 0;                  // save_0 (first character)
        const string prismProfileId = "/Game/Events/Paragon/PrismStone/UniquePrisms/PrismOfVoracity.PrismOfVoracity_C";
        const string targetFileName = "prism_segments_set.sav";
        // The exact slot state to write (one tuple per slot, in order). The array is grown/shrunk to
        // match; RowName = single fragment, fusion, or legendary (see the reference comment above),
        // Level = the slot's "+N".
        (string row, int lvl)[] target =
        [
            ("WeakspotDamageCriticalDamage", 10),  // Meta
            ("ShieldEfficacyArmorFlat", 10),       // Protected
            ("MovementSpeedEvadeSpeed", 10),       // Athletic
            ("RangedDamageMeleeDamage", 10),       // Pirate
            ("CastSpeed", 10),                     // Cast Speed
            ("Vaccinated", 1),                     // Vaccinated (legendary)
        ];
        // Overall prism state, so the edit looks plausible (a save-edited prism is otherwise internally
        // inconsistent - e.g. maxed slots on a level-1 prism). The game shows the slot-sum as the "+N";
        // `level` is the hidden raw roll count and should be AT LEAST the minimum the slots imply:
        //     ~lvl          per single or legendary slot
        //     ~(2*5 + lvl)  per FUSION slot  (a fusion = two components leveled to +5, then re-leveled)
        // The target above (4 fusions +10, a single +10, a legendary +1)  ->  20*4 + 10 + 1 = 91.
        const int level = 91;                          // raw prism level (byte 0-255; >= the minimum above)
        const bool hasBeenFed = false;                 // whether the prism has fed ROLL CHANCES
        const float pendingExperience = 0f;            // banked XP toward the next level (0 once maxed)
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

        // The enclosing SaveData — owns the names table we resolve/append FName indices into for any
        // property we build from scratch (a never-leveled prism is missing CurrentSegments, Level, etc.).
        SaveData saveData = prismItemBp.EnclosingSaveData(navigator);

        // Create the CurrentSegments array from scratch if the prism was never leveled.
        if (!instance.Contains("CurrentSegments"))
        {
            ArrayStructProperty newArray = new()
            {
                HasPropertyGuid = 0,
                PropertyGuid = null,
                OuterElementType = saveData.MakeFName("StructProperty"),
                NameIndex = saveData.MakeFName("CurrentSegments").Index,
                TypeIndex = saveData.MakeFName("StructProperty").Index,
                Size = 0,
                Index = 0,
                Count = 0,
                ElementType = saveData.MakeFName("PrismSegment"),   // the inner struct type for a slot entry
                Guid = default,
                InnerHasPropertyGuid = 0,
                InnerPropertyGuid = null,
                Items = []
            };
            instance.Properties.Add(new("CurrentSegments", new Property
            {
                Name = saveData.MakeFName("CurrentSegments"),
                Type = saveData.MakeFName("ArrayProperty"),
                Index = 0, Size = 0, HasPropertyGuid = null, PropertyGuid = null,
                Value = newArray
            }));
            instance.RefreshLookup();
            Console.WriteLine("Created empty CurrentSegments array (prism was never leveled).");
        }
        ArrayStructProperty segments = (ArrayStructProperty)instance["CurrentSegments"].Value!;

        // Grow/shrink the array to the target length: drop surplus entries from the end, append fresh
        // ones built from scratch, then set RowName + Level on every slot.
        int original = segments.Items.Count;
        while (segments.Items.Count > target.Length)
            segments.Items.RemoveAt(segments.Items.Count - 1);
        while (segments.Items.Count < target.Length)
            segments.Items.Add(NewSegment());
        if (original != target.Length)
            Console.WriteLine($"Resized CurrentSegments {original} -> {target.Length} slot(s).");

        for (int i = 0; i < target.Length; i++)
        {
            PropertyBag seg = (PropertyBag)segments.Items[i]!;
            seg["RowName"].Value = new FName { Index = 0, Number = null, Name = target[i].row };
            seg["Level"].Value = target[i].lvl;
            Console.WriteLine($"  segment[{i}] = {target[i].row} L{target[i].lvl}");
        }

        // Overall prism state (Level is a ByteProperty, HasBeenFed a BoolProperty stored as a byte,
        // PendingExperience a float). Set it if present, otherwise create it from scratch.
        if (instance.Contains("Level"))
            ((ByteProperty)instance["Level"].Value!).EnumByte = level;
        else
            instance.Properties.Add(new("Level", new Property
            {
                Name = saveData.MakeFName("Level"),
                Type = saveData.MakeFName("ByteProperty"),
                Index = 0, Size = 0, HasPropertyGuid = null, PropertyGuid = null,
                Value = new ByteProperty { EnumName = saveData.MakeFName("None"), HasPropertyGuid = 0, PropertyGuid = null, EnumByte = level, EnumValue = null }
            }));

        if (instance.Contains("HasBeenFed"))
            instance["HasBeenFed"].Value = (byte)(hasBeenFed ? 1 : 0);
        else
            instance.Properties.Add(new("HasBeenFed", new Property
            {
                Name = saveData.MakeFName("HasBeenFed"),
                Type = saveData.MakeFName("BoolProperty"),
                Index = 0, Size = 0, HasPropertyGuid = 0, PropertyGuid = null,
                Value = (byte)(hasBeenFed ? 1 : 0)
            }));

        if (instance.Contains("PendingExperience"))
            instance["PendingExperience"].Value = pendingExperience;
        else
            instance.Properties.Add(new("PendingExperience", new Property
            {
                Name = saveData.MakeFName("PendingExperience"),
                Type = saveData.MakeFName("FloatProperty"),
                Index = 0, Size = 4, HasPropertyGuid = 0, PropertyGuid = null,
                Value = pendingExperience
            }));
        instance.RefreshLookup();
        Console.WriteLine($"Level = {level}, HasBeenFed = {hasBeenFed}, PendingExperience = {pendingExperience}");

        Console.WriteLine($"Writing to {targetFileName}...");
        SaveFile.Write(targetFileName, sf);
        Console.WriteLine($"Copy {targetFileName} over your profile.sav to apply.");

        // A CurrentSegments entry is a struct (PropertyBag) with a RowName (NameProperty) and a Level
        // (IntProperty). Both are tagged properties (HasPropertyGuid = 0); FName indices and the array
        // Count/Size are resolved/recomputed on write, so the placeholder 0s here are fine. (Same shape
        // as the feed-entry builder in Example.PrismAddRollChance.)
        static PropertyBag NewSegment()
        {
            PropertyBag seg = new()
            {
                Properties =
                [
                    new("RowName", new Property
                    {
                        Name = new FName { Index = 0, Number = null, Name = "RowName" },
                        Type = new FName { Index = 0, Number = null, Name = "NameProperty" },
                        Index = 0, Size = 0, HasPropertyGuid = 0, PropertyGuid = null,
                        Value = new FName { Index = 0, Number = null, Name = "" }
                    }),
                    new("Level", new Property
                    {
                        Name = new FName { Index = 0, Number = null, Name = "Level" },
                        Type = new FName { Index = 0, Number = null, Name = "IntProperty" },
                        Index = 0, Size = 0, HasPropertyGuid = 0, PropertyGuid = null,
                        Value = 0
                    }),
                ]
            };
            seg.RefreshLookup();
            return seg;
        }
    }
}

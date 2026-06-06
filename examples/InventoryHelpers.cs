using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

internal partial class Example
{
    // Build a brand-new inventory item (an ItemBP reference object + its InstanceData object) entirely from
    // scratch and add it to the character's inventory. `instanceDataType` is the InstanceData class name -
    // "ItemInstanceData" for materials/consumables, "EquipmentInstanceData" for equipment. `quantity` is the
    // stack count for materials; pass null for equipment, whose InstanceData carries an EMPTY property bag in
    // real saves (verified - rings/amulets/armor store no Quantity). This is the from-scratch alternative to
    // shallow-cloning an existing item (compare Example.AddRing, which clones).
    private static void AddInventoryItem(Navigator navigator, ObjectProperty character, string itemId, string instanceDataType, int? quantity)
    {
        // Anchor on any existing inventory item to locate the inventory array and the enclosing SaveData
        // (whose Objects list and names table the two new objects must join).
        Property anchor = navigator.GetProperties("ItemBP", character.Object).First();
        ArrayStructProperty inventory = anchor.GetParent<PropertyBag>(navigator).GetParent<ArrayStructProperty>(navigator);
        SaveData saveData = anchor.EnclosingSaveData(navigator);

        // The item-blueprint reference object: a "loaded" object, just the path.
        UObject itemObject = new() { WasLoadedByte = 1, ObjectPath = itemId };
        saveData.Objects.Add(itemObject);
        itemObject.ObjectIndex = saveData.Objects.Count - 1;

        // The per-item instance data object. Materials carry a stack Quantity; real-save equipment has an
        // EMPTY property bag, so we add Quantity only when one is supplied (null => equipment).
        List<KeyValuePair<string, Property>> instanceProps = quantity.HasValue ? [MakeInt("Quantity", quantity.Value)] : [];
        UObject instanceData = new()
        {
            WasLoadedByte = 0,
            ObjectPath = $"/Script/GunfireRuntime.{instanceDataType}",
            ExtraPropertiesData = new byte[4],
            LoadedData = new() { OuterId = 0, Name = saveData.MakeFName(instanceDataType) },
            Properties = new() { Properties = instanceProps }
        };
        instanceData.Properties.RefreshLookup();
        saveData.Objects.Add(instanceData);
        instanceData.ObjectIndex = saveData.Objects.Count - 1;

        // The inventory entry tying the item BP to its instance data.
        int newId = NextInventoryId(navigator, inventory);
        PropertyBag entry = new()
        {
            Properties =
            [
                MakeInt("ID", newId),
                MakeObjectRef("ItemBP", itemObject.ObjectIndex),
                MakeBool("New", true),
                MakeBool("Favorited", false),
                MakeBool("Hidden", false),
                MakeInt("EquipmentSlotIndex", -1),
                MakeObjectRef("InstanceData", instanceData.ObjectIndex),
            ]
        };
        entry.RefreshLookup();
        inventory.Items.Add(entry);

        // ----- local property builders (each tags the new property's FName via saveData.MakeFName) -----
        KeyValuePair<string, Property> MakeInt(string name, int value) =>
            new(name, new Property { Name = saveData.MakeFName(name), Type = saveData.MakeFName("IntProperty"), Index = 0, Size = 4, HasPropertyGuid = 0, PropertyGuid = null, Value = value });

        KeyValuePair<string, Property> MakeBool(string name, bool value) =>
            new(name, new Property { Name = saveData.MakeFName(name), Type = saveData.MakeFName("BoolProperty"), Index = 0, Size = 0, HasPropertyGuid = 0, PropertyGuid = null, Value = (byte)(value ? 1 : 0) });

        KeyValuePair<string, Property> MakeObjectRef(string name, int objectIndex) =>
            new(name, new Property { Name = saveData.MakeFName(name), Type = saveData.MakeFName("ObjectProperty"), Index = 0, Size = 4, HasPropertyGuid = 0, PropertyGuid = null, Value = new ObjectProperty { ObjectIndex = objectIndex } });
    }

    // Allocate the next inventory item ID and advance the inventory's IDGen counter. IDGen is the
    // inventory's last-assigned ID, so the next ID is IDGen + 1; we write it back to keep the game's
    // invariant (IDGen == highest ID). The Max(maxId, ...) is purely defensive: in every inventory
    // observed across the saves maxId == IDGen, so IDGen + 1 alone would do - the Max() only guards
    // against an IDGen left stale (below maxId) by an earlier max(ID)+1-style edit. Shared by the
    // from-scratch AddInventoryItem and the clone examples (AddRing / EditCurrencyClone) so both keep IDGen in sync.
    private static int NextInventoryId(Navigator navigator, ArrayStructProperty inventory)
    {
        PropertyBag container = inventory.GetParent<Property>(navigator).GetParent<PropertyBag>(navigator);
        int maxId = inventory.Items.Select(x => (int)((PropertyBag)x!)["ID"].Value!).Max();
        int idGen = container.Contains("IDGen") ? (int)container["IDGen"].Value! : maxId;
        int newId = Math.Max(maxId, idGen) + 1;
        if (container.Contains("IDGen"))
            container["IDGen"].Value = newId;
        return newId;
    }
}

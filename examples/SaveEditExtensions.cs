using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Memory;
using lib.remnant2.saves.Model.Parts;
using lib.remnant2.saves.Model.Properties;
using lib.remnant2.saves.Navigation;

namespace examples;

// Shared save-editing helpers for the examples: navigating to the enclosing SaveData, FName / names-table
// handling, and the shallow-copy builders used when cloning items. (Consolidated from the former
// LoadedDataExtensions / UObjectExtensions / PropertyExtensions one-method classes.)
public static class SaveEditExtensions
{
    // Climb the navigation graph from any node up to its enclosing SaveData. Works from a deeply-nested
    // property OR a UObject (the latter is a single hop, since objects sit directly under the SaveData) -
    // unlike the library's single-hop GetParent<SaveData>, which only works when SaveData is the immediate parent.
    public static SaveData EnclosingSaveData(this ModelBase from, Navigator navigator)
    {
        Node? cur = navigator.Lookup(from);
        while (cur != null && cur.Object is not SaveData) cur = cur.Parent;
        return (SaveData?)cur?.Object ?? throw new InvalidOperationException("could not find the save's SaveData");
    }

    // Resolve a name to an FName against the save's names table, appending it if it isn't there yet.
    public static FName MakeFName(this SaveData saveData, string name)
    {
        int i = saveData.NamesTable.FindIndex(x => x == name);
        if (i < 0) { saveData.NamesTable.Add(name); i = saveData.NamesTable.Count - 1; }
        return new FName { Index = (ushort)i, Number = null, Name = name };
    }

    // Shallow-copy a UObject, register the copy in its parent SaveData's Objects list, and return it.
    public static UObject ShallowCopyObject(this UObject obj, Navigator navigator)
    {
        UObject result = new()
        {
            Components = obj.Components,
            ExtraPropertiesData = obj.ExtraPropertiesData,
            IsActor = obj.IsActor,
            LoadedData = obj.LoadedData,
            ObjectPath = obj.ObjectPath,
            Properties = obj.Properties,
            WasLoadedByte = obj.WasLoadedByte,
        };
        SaveData parent = obj.GetParent<SaveData>(navigator);
        parent.Objects.Add(result);
        result.ObjectIndex = parent.Objects.FindIndex(x => x == result);
        return result;
    }

    // Shallow-copy a Property (new wrapper sharing the same FName / type / value references).
    public static Property ShallowCopyProperty(this Property property)
    {
        Property result = new()
        {
            Name = property.Name,
            Index = property.Index,
            Size = property.Size,
            HasPropertyGuid = property.HasPropertyGuid,
            PropertyGuid = property.PropertyGuid,
            Type = property.Type,
            Value = property.Value
        };
        return result;
    }

    // Shallow-copy a UObjectLoadedData (fresh FName wrapper, same OuterId).
    public static UObjectLoadedData ShallowCopyLoadedData(this UObjectLoadedData data)
    {
        UObjectLoadedData result = new()
        {
            Name = new FName()
            {
                Name = data.Name.Name,
                Index = data.Name.Index,
                Number = null
            },
            OuterId = data.OuterId
        };
        return result;
    }
}

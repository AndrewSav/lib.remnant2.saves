using rd2parser.Model;
using rd2parser.Navigation;

namespace rd2parser.examples;
public static class UObjectExtension
{
    public static UObject ShallowCopyObject(this UObject obj, Navigator navigator)
    {
        UObject result = new()
        {
            Components = obj.Components,
            ExtraPropertiesData = obj.ExtraPropertiesData,
            IsActor = obj.IsActor,
            LoadedData = obj.LoadedData,
            ObjectIndex = obj.ObjectIndex,
            ObjectPath = obj.ObjectPath,
            Properties = obj.Properties,
            WasLoadedByte = obj.WasLoadedByte,
        };
        SaveData parent = obj.GetParent<SaveData>(navigator);
        parent.Objects.Add(result);
        result.ObjectIndex = parent.Objects.FindIndex(x => x == result);
        return result;
    }
}

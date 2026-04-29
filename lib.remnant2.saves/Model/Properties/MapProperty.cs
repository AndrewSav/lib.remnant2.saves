using System.Diagnostics.CodeAnalysis;
using lib.remnant2.saves.Model.Memory;
using lib.remnant2.saves.Model.Parts;
using lib.remnant2.saves.Model.Properties.Parts;
using Serilog;

namespace lib.remnant2.saves.Model.Properties;

public class MapProperty : ModelBase
{
    public static ILogger Logger => Log.Logger.ForContext(Log.Category, Log.Parser).ForContext<MapProperty>();
    public required FName KeyType;
    public required FName ValueType;
    public required byte HasPropertyGuid;
    public FGuid? PropertyGuid;
    public required int KeysToRemoveCount;
    public required List<object> KeysToRemove;
    public required List<KeyValuePair<object, object>> Values;

    public MapProperty()
    {
    }

    [SetsRequiredMembers]
    public MapProperty(Reader r, SerializationContext ctx)
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        KeyType = new(r, ctx.NamesTable);
        ValueType = new(r, ctx.NamesTable);
        (HasPropertyGuid, PropertyGuid) = PropertyValue.ReadPropertyGuid(r);

        KeysToRemoveCount = r.Read<int>();
        KeysToRemove = new(KeysToRemoveCount);
        for (int i = 0; i < KeysToRemoveCount; i++)
        {
            object key = PropertyValue.ReadPropertyValue(r, ctx, KeyType.Name).Value!;
            KeysToRemove.Add(key);
        }

        int len = r.Read<int>();
        Values = new(len);
        for (int i = 0; i < len; i++)
        {
            object key = PropertyValue.ReadPropertyValue(r, ctx, KeyType.Name).Value!;
            object value = PropertyValue.ReadPropertyValue(r, ctx, ValueType.Name).Value!;
            Values.Add(new(key, value));
        }
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        KeyType.Write(w, ctx);
        ValueType.Write(w, ctx);
        PropertyValue.WritePropertyGuid(w, HasPropertyGuid, PropertyGuid);
        KeysToRemoveCount = KeysToRemove.Count;
        w.Write(KeysToRemoveCount);
        foreach (object keyToRemove in KeysToRemove)
        {
            PropertyValue.WritePropertyValue(w, ctx, keyToRemove, KeyType.Name);
        }

        w.Write(Values.Count);
        foreach (KeyValuePair<object, object> keyValuePair in Values)
        {
            PropertyValue.WritePropertyValue(w, ctx, keyValuePair.Key, KeyType.Name);
            PropertyValue.WritePropertyValue(w, ctx, keyValuePair.Value, ValueType.Name);
        }
        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
    }
    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        for (int index = 0; index < KeysToRemove.Count; index++)
        {
            if (KeysToRemove[index] is ModelBase keyToRemove)
                yield return (keyToRemove, index);
        }

        for (int index = 0; index < Values.Count; index++)
        {
            KeyValuePair<object, object> kvp = Values[index];
            if (kvp.Key is ModelBase key)
                yield return (key, index);

            if (kvp.Value is ModelBase value)
                yield return (value, index);
        }
    }
}

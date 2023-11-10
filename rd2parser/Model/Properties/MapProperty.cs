﻿using System.Diagnostics.CodeAnalysis;
using rd2parser.Model.Parts;
using rd2parser.Model.Properties.Parts;

namespace rd2parser.Model.Properties;

public class MapProperty : ModelBase
{
    public required FName KeyType;
    public required FName ValueType;
    public required byte[] Unknown;
    public required List<KeyValuePair<object, object>> Values;

    public MapProperty()
    {
    }

    [SetsRequiredMembers]
    public MapProperty(Reader r, SerializationContext ctx)
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        Values = new List<KeyValuePair<object, object>>();
        KeyType = new(r, ctx.NamesTable);
        ValueType = new(r, ctx.NamesTable);
        Unknown = r.ReadBytes(5);
        if (Unknown.Any(x => x != 0))
        {
            string debug = BitConverter.ToString(Unknown);
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown bytes at {Offset}", debug, r.Position);
        }

        int len = r.Read<int>();
        for (int i = 0; i < len; i++)
        {
            object key = PropertyValue.ReadPropertyValue(r, ctx, KeyType.Name).Value!;
            object value = PropertyValue.ReadPropertyValue(r, ctx, ValueType.Name).Value!;
            Values.Add(new KeyValuePair<object, object>(key, value));
        }
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        KeyType.Write(w, ctx);
        ValueType.Write(w, ctx);
        w.WriteBytes(Unknown);
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

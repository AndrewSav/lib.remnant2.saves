using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model.Properties;

public class MapProperty : Node
{
    public required FName KeyType;
    public required FName ValueType;
    public required byte[] Unknown;
    public required List<KeyValuePair<object, object>> Values;

    public MapProperty(Node? parent) : base(parent, parent!.Path )
    {
        Path.Add(new() { Type = "MapProperty" });
    }

    [SetsRequiredMembers]
    public MapProperty(Reader r, SerializationContext ctx, Node? parent) : this(parent)
    {
        Values = new List<KeyValuePair<object, object>>();
        KeyType = new(r, ctx.NamesTable);
        ValueType = new(r, ctx.NamesTable);
        Unknown = r.ReadBytes(5);
        int len = r.Read<int>();
        for (int i = 0; i < len; i++)
        {
            object key = PropertyValue.ReadPropertyValue(r, ctx, KeyType.Name, this).Value!;
            AddIndexToChild(key, i);
            object value = PropertyValue.ReadPropertyValue(r, ctx, ValueType.Name, this).Value!;
            AddIndexToChild(value, i);
            Values.Add(new KeyValuePair<object, object>(key, value));
        }
    }

    public MapProperty()
    {
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        KeyType.Write(w, ctx);
        ValueType.Write(w, ctx);
        w.WriteBytes(Unknown);
        w.Write(Values.Count);
        foreach (var keyValuePair in Values)
        {
            PropertyValue.WritePropertyValue(w, ctx, keyValuePair.Key, KeyType.Name);
            PropertyValue.WritePropertyValue(w, ctx, keyValuePair.Value, ValueType.Name);
        }
    }
}
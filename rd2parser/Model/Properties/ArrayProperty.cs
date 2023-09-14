using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class ArrayProperty : Node
{
    public required FName ElementType;
    public required List<object?> Items;
    public required byte Unknown;

    public ArrayProperty(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new Segment { Type = "ArrayProperty" });
    }

    [SetsRequiredMembers]
    public ArrayProperty(Reader r, SerializationContext ctx, uint count, byte unknown, FName elementType,
        Node? parent) : this(parent)
    {
        Unknown = unknown;
        if (Unknown != 0)
        {
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown byte at {Location}, {Offset}", Unknown, DisplayPath, r.Position);
        }
        ElementType = elementType;
        Items = new List<object?>();
        for (int i = 0; i < count; i++)
        {
            object o = PropertyValue.ReadPropertyValue(r, ctx, ElementType.Name, this).Value!;
            AddIndexToChild(o, i);
            Items.Add(o);
        }
    }

    public ArrayProperty()
    {
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        ElementType.Write(w, ctx);
        w.Write(Unknown);
        w.Write(Items.Count);
        foreach (object? item in Items) PropertyValue.WritePropertyValue(w, ctx, item, ElementType.Name);
    }
}

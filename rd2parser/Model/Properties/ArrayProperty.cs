using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model.Properties;

public class ArrayProperty
{
    public required byte Unknown;
    public required FName ElementType;
    public required List<object?> Items;

    public ArrayProperty()
    {

    }

    [SetsRequiredMembers]
    public ArrayProperty(Reader r, SerializationContext ctx, uint count, byte unknown, FName elementType)
    {
        Unknown = unknown;
        ElementType = elementType;
        Items = new List<object?>();
        for (int i = 0; i < count; i++)
        {
            Items.Add(PropertyValue.ReadPropertyValue(r, ctx, ElementType.Name).Value);
        }
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        ElementType.Write(w, ctx);
        w.Write(Unknown);
        w.Write(Items.Count);
        foreach (object? item in Items)
        {
            PropertyValue.WritePropertyValue(w,ctx,item,ElementType.Name);
        }
    }
}
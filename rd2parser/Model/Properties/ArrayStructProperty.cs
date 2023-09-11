using System.Diagnostics.CodeAnalysis;
using rd2parser.Model.Memory;

namespace rd2parser.Model.Properties;

public class ArrayStructProperty
{
    public required byte Unknown;
    public required FName OuterElementType;
    public required ushort NameIndex;
    public required ushort TypeIndex;
    public required uint Size;
    public required uint Index;
    public required uint Count;
    public required FName ElementType;
    public required FGuid Guid;
    public required byte Unknown2;
    public required List<object?> Items;

    public ArrayStructProperty()
    {

    }

    [SetsRequiredMembers]
    public ArrayStructProperty(Reader r, SerializationContext ctx, uint count, byte unknown, FName elementType)
    {
        Unknown = unknown;
        OuterElementType = elementType;
        NameIndex = r.Read<ushort>();
        TypeIndex = r.Read<ushort>();
        Size = r.Read<uint>();
        Index = r.Read<uint>();
        ElementType = new(r, ctx.NamesTable);
        Guid = r.Read<FGuid>();
        Unknown2 = r.Read<byte>();

        Count = count;
        Items = new List<object?>();

        for (int i = 0; i < Count; i++)
        {
            Items.Add(StructProperty.ReadStructPropertyValue(r, ctx, ElementType.Name));
        }
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        OuterElementType.Write(w, ctx);
        w.Write(Unknown);
        w.Write(Items.Count);
        w.Write(NameIndex);
        w.Write(TypeIndex);
        w.Write(Size);
        w.Write(Index);
        ElementType.Write(w,ctx);
        w.Write(Guid);
        w.Write(Unknown2);
        foreach (object? item in Items)
        {
            StructProperty.WriteStructPropertyValue(w, ctx, ElementType.Name, item);
        }
    }
}
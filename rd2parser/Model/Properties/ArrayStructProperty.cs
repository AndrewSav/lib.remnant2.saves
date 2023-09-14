using System.Diagnostics.CodeAnalysis;
using rd2parser.Model.Memory;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class ArrayStructProperty : Node
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

    public ArrayStructProperty(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "ArrayStructProperty" });
    }

    [SetsRequiredMembers]
    public ArrayStructProperty(Reader r, SerializationContext ctx, uint count, byte unknown, FName elementType, int readOffset, Node? parent) : this(parent)
    {
        ReadOffset = readOffset;
        Unknown = unknown;
        if (Unknown != 0)
        {
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown byte at {Location}, {Offset}", Unknown, DisplayPath, r.Position);
        }
        OuterElementType = elementType;
        NameIndex = r.Read<ushort>();
        TypeIndex = r.Read<ushort>();
        Size = r.Read<uint>();
        Index = r.Read<uint>();
        ElementType = new(r, ctx.NamesTable);
        Guid = r.Read<FGuid>();
        Unknown2 = r.Read<byte>();
        if (Unknown != 0)
        {
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown2 byte at {Location}, {Offset}", Unknown2, DisplayPath, r.Position);
        }

        Count = count;
        Items = new List<object?>();

        for (int i = 0; i < Count; i++)
        {
            object o = StructProperty.ReadStructPropertyValue(r, ctx, ElementType.Name, this)!;
            AddIndexToChild(o,i);
            Items.Add(o);
        }
    }

    public ArrayStructProperty()
    {
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        OuterElementType.Write(w, ctx);
        w.Write(Unknown);
        w.Write(Items.Count);
        w.Write(NameIndex);
        w.Write(TypeIndex);
        long sizeOffset = w.Position;
        w.Write(Size);
        w.Write(Index);
        ElementType.Write(w,ctx);
        w.Write(Guid);
        w.Write(Unknown2);
        long startOffset = w.Position;
        foreach (object? item in Items)
        {
            StructProperty.WriteStructPropertyValue(w, ctx, ElementType.Name, item);
        }
        long endOffset = w.Position;
        uint newSize = (uint)(endOffset - startOffset);
        Size = newSize;
        w.Position = sizeOffset;
        w.Write(Size);
        w.Position = endOffset;
    }
    public override IEnumerable<Node> GetChildren()
    {
        foreach (object? item in Items)
        {
            if (item is Node node)
                yield return node;
        }
    }
}

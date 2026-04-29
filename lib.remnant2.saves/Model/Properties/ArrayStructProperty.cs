using System.Diagnostics.CodeAnalysis;
using lib.remnant2.saves.Model.Memory;
using lib.remnant2.saves.Model.Parts;
using lib.remnant2.saves.Model.Properties.Parts;
using Serilog;

namespace lib.remnant2.saves.Model.Properties;

public class ArrayStructProperty : ModelBase
{
    public static ILogger Logger => Log.Logger.ForContext(Log.Category, Log.Parser).ForContext<ArrayStructProperty>();

    public required byte HasPropertyGuid;
    public FGuid? PropertyGuid;
    public required FName OuterElementType;
    public required ushort NameIndex;
    public required ushort TypeIndex;
    public required uint Size;
    public required uint Index;
    public required uint Count;
    public required FName ElementType;
    public required FGuid Guid;
    public required byte InnerHasPropertyGuid;
    public FGuid? InnerPropertyGuid;
    public required List<object?> Items;


    public ArrayStructProperty()
    {
    }

    [SetsRequiredMembers]
    public ArrayStructProperty(Reader r, SerializationContext ctx, uint count, byte hasPropertyGuid, FGuid? propertyGuid, FName elementType, int readOffset)
    {
        ReadOffset = readOffset;
        HasPropertyGuid = hasPropertyGuid;
        PropertyGuid = propertyGuid;
        OuterElementType = elementType;
        NameIndex = r.Read<ushort>();
        TypeIndex = r.Read<ushort>();
        Size = r.Read<uint>();
        Index = r.Read<uint>();
        ElementType = new(r, ctx.NamesTable);
        Guid = r.Read<FGuid>();
        (InnerHasPropertyGuid, InnerPropertyGuid) = PropertyValue.ReadPropertyGuid(r);

        Count = count;
        Items = new((int)Count);

        for (int i = 0; i < Count; i++)
        {
            object o = StructProperty.ReadStructPropertyValue(r, ctx, ElementType.Name)!;
            Items.Add(o);
        }
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        OuterElementType.Write(w, ctx);
        PropertyValue.WritePropertyGuid(w, HasPropertyGuid, PropertyGuid);
        w.Write(Items.Count);
        w.Write(NameIndex);
        w.Write(TypeIndex);
        long sizeOffset = w.Position;
        w.Write(Size);
        w.Write(Index);
        ElementType.Write(w,ctx);
        w.Write(Guid);
        PropertyValue.WritePropertyGuid(w, InnerHasPropertyGuid, InnerPropertyGuid);
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
        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
    }
    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        for (int index = 0; index < Items.Count; index++)
        {
            object? item = Items[index];
            if (item is ModelBase node)
                yield return (node, index);
        }
    }

    public override string ToString()
    {
        return $"{GetType().Name}({ElementType})";
    }
}

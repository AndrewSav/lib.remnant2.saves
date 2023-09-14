using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class Property : Node
{
    public required FName Name;
    public uint? Index;
    public uint? Size;
    public byte? NoRaw;
    public FName? Type;
    public object? Value;

    [JsonIgnore]
    public PropertyBag Bag => (PropertyBag)Parent!;

    public Property()
    {

    }
    public Property(Node? parent, string name) : base(parent, parent?.Path ?? new())
    {
        Path.Add(new() { Name = name, Type = "Property" });
    }
    [SetsRequiredMembers]
    public Property(Reader r, SerializationContext ctx, PropertyBag parent) : base(parent, new List<Segment>(parent.Path))
    {
        Name = new(r, ctx.NamesTable);
        Path.Add(new() { Name = Name.Name, Type = "Property" });
        if (Name.Name == "None")
        {
            return;
        }
        Type = new(r, ctx.NamesTable);
        Size = r.Read<uint>();
        Index = r.Read<uint>();

        // Just to make results a bit more compact
        // since we are usually not interested very much in these
        //if (Name.Name == "FowVisitedCoordinates")
        //{
        //    Value = r.ReadBytes((int)Size + 19);
        //}
        //else
        //{
        PropertyValue pv = PropertyValue.ReadPropertyValue(r, ctx, Type.Name, this,false);
        NoRaw = pv.NoRawByte;
        Value = pv.Value;
        //}

        ctx.PropertyRegistry.Add(Name.Name, this);
    }

    public override string? ToString()
    {
        return Value?.ToString();
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        Name.Write(w, ctx);
        if (Name.Name == "None")
        {
            return;
        }
        Type!.Write(w, ctx);
        long sizeOffset = w.Position;
        w.Write(Size!.Value);
        w.Write(Index!.Value);
        //if (Name.Name == "FowVisitedCoordinates")
        //{
        //    w.WriteBytes((byte[])Value!);
        //}
        //else
        //{
        long startOffset = w.Position;
        PropertyValue.WritePropertyValue(w,ctx,Value!,Type.Name, NoRaw??0);
        //}

        //ByteProperty


        long endOffset = w.Position;
        uint newSize = (uint)(endOffset - startOffset);

        uint sizeAdjustment = 0;
        if (Type.Name == "StructProperty")
        {
            sizeAdjustment = 19;
        }
        if (Type.Name == "ArrayProperty")
        {
            sizeAdjustment = 3;
        }

        if (sizeAdjustment != 0)
        {
            newSize -= sizeAdjustment;
            Size = newSize;
            w.Position = sizeOffset;
            w.Write(Size!.Value);
            w.Position = endOffset;
        }
    }
    public List<T> GetItems<T>()
    {
        if (Type?.Name != "ArrayProperty")
        {
            throw new InvalidOperationException($"this method only works for ArrayProperty. Current type: '{Type?.Name}'");
        }

        return Value switch
        {
            ArrayProperty ap => ap.Items.Select(x => (T)x!).ToList(),
            ArrayStructProperty asp => asp.Items.Select(x => (T)x!).ToList(),
            _ => throw new InvalidOperationException("unexpected value type")
        };
    }

    public override Node Copy()
    {
        Property result = (Property)MemberwiseClone();
        result.Path = new(Path);
        return result;
    }
    public override IEnumerable<Node> GetChildren()
    {
        if (Value is Node node)
            yield return node;
    }
}

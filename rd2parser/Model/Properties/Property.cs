using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;


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
    public required PropertyBag Bag;

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
        Bag = parent;
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
        w.Write(Size!.Value);
        w.Write(Index!.Value);
        //if (Name.Name == "FowVisitedCoordinates")
        //{
        //    w.WriteBytes((byte[])Value!);
        //}
        //else
        //{
        PropertyValue.WritePropertyValue(w,ctx,Value!,Type.Name, NoRaw??0);
        //}
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
}

using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model.Properties;

public class Property
{
    public required FName Name;
    public uint? Index;
    public uint? Size;
    public byte? NoRaw;
    public FName? Type;
    public object? Value;

    public Property()
    {

    }
    [SetsRequiredMembers]
    public Property(Reader r, SerializationContext ctx)
    {
        Name = new(r, ctx.NamesTable);
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
        PropertyValue pv = PropertyValue.ReadPropertyValue(r, ctx, Type.Name, false);
        NoRaw = pv.NoRawByte;
        Value = pv.Value;
        //}
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
}
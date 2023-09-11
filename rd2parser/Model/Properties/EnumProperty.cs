using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model.Properties;

public class EnumProperty
{
    public required FName EnumType;
    public required byte Unknown;
    public required FName EnumValue;

    public EnumProperty()
    {

    }
    [SetsRequiredMembers]
    public EnumProperty(Reader r, SerializationContext ctx)
    {
        EnumType = new(r, ctx.NamesTable);
        Unknown = r.Read<byte>();
        EnumValue = new(r, ctx.NamesTable);
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        EnumType.Write(w, ctx);
        w.Write(Unknown);
        EnumValue.Write(w, ctx);
    }
}
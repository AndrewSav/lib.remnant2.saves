using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model.Properties;

public class ByteProperty
{
    public required FName EnumName;
    public required byte Unknown;
    public required byte? EnumByte;
    public required FName? EnumValue;

    public ByteProperty()
    {

    }
    [SetsRequiredMembers]
    public ByteProperty(Reader r, SerializationContext ctx)
    {
        EnumName = new(r, ctx.NamesTable);
        Unknown = r.Read<byte>();

        if (EnumName.Name == "None")
        {
            EnumByte = r.Read<byte>();
        }
        else
        {
            EnumValue = new(r, ctx.NamesTable);
        }
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        EnumName.Write(w, ctx);
        w.Write(Unknown);
        if (EnumName.Name == "None")
        {
            w.Write(EnumByte!.Value);
        }
        else
        {
            EnumValue!.Write(w, ctx);
        }
    }
}
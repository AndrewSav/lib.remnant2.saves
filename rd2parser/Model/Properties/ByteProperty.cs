using System.Diagnostics.CodeAnalysis;
using rd2parser.Model.Parts;

namespace rd2parser.Model.Properties;

public class ByteProperty : ModelBase
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
        ReadOffset = r.Position + ctx.ContainerOffset;
        EnumName = new(r, ctx.NamesTable);
        Unknown = r.Read<byte>();
        if (Unknown != 0)
        {
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown byte at {Offset}", Unknown, r.Position);
        }

        if (EnumName.Name == "None")
        {
            EnumByte = r.Read<byte>();
        }
        else
        {
            EnumValue = new(r, ctx.NamesTable);
        }
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
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
        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
    }

    public override string? ToString()
    {
        return EnumName.Name == "None" ? EnumByte!.ToString() : EnumValue!.ToString();
    }
    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        yield break;
    }
}

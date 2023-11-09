using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class EnumProperty : Node
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
        ReadOffset = r.Position + ctx.ContainerOffset;
        EnumType = new(r, ctx.NamesTable);
        Unknown = r.Read<byte>();
        if (Unknown != 0)
        {
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown byte at {Offset}", Unknown, r.Position);
        }
        EnumValue = new(r, ctx.NamesTable);
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        EnumType.Write(w, ctx);
        w.Write(Unknown);
        EnumValue.Write(w, ctx);
        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
    }

    public override string ToString()
    {
        return EnumValue.ToString();
    }
    public override IEnumerable<Node> GetChildren()
    {
        yield break;
    }
}

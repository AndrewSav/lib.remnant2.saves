using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class EnumProperty : Node
{
    public required FName EnumType;
    public required byte Unknown;
    public required FName EnumValue;

    public EnumProperty(Node? parent, string name) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Name = name, Type = "EnumProperty" });
    }

    public EnumProperty()
    {
    }

    [SetsRequiredMembers]
    public EnumProperty(Reader r, SerializationContext ctx, Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        EnumType = new(r, ctx.NamesTable);
        Path.Add(new() { Name = EnumType.Name, Type = "EnumProperty" });
        Unknown = r.Read<byte>();
        if (Unknown != 0)
        {
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown byte at {Location}, {Offset}", Unknown, DisplayPath, r.Position);
        }
        EnumValue = new(r, ctx.NamesTable);
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        EnumType.Write(w, ctx);
        w.Write(Unknown);
        EnumValue.Write(w, ctx);
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

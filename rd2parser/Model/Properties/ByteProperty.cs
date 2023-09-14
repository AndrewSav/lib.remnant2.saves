using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class ByteProperty : Node
{
    public required FName EnumName;
    public required byte Unknown;
    public required byte? EnumByte;
    public required FName? EnumValue;

    public ByteProperty(Node? parent, string name) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Name = name, Type = "ByteProperty" });
    }

    [SetsRequiredMembers]
    public ByteProperty(Reader r, SerializationContext ctx, Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        EnumName = new(r, ctx.NamesTable);
        Path.Add(new() { Name = EnumName.Name, Type = "ByteProperty" });
        Unknown = r.Read<byte>();
        if (Unknown != 0)
        {
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown byte at {Location}, {Offset}", Unknown, DisplayPath, r.Position);
        }


        if (EnumName.Name == "None")
        {
            EnumByte = r.Read<byte>();
        }
        else
        {
            EnumValue = new(r, ctx.NamesTable);
        }
    }

    public ByteProperty()
    {
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
    }

    public override string? ToString()
    {
        return EnumName.Name == "None" ? EnumByte!.ToString() : EnumValue!.ToString();
    }
    public override IEnumerable<Node> GetChildren()
    {
        yield break;
    }
}

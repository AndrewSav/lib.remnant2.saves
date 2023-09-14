using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class TextPropertyData255 : Node
{
    public required uint Flag;
    public string? Value;

    public TextPropertyData255()
    {
    }

    public TextPropertyData255(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new () { Type = "TextPropertyData255" });
    }

    [SetsRequiredMembers]
    public TextPropertyData255(Reader r, SerializationContext ctx, Node? parent) : this(parent)
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        Flag = r.Read<uint>();
        if (Flag != 0)
        {
            Value = r.ReadFString();
        }
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        w.Write(Flag);
        if (Flag != 0)
        {
            w.WriteFString(Value!);
        }
    }
    public override IEnumerable<Node> GetChildren()
    {
        yield break;
    }
}

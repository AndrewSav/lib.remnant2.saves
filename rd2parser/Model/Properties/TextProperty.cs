using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class TextProperty : Node
{
    public required uint Flags;
    public required byte HistoryType;
    public required object Value;

    public TextProperty()
    {
    }

    public TextProperty(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "TextProperty" });
    }

    [SetsRequiredMembers]
    public TextProperty(Reader r, SerializationContext ctx, Node? parent) : this(parent)
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        Flags = r.Read<uint>();
        HistoryType = r.Read<byte>();
        Value = HistoryType switch
        {
            0 => new TextPropertyData0(r, ctx, this),
            255 => new TextPropertyData255(r, ctx, this),
            _ => throw new ApplicationException("unsupported history type")
        };
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        w.Write(Flags);
        w.Write(HistoryType);
        switch (HistoryType)
        {
            case 0:
                ((TextPropertyData0)Value).Write(w, ctx);
                break;
            case 255:
                ((TextPropertyData255)Value).Write(w, ctx);
                break;
            default:
                throw new ApplicationException("unsupported history type");
        }
    }

    public override string? ToString()
    {
        return HistoryType switch
        {
            0 => ((TextPropertyData0)Value).SourceString,
            255 => ((TextPropertyData255)Value).Value,
            _ => throw new ApplicationException("unsupported history type")
        };
    }
    public override IEnumerable<Node> GetChildren()
    {
        if (Value is Node node )
            yield return node;
    }
}

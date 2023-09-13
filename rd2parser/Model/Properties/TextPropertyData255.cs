using System.Diagnostics.CodeAnalysis;

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
    public TextPropertyData255(Reader r, Node? parent) : this(parent)
    {
        Flag = r.Read<uint>();
        if (Flag != 0)
        {
            Value = r.ReadFString();
        }
    }

    public void Write(Writer w)
    {
        w.Write(Flag);
        if (Flag != 0)
        {
            w.WriteFString(Value!);
        }
    }
}

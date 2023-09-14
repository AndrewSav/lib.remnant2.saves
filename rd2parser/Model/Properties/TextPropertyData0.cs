using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class TextPropertyData0 : Node
{
    public required string? Namespace;
    public required string? Key;
    public required string? SourceString;

    public TextPropertyData0()
    {
    }

    public TextPropertyData0(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "TextPropertyData0" });
    }

    [SetsRequiredMembers]
    public TextPropertyData0(Reader r, Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Namespace = r.ReadFString();
        Key = r.ReadFString();
        SourceString = r.ReadFString();
        Path.Add(new() { Name = Key, Type = "TextPropertyData0" });
    }

    public void Write(Writer w)
    {
        w.WriteFString(Namespace);
        w.WriteFString(Key);
        w.WriteFString(SourceString);
    }
}

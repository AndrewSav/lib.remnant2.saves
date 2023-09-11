using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model.Properties;

public class TextPropertyData0
{
    public required string? Namespace;
    public required string? Key;
    public required string? SourceString;

    public TextPropertyData0()
    {

    }

    [SetsRequiredMembers]
    public TextPropertyData0(Reader r)
    {
        Namespace = r.ReadFString();
        Key = r.ReadFString();
        SourceString = r.ReadFString();
    }

    public void Write(Writer w)
    {
        w.WriteFString(Namespace);
        w.WriteFString(Key);
        w.WriteFString(SourceString);
    }
}
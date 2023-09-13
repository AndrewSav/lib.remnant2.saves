using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model;

public class FTopLevelAssetPath
{
    public required string? Path;
    public required string? Name;

    public FTopLevelAssetPath()
    {

    }

    [SetsRequiredMembers]
    public FTopLevelAssetPath(Reader r)
    {
        Path = r.ReadFString();
        Name = r.ReadFString();
    }

    public void Write(Writer w)
    {
        w.WriteFString(Path);
        w.WriteFString(Name);
    }
}

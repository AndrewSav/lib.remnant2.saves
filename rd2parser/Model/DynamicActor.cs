using System.Diagnostics.CodeAnalysis;
using rd2parser.Model.Memory;

namespace rd2parser.Model;

public class DynamicActor
{
    public required ulong UniqueId;
    public required FTransform Transform;
    public required FTopLevelAssetPath ClassPath;

    public DynamicActor()
    {

    }
    [SetsRequiredMembers]
    public DynamicActor(Reader r)
    {
        UniqueId = r.Read<ulong>();
        Transform = r.Read<FTransform>();
        ClassPath = new(r);
    }

    public void Write(Writer w)
    {
        w.Write(UniqueId);
        w.Write(Transform);
        ClassPath.Write(w);
    }
}
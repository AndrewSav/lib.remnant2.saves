using System.Diagnostics.CodeAnalysis;
using rd2parser.Model.Memory;

namespace rd2parser.Model;

public class ActorDynamicData
{
    public required ulong UniqueId;
    public required FTransform Transform;
    public required FTopLevelAssetPath ClassPath;

    public ActorDynamicData()
    {

    }
    [SetsRequiredMembers]
    public ActorDynamicData(Reader r)
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

using System.Runtime.InteropServices;

namespace rd2parser.Model.Memory;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FTransform
{
    public FQuaternion Rotation;
    public FVector Position;
    public FVector Scale;
}

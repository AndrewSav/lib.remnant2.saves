using System.Runtime.InteropServices;

namespace rd2parser.Model.Memory;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FQuaternion
{
    public double X;
    public double Y;
    public double Z;
    public double W;
}

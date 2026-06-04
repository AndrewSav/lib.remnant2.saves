using System.Runtime.InteropServices;

namespace lib.remnant2.saves.Model.Memory;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FIntVector16
{
    public short X;
    public short Y;
    public short Z;
}

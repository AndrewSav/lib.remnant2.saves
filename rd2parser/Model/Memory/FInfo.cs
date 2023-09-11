using System.Runtime.InteropServices;

namespace rd2parser.Model.Memory;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FInfo
{
    public ulong UniqueID;
    public int Offset;
    public int Size;
}
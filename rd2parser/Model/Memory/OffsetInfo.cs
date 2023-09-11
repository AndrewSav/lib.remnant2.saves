using System.Runtime.InteropServices;

namespace rd2parser.Model.Memory;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct OffsetInfo
{
    public long Names;
    public uint Version;
    public long Objects;
}
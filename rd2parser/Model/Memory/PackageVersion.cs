using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace rd2parser.Model.Memory;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PackageVersion
{
    public uint UE4Version;
    public uint UE5Version;

    public PackageVersion(Reader r)
    {
        UE4Version = r.Read<uint>();
        if (r.GameVersion > 5)
            UE5Version = r.Read<uint>();
    }

    public void Write(Writer w)
    {
        // TODO:
    }
}

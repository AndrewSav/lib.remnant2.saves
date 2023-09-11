using Serilog;
using System.Runtime.InteropServices;

namespace rd2parser.Compression.Model;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CompressedFileHeader
{
    public uint Crc32;
    public int DecompressedSize;
    public int Version;

    public readonly void DumpDebug()
    {
        Log.Debug("Dumping CompressedFileHeader");
        Log.Debug($"Crc32 {Crc32:X8}");
        Log.Debug($"DecompressedSize {DecompressedSize}");
        Log.Debug($"Version {Version}");
        Log.Debug("--");
    }
}

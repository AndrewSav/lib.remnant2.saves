using System.Runtime.InteropServices;

namespace lib.remnant2.saves.Compression.Model;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CompressedFileHeader
{
    public uint Crc32;
    public int DecompressedSize;
    public int Version;

    public readonly void DumpDebug()
    {
        Log.Logger.Debug("Dumping CompressedFileHeader");
        Log.Logger.Debug($"Crc32 {Crc32:X8}");
        Log.Logger.Debug($"DecompressedSize {DecompressedSize}");
        Log.Logger.Debug($"Version {Version}");
        Log.Logger.Debug("--");
    }
}

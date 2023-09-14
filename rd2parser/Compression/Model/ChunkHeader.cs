using System.Runtime.InteropServices;

namespace rd2parser.Compression.Model;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChunkHeader
{
    public const ulong ArchiveHeaderTag = 0x222222229E2A83C1;
    public const byte CompressorId = 0x3;
    public const ulong FullChunkSize = 0x20000;

    public ulong HeaderTag;
    public ulong ChunkSize;
    public byte Compressor;
    public ulong CompressedSize1;
    public ulong DecompressedSize1;
    public ulong CompressedSize2;
    public ulong DecompressedSize2;

    public readonly void DumpDebug(int chunk = -1)
    {
        Log.Logger.Debug("Dumping ChunkHeader");
        if (chunk != -1)
        {
            Log.Logger.Debug($"Chunk {chunk}");
        }
        Log.Logger.Debug($"HeaderTag {HeaderTag:X8}");
        Log.Logger.Debug($"ChunkSize {ChunkSize}");
        Log.Logger.Debug($"Compressor {Compressor}");
        Log.Logger.Debug($"CompressedSize1 {CompressedSize1}");
        Log.Logger.Debug($"DecompressedSize1 {DecompressedSize1}");
        Log.Logger.Debug($"CompressedSize2 {CompressedSize2}");
        Log.Logger.Debug($"DecompressedSize2 {DecompressedSize2}");
        Log.Logger.Debug("--");

    }

    internal readonly void Validate(bool isEof, int chunkCounter)
    {
        if (HeaderTag != ArchiveHeaderTag)
        {
            throw new ApplicationException("bad chunk header tag");
        }
        if (Compressor != CompressorId)
        {
            throw new ApplicationException("bad compressor id");
        }
        if (DecompressedSize1 != DecompressedSize2)
        {
            Log.Logger.Warning("Decompressed size mismatch in chunk {chunk}: {DecompressedSize1} vs {DecompressedSize2}", chunkCounter, DecompressedSize1, DecompressedSize2);
        }
        if (CompressedSize1 != CompressedSize2)
        {
            Log.Logger.Warning("Compressed size mismatch in chunk {chunk}: {CompressedSize1} vs {CompressedSize2}", chunkCounter, CompressedSize1, CompressedSize2);
        }
        if (ChunkSize != FullChunkSize)
        {
            Log.Logger.Warning("Unexpected ChunkSize in chunk {chunk}: {ChunkSize} vs {ExpectedChunkSize}", chunkCounter, ChunkSize, FullChunkSize);
        }
        if (!isEof && ChunkSize != DecompressedSize1)
        {
            Log.Logger.Warning("Unexpected DecompressedSize1 in chunk {chunk}: {ChunkSize} vs {DecompressedSize1}", chunkCounter, ChunkSize, DecompressedSize1);
        }
        if (!isEof && ChunkSize != DecompressedSize2)
        {
            Log.Logger.Warning("Unexpected DecompressedSize2 in chunk {chunk}: {ChunkSize} vs {DecompressedSize2}", chunkCounter, ChunkSize, DecompressedSize2);
        }
    }
}

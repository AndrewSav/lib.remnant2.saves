﻿using Serilog;
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
        Log.Debug("Dumping ChunkHeader");
        if (chunk != -1)
        {
            Log.Debug($"Chunk {chunk}");
        }
        Log.Debug($"HeaderTag {HeaderTag:X8}");
        Log.Debug($"ChunkSize {ChunkSize}");
        Log.Debug($"Compressor {Compressor}");
        Log.Debug($"CompressedSize1 {CompressedSize1}");
        Log.Debug($"DecompressedSize1 {DecompressedSize1}");
        Log.Debug($"CompressedSize2 {CompressedSize2}");
        Log.Debug($"DecompressedSize2 {DecompressedSize2}");
        Log.Debug("--");

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
            Log.Warning("Decompressed size mismatch in chunk {chunk}: {DecompressedSize1} vs {DecompressedSize2}", chunkCounter, DecompressedSize1, DecompressedSize2);
        }
        if (CompressedSize1 != CompressedSize2)
        {
            Log.Warning("Compressed size mismatch in chunk {chunk}: {CompressedSize1} vs {CompressedSize2}", chunkCounter, CompressedSize1, CompressedSize2);
        }
        if (ChunkSize != FullChunkSize)
        {
            Log.Warning("Unexpected ChunkSize in chunk {chunk}: {ChunkSize} vs {ExpectedChunkSize}", chunkCounter, ChunkSize, FullChunkSize);
        }
        if (!isEof && ChunkSize != DecompressedSize1)
        {
            Log.Warning("Unexpected DecompressedSize1 in chunk {chunk}: {ChunkSize} vs {DecompressedSize1}", chunkCounter, ChunkSize, DecompressedSize1);
        }
        if (!isEof && ChunkSize != DecompressedSize2)
        {
            Log.Warning("Unexpected DecompressedSize2 in chunk {chunk}: {ChunkSize} vs {DecompressedSize2}", chunkCounter, ChunkSize, DecompressedSize2);
        }
    }
}

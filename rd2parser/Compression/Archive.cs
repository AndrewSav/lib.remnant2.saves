using System.Buffers.Binary;
using System.IO.Compression;
using System.IO.Hashing;
using rd2parser.Compression.Model;
using rd2parser.IO;
using Serilog;

namespace rd2parser.Compression;

public class Archive
{
    public static byte[] DecompressSave(string saveFile)
    {
        ReaderBase r = new(File.ReadAllBytes(saveFile));
        using var output = new MemoryStream();

        CompressedFileHeader fh = r.Read<CompressedFileHeader>();
        fh.DumpDebug();

        output.Seek(8, SeekOrigin.Current);

        int chunkCounter = 0;
        while (!r.IsEof)
        {
            ChunkHeader header = r.Read<ChunkHeader>();
            header.DumpDebug(chunkCounter);
            byte[] buffer = r.ReadBytes((int)header.CompressedSize1);
            header.Validate(r.IsEof, chunkCounter);

            using var bufferStream = new MemoryStream(buffer);
            using var decompressor = new ZLibStream(bufferStream, CompressionMode.Decompress);
            decompressor.CopyTo(output);
            chunkCounter++;
        }

        var span = new Span<byte>(output.GetBuffer());
        int saveSize = BinaryPrimitives.ReadInt32LittleEndian(span[8..]);

        if (saveSize + 12 != fh.DecompressedSize)
        {
            Log.Warning("Expected saveSize + 12 == fh.DecompressedSize, got: saveSize: {saveSize} DecompressedSize: {DecompressedSize}", saveSize, fh.DecompressedSize);
        }
        if (9 != fh.Version)
        {
            Log.Warning("Expected save version 9, got: saveSize: {Version}", fh.Version);
        }

        BinaryPrimitives.WriteUInt32LittleEndian(span, fh.Crc32);
        BinaryPrimitives.WriteInt32LittleEndian(span[4..], fh.DecompressedSize);
        BinaryPrimitives.WriteInt32LittleEndian(span[8..], fh.Version);        

        Crc32 crc32 = new();
        output.Seek(4, SeekOrigin.Begin); // first 4 bytes are crc itself, so skip
        crc32.Append(output);
        if (fh.Crc32 != BitConverter.ToUInt32(crc32.GetCurrentHash(), 0))
        {
            throw new ApplicationException("crc32 mismatch");
        }
        return output.ToArray();
    }

    public static void CompressSave(string path, byte[] data)
    {
        WriterBase w = new();
        CompressedFileHeader fh = new()
        {
            Crc32 = BitConverter.ToUInt32(data, 0),
            DecompressedSize = BitConverter.ToInt32(data, 4),
            Version = BitConverter.ToInt32(data, 8)
        };
        w.Write(fh);

        var span = new Span<byte>(data);
        BinaryPrimitives.WriteInt32LittleEndian(span[8..], fh.DecompressedSize-12);
        int current = 8;
        while (current < data.Length)
        {
            int decompressedSize = data.Length - current >= (int)ChunkHeader.FullChunkSize ? (int)ChunkHeader.FullChunkSize : data.Length - current;
            ChunkHeader header = new()
            {
                ChunkSize = ChunkHeader.FullChunkSize,
                HeaderTag = ChunkHeader.ArchiveHeaderTag,
                Compressor = ChunkHeader.CompressorId,
                DecompressedSize1 = (ulong)decompressedSize,
                DecompressedSize2 = (ulong)decompressedSize
            };
            long headerOffset = w.Position;
            w.Write(header);
            long startOffset = w.Position;
            {
                using var compressor = new ZLibStream(w.Stream, CompressionMode.Compress, true);
                compressor.Write(span[current..(current + decompressedSize)]);
            }
            long endOffset = w.Stream.Position;
            long compressedSize = w.Stream.Position - startOffset;
            w.Position = headerOffset;
            header.CompressedSize1 = (ulong)compressedSize;
            header.CompressedSize2 = (ulong)compressedSize;
            w.Write(header);
            w.Position = endOffset;
            current += decompressedSize;
        }
        File.WriteAllBytes(path,w.ToArray());
    }
}

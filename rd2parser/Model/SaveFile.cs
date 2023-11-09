using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using rd2parser.Model.Memory;
using rd2parser.Compression;
using rd2parser.IO.AddressUsageTracker;

namespace rd2parser.Model;
public class SaveFile
{
    public required FileHeader FileHeader;
    public required SaveData SaveData;

    public SaveFile()
    {

    }

    [SetsRequiredMembers]
    public SaveFile(Reader r, Options? opts = null)
    {
        r.ActivateTracker();
        FileHeader = r.Read<FileHeader>();
        SaveData = new(r: r, opts: opts);
        SortedDictionary<int, AddressRange> d = r.GetTracker().GetRanges();
        if (d.Count > 1)
        {
            Log.Logger.Warning("unexpected gaps in the the read data");
        }
        if (d[0].End != r.Position)
        {
            Log.Logger.Warning("unexpected position after read");
        }
    }

    public void Write(Writer w)
    {
        w.Write(FileHeader);
        SaveData.Write(w);
        FileHeader.DecompressedSize = (int)w.Position;
        w.Position = 0;
        w.Write(FileHeader);
        Crc32 crc32 = new();
        crc32.Append(w.ToArray()[4..]);
        FileHeader.Crc32 = BitConverter.ToUInt32(crc32.GetCurrentHash(), 0);
        w.Position = 0;
        w.Write(FileHeader);
    }

    public static SaveFile Read(string path, Options? opts = null)
    {
        byte[] b = Archive.DecompressSave(path);
        File.WriteAllBytes("debug.dec",b);
        Reader r = new(b);
        return new SaveFile(r,opts);
    }
    public static void Write(string path, SaveFile data)
    {
        Writer w = new();
        data.Write(w);
        Archive.CompressSave(path, w.ToArray());
    }

    public void VisitObjects(Action<ModelBase, int?> f)
    {
        Queue<(ModelBase obj, int? index)> q = new();
        q.Enqueue((SaveData, 0));
        while (q.Count > 0)
        {
            (ModelBase obj, int? index) = q.Dequeue();
            f(obj, index);
            foreach ((ModelBase o, int? i) in obj.GetChildren())
            {
                q.Enqueue((o, i));
            }
        }
    }
}

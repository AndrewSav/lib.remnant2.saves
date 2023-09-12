using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using rd2parser.Model.Memory;
using rd2parser.Compression;
using rd2parser.Model.Properties;

namespace rd2parser.Model;
public class SaveFile
{
    public required FileHeader FileHeader;
    public required SaveData SaveData;

    public SaveFile()
    {

    }

    [SetsRequiredMembers]
    public SaveFile(Reader r)
    {
        FileHeader = r.Read<FileHeader>();
        SaveData = new(r);
    }

    public void Write(Writer w)
    {
        w.Write(FileHeader);
        SaveData.Write(w);
        FileHeader.DecompressedSize = (int)w.Position;
        Crc32 crc32 = new();
        crc32.Append(w.ToArray()[4..]);
        FileHeader.Crc32 = BitConverter.ToUInt32(crc32.GetCurrentHash(), 0);
        w.Position = 0;
        w.Write(FileHeader);
    }

    public static SaveFile Read(string path)
    {
        byte[] b = Archive.DecompressSave(path);
        Reader r = new(b);
        return new SaveFile(r);
    }
    public static void Write(string path, SaveFile data)
    {
        Writer w = new();
        data.Write(w);
        Archive.CompressSave(path, w.ToArray());
    }

    public List<Property>? GetProperties(string name)
    {
        return SaveData.GetProperty(name);
    }

    public List<Variable>? GetVariables(string name)
    {
        return SaveData.GetVariable(name);
    }

    public Property? GetProperty(string name)
    {
        List<Property>? l =  SaveData.GetProperty(name);
        if (l == null)
        {
            return null;
        }

        if (l.Count == 1)
        {
            return l[0];
        }

        throw new InvalidOperationException("there are more than one property");
    }

    public Variable? GetVariable(string name)
    {
        List<Variable>? l = SaveData.GetVariable(name);
        if (l == null)
        {
            return null;
        }

        if (l.Count == 1)
        {
            return l[0];
        }

        throw new InvalidOperationException("there are more than one variable");
    }
}

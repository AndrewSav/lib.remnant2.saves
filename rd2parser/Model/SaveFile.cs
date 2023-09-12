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

    private static List<T>? Filter<T>(List<T>? list, string? filter) where T : Node
    {
        if (list == null) return list;
        string f = filter ?? string.Empty;
        List<T> result = list;
        foreach (string part in f.Split(','))
        {
            string p = part.Trim();
            string[] t = p.Split("=");
            if (t.Length > 2) throw new ArgumentOutOfRangeException(nameof(filter));
            if (t.Length == 1)
            {
                result = list.Where(x => x.Path.Any(y => y.Name == t[0])).ToList();
            }
            bool isInt = uint.TryParse(t[1], out uint index);
            if (isInt)
            {
                result = list.Where(x => x.Path.Any(y => y.Type == t[0] && y.Index == index)).ToList();
            } else
            {
                result = list.Where(x => x.Path.Any(y => y.Type == t[0] && y.Name == t[1])).ToList();
            }
        }

        return result;
    }

    public List<Property>? GetProperties(string name, string? filter = null)
    {
        return Filter(SaveData.GetProperty(name),filter);
    }

    public List<Variable>? GetVariables(string name, string? filter = null)
    {
        return Filter(SaveData.GetVariable(name),filter);
    }

    public Property? GetProperty(string name, string? filter = "")
    {
        List<Property>? l = GetProperties(name, filter); ;
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

    public Variable? GetVariable(string name, string? filter = "")
    {
        List<Variable>? l = GetVariables(name, filter);
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
    public List<string> GetPropertyNames()
    {
        return SaveData.GetPropertyNames();
    }

    public List<string> GetVariableNames()
    {
        return SaveData.GetVariableNames();
    }

    public List<string> GetPropertyTypes()
    {
        return SaveData.GetPropertyTypes();
    }

    public List<string> GetVariableTypes()
    {
        return SaveData.GetVariableTypes();
    }
}

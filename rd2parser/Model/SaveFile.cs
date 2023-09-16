using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using rd2parser.Model.Memory;
using rd2parser.Compression;
using rd2parser.IO.AddressUsageTracker;
using rd2parser.Model.Properties;
using rd2parser.Navigation;

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
            Log.Logger.Warning("unexpected unexpected position after read");
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

    // Filter is "part,part,part,..."
    // part is one of the following:
    // - name
    // - object=name
    // - object=index
    // - object=name:index
    private static List<T>? Filter<T>(List<T>? list, string? filter) where T : Node
    {
        if (list == null) return list;
        string f = filter ?? string.Empty;
        List<T> result = list;
        Dictionary<T, List<Segment>> segments = list.ToDictionary(x=> x, x=>new List<Segment>(x.Path));
        foreach (string part in f.Split(','))
        {
            string p = part.Trim();
            string[] t = p.Split("=");
            switch (t.Length)
            {
                case > 2:
                    throw new ArgumentOutOfRangeException(nameof(filter));
                case 1:
                {
                    string name = t[0].Trim();
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    result = RemoveUsedSegments(result,y => y.Name == name || y.Type == name);
                    continue;
                }
            }

            bool isInt = uint.TryParse(t[1].Trim(), out uint index);
            string type = t[0].Trim();
            if (isInt)
            {
                result = result.Where(x => segments[x].Any(y => y.Type == type && y.Index == index)).ToList();
                result = RemoveUsedSegments(result, y => y.Type == type && y.Index == index);
            }
            else
            {
                string name = t[1].Trim();
                string[] ni = name.Split(":");
                switch (ni.Length)
                {
                    case > 2:
                        throw new ArgumentOutOfRangeException(nameof(filter));
                    case 1:
                        result = result.Where(x => segments[x].Any(y => y.Type == type && y.Name == name)).ToList();
                        result = RemoveUsedSegments(result, y => y.Type == type && y.Name == name);
                        continue;
                }

                name = ni[0].Trim();
                if (!uint.TryParse(ni[1].Trim(), out index))
                {
                    throw new ArgumentOutOfRangeException(nameof(filter));
                }
                result = result.Where(x => segments[x].Any(y => y.Type == type && y.Name == name && y.Index == index)).ToList();
                result = RemoveUsedSegments(result,y => y.Type == type && y.Name == name && y.Index == index);
            }

            continue;

            List<T> RemoveUsedSegments(List<T> r, Func<Segment, bool> func)
            {
                r = r.Where(x => segments[x].Any(func)).ToList();
                foreach (T node in r)
                {
                    Segment? elem = segments[node].FirstOrDefault(func);
                    if (elem != null)
                    {
                        segments[node].Remove(elem);
                    }
                }
                return r;
            }
        }

        return result;
    }

    public List<T>? GetRegistryItems<T>(string name, string? filter = null) where T : Node
    {
        return Filter(SaveData.GetRegistryItem<T>(name), filter);
    }

    public T? GetRegistryItem<T>(string name, string? filter = "") where T : Node
    {
        List<T>? l = GetRegistryItems<T>(name, filter);
        if (l == null || l.Count == 0)
        {
            return null;
        }

        if (l.Count == 1)
        {
            return l[0];
        }

        throw new InvalidOperationException("there are more than one property");
    }


    public List<T>? GetProperties<T>(string name, string? filter = null)
    {
        List<Property>? list = Filter(SaveData.GetRegistryItem<Property>(name), filter);
        return list?.Select(x => (T)x.Value!).ToList();
    }

    public List<Property>? GetProperties(string name, string? filter = null)
    {
        return GetRegistryItems<Property>(name, filter);
    }

    public List<Property>? GetAllProperties()
    {
        return SaveData.GetAllRegistryItem<Property>();
    }

    public Property? GetProperty(string name, string? filter = null)
    {
        return GetRegistryItem<Property>(name, filter);
    }

    public List<T>? GetVariables<T>(string name, string? filter = null)
    {
        List<Variable>? list = Filter(SaveData.GetRegistryItem<Variable>(name), filter);
        return list?.Select(x => (T)x.Value!).ToList();
    }

    public List<Variable>? GetVariables(string name, string? filter = null)
    {
        return GetRegistryItems<Variable>(name, filter);
    }

    public List<Variable>? GetAllVariables()
    {
        return SaveData.GetAllRegistryItem<Variable>();
    }

    public Variable? GetVariable(string name, string? filter = null)
    {
        return GetRegistryItem<Variable>(name, filter);
    }

    public List<Actor>? GetActors(string name, string? filter = null)
    {
        return GetRegistryItems<Actor>(name, filter);
    }

    public List<Actor>? GetAllActors()
    {
        return SaveData.GetAllRegistryItem<Actor>();
    }
    public Actor? GetActor(string name, string? filter = null)
    {
        return GetRegistryItem<Actor>(name, filter);
    }
    
    public List<UObject>? GetObjects(string name, string? filter = null)
    {
        return GetRegistryItems<UObject>(name, filter);
    }

    public List<UObject>? GetAllObjects()
    {
        return SaveData.GetAllRegistryItem<UObject>();
    }

    public UObject? GetObject(string name, string? filter = null)
    {
        return GetRegistryItem<UObject>(name, filter);
    }



    public void VisitObjects(Action<Node> f)
    {
        Queue<Node> q = new();
        q.Enqueue(SaveData);
        while (q.Count > 0)
        {
            Node n = q.Dequeue();
            f(n);
            foreach (Node c in n.GetChildren())
            {
                q.Enqueue(c);
            }
        }
    }
}

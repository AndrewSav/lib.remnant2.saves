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

    public List<T>? GetRegistryItems<T>(string name, Node? parent = null) where T : Node
    {
        return Filter(SaveData.GetRegistryItem<T>(name), parent);
    }

    public List<T>? FindRegistryItems<T>(string namePattern, Node? parent = null) where T : Node
    {
        return Filter(SaveData.FindRegistryItem<T>(namePattern), parent);
    }

    public T? GetRegistryItem<T>(string name, Node? parent = null) where T : Node
    {
        List<T>? l = GetRegistryItems<T>(name, parent);
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

    private static List<T>? Filter<T>(List<T>? items, Node? parent) where T : Node
    {
        if (items == null) return null;
        if (parent == null) return items;

        static bool IsParent(List<Segment> obj, List<Segment> filter)
        {
            if (filter.Count > obj.Count) return false;
            for (int i = 0; i < filter.Count; i++)
            {
                if (obj[i] != filter[i]) return false;
            }
            return true;
        }
        return items.Where(x => IsParent(x.Path,parent.Path)).ToList();
    }

    public List<T>? GetProperties<T>(string name, Node? parent = null)
    {
        List<Property>? list = Filter(SaveData.GetRegistryItem<Property>(name), parent);
        return list?.Select(x => (T)x.Value!).ToList();
    }
    
    public List<Property>? GetProperties(string name, Node? parent = null)
    {
        return GetRegistryItems<Property>(name, parent);
    }

    public List<Property>? FindProperties(string namePattern, Node? parent = null)
    {
        return FindRegistryItems<Property>(namePattern, parent);
    }

    public List<Property>? GetAllProperties()
    {
        return SaveData.GetAllRegistryItem<Property>();
    }

    public Property? GetProperty(string name, Node? parent = null)
    {
        return GetRegistryItem<Property>(name, parent);
    }

    public List<T>? GetVariables<T>(string name, Node? parent = null)
    {
        List<Variable>? list = Filter(SaveData.GetRegistryItem<Variable>(name), parent);
        return list?.Select(x => (T)x.Value!).ToList();
    }

    public List<Variable>? GetVariables(string name, Node? parent = null)
    {
        return GetRegistryItems<Variable>(name, parent);
    }

    public List<Variable>? FindVariables(string namePattern, Node? parent = null)
    {
        return FindRegistryItems<Variable>(namePattern, parent);
    }
    public List<Variable>? GetAllVariables()
    {
        return SaveData.GetAllRegistryItem<Variable>();
    }

    public Variable? GetVariable(string name, Node? parent = null)
    {
        return GetRegistryItem<Variable>(name, parent);
    }

    public List<Actor>? GetActors(string name, Node? parent = null)
    {
        return GetRegistryItems<Actor>(name, parent);
    }

    public List<Actor>? FindActors(string namePattern, Node? parent = null)
    {
        return FindRegistryItems<Actor>(namePattern, parent);
    }
    public List<Actor>? GetAllActors()
    {
        return SaveData.GetAllRegistryItem<Actor>();
    }
    public Actor? GetActor(string name, Node? parent = null)
    {
        return GetRegistryItem<Actor>(name, parent);
    }
    
    public List<UObject>? GetObjects(string name, Node? parent = null)
    {
        return GetRegistryItems<UObject>(name, parent);
    }

    public List<UObject>? FindObjects(string namePattern, Node? parent = null)
    {
        return FindRegistryItems<UObject>(namePattern, parent);
    }
    public List<UObject>? GetAllObjects()
    {
        return SaveData.GetAllRegistryItem<UObject>();
    }

    public UObject? GetObject(string name, Node? parent = null)
    {
        return GetRegistryItem<UObject>(name, parent);
    }

    public List<Component>? GetComponents(string name, Node? parent = null)
    {
        return GetRegistryItems<Component>(name, parent);
    }
    public List<Component>? FindComponents(string namePattern, Node? parent = null)
    {
        return FindRegistryItems<Component>(namePattern, parent);
    }
    public List<Component>? GetAllComponents()
    {
        return SaveData.GetAllRegistryItem<Component>();
    }
    public Component? GetComponent(string name, Node? parent = null)
    {
        return GetRegistryItem<Component>(name, parent);
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

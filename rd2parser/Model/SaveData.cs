using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using rd2parser.Model.Memory;
using rd2parser.Model.Properties;
using rd2parser.Navigation;

namespace rd2parser.Model;

public class SaveData : Node
{
    // The members order roughly correspond to the order of data in a save file
    // after objects offset object properties and components go, one set for
    // each object in the following objects table
    public PackageVersion? PackageVersion;
    public FTopLevelAssetPath? SaveGameClassPath;
    public required long NameTableOffset;
    public required uint Version;
    public required long ObjectsOffset;
    public required List<UObject> Objects;
    public required List<string> NamesTable;

    // The next two is for navigation
    [JsonIgnore]
    private readonly ItemRegistry _registry;

    public SaveData() :base(null, new())
    {
        _registry = new();
    }

    [SetsRequiredMembers]
    public SaveData(Reader r, Node? parent = null, bool hasPackageVersion = true, bool hasTopLevelAssetPath = true, SerializationContext? oldCtx = null, int containerOffset = 0, Options? opts = null) :
        base(parent, parent?.Path ?? new())
    {
        ReadOffset = r.Position + containerOffset;
        Path.Add(new(){Type = "SaveData"});
        if (hasPackageVersion) PackageVersion = new PackageVersion(r);

        if (hasTopLevelAssetPath) SaveGameClassPath = new FTopLevelAssetPath(r);

        OffsetInfo oi = r.Read<OffsetInfo>();

        NameTableOffset = oi.Names;
        Version = oi.Version;
        ObjectsOffset = oi.Objects;

        int objectsDataOffset = r.Position;

        int maxPosition = r.Position;
        r.Position = (int)NameTableOffset;

        int len = r.Read<int>();
        NamesTable = new();
        for (int i = 0; i < len; i++)
        {
            string name = r.ReadFString() ?? throw new ApplicationException("unexpected null entry in names table");
            NamesTable.Add(name);
        }

        SerializationContext ctx = new()
        {
            NamesTable = NamesTable,
            ClassPath = SaveGameClassPath?.Path,
            ContainerOffset = containerOffset,
            Options = opts
        };

        maxPosition = int.Max(maxPosition, r.Position);
        r.Position = (int)ObjectsOffset;

        int numUniqueObjects = r.Read<int>();
        Objects = new();
        for (int i = 0; i < numUniqueObjects; i++)
        {
            UObject o = new(r, ctx, i, this);
            Objects.Add(o);
        }

        ctx.Objects = Objects;

        maxPosition = int.Max(maxPosition, r.Position);
        r.Position = objectsDataOffset;

        foreach (UObject o in Objects)
        {
            o.ReadData(r, ctx);
        }

        maxPosition = int.Max(maxPosition, r.Position);
        r.Position = maxPosition;

        _registry = ctx.Registry;
        if (oldCtx == null) return;
        oldCtx.Registry.Add(_registry);
    }

    public void Write(Writer w, int containerOffset = 0)
    {
        WriteOffset = (int)w.Position + containerOffset;

        if (PackageVersion != null)
        {
            w.Write(PackageVersion.Value);
        }

        SaveGameClassPath?.Write(w);

        OffsetInfo offsetInfo = new()
        {
            Version = Version
        };
        // We do not know offsets yet, we will need to patch this later
        long offsetPosition = w.Position;
        w.Write(offsetInfo);

        SerializationContext ctx = new()
        {
            NamesTable = NamesTable,
            ClassPath = SaveGameClassPath?.Path,
            Objects = Objects, // for ObjectProperty
            ContainerOffset = containerOffset
        };

        foreach (UObject o in Objects)
        {
            // Save will fail if this link is broken: object will not know their indices
            // This can happen when deserializing from json or because of save editing
            o.Parent = this;
        }

        foreach (UObject o in Objects)
        {
            o.WriteData(w, ctx);
        }

        offsetInfo.Objects = w.Position;
        w.Write(Objects.Count);
        foreach (UObject o in Objects)
        {
            o.Write(w, ctx);
        }

        offsetInfo.Names = w.Position;
        w.Write(NamesTable.Count);
        foreach (string? item in NamesTable)
        {
            w.WriteFString(item);
        }

        long endOffset = w.Position;
        w.Position = offsetPosition;
        w.Write(offsetInfo);
        w.Position = endOffset;
    }

    public List<T>? GetRegistryItem<T>(string name) where T : Node
    {
        return _registry.Get<T>(name);
    }

    public List<T>? GetAllRegistryItem<T>() where T : Node
    {
        return _registry.GetAll<T>();
    }
    public List<T>? FindRegistryItem<T>(string namePattern) where T : Node
    {
        return _registry.Find<T>(namePattern);
    }

    public override IEnumerable<Node> GetChildren()
    {
        foreach (UObject o in Objects)
        {
                yield return o;
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using rd2parser.Model.Memory;
using rd2parser.Model.Properties;
using rd2parser.Navigation;

namespace rd2parser.Model;

// The members order roughly correspond to the order of data in a save file
// after objects offset object properties and components go, one set for
// each object in the following objects table
public class SaveData : Node
{
    public PackageVersion? PackageVersion;
    public FTopLevelAssetPath? SaveGameClassPath;
    public required long NameTableOffset;
    public required uint Version;
    public required long ObjectsOffset;
    public required List<UObject> Objects;
    public required List<string> NamesTable;

    [JsonIgnore]
    private readonly ItemRegistry<Property> _propertyRegistry;
    [JsonIgnore]
    private readonly ItemRegistry<Variable> _variableRegistry;

    public SaveData() :base(null, new())
    {
        _propertyRegistry = new();
        _variableRegistry = new();
    }

    [SetsRequiredMembers]
    public SaveData(Reader r, Node? parent = null, bool hasPackageVersion = true, bool hasTopLevelAssetPath = true, SerializationContext? oldCtx = null) :
        base(parent, parent?.Path ?? new())
    {
        Path.Add(new(){Type = "SaveData"});
        if (hasPackageVersion) PackageVersion = r.Read<PackageVersion>();

        if (hasTopLevelAssetPath) SaveGameClassPath = new FTopLevelAssetPath(r);

        OffsetInfo oi = r.Read<OffsetInfo>();

        NameTableOffset = oi.Names;
        Version = oi.Version;
        ObjectsOffset = oi.Objects;

        int objectsDataOffset = r.Position;


        int maxPosition = r.Position;
        r.Position = (int)NameTableOffset;
        NamesTable = ReadNameTable(r);
        SerializationContext ctx = new()
        {
            NamesTable = NamesTable,
            ClassPath = SaveGameClassPath?.Path
        };

        maxPosition = int.Max(maxPosition, r.Position);
        r.Position = (int)ObjectsOffset;

        ctx.Objects = Objects = ReadObjects(r, ctx);

        maxPosition = int.Max(maxPosition, r.Position);
        r.Position = objectsDataOffset;

        for (int i = 0; i < Objects.Count; i++)
        {
            int objIndex = r.Read<int>();
            if (i != objIndex)
            {
                throw new ApplicationException("unexpected object index");
            }
            (Objects[objIndex].Properties, Objects[objIndex].ExtraPropertiesData) = ReadProperties(r, ctx,Objects[objIndex]);

            Objects[objIndex].IsActor = r.Read<byte>();
            if (Objects[objIndex].IsActor != 0)
                Objects[objIndex].Components = ReadComponents(r, ctx);
        }

        _propertyRegistry = ctx.PropertyRegistry;
        _variableRegistry = ctx.VariableRegistry;

        maxPosition = int.Max(maxPosition, r.Position);
        r.Position = maxPosition;
        if (oldCtx == null) return;
        oldCtx.PropertyRegistry.Add(_propertyRegistry);
        oldCtx.VariableRegistry.Add(_variableRegistry);
    }

    public List<Property>? GetProperty(string name)
    {
        return _propertyRegistry.Get(name);
    }

    public List<Variable>? GetVariable(string name)
    {
        return _variableRegistry.Get(name);
    }

    public List<string> GetPropertyNames()
    {
        return _propertyRegistry.GetNames();
    }

    public List<string> GetVariableNames()
    {
        return _variableRegistry.GetNames();
    }

    public List<string> GetPropertyTypes()
    {
        return _propertyRegistry.GetTypes();
    }

    public List<string> GetVariableTypes()
    {
        return _variableRegistry.GetTypes();
    }

    private List<UObject> ReadObjects(Reader r, SerializationContext ctx)
    {
        List<UObject> result = new();
        int numUniqueObjects = r.Read<int>();

        for (int i = 0; i < numUniqueObjects; i++)
        {
            UObject o = ReadObject(r, ctx, i);
            result.Add(o);
        }

        return result;
    }

    private UObject ReadObject(Reader r, SerializationContext ctx, int index)
    {
        UObject result = new(this)
        {
            WasLoadedByte = r.Read<byte>()
        };
        result.Path[^1].Index = index;
        bool wasLoaded = result.WasLoadedByte != 0;
        result.ObjectPath = wasLoaded && index == 0 && ctx.ClassPath != null ? ctx.ClassPath : r.ReadFString();

        if (wasLoaded) return result;
        result.LoadedData = new UObjectLoadedData
        {
            Name = new(r, ctx.NamesTable),
            OuterId = r.Read<uint>()
        };
        if (!string.IsNullOrWhiteSpace(result.LoadedData.Name.Name))
        {
            result.Path[^1].Name = result.LoadedData.Name.Name;
        }

        return result;
    }

    private static (PropertyBag?, byte[]?) ReadProperties(Reader r, SerializationContext ctx, Node? parent)
    {
        byte[]? extraData = null;
        uint len = r.Read<uint>();
        int start = r.Position;
        PropertyBag? result = null;
        if (len <= 0) return (result, extraData);
        result = new PropertyBag(r,ctx, parent);
        // After each property we can always observe either 4 ot 8 zeroes
        if (r.Position == (int)(start + len)) return (result, extraData);
        if (r.Position > (int)(start + len))
            throw new ApplicationException("ReadProperties read too much data unexpectedly");

        extraData = r.ReadBytes((int)(start + len) - r.Position);

        if (extraData.Any(x => x != 0))
        {
            string debug = BitConverter.ToString(extraData);
            Log.Logger.Warning("unexpected non-zero extra data while reading properties at {Location}, {Offset:x8}",parent?.DisplayPath, r.Position);
            Log.Logger.Debug(debug);
        }

        return (result, extraData);
    }

    private List<Component> ReadComponents(Reader r, SerializationContext ctx)
    {
        List<Component> result = new();
        uint componentCount = r.Read<uint>();
        for (int i = 0; i < componentCount; i++)
        {
            string componentKey = r.ReadFString() ?? throw new ApplicationException("unexpected null component key");
            Component c = new(this, componentKey);
            c.Path[^1].Index = i;
            int len = r.Read<int>();

            int start = r.Position;
            switch (c.ComponentKey)
            {
                case "GlobalVariables":
                case "Variables":
                case "Variable":
                case "PersistenceKeys":
                case "PersistanceKeys1":
                case "PersistenceKeys1":
                    c.Variables = new Variables(r, ctx, c);
                    break;
                default:
                    c.Properties = new PropertyBag(r, ctx, c);
                    break;
            }

            // After some components we can observe 8 zeroes
            if (r.Position != start + len)
            {
                if (r.Position > start + len)
                    throw new ApplicationException("ReadComponents read too much data unexpectedly");
                c.ExtraComponentsData = r.ReadBytes(start + len - r.Position);
                if (c.ExtraComponentsData.Any(x => x != 0))
                {
                    Node child = c.Variables as Node ?? c.Properties!;

                    string debug = BitConverter.ToString(c.ExtraComponentsData);
                    Log.Logger.Warning("unexpected non-zero extra data while reading components at {Location}, {Offset:x8}", child.DisplayPath, r.Position);
                    Log.Logger.Debug(debug);
                }
            }

            result.Add(c);
        }

        return result;
    }

    private static List<string> ReadNameTable(Reader r)
    {
        int len = r.Read<int>();
        List<string> result = new();
        for (int i = 0; i < len; i++)
        {
            string name = r.ReadFString() ?? throw new ApplicationException("unexpected null entry in names table");
            result.Add(name);
        }
        return result;
    }

    public void Write(Writer w)
    {
        if (PackageVersion != null)
        {
            w.Write(PackageVersion.Value);
        }

        SaveGameClassPath?.Write(w);

        OffsetInfo oi = new()
        {
            Version = Version
        };
        // We do not know offsets yet, we will need to patch this later
        long offsetPosition = w.Position;
        w.Write(oi);

        SerializationContext ctx = new()
        {
            NamesTable = NamesTable,
            ClassPath = SaveGameClassPath?.Path,
            Objects = Objects
        };

        foreach (UObject o in Objects)
        {
            o.Parent = this;
        }

        for (int i = 0; i < Objects.Count; i++)
        {
            UObject o = Objects[i];
            w.Write(i);
            WriteProperties(w, ctx, o);
            w.Write(o.IsActor);
            if (o.IsActor != 0)
            {
                WriteComponents(w, ctx,o);
            }
        }

        oi.Objects = w.Position;
        WriteObjects(w,ctx);
        oi.Names = w.Position;
        WriteNameTable(w);
        long endOffset = w.Position;
        w.Position = offsetPosition;
        w.Write(oi);
        w.Position = endOffset;

    }

    public void WriteNameTable(Writer w)
    {
        w.Write(NamesTable.Count);
        foreach (string? item in NamesTable)
        {
            w.WriteFString(item);
        }
    }

    private void WriteObjects(Writer w, SerializationContext ctx)
    {
        w.Write(Objects.Count);
        foreach (UObject o in Objects)
        {
            w.Write(o.WasLoadedByte);
            if (o.WasLoadedByte == 0 || o.ObjectIndex != 0 || SaveGameClassPath == null)
            {
                w.WriteFString(o.ObjectPath);
            }

            if (o is not { WasLoadedByte: 0, LoadedData: not null }) continue;
            o.LoadedData.Name.Write(w, ctx);
            w.Write(o.LoadedData.OuterId);
        }
    }

    private static void WriteProperties(Writer w, SerializationContext ctx, UObject o)
    {
        long lengthOffset = w.Position;
        w.Write(0); // we will patch this later
        if (o.Properties == null)
        {
            return;
        }
        long startOffset = w.Position;
        o.Properties.Write(w,ctx);
        if (o.ExtraPropertiesData != null)
        {
            w.WriteBytes(o.ExtraPropertiesData);
        }
        long endOffset = w.Position;
        int len = (int)(endOffset - startOffset);
        w.Position = lengthOffset;
        w.Write(len);
        w.Position = endOffset;
    }

    private static void WriteComponents(Writer w, SerializationContext ctx, UObject o)
    {
        w.Write(o.Components!.Count);
        foreach (Component c in o.Components)
        {
            w.WriteFString(c.ComponentKey);
            long lengthOffset = w.Position;
            w.Write(0); // we will patch this later
            long startOffset = w.Position;
            switch (c.ComponentKey)
            {
                case "GlobalVariables":
                case "Variables":
                case "Variable":
                case "PersistenceKeys":
                case "PersistanceKeys1":
                case "PersistenceKeys1":
                    c.Variables!.Write(w,ctx);
                    break;
                default:
                    c.Properties!.Write(w,ctx);
                    break;
            }
            if (c.ExtraComponentsData != null)
            {
                w.WriteBytes(c.ExtraComponentsData);
            }
            long endOffset = w.Position;
            int len = (int)(endOffset - startOffset);
            w.Position = lengthOffset;
            w.Write(len);
            w.Position = endOffset;
        }
    }

}

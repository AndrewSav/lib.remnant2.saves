﻿using System.Diagnostics.CodeAnalysis;
using rd2parser.Model.Memory;
using rd2parser.Model.Properties;

namespace rd2parser.Model;

// The members order roughly correspond to the order of data in a save file
// after objects offset object properties and components go, one set for
// each object in the following objects table
public class SaveData
{
    public PackageVersion? PackageVersion;
    public FTopLevelAssetPath? SaveGameClassPath;
    public required long NameTableOffset;
    public required uint Version;
    public required long ObjectsOffset;
    public required List<UObject> Objects;
    public required List<string> NamesTable;

    public SaveData()
    {
    }

    [SetsRequiredMembers]
    public SaveData(Reader r, bool hasPackageVersion = true, bool hasTopLevelAssetPath = true)
    {
        if (hasPackageVersion) PackageVersion = r.Read<PackageVersion>();

        if (hasTopLevelAssetPath) SaveGameClassPath = new FTopLevelAssetPath(r);

        var oi = r.Read<OffsetInfo>();

        NameTableOffset = oi.Names;
        Version = oi.Version;
        ObjectsOffset = oi.Objects;

        int objectsDataOffset = r.Position;

        r.Position = (int)NameTableOffset;
        NamesTable = ReadNameTable(r);
        SerializationContext ctx = new()
        {
            NamesTable = NamesTable,
            ClassPath = SaveGameClassPath?.Path
        };

        r.Position = (int)ObjectsOffset;

        ctx.Objects = Objects = ReadObjects(r, ctx);

        r.Position = objectsDataOffset;

        for (int i = 0; i < Objects.Count; i++)
        {
            int objIndex = r.Read<int>();
            if (i != objIndex)
            {
                throw new ApplicationException("unexpected object index");
            }
            (Objects[objIndex].Properties, Objects[objIndex].ExtraPropertiesData) = ReadProperties(r, ctx);
            Objects[objIndex].IsActor = r.Read<byte>();
            if (Objects[objIndex].IsActor != 0)
                Objects[objIndex].Components = ReadComponents(r, ctx);
        }
    }

    private static List<UObject> ReadObjects(Reader r, SerializationContext ctx)
    {
        List<UObject> result = new();
        int numUniqueObjects = r.Read<int>();

        for (int i = 0; i < numUniqueObjects; i++)
        {
            UObject o = ReadObject(r, ctx, i);
            o.Parent = result;
            result.Add(o);
        }

        return result;
    }

    private static UObject ReadObject(Reader r, SerializationContext ctx, int objectId)
    {
        UObject result = new()
        {
            WasLoadedByte = r.Read<byte>(),
        };
        bool wasLoaded = result.WasLoadedByte != 0;
        result.ObjectPath = wasLoaded && objectId == 0 && ctx.ClassPath != null ? ctx.ClassPath : r.ReadFString();

        if (!wasLoaded)
            result.LoadedData = new UObjectLoadedData
            {
                Name = new(r, ctx.NamesTable),
                OuterId = r.Read<uint>()
            };

        return result;
    }

    private static (List<KeyValuePair<string, Property>>?, byte[]?) ReadProperties(Reader r, SerializationContext ctx)
    {
        byte[]? extraData = null;
        uint len = r.Read<uint>();
        int start = r.Position;
        List<KeyValuePair<string, Property>>? result = null;
        if (len > 0)
        {
            result = r.ReadProperties(ctx);
            // After each property we can always observe either 4 ot 8 zeroes
            if (r.Position != (int)(start + len))
            {
                if (r.Position > (int)(start + len))
                    throw new ApplicationException("ReadProperties read too much data unexpectedly");

                extraData = r.ReadBytes((int)(start + len) - r.Position);
            }
        }

        return (result, extraData);
    }

    private static List<Component> ReadComponents(Reader r, SerializationContext ctx)
    {
        List<Component> result = new();
        uint componentCount = r.Read<uint>();
        for (int i = 0; i < componentCount; i++)
        {
            Component c = new()
            {
                ComponentKey = r.ReadFString() ?? throw new ApplicationException("unexpected null component key")
            };
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
                    c.Variables = new Variables(r, ctx);
                    break;
                default:
                    c.Properties = r.ReadProperties(ctx);
                    break;
            }

            // After some components we can observe 8 zeroes
            if (r.Position != start + len)
            {
                if (r.Position > start + len)
                    throw new ApplicationException("ReadComponents read too much data unexpectedly");
                c.ExtraComponentsData = r.ReadBytes(start + len - r.Position);
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

        foreach (var o in Objects)
        {
            o.Parent = Objects;
        }

        for (int i = 0; i < Objects.Count; i++)
        {
            var o = Objects[i];
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
        foreach (var o in Objects)
        {
            w.Write(o.WasLoadedByte);
            if (o.WasLoadedByte == 0 || o.ObjectIndex != 0 || SaveGameClassPath == null)
            {
                w.WriteFString(o.ObjectPath);
            }

            if (o is { WasLoadedByte: 0, LoadedData: not null })
            {
                o.LoadedData.Name.Write(w, ctx);
                w.Write(o.LoadedData.OuterId);
            }
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
        w.WriteProperties(ctx,o.Properties);
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
                    w.WriteProperties(ctx,c.Properties!);
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
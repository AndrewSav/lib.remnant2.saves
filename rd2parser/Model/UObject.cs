using rd2parser.Model.Properties;
using rd2parser.Navigation;

namespace rd2parser.Model;

public class UObject : Node
{
    public byte WasLoadedByte;
    public string? ObjectPath;
    public UObjectLoadedData? LoadedData;
    public PropertyBag? Properties;
    public byte[]? ExtraPropertiesData;
    public List<Component>? Components;
    public byte IsActor;
    public int ObjectIndex;

    public UObject()
    {
    }

    public UObject(Reader r, SerializationContext ctx, int index)
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        WasLoadedByte = r.Read<byte>();
    
        bool wasLoaded = WasLoadedByte != 0;
        ObjectPath = wasLoaded && index == 0 && ctx.ClassPath != null ? ctx.ClassPath : r.ReadFString();

        if (wasLoaded) return;
        LoadedData = new UObjectLoadedData
        {
            Name = new(r, ctx.NamesTable),
            OuterId = r.Read<uint>()
        };
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset; // Does not include object data, which is saved separately
    }

    internal void ReadData(Reader r, SerializationContext ctx)
    {
        ObjectIndex = r.Read<int>();
        (Properties, ExtraPropertiesData) = ReadProperties(r, ctx);
        IsActor = r.Read<byte>();
        if (IsActor != 0)
            Components = ReadComponents(r, ctx);
    }

    public (PropertyBag?, byte[]?) ReadProperties(Reader r, SerializationContext ctx)
    {
        byte[]? extraData = null;
        uint len = r.Read<uint>();
        int start = r.Position;
        PropertyBag? result = null;
        if (len <= 0) return (result, extraData);
        result = new PropertyBag(r, ctx);
        // After each property we can always observe either 4 ot 8 zeroes
        if (r.Position == (int)(start + len)) return (result, extraData);
        if (r.Position > (int)(start + len))
            throw new ApplicationException("ReadProperties read too much data unexpectedly");

        extraData = r.ReadBytes((int)(start + len) - r.Position);

        if (extraData.Any(x => x != 0))
        {
            string debug = BitConverter.ToString(extraData);
            Log.Logger.Warning("unexpected non-zero extra data while reading properties at {Offset:x8}", r.Position);
            Log.Logger.Debug(debug);
        }

        return (result, extraData);
    }

    public List<Component> ReadComponents(Reader r, SerializationContext ctx)
    {
        List<Component> result = new();
        uint componentCount = r.Read<uint>();
        for (int i = 0; i < componentCount; i++)
        {
            int readOffset = r.Position + ctx.ContainerOffset;
            string componentKey = r.ReadFString() ?? throw new ApplicationException("unexpected null component key");
            Component c = new(componentKey)
            {
                ReadOffset = readOffset
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
                    c.Properties = new PropertyBag(r, ctx);
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
                    string debug = BitConverter.ToString(c.ExtraComponentsData);
                    Log.Logger.Warning("unexpected non-zero extra data while reading components at {Offset:x8}", r.Position);
                    Log.Logger.Debug(debug);
                }
            }

            result.Add(c);
            c.ReadLength = r.Position + ctx.ContainerOffset - c.ReadOffset;
        }

        return result;
    }

    internal void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        w.Write(WasLoadedByte);
        if (WasLoadedByte == 0 || ObjectIndex != 0 || ctx.ClassPath == null)
        {
            w.WriteFString(ObjectPath);
        }

        if (this is not { WasLoadedByte: 0, LoadedData: not null }) return;
        LoadedData.Name.Write(w, ctx);
        w.Write(LoadedData.OuterId);
        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
    }

    internal void WriteData(Writer w, SerializationContext ctx)
    {
        w.Write(ObjectIndex);
        WriteProperties(w, ctx);
        w.Write(IsActor);
        if (IsActor != 0)
        {
            WriteComponents(w, ctx);
        }
    }

    public void WriteProperties(Writer w, SerializationContext ctx)
    {
        long lengthOffset = w.Position;
        w.Write(0); // we will patch this later
        if (Properties == null)
        {
            return;
        }
        long startOffset = w.Position;
        Properties.Write(w, ctx);
        if (ExtraPropertiesData != null)
        {
            w.WriteBytes(ExtraPropertiesData);
        }
        long endOffset = w.Position;
        int len = (int)(endOffset - startOffset);
        w.Position = lengthOffset;
        w.Write(len);
        w.Position = endOffset;
    }

    public void WriteComponents(Writer w, SerializationContext ctx)
    {
        w.Write(Components!.Count);
        foreach (Component c in Components)
        {
            c.WriteOffset = (int)w.Position + ctx.ContainerOffset;
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
                    c.Variables!.Write(w, ctx);
                    break;
                default:
                    c.Properties!.Write(w, ctx);
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
            c.WriteLength = (int)w.Position + ctx.ContainerOffset - c.WriteOffset; // Does not include object data, which is saved separately
        }
    }

    // To make it easier to navigate in the debugger
    public string? Name => LoadedData?.Name.Name;

    // To make it easier to navigate in the debugger
    public override string? ToString()
    {
        return Key ?? ObjectPath;
    }

    // To make it easier to navigate in the debugger
    public string? Key
    {
        get
        {
            if (Properties is { Properties.Count: > 0 } && Properties.Properties[0].Key == "Key")
            {
                return Properties.Properties[0].ToString();
            }

            return null;
        }
    }

    public override IEnumerable<Node> GetChildren()
    {
        if (Properties != null)
            yield return Properties;

        if (Components != null)
        {
            foreach (Component c in Components)
            {
                    yield return c;
            }
        }
    }
}

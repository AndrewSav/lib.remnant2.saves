using rd2parser.Model.Memory;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using rd2parser.IO;

namespace rd2parser.Model.Properties;

public class StructProperty : Node
{
    public required FName Type;
    public required FGuid Guid;
    public required byte Unknown;
    public required object? Value;

    public StructProperty()
    {
    }

    public StructProperty(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "StructProperty" });
    }

    [SetsRequiredMembers]
    public StructProperty(Reader r, SerializationContext ctx, Node? parent) : this(parent)
    {
        Type = new(r, ctx.NamesTable);
        Guid = r.Read<FGuid>();
        Unknown = r.Read<byte>();
        Value = ReadStructPropertyValue(r, ctx, Type.Name, this);
    }

    // Also called by ArrayStructProperty constructor
    public static object? ReadStructPropertyValue(Reader r, SerializationContext ctx, string type, Node parent)
    {
        return type switch
        {
            "SoftClassPath" or "SoftObjectPath" => r.ReadFString(),
            "Timespan" => new TimeSpan(r.Read<long>()),
            "Guid" => r.Read<FGuid>(),
            "Vector" => r.Read<FVector>(),
            "DateTime" => new DateTime(r.Read<long>()),
            "PersistenceBlob" => ReadPersistenceBlob(r, ctx, parent),
            _ => new PropertyBag(r,ctx, parent)
        };
    }
    private static object ReadPersistenceBlob(ReaderBase r, SerializationContext ctx, Node parent)
    {
        int persistenceSize = r.Read<int>();
        byte[] bytes = r.ReadBytes(persistenceSize);
        Reader persistenceReader = new(bytes);

        if (ctx.ClassPath == "/Game/_Core/Blueprints/Base/BP_RemnantSaveGameProfile")
        {
            return new SaveData(persistenceReader, parent, true, false, ctx);
        }
        return new PersistenceContainer(persistenceReader, ctx,parent);
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        Type.Write(w, ctx);
        w.Write(Guid);
        w.Write(Unknown);
        WriteStructPropertyValue(w, ctx, Type.Name, Value);
    }

    public static void WriteStructPropertyValue(Writer w, SerializationContext ctx, string type, object? value)
    {
        switch (type)
        {
            case "SoftClassPath":
            case "SoftObjectPath":
                w.WriteFString(value as string);
                break;
            case "Timespan":
                w.Write(GetTimeSpan(value).Ticks);
                break;
            case "Guid":
                w.Write((FGuid)value!);
                break;
            case "Vector":
                w.Write((FVector)value!);
                break;
            case "DateTime":
                w.Write(GetDateTime(value).Ticks);
                break;
            case "PersistenceBlob":
                WritePersistenceBlob(w, ctx, value!);
                break;
            default:
                ((PropertyBag)value!).Write(w,ctx);
                break;
        }
    }

    private static void WritePersistenceBlob(Writer w, SerializationContext ctx, object value)
    {
        Writer persistenceWriter = new();
        if (ctx.ClassPath == "/Game/_Core/Blueprints/Base/BP_RemnantSaveGameProfile")
        {
            ((SaveData)value).Write(persistenceWriter);
        }
        else
        {
            ((PersistenceContainer)value).Write(persistenceWriter);
        }

        byte[] data = persistenceWriter.ToArray();
        w.Write(data.Length);
        w.WriteBytes(data);
    }

    private static TimeSpan GetTimeSpan(object? value)
    {
        return value switch
        {
            null => throw new ApplicationException("expected timestamp got null"),
            string timespan => TimeSpan.Parse(timespan, CultureInfo.InvariantCulture),
            _ => (TimeSpan)value
        };
    }
    private static DateTime GetDateTime(object? value)
    {
        return value switch
        {
            null => throw new ApplicationException("expected datetime got null"),
            string timespan => DateTime.Parse(timespan, CultureInfo.InvariantCulture),
            _ => (DateTime)value
        };
    }
}

using rd2parser.Model.Memory;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using rd2parser.IO;

namespace rd2parser.Model.Properties;

public class StructProperty
{
    public required FName Type;
    public required FGuid Guid;
    public required byte Unknown;
    public required object? Value;

    public StructProperty()
    {

    }

    [SetsRequiredMembers]
    public StructProperty(Reader r, SerializationContext ctx)
    {
        Type = new(r, ctx.NamesTable);
        Guid = r.Read<FGuid>();
        Unknown = r.Read<byte>();
        Value = ReadStructPropertyValue(r, ctx, Type.Name);
    }

    // Also called by ArrayStructProperty constructor
    public static object? ReadStructPropertyValue(Reader r, SerializationContext ctx, string type)
    {
        return type switch
        {
            "SoftClassPath" or "SoftObjectPath" => r.ReadFString(),
            "Timespan" => new TimeSpan(r.Read<long>()),
            "Guid" => r.Read<FGuid>(),
            "Vector" => r.Read<FVector>(),
            "DateTime" => new DateTime(r.Read<long>()),
            "PersistenceBlob" => ReadPersistenceBlob(r, ctx),
            _ => r.ReadProperties(ctx),
        };
    }
    private static object ReadPersistenceBlob(ReaderBase r, SerializationContext ctx)
    {
        int persistenceSize = r.Read<int>();
        byte[] bytes = r.ReadBytes(persistenceSize);
        Reader persistenceReader = new(bytes);

        if (ctx.ClassPath == "/Game/_Core/Blueprints/Base/BP_RemnantSaveGameProfile")
        {
            return new SaveData(persistenceReader, true, false);
        }
        return new PersistenceContainer(persistenceReader);
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
                w.WriteProperties(ctx, (List<KeyValuePair<string, Property>>)value!);
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
        if (value == null)
        {
            throw new ApplicationException("expected timestamp got null");
        }

        if (value is string timespan)
        {
            return TimeSpan.Parse(timespan, CultureInfo.InvariantCulture);
        }

        return (TimeSpan)value;
    }
    private static DateTime GetDateTime(object? value)
    {
        if (value == null)
        {
            throw new ApplicationException("expected datetime got null");
        }

        if (value is string timespan)
        {
            return DateTime.Parse(timespan, CultureInfo.InvariantCulture);
        }

        return (DateTime)value;
    }
}

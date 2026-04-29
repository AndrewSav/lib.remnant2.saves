using System.Globalization;
using System.Numerics;
using lib.remnant2.saves.Model.Memory;
using lib.remnant2.saves.Model.Parts;

namespace lib.remnant2.saves.Model.Properties.Parts;

public class PropertyValue
{
    public required object? Value;
    public byte? HasPropertyGuid;
    public FGuid? PropertyGuid;

    // This is also called from MapProperty and ArrayProperty constructor
    public static PropertyValue ReadPropertyValue(Reader r, SerializationContext ctx, string type, bool isRaw = true)
    {
        return type switch
        {
            "IntProperty" => ReadTaggedProperty(r, isRaw, () => r.Read<int>()),
            "Int16Property" => ReadTaggedProperty(r, isRaw, () => r.Read<short>()),
            "Int64Property" => ReadTaggedProperty(r, isRaw, () => r.Read<long>()),
            "UInt64Property" => ReadTaggedProperty(r, isRaw, () => r.Read<ulong>()),
            "FloatProperty" => ReadTaggedProperty(r, isRaw, () => r.Read<float>()),
            "DoubleProperty" => ReadTaggedProperty(r, isRaw, () => r.Read<double>()),
            "UInt16Property" => ReadTaggedProperty(r, isRaw, () => r.Read<ushort>()),
            "UInt32Property" => ReadTaggedProperty(r, isRaw, () => r.Read<uint>()),
            "StrProperty" or "SoftClassPath" or "SoftObjectProperty" => ReadTaggedProperty(r, isRaw, r.ReadFString),
            "BoolProperty" => ReadBoolProperty(r, isRaw),
            "NameProperty" => ReadTaggedProperty(r, isRaw, () => new FName(r, ctx.NamesTable)),

            "ByteProperty" => new() { HasPropertyGuid = null, Value = isRaw ? r.Read<byte>() : new ByteProperty(r, ctx) },
            "StructProperty" => new() { HasPropertyGuid = null, Value = isRaw ? r.Read<FGuid>() : new StructProperty(r, ctx) },
            "ObjectProperty" => ReadTaggedProperty(r, isRaw, () => new ObjectProperty(r, ctx)),
            "EnumProperty" => new() { HasPropertyGuid = null, Value = new EnumProperty(r, ctx) },
            //if (isRaw) { throw new InvalidOperationException("Raw map properties are not supported"); }
            "MapProperty" => new() { HasPropertyGuid = null, Value = new MapProperty(r, ctx) },
            "TextProperty" => ReadTaggedProperty(r, isRaw, () => new TextProperty(r, ctx)),
            "ArrayProperty" => new() { HasPropertyGuid = null, Value = ReadArrayProperty(r, ctx) },
            _ => throw new InvalidOperationException($"unknown property type {type}")
        };
    }

    public static (byte HasPropertyGuid, FGuid? PropertyGuid) ReadPropertyGuid(Reader r)
    {
        byte hasPropertyGuid = r.Read<byte>();
        FGuid? propertyGuid = hasPropertyGuid == 0 ? null : r.Read<FGuid>();
        return (hasPropertyGuid, propertyGuid);
    }

    public static void WritePropertyGuid(Writer w, byte hasPropertyGuid, FGuid? propertyGuid)
    {
        w.Write(hasPropertyGuid);
        if (hasPropertyGuid != 0)
        {
            w.Write(propertyGuid ?? default);
        }
    }

    private static PropertyValue ReadTaggedProperty(Reader r, bool isRaw, Func<object?> readValue)
    {
        (byte? hasPropertyGuid, FGuid? propertyGuid) = ReadOptionalPropertyGuid(r, isRaw);
        return new() { HasPropertyGuid = hasPropertyGuid, PropertyGuid = propertyGuid, Value = readValue() };
    }

    private static PropertyValue ReadBoolProperty(Reader r, bool isRaw)
    {
        byte value = r.Read<byte>();
        (byte? hasPropertyGuid, FGuid? propertyGuid) = ReadOptionalPropertyGuid(r, isRaw);
        return new() { HasPropertyGuid = hasPropertyGuid, PropertyGuid = propertyGuid, Value = value };
    }

    private static (byte? HasPropertyGuid, FGuid? PropertyGuid) ReadOptionalPropertyGuid(Reader r, bool isRaw)
    {
        if (isRaw)
        {
            return (null, null);
        }

        (byte hasPropertyGuid, FGuid? propertyGuid) = ReadPropertyGuid(r);
        return (hasPropertyGuid, propertyGuid);
    }

    private static T Get<T>(object? value)
    {
        if (value is BigInteger z)
        {
            value = (ulong)(z & ulong.MaxValue);
        }
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }
    private static object ReadArrayProperty(Reader r, SerializationContext ctx)
    {
        int readOffset = r.Position + ctx.ContainerOffset;
        FName elementType = new(r, ctx.NamesTable);
        (byte hasPropertyGuid, FGuid? propertyGuid) = ReadPropertyGuid(r);
        uint count = r.Read<uint>();
        return elementType.Name == "StructProperty"
            ? new ArrayStructProperty(r, ctx, count, hasPropertyGuid, propertyGuid, elementType, readOffset)
            : new ArrayProperty(r, ctx, count, hasPropertyGuid, propertyGuid, elementType, readOffset);
    }

    public static void WritePropertyValue(Writer w, SerializationContext ctx, object? value, string type, byte? hasPropertyGuid = null, FGuid? propertyGuid = null)
    {
        switch (type)
        {
            case "IntProperty":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                w.Write(Get<int>(value));
                break;
            case "Int16Property":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                w.Write(Get<short>(value));
                break;
            case "Int64Property":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                w.Write(Get<long>(value));
                break;
            case "UInt64Property":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                w.Write(Get<ulong>(value));
                break;
            case "FloatProperty":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                w.Write(Get<float>(value));
                break;
            case "DoubleProperty":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                w.Write(Get<double>(value));
                break;
            case "UInt16Property":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                w.Write(Get<ushort>(value));
                break;
            case "UInt32Property":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                w.Write(Get<uint>(value));
                break;
            case "SoftClassPath":
            case "SoftObjectProperty":
            case "StrProperty":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                w.WriteFString(value as string);
                break;
            case "BoolProperty":
                w.Write(Get<byte>(value));
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                break;
            case "NameProperty":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                ((FName)value!).Write(w, ctx);
                break;
            case "ByteProperty":
                if (hasPropertyGuid != null) { ((ByteProperty)value!).Write(w, ctx); } else { w.Write((byte)value!); }
                break;
            case "StructProperty":
                if (hasPropertyGuid != null) { ((StructProperty)value!).Write(w, ctx); } else { w.Write((FGuid)value!); }
                break;
            case "ObjectProperty":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                ((ObjectProperty)value!).Write(w, ctx);
                break;
            case "EnumProperty":
                ((EnumProperty)value!).Write(w, ctx);
                break;
            case "MapProperty":
                //if (noRaw == null) { throw new InvalidOperationException("Raw map properties are not supported"); }
                ((MapProperty)value!).Write(w, ctx);
                break;
            case "TextProperty":
                WriteOptionalPropertyGuid(w, hasPropertyGuid, propertyGuid);
                ((TextProperty)value!).Write(w, ctx);
                break;
            case "ArrayProperty":
                WriteArrayProperty(w, ctx, value!);
                break;
            default:
                throw new InvalidOperationException($"unknown property type {type}");
        }
    }

    private static void WriteOptionalPropertyGuid(Writer w, byte? hasPropertyGuid, FGuid? propertyGuid)
    {
        if (hasPropertyGuid != null)
        {
            WritePropertyGuid(w, hasPropertyGuid.Value, propertyGuid);
        }
    }

    private static void WriteArrayProperty(Writer w, SerializationContext ctx, object value)
    {
        switch (value)
        {
            case ArrayStructProperty property:
                {
                    property.Write(w, ctx);
                    break;
                }
            case ArrayProperty property:
                {
                    property.Write(w, ctx);
                    break;
                }
            default:
                throw new InvalidOperationException("unexpected array type");
        }
    }
}

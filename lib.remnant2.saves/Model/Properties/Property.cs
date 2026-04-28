using System.Diagnostics.CodeAnalysis;
using lib.remnant2.saves.Model.Parts;
using lib.remnant2.saves.Model.Properties.Parts;
using Serilog;

namespace lib.remnant2.saves.Model.Properties;

public class Property : ModelBase
{
    public static ILogger Logger => Log.Logger.ForContext(Log.Category, Log.Parser).ForContext<Property>();

    public required FName Name;
    public uint? Index;
    public uint? Size;
    public byte? NoRaw;
    public FName? Type;
    public object? Value;

    public Property()
    {
    }

    [SetsRequiredMembers]
    public Property(Reader r, SerializationContext ctx)
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        Name = new(r, ctx.NamesTable);
        if (Name.Name == "None")
        {
            return;
        }
        Type = new(r, ctx.NamesTable);
        Size = r.Read<uint>();
        Index = r.Read<uint>();

        // Just to make results a bit more compact
        // since we are usually not interested very much in these
        if (Name.Name == "FowVisitedCoordinates" && !(ctx.Options?.ParseFowVisitedCoordinates ?? false))
        {
            Value = r.ReadBytes((int)Size + 19);
        }
        else
        {
            PropertyValue pv = PropertyValue.ReadPropertyValue(r, ctx, Type.Name, false);
            NoRaw = pv.NoRawByte;
            Value = pv.Value;
        }
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
    }

    public string? ToStringValue()
    {
        return Value?.ToString();
    }

    public override string ToString()
    {
        return $"{Name}({Value?.GetType().Name}): {Value}";
    }

    public T Get<T>()
    {
        return Value switch
        {
            T prop => prop,
            StructProperty { Value: T inner } => inner,
            _ => throw new InvalidOperationException($"requested value is of a different type. Requested type: '{typeof(T)}' actual type: '{Value.ToTypeString()}'")
        };
    }

    public T Get<T>(T @default)
    {
        return Value switch
        {
            T prop => prop,
            StructProperty { Value: T inner } => inner,
            _ => @default
        };
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        Name.Write(w, ctx);
        if (Name.Name == "None")
        {
            return;
        }
        Type!.Write(w, ctx);
        long sizeOffset = w.Position;
        uint oldSize = Size!.Value;
        w.Write(oldSize);
        w.Write(Index!.Value);
        long startOffset = w.Position;
        if (Name.Name == "FowVisitedCoordinates" && Value is byte[])
        {
            w.WriteBytes((byte[])Value!);
        }
        else
        {
            PropertyValue.WritePropertyValue(w,ctx,Value!,Type.Name, NoRaw??0);
        }

        long endOffset = w.Position;
        uint newSize = CalculateSize(Name.Name, Type.Name, Value, startOffset, endOffset);

        if (newSize != oldSize)
        {
            Size = newSize;
            w.Position = sizeOffset;
            w.Write(Size!.Value);
            w.Position = endOffset;
            Logger.Information("Adjusted {PropertyType} size for {PropertyName} at {Offset} from {OldSize} to {NewSize} ({Delta:+#;-#;0})",
                Type.Name, Name.Name, WriteOffset, oldSize, newSize, (long)newSize - oldSize);
        }

        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
    }

    private static uint CalculateSize(string name, string type, object? value, long startOffset, long endOffset)
    {
        long size = endOffset - startOffset - GetSizeOverhead(name, type, value);
        if (size < 0 || size > uint.MaxValue)
        {
            throw new InvalidOperationException($"calculated invalid size {size} for property type {type}");
        }

        return (uint)size;
    }

    private static int GetSizeOverhead(string name, string type, object? value)
    {
        if (name == "FowVisitedCoordinates" && value is byte[])
        {
            return type == "StructProperty"
                ? 19
                : throw new InvalidOperationException($"expected FowVisitedCoordinates to be StructProperty got {type}");
        }

        return type switch
        {
            "ArrayProperty" => value switch
            {
                ArrayProperty property => GetFNameWriteLength(property.ElementType) + 1,
                ArrayStructProperty property => GetFNameWriteLength(property.OuterElementType) + 1,
                _ => throw new InvalidOperationException($"expected ArrayProperty value got {value.ToTypeString()}")
            },
            "BoolProperty" => 2,
            "ByteProperty" => value is ByteProperty byteProperty
                ? GetFNameWriteLength(byteProperty.EnumName) + 1
                : throw new InvalidOperationException($"expected ByteProperty value got {value.ToTypeString()}"),
            "EnumProperty" => value is EnumProperty enumProperty
                ? GetFNameWriteLength(enumProperty.EnumType) + 1
                : throw new InvalidOperationException($"expected EnumProperty value got {value.ToTypeString()}"),
            "MapProperty" => value is MapProperty mapProperty
                ? mapProperty.Unknown.Length
                : throw new InvalidOperationException($"expected MapProperty value got {value.ToTypeString()}"),
            "StructProperty" => value is StructProperty structProperty
                ? GetFNameWriteLength(structProperty.Type) + 17
                : throw new InvalidOperationException($"expected StructProperty value got {value.ToTypeString()}"),
            _ => 1
        };
    }

    private static int GetFNameWriteLength(FName name)
    {
        return name.Number == null ? 2 : 6;
    }

    public List<T> GetItems<T>()
    {
        if (Type?.Name != "ArrayProperty")
        {
            throw new InvalidOperationException($"this method only works for ArrayProperty. Current type: '{Type?.Name}'");
        }

        return Value switch
        {
            ArrayProperty ap => ap.Items.Select(x => (T)x!).ToList(),
            ArrayStructProperty asp => asp.Items.Select(x => (T)x!).ToList(),
            _ => throw new InvalidOperationException("unexpected value type")
        };
    }

    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        if (Value is ModelBase node)
            yield return (node, null);
    }
}

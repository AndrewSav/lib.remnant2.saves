﻿using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class Property : Node
{
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
    }

    public override string? ToString()
    {
        return Value?.ToString();
    }

    public T Get<T>()
    {
        return Value switch
        {
            T prop => prop,
            StructProperty { Value: T inner } => inner,
            _ => throw new InvalidOperationException("requested value is of a different type")
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
        w.Write(Size!.Value);
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
        uint newSize = (uint)(endOffset - startOffset);

        uint sizeAdjustment = 0;
        if (Type.Name == "StructProperty")
        {
            sizeAdjustment = 19;
        }
        if (Type.Name == "ArrayProperty")
        {
            sizeAdjustment = 3;
        }

        if (sizeAdjustment != 0)
        {
            newSize -= sizeAdjustment;
            Size = newSize;
            w.Position = sizeOffset;
            w.Write(Size!.Value);
            w.Position = endOffset;
        }
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

    public override IEnumerable<Node> GetChildren()
    {
        if (Value is Node node)
            yield return node;
    }
}

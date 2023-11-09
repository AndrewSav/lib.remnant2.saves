﻿using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class ArrayProperty : Node
{
    public required FName ElementType;
    public required List<object?> Items;
    public required byte Unknown;

    public ArrayProperty()
    {
    }

    [SetsRequiredMembers]
    public ArrayProperty(Reader r, SerializationContext ctx, uint count, byte unknown, FName elementType, int readOffset) 
    {
        ReadOffset = readOffset;
        Unknown = unknown;
        if (Unknown != 0)
        {
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown byte at {Offset}", Unknown,  r.Position);
        }
        ElementType = elementType;
        Items = new List<object?>();
        for (int i = 0; i < count; i++)
        {
            object o = PropertyValue.ReadPropertyValue(r, ctx, ElementType.Name).Value!;
            Items.Add(o);
        }
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        ElementType.Write(w, ctx);
        w.Write(Unknown);
        w.Write(Items.Count);
        foreach (object? item in Items) PropertyValue.WritePropertyValue(w, ctx, item, ElementType.Name);
    }
    public override IEnumerable<Node> GetChildren()
    {
        foreach (object? item in Items)
        {
            if (item is Node node)
                yield return node;
        }
    }
}

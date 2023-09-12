﻿using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model;

public class Variables : Node
{
    public required FName Name;
    public required ulong Unknown;
    public required List<KeyValuePair<string, Variable>> Properties;

    public Variables()
    {
    }

    public Variables(Node? parent, string name) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Name = name, Type = "Variables" });
    }

    [SetsRequiredMembers]
    public Variables(Reader r, SerializationContext ctx, Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Properties = new();
        Name = new FName(r, ctx.NamesTable);
        Path.Add(new() { Name = Name.Name, Type = "Variables" });
        Unknown = r.Read<ulong>();
        int len = r.Read<int>();

        for (int i = 0; i < len; i++)
        {
            Variable v = new(r, ctx,this);
            v.Path[^1].Index = i;
            Properties.Add(new KeyValuePair<string, Variable>(v.Name.Name,v));
        }
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        Name.Write(w, ctx);
        w.Write(Unknown);
        w.Write(Properties.Count);
        foreach (KeyValuePair<string, Variable> keyValuePair in Properties)
        {
            keyValuePair.Value.Write(w, ctx);
        }
    }
}

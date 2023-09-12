﻿using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model.Properties;

public class TextProperty : Node
{
    public required uint Flags;
    public required byte HistoryType;
    public required object Value;

    public TextProperty()
    {
    }

    public TextProperty(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "TextProperty" });
    }

    [SetsRequiredMembers]
    public TextProperty(Reader r, Node? parent) : this(parent)
    {
        Flags = r.Read<uint>();
        HistoryType = r.Read<byte>();
        Value = HistoryType switch
        {
            0 => new TextPropertyData0(r, parent),
            255 => new TextPropertyData255(r, parent),
            _ => throw new ApplicationException("unsupported history type"),
        };
    }

    public void Write(Writer w)
    {
        w.Write(Flags);
        w.Write(HistoryType);
        switch (HistoryType)
        {
            case 0:
                ((TextPropertyData0)Value).Write(w);
                break;
            case 255:
                ((TextPropertyData255)Value).Write(w);
                break;
            default:
                throw new ApplicationException("unsupported history type");
        }
    }
}
﻿using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace rd2parser.Model;
public class Variable
{
    public required FName Name;
    public required string Type;
    public object? Value;

    private readonly string[] _varTypeNames = {
        "None",
        "BoolProperty",
        "IntProperty",
        "FloatProperty",
        "NameProperty"
    };

    public Variable()
    {

    }

    [SetsRequiredMembers]
    public Variable(Reader r, SerializationContext ctx)
    {
        FName name = new(r, ctx.NamesTable);
        if (name.Name == "None")
        {
            throw new ApplicationException("unexpected None in variable");
        }
        byte enumVal = r.Read<byte>();
        Name = name;
        Type = _varTypeNames[enumVal];
    
        switch (Type)
        {
            case "None":
                break;
            case "BoolProperty":
            case "IntProperty":
                Value = r.Read<uint>();
                break;
            case "FloatProperty":
                Value = r.Read<float>();
                break;
            case "NameProperty":
                Value = new FName(r, ctx.NamesTable);
                break;
            default:
                throw new ApplicationException("unknown variable type");
        }
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        Name.Write(w,ctx);
        byte enumVal = (byte)_varTypeNames
            .Select((input, index) => new { index, input })
            .Single(x => x.input == Type).index;
        w.Write(enumVal);
        switch (Type)
        {
            case "None":
                break;
            case "BoolProperty":
            case "IntProperty":
                w.Write(Get<uint>(Value));
                break;
            case "FloatProperty":
                w.Write(Get<float>(Value));
                break;
            case "NameProperty":
                ((FName)Value!).Write(w, ctx);
                break;
            default:
                throw new ApplicationException("unknown variable type");
        }
    }
    private static T Get<T>(object? value)
    {
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }

}

﻿using Newtonsoft.Json;
using rd2parser.Model.Properties;

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

    public UObject()
    {
    }

    public UObject(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "UObject" });
    }

    public int ObjectIndex
    {
        get
        {
            if (Parent == null)
            {
                throw new InvalidOperationException("UObject has no parent");
            }

            return (Parent as SaveData)!.Objects.FindIndex(x => x == this);
        } 
    }

    public string? Name => LoadedData?.Name.Name;

    // To make it easier to navigate in the debugger
    public override string? ToString()
    {
        return Key ?? ObjectPath ?? Name;
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

    // To make it easier to navigate in the debugger
    [JsonIgnore]
    public List<Dictionary<string, UObject>>? Children
    {
        get
        {
            if (Properties is not { Properties.Count: > 1 })
            {
                return null;
            }

            KeyValuePair<string, Property> property = Properties.Properties[1];
            if (property.Key != "Blob")
            {
                return null;
            }

            if (property.Value.Value is not StructProperty s)
            {
                return null;
            }

            if (s.Value is not PersistenceContainer p)
            {
                return null;
            }

            return p.Children;
        }
    }

    // To make it easier to navigate in the debugger
    [JsonIgnore]
    public List<UObject>? FlattenChildren => Children?.SelectMany(x => x.Values).ToList();

}
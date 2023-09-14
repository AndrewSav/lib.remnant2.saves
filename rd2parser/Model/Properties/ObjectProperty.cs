using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;

public class ObjectProperty : Node
{
    public required int ObjectIndex;
    public string? ClassName; // To make it easier to navigate in the debugger
    [JsonIgnore]
    public UObject? Object;

    public ObjectProperty(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "ObjectProperty" });
    }

    [SetsRequiredMembers]
    public ObjectProperty(Reader r, SerializationContext ctx, Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "ObjectProperty" });
        ObjectIndex = r.Read<int>();
        SetIndex(ObjectIndex, ctx.Objects!);
    }

    public ObjectProperty()
    {
    }

    public override string? ToString()
    {
        return ClassName;
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        SetIndex(ObjectIndex, ctx.Objects!);
        w.Write(Object?.ObjectIndex ?? -1);
    }

    public void SetIndex(int index,List<UObject> objects )
    {
        if (index != -1)
        {
            ClassName = objects[index].ObjectPath;
            Object = objects[index];
            if (Parent != null) Path[^1].Name = ClassName;
        }
        else
        {
            if (Parent != null) Path[^1].Name = null;
        }
        ObjectIndex = index;
    }
    public void SetObject(UObject o)
    {
        SetIndex(o.ObjectIndex, o.SaveData.Objects);
    }
    public override IEnumerable<Node> GetChildren()
    {
        yield break;
    }
}

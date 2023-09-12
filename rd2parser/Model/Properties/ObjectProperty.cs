using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

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
        ObjectIndex = r.Read<int>();
        if (ObjectIndex != -1)
        {
            // This is never called before we read Objects and put it in the context
            ClassName = ctx.Objects![ObjectIndex].ObjectPath;
            Object = ctx.Objects![ObjectIndex];
            Path.Add(new() { Name = ClassName, Type = "ObjectProperty" });
        }
        else
        {
            Path.Add(new() { Type = "ObjectProperty" });
        }
    }

    public ObjectProperty()
    {
    }

    // To make it easier to navigate in the debugger
    public override string ToString()
    {
        if (ClassName == null)
        {
            return "null";
        }

        return $"{ClassName} (id:{ObjectIndex})";
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        if (Object == null && ObjectIndex != -1)
        {
            Object = ctx.Objects![ObjectIndex];
        }
        w.Write(Object?.ObjectIndex ?? -1);
    }
}
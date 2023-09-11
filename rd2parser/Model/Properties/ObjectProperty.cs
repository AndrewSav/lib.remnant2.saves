using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace rd2parser.Model.Properties;

public class ObjectProperty
{
    public required int ObjectIndex;
    public string? ClassName; // To make it easier to navigate in the debugger
    [JsonIgnore]
    public UObject? Object;

    public ObjectProperty()
    {

    }
    [SetsRequiredMembers]
    public ObjectProperty(Reader r, SerializationContext ctx)
    {
        ObjectIndex = r.Read<int>();
        if (ObjectIndex != -1)
        {
            // This is never called before we read Objects and put it in the context
            ClassName = ctx.Objects![ObjectIndex].ObjectPath;
            Object = ctx.Objects![ObjectIndex];
        }
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
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace rd2parser.Model.Properties;

public class ObjectProperty : ModelBase
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
        ReadOffset = r.Position + ctx.ContainerOffset;
        ObjectIndex = r.Read<int>();
        if (ObjectIndex != -1)
        {
            ClassName = ctx.Objects![ObjectIndex].ObjectPath;
            Object = ctx.Objects[ObjectIndex];
        }
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        w.Write(ObjectIndex);
        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
    }

    public override string? ToString()
    {
        return ClassName;
    }
    
    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        yield break;
    }
}

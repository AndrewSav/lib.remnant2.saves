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
        SetIndex(ObjectIndex, ctx.Objects!);
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
    }

    public override string? ToString()
    {
        return ClassName;
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        SetIndex(ObjectIndex, ctx.Objects!);
        w.Write(Object?.ObjectIndex ?? -1);
        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
    }

    public void SetIndex(int index,List<UObject> objects )
    {
        if (index != -1)
        {
            ClassName = objects[index].ObjectPath;
            Object = objects[index];
        }
        ObjectIndex = index;
    }
    //public void SetObject(UObject o)
    //{
    //    SetIndex(o.ObjectIndex, o.SaveData.Objects);
    //}
    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        yield break;
    }
}

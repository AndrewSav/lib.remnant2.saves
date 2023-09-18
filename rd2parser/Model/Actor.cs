using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using rd2parser.Model.Memory;
using rd2parser.Model.Properties;
using rd2parser.Navigation;

namespace rd2parser.Model;

public class Actor : Node
{
    public required uint HasTransform;
    public FTransform? Transform;
    public required SaveData Archive;
    // DynamicData block is read after actors have been read
    public DynamicActor? DynamicData;

    public Actor()
    {
    }

    public Actor(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "Actor" });
    }

    [SetsRequiredMembers]

    public Actor(Reader r, SerializationContext ctx, Node? parent, int containerOffset) : this(parent)
    {
        ReadOffset = r.Position + containerOffset;
        HasTransform = r.Read<uint>();
        if (HasTransform != 0)
        {
            Transform = r.Read<FTransform>();
        }
        Archive =  new SaveData(r,this, false, false, ctx, containerOffset,ctx.Options);
    }

    public void WriteNonDynamic(Writer w, int containerOffset)
    {
        WriteOffset = (int)w.Position + containerOffset;
        w.Write(HasTransform);
        if (HasTransform != 0)
        {
            w.Write(Transform!.Value);
        }
        Archive.Write(w, containerOffset);
    }


    public override string? ToString()
    {
        if (DynamicData?.ClassPath.Name != null)
        {
            if (DynamicData.ClassPath.Name == "ZoneActor")
            {
                string? label = Archive.Objects[0].Properties?["Label"].ToString();
                return $"ZoneActor({label})";
            }
            return DynamicData.ClassPath.Name;
        }
        return base.ToString();
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Archive;
    }

    [JsonIgnore]
    public PropertyBag? ZoneActorProperties
    {
        get
        {
            UObject? zoneActorObject = Archive.Objects.FirstOrDefault(x => x.Name == "ZoneActor");
            if (zoneActorObject != null)
            {
                return zoneActorObject.Properties;
            }
            return null;
        }
    }
    [JsonIgnore]
    public PropertyBag? FirstObjectProperties
    {
        get
        {
            UObject? firstObject = Archive.Objects.FirstOrDefault();
            if (firstObject != null)
            {
                return firstObject.Properties;
            }
            return null;
        }
    }
}

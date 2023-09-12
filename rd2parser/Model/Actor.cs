using System.Diagnostics.CodeAnalysis;
using rd2parser.Model.Memory;

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

    public Actor(Reader r, SerializationContext ctx, Node? parent) : this(parent)
    {
        HasTransform = r.Read<uint>();
        if (HasTransform != 0)
        {
            Transform = r.Read<FTransform>();
        }
        Archive =  new SaveData(r,this, false, false, ctx);
    }

    public void WriteNonDynamic(Writer w)
    {
        w.Write(HasTransform);
        if (HasTransform != 0)
        {
            w.Write(Transform!.Value);
        }
        Archive.Write(w);
    }
}

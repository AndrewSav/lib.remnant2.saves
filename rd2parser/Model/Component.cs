using System.Diagnostics.CodeAnalysis;
using rd2parser.Model.Properties;
using rd2parser.Navigation;

namespace rd2parser.Model;

public class Component : Node
{
    public required string ComponentKey;
    public PropertyBag? Properties;
    public Variables? Variables;
    public byte[]? ExtraComponentsData;

    public Component()
    {
    }

    [SetsRequiredMembers]
    public Component(Node? parent, string componentKey) : base(parent, new List<Segment>(parent!.Path))
    {
        ComponentKey = componentKey;
        Path.Add(new() { Name = componentKey, Type = "Component" });
    }
    public override IEnumerable<Node> GetChildren()
    {
        if (Properties != null)
            yield return Properties;
        if (Variables != null)
            yield return Variables;
    }
}

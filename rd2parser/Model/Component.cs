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
    public Component(string componentKey)
    {
        ComponentKey = componentKey;
    }
    public override IEnumerable<Node> GetChildren()
    {
        if (Properties != null)
            yield return Properties;
        if (Variables != null)
            yield return Variables;
    }

    public override string ToString()
    {
        return ComponentKey;
    }
}

using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model.Properties;
public class PropertyBag : Node
{
    public required List<KeyValuePair<string, Property>> Properties;

    public PropertyBag()
    {
    }

    [SetsRequiredMembers]
    public PropertyBag(Reader r, SerializationContext ctx)
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        Properties = new();
        while (true)
        {
            Property p = new(r, ctx);
            if (p.Name.Name == "None")
            {
                break;
            }
            Properties.Add(new KeyValuePair<string, Property>(p.Name.Name, p));
        }
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        foreach (KeyValuePair<string, Property> keyValuePair in Properties)
        {
            keyValuePair.Value.Write(w, ctx);
        }

        ushort index = (ushort)ctx.GetNamesTableIndex("None");
        new Property { Name = new FName { Name = "None", Index = index, Number = null }}.Write(w, ctx);
    }

    public Property this[string s]
    {
        get
        {
            return Properties.Single(x=>x.Key == s).Value;

        }
    }
    
    public bool Contains(string s)
    {
        return Properties.Any(x => x.Key == s);
    }

    public override IEnumerable<Node> GetChildren()
    {
        foreach (Property p in Properties.Select(x => x.Value))
        {
            yield return p;
        }
    }
}

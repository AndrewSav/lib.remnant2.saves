using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model.Properties;
public class PropertyBag : Node
{
    public required List<KeyValuePair<string, Property>> Properties;

    public static readonly PropertyBag Dummy = new() { Properties = new() };

    public PropertyBag()
    {

    }
    public PropertyBag(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "PropertyBag" });
    }

    [SetsRequiredMembers]
    public PropertyBag(Reader r, SerializationContext ctx, Node? parent) : this(parent)
    {
        Properties = new();
        int i = 0;
        while (true)
        {
            Property p = new(r, ctx, this);
            p.Path[^1].Index = i;
            if (p.Name.Name == "None")
            {
                break;
            }
            Properties.Add(new KeyValuePair<string, Property>(p.Name.Name, p));
            i++;
        }
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        foreach (KeyValuePair<string, Property> keyValuePair in Properties)
        {
            keyValuePair.Value.Write(w, ctx);
        }

        ushort index = (ushort)ctx.GetNamesTableIndex("None");
        new Property(null, "None") { Name = new FName { Name = "None", Index = index, Number = null }, Bag = Dummy }.Write(w, ctx);
    }

    public Property this[string s]
    {
        get
        {
            return Properties.Single(x=>x.Key == s).Value;

        }
    }
}

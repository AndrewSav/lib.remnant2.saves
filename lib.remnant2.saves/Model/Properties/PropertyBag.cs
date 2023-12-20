using System.Diagnostics.CodeAnalysis;
using lib.remnant2.saves.Model.Parts;

namespace lib.remnant2.saves.Model.Properties;
public class PropertyBag : ModelBase
{
    public required List<KeyValuePair<string, Property>> Properties;

    public PropertyBag()
    {
    }

    [SetsRequiredMembers]
    public PropertyBag(Reader r, SerializationContext ctx)
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        Properties = [];
        while (true)
        {
            Property p = new(r, ctx);
            if (p.Name.Name == "None")
            {
                break;
            }
            Properties.Add(new KeyValuePair<string, Property>(p.Name.Name, p));
        }
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
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
        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
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

    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        for (int index = 0; index < Properties.Count; index++)
        {
            KeyValuePair<string, Property> p = Properties[index];
            yield return (p.Value, index);
        }
    }
}

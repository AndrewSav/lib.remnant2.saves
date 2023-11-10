using rd2parser.Model.Properties;
namespace rd2parser.Navigation;
public static class PropertyExtension
{
    //public static PropertyBag GetBag(this Property property, Navigator navigator)
    //{
    //    return (PropertyBag)navigator.Lookup(property).Parent!.Object;
    //}

    public static Property ShallowCopyProperty(this Property property)
    {
        Property result = new()
        {
            Name = property.Name,
            Index = property.Index,
            Size = property.Size,
            NoRaw = property.NoRaw,
            Type = property.Type,
            Value = property.Value
        };
        return result;
    }
}


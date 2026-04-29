using lib.remnant2.saves.Model.Properties;

namespace examples;
public static class PropertyExtension
{
    public static Property ShallowCopyProperty(this Property property)
    {
        Property result = new()
        {
            Name = property.Name,
            Index = property.Index,
            Size = property.Size,
            HasPropertyGuid = property.HasPropertyGuid,
            PropertyGuid = property.PropertyGuid,
            Type = property.Type,
            Value = property.Value
        };
        return result;
    }
}

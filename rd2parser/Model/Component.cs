using rd2parser.Model.Properties;

namespace rd2parser.Model;

public class Component
{
    public required string ComponentKey;
    public List<KeyValuePair<string, Property>>? Properties;
    public Variables? Variables;
    public byte[]? ExtraComponentsData;
}
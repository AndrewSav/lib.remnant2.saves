using rd2parser.Model;
using rd2parser.Model.Properties;
using rd2parser.Navigation;

namespace rd2parser;

public class SerializationContext
{
    public required List<string> NamesTable;
    // Required when reading UObject and also when reading Persistence Blob
    // so we could decode individual character data, while reading profile save
    // This can be null on during nested SaveData reads
    public required string? ClassPath;
    // This is used so we could populate ObjectProperty with object path for display purposes
    // This is also used during writing of ObjectProperty to update object index
    public List<UObject>? Objects;

    // These two properties keep names of all properties and variables
    // So they are easier to find with SaveFile.GetVariable(s)/Property(ies)
    public ItemRegistry Registry = new();

    // This is used during read/write in a nested container
    // to keep track of outermost file offsets
    public int ContainerOffset;

    public Options? Options;

    // Caching NameTable entries for faster access
    private readonly Dictionary<string, int> _namesTableIndex = new();

    public int GetNamesTableIndex(string name)
    {
        if (_namesTableIndex.TryGetValue(name, out int index))
        {
            return index;
        }

        index = NamesTable.FindIndex(x => x == name);
        if (index < 0)
        {
            return index;
        }
        _namesTableIndex.Add(name, index);
        return index;
    }
}

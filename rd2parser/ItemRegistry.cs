namespace rd2parser;
public class ItemRegistry<T> where T : Node
{
    private readonly Dictionary<string,List<T>> _registry = new ();

    public void Add(string name, T item)
    {
        if (!_registry.ContainsKey(name))
        {
            _registry[name] = new();
        }
        _registry[name].Add(item);
    }

    public void Add(ItemRegistry<T> registry)
    {
        foreach (KeyValuePair<string, List<T>> registryItem in registry._registry)
        {
            foreach (T item in registryItem.Value)
            {
                Add(registryItem.Key, item);
            }
        }
    }

    public List<T>? Get(string name)
    {
        if (!_registry.ContainsKey(name))
        {
            return null;
        }
        return new List<T>(_registry[name]);
    }

    public List<string> GetTypes()
    {
        List<string> type = new();
        foreach (KeyValuePair<string, List<T>> registryItem in _registry)
        {
            foreach (T item in registryItem.Value)
            {
                foreach (Segment s in item.Path)
                {
                    if (!type.Contains(s.Type))
                    {
                        type.Add(s.Type);
                    }
                }
            }
        }
        return type;
    }

    public List<string> GetNames()
    {
        List<string> name = new();

        foreach (KeyValuePair<string, List<T>> registryItem in _registry)
        {
            foreach (T item in registryItem.Value)
            {
                foreach(Segment s in item.Path) 
                { 
                    if (s.Name != null)
                    {
                        if (!name.Contains(s.Name))
                        {
                            name.Add(s.Name);
                        }
                    }
                }
            }
        }
        return name;
    }
}
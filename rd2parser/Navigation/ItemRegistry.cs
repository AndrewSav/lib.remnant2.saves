namespace rd2parser.Navigation;
public class ItemRegistry
{
    private readonly Dictionary<string, Dictionary<string, List<Node>>> _registry = new();

    public void Add<T>(string name, T item) where T : Node
    {
        string type = typeof(T).Name;
        if (!_registry.ContainsKey(type))
        {
            _registry[type] = new();
        }
        var byType = _registry[type];
        if (!byType.ContainsKey(name))
        {
            byType[name] = new();
        }
        byType[name].Add(item);
    }

    public void Add(ItemRegistry registry)
    {
        foreach (KeyValuePair<string, Dictionary<string, List<Node>>> byType in registry._registry)
        {
            foreach (KeyValuePair<string, List<Node>> registryItem in byType.Value)
            {
                {
                    foreach (Node item in registryItem.Value)
                    {
                        Add(registryItem.Key, (dynamic)item);
                    }
                }
            }
        }
    }

    public List<T>? Get<T>(string name) where T : Node
    {
        string type = typeof(T).Name;
        if (!_registry.ContainsKey(type)) return null;
        var byType = _registry[type];
        return !byType.ContainsKey(name) ? null : byType[name].Select(x =>(T)x).ToList();
    }

    public List<string> GetTypes()
    {
        List<string> type = new();
        foreach (KeyValuePair<string, Dictionary<string, List<Node>>> byType in _registry)
        {
            foreach (KeyValuePair<string, List<Node>> registryItem in byType.Value)
            {
                foreach (Node item in registryItem.Value)
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
        }

        return type;
    }

    public List<string> GetNames()
    {
        List<string> name = new();

        foreach (KeyValuePair<string, Dictionary<string, List<Node>>> byType in _registry)
        {
            foreach (KeyValuePair<string, List<Node>> registryItem in byType.Value)
            {
                foreach (Node item in registryItem.Value)
                {
                    foreach (Segment s in item.Path)
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
        }
        return name;
    }
}

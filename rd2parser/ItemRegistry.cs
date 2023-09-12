namespace rd2parser;
public class ItemRegistry<T>
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
}
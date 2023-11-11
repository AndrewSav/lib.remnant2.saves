using System.Text.RegularExpressions;
using rd2parser.Model;

namespace rd2parser.Navigation;
internal class ItemRegistry
{
    private readonly Dictionary<string, Dictionary<string, List<Node>>> _registry = new();

    public bool Add(Node item)
    {
        string type = item.Path[^1].Type;
        if (string.IsNullOrEmpty(item.Path[^1].Name))
        {
            return false;
        }
        string name = item.Path[^1].Name;
        if (!_registry.ContainsKey(type))
        {
            _registry[type] = new();
        }
        Dictionary<string, List<Node>> byType = _registry[type];
        if (!byType.ContainsKey(name))
        {
            byType[name] = new();
        }
        byType[name].Add(item);
        return true;
    }

    public List<T> Get<T>(string name) where T : ModelBase
    {
        string type = typeof(T).Name;
        if (!_registry.ContainsKey(type)) throw new InvalidOperationException($"Trying to get an object of unknown type {type}");
        Dictionary<string, List<Node>> byType = _registry[type];
        return !byType.ContainsKey(name) ? new() : byType[name].Select(x => (T)x.Object).ToList();
    }

    public List<T> GetAll<T>() where T : ModelBase
    {
        string type = typeof(T).Name;
        if (!_registry.ContainsKey(type)) throw new InvalidOperationException($"Trying to get an object of unknown type {type}");
        Dictionary<string, List<Node>> byType = _registry[type];
        return byType.SelectMany(x => x.Value).Select(x => (T)x.Object).ToList();
    }

    public List<T> Find<T>(string namePattern) where T : ModelBase
    {
        string type = typeof(T).Name;
        if (!_registry.ContainsKey(type)) throw new InvalidOperationException($"Trying to get an object of unknown type {type}");
        Dictionary<string, List<Node>> byType = _registry[type];
        return byType.SelectMany(x => x.Value).Where(x => !string.IsNullOrEmpty(x.Path[^1].Name) && Regex.IsMatch(x.Path[^1].Name, namePattern)).Select(x => (T)x.Object).ToList();
    }

    public Dictionary<string,List<string>> GetNames()
    {
        return _registry.Keys.ToDictionary(x => x,x=> _registry[x].Keys.ToList());
    }
}

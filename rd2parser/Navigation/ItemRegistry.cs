using System.Text.RegularExpressions;
using rd2parser.Model;

namespace rd2parser.Navigation;
public class ItemRegistry
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

    public List<T>? Get<T>(string name) where T : ModelBase
    {
        string type = typeof(T).Name;
        if (!_registry.ContainsKey(type)) return null;
        Dictionary<string, List<Node>> byType = _registry[type];
        return !byType.ContainsKey(name) ? null : byType[name].Select(x => (T)x.Object).ToList();
    }

    public List<T>? GetAll<T>() where T : ModelBase
    {
        string type = typeof(T).Name;
        if (!_registry.ContainsKey(type)) return null;
        Dictionary<string, List<Node>> byType = _registry[type];
        return byType.SelectMany(x => x.Value).Select(x => (T)x.Object).ToList();
    }

    public List<T>? Find<T>(string namePattern) where T : ModelBase
    {
        string type = typeof(T).Name;
        if (!_registry.ContainsKey(type)) return null;
        Dictionary<string, List<Node>> byType = _registry[type];
        List<T> result = byType.SelectMany(x => x.Value).Where(x => x.Path[^1].Name != null && Regex.IsMatch(x.Path[^1].Name!, namePattern)).Select(x => (T)x.Object).ToList();
        return result.Count > 0 ? result : null;
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

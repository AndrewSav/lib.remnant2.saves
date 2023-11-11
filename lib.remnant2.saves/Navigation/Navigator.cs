using lib.remnant2.saves.Model;
using lib.remnant2.saves.Model.Properties;

namespace lib.remnant2.saves.Navigation;

public class Navigator
{
    private readonly ItemRegistry _registry = new();
    private readonly Dictionary<ModelBase, Node> _lookup = new();
    private readonly Node _root;
    public Navigator(SaveFile saveFile)
    {
        _root = new Node(saveFile.SaveData, this);
        Queue<Node> q = new();
        q.Enqueue(_root);
        while (q.Count > 0)
        {
            Node n = q.Dequeue();
            _lookup.Add(n.Object, n);
            string type = n.Path[^1].Type;
            switch (type)
            {
                case "Property":
                case "Variable":
                case "Actor":
                case "UObject":
                case "Component":
                    _registry.Add(n);
                    break;
            }

            foreach (Node c in n.Children)
            {
                q.Enqueue(c);
            }
        }
    }

    public Node Root => _root;

    public Node Lookup(ModelBase o)
    {
        return _lookup[o];
    }

    private List<T> Filter<T>(List<T> items, ModelBase? parent) where T : ModelBase
    {
        if (parent == null) return items;

        static bool IsParent(List<Segment> obj, List<Segment> filter)
        {
            if (filter.Count > obj.Count) return false;
            for (int i = 0; i < filter.Count; i++)
            {
                if (obj[i] != filter[i]) return false;
            }

            return true;
        }

        return items.Where(x => IsParent(_lookup[x].Path, _lookup[parent].Path)).ToList();
    }

    private List<T> GetRegistryItems<T>(string name, ModelBase? parent = null) where T : ModelBase
    {
        return Filter(_registry.Get<T>(name), parent);
    }

    private List<T> FindRegistryItems<T>(string namePattern, ModelBase? parent = null) where T : ModelBase
    {
        return Filter(_registry.Find<T>(namePattern), parent);
    }

    private T? GetRegistryItem<T>(string name, ModelBase? parent = null) where T : ModelBase
    {
        List<T> l = GetRegistryItems<T>(name, parent);
        if (l.Count == 0)
        {
            return null;
        }

        if (l.Count == 1)
        {
            return l[0];
        }

        throw new InvalidOperationException("there are more than one item");
    }
    public List<T> GetPropertiesValues<T>(string name, ModelBase? parent = null)
    {
        List<Property> list = Filter(_registry.Get<Property>(name), parent);
        return list.Select(x => (T)x.Value!).ToList();
    }

    public List<Property> GetProperties(string name, ModelBase? parent = null)
    {
        return GetRegistryItems<Property>(name, parent);
    }

    public List<Property> FindProperties(string namePattern, ModelBase? parent = null)
    {
        return FindRegistryItems<Property>(namePattern, parent);
    }

    public List<Property> GetAllProperties()
    {
        return _registry.GetAll<Property>();
    }

    public Property? GetProperty(string name, ModelBase? parent = null)
    {
        return GetRegistryItem<Property>(name, parent);
    }

    public List<T> GetVariablesValues<T>(string name, ModelBase? parent = null)
    {
        List<Variable> list = Filter(_registry.Get<Variable>(name), parent);
        return list.Select(x => (T)x.Value!).ToList();
    }

    public List<Variable> GetVariables(string name, ModelBase? parent = null)
    {
        return GetRegistryItems<Variable>(name, parent);
    }

    public List<Variable> FindVariables(string namePattern, ModelBase? parent = null)
    {
        return FindRegistryItems<Variable>(namePattern, parent);
    }

    public List<Variable> GetAllVariables()
    {
        return _registry.GetAll<Variable>();
    }

    public Variable? GetVariable(string name, ModelBase? parent = null)
    {
        return GetRegistryItem<Variable>(name, parent);
    }

    public List<Actor> GetActors(string name, ModelBase? parent = null)
    {
        return GetRegistryItems<Actor>(name, parent);
    }

    public List<Actor> FindActors(string namePattern, ModelBase? parent = null)
    {
        return FindRegistryItems<Actor>(namePattern, parent);
    }

    public List<Actor> GetAllActors()
    {
        return _registry.GetAll<Actor>();
    }

    public Actor? GetActor(string name, ModelBase? parent = null)
    {
        return GetRegistryItem<Actor>(name, parent);
    }

    public List<UObject> GetObjects(string name, ModelBase? parent = null)
    {
        return GetRegistryItems<UObject>(name, parent);
    }

    public List<UObject> FindObjects(string namePattern, ModelBase? parent = null)
    {
        return FindRegistryItems<UObject>(namePattern, parent);
    }

    public List<UObject> GetAllObjects()
    {
        return _registry.GetAll<UObject>();
    }

    public UObject? GetObject(string name, ModelBase? parent = null)
    {
        return GetRegistryItem<UObject>(name, parent);
    }

    public List<Component> GetComponents(string name, ModelBase? parent = null)
    {
        return GetRegistryItems<Component>(name, parent);
    }

    public List<Component> FindComponents(string namePattern, ModelBase? parent = null)
    {
        return FindRegistryItems<Component>(namePattern, parent);
    }

    public List<Component> GetAllComponents()
    {
        return _registry.GetAll<Component>();
    }

    public Component? GetComponent(string name, ModelBase? parent = null)
    {
        return GetRegistryItem<Component>(name, parent);
    }

    public Dictionary<string, List<string>> GetSearchableNames()
    {
        return _registry.GetNames();
    }
}

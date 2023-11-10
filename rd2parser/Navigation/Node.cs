using System.Text;
using rd2parser.Model;
using rd2parser.Model.Properties;

namespace rd2parser.Navigation;
public class Node
{
    private readonly ModelBase _object;
    public Node? Parent { get; set; }
    public List<Segment> Path { get; set; }

    private List<Node>? _children;
    private readonly object _lock = new ();

    public List<Node> Children
    {
        get
        {
            if (_children == null)
            {
                lock (_lock)
                {
                    _children ??= GetChildren().ToList();
                }
            }
            return _children;
        }
    }

    public ModelBase Object => _object;

    public Node(ModelBase obj)
    {
        _object = obj;
        Path = new() { new() { Name = GetName(obj), Type = obj.GetType().Name } };
    }

    public Node(ModelBase obj, int? index, Node parent)
    {
        _object = obj;
        Parent = parent;
        Path = new(parent.Path) { new() { Name = GetName(obj), Type = obj.GetType().Name, Index = index } };
    }

    private static string GetName(ModelBase item)
    {
        return item switch
        {
            Variable x => x.Name.Name,
            Variables x => x.Name.Name,
            UObject x => x.LoadedData?.Name.Name,
            Component x => x.ComponentKey,
            TextPropertyData0 x => x.Key,
            Property x => x.Name.Name,
            ObjectProperty x => x.ClassName,
            EnumProperty x => x.EnumType.Name,
            ByteProperty x => x.EnumName.Name,
            Actor x => x.DynamicData?.ClassPath.Name,
            _ => ""
        } ?? "";
    }

    private IEnumerable<Node> GetChildren()
    {
        foreach ((ModelBase obj, int? index) in _object.GetChildren())
        {
            yield return new(obj, index, this);
        }
    }
    public string DisplayPath
    {
        get
        {
            StringBuilder sb = new();
            foreach (Segment segment in Path)
            {
                string name = string.IsNullOrEmpty(segment.Name) ? "" : $"({segment.Name})";
                string index = segment.Index == null ? "" : $"[{segment.Index}]";
                sb.Append($"{index}{segment.Type}{name}.");
            }
            return sb.ToString()[..(sb.Length - 1)];
        }
    }

    public override string? ToString()
    {
        return _object.ToString();
    }
}

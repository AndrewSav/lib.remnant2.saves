using System.Text;
using Newtonsoft.Json;
using rd2parser.Model;
using rd2parser.Model.Properties;

namespace rd2parser;
public abstract class Node
{
    protected Node()
    {
        Path = new();
    }

    protected Node(Node? parent, List<Segment> path)
    {
        Parent = parent;
        Path = path;
    }

    [JsonIgnore]
    public Node? Parent { get; set; }
    [JsonIgnore]
    public List<Segment> Path { get;  set; }
    protected static void AddIndexToChild(object child, int index)
    {
        switch (child)
        {
            case Node n:
                n.Path[^1].Index = index;
                break;
            case FName:
                break;
        }
    }

    [JsonIgnore]
    public string DisplayPath
    {
        get
        {
            StringBuilder sb = new();
            foreach (Segment segment in Path)
            {
                string name = segment.Name == null ?"" : $"({segment.Name})";
                string index = segment.Index == null ? "" : $"[{segment.Index}]";
                sb.Append($"{index}{segment.Type}{name}.");
            }
            return sb.ToString()[..(sb.Length - 1)];
        }
    }

    public virtual Node Copy()
    {
        throw new NotImplementedException();
    }
}

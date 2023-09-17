using System.Text;
using Newtonsoft.Json;
using rd2parser.Model;

namespace rd2parser.Navigation;
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
    public List<Segment> Path { get; set; }

    
    [JsonIgnore] //TODO: remove temp
    public int ReadOffset; // For debugging, reader offset where the object starts when read
    public int WriteOffset; // For debugging, writer offset where the object starts when written
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
                string name = segment.Name == null ? "" : $"({segment.Name})";
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

    public abstract IEnumerable<Node> GetChildren();

}

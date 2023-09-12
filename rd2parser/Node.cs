using Newtonsoft.Json;
using rd2parser.Model;

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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rd2parser.Model;

namespace rd2parser.Navigation;
public class Navigator
{
    private SaveFile _saveFile;
    private Node _root;
    private ItemRegistry _registry = new();

    public Navigator(SaveFile saveFile)
    {
        _saveFile = saveFile;
        _root = new Node(_saveFile.SaveData);
        Queue<Node> q = new();
        q.Enqueue(_root);
        while (q.Count > 0)
        {
            Node n = q.Dequeue();
            foreach (Node c in n.Children)
            {
                q.Enqueue(c);
            }
        }
    }
}

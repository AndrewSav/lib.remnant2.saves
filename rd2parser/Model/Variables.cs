using System.Diagnostics.CodeAnalysis;
using rd2parser.Navigation;

namespace rd2parser.Model;

public class Variables : Node
{
    public required FName Name;
    public required ulong Unknown;
    public required List<KeyValuePair<string, Variable>> Properties;

    public Variables()
    {
    }

    public Variables(Node? parent, string name) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Name = name, Type = "Variables" });
    }

    [SetsRequiredMembers]
    public Variables(Reader r, SerializationContext ctx, Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Properties = new();
        Name = new FName(r, ctx.NamesTable);
        Path.Add(new() { Name = Name.Name, Type = "Variables" });
        Unknown = r.Read<ulong>();
        if (Unknown != 0)
        {
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown byte at {Location}, {Offset}", Unknown, DisplayPath, r.Position);
        }
        int len = r.Read<int>();

        for (int i = 0; i < len; i++)
        {
            Variable v = new(r, ctx,this);
            v.Path[^1].Index = i;
            Properties.Add(new KeyValuePair<string, Variable>(v.Name.Name,v));
        }
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        Name.Write(w, ctx);
        w.Write(Unknown);
        w.Write(Properties.Count);
        foreach (KeyValuePair<string, Variable> keyValuePair in Properties)
        {
            keyValuePair.Value.Write(w, ctx);
        }
    }
    public override IEnumerable<Node> GetChildren()
    {
        foreach (Variable v in Properties.Select(x => x.Value))
        {
            yield return v;
        }
    }
}

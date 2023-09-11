using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace rd2parser.Model;

public class Variables
{
    public required FName Name;
    public required ulong Unknown;
    public required List<KeyValuePair<string, Variable>> Properties;

    public Variables()
    {
    }

    [SetsRequiredMembers]
    public Variables(Reader r, SerializationContext ctx)
    {
        Properties = new();
        Name = new FName(r, ctx.NamesTable);
        Unknown = r.Read<ulong>();
        int len = r.Read<int>();

        for (int i = 0; i < len; i++)
        {
            Variable v = new(r, ctx);
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
}

using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model;

public class Variables : ModelBase
{
    public required FName Name;
    public required ulong Unknown;
    public required List<KeyValuePair<string, Variable>> Items;

    public Variables()
    {
    }

    [SetsRequiredMembers]
    public Variables(Reader r, SerializationContext ctx)
    {
        ReadOffset = r.Position + ctx.ContainerOffset;
        Items = new();
        Name = new FName(r, ctx.NamesTable);
        Unknown = r.Read<ulong>();
        if (Unknown != 0)
        {
            Log.Logger.Warning("unexpected non-zero value {value} of an unknown byte at {Offset}", Unknown, r.Position);
        }
        int len = r.Read<int>();

        for (int i = 0; i < len; i++)
        {
            Variable v = new(r, ctx);
            Items.Add(new KeyValuePair<string, Variable>(v.Name.Name,v));
        }
        ReadLength = r.Position + ctx.ContainerOffset - ReadOffset;
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        WriteOffset = (int)w.Position + ctx.ContainerOffset;
        Name.Write(w, ctx);
        w.Write(Unknown);
        w.Write(Items.Count);
        foreach (KeyValuePair<string, Variable> keyValuePair in Items)
        {
            keyValuePair.Value.Write(w, ctx);
        }
        WriteLength = (int)w.Position + ctx.ContainerOffset - WriteOffset;
    }
    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        for (int index = 0; index < Items.Count; index++)
        {
            KeyValuePair<string, Variable> v = Items[index];
            yield return (v.Value, index);
        }
    }
}

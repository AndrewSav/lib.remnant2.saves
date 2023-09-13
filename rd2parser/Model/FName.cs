using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model;

public class FName
{
    public required ushort Index;
    public required int? Number;
    public required string Name;

    public FName()
    {

    }

    [SetsRequiredMembers]
    public FName(Reader r, List<string> namesTable)
    {
        const ushort hasNumberBit = 1 << 15;
        ushort i = r.Read<ushort>();
        bool hasNumber = (i & hasNumberBit) != 0;
        if (hasNumber)
        {
            Index = (ushort)(i & ~hasNumberBit);
            Number = r.Read<int>();
        }
        else
        {
            Index = i;
        }
        if (Index >= namesTable.Count)
        {
            throw new ApplicationException("invalid name index");
        }
        Name = namesTable[Index];
    }

    public void Write(Writer w, SerializationContext ctx)
    {
        const ushort hasNumberBit = 1 << 15;
        int readIndex = ctx.GetNamesTableIndex(Name);
        if (readIndex == -1)
        {
            readIndex = ctx.NamesTable.Count;
            ctx.NamesTable.Add(Name);
        }

        ushort index = (ushort)readIndex;

        if (Number != null)
        {
            index |= hasNumberBit;
        }

        w.Write(index);

        if (Number != null)
        {
            w.Write(Number.Value);
        }
    }

    // To make it easier to navigate in the debugger
    public override string ToString()
    {
        return Name;
    }
}

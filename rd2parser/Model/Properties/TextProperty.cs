using System.Diagnostics.CodeAnalysis;

namespace rd2parser.Model.Properties;

public class TextProperty
{
    public required uint Flags;
    public required byte HistoryType;
    public required object Value;

    public TextProperty()
    {

    }

    [SetsRequiredMembers]
    public TextProperty(Reader r)
    {
        Flags = r.Read<uint>();
        HistoryType = r.Read<byte>();
        Value = HistoryType switch
        {
            0 => new TextPropertyData0(r),
            255 => new TextPropertyData255(r),
            _ => throw new ApplicationException("unsupported history type"),
        };
    }

    public void Write(Writer w)
    {
        w.Write(Flags);
        w.Write(HistoryType);
        switch (HistoryType)
        {
            case 0:
                ((TextPropertyData0)Value).Write(w);
                break;
            case 255:
                ((TextPropertyData255)Value).Write(w);
                break;
            default:
                throw new ApplicationException("unsupported history type");
        }
    }
}
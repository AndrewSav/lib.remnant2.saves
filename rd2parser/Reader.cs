using System.Text;
using rd2parser.IO;
using rd2parser.Model.Properties;

namespace rd2parser;

public class Reader : ReaderBase
{
    public Reader(byte[] buffer) : base(buffer)
    {

    }    

    public string? ReadFString()
    {
        int len = Read<int>();
        if (len == 0) 
            return null;
        if (len < 0)
        {
            throw new ApplicationException("FString length is not positive");
        }
        if (len + Position > Size)
        {
            throw new ApplicationException("FString length is too large");
        }
        string result = Encoding.ASCII.GetString(ReadBytes(len-1));
        byte zero = Read<byte>();
        if (zero != 0)
        {
            throw new ApplicationException("did not read expected string terminator");
        }
        return result;
    }

    public List<KeyValuePair<string, Property>> ReadProperties(SerializationContext ctx)
    {
        List<KeyValuePair<string, Property>> result = new();
        while (true)
        {
            Property p = new(this, ctx);
            if (p.Name.Name == "None")
            {
                break;
            }
            result.Add(new KeyValuePair<string, Property>(p.Name.Name, p));
        }
        return result;
    }
}

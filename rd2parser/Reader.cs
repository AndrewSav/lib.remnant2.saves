using System.Text;
using rd2parser.IO;

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
}

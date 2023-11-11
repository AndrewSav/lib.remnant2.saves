using System.Text;
using rd2parser.IO;

namespace rd2parser;
public class Writer : WriterBase
{
    public void WriteFString(string? value)
    {
        if (value == null)
        {
            Write(0);
            return;
        }
        Write(value.Length + 1);
        WriteBytes(Encoding.ASCII.GetBytes(value));
        Write<byte>(0);
    }
}

using System.Text;
using rd2parser.IO;
using rd2parser.Model;
using rd2parser.Model.Properties;

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

    public void WriteProperties(SerializationContext ctx, List<KeyValuePair<string, Property>> properties)
    {
        foreach (KeyValuePair<string, Property> keyValuePair in properties)
        {
            keyValuePair.Value.Write(this, ctx);
        }

        ushort index = (ushort)ctx.GetNamesTableIndex("None");
        new Property{Name=new FName{Name = "None",Index = index,Number=null}}.Write(this, ctx);
    }
}

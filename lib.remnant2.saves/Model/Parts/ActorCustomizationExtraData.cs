namespace lib.remnant2.saves.Model.Parts;

public class ActorCustomizationExtraData : ModelBase
{
    public int Unknown0;
    public int Unknown1;
    public int? Unknown2;
    public int? Unknown3;
    public byte[] ExtraData = [];

    public ActorCustomizationExtraData()
    {
    }

    public ActorCustomizationExtraData(byte[] data, int readOffset)
    {
        if (!CanRead(data))
        {
            throw new InvalidOperationException("invalid ActorCustomization extra data length");
        }

        Reader r = new(data);
        ReadOffset = readOffset;
        Unknown0 = r.Read<int>();
        Unknown1 = r.Read<int>();
        if (!r.IsEof)
        {
            Unknown2 = r.Read<int>();
            Unknown3 = r.Read<int>();
            ExtraData = r.ReadBytes(r.Size - r.Position);
        }

        ReadLength = r.Position;
    }

    public static bool CanRead(byte[] data)
    {
        return data.Length is 8 or 30;
    }

    public void Write(Writer w, int containerOffset)
    {
        WriteOffset = (int)w.Position + containerOffset;
        w.Write(Unknown0);
        w.Write(Unknown1);
        if (Unknown2 != null || Unknown3 != null || ExtraData.Length > 0)
        {
            w.Write(Unknown2 ?? 0);
            w.Write(Unknown3 ?? 0);
            w.WriteBytes(ExtraData);
        }

        WriteLength = (int)w.Position + containerOffset - WriteOffset;
    }

    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        yield break;
    }
}

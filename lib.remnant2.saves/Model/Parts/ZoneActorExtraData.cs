using lib.remnant2.saves.Model.Memory;

namespace lib.remnant2.saves.Model.Parts;

public class ZoneActorExtraData : ModelBase
{
    private const int HeaderLength = sizeof(double) + sizeof(double) + 3 * sizeof(double) + sizeof(int) + sizeof(int);
    private const int CoordinateLength = 3 * sizeof(short);

    public double Unknown;
    public double GridResolution;
    public FVector GridOrigin;
    public int ApplyMapOffset;
    public List<FIntVector16> VisitedCoordinates = [];

    public ZoneActorExtraData()
    {
    }

    public ZoneActorExtraData(byte[] data, int readOffset)
    {
        if (!CanRead(data))
        {
            throw new InvalidOperationException("invalid ZoneActor extra data length");
        }

        Reader r = new(data);
        ReadOffset = readOffset;
        Unknown = r.Read<double>();
        GridResolution = r.Read<double>();
        GridOrigin = r.Read<FVector>();
        ApplyMapOffset = r.Read<int>();

        int count = r.Read<int>();
        VisitedCoordinates = new(count);
        for (int i = 0; i < count; i++)
        {
            VisitedCoordinates.Add(r.Read<FIntVector16>());
        }

        ReadLength = r.Position;
    }

    public static bool CanRead(byte[] data)
    {
        if (data.Length < HeaderLength)
        {
            return false;
        }

        int payloadLength = data.Length - HeaderLength;
        if (payloadLength % CoordinateLength != 0)
        {
            return false;
        }

        int count = BitConverter.ToInt32(data, HeaderLength - sizeof(int));
        return count >= 0 && payloadLength == count * CoordinateLength;
    }

    public void Write(Writer w, int containerOffset)
    {
        WriteOffset = (int)w.Position + containerOffset;
        w.Write(Unknown);
        w.Write(GridResolution);
        w.Write(GridOrigin);
        w.Write(ApplyMapOffset);
        w.Write(VisitedCoordinates.Count);
        foreach (FIntVector16 coordinate in VisitedCoordinates)
        {
            w.Write(coordinate);
        }

        WriteLength = (int)w.Position + containerOffset - WriteOffset;
    }

    public override IEnumerable<(ModelBase obj, int? index)> GetChildren()
    {
        yield break;
    }
}

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using rd2parser.Model.Memory;
using rd2parser.Navigation;

namespace rd2parser.Model;

public class PersistenceContainer : Node
{
    public required uint Version;
    public required List<ulong> Destroyed;
    public required List<KeyValuePair<ulong, Actor>> Actors;

    public PersistenceContainer()
    {

    }

    public PersistenceContainer(Node? parent) : base(parent, new List<Segment>(parent!.Path))
    {
        Path.Add(new() { Type = "PersistenceContainer" });
    }

    [SetsRequiredMembers]
    public PersistenceContainer(Reader r, SerializationContext ctx, Node? parent, int containerOffset) : this(parent)
    {
        ReadOffset = r.Position + containerOffset;
        Version = r.Read<uint>();
        int indexOffset = r.Read<int>();
        int dynamicOffset = r.Read<int>();

        r.Position = indexOffset;
        uint infoCount = r.Read<uint>();
        List<FInfo> actorInfo = new();
        for (uint i = 0; i < infoCount; i++)
        {
            actorInfo.Add(r.Read<FInfo>());
        }

        Destroyed = new();
        uint destroyedCount = r.Read<uint>();
        for (uint i = 0; i < destroyedCount; i++)
        {
            Destroyed.Add(r.Read<ulong>());
        }

        Actors = new();
        for (int index = 0; index < actorInfo.Count; index++)
        {
            FInfo info = actorInfo[index];
            r.Position = info.Offset;
            byte[] actorBytes = r.ReadBytes(info.Size);
            Reader actorReader = new(actorBytes);
            Actor a = new(actorReader, ctx, this, info.Offset+ containerOffset);
            a.Path[^1].Index = index;
            Actors.Add(new KeyValuePair<ulong, Actor>(info.UniqueID, a));
        }

        r.Position = dynamicOffset;
        uint dynamicCount = r.Read<uint>();
        for (uint i = 0; i < dynamicCount; i++)
        {
            DynamicActor da = new(r);
            Actor a = Actors.Single(x => x.Key == da.UniqueId).Value;
            a.DynamicData = da;
            if (a.DynamicData.ClassPath.Name != null)
            {
                ctx.Registry.Add(a.DynamicData.ClassPath.Name, a);
                a.Path[^1].Name = a.DynamicData.ClassPath.Name;
            }
        }
    }

    // To make it easier to navigate in the debugger
    [JsonIgnore]
    public List<Dictionary<string, UObject>> Children =>
        Actors.Select(
                x => x.Value.Archive.Objects
                    .Select((input, index) => new { index, input })
                    .ToDictionary(o => o.index + "|" + (o.input.ObjectPath ?? ""),o => o.input))
            .ToList();

    // To make it easier to navigate in the debugger
    [JsonIgnore]
    public List<UObject> FlattenChildren => Children.SelectMany(x => x.Values).ToList();

    public void Write(Writer w, int containerOffset)
    {
        WriteOffset = (int)w.Position + containerOffset;
        w.Write(Version);
        long patchOffset = (int)w.Position;
        w.Write(0); // index offset, unknown yet, will patch later
        w.Write(0); // dynamic offset, unknown yet, will patch later
        List<FInfo> actorInfo = new();
        foreach (KeyValuePair<ulong, Actor> a in Actors)
        {
            Writer actorWriter = new();
            a.Value.WriteNonDynamic(actorWriter, (int)w.Position+ containerOffset);
            byte []actorData = actorWriter.ToArray();
            actorInfo.Add(new()
            {
                UniqueID = a.Key,
                Offset = (int)w.Position,
                Size = actorData.Length
            });
            w.WriteBytes(actorData);
        }
        int dynamicOffset = (int)w.Position;
        w.Write(Actors.Count(x => x.Value.DynamicData != null));
        foreach (KeyValuePair<ulong, Actor> a in Actors)
        {
            a.Value.DynamicData?.Write(w);
        }
        int indexOffset = (int)w.Position;
        w.Write(actorInfo.Count);
        foreach (FInfo info in actorInfo)
        {
            w.Write(info);
        }
        w.Write(Destroyed.Count);
        foreach (ulong d in Destroyed)
        {
            w.Write(d);
        }

        long endOffset = w.Position;
        w.Position = patchOffset;
        w.Write(indexOffset);
        w.Write(dynamicOffset);
        w.Position = endOffset;
    }
    public override IEnumerable<Node> GetChildren()
    {
        foreach (Actor a in Actors.Select(x => x.Value))
        {
            yield return a;
        }
    }
}

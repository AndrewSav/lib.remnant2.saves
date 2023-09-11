using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using rd2parser.Model.Memory;

namespace rd2parser.Model;

public class PersistenceContainer
{
    public required uint Version;
    public required List<ulong> Destroyed;
    public required List<KeyValuePair<ulong, Actor>> Actors;

    public PersistenceContainer()
    {

    }

    [SetsRequiredMembers]
    public PersistenceContainer(Reader r)
    {
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
        foreach (var info in actorInfo)
        {
            r.Position = info.Offset;
            byte[] actorBytes = r.ReadBytes(info.Size);
            Reader actorReader = new(actorBytes);
            Actors.Add(new KeyValuePair<ulong, Actor>(info.UniqueID, new Actor(actorReader)));
        }

        r.Position = dynamicOffset;
        uint dynamicCount = r.Read<uint>();
        for (uint i = 0; i < dynamicCount; i++)
        {
            DynamicActor da = new(r);
            Actors.Single( x => x.Key == da.UniqueId).Value.DynamicData = da;
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

    public void Write(Writer w)
    {
        w.Write(Version);
        long patchOffset = (int)w.Position;
        w.Write(0); // index offset, unknown yet, will patch later
        w.Write(0); // dynamic offset, unknown yet, will patch later
        List<FInfo> actorInfo = new();
        foreach (KeyValuePair<ulong, Actor> a in Actors)
        {
            Writer actorWriter = new();
            a.Value.WriteNonDynamic(actorWriter);
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
            if (a.Value.DynamicData == null) continue;
            a.Value.DynamicData.Write(w);
        }
        int indexOffset = (int)w.Position;
        w.Write(actorInfo.Count);
        foreach (var info in actorInfo)
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
}
﻿using System.Diagnostics.CodeAnalysis;
using rd2parser.Model.Memory;
using rd2parser.Navigation;

namespace rd2parser.Model;

public class Actor : Node
{
    public required uint HasTransform;
    public FTransform? Transform;
    public required SaveData Archive;
    // DynamicData block is read after actors have been read
    public ActorDynamicData? DynamicData;

    public Actor()
    {
    }


    [SetsRequiredMembers]

    public Actor(Reader r, SerializationContext ctx, int containerOffset)
    {
        ReadOffset = r.Position + containerOffset;
        HasTransform = r.Read<uint>();
        if (HasTransform != 0)
        {
            Transform = r.Read<FTransform>();
        }
        Archive =  new SaveData(r, false, false, containerOffset,ctx.Options);
        ReadLength = r.Position + containerOffset - ReadOffset; // Does not include DynamicData
    }

    public void WriteNonDynamic(Writer w, int containerOffset)
    {
        WriteOffset = (int)w.Position + containerOffset;
        w.Write(HasTransform);
        if (HasTransform != 0)
        {
            w.Write(Transform!.Value);
        }
        Archive.Write(w, containerOffset);
        WriteLength = (int)w.Position + containerOffset - WriteOffset; // Does not include DynamicData
    }


    public override string? ToString()
    {
        if (DynamicData?.ClassPath.Name != null)
        {
            if (DynamicData.ClassPath.Name == "ZoneActor")
            {
                string? label = Archive.Objects[0].Properties?["Label"].ToString();
                return $"ZoneActor({label})";
            }
            return DynamicData.ClassPath.Name;
        }
        return base.ToString();
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Archive;
    }
}

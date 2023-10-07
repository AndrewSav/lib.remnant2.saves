﻿using rd2parser.Model;
using rd2parser.Model.Properties;

namespace rd2parser.examples;

internal partial class Example
{
    // This is probably most useful connected to File System Watcher,
    // and watched as you traverse areas, changes system clock, reload the game, etc
    public static void BloodMoon()
    {
        Console.WriteLine("Blood Moon===========");

        string folder = Utils.GetSteamSavePath();
        string savePath =
            Path.Combine(folder, Environment.GetEnvironmentVariable("DEBUG_REMNANT_SAVE") ?? "save_0.sav");

        SaveFile sf = SaveFile.Read(savePath);

        UObject main = sf.GetObject("/Game/Maps/Main.Main:PersistentLevel")!;
        UObject campaignMeta = sf.FindActors("Quest_Campaign", main)!.Single().Archive.Objects[0];
        int campaignId = campaignMeta.Properties!["ID"].Get<int>();
        UObject campaignObject = sf.GetObject($"/Game/Quest_{campaignId}_Container.Quest_Container:PersistentLevel")!;

        UObject? adventureMeta = sf.FindActors("Quest_AdventureMode", main)!.SingleOrDefault()?.Archive.Objects[0];
        int? adventureId = adventureMeta?.Properties!["ID"].Get<int>();
        UObject? adventureObject = sf.GetObjects($"/Game/Quest_{adventureId}_Container.Quest_Container:PersistentLevel")?[0];

        Console.WriteLine("Campaign");
        PrintBloodMoonData(sf, campaignMeta, campaignObject);
        if (adventureMeta != null && adventureObject != null)
        {
            Console.WriteLine("Adventure");
            PrintBloodMoonData(sf, adventureMeta, adventureObject);
        }
    }

    private static void PrintBloodMoonData(SaveFile sf, UObject campaignMeta, UObject campaignObject)
    {
        Variable? isBloodMoon = sf.GetVariable("IsBloodMoon", campaignMeta);
        List<UObject>? bloodMoonQuest = sf.FindObjects("Quest_Event_Bloodmoon_C", campaignObject);
        if (isBloodMoon == null && bloodMoonQuest == null)
        {
            Console.WriteLine("No Blood Moon data found");
            return;
        }

        if (isBloodMoon != null)
        {
            string message = (uint)isBloodMoon.Value! != 0 ? "true" : "false";
            Console.WriteLine($"IsBloodMoon: {message}");
        }

        if (bloodMoonQuest is { Count: > 0 })
        {
            Component? c = sf.GetComponent("Bloodmoon", bloodMoonQuest[0]);
            if (c == null) return;
            foreach (KeyValuePair<string, Property> kvp in c.Properties!.Properties)
            {
                if (kvp.Value.Type!.Name == "StructProperty")
                {
                    StructProperty sp = (StructProperty)kvp.Value.Value!;
                    if (sp.Type.Name == "DateTime" && sp.Value is DateTime dt)
                    {
                        Console.WriteLine($"{kvp.Key}: {dt.ToLocalTime()}");
                        continue;
                    }
                }

                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }
    }
}
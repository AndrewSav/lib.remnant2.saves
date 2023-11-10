using rd2parser.Model;
using rd2parser.Model.Properties;
using rd2parser.Navigation;

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
        Navigator navigator = new(sf);

        UObject main = navigator.GetObject("/Game/Maps/Main.Main:PersistentLevel")!;
        UObject campaignMeta = navigator.FindActors("Quest_Campaign", main).Single().Archive.Objects[0];
        int campaignId = campaignMeta.Properties!["ID"].Get<int>();
        UObject campaignObject = navigator.GetObject($"/Game/Quest_{campaignId}_Container.Quest_Container:PersistentLevel")!;

        UObject? adventureMeta = navigator.FindActors("Quest_AdventureMode", main).SingleOrDefault()?.Archive.Objects[0];
        int? adventureId = adventureMeta?.Properties!["ID"].Get<int>();
        UObject? adventureObject = navigator.GetObject($"/Game/Quest_{adventureId}_Container.Quest_Container:PersistentLevel");

        Console.WriteLine("Campaign");
        PrintBloodMoonData(navigator, campaignMeta, campaignObject);
        if (adventureMeta != null && adventureObject != null)
        {
            Console.WriteLine("Adventure");
            PrintBloodMoonData(navigator, adventureMeta, adventureObject);
        }
    }

    private static void PrintBloodMoonData(Navigator navigator, UObject campaignMeta, UObject campaignObject)
    {
        Variable? isBloodMoon = navigator.GetVariable("IsBloodMoon", campaignMeta);
        List<UObject>? bloodMoonQuest = navigator.FindObjects("Quest_Event_Bloodmoon_C", campaignObject);
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
            Component? c = navigator.GetComponent("Bloodmoon", bloodMoonQuest[0]);
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
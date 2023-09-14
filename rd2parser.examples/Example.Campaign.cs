using System.Text.RegularExpressions;
using rd2parser.Model;
using rd2parser.Model.Properties;

namespace rd2parser.examples;

internal partial class Example
{
    public static void Campaign()
    {
        Console.WriteLine("Campaign===========");

        Dictionary<string,string> worlds = new()
        {
            { "Jungle", "Yaesha" },
            { "Nerud", "Nerud" },
            { "Fae", "Losomn" }
        };


        string folder = Utils.GetSteamSavePath();
        string savePath = Path.Combine(folder, Environment.GetEnvironmentVariable("DEBUG_REMNANT_SAVE") ?? "save_0.sav");
        //string savePath = Path.Combine(folder, "save_3.sav");

        SaveFile sf = SaveFile.Read(savePath);

        float timePlayed = (float)sf.GetProperty("TimePlayed")!.Value!;
        TimeSpan tp = TimeSpan.FromSeconds(timePlayed);
        string timePlayedString = $"{(int)tp.TotalHours}:{tp.Minutes:D2}:{tp.Seconds:D2}";
        Console.WriteLine($"Time Played: {timePlayedString}");

        int slot = (int)sf.GetProperty("LastActiveRootSlot")!.Value!;

        string mode = slot == 0 ? "campaign" : "adventure";
        Console.WriteLine($"You are playing in {mode} mode");

        Property slot0 = sf.GetProperties("SlotID")!.SingleOrDefault(x => (int)x.Value! == 0)!;
        PrintMode(slot0, "campaign");
        Property? slot1 = sf.GetProperties("SlotID")!.SingleOrDefault(x => (int)x.Value! == 1);
        if (slot1 != null)
        {
            string world = worlds[Regex.Match(slot1.Path[^3].Name!, @"Quest_AdventureMode_(\w+)_C").Groups[1].Value];
            PrintMode(slot1, "adventure", $" ({world})");
        }
        else
        {
            Console.WriteLine("You do not have adventure mode");
        }

        /*
        var dyn =  sf.GetProperties("DynamicResources");

        foreach (Property p in dyn)
        {
            ArrayStructProperty s = p.Get<ArrayStructProperty>();
            //if (s.Items.Any(x => ((string)x).Contains("LivingStone")))
            //{
            //}
            foreach (string ss in s.Items)
            {
                //Console.WriteLine(ss);    
            }

        }

        var spw = sf.GetProperties("Tag");
        */

    }

    private static void PrintMode(Property slot, string mode, string world = "")
    {
        string[] difficulties =
        {
            "None",
            "Survivor",
            "Veteran",
            "Nightmare",
            "Apocalypse"
        };
        
        TimeSpan tp2 = slot.Bag["PlayTime"].Get<TimeSpan>();
        string timePlayedString2 = $"{(int)tp2.TotalHours}:{tp2.Minutes:D2}:{tp2.Seconds:D2}";
        int difficulty = slot.Bag.Contains("Difficulty") ? slot.Bag["Difficulty"].Get<int>() : 1;
        Console.WriteLine($"Mode: {mode}{world}, difficulty: {difficulties[difficulty]}, time played: {timePlayedString2}");
    }
}

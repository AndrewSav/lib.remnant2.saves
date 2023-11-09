//using System.Text.RegularExpressions;
//using rd2parser.Model;
//using rd2parser.Model.Properties;

//namespace rd2parser.examples;

//internal partial class Example
//{
//    public static void Campaign()
//    {
//        Console.WriteLine("Campaign===========");

//        Dictionary<string, string> worlds = new()
//        {
//            { "Jungle", "Yaesha" },
//            { "Nerud", "Nerud" },
//            { "Fae", "Losomn" }
//        };


//        string folder = Utils.GetSteamSavePath();
//        string savePath =
//            Path.Combine(folder, Environment.GetEnvironmentVariable("DEBUG_REMNANT_SAVE") ?? "save_0.sav");

//        SaveFile sf = SaveFile.Read(savePath);

//        float timePlayed = (float)sf.GetProperty("TimePlayed")!.Value!;
//        TimeSpan tp = TimeSpan.FromSeconds(timePlayed);
//        string timePlayedString = $"{(int)tp.TotalHours}:{tp.Minutes:D2}:{tp.Seconds:D2}";
//        Console.WriteLine($"Time Played: {timePlayedString}");

//        Dictionary<string, string> quests = new()
//        {
//            //ReSharper disable StringLiteralTypo
//            { "Quest_Boss_Faelin", "Faelin" },
//            { "Quest_Boss_Faerlin", "Faerin" },
//            { "Quest_Boss_ManyFaces", "Corruptor" },
//            { "Quest_Boss_NerudGuardian", "Sha'Hala" },
//            { "Quest_Boss_NightWeaver", "The Nightweaver" },
//            { "Quest_Boss_Ravager", "Corrupted Ravager" },
//            { "Quest_Boss_TalRatha", "Tal'Ratha" },
//            { "Quest_Earth_TowerBoss", "Annihilation" },
//            { "Quest_Global_RootWalkerDream", "Bane" },
//            { "Quest_Miniboss_Abomination", "Abomination" },
//            { "Quest_Miniboss_BloatKing", "Bloat King" },
//            { "Quest_Miniboss_CustodianEye", "The Custodian's Eye" },
//            { "Quest_Miniboss_DranGrenadier", "Gwentdil The Unburnt" },
//            { "Quest_Miniboss_FaeArchon", "Magister Dullain" },
//            { "Quest_Miniboss_Hatchery", "Primogenitor" },
//            { "Quest_Miniboss_MotherMind", "Mother Mind" },
//            { "Quest_Miniboss_Phantom", "The Astropath" },
//            { "Quest_Miniboss_RedPrince", "The Red Prince" },
//            { "Quest_Miniboss_RootCultist", "Legion" },
//            { "Quest_Miniboss_RootHorror", "Kaeula's Shadow" },
//            { "Quest_Miniboss_Shrewd", "Shrewd" },
//            { "Quest_OverworldPOI_DranOracle", "Dran Orcale" },
//            { "Quest_SideD_CathedralOfSeasons", "Cathedral of Omens" },
//            { "Quest_SideD_CharnelHouse", "Harvester's Reach" },
//            { "Quest_SideD_CrimsonHarvest", "Butchers Quarter" },
//            { "Quest_SideD_Extermination", "Dormant N'Erudian Facility" },
//            { "Quest_SideD_Extraction", "The Dark Conduit" },
//            { "Quest_SideD_FaeCouncil", "Council Chamber" },
//            { "Quest_SideD_HiddenMaze", "Endaira's End" },
//            { "Quest_SideD_Ravenous", "The Great Hall" },
//            { "Quest_SideD_StasisSiege", "Vault of the Formless" },
//            { "Quest_SideD_TheChimney", "The Chimney" },
//            { "Quest_SideD_TheDreamlessSleep_POI", "Void Vessel Facility" },
//            { "Quest_SideD_TheLament", "The Lament" },
//            { "Quest_SideD_TheTangle", "Imperial Gardens" },
//            { "Quest_SideD_ThreeMenMorris", "Postulant's Parlor" },
//            { "Quest_SideD_TowerHeist", "Tower of the Unseen" },
//            { "Quest_SideD_TownTurnToDust", "Tiller's Rest" },
//            { "Quest_SideD_Train", "Terminus Station" },
//            { "Quest_SideD_WailingField", "Forgotten Field" },
//            { "Quest_Story_AllFather", "Story - All Father (Beautific Palace)" },
//            { "Quest_Story_Asylum", "Story - Asylum (Morrow Parish)" },
//            { "Quest_Story_Empress", "Story - Empress (Red Throne)" },
//            { "Quest_Story_IAmLegend", "Story - I Am Legend (Forgotten Prison)" },
//            { "Quest_Story_Labyrinth", "Story - Labyrinth" },
//            { "Quest_Story_Ravager", "Story - Ravager (Forbidden Grove)" },
//            { "Quest_Story_RootEarth_Zone1", "Cancer" },
//            { "Quest_Story_RootEarth_Zone3", "Venom" },
//            { "Quest_Story_TheCore", "Story - The Core (Seeker's Rest)" },
//            { "MainCampaign", "Main Campaign" },
//            // ReSharper restore StringLiteralTypo
//        };

//        Property? qcl = sf.GetProperty("QuestCompletedLog");
//        if (qcl != null)
//        {
//            Console.WriteLine("Quest Completed Log");
//            foreach (object? q in sf.GetProperty("QuestCompletedLog")!.Get<ArrayProperty>().Items)
//            {
//                FName quest = (FName)q!;
//                if (quest.ToString() == "None") continue;
//                string line = quests.ContainsKey(quest.ToString()) ? quests[quest.ToString()] : quest.ToString();
//                Console.WriteLine($"  {line}");
//            }
//        }

//        int slot = (int)sf.GetProperty("LastActiveRootSlot")!.Value!;

//        string mode = slot == 0 ? "campaign" : "adventure";
//        Console.WriteLine($"You are playing in {mode} mode");

//        Property slot0 = sf.GetProperties("SlotID")!.SingleOrDefault(x => (int)x.Value! == 0)!;
//        PrintMode(slot0, "campaign");
//        Property? slot1 = sf.GetProperties("SlotID")!.SingleOrDefault(x => (int)x.Value! == 1);
//        if (slot1 != null)
//        {
//            string world = worlds[Regex.Match(slot1.Path[^3].Name!, @"Quest_AdventureMode_(\w+)_C").Groups[1].Value];
//            PrintMode(slot1, "adventure", $" ({world})");
//        }
//        else
//        {
//            Console.WriteLine("You do not have adventure mode");
//        }


//        UObject main = sf.GetObject("/Game/Maps/Main.Main:PersistentLevel")!;

//        Console.WriteLine("Campaign");

//        UObject campaignMeta = sf.FindActors("Quest_Campaign", main)!.Single().Archive.Objects[0];
//        int campaignId = campaignMeta.Properties!["ID"].Get<int>();
//        UObject? campaignObject = sf.GetObject($"/Game/Quest_{campaignId}_Container.Quest_Container:PersistentLevel");

//        int world1 = sf.GetComponent("World1", campaignMeta)!.Properties!["QuestID"].Get<int>();
//        int world2 = sf.GetComponent("World2", campaignMeta)!.Properties!["QuestID"].Get<int>();
//        int world3 = sf.GetComponent("World3", campaignMeta)!.Properties!["QuestID"].Get<int>();
//        int labyrinth = sf.GetComponent("Labyrinth", campaignMeta)!.Properties!["QuestID"].Get<int>();
//        int rootEarth = sf.GetComponent("RootEarth", campaignMeta)!.Properties!["QuestID"].Get<int>();

//        PropertyBag campaignInventory = sf.GetComponent("RemnantPlayerInventory", campaignMeta)!.Properties!;

//        DoInventory(campaignInventory);

//        List<Actor> zoneActors = sf.GetActors("ZoneActor", campaignObject)!;
//        List<Actor> events = sf.FindActors("^((?!ZoneActor).)*$", campaignObject)!
//            .Where(x => x.FirstObjectProperties!.Contains("ID")).ToList();

//        //ZoneLinksToCsv(zoneActors);

//        DoZone(zoneActors, world1, labyrinth, events);
//        DoZone(zoneActors, labyrinth, labyrinth, events);
//        DoZone(zoneActors, world2, labyrinth, events);
//        DoZone(zoneActors, world3, labyrinth, events);
//        DoZone(zoneActors, rootEarth, labyrinth, events);

//        if (slot1 != null)
//        {
//            Console.WriteLine("Adventure");
//            UObject adventureMeta = sf.FindActors("Quest_AdventureMode", main)!.Single().Archive.Objects[0];
//            int? adventureId = adventureMeta.Properties!["ID"].Get<int>();
//            UObject? adventureObject = sf.GetObject($"/Game/Quest_{adventureId}_Container.Quest_Container:PersistentLevel");
//            int quest = sf.GetComponent("Quest", adventureMeta)!.Properties!["QuestID"].Get<int>();
//            PropertyBag adventureInventory = sf.GetComponent("RemnantPlayerInventory", adventureMeta)!.Properties!;
//            DoInventory(adventureInventory);

//            List<Actor> zoneActorsAdventure = sf.GetActors("ZoneActor", adventureObject)!;
//            List<Actor> eventsAdventure = sf.FindActors("^((?!ZoneActor).)*$", adventureObject)!
//                .Where(x => x.FirstObjectProperties!.Contains("ID")).ToList();
//            DoZone(zoneActorsAdventure, quest, 0, eventsAdventure);
//        }
//    }

//    private static void DoInventory(PropertyBag inventoryBag)
//    {
//        ArrayStructProperty? inventory = null;
//        if (inventoryBag.Contains("Items"))
//        {
//            inventory = inventoryBag["Items"].Get<ArrayStructProperty>();
//        }


//        if (inventory != null)
//        {
//            Console.WriteLine("  You have following inventory:");

//            foreach (object? o in inventory.Items)
//            {
//                PropertyBag itemProperties = (PropertyBag)o!;

//                Property item = itemProperties.Properties.Single(x => x.Key == "ItemBP").Value;
//                Property hidden = itemProperties.Properties.Single(x => x.Key == "Hidden").Value;

//                if ((byte)hidden.Value! != 0)
//                {
//                    continue;
//                }

//                Console.WriteLine($"    {Utils.GetShortenedAssetPath(((ObjectProperty)item.Value!).ClassName!)}");
//            }
//        }
//        else
//        {
//            Console.WriteLine("  You do not have items in your inventory");
//        }
//    }

//    private static void DoZone(List<Actor> zoneActors, int world, int labyrinth, List<Actor> events)
//    {
//        Console.WriteLine("Zone Starts");
//        List<Actor> actors = zoneActors.Where(x => x.ZoneActorProperties!["QuestID"].Get<int>() == world && !x.ZoneActorProperties!.Contains("ParentZoneID")).ToList();
//        Actor start = actors.Count > 1 ?
//            actors.Single(x => x.ZoneActorProperties!["NameID"].ToString()!.Contains("one1")) : actors[0];
//        //Actor start = zoneActors.Single(x => x.ZoneActorProperties!["QuestID"].Get<int>() == world && !x.ZoneActorProperties!.Contains("ParentZoneID"));
//        string category = "";
//        Queue<Actor> queue = new();
//        queue.Enqueue(start);
//        List<string> seen = new();
//        while (queue.Count > 0)
//        {
//            Actor current = queue.Dequeue();
//            PropertyBag pb = current.ZoneActorProperties!;
//            string label = pb["Label"].ToString()!;
//            int zoneId = pb["ID"].Get<int>();
//            int questId = pb["QuestID"].Get<int>();

//            if (seen.Contains(label)) continue;
//            seen.Add(label);

//            ArrayStructProperty links = pb["ZoneLinks"].Get<ArrayStructProperty>();
//            Console.WriteLine(label);

//            List<string> waypoints = new();
//            List<string> connectsTo = new();

//            foreach (object? o in links.Items)
//            {
//                PropertyBag link = (PropertyBag)o!;

//                if (string.IsNullOrEmpty(category))
//                {
//                    string? linkCategory = link["Category"].ToString();
//                    if (!string.IsNullOrEmpty(linkCategory) && linkCategory != "None")
//                    {
//                        category = linkCategory;
//                    }
//                }

//                string type = link["Type"].ToString()!;
//                string linkLabel = link["Label"].ToString()!;
//                string name = link["NameID"].ToString()!;
//                string? destinationZoneName = link["DestinationZone"].ToString();

//                switch (type)
//                {
//                    case "EZoneLinkType::Waypoint":
//                        waypoints.Add(linkLabel);
//                        break;
//                    case "EZoneLinkType::Checkpoint":
//                        break;
//                    case "EZoneLinkType::Link":
//                        if (destinationZoneName != "None" && !name.Contains("CardDoor") && destinationZoneName != "2_Zone")
//                        {
//                            Actor destinationZone = zoneActors.Single(x =>
//                                x.ZoneActorProperties!["NameID"].ToString() == destinationZoneName);
//                            string destinationZoneLabel = destinationZone.ZoneActorProperties!["Label"].ToString()!;

//                            if (linkLabel == "Malefic Palace" && destinationZoneLabel == "Beatific Palace"
//                                || destinationZoneLabel == "Malefic Palace" && linkLabel == "Beatific Palace") continue;

//                            bool isLabyrinth = destinationZone.ZoneActorProperties!["QuestID"].Get<int>() == labyrinth && world != labyrinth
//                                || destinationZone.ZoneActorProperties!["QuestID"].Get<int>() != labyrinth && world == labyrinth;
//                            if (!isLabyrinth)
//                            {
//                                connectsTo.Add(destinationZoneLabel);
//                                if (!seen.Contains(destinationZoneLabel))
//                                {
//                                    queue.Enqueue(destinationZone);
//                                }
//                            }
//                        }

//                        break;
//                    default:
//                        throw new InvalidDataException($"unexpected link type '{type}'");
//                }
//            }

//            if (waypoints.Count > 0)
//            {
//                Console.WriteLine($"  World stones: {string.Join(", ", waypoints)}");
//            }


//            string cat = category;
//            IEnumerable<string> GetConnectsTo(List<string> connections)
//            {
//                foreach (IGrouping<string, string> c in connections.GroupBy(x => x))
//                {
//                    string x = "";
//                    if (c.Count() > 1 && cat == "Jungle")
//                    {
//                        x = $" x{c.Count()}";
//                    }
//                    yield return $"{c.Key}{x}";
//                }
//            }

//            Console.WriteLine($"  Connects to: {string.Join(", ", GetConnectsTo(connectsTo))}");

//            foreach (Actor e in new List<Actor>(events).Where(x => x.FirstObjectProperties!["ID"].Get<int>() == questId))
//            {
//                events.Remove(e);
//                Console.WriteLine($"  {e}");
//            }

//            foreach (Actor e in new List<Actor>(events)
//                         .Where(x => x.FirstObjectProperties!.Contains("ZoneID"))
//                         .Where(x => x.FirstObjectProperties!["ZoneID"].Get<int>() == zoneId)
//                         .Where(x => !zoneActors.Select(y => y.ZoneActorProperties!["QuestID"].Get<int>())
//                             .Contains(x.FirstObjectProperties!["ID"].Get<int>())))
//            {
//                events.Remove(e);
//                Console.WriteLine($"  {e}");
//            }
//        }
//    }

//    private static void PrintMode(Property slot, string mode, string world = "")
//    {
//        string[] difficulties =
//        {
//            "None",
//            "Survivor",
//            "Veteran",
//            "Nightmare",
//            "Apocalypse"
//        };
        
//        TimeSpan tp2 = slot.Bag["PlayTime"].Get<TimeSpan>();
//        string timePlayedString2 = $"{(int)tp2.TotalHours}:{tp2.Minutes:D2}:{tp2.Seconds:D2}";
//        int difficulty = slot.Bag.Contains("Difficulty") ? slot.Bag["Difficulty"].Get<int>() : 1;
//        Console.WriteLine($"Mode: {mode}{world}, difficulty: {difficulties[difficulty]}, time played: {timePlayedString2}");
//    }
    
//    // ReSharper disable once UnusedMember.Local
//    private static void ZoneLinksToCsv(List<Actor> zoneActors)
//    {
//        foreach (Actor actor in zoneActors)
//        {
//            PropertyBag pb = actor.ZoneActorProperties!;
//            int id = pb["ID"].Get<int>();
//            int questId = pb["QuestID"].Get<int>();
//            string? nameId = pb["NameID"].ToString();
//            string? label = pb["Label"].ToString();
//            ArrayStructProperty links = pb["ZoneLinks"].Get<ArrayStructProperty>();
//            foreach (object? o in links.Items)
//            {
//                PropertyBag link = (PropertyBag)o!;
//                string? linkLabel = link["Label"].ToString();
//                int zoneId = link["ZoneID"].Get<int>();
//                string? lnameId = link["NameID"].ToString();
//                string? worldMapId = link["WorldMapID"].ToString();
//                string? category = link["Category"].ToString();
//                string? type = link["Type"].ToString();
//                byte isActive = link["IsActive"].Get<byte>();
//                byte isMainPath = link["IsMainPath"].Get<byte>();
//                byte isDisabled = link["IsDisabled"].Get<byte>();
//                byte canBeRespawnLink = link["CanBeRespawnLink"].Get<byte>();
//                byte used = link["Used"].Get<byte>();
//                string? destinationZone = link["DestinationZone"].ToString();
//                string? destinationLink = link["DestinationLink"].ToString();
//                Console.WriteLine(
//                    $"{label},{id},{questId},{nameId},{linkLabel},{zoneId},{lnameId},{worldMapId},{category},{type},{isActive},{isMainPath},{isDisabled},{canBeRespawnLink},{used},{destinationZone},{destinationLink}");
//            }
//        }
//    }
//}

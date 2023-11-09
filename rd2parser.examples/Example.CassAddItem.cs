//using rd2parser.Model;
//using rd2parser.Model.Properties;

//namespace rd2parser.examples;

//internal partial class Example
//{
//    public static void CassAddItem()
//    {
//        Console.WriteLine("Cass add item===========");

//        string folder = Utils.GetSteamSavePath();
//        string savePath = Path.Combine(folder, Environment.GetEnvironmentVariable("DEBUG_REMNANT_SAVE") ?? "save_0.sav");

//        SaveFile sf = SaveFile.Read(savePath);

//        string item = @"/Game/World_Base/Items/Trinkets/Amulets/LetosAmulet/Amulet_LetosAmulet.Amulet_LetosAmulet_C";
//        const string targetFileName = "cass_changed.sav";

//        Actor cass = sf.GetActor("Character_NPC_Cass_C")!;

//        if (cass.Archive.Objects.Any(x => x.ObjectPath == item))
//        {
//            Console.WriteLine("Your Cass shop already has the item");
//            return;
//        }

//        UObject itemObject = new()
//        {
//            WasLoadedByte = 1,
//            ObjectPath = item
//        };

//        UObject equipmentData = new()
//        {
//            ObjectPath = "/Script/GunfireRuntime.EquipmentInstanceData",
//            WasLoadedByte = 0,
//            ExtraPropertiesData = new byte[4],
//            LoadedData = new()
//            {
//                OuterId = 0,
//                Name = FName.Create("EquipmentInstanceData", cass.Archive.NamesTable)
//            },
//            Properties = new()
//            {
//                Properties = new()
//                {
//                    new("Quantity", new()
//                    {
//                        Name = FName.Create("Quantity", cass.Archive.NamesTable),
//                        Type = FName.Create("IntProperty", cass.Archive.NamesTable),
//                        Value = -1,
//                        Size = 4,
//                        Index = 0,
//                        NoRaw = 0
//                    })
//                }
//            }
//        };

//        itemObject.Parent = cass.Archive;
//        cass.Archive.Objects.Add(itemObject);
//        equipmentData.Parent = cass.Archive;
//        cass.Archive.Objects.Add(equipmentData);

//        PropertyBag newItemBag = new()
//        {
//            Properties = new()
//            {
//                new("ID", new()
//                {
//                    Name = FName.Create("ID", cass.Archive.NamesTable),
//                    Type = FName.Create("IntProperty", cass.Archive.NamesTable),
//                    Value = 0,
//                    Size = 4,
//                    Index = 0,
//                    NoRaw = 0
//                }),
//                new("ItemBP", new()
//                {
//                    Name = FName.Create("ItemBP", cass.Archive.NamesTable),
//                    Type = FName.Create("ObjectProperty", cass.Archive.NamesTable),
//                    Value = new ObjectProperty
//                    {
//                      ObjectIndex = -1
//                    },
//                    Size = 4,
//                    Index = 0,
//                    NoRaw = 0
//                }),
//                new("New", new()
//                {
//                    Name = FName.Create("New", cass.Archive.NamesTable),
//                    Type = FName.Create("BoolProperty", cass.Archive.NamesTable),
//                    Value = 1,
//                    Size = 0,
//                    Index = 0,
//                    NoRaw = 0
//                }),
//                new("Favorited", new()
//                {
//                    Name = FName.Create("Favorited", cass.Archive.NamesTable),
//                    Type = FName.Create("BoolProperty", cass.Archive.NamesTable),
//                    Value = 0,
//                    Size = 0,
//                    Index = 0,
//                    NoRaw = 0
//                }),
//                new("Hidden", new()
//                {
//                    Name = FName.Create("Hidden", cass.Archive.NamesTable),
//                    Type = FName.Create("BoolProperty", cass.Archive.NamesTable),
//                    Value = 0,
//                    Size = 0,
//                    Index = 0,
//                    NoRaw = 0
//                }),
//                new("EquipmentSlotIndex", new()
//                {
//                    Name = FName.Create("EquipmentSlotIndex", cass.Archive.NamesTable),
//                    Type = FName.Create("IntProperty", cass.Archive.NamesTable),
//                    Value = -1,
//                    Size = 4,
//                    Index = 0,
//                    NoRaw = 0
//                }),
//                new("InstanceData", new()
//                {
//                    Name = FName.Create("InstanceData", cass.Archive.NamesTable),
//                    Type = FName.Create("ObjectProperty", cass.Archive.NamesTable),
//                    Value = new ObjectProperty
//                    {
//                        ObjectIndex = -1
//                    },
//                    Size = 4,
//                    Index = 0,
//                    NoRaw = 0
//                }),
//            }
//        };

//        (newItemBag["ItemBP"].Value as ObjectProperty)!.SetObject(itemObject);
//        (newItemBag["InstanceData"].Value as ObjectProperty)!.SetObject(equipmentData);

//        PropertyBag inventory = sf.FindComponents("Inventory", cass)![0].Properties!;
//        ArrayStructProperty asp = (ArrayStructProperty)inventory["Items"].Value!;
//        //int maxId = asp.Items.Select(x => (int)((PropertyBag)x!)["ID"].Value!).Max();
//        //newItemBag["ID"].Value = maxId + 1;
//        //inventory["IDGen"].Value = maxId + 1;
//        asp.Items.Add(newItemBag);

//        Console.WriteLine($"Writing to {targetFileName}...");
//        SaveFile.Write(targetFileName, sf);
//        Console.WriteLine($"You have to copy {targetFileName} over your {Path.GetFileName(savePath)} in the game save folder!");
//    }
//}
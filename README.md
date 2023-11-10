# Remnant 2 save files parser and editor library

All credit for the file format goes to <https://github.com/t1nky/remnant-item-finder> which I used as the base for this library. Big thanks to [crackedmind](https://github.com/crackedmind) for all the discussions, information and inspiration.

## What it does

This is a library and a set of examples on reading, editing and writing Remnant 2 save files. It was developed with Microsoft .net 7.0 framework and Microsoft Visual Studio Community 2022. This is not a save editor application, but can be used to build one.

- Read both `profile.sav` and `save_x.sav` files
- List characters inventory, equipment, progression, achievements, loadouts and anything else that can be found int them
- Determine which zones generated in rolled words and what items they may spawn
- Persist the saves in `json` text format, edit them and convert them back into the save files
- Change save data via object model and persist the changes by writing the save files back

## On file format

The file format used by the game is based on Unreal Engine and is quite complicated. I will not endeavor to explain it here, and refer the reader to the source code as the source of truth. See the `Save game object types` section for the list of the all types the game engine uses within save files.

The most practical way to familiarize oneself with the save files format, is to ran an example in the debugger, put a breakpoint just after a navigator object is created, and then drill down the navigator variable in the locals / watch window. You can also have a  look at the `json` dump produced by one of the examples, but you might find it difficult to navigate.

## Code overview

- `Compression` folder contains code for decompressing and re-compressing the save file. The top level `SaveFile` data type contains `FileHeader` type used for the compression and `SaveData` type which contains all the data after they were decompressed
- `IO` folder contains `ReaderBase` and `WriterBase` classes that service all file input and output the library needs. It also contains `AddressUsageTracker` folder with classes that help track if there are any unread gaps in the save file being read. This is to ensure that we understand the format correctly and not leaving any data unread.
- `Model` folder contains all the type in  Remnant 2 save object model
- `Navigation` folder has a set of helper classes to facilitate navigation in the save file object graph
- `Reader` and `Writer` classes add functionality to read and write `FString`s that is used by many types
- `SerializationContext` hold global information that is passed around the object graph during reading and writing
- `Options` and `Log` are utility classes

## Basic usage

To read a save use:

```c#
 SaveFile sf = SaveFile.Read(path);
```

Or:

```c#
SaveFile sf = SaveFile.Read(path, new Options{ParseFowVisitedCoordinates = true});
```

The only option currently is `ParseFowVisitedCoordinates`. If it is not provided, `FowVisitedCoordinates` are not parsed and surfaced as a byte array. This is usually alright, because we do not need them for most common purposes, and they consist of a lot of small objects. If you want them parsed, pass `true`.

Once you have a `SaveFile` object you can drill down to whatever part of the save you need. There are examples of doing this without using the navigation API in `ReadProfile` and `EditScrapRaw` samples.

The `Navigator` class provides functionality to search for objects in the object graph by name to skip tedious drilldown from the top. To create a navigator do this:

```c#
Navigator navigator = new Navigator(sf)
```

Then you can use the navigator object to search for interesting data:

```c#
Property characters = navigator.GetProperty("Characters")!;
```

See below for more details on navigation.

## Save editing

Save editing is more an art than a science, it is very easy to produce an invalid save structure, that the game will not be able to read. `AddRing`, `CassAddItem`, `EditScrap` and `EditScrapRaw` are all examples of save editing. The library does very little handholding here as it is not possible to determine which edit constitutes a valid save and which is not.

Sometimes it may be easier to clone an existing object, than create a new one from scratch. `AddRing` is an example of that. Please be mindful, that unless you do deep cloning yourself you most likely will end up with shallow clones. This means that when you edit data in your clones you have a potential to also edit data on objects contained in the original, and this most of the time is not what you want. Make sure to create a copy on every level of nesting, where you intend to change the data. Note, that this approach can lead to an object graph where some (unchanged) objects are reused in several places. That could be fine for generating a save, but it will be reasonable to re-read the save after the write operation to get a clean object graph again.

Alternatively to cloning you can create an object structure that you need for your edit from scratch, `CassAddItem` is an example of that. 

Either you use cloning or creating a new objects from scratch, the resulting object will not be in your `Navigator`. If you intend to use `Navigator` on the new objects it is recommended to either recreate the `Navigator` (in case of creating objects from scratch), or save and reload the save file (in case of cloning, since the `Navigator` assumes object uniqueness that may be violated after shallow cloning).

This shows how to write the save file back:

```c#
SaveFile.Write(targetFileName, sf);
```

You will notice, that when you read a save and then save it without changes you will get a different file with different length. This is because the compression standard gives quite a bit of leeway to implementations, and .net and Unreal Engine implement them differently. Both are following the standard though, so the results are compatible. 

## JSON serialization

There is an example called `json` that shows the possibility of serializing and deserializing the save file to / from `json`. The was not much work put into this, it was just proven that it is possible and works. When you deserialize a save file from json it is recommended to write it straight away, and re-read it if further work on the file is required, because some links are not serialized due to cyclic nature. Everything that is required to produce a save file is serialized. Same as in previous section it cannot be guaranteed that your `json` edit will produce a valid save file, it's up to you to ensure, that the structure is correct from the game perspective.

## `ModelBase` derived types

These are the core types in the save file (see the list below). They all follow the same pattern:

- Parameter-less constructor for JSON deserialization
- Constructor taking `Reader` object and sometimes `SerializationContext` object reads the object and all its children from the file. Reading of the children is usually delegated to the respective children objects.
- `Write` method taking `Writer` object and sometimes `SerializationContext` objects - same as above but for writing
- `GetChildren` method, returns all the children of the object for object graph traversal
- `ToString` method - some types have this to provide a useful representation of the object in the debugger window

## Navigation

### Navigator

The navigator objects wraps every `ModelBase` derived object in the object graph in a `Node` object. It also keeps a registry on object names for certain types:

- Actor
- Component
- Property
- UObject
- Variable

For each of them, the following methods are available (this is an example for Property type):

```c#
public List<Property> GetProperties(string name, ModelBase? parent = null)
public List<Property> FindProperties(string namePattern, ModelBase? parent = null)
public List<Property> GetAllProperties()
public Property? GetProperty(string name, ModelBase? parent = null)
```

Additionally, for Properties and Variables the following method is available (again, this is an example for Property type):

```c#
public List<T> GetPropertiesValues<T>(string name, ModelBase? parent = null)
```

- `GetProperties` - Get items with the specified name, optionally belonging to the specified parent with any level of nesting
- `FindProperties` - Same as above but uses a regex match on the name
- `GetAllProperties` - Returns all items of the type (any name)
- `GetProperty` - get a single property, throws when more than one match
- `GetPropertiesValues` - for each item returned, get its value and then convert it to given type. Only call this if you are confident that all the values are of the same type

Additionally `Navigator` has these methods:

```
public Node Lookup(ModelBase o)
public Dictionary<string, List<string>> GetSearchableNames()
```

- `Lookup` - get navigation node from node graph
- `GetSearchableNames` - returns all the types and names that can be searched for by the methods further above

If you have a `ModelBase` derived object you can get its parent by using `Navigator` and the following extension method:

```c#
public static T GetParent<T>(this ModelBase obj, Navigator navigator) where T : ModelBase
```

You need to know the type of the parent, which you usually do.

### Navigation Node

Each navigation node has following properties:

- Object - the `ModelBase` derived object it wraps
- Children - the child nodes of this node
- Parent - the parent node of this node
- Path - this is an array of `Segment`s (see below). The first segment is that of the topmost `SaveData` object and the last one is of the current `Object` itself
- DisplayPath - string representation of the `Path` above

```c#
public class Segment
{
    public required string Type;
    public required string Name;
    public int? Index;
}
```

Each path segment always have Type which is the underlying `ModelBase` derived object type. Those types that have object names have a non-empty `Name` property within a segment. Those types that have object indices have a non-null`Index` property within a segment. Here is an example of `DisplayPath` for scrap quantity in a save file:

```text
SaveData.[1]UObject(SavedCharacter).PropertyBag.[8]Property(CharacterData).StructProperty.SaveData.[29]UObject(ItemInstanceData).PropertyBag.[0]Property(Quantity)
```

Above, text in round brackets represent names, numbers in square brackets represent indices and text not in brackets represent type. The path in the example contains 9 segments, i.e the object is nested 9 levels deep. Note, that you do not see `scrap` word in there. This is because id for scrap in the game is quite long: `/Game/World_Base/Items/Materials/Scraps/Material_Scraps.Material_Scraps_C` and thus it is impractical to include similar values in the `DisplayPath`

There are a few other navigational properties on the Node object specific to particular `ModelBase` derived object, e.g `UObject`, `PersistenceContainer` and `Actor` these are mostly designed to surface useful information in the debugger locals / watch window.

## Save game object types

The following table gives an overview of types used in the Remnant 2 save object model:

| Name                 | Type                                  | Has Name/Value | Navigator | Index |
| -------------------- | ------------------------------------- | -------------- | --------- | ----- |
| Actor                | ModelBase                             | Name           | Yes       |       |
| Component            | ModelBase                             | Name           | Yes       |       |
| PresistenceContainer | ModelBase                             | None           |           | Yes   |
| SaveData             | ModelBase                             | None           |           |       |
| SaveFile             | Top Level                             |                |           |       |
| UObject              | ModelBase                             | Name, Value    | Yes       |       |
| Variable             | ModelBase                             | Name           | Yes       |       |
| Variables            | ModelBase                             | Name           |           |       |
| ArrayProprty         | ModelBase                             | None           |           | Yes   |
| ArrayStructProperty  | ModelBase                             | None           |           | Yes   |
| ByteProperty         | ModelBase                             | Name, Value    |           |       |
| EnumProperty         | ModelBase                             | Name, Value    |           |       |
| MapProperty          | ModelBase                             | None           |           | Yes   |
| ObjectProperty       | ModelBase                             | Name           |           |       |
| Property             | ModelBase                             | Name, Value    | Yes       |       |
| PropertyBag          | ModelBase                             | None           |           | Yes   |
| StructProperty       | ModelBase                             | None           |           |       |
| TextProperty         | ModelBase                             | None           |           |       |
| TextPropertyData0    | ModelBase                             | Name           |           |       |
| TextPropertyData255  | ModelBase                             | None           |           |       |
| PropertyValue        | Part of Property                      |                |           |       |
| ActorDynamicData     | Part of Actor                         |                |           |       |
| FName                | Part of many types                    |                |           |       |
| FTopLevelAssetPath   | Part of SaveData and ActorDynamicData |                |           |       |
| FGuid                | Memory                                |                |           |       |
| FileHeader           | Memory                                |                |           |       |
| FInfo                | Memory                                |                |           |       |
| FQuaternion          | Memory                                |                |           |       |
| FTransform           | Memory                                |                |           |       |
| FVector              | Memory                                |                |           |       |
| OffsetInfo           | Memory                                |                |           |       |
| PackageVersion       | Memory                                |                |           |       |
| UObjectLoadedData    | Memory                                |                |           |       |

The type in this table is as follows:

- Top Level - `SaveFile` is the top lever type used to load the save into its object graph
- ModelBase - this type is inherited from `ModelBase` and is a part of the save object graph
- Part - this type reads and writes from/to input/output streams same as `ModelBase` types, but it is not inherited from `ModelBase` and is only used as a (small) part of a `ModelBase` type
- Memory - this type has memory layout matching to the (uncompressed) data in the save files. As a result it does not have a dedicated read/write routines as it can be loaded and saved generically

Objects of some type have name, and object of some types have a useful name / value pair, navigation system uses the names for those types that have them for their objects.

Most useful to search objects that have a name are supported by `Navigator` for easy search.

Objects of some types has an index, which indicate their position in a parent object.

## Error handling

I'm a proponent of a minimal error handling upfront, with adding it as necessary, when a clear use cases for it emerge. The disadvantage of that approach that for projects that are not likely to have a large audience (and/or used in production), they will never emerge, so a new person using it for their own needs is likely to stumble on an exception thrown that they would expect to be processed by the library. If you come across such a situation, please report.

## Items database

There is `db.json` file included in this repository which is not used by the library. This is a Remnant 2 item database which can be used for other applications working with Remnant 2 save files.

## Examples

This library comes with the following code examples:

- Add Ring - edit save file to ad an arbitrary ring to your character
- Blood Moon - display little information that the save file has about blood moon generation
- Campaign - display various data from `save_x.sav` file:
  - Time Played with this character
  - Quest Completed Log
  - Which mode you are playing currently, Campaign or Adventure
  - Difficulty and time played for Campaign
  - Difficulty and time played for Adventure
  - Quest inventory
  - List of Zones, World stones, Connections and events generated for Campaign and Adventure
- Cass - print your Cass's inventory
- Cass Add Item - add an item to your Cass's inventory
- Challenges - Achievements and Challenges progress for each of your characters
- Edit Scrap - edit scrap quantity in your save using navigation system
- Edit Scrap Raw - edit scrap quantity in your save using raw API
- JSON - serialize safe file to and deserialize from JSON
- Loadouts - display characters loadouts
- Read Profile - read profile data:
  - Active character
  - Character's Power level
  - Character's Trait rank
  - Character's total and unallocated trait points
  - Character's gender
  - Character's archetypes
  - Inventory
  - Equipped items, skills, consumables
  - Traits and trait levels

## Dependencies

- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON serialization / deserialization support
- [Serilog](https://serilog.net/) - Debug logging
- [System.IO.Hashing](https://www.nuget.org/packages/System.IO.Hashing/) - CRC-32 which is used in Remnant 2 saves


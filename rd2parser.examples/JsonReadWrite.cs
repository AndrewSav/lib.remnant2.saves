using Newtonsoft.Json;
using rd2parser.Model;

namespace rd2parser.examples;
public class JsonReadWrite
{
    public static void ToJson(string path, SaveData o)
    {
        JsonSerializer serializer = new()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.All,
        };

        using StreamWriter sw = new(path);
        using JsonWriter writer = new JsonTextWriter(sw);
        serializer.Serialize(writer, o);
    }
    public static SaveData FromJson(string path)
    {
        JsonSerializer serializer = new()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        using StreamReader sr = new(path);
        using JsonReader reader = new JsonTextReader(sr);
        return serializer.Deserialize<SaveData>(reader)!;
    }
}

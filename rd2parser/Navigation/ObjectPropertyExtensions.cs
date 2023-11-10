using rd2parser.Model;
using rd2parser.Model.Properties;
namespace rd2parser.Navigation;
public static class ObjectPropertyExtensions
{
    //public static UObject GetObject(this ObjectProperty objectProperty, Navigator navigator)
    //{
    //    Node current = navigator.Lookup(objectProperty);
    //    while (current.Path[^1].Type != "SaveData" && current.Parent != null)
    //    {
    //        current = current.Parent;
    //    }

    //    SaveData sd = (SaveData)current.Object;
    //    return sd.Objects[objectProperty.ObjectIndex];
    //}

    //public static string? GetClassName(this ObjectProperty objectProperty, Navigator navigator)
    //{
    //    return objectProperty.GetObject(navigator).ObjectPath;
    //}
}


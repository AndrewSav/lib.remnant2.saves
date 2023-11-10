using rd2parser.Model;
namespace rd2parser.Navigation;
public static class ModelBaseExtension
{
    public static T GetParent<T>(this ModelBase obj, Navigator navigator) where T : ModelBase
    {
        return (T)navigator.Lookup(obj).Parent!.Object;
    }
}

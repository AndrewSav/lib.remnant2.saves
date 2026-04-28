namespace examples;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "tests")
        {
            Tests.Run();
            return;
        }

        Tests.Run();
        Example.Json();
        Example.ReadProfile();
        Example.Loadouts();
        Example.Cass();
        Example.Campaign();
        Example.BloodMoon();
        Example.Challenges();
        Example.EditScrapRaw();
        Example.EditScrap();
        Example.AddRing();
        Example.CassAddItem();
        Example.MaxShards();
        Example.Alloys();
        Example.ResetOneShots();
        Example.Decompress();
    }
}

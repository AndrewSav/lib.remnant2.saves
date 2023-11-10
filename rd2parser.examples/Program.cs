namespace rd2parser.examples;

internal class Program
{
    // TODO: Choose a better name
    // TODO: Nuget

    private static void Main()
    {
        Tests.Run();
        Example.Json();
        Example.ReadProfile();
        Example.Cass();
        Example.Campaign();
        Example.BloodMoon();
        Example.Challenges();
        Example.EditScrapRaw();
        Example.EditScrap();
        Example.AddRing();
        Example.CassAddItem();
    }
}

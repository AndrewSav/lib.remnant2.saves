namespace rd2parser.examples;

internal class Program
{

    // TODO: Replace ApplicationException in library
    // TODO: Nuget
    // TODO: README - do not expect read json to work as is
    // TODO: README - similarly, editing breaks navigation
    // TODO: README - how we (do not) handle errors
    // TODO: README - why navigation adapters may be imprecise
    // TODO: README - ParseFowVisitedCoordinates
    // TODO: README - Note on absence of deep copy
    // TODO: README - Names va ToString()

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

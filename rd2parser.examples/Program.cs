namespace rd2parser.examples;

internal class Program
{

    // TODO: Replace ApplicationException in library
    // TODO: Nuget
    // TODO: README - do not expect read json to work as is
    // TODO: README - how we (do not) handle errors
    // TODO: README - why navigation adapters may be imprecise
    // TODO: README - FIX UPS: UObject Children/FlattenChildren
    // TODO: README - FIX UPS: PersistenceContainer Children/FlattenChildren
    // TODO: README - FIX UPS: Constructors
    // TODO: README - FIX UPS: UObject: ObjectIndex and Parent (write)
    // TODO: README - FIX UPS: Property: Bag
    // TODO: README - FIX UPS: ObjectProperty: UObject (write)
    // TODO: README - FIX UPS: Navigation
    // TODO: README - FIX UPS: Copying object will screw up navigation
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
        //Example.CassAddItem();
    }
}

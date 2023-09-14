using Serilog;

namespace rd2parser.examples;

internal class Program
{

    // TODO: selected adv/campaign,
    // TODO: adv/campaign locations/events examples
    // TODO: blood moon?
    // TODO: show Cass Shop example
    // TODO: edit Cass Shop example
    // TODO: Replace ApplicationException in library
    // TODO: Nuget
    // TODO: check lists copying
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
    // TODO: move UObject read/write code from SaveData to UObject & Components


    private static void Main()
    {

        Tests.Run();

        Example.AddRing();
        Example.EditScrap();
        Example.EditScrapRaw();
        Example.ReadProfile();
        Example.Json();
    }
}

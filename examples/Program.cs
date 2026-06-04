namespace examples;

internal class Program
{
    private static void Main()
    {
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
        RunWithPropertySizeAdjustmentLogging(Example.AddRing);
        RunWithPropertySizeAdjustmentLogging(Example.CassAddItem);
        RunWithPropertySizeAdjustmentLogging(Example.MaxShards);
        RunWithPropertySizeAdjustmentLogging(Example.Alloys);
        Example.ResetOneShots();
        Example.Decompress();
        Example.PrismRemoveSlot();
        Example.PrismRemoveRollChance();
        Example.PrismAddRollChance();
        Example.PrismSetExperience();
        Example.PrismSetSeed();
        Example.PrismSetSegments();

    }

    private static void RunWithPropertySizeAdjustmentLogging(Action example)
    {
        lib.remnant2.saves.Log.Logger = Serilog.ConsoleLoggerConfigurationExtensions
            .Console(new Serilog.LoggerConfiguration().MinimumLevel.Information().WriteTo)
            .CreateLogger();

        try
        {
            example();
        }
        finally
        {
            lib.remnant2.saves.Log.Logger = null!;
        }
    }
}

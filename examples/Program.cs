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
        RunWithPropertySizeAdjustmentLogging(Example.AddRing);
        RunWithPropertySizeAdjustmentLogging(Example.CassAddItem);
        RunWithPropertySizeAdjustmentLogging(Example.MaxShards);
        RunWithPropertySizeAdjustmentLogging(Example.Alloys);
        Example.ResetOneShots();
        Example.Decompress();
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

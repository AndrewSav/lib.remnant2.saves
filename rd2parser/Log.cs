using Serilog;

namespace rd2parser;

public class Log
{
    private static ILogger? _logger;

    public static ILogger Logger
    {
        get => _logger ?? Serilog.Log.Logger;
        set => _logger = value;
    }
}
using Serilog;

namespace rd2parser;

// Rudimentary logging facility for debugging and troubleshooting
// We do not need anything more advanced
public class Log
{
    private static ILogger? _logger;

    public static ILogger Logger
    {
        get => _logger ?? Serilog.Log.Logger;
        set => _logger = value;
    }
}

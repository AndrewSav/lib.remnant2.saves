using System.Runtime.InteropServices;

namespace examples;

public class Utils
{
    private static readonly Guid SavedGamesGuid = new("4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4");

    [DllImport("shell32.dll")]
    private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
        IntPtr hToken,
        out IntPtr pszPath);

    private static string GetSavePath()
    {
        IntPtr path = IntPtr.Zero;

        try
        {
            int hr = SHGetKnownFolderPath(SavedGamesGuid, 0, IntPtr.Zero, out path);
            Marshal.ThrowExceptionForHR(hr);
            return $@"{Marshal.PtrToStringUni(path)}\Remnant2";
        }
        finally
        {
            Marshal.FreeCoTaskMem(path);
        }
    }

    public static string GetSteamSavePath()
    {
        string? envPath = Environment.GetEnvironmentVariable("DEBUG_REMNANT_FOLDER");
        if (envPath != null) return envPath;
        string generalPath = GetSavePath();
        string[] possiblePaths = Directory.GetDirectories($@"{generalPath}\Steam");
        return possiblePaths[0];
    }

    // The save / character SLOT index the examples target, from the DEBUG_REMNANT_SAVE_INDEX env var
    // (default 0). One index selects BOTH the profile character (profile.sav `Characters[index]`) and the
    // world/campaign save (`save_<index>.sav`), so an example hits the same slot either way. Each editing
    // example surfaces it as an editable `saveIndex` in its CHANGE-THESE block; the env var is just the default.
    // NOTE: this is the save SLOT, not an ordinal. Deleting characters leaves GAPS - if you had slots 0..4 and
    // deleted 1,2,3, your "second" remaining character is now at slot 4. No validation: a bad value just throws.
    public static int GetSaveIndex() =>
        Environment.GetEnvironmentVariable("DEBUG_REMNANT_SAVE_INDEX") is { } s ? int.Parse(s) : 0;

    // The world/campaign save file for a slot: save_<saveIndex>.sav in the save folder.
    public static string GetWorldSavePath(int saveIndex) => Path.Combine(GetSteamSavePath(), $"save_{saveIndex}.sav");

    public static string GetShortenedAssetPath(string path)
    {
        path = path[(path.LastIndexOfAny("/.".ToCharArray()) + 1)..];
        if (path.EndsWith("_C")) path = path[..^2];
        if (path.EndsWith("_UI")) path = path[..^2];
        path = path.Replace("_", " ").Trim();
        if (path.Contains(':')) path = path[(path.LastIndexOf(":", StringComparison.Ordinal) + 1)..];
        return $"'{path}'";
    }
}

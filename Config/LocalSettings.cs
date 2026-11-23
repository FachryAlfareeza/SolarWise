using System.Text.Json;

namespace SolarWise;

// Small per-Windows-user settings kept on this computer (%LOCALAPPDATA%\SolarWise\settings.json).
// Holds the remembered email/username (never the password) and window preferences.
public static class LocalSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SolarWise", "settings.json");

    private sealed class Data
    {
        public string RememberedLogin { get; set; }
        public bool StartMaximized { get; set; }
    }

    // Open the main window maximized after login.
    public static bool StartMaximized
    {
        get => Load().StartMaximized;
        set
        {
            var data = Load();
            data.StartMaximized = value;
            Save(data);
        }
    }

    // The email/username saved with "Remember me", or null when nothing is remembered.
    public static string RememberedLogin
    {
        get => Load().RememberedLogin;
        set
        {
            var data = Load();
            data.RememberedLogin = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            Save(data);
        }
    }

    private static Data Load()
    {
        try
        {
            return File.Exists(FilePath) ? JsonSerializer.Deserialize<Data>(File.ReadAllText(FilePath)) ?? new Data() : new Data();
        }
        catch
        {
            return new Data(); // a damaged settings file just means nothing is remembered
        }
    }

    private static void Save(Data data)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, JsonSerializer.Serialize(data));
        }
        catch
        {
            // Not being able to remember the login must never block signing in.
        }
    }
}

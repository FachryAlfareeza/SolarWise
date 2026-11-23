using System.Text.Json;

namespace SolarWise;

// Small per-Windows-user settings kept on this computer (%LOCALAPPDATA%\SolarWise\settings.json).
// Only the remembered email/username is stored here; the password is never saved.
public static class LocalSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SolarWise", "settings.json");

    private sealed class Data
    {
        public string RememberedLogin { get; set; }
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

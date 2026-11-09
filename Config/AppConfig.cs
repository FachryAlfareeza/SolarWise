using DotNetEnv;

namespace SolarWise;

// Application settings read from the .env file (found by searching upward from the run folder).
// Keys: host, port, database, user, password. The older DB_HOST, DB_PORT, DB_NAME, DB_USER and
// DB_PASSWORD keys still work as a fallback.
public static class AppConfig
{
    static AppConfig()
    {
        Env.TraversePath().Load();
    }

    public static string DbHost => Get("host", "DB_HOST");
    public static string DbPort => Get("port", "DB_PORT", "5432");
    public static string DbName => Get("database", "DB_NAME", "postgres");
    public static string DbUser => Get("user", "DB_USER");
    public static string DbPassword => Get("password", "DB_PASSWORD");

    private static string Get(string key, string fallbackKey, string defaultValue = null) =>
        Env.GetString(key, null) ?? Env.GetString(fallbackKey, defaultValue);
}

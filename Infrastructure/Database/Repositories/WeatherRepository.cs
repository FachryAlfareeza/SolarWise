using SolarWise.Database;

namespace SolarWise;

// Saved weather records, stored in Supabase (table weather_data). Lookups live in WeatherService.
public static class WeatherRepository
{
    private static readonly WeatherDbRepository Db = new();

    // Records of a saved location, oldest first. Empty for a temporary location (ID 0).
    public static List<WeatherData> ForLocation(SiteLocation site)
    {
        if (site == null || site.LocationID <= 0) return new List<WeatherData>();
        var records = Db.ForLocation(site.LocationID);
        foreach (var record in records) record.Location = site;
        return records;
    }

    public static WeatherData Add(WeatherData record) => Db.Insert(record);

    public static List<WeatherData> GetByIds(IEnumerable<int> ids) => Db.GetByIds(ids);
}

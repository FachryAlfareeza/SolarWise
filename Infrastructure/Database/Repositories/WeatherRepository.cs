using SolarWise.Database;

namespace SolarWise;

// Saved weather records, stored in Supabase (table weather_data). Lookups live in WeatherService.
public static class WeatherRepository
{
    private static readonly WeatherDbRepository Db = new();

    // Climatology is stored on the first day of each month in year 0001, reserved as a sentinel.
    // weather_data has a date column but no separate month/source columns.
    public const int ClimatologyDateYear = 1;

    // Records of a saved location, oldest first. Empty for a temporary location (ID 0).
    public static List<WeatherData> ForLocation(SiteLocation site)
    {
        if (site == null || site.LocationID <= 0) return new List<WeatherData>();
        var records = Db.ForLocation(site.LocationID);
        foreach (var record in records) record.Location = site;
        return records;
    }

    public static WeatherData Add(WeatherData record) => Db.Insert(record);

    public static List<WeatherData> MonthlyClimatology(IEnumerable<WeatherData> records) =>
        records.Where(w => w.Date.Year == ClimatologyDateYear)
            .OrderBy(w => w.Date.Month)
            .ToList();

    // Re-fetches update each month in place, avoiding duplicate climatology rows.
    public static void SaveMonthlyClimatology(SiteLocation site, IReadOnlyList<MonthlySolarIrradiance> monthly)
    {
        if (site == null || site.LocationID <= 0) throw new ArgumentException("Climatology needs a saved location.", nameof(site));
        if (monthly == null || monthly.Count != 12) throw new ArgumentException("All 12 monthly values are required.", nameof(monthly));

        var existing = MonthlyClimatology(ForLocation(site)).ToDictionary(w => w.Date.Month);
        foreach (var value in monthly)
        {
            if (value.Month < 1 || value.Month > 12 || value.KwhPerSquareMeterPerDay < 0)
                throw new ArgumentException("Monthly climatology contains an invalid value.", nameof(monthly));

            var record = existing.TryGetValue(value.Month, out var saved)
                ? saved
                : new WeatherData { Location = site, Date = new DateTime(ClimatologyDateYear, value.Month, 1) };
            record.Location = site;
            record.Date = new DateTime(ClimatologyDateYear, value.Month, 1);
            record.SolarIrradiance = value.KwhPerSquareMeterPerDay;
            record.Temperature = 0;
            record.SunshineHours = 0;
            record.IsSample = false;

            if (record.WeatherDataID > 0) Db.Update(record);
            else Db.Insert(record);
        }
    }

    public static List<WeatherData> GetByIds(IEnumerable<int> ids) => Db.GetByIds(ids);
}

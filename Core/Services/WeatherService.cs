namespace SolarWise;

// Weather records: lookups by location and date, and irradiance averages.
public static class WeatherService
{
    public static WeatherData WeatherFor(SiteLocation site, DateTime date) =>
        DataStore.WeatherRecords.FirstOrDefault(w => w.Location.LocationID == site.LocationID && w.Date.Date == date.Date);

    // Latest saved record for a location; null for temporary locations (ID 0).
    public static WeatherData LatestFor(SiteLocation site) =>
        site.LocationID > 0
            ? DataStore.WeatherRecords.LastOrDefault(w => w.Location.LocationID == site.LocationID)
            : null;

    // Average irradiance of saved records; null when the location has none.
    public static double? SavedIrradiance(SiteLocation site)
    {
        var records = DataStore.WeatherRecords
            .Where(w => site.LocationID > 0 && w.Location.LocationID == site.LocationID)
            .ToList();
        return records.Count == 0 ? null : records.Average(w => w.SolarIrradiance);
    }
}

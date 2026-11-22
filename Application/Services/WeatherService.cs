namespace SolarWise;

// Weather records: lookups by location and date, and irradiance averages. Records live in Supabase.
public static class WeatherService
{
    public static WeatherData WeatherFor(SiteLocation site, DateTime date) =>
        WeatherRepository.ForLocation(site).FirstOrDefault(w => w.Date.Date == date.Date);

    // Latest saved record for a location; null for temporary locations (ID 0).
    public static WeatherData LatestFor(SiteLocation site) =>
        WeatherRepository.ForLocation(site).LastOrDefault();

    // Average irradiance of saved records; null when the location has none.
    public static double? SavedIrradiance(SiteLocation site)
    {
        var records = WeatherRepository.ForLocation(site);
        return records.Count == 0 ? null : records.Average(w => w.SolarIrradiance);
    }
}

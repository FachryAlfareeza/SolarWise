using System.Globalization;

namespace SolarWise;

// Coordinates external weather lookups and persistence of NASA POWER monthly climatology.
public static class WeatherService
{
    public static WeatherData WeatherFor(SiteLocation site, DateTime date)
    {
        var records = WeatherRepository.ForLocation(site);
        return records.FirstOrDefault(w => w.Date.Date == date.Date)
            ?? WeatherRepository.MonthlyClimatology(records).FirstOrDefault(w => w.Date.Month == date.Month);
    }

    // Latest saved record for a location; null for temporary locations (ID 0).
    public static WeatherData LatestFor(SiteLocation site) =>
        WeatherRepository.ForLocation(site).LastOrDefault();

    // Fetch a region's coordinates, request its monthly solar climatology, and persist the result.
    public static async Task<SiteLocation> FetchAndSaveClimatologyAsync(
        string place, User owner, CancellationToken cancellationToken = default)
    {
        if (owner == null || owner.UserID <= 0) throw new ArgumentException("A signed-in user is required.", nameof(owner));

        var geocoded = await WeatherProvider.GeocodeIndonesiaAsync(place, cancellationToken);
        var monthly = await WeatherProvider.GetNasaPowerClimatologyAsync(
            geocoded.Latitude, geocoded.Longitude, cancellationToken);

        var site = LocationRepository.LocationsFor(owner).FirstOrDefault(existing =>
            CoordinatesMatch(existing, geocoded.Latitude, geocoded.Longitude));

        if (site == null)
        {
            site = LocationRepository.AddLocation(new SiteLocation
            {
                Address = geocoded.Name,
                Latitude = geocoded.Latitude.ToString("0.######", CultureInfo.InvariantCulture),
                Longitude = geocoded.Longitude.ToString("0.######", CultureInfo.InvariantCulture)
            }, owner);
        }
        else
        {
            // Keep the user's saved location name in sync with the geocoding result.
            site.Address = geocoded.Name;
            LocationRepository.UpdateLocation(site);
        }

        WeatherRepository.SaveMonthlyClimatology(site, monthly);
        return site;
    }

    // Weighted annual daily average from the 12 monthly climatology values.
    // For legacy saved observations, retain the previous mean-of-records behavior.
    public static double? SavedIrradiance(SiteLocation site)
    {
        var records = WeatherRepository.ForLocation(site);
        var climatology = WeatherRepository.MonthlyClimatology(records);
        if (climatology.Count == 12)
        {
            double totalDays = climatology.Sum(w => DateTime.DaysInMonth(WeatherRepository.ClimatologyDateYear, w.Date.Month));
            return climatology.Sum(w => w.SolarIrradiance * DateTime.DaysInMonth(WeatherRepository.ClimatologyDateYear, w.Date.Month)) / totalDays;
        }
        return records.Count == 0 ? null : records.Average(w => w.SolarIrradiance);
    }

    public static List<WeatherData> MonthlyClimatologyFor(SiteLocation site) =>
        WeatherRepository.MonthlyClimatology(WeatherRepository.ForLocation(site));

    private static bool CoordinatesMatch(SiteLocation site, double latitude, double longitude)
    {
        var coordinates = site.GetCoordinates();
        return Math.Abs(coordinates.Lat - latitude) < 0.005 && Math.Abs(coordinates.Lon - longitude) < 0.005;
    }
}

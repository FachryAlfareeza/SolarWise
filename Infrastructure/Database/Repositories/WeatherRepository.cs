namespace SolarWise;

// Saved weather records (lookups live in WeatherService).
public static class WeatherRepository
{
    public static readonly List<WeatherData> WeatherRecords = new();
}

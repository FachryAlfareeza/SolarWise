namespace SolarWise.Core.Models;

public class WeatherData
{
    public Location Location { get; set; } = new();
    public DateTime Date { get; set; }
    public double SolarIrradiance { get; set; }
    public double Temperature { get; set; }
    public double SunshineHours { get; set; }
}
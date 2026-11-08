namespace SolarWise;

public class WeatherData
{
    public int WeatherDataID { get; set; }
    public SiteLocation Location { get; set; }
    public DateTime Date { get; set; }
    public double SolarIrradiance { get; set; } // kWh/m²/day (daily total)
    public double Temperature { get; set; }     // °C
    public double SunshineHours { get; set; }   // h/day
    public bool IsSample { get; set; }
}

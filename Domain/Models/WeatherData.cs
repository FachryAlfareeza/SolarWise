namespace SolarWise;

public class WeatherData
{
    // Objects loaded from the database are new instances each time, so saved ones are equal by ID.
    public override bool Equals(object obj) =>
        obj is WeatherData other && (WeatherDataID > 0 ? other.WeatherDataID == WeatherDataID : ReferenceEquals(this, other));
    public override int GetHashCode() => WeatherDataID > 0 ? WeatherDataID.GetHashCode() : base.GetHashCode();
    public int WeatherDataID { get; set; }
    public SiteLocation Location { get; set; }
    public DateTime Date { get; set; }
    public double SolarIrradiance { get; set; } // kWh/m²/day (daily total)
    public double Temperature { get; set; }     // °C
    public double SunshineHours { get; set; }   // h/day
    public bool IsSample { get; set; }
}

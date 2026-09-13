namespace MySolarApp.Core.Models;

public class SolarPanel
{
    public string Name { get; set; } = string.Empty;
    public double Efficiency { get; set; }
    public double PricePerMeter { get; set; }
    public double Wattage { get; set; }
    public string Description { get; set; } = string.Empty;
}
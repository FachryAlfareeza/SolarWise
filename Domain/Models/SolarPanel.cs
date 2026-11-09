namespace SolarWise;

public class SolarPanel
{
    public int PanelID { get; set; }
    public string Name { get; set; } = "";
    public double Efficiency { get; set; }     // percent
    public double PricePerMeter { get; set; }  // IDR per meter; unit to confirm
    public double Wattage { get; set; }        // Wp
    public string Description { get; set; } = "";
    public override string ToString() => Name;
}

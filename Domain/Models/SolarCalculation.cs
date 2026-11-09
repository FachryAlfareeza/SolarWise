namespace SolarWise;

public class SolarCalculation
{
    public int CalculationID { get; set; }
    public int OwnerUserID { get; set; }
    public int OrganizationID { get; set; }     // 0 = no organization (prototype extension)
    public string ProjectName { get; set; } = "";
    public SiteLocation Location { get; set; }
    public SolarPanel PanelType { get; set; }
    public WeatherData Weather { get; set; }    // null when no weather record exists
    public double InputArea { get; set; }       // m²
    public double Irradiance { get; set; }      // kWh/m²/day used
    public int PanelCount { get; set; }         // calculated
    public double SystemCapacityKwp { get; set; } // calculated
    public double EstimatedOutput { get; set; } // kWh/year
    public double EstimateSaving { get; set; }  // IDR/year
    public DateTime CreatedAt { get; set; }
    public bool IsSample { get; set; }
    public bool IsSaved => CalculationID > 0;

    public override string ToString() => $"{ProjectName} - {CreatedAt:dd MMM yyyy}";
}

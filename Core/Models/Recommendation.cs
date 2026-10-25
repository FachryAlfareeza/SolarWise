namespace SolarWise;

public class Recommendation
{
    public int RecommendationID { get; set; }
    public int CalculationID { get; set; }
    public double OptimalArea { get; set; }     // m²
    public double EstimatedOutput { get; set; } // kWh/year
    public double EstimatedSaving { get; set; } // IDR/year
    public string Summary { get; set; } = "";
}

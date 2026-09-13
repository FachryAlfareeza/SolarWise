namespace SolarWise.Core.Models;

public class Recommendation
{
    public double OptimalArea { get; set; }
    public double EstimatedOutput { get; set; }
    public double EstimatedSaving { get; set; }

    public string GetSummary()
    {
        return $"Optimal Area: {OptimalArea} m², Estimated Output: {EstimatedOutput} kWh, Estimated Saving: Rp{EstimatedSaving:N0}";
    }
}
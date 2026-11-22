namespace SolarWise;

public class Recommendation
{
    // Objects loaded from the database are new instances each time, so saved ones are equal by ID.
    public override bool Equals(object obj) =>
        obj is Recommendation other && (RecommendationID > 0 ? other.RecommendationID == RecommendationID : ReferenceEquals(this, other));
    public override int GetHashCode() => RecommendationID > 0 ? RecommendationID.GetHashCode() : base.GetHashCode();
    public int RecommendationID { get; set; }
    public int CalculationID { get; set; }
    public double OptimalArea { get; set; }     // m²
    public double EstimatedOutput { get; set; } // kWh/year
    public double EstimatedSaving { get; set; } // IDR/year
    public string Summary { get; set; } = "";
}

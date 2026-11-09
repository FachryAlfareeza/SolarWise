namespace SolarWise;

// Recommendations, each linked to one calculation.
public static class RecommendationRepository
{
    public static readonly List<Recommendation> Recommendations = new();

    public static Recommendation RecommendationFor(SolarCalculation calc) =>
        Recommendations.FirstOrDefault(r => r.CalculationID == calc.CalculationID);

    public static List<Recommendation> RecommendationsFor(User user)
    {
        var ids = CalculationRepository.CalculationsFor(user).Select(c => c.CalculationID).ToHashSet();
        return Recommendations.Where(r => ids.Contains(r.CalculationID)).ToList();
    }

    public static void AddRecommendation(Recommendation rec)
    {
        rec.RecommendationID = InMemoryDatabase.NextId();
        Recommendations.Add(rec);
    }
}

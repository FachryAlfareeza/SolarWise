using SolarWise.Database;

namespace SolarWise;

// Recommendations, each linked to one calculation. Stored in Supabase (table recomendation).
public static class RecommendationRepository
{
    private static readonly RecommendationDbRepository Db = new();

    public static Recommendation RecommendationFor(SolarCalculation calc) =>
        calc.IsSaved ? Db.ForCalculation(calc.CalculationID) : null;

    public static List<Recommendation> RecommendationsFor(User user) =>
        Db.ForCalculations(CalculationRepository.CalculationsFor(user).Select(c => c.CalculationID));

    public static void AddRecommendation(Recommendation rec) => Db.Insert(rec);
    public static bool UpdateRecommendation(Recommendation rec) => Db.Update(rec);
    public static bool DeleteRecommendation(Recommendation rec) => Db.Delete(rec);
}

namespace SolarWise;

// Calculations (scoped to the current user and active organization).
public static class CalculationRepository
{
    public static readonly List<SolarCalculation> Calculations = new();

    public static List<SolarCalculation> CalculationsFor(User user) =>
        Calculations
            .Where(c => c.OwnerUserID == user.UserID && c.OrganizationID == SessionContext.ActiveOrganizationId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

    public static void SaveCalculation(SolarCalculation calc)
    {
        if (calc.IsSaved) return;
        calc.CalculationID = InMemoryDatabase.NextId();
        Calculations.Add(calc);
    }

    public static void DeleteCalculation(SolarCalculation calc)
    {
        Calculations.Remove(calc);
        RecommendationRepository.Recommendations.RemoveAll(r => r.CalculationID == calc.CalculationID);
    }

    public static SolarCalculation CalculationFor(Recommendation rec) =>
        Calculations.FirstOrDefault(c => c.CalculationID == rec.CalculationID);
}

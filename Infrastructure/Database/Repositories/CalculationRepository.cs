using SolarWise.Database;

namespace SolarWise;

// Calculations (scoped to the current user and active organization). Stored in Supabase (table calculations).
public static class CalculationRepository
{
    private static readonly CalculationDbRepository Db = new();

    public static List<SolarCalculation> CalculationsFor(User user) =>
        Hydrate(Db.ForUser(user.UserID, SessionContext.ActiveOrganizationId));

    public static void SaveCalculation(SolarCalculation calc)
    {
        if (calc.IsSaved) return;

        // A location typed in the calculator is temporary; the calculation needs it saved first.
        if (calc.Location.LocationID == 0) LocationRepository.AddLocation(calc.Location, calc.OwnerUserID);
        Db.Insert(calc);
    }

    public static bool UpdateCalculation(SolarCalculation calc) => calc.IsSaved && Db.Update(calc);

    // Its recommendation is removed with it (ON DELETE CASCADE).
    public static void DeleteCalculation(SolarCalculation calc) => Db.Delete(calc);

    public static SolarCalculation CalculationFor(Recommendation rec)
    {
        var calc = Db.GetById(rec.CalculationID);
        return calc == null ? null : Hydrate(new List<SolarCalculation> { calc })[0];
    }

    // The table stores IDs; load the real Location, SolarPanel and WeatherData objects in three queries.
    private static List<SolarCalculation> Hydrate(List<SolarCalculation> calcs)
    {
        if (calcs.Count == 0) return calcs;

        var locations = new LocationDbRepository().GetByIds(calcs.Select(c => c.Location.LocationID)).ToDictionary(l => l.LocationID);
        var panels = new SolarPanelDbRepository().GetByIds(calcs.Select(c => c.PanelType.PanelID)).ToDictionary(p => p.PanelID);
        var weather = WeatherRepository.GetByIds(calcs.Where(c => c.Weather != null).Select(c => c.Weather.WeatherDataID)).ToDictionary(w => w.WeatherDataID);

        foreach (var calc in calcs)
        {
            calc.Location = locations[calc.Location.LocationID];
            calc.PanelType = panels[calc.PanelType.PanelID];
            if (calc.Weather != null && weather.TryGetValue(calc.Weather.WeatherDataID, out var w))
            {
                w.Location = calc.Location;
                calc.Weather = w;
            }
            else
            {
                calc.Weather = null;
            }
        }
        return calcs;
    }
}

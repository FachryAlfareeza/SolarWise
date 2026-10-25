namespace SolarWise;

// Calculation engine for the prototype. Assumptions are constants so they are easy to find and change.
// The class keeps its name SolarCalculator so the views do not change.
public static class SolarCalculator
{
    public const double PerformanceRatio = 0.80;    // assumed overall system losses
    public const double TariffPerKwh = 1444.70;     // assumed IDR/kWh; confirm against the current PLN tariff
    public const double DefaultIrradiance = 4.8;    // kWh/m²/day; used only when no weather record exists (sample)
    public const double AnnualDegradation = 0.005;  // 0.5 % output loss per year

    // m² per panel at standard test conditions (1000 W/m²).
    public static double PanelArea(SolarPanel panel) => panel.Wattage / (1000.0 * panel.Efficiency / 100.0);

    // getAverageIrradiance(): average of saved weather records, or the sample default.
    public static double AverageIrradiance(SiteLocation site) =>
        WeatherService.SavedIrradiance(site) ?? DefaultIrradiance;

    // findOptimal(): the largest whole number of panels that fits the area.
    public static int FindOptimalPanelCount(double area, SolarPanel panel) =>
        (int)Math.Floor(area / PanelArea(panel));

    public static double CalculateOutput(double systemKwp, double irradiance) =>
        systemKwp * irradiance * 365 * PerformanceRatio;

    public static double CalculateSaving(double annualOutput) => annualOutput * TariffPerKwh;

    // generateCalculation()
    public static SolarCalculation GenerateCalculation(string projectName, SiteLocation site, SolarPanel panel,
        double area, User owner, int organizationId)
    {
        int count = FindOptimalPanelCount(area, panel);
        double kwp = count * panel.Wattage / 1000.0;
        var weather = WeatherService.LatestFor(site);
        double irradiance = AverageIrradiance(site);
        double output = CalculateOutput(kwp, irradiance);

        return new SolarCalculation
        {
            ProjectName = projectName,
            Location = site,
            PanelType = panel,
            Weather = weather,
            InputArea = area,
            Irradiance = irradiance,
            PanelCount = count,
            SystemCapacityKwp = kwp,
            EstimatedOutput = output,
            EstimateSaving = CalculateSaving(output),
            OwnerUserID = owner.UserID,
            OrganizationID = organizationId,
            CreatedAt = DateTime.Now,
            IsSample = weather == null
        };
    }

    // generateFromCalculation()
    public static Recommendation GenerateFromCalculation(SolarCalculation calc)
    {
        double optimalArea = calc.PanelCount * PanelArea(calc.PanelType);
        string note = calc.IsSample ? " Uses sample irradiance (no weather data for this location)." : "";

        return new Recommendation
        {
            CalculationID = calc.CalculationID,
            OptimalArea = optimalArea,
            EstimatedOutput = calc.EstimatedOutput,
            EstimatedSaving = calc.EstimateSaving,
            Summary = $"Install {calc.PanelCount} x {Theme.Num(calc.PanelType.Wattage)} Wp panels " +
                      $"({Theme.Num(calc.SystemCapacityKwp, "0.00")} kWp) on about {Theme.Num(optimalArea, "0.0")} m². " +
                      $"Estimated {Theme.Num(calc.EstimatedOutput)} kWh/year, about {Theme.Idr(calc.EstimateSaving)}/year." + note
        };
    }

    // Savings per year for years 1..n, with degradation applied.
    public static double[] ProjectSavings(double annualSaving, int years = 10)
    {
        var values = new double[years];
        for (int y = 0; y < years; y++)
        {
            values[y] = annualSaving * Math.Pow(1 - AnnualDegradation, y);
        }
        return values;
    }
}

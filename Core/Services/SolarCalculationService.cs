using SolarWise.Core.Models;

namespace SolarWise.Core.Services;

public class SolarCalculationService
{
    public double CalculateOutput(double inputArea, SolarPanel panelType, double averageIrradiance, double sunshineHours)
    {
        // Formula perhitungan output listrik (kWh)
        return inputArea * panelType.Efficiency * averageIrradiance * sunshineHours;
    }

    public double CalculateSaving(double estimatedOutput, double electricityRatePerKwh)
    {
        // Perhitungan estimasi penghematan biaya
        return estimatedOutput * electricityRatePerKwh;
    }

    public SolarPanel? FindOptimal(List<SolarPanel> availablePanels, double targetBudget, double availableArea)
    {
        // Logic memilih panel surya paling efisien sesuai area & budget
        return availablePanels.FirstOrDefault();
    }

    public Recommendation GenerateCalculation(Location location, double inputArea, SolarPanel panel, List<WeatherData> weatherHistory)
    {
        double avgIrradiance = weatherHistory.Count > 0 ? weatherHistory.Average(w => w.SolarIrradiance) : 0;
        double avgSunshine = weatherHistory.Count > 0 ? weatherHistory.Average(w => w.SunshineHours) : 0;

        double output = CalculateOutput(inputArea, panel, avgIrradiance, avgSunshine);
        double saving = CalculateSaving(output, 1444.70); // Contoh tarif PLN

        return new Recommendation
        {
            OptimalArea = inputArea,
            EstimatedOutput = output,
            EstimatedSaving = saving
        };
    }
}
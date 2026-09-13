using SolarWise.Core.Models;

namespace SolarWise.Core.Services;

public class WeatherService
{
    public async Task<List<WeatherData>> FetchDataAsync(Location location, DateTime startDate, DateTime endDate)
    {
        // Logic ambil data cuaca dari API eksternal atau database
        await Task.CompletedTask;
        return new List<WeatherData>();
    }

    public double GetAverageIrradiance(List<WeatherData> weatherDataList)
    {
        if (weatherDataList == null || weatherDataList.Count == 0) return 0;
        return weatherDataList.Average(w => w.SolarIrradiance);
    }
}
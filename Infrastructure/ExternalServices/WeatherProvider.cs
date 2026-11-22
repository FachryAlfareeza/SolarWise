using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace SolarWise;

// External data sources used by the weather flow: Open-Meteo geocoding and NASA POWER.
public static class WeatherProvider
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(30) };
    private static readonly string[] MonthNames =
        { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

    public static async Task<GeocodedPlace> GeocodeIndonesiaAsync(string place, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(place)) throw new ArgumentException("Enter a region or city name.", nameof(place));

        string url = "https://geocoding-api.open-meteo.com/v1/search" +
                     $"?name={Uri.EscapeDataString(place.Trim())}&count=1&language=id&countryCode=ID";
        using var response = await Client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

        if (!document.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            throw new InvalidOperationException($"No Indonesian region matched '{place}'. Try a city or regency name.");

        var result = results[0];
        string name = ReadString(result, "name");
        string admin = ReadString(result, "admin1");
        string label = string.IsNullOrWhiteSpace(admin) ||
                       string.Equals(name, admin, StringComparison.OrdinalIgnoreCase)
            ? name
            : $"{name}, {admin}";

        return new GeocodedPlace
        {
            Name = label,
            Latitude = result.GetProperty("latitude").GetDouble(),
            Longitude = result.GetProperty("longitude").GetDouble()
        };
    }

    public static async Task<IReadOnlyList<MonthlySolarIrradiance>> GetNasaPowerClimatologyAsync(
        double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var inv = CultureInfo.InvariantCulture;
        string url = "https://power.larc.nasa.gov/api/temporal/climatology/point" +
                     "?parameters=ALLSKY_SFC_SW_DWN&community=RE" +
                     $"&latitude={latitude.ToString(inv)}&longitude={longitude.ToString(inv)}&format=JSON";
        using var response = await Client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

        var values = document.RootElement
            .GetProperty("properties")
            .GetProperty("parameter")
            .GetProperty("ALLSKY_SFC_SW_DWN");

        var monthly = new List<MonthlySolarIrradiance>(12);
        for (int month = 1; month <= 12; month++)
        {
            string monthName = MonthNames[month - 1];
            if (!values.TryGetProperty(monthName, out var value) || !TryReadDouble(value, out double irradiance) || irradiance < 0)
                throw new InvalidOperationException($"NASA POWER did not return a valid irradiance value for {monthName}.");

            // NASA POWER returns this parameter as daily solar irradiation in kWh/m²/day.
            monthly.Add(new MonthlySolarIrradiance(month, irradiance));
        }

        return monthly;
    }

    private static string ReadString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) ? value.GetString() ?? "" : "";

    private static bool TryReadDouble(JsonElement element, out double value)
    {
        if (element.ValueKind == JsonValueKind.Number) return element.TryGetDouble(out value);
        return double.TryParse(element.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}

public sealed class GeocodedPlace
{
    public string Name { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

public sealed record MonthlySolarIrradiance(int Month, double KwhPerSquareMeterPerDay);

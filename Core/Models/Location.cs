namespace SolarWise.Core.Models;

public class Location
{
    public string Longitude { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public string GetCoordinates()
    {
        return $"{Latitude}, {Longitude}";
    }
}
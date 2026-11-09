using System.Globalization;

namespace SolarWise;

// Named SiteLocation to avoid clashing with Control.Location in the WinForms classes.
public class SiteLocation
{
    public int LocationID { get; set; }
    public int OwnerUserID { get; set; }       // ERD: user 1 -- 0..* locations
    public string Address { get; set; } = "";
    public string Latitude { get; set; } = ""; // string, as in the class diagram
    public string Longitude { get; set; } = ""; // string, as in the class diagram

    public (double Lat, double Lon) GetCoordinates() => (
        double.Parse(Latitude, CultureInfo.InvariantCulture),
        double.Parse(Longitude, CultureInfo.InvariantCulture));

    public override string ToString() => Address;
}

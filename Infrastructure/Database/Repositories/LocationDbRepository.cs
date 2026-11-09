using System.Globalization;
using Npgsql;

namespace SolarWise.Database;

// Table locations (ERD). Inherits CRUD from BaseRepository and only describes its own table.
public class LocationDbRepository : BaseRepository<SiteLocation>
{
    public LocationDbRepository() { }
    public LocationDbRepository(Func<NpgsqlConnection> connectionFactory) : base(connectionFactory) { }

    protected override string TableName => "locations";
    protected override string KeyColumn => "location_id";

    // Supabase columns: user_id bigint (nullable, FK to users), latitude and longitude numeric(9,6),
    // address text. The model keeps coordinates as strings (class diagram), so they are converted here.
    protected override Dictionary<string, object> ToColumns(SiteLocation site) => new()
    {
        ["user_id"] = site.OwnerUserID > 0 ? (long)site.OwnerUserID : null,   // 0 = no owner -> NULL
        ["latitude"] = ParseCoordinate(site.Latitude),
        ["longitude"] = ParseCoordinate(site.Longitude),
        ["address"] = site.Address
    };

    protected override SiteLocation FromRow(NpgsqlDataReader row) => new()
    {
        LocationID = Convert.ToInt32(row["location_id"]),
        OwnerUserID = row["user_id"] is DBNull ? 0 : Convert.ToInt32(row["user_id"]),
        Latitude = ((decimal)row["latitude"]).ToString(CultureInfo.InvariantCulture),
        Longitude = ((decimal)row["longitude"]).ToString(CultureInfo.InvariantCulture),
        Address = row["address"] as string ?? ""
    };

    protected override int GetId(SiteLocation site) => site.LocationID;
    protected override void SetId(SiteLocation site, int id) => site.LocationID = id;

    protected override void Validate(SiteLocation site)
    {
        base.Validate(site);
        if (string.IsNullOrWhiteSpace(site.Address)) throw new ArgumentException("Address is required.");
        if (!InRange(site.Latitude, 90)) throw new ArgumentException("Latitude must be between -90 and 90.");
        if (!InRange(site.Longitude, 180)) throw new ArgumentException("Longitude must be between -180 and 180.");
    }

    private static decimal ParseCoordinate(string value) =>
        decimal.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

    private static bool InRange(string value, double limit) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
        && number >= -limit && number <= limit;
}

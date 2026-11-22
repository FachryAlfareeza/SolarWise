using Npgsql;

namespace SolarWise.Database;

// Table weather_data (ERD).
public class WeatherDbRepository : BaseRepository<WeatherData>
{
    public WeatherDbRepository() { }
    public WeatherDbRepository(Func<NpgsqlConnection> connectionFactory) : base(connectionFactory) { }

    protected override string TableName => "weather_data";
    protected override string KeyColumn => "weather_data_id";

    protected override Dictionary<string, object> ToColumns(WeatherData w) => new()
    {
        ["location_id"] = (long)w.Location.LocationID,
        ["date"] = DateOnly.FromDateTime(w.Date),
        ["solar_irradiance"] = (decimal)w.SolarIrradiance,
        ["temperature"] = (decimal)w.Temperature,
        ["sunshine_hours"] = (decimal)w.SunshineHours,
        ["is_sample"] = w.IsSample
    };

    // Location is only an ID placeholder here; the caller fills in the real SiteLocation.
    protected override WeatherData FromRow(NpgsqlDataReader row) => new()
    {
        WeatherDataID = Convert.ToInt32(row["weather_data_id"]),
        Location = new SiteLocation { LocationID = Convert.ToInt32(row["location_id"]) },
        Date = row.GetDateTime(row.GetOrdinal("date")),
        SolarIrradiance = ReadDouble(row, "solar_irradiance"),
        Temperature = ReadDouble(row, "temperature"),
        SunshineHours = ReadDouble(row, "sunshine_hours"),
        IsSample = row["is_sample"] is bool b && b
    };

    private static double ReadDouble(NpgsqlDataReader row, string column) =>
        row[column] is DBNull ? 0 : Convert.ToDouble(row[column]);

    protected override int GetId(WeatherData w) => w.WeatherDataID;
    protected override void SetId(WeatherData w, int id) => w.WeatherDataID = id;

    protected override void Validate(WeatherData w)
    {
        base.Validate(w);
        if (w.Location == null || w.Location.LocationID <= 0) throw new ArgumentException("Weather data needs a saved location.");
    }

    // Records of one location, oldest date first.
    public List<WeatherData> ForLocation(int locationId) =>
        Select("location_id = @l", ("l", (long)locationId)).OrderBy(w => w.Date).ThenBy(w => w.WeatherDataID).ToList();
}

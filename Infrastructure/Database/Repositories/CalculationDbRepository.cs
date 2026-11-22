using Npgsql;

namespace SolarWise.Database;

// Table calculations (ERD, plus the extra columns from schema_additions.sql).
public class CalculationDbRepository : BaseRepository<SolarCalculation>
{
    public CalculationDbRepository() { }
    public CalculationDbRepository(Func<NpgsqlConnection> connectionFactory) : base(connectionFactory) { }

    protected override string TableName => "calculations";
    protected override string KeyColumn => "calculation_id";

    // The model holds whole objects (Location, PanelType, Weather); the table holds their IDs.
    protected override Dictionary<string, object> ToColumns(SolarCalculation c) => new()
    {
        ["user_id"] = (long)c.OwnerUserID,
        ["location_id"] = (long)c.Location.LocationID,
        ["panel_id"] = (long)c.PanelType.PanelID,
        ["weather_data_id"] = c.Weather != null && c.Weather.WeatherDataID > 0 ? (long)c.Weather.WeatherDataID : null,
        ["project_name"] = c.ProjectName ?? "",
        ["panel_type"] = c.PanelType.Name,
        ["input_area"] = (decimal)c.InputArea,
        ["irradiance"] = (decimal)c.Irradiance,
        ["panel_count"] = c.PanelCount,
        ["system_capacity_kwp"] = (decimal)c.SystemCapacityKwp,
        ["estimated_output"] = (decimal)c.EstimatedOutput,
        ["estimated_saving"] = (decimal)c.EstimateSaving,
        ["is_sample"] = c.IsSample,
        ["created_at"] = c.CreatedAt.ToUniversalTime()
    };

    // Location, PanelType and Weather are ID-only placeholders; CalculationRepository fills in the real objects.
    protected override SolarCalculation FromRow(NpgsqlDataReader row) => new()
    {
        CalculationID = Convert.ToInt32(row["calculation_id"]),
        OwnerUserID = Convert.ToInt32(row["user_id"]),
        ProjectName = row["project_name"] as string ?? "",
        Location = new SiteLocation { LocationID = Convert.ToInt32(row["location_id"]) },
        PanelType = new SolarPanel { PanelID = Convert.ToInt32(row["panel_id"]) },
        Weather = row["weather_data_id"] is DBNull ? null : new WeatherData { WeatherDataID = Convert.ToInt32(row["weather_data_id"]) },
        InputArea = ReadDouble(row, "input_area"),
        Irradiance = ReadDouble(row, "irradiance"),
        PanelCount = row["panel_count"] is DBNull ? 0 : Convert.ToInt32(row["panel_count"]),
        SystemCapacityKwp = ReadDouble(row, "system_capacity_kwp"),
        EstimatedOutput = ReadDouble(row, "estimated_output"),
        EstimateSaving = ReadDouble(row, "estimated_saving"),
        IsSample = row["is_sample"] is bool b && b,
        CreatedAt = row["created_at"] is DateTime t ? t.ToLocalTime() : DateTime.Now
    };

    private static double ReadDouble(NpgsqlDataReader row, string column) =>
        row[column] is DBNull ? 0 : Convert.ToDouble(row[column]);

    protected override int GetId(SolarCalculation c) => c.CalculationID;
    protected override void SetId(SolarCalculation c, int id) => c.CalculationID = id;

    protected override void Validate(SolarCalculation c)
    {
        base.Validate(c);
        if (c.OwnerUserID <= 0) throw new ArgumentException("A calculation needs an owner.");
        if (c.Location == null || c.Location.LocationID <= 0) throw new ArgumentException("A calculation needs a saved location.");
        if (c.PanelType == null || c.PanelType.PanelID <= 0) throw new ArgumentException("A calculation needs a saved solar panel.");
    }

    // A user's calculations, newest first.
    public List<SolarCalculation> ForUser(int userId) =>
        Select("user_id = @u", ("u", (long)userId))
            .OrderByDescending(c => c.CreatedAt).ToList();
}

using Npgsql;

namespace SolarWise.Database;

// Table solar_panels (ERD). Inherits CRUD from BaseRepository and only describes its own table.
public class SolarPanelDbRepository : BaseRepository<SolarPanel>
{
    public SolarPanelDbRepository() { }
    public SolarPanelDbRepository(Func<NpgsqlConnection> connectionFactory) : base(connectionFactory) { }

    protected override string TableName => "solar_panels";
    protected override string KeyColumn => "panel_id";

    // Supabase columns: name varchar(100), efficiency numeric(5,2), price_per_meter numeric(12,2),
    // wattage numeric(8,2), description text. The model uses double, the table uses numeric (decimal).
    protected override Dictionary<string, object> ToColumns(SolarPanel panel) => new()
    {
        ["name"] = panel.Name,
        ["efficiency"] = (decimal)panel.Efficiency,
        ["price_per_meter"] = (decimal)panel.PricePerMeter,
        ["wattage"] = (decimal)panel.Wattage,
        ["description"] = panel.Description
    };

    protected override SolarPanel FromRow(NpgsqlDataReader row) => new()
    {
        PanelID = Convert.ToInt32(row["panel_id"]),
        Name = (string)row["name"],
        Efficiency = ReadDouble(row, "efficiency"),
        PricePerMeter = ReadDouble(row, "price_per_meter"),
        Wattage = ReadDouble(row, "wattage"),
        Description = row["description"] as string ?? ""
    };

    private static double ReadDouble(NpgsqlDataReader row, string column) =>
        row[column] is DBNull ? 0 : Convert.ToDouble(row[column]);

    protected override int GetId(SolarPanel panel) => panel.PanelID;
    protected override void SetId(SolarPanel panel, int id) => panel.PanelID = id;

    protected override void Validate(SolarPanel panel)
    {
        base.Validate(panel);
        if (string.IsNullOrWhiteSpace(panel.Name)) throw new ArgumentException("Panel name is required.");
        if (panel.Name.Length > 100) throw new ArgumentException("Panel name must be 100 characters or fewer.");
        if (panel.Efficiency <= 0 || panel.Efficiency > 100) throw new ArgumentException("Efficiency must be between 0 and 100 %.");
        if (panel.Wattage <= 0 || panel.Wattage >= 1_000_000) throw new ArgumentException("Wattage must be greater than 0 and below 1,000,000.");
        if (panel.PricePerMeter < 0) throw new ArgumentException("Price per meter cannot be negative.");
    }
}

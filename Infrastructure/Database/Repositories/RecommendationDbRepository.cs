using Npgsql;

namespace SolarWise.Database;

// Table recomendation (spelled as in the ERD).
public class RecommendationDbRepository : BaseRepository<Recommendation>
{
    public RecommendationDbRepository() { }
    public RecommendationDbRepository(Func<NpgsqlConnection> connectionFactory) : base(connectionFactory) { }

    protected override string TableName => "recomendation";
    protected override string KeyColumn => "recomendation_id";

    protected override Dictionary<string, object> ToColumns(Recommendation r) => new()
    {
        ["calculation_id"] = (long)r.CalculationID,
        ["optimal_area"] = (decimal)r.OptimalArea,
        ["estimated_output"] = (decimal)r.EstimatedOutput,
        ["estimated_saving"] = (decimal)r.EstimatedSaving,
        ["summary"] = r.Summary ?? ""
    };

    protected override Recommendation FromRow(NpgsqlDataReader row) => new()
    {
        RecommendationID = Convert.ToInt32(row["recomendation_id"]),
        CalculationID = Convert.ToInt32(row["calculation_id"]),
        OptimalArea = ReadDouble(row, "optimal_area"),
        EstimatedOutput = ReadDouble(row, "estimated_output"),
        EstimatedSaving = ReadDouble(row, "estimated_saving"),
        Summary = row["summary"] as string ?? ""
    };

    private static double ReadDouble(NpgsqlDataReader row, string column) =>
        row[column] is DBNull ? 0 : Convert.ToDouble(row[column]);

    protected override int GetId(Recommendation r) => r.RecommendationID;
    protected override void SetId(Recommendation r, int id) => r.RecommendationID = id;

    protected override void Validate(Recommendation r)
    {
        base.Validate(r);
        if (r.CalculationID <= 0) throw new ArgumentException("A recommendation needs a saved calculation.");
    }

    public Recommendation ForCalculation(int calculationId) =>
        Select("calculation_id = @c", ("c", (long)calculationId)).FirstOrDefault();

    public List<Recommendation> ForCalculations(IEnumerable<int> calculationIds)
    {
        var ids = calculationIds.Select(i => (long)i).ToArray();
        return ids.Length == 0 ? new List<Recommendation>() : Select("calculation_id = ANY(@ids)", ("ids", ids));
    }
}

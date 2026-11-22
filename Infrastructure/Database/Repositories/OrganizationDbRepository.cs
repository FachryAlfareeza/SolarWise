using Npgsql;

namespace SolarWise.Database;

// Tables organizations and memberships (added on top of the ERD, see schema_additions.sql).
public class OrganizationDbRepository : BaseRepository<Organization>
{
    public OrganizationDbRepository() { }
    public OrganizationDbRepository(Func<NpgsqlConnection> connectionFactory) : base(connectionFactory) { }

    protected override string TableName => "organizations";
    protected override string KeyColumn => "organization_id";

    protected override Dictionary<string, object> ToColumns(Organization org) => new()
    {
        ["name"] = org.Name,
        ["description"] = org.Description ?? "",
        ["join_code"] = org.JoinCode
    };

    protected override Organization FromRow(NpgsqlDataReader row) => new()
    {
        OrganizationID = Convert.ToInt32(row["organization_id"]),
        Name = (string)row["name"],
        Description = row["description"] as string ?? "",
        JoinCode = (string)row["join_code"]
    };

    protected override int GetId(Organization org) => org.OrganizationID;
    protected override void SetId(Organization org, int id) => org.OrganizationID = id;

    protected override void Validate(Organization org)
    {
        base.Validate(org);
        if (string.IsNullOrWhiteSpace(org.Name)) throw new ArgumentException("Organization name is required.");
        if (string.IsNullOrWhiteSpace(org.JoinCode)) throw new ArgumentException("Join code is required.");
    }

    public List<Organization> ForUser(int userId) =>
        Select("organization_id IN (SELECT organization_id FROM memberships WHERE user_id = @u)", ("u", (long)userId));

    public Organization FindByJoinCode(string code) =>
        Select("lower(join_code) = lower(@c)", ("c", code ?? "")).FirstOrDefault();

    public bool NameTaken(string name, int exceptId) =>
        Select("lower(name) = lower(@n) AND organization_id <> @id", ("n", name), ("id", (long)exceptId)).Count > 0;

    // ---- memberships (join table user <-> organization) ----

    public void AddMember(int organizationId, int userId, string role) =>
        Execute("INSERT INTO memberships (user_id, organization_id, role) VALUES (@u, @o, @r) " +
                "ON CONFLICT (user_id, organization_id) DO NOTHING",
            ("u", (long)userId), ("o", (long)organizationId), ("r", role));

    public string RoleOf(int organizationId, int userId) =>
        Scalar("SELECT role FROM memberships WHERE user_id = @u AND organization_id = @o",
            ("u", (long)userId), ("o", (long)organizationId)) as string;

    public int MemberCount(int organizationId) =>
        Convert.ToInt32(Scalar("SELECT count(*) FROM memberships WHERE organization_id = @o", ("o", (long)organizationId)));
}

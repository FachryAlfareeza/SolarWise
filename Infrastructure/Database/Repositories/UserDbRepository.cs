using Npgsql;

namespace SolarWise.Database;

// Tables users and admins (ERD). A user who also has a row in admins is loaded as an Admin
// (inheritance in the class diagram: Admin is a User).
public class UserDbRepository : BaseRepository<User>
{
    public UserDbRepository() { }
    public UserDbRepository(Func<NpgsqlConnection> connectionFactory) : base(connectionFactory) { }

    protected override string TableName => "users";
    protected override string KeyColumn => "user_id";

    // Reads users together with their admin_id (NULL for normal users).
    protected override string FromClause =>
        "(SELECT u.*, a.admin_id FROM users u LEFT JOIN admins a ON a.user_id = u.user_id) AS users_with_role";

    // The "password" column stores the password hash, never the plain text.
    protected override Dictionary<string, object> ToColumns(User user) => new()
    {
        ["username"] = user.Username,
        ["password"] = user.PasswordHash,
        ["email"] = user.EmailAddress,
        ["full_name"] = user.FullName ?? ""
    };

    protected override User FromRow(NpgsqlDataReader row)
    {
        var user = row["admin_id"] is DBNull ? new User() : new Admin { AdminID = Convert.ToInt32(row["admin_id"]) };
        user.UserID = Convert.ToInt32(row["user_id"]);
        user.Username = (string)row["username"];
        user.PasswordHash = (string)row["password"];
        user.EmailAddress = (string)row["email"];
        user.FullName = row["full_name"] as string ?? "";
        return user;
    }

    protected override int GetId(User user) => user.UserID;
    protected override void SetId(User user, int id) => user.UserID = id;

    protected override void Validate(User user)
    {
        base.Validate(user);
        if (string.IsNullOrWhiteSpace(user.Username)) throw new ArgumentException("Username is required.");
        if (string.IsNullOrWhiteSpace(user.EmailAddress)) throw new ArgumentException("Email address is required.");
        if (string.IsNullOrEmpty(user.PasswordHash)) throw new ArgumentException("Password is required.");
    }

    // Case-insensitive match on username or email, as the login screen accepts either.
    public User FindByLogin(string login) =>
        Select("lower(username) = lower(@login) OR lower(email) = lower(@login)", ("login", login)).FirstOrDefault();

    public User FindByUsername(string username) =>
        Select("lower(username) = lower(@v)", ("v", username)).FirstOrDefault();

    public User FindByEmail(string email) =>
        Select("lower(email) = lower(@v)", ("v", email)).FirstOrDefault();

    public bool HasAnyUser() => GetAll().Count > 0;

    // Gives an existing user a row in admins (makes them an Admin). Returns the new admin_id.
    public int GrantAdmin(User user)
    {
        Execute("INSERT INTO admins (user_id) VALUES (@u)", ("u", (long)user.UserID));
        return Select("user_id = @u", ("u", user.UserID)).First() is Admin a ? a.AdminID : 0;
    }
}

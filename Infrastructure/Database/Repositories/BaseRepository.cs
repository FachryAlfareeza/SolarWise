using Npgsql;

namespace SolarWise.Database;

// Base class for every table repository: CRUD (INSERT, SELECT, UPDATE, DELETE) to Supabase.
// The tables already exist in Supabase (they follow the ERD); this code does not create them.
//
// OOP concepts:
// - Inheritance: each table repository (SolarPanelDbRepository, LocationDbRepository) inherits
//   GetAll, GetById, Insert, Update and Delete, so the SQL code is written only once.
// - Encapsulation: the connection and SQL building are private. Subclasses and callers cannot
//   open connections or write SQL themselves; they only use the public CRUD methods.
// - Polymorphism: the abstract/virtual members below are overridden by each subclass, so the same
//   Insert() call runs different SQL and validation depending on the actual repository object.
public abstract class BaseRepository<T> where T : class
{
    private readonly Func<NpgsqlConnection> connectionFactory;

    protected BaseRepository() : this(SupabaseConnectionFactory.GetConnection) { }

    // Lets tests or another database supply their own connection.
    protected BaseRepository(Func<NpgsqlConnection> connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    // ---- Members each table must define (polymorphism) ----

    protected abstract string TableName { get; }

    // What SELECT reads from. A table name by default; a repository can override it with a
    // sub-select (for example users joined with admins) and the inherited CRUD still works.
    protected virtual string FromClause => TableName;
    protected abstract string KeyColumn { get; }

    // Column name -> value for INSERT and UPDATE (the key column is excluded).
    protected abstract Dictionary<string, object> ToColumns(T entity);

    // Builds an entity from one row of the SELECT result.
    protected abstract T FromRow(NpgsqlDataReader row);

    protected abstract int GetId(T entity);
    protected abstract void SetId(T entity, int id);

    // Optional rule check before INSERT and UPDATE; subclasses override to add their own rules.
    protected virtual void Validate(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
    }

    // ---- CRUD inherited by every repository ----

    public List<T> GetAll()
    {
        var result = new List<T>();
        using var conn = Open();
        using var cmd = new NpgsqlCommand($"SELECT * FROM {FromClause} ORDER BY {KeyColumn}", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) result.Add(FromRow(reader));
        return result;
    }

    public T GetById(int id)
    {
        using var conn = Open();
        using var cmd = new NpgsqlCommand($"SELECT * FROM {FromClause} WHERE {KeyColumn} = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? FromRow(reader) : null;
    }

    // Several rows by ID in one query (used to fill in related objects without one query per row).
    public List<T> GetByIds(IEnumerable<int> ids)
    {
        var list = ids.Distinct().Select(i => (long)i).ToArray();
        if (list.Length == 0) return new List<T>();
        return Select($"{KeyColumn} = ANY(@ids)", ("ids", list));
    }

    // INSERT; the new ID from the database is written back to the entity.
    public T Insert(T entity)
    {
        Validate(entity);
        var columns = ToColumns(entity);
        string names = string.Join(", ", columns.Keys);
        string values = string.Join(", ", columns.Keys.Select(c => "@" + c));

        using var conn = Open();
        using var cmd = new NpgsqlCommand(
            $"INSERT INTO {TableName} ({names}) VALUES ({values}) RETURNING {KeyColumn}", conn);
        AddParameters(cmd, columns);
        SetId(entity, Convert.ToInt32(cmd.ExecuteScalar()));
        return entity;
    }

    // UPDATE; returns false when no row has the entity's ID.
    public bool Update(T entity)
    {
        Validate(entity);
        var columns = ToColumns(entity);
        string assignments = string.Join(", ", columns.Keys.Select(c => $"{c} = @{c}"));

        using var conn = Open();
        using var cmd = new NpgsqlCommand(
            $"UPDATE {TableName} SET {assignments} WHERE {KeyColumn} = @key_id", conn);
        AddParameters(cmd, columns);
        cmd.Parameters.AddWithValue("key_id", GetId(entity));
        return cmd.ExecuteNonQuery() > 0;
    }

    // DELETE; returns false when no row has that ID.
    public bool Delete(int id)
    {
        using var conn = Open();
        using var cmd = new NpgsqlCommand($"DELETE FROM {TableName} WHERE {KeyColumn} = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(T entity) => Delete(GetId(entity));

    // ---- Helpers for subclasses: filtered queries. Values are always parameters. ----

    // SELECT with a WHERE clause written by the subclass (never built from user input).
    protected List<T> Select(string where, params (string Name, object Value)[] args)
    {
        var result = new List<T>();
        using var conn = Open();
        using var cmd = new NpgsqlCommand($"SELECT * FROM {FromClause} WHERE {where} ORDER BY {KeyColumn}", conn);
        foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) result.Add(FromRow(reader));
        return result;
    }

    // INSERT/UPDATE/DELETE written by the subclass; returns the number of rows changed.
    protected int Execute(string sql, params (string Name, object Value)[] args)
    {
        using var conn = Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return cmd.ExecuteNonQuery();
    }

    // A single value (for example a COUNT) from a query written by the subclass.
    protected object Scalar(string sql, params (string Name, object Value)[] args)
    {
        using var conn = Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return cmd.ExecuteScalar();
    }

    // ---- Private helpers (encapsulation) ----

    private NpgsqlConnection Open()
    {
        var conn = connectionFactory();
        conn.Open();
        return conn;
    }

    // Values are always sent as parameters, never pasted into the SQL text (prevents SQL injection).
    private static void AddParameters(NpgsqlCommand cmd, Dictionary<string, object> columns)
    {
        foreach (var (name, value) in columns)
        {
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }
    }
}

using Npgsql;

namespace SolarWise.Database
{
    public class SupabaseConnectionFactory
    {
        private static string connectionString =>
            $"Host={AppConfig.DbHost};" +
            $"Port={AppConfig.DbPort};" +
            $"Database={AppConfig.DbName};" +
            $"Username={AppConfig.DbUser};" +
            $"Password={AppConfig.DbPassword};" +
            "SSL Mode=Require;" +
            // Needed for the Supabase transaction pooler (port 6543): without it, a reused pooled
            // connection sends a reset command the pooler does not answer and the next query times out.
            "No Reset On Close=true;";

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }

        public static bool TestConnection(out string message)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand("select version()", conn);
                message = "Terhubung!\n" + cmd.ExecuteScalar();
                return true;
            }
            catch (Exception ex)
            {
                message = "Gagal: " + ex.Message;
                return false;
            }
        }
    }
}

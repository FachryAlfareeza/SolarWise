using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using DotNetEnv;

namespace SolarWise.Database
{
    public class SupabaseConnection
    {
        static SupabaseConnection()
        {
            Env.TraversePath().Load();
        }

        private static string connectionString =>
            $"Host={Env.GetString("DB_HOST")};" +
            $"Port={Env.GetString("DB_PORT", "5432")};" +
            "Database=postgres;" +
            $"Username={Env.GetString("DB_USER")};" +
            $"Password={Env.GetString("DB_PASSWORD")};" +
            "SSL Mode=Require;";

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

using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace Query
{
    public static class Sql
    {
        public static string connectionString;

        private static string GetStoredProc(string storedproc, Dictionary<string, object> parameters = null)
        {
            var sql = new StringBuilder("EXEC " + storedproc);
            if (parameters != null)
            {
                var x = 0;
                foreach (var parm in parameters)
                {
                    sql.Append(" " + (x > 0 ? "," : "") + "@" + parm.Key + "=@" + parm.Key);
                    x++;
                }
            }
            return sql.ToString();
        }

        private static List<SqlParameter> GetSqlParameters(Dictionary<string, object> parameters = null)
        {
            var parms = new List<SqlParameter>();
            foreach (var parm in parameters)
            {
                parms.Add(new SqlParameter("@" + parm.Key, parm.Value));
            }
            return parms;
        }

        public static SqlDataReader ExecuteReader(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(GetStoredProc(storedproc, parameters), conn))
                {
                    conn.Open();
                    return cmd.ExecuteReader();
                }
            }
        }

        public static void ExecuteNonQuery(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(GetStoredProc(storedproc, parameters), conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static T ExecuteScalar<T>(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(GetStoredProc(storedproc, parameters), conn))
                {
                    conn.Open();
                    return (T)cmd.ExecuteScalar();
                }
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(GetStoredProc(storedproc, parameters), conn))
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public static List<T> Populate<T>(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(GetStoredProc(storedproc, parameters), conn))
                {
                    conn.Open();
                    return conn.Query<T>(GetStoredProc(storedproc, parameters), parameters).AsList<T>();
                }
            }
        }

        public static SqlMapper.GridReader PopulateMultiple(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(GetStoredProc(storedproc, parameters), conn))
                {
                    conn.Open();
                    return conn.QueryMultiple(GetStoredProc(storedproc, parameters), parameters);
                }
            }
        }
    }
}

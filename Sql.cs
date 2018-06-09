using System;
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
                    if(parm.Value == null)
                    {
                        sql.Append(" " + (x > 0 ? "," : "") + "@" + parm.Key + "=NULL");
                    }
                    else
                    {
                        sql.Append(" " + (x > 0 ? "," : "") + "@" + parm.Key + "=@" + parm.Key);
                    }
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
                parms.Add(new SqlParameter("@" + parm.Key, parm.Value ?? DBNull.Value));
            }
            return parms;
        }

        public static void ExecuteNonQuery(string storedproc, Dictionary<string, object> parameters = null)
        {
            using(SqlConnection conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = GetStoredProc(storedproc, parameters);
                if (parameters != null) { cmd.Parameters.AddRange(GetSqlParameters(parameters).ToArray()); }
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            
        }

        public static T ExecuteScalar<T>(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = GetStoredProc(storedproc, parameters);
                if (parameters != null) { cmd.Parameters.AddRange(GetSqlParameters(parameters).ToArray()); }
                cmd.Connection.Open();
                return (T)cmd.ExecuteScalar();
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var newConnection = new SqlConnection(connectionString))
            {
                using (var newCommand = new SqlCommand(GetStoredProc(storedproc, parameters), newConnection))
                {
                    try
                    {
                        if (parameters != null) newCommand.Parameters.AddRange(GetSqlParameters(parameters).ToArray());
                        await newConnection.OpenAsync().ConfigureAwait(false);
                        return await newCommand.ExecuteNonQueryAsync().ConfigureAwait(false);

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        public static List<T> Populate<T>(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.Query<T>(GetStoredProc(storedproc, parameters), parameters).AsList<T>();
            }
        }

        public static SqlMapper.GridReader PopulateMultiple(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.QueryMultiple(GetStoredProc(storedproc, parameters), parameters);
            }
        }
    }
}

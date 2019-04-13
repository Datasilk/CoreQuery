using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Dapper;

namespace Query
{
    public class Connection : IDisposable
    {
        private SqlConnection conn;
        private string query;
        private Dictionary<string, object> parameters;

        public Connection(string storedProcedure, Dictionary<string, object> parameters)
        {
            conn = new SqlConnection(Sql.connectionString);
            query = GetStoredProc(storedProcedure, parameters);
            this.parameters = parameters;
            conn.Open();
        }

        public void ExecuteNonQuery()
        {
            var cmd = new SqlCommand(query, conn);
            if (parameters != null) { cmd.Parameters.AddRange(GetSqlParameters(parameters).ToArray()); }
            cmd.ExecuteNonQuery();
        }

        public T ExecuteScalar<T>()
        {
            var cmd = new SqlCommand(query, conn);
            if (parameters != null) { cmd.Parameters.AddRange(GetSqlParameters(parameters).ToArray()); }
            return (T)cmd.ExecuteScalar();
        }
        
        public List<T> Query<T>()
        {
            return conn.Query<T>(query, parameters).AsList<T>();
        }

        public SqlMapper.GridReader PopulateMultiple()
        {
            return conn.QueryMultiple(query, parameters);
        }

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
                var param = new SqlParameter("@" + parm.Key, parm.Value == null ? DBNull.Value : parm.Value);
                parms.Add(param);
            }
            return parms;
        }

        public void Dispose()
        {
            if(conn.State != System.Data.ConnectionState.Closed)
            {
                conn.Close();
            }
            conn.Dispose();
        }
    }

    public static class Sql
    {
        public static string connectionString;

        public static void ExecuteNonQuery(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var sql = new Connection(storedproc, parameters))
            {
                sql.ExecuteNonQuery();
            }
        }

        public static T ExecuteScalar<T>(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var sql = new Connection(storedproc, parameters))
            {
                return (T)sql.ExecuteScalar<T>();
            }
        }

        public static List<T> Populate<T>(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var sql = new Connection(storedproc, parameters))
            {
                    return sql.Query<T>();
            }
        }
    }
}

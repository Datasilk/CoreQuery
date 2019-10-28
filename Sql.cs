using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Dapper;

namespace Query
{
    public class Connection : IDisposable
    {
        private SqlConnection conn;
        private string query;
        private dynamic parameters { get; set; }

        public Connection(string storedProcedure)
        {
            conn = new SqlConnection(Sql.connectionString);
            query = GetStoredProc(storedProcedure);
            conn.Open();
        }

        public Connection(string storedProcedure, dynamic parameters)
        {
            conn = new SqlConnection(Sql.connectionString);
            query = GetStoredProc(storedProcedure, parameters);
            this.parameters = parameters;
            conn.Open();
        }

        public void ExecuteNonQuery()
        {
            var cmd = new CommandDefinition(query, parameters);
            conn.Execute(cmd);
        }

        public T ExecuteScalar<T>()
        {
            var cmd = new CommandDefinition(query, parameters);
            return (T)conn.ExecuteScalar(cmd);
        }

        public List<T> Query<T>()
        {
            var cmd = new CommandDefinition(query, parameters);
            return conn.Query<T>(cmd).AsList<T>();
        }

        public SqlMapper.GridReader PopulateMultiple()
        {
            var cmd = new CommandDefinition(query, parameters);
            return conn.QueryMultiple(cmd);
        }

        private static string GetStoredProc(string storedproc)
        {
            var sql = new StringBuilder("EXEC " + storedproc);
            return sql.ToString();
        }

        private static string GetStoredProc(string storedproc, dynamic parameters = null)
        {
            var sql = new StringBuilder("EXEC " + storedproc);
            if (parameters != null)
            {
                var properties = parameters.GetType().GetProperties();
                var x = 0;
                foreach (var prop in properties)
                {
                    sql.Append(" " + (x > 0 ? "," : "") + "@" + prop.Name + "=@" + prop.Name);
                    x++;
                }
            }
            return sql.ToString();
        }

        public void Dispose()
        {
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
            }
            conn.Dispose();
        }
    }

    public static class Sql
    {
        public static string connectionString;

        public static void ExecuteNonQuery(string storedproc)
        {
            using (var sql = new Connection(storedproc))
            {
                sql.ExecuteNonQuery();
            }
        }

        public static void ExecuteNonQuery(string storedproc, dynamic parameters = null)
        {
            using (var sql = new Connection(storedproc, parameters))
            {
                sql.ExecuteNonQuery();
            }
        }

        public static T ExecuteScalar<T>(string storedproc)
        {
            using (var sql = new Connection(storedproc))
            {
                return (T)sql.ExecuteScalar<T>();
            }
        }

        public static T ExecuteScalar<T>(string storedproc, dynamic parameters = null)
        {
            using (var sql = new Connection(storedproc, parameters))
            {
                return (T)sql.ExecuteScalar<T>();
            }
        }

        public static List<T> Populate<T>(string storedproc)
        {
            using (var sql = new Connection(storedproc))
            {
                return sql.Query<T>();
            }
        }

        public static List<T> Populate<T>(string storedproc, dynamic parameters = null)
        {
            using (var sql = new Connection(storedproc, parameters))
            {
                return sql.Query<T>();
            }
        }
    }
}
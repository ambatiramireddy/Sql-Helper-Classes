using AddAppAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AddAppAPI.Helpers
{
    public class SqlHelper : ISqlHelper
    {
        string connectionString;

        public SqlHelper()
        {
            this.connectionString = ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString;
        }

        public SqlHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<T> SelectOneAsync<T>(List<Parameter> parameters) where T : class, new()
        {
            T item = null;
            using (var con = this.GetConnection())
            {
                var procName = string.Format("usp_Select_{0}", typeof(T).Name);
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();

                    foreach (var nv in parameters)
                    {
                        cmd.Parameters.AddWithValue($"@{nv.Name}", nv.Value);
                    }

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        item = reader.ToObject<T>();
                    }
                }
            }

            return item;
        }

        public async Task<List<T>> SelectAllAsync<T>() where T : class, new()
        {
            List<T> items = null;
            using (var con = this.GetConnection())
            {
                var typeName = typeof(T).Name;
                var procName = string.Format("usp_SelectAll_{0}", (typeName.EndsWith("y") ? $"{typeName.TrimEnd('y')}ies" : $"{typeName}s"));
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        items = reader.ToList<T>();
                    }
                }
            }

            return items;
        }

        public async Task<List<R>> SelectIdsAsync<T, R>(List<Parameter> parameters) where T : class, new() where R : struct
        {
            List<R> items = null;
            using (var con = this.GetConnection())
            {
                var procName = string.Format("usp_Select_{0}Ids_By_{1}", typeof(T).Name, parameters.First().Name);
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();

                    foreach (var nv in parameters)
                    {
                        cmd.Parameters.AddWithValue($"@{nv.Name}", nv.Value);
                    }

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        items = reader.ToValueTypeList<R>();
                    }
                }
            }

            return items;
        }

        public async Task<List<KeyValuePair<K, V>>> SelectIdNamePairsAsync<T, K, V>() where T : class, new()
        {
            List<KeyValuePair<K, V>> idNamePairs = null;
            using (var con = this.GetConnection())
            {
                var procName = string.Format("usp_Select_{0}_IdNamePairs", typeof(T).Name);
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        idNamePairs = reader.ToKeyValuePairList<K, V>();
                    }
                }
            }

            return idNamePairs;
        }

        public async Task<R> InsertOneAsync<T, R>(T item) where T : class, new()
        {
            R result = default(R);
            try
            {
                using (var con = this.GetConnection())
                {
                    var typeName = typeof(T).Name;
                    var procName = string.Format("usp_Insert_{0}", typeName);
                    using (var cmd = new SqlCommand(procName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        DataTable dt = new DataTable();
                        dt.Fill<T>(item);
                        cmd.Parameters.AddWithValue($"@{typeName}", dt);
                        await con.OpenAsync();
                        result = (R)Convert.ChangeType(await cmd.ExecuteScalarAsync(), typeof(R));
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    // violation of primary key
                }

                throw ex;
            }

            return result;
        }

        public async Task<bool> InsertManyAsync<T>(T[] items) where T : class, new()
        {
            int rowsAffected = 0;
            try
            {
                using (var con = this.GetConnection())
                {
                    var typeName = typeof(T).Name;
                    var procName = string.Format("usp_Insert_{0}", typeName);
                    using (var cmd = new SqlCommand(procName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        DataTable dt = new DataTable();
                        dt.Fill<T>(items);
                        cmd.Parameters.AddWithValue($"@{typeName}", dt);
                        await con.OpenAsync();
                        rowsAffected = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    // violation of primary key
                }

                throw ex;
            }

            return rowsAffected == 0 ? false : true;
        }

        public async Task<bool> UpdateAsync<T>(T item) where T : class, new()
        {
            int rowsAffected = 0;
            using (var con = this.GetConnection())
            {
                var typeName = typeof(T).Name;
                var procName = string.Format("usp_Update_{0}", typeName);
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    DataTable dt = new DataTable();
                    dt.Fill<T>(item);
                    cmd.Parameters.AddWithValue($"@{typeName}", dt);
                    await con.OpenAsync();
                    rowsAffected = await cmd.ExecuteNonQueryAsync();
                }
            }

            return rowsAffected == 0 ? false : true;
        }


        public async Task<bool> DeleteAsync<T>(List<Parameter> parameters) where T : class, new()
        {
            int rowsAffected = 0;
            using (var con = this.GetConnection())
            {
                var typeName = typeof(T).Name;
                var procName = string.Format("usp_Delete_{0}", typeName);
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();
                    foreach (var nv in parameters)
                    {
                        cmd.Parameters.AddWithValue($"@{nv.Name}", nv.Value);
                    }

                    rowsAffected = await cmd.ExecuteNonQueryAsync();
                }
            }

            return rowsAffected == 0 ? false : true;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(this.connectionString);
        }
    }
}
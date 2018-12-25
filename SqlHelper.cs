﻿using AddAppAPI.Extensions;
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
    public class SqlHelper<T> : ISqlHelper<T> where T : class, new()
    {
        string connectionString;
        string tableName;
        string selectProcName;
        string selectAllProcName;
        string selectIdNamePairsProcName;
        string insertProcName;
        string updateProcName;
        string deleteProcName;

        public SqlHelper()
        {
            this.connectionString = ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString;
            this.Initialize();
        }

        public SqlHelper(string connectionString)
        {
            this.connectionString = connectionString;
            this.Initialize();
        }

        public void Initialize()
        {
            this.tableName = typeof(T).Name;
            this.selectProcName = string.Format("usp_Select_{0}", tableName);
            this.selectAllProcName = string.Format("usp_SelectAll_{0}s", tableName);
            this.selectIdNamePairsProcName = string.Format("usp_Select_{0}_IdNamePairs", tableName);
            this.insertProcName = string.Format("usp_Insert_{0}", tableName);
            this.updateProcName = string.Format("usp_Update_{0}", tableName);
            this.deleteProcName = string.Format("usp_Delete_{0}", tableName);
        }

        public async Task<T> SelectOneAsync(Dictionary<string, object> parameters)
        {
            T item = null;
            using (var con = this.GetConnection())
            {
                using (var cmd = new SqlCommand(this.selectProcName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();
                    foreach (var kv in parameters)
                    {
                        cmd.Parameters.AddWithValue($"@{kv.Key}", kv.Value);
                    }
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        item = reader.ToObject<T>();
                    }
                }
            }

            return item;
        }

        public async Task<List<T>> SelectAllAsync()
        {
            List<T> items = null;
            using (var con = this.GetConnection())
            {
                using (var cmd = new SqlCommand(this.selectAllProcName, con))
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

        public async Task<Dictionary<I, N>> SelectIdNamePairsAsync<I, N>()
        {
            Dictionary<I, N> idNamePairs = null;
            using (var con = this.GetConnection())
            {
                using (var cmd = new SqlCommand(this.selectIdNamePairsProcName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        idNamePairs = reader.ToDictionary<I, N>();
                    }
                }
            }

            return idNamePairs;
        }

        public async Task<bool> InsertAsync(T item)
        {
            int rowsAffected = 0;
            try
            {
                using (var con = this.GetConnection())
                {
                    using (var cmd = new SqlCommand(this.insertProcName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue($"@{this.tableName}", new DataTable().Fill<T>(item));
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

        public async Task<bool> UpdateAsync(T item)
        {
            int rowsAffected = 0;
            using (var con = this.GetConnection())
            {
                using (var cmd = new SqlCommand(this.updateProcName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue($"@{this.tableName}", new DataTable().Fill<T>(item));
                    await con.OpenAsync();
                    rowsAffected = await cmd.ExecuteNonQueryAsync();
                }
            }

            return rowsAffected == 0 ? false : true;
        }


        public async Task<bool> DeleteAsync(Dictionary<string, object> parameters)
        {
            int rowsAffected = 0;
            using (var con = this.GetConnection())
            {
                using (var cmd = new SqlCommand(this.deleteProcName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();
                    foreach (var kv in parameters)
                    {
                        cmd.Parameters.AddWithValue($"@{kv.Key}", kv.Value);
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
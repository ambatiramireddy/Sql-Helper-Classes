using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AddAppAPI.Helpers
{
    public interface ISqlHelper
    {
        Task<List<T>> SelectAllAsync<T>() where T : class, new();

        Task<List<R>> SelectIdsByReferenceKeyAsync<T, R>(List<Parameter> parameters) where T : class, new() where R : struct;

        Task<List<T>> SelectAllByReferenceKeyAsync<T>(List<Parameter> parameters) where T : class, new();

        Task<List<KeyValuePair<K, V>>> SelectIdNamePairsAsync<T, K, V>() where T : class, new();

        Task<T> SelectOneAsync<T>(List<Parameter> parameters) where T : class, new();

        Task<R> InsertOneAsync<T, R>(T item) where T : class, new();

        Task<bool> InsertManyAsync<T>(T[] items) where T : class, new();

        Task<bool> UpdateAsync<T>(T item) where T : class, new();

        Task<bool> DeleteAsync<T>(List<Parameter> parameters) where T : class, new();
    }
}
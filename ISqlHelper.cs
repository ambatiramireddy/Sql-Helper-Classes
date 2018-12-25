using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AddAppAPI.Helpers
{
    public interface ISqlHelper<T>
    {
        Task<List<T>> SelectAllAsync();

        Task<Dictionary<I, N>> SelectIdNamePairsAsync<I, N>();

        Task<T> SelectOneAsync(Dictionary<string, object> parameters);

        Task<bool> InsertAsync(T item);

        Task<bool> UpdateAsync(T item);

        Task<bool> DeleteAsync(Dictionary<string, object> parameters);
    }
}
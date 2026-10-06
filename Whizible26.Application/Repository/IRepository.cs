using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhizibleTeams.Application.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<int> AddAsyncSP(string storedProcedureName, List<SqlParameter> sqlParameters);
        Task<int> UpdateAsyncSP(string storedProcedureName, List<SqlParameter> sqlParameters);
        Task<int> DeleteByIdAsyncSP(string storedProcedureName, object id);
    }
}

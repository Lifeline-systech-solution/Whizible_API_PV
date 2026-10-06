using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleTeams.Application.Repository;

namespace Whizible26.Application.Repository.DashboardRepo
{
    public class RM_CapacityPlanningRepo : BaseRepository<CapacityPlanningEntity>
    {
        private readonly string _sqlConnectionString;

        public RM_CapacityPlanningRepo()
        {
        }

        public RM_CapacityPlanningRepo(string connectionString)
            : base(connectionString)
        {
            _sqlConnectionString = connectionString;
        }

        public async Task<DataTable> GetDataTableFromStoredProcedureAsync(
            string storedProcedure,
            List<SqlParameter>? parameters)
        {
            using var con = new SqlConnection(_sqlConnectionString);
            using var cmd = new SqlCommand(storedProcedure, con)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Count > 0)
            {
                cmd.Parameters.AddRange(parameters.ToArray());
            }

            await con.OpenAsync();

            using var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        /// <summary>
        /// Capacity Planning multi-result SP reader.
        /// Returns Data, WeekHeaders, MonthHeaders, Pagination without changing BaseRepository.
        /// </summary>
        public async Task<dynamic> GetCpDynamicAsyncSP(
            string storedProcedureName,
            List<SqlParameter> sqlParameters)
        {
            using var connection = new SqlConnection(_sqlConnectionString);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            foreach (var param in sqlParameters)
            {
                parameters.Add(
                    param.ParameterName,
                    param.Value,
                    direction: ParameterDirection.Input);
            }

            using var multi = await connection.QueryMultipleAsync(
                storedProcedureName,
                parameters,
                commandType: CommandType.StoredProcedure);

            var data = (await multi.ReadAsync())
                .Select(x => (IDictionary<string, object>)x)
                .ToList();

            var weekHeaders = new List<IDictionary<string, object>>();
            var monthHeaders = new List<IDictionary<string, object>>();
            var pagination = new List<IDictionary<string, object>>();

            while (!multi.IsConsumed)
            {
                var table = (await multi.ReadAsync())
                    .Select(x => (IDictionary<string, object>)x)
                    .ToList();

                if (table.Count == 0)
                    continue;

                var first = table[0];

                if (HasAllKeysIgnoreCase(first, "CurrentPage", "PageSize", "TotalRecords"))
                {
                    pagination = table;
                }
                else if (HasAnyKeyIgnoreCase(first, "WkStartDate", "WkEndDate", "WkNo"))
                {
                    weekHeaders = table;
                }
                else if (HasAnyKeyIgnoreCase(first, "MonthDate")
                    || (HasAnyKeyIgnoreCase(first, "Month") && HasAnyKeyIgnoreCase(first, "Quarter")))
                {
                    monthHeaders = table;
                }
            }

            var result = new ExpandoObject() as IDictionary<string, object>;
            result.Add("Data", data);
            result.Add("WeekHeaders", weekHeaders);
            result.Add("MonthHeaders", monthHeaders);
            result.Add("Pagination", pagination);
            return result;
        }

        private static bool HasAllKeysIgnoreCase(IDictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (!HasAnyKeyIgnoreCase(row, key))
                    return false;
            }
            return true;
        }

        private static bool HasAnyKeyIgnoreCase(IDictionary<string, object> row, params string[] keys)
        {
            if (row == null || keys == null || keys.Length == 0)
                return false;

            foreach (var key in keys)
            {
                if (row.Keys.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
            return false;
        }
    }
}

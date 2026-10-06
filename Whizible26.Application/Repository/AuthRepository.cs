using Microsoft.Data.SqlClient;
using System.Data;
using Whizible26.Domain.Entity;
using WhizibleTeams.Application.Repository;

namespace Whizible26.Application.Repository
{
    /// <summary>
    /// Data access for authentication (token / login validation).
    /// </summary>
    public class AuthRepository : BaseRepository<AuthRepository>
    {
        public AuthRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Calls usp_ValidateActiveLogin with parameterized SP (same pattern as EmployeeSkillsBulkUploadRepo).
        /// </summary>
        public async Task<List<ValidateActiveLoginEntity>> ValidateActiveLoginAsync(
            string loginName,
            string encryptedPassword)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@LoginName", SqlDbType.NVarChar, 500) { Value = loginName },
                new SqlParameter("@Password", SqlDbType.NVarChar, 500) { Value = encryptedPassword }
            };

            var dataSet = await GetDataSetAsync("usp_ValidateActiveLogin", parameters).ConfigureAwait(false);
            return MapLoginRowsFromDataSet(dataSet);
        }

        private static List<ValidateActiveLoginEntity> MapLoginRowsFromDataSet(DataSet dataSet)
        {
            var mapped = new List<ValidateActiveLoginEntity>();
            if (dataSet.Tables.Count == 0)
            {
                return mapped;
            }

            foreach (DataTable table in dataSet.Tables)
            {
                if (table.Rows.Count == 0 || !HasColumn(table, "LoginID"))
                {
                    continue;
                }

                foreach (DataRow row in table.Rows)
                {
                    mapped.Add(new ValidateActiveLoginEntity
                    {
                        LoginID = row["LoginID"] == DBNull.Value ? 0 : Convert.ToInt32(row["LoginID"]),
                        LoginName = row.Table.Columns.Contains("LoginName") ? row["LoginName"]?.ToString() : null,
                        Password = row.Table.Columns.Contains("Password") ? row["Password"]?.ToString() : null,
                        SecurityStamp = ReadGuid(row, "SecurityStamp"),
                        UserName = row.Table.Columns.Contains("UserName") ? row["UserName"]?.ToString() : null,
                        DispalyName = row.Table.Columns.Contains("DispalyName") ? row["DispalyName"]?.ToString() : null
                    });
                }

                if (mapped.Count > 0)
                {
                    return mapped;
                }
            }

            return mapped;
        }

        private static bool HasColumn(DataTable table, string columnName)
            => table.Columns.Cast<DataColumn>()
                .Any(c => c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));

        private static Guid? ReadGuid(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return null;
            }

            return row[columnName] switch
            {
                Guid guid => guid,
                string text when Guid.TryParse(text, out var parsed) => parsed,
                _ => null
            };
        }
    }
}

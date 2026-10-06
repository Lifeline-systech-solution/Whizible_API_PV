using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.Repository.DashboardRepo;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.Application.CommandsQueries.Dashboard
{
    // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API
    public class ProjectProfitByCustomer
    {
        private readonly IConfiguration _configuration;
        private readonly ProjectProfitByCustomerRepo _repository;
        private readonly string _connectionString;

        // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API - default constructor required by the repository pattern
        public ProjectProfitByCustomer() { }
        // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API

        // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API - initialise the service with configuration
        public ProjectProfitByCustomer(IConfiguration configuration)
        {
            _configuration = configuration;
            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(
                    _configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);
                _repository = new ProjectProfitByCustomerRepo(_connectionString);
            }
        }
        // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API

        #region Helpers

        // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API - guard against an unconfigured repository
        private ResponseEntity RepositoryNotConfigured()
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message = "Database connection is not configured." }
            };
        }
        // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API

        // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API - Empty / Swagger "string" -> no filter
        private static string? NullIfBlank(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            if (trimmed.Equals("string", StringComparison.OrdinalIgnoreCase)) return null;
            return trimmed;
        }
        // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API

        // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API - CSV of ints; no valid int -> NULL (no filter)
        private static string? NullIfBlankIntCsv(string? value)
        {
            var trimmed = NullIfBlank(value);
            if (trimmed == null) return null;

            var valid = trimmed.Split(',')
                .Select(p => p.Trim())
                .Where(p => p.Length > 0 && int.TryParse(p, out _))
                .ToList();

            return valid.Count == 0 ? null : string.Join(",", valid);
        }
        // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API

        private static ResponseEntity Success(string message, object data)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.SUCCESS,
                Data = new { message, data }
            };
        }

        private static ResponseEntity Failure(Exception ex)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new
                {
                    message = ex.Message
                        + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")
                }
            };
        }

        #endregion

        // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API
        public async Task<ResponseEntity> GetProjectProfitByCustomer(ProjectProfitByCustomerRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectProfitByCustomerRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intCustomerID",
                        (object?)request.CustomerID ?? DBNull.Value),
                    new SqlParameter("@strProjectID",
                        (object?)NullIfBlankIntCsv(request.ProjectIDs) ?? DBNull.Value),
                    new SqlParameter("@intProjectID",
                        (object?)request.ProjectID ?? DBNull.Value),
                    new SqlParameter("@GPMTrend", request.GPMTrend),
                    // Added By Vyankat B. on 24th Aug 2026
                    new SqlParameter("@strBusinessGroupID",
                        (object?)NullIfBlankIntCsv(request.BusinessGroupIDs) ?? DBNull.Value),
                    new SqlParameter("@strOrganizationUnitID",
                        (object?)NullIfBlankIntCsv(request.OrganizationUnitIDs) ?? DBNull.Value),
                    new SqlParameter("@strPeriod",
                        (object?)NullIfBlank(request.Period) ?? DBNull.Value)
                    // End of Added By Vyankat B. on 24th Aug 2026
                };

                // 1 result set -> the main grid rows
                var result = await _repository.GetAsyncSP<ProjectProfitByCustomerModel>(
                    "usp_SEL_Whizible2_ProjectProfitByCustomer", sqlParams);

                return Success("Project profitability by customer", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API

        public async Task<ResponseEntity> GetGpm(ProjectProfitabilityGpmRequest request)
        {
            try
            {
                if (_repository == null)
                    return new ResponseEntity { Status = ResponseStatus.FAILURE, Data = new { message = "Database connection is not configured." } };

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectID", request.ProjectID)
                };

                // 1 result set: latest GPM row for the project, including CurrencySymbol
                var result = await _repository.GetAsyncSP<ProjectProfitabilityGpmModel>(
                    "usp_SEL_Whizible2_ProjectProfitability_GPM", sqlParams);

                return new ResponseEntity { Status = ResponseStatus.SUCCESS, Data = new { message = "Project profitability GPM", data = result } };
            }
            catch (Exception ex)
            {
                return new ResponseEntity { Status = ResponseStatus.FAILURE, Data = new { message = ex.Message } };
            }
        }

        // Added by Aditya J. on 07-08-2026 for Project Task Case Structure API
        public async Task<ResponseEntity> GetTaskCaseStructure(ProjectTaskCaseStructureRequest request)
        {
            try
            {
                if (_repository == null)
                    return new ResponseEntity { Status = ResponseStatus.FAILURE, Data = new { message = "Database connection is not configured." } };

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectId", request.ProjectId)
                };

                // 1 result set: Task CASE fields for the project, including CostMethod
                var result = await _repository.GetAsyncSP<ProjectTaskCaseStructureModel>(
                    "usp_SEL_Whizible2_ProjectTaskCaseStructure", sqlParams);

                return new ResponseEntity { Status = ResponseStatus.SUCCESS, Data = new { message = "Project task case structure", data = result } };
            }
            catch (Exception ex)
            {
                return new ResponseEntity { Status = ResponseStatus.FAILURE, Data = new { message = ex.Message } };
            }
        }
        // End of Added by Aditya J. on 07-08-2026 for Project Task Case Structure API

        // Added by Aditya J. on 12-08-2026 for Customer Dropdown API
        public async Task<ResponseEntity> GetCustomerDropdown(CustomerDropdownRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new CustomerDropdownRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intCustomerID",
                        (object?)request.CustomerID ?? DBNull.Value)
                };

                // 1 result set -> Customer, CustomerName for the dropdown
                var result = await _repository.GetAsyncSP<CustomerDropdownModel>(
                    "usp_SEL_Whizible2_ProjectProfitByCustomer_CustomerDropdown", sqlParams);

                return Success("Customer dropdown", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 12-08-2026 for Customer Dropdown API

        // Added by Aditya J. on 12-08-2026 for Project Dropdown API
        public async Task<ResponseEntity> GetProjectDropdown(ProjectDropdownRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectDropdownRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectTypeId",
                        (object?)request.ProjectTypeId ?? DBNull.Value),
                    new SqlParameter("@intBusinessgroupID",
                        (object?)request.BusinessgroupID ?? DBNull.Value),
                    new SqlParameter("@intOraganizationUnitID",
                        (object?)request.OraganizationUnitID ?? DBNull.Value),
                    new SqlParameter("@intProjectGroupId",
                        (object?)request.ProjectGroupID ?? DBNull.Value),
                    new SqlParameter("@intEmployeeID",
                        (object?)request.EmployeeID ?? DBNull.Value),
                    new SqlParameter("@strProjectIDs",
                        (object?)NullIfBlankIntCsv(request.ProjectIDs) ?? DBNull.Value),
                    new SqlParameter("@strLoginType",
                        "E"),
                    new SqlParameter("@intCustomerID",
                        (object?)(request.CustomerID) ?? DBNull.Value)
                };

                // 1 result set -> ProjectID, ProjectName for the dropdown
                var result = await _repository.GetAsyncSP<ProjectDropdownModel>(
                    "usp_SEL_Whizible2_ProjectProfitByCustomer_ProjectDropdown", sqlParams);

                return Success("Project dropdown", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 12-08-2026 for Project Dropdown API

        // Added By Vyankat B. on 24th Aug 2026 for the Project Profit Filter Masters API
        public async Task<ResponseEntity> GetFilterMasters(ProjectProfitFilterMastersRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectProfitFilterMastersRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@LoginID",
                        (object?)request.LoginID ?? DBNull.Value),
                    new SqlParameter("@intCustomerID",
                        request.CustomerID.HasValue && request.CustomerID.Value != 0
                            ? (object)request.CustomerID.Value
                            : DBNull.Value)
                };

                // 5 result sets: BG → OU → Project → Customer → Period
                var result = await _repository.GetAsyncSP<
                        ProjectProfitFilterBusinessGroupModel,
                        ProjectProfitFilterOrganizationUnitModel,
                        ProjectProfitFilterProjectModel,
                        ProjectProfitFilterCustomerModel,
                        ProjectProfitFilterPeriodModel>(
                    "usp_Whizible2_Sel_ProjectProfit_FilterMasters", sqlParams);

                return Success("Project profit filter masters", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added By Vyankat B. on 24th Aug 2026 for the Project Profit Filter Masters API

        // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer Access Filter API
        public async Task<ResponseEntity> GetRoleLevelAccessFilter(ProjectProfitByCustomerAccessFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectProfitByCustomerAccessFilterRequest();

                var sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@strAccessParameter", request.AccessParameter),
            new SqlParameter("@intUserID", request.UserID),
            new SqlParameter("@strLoginType", request.LoginType),
            new SqlParameter("@intRoleLevel", request.RoleLevel),
            new SqlParameter("@blnShowReleasedProjects", request.ShowReleasedProjects),
            new SqlParameter("@intLoginID", (object?)request.LoginID ?? DBNull.Value)
        };

                var result = await _repository.GetAsyncSP<ProjectProfitByCustomerAccessFilterModel>(
                    "usp_QRB_Whizible2_ProjectProfitByCustomer_AccessFilter", sqlParams);

                return Success("Project profitability by customer access filter", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer Access Filter API
    }
}

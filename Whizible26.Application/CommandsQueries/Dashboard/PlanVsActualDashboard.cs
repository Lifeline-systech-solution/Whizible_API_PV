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
    public class PlanVsActualDashboard
    {
        private readonly IConfiguration _configuration;
        private readonly PlanVsActualRepo _repository;
        private readonly string _connectionString;

        // Added by Vikas T on 28-07-2026 - Default constructor required by the repository pattern
        public PlanVsActualDashboard() { }
        // End of default constructor

        // Added by Vikas T on 28-07-2026 - Initialise the dashboard service with configuration
        public PlanVsActualDashboard(IConfiguration configuration)
        {
            _configuration = configuration;
            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(
                    _configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);
                _repository = new PlanVsActualRepo(_connectionString);
            }
        }
        // End of Added by Vikas T on 28-07-2026

        #region Helpers

        // Added by <Name> on 28-07-2026 - Guard against an unconfigured repository
        // Deviation from the reference code: ProjectProfitability leaves _repository null
        // when configuration is missing and throws NullReferenceException at first use,
        // which surfaces as a confusing 400. See KNOWN_ISSUES #16.
        private ResponseEntity RepositoryNotConfigured()
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message = "Database connection is not configured." }
            };
        }
        // End of RepositoryNotConfigured

        // Added by Vikas T on 28-07-2026 - Build the shared filter parameters once
        private static List<SqlParameter> BuildFilterParameters(PlanVsActualFilterRequest request)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@BusinessGroupIDs",
                    (object?)NullIfBlankIntCsv(request.BusinessGroupIDs) ?? DBNull.Value),
                new SqlParameter("@OrganizationUnitIDs",
                    (object?)NullIfBlankIntCsv(request.OrganizationUnitIDs) ?? DBNull.Value),
                new SqlParameter("@ProjectIDs",
                    (object?)NullIfBlankIntCsv(request.ProjectIDs) ?? DBNull.Value),
                new SqlParameter("@FinancialYears",
                    (object?)NullIfBlankIntCsv(request.FinancialYears) ?? DBNull.Value),
                new SqlParameter("@MonthKeys",
                    (object?)NullIfBlankIntCsv(request.MonthKeys) ?? DBNull.Value),
                new SqlParameter("@TimesheetStatusFlags",
                    (object?)NullIfBlank(request.TimesheetStatusFlags) ?? DBNull.Value),
                new SqlParameter("@IncludeOvertime", request.IncludeOvertime),
                new SqlParameter("@PlanSpreadMethod",
                    request.PlanSpreadMethod == 2 ? (byte)2 : (byte)1)
            };
        }
        // End of BuildFilterParameters

        // Added by Vikas T on 28-07-2026 - Empty / Swagger "string" → no filter
        private static string? NullIfBlank(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            if (trimmed.Equals("string", StringComparison.OrdinalIgnoreCase)) return null;
            return trimmed;
        }
        // End of NullIfBlank

        // CSV of ints; no valid int → NULL (no filter)
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
        // End of NullIfBlankIntCsv

        // Added by Vishal.M on 12-08-2026 - Parse session IDs for InsUpd SP (INT params)
        private static bool TryParsePositiveInt(string? value, out int id)
        {
            id = 0;
            var trimmed = NullIfBlank(value);
            if (trimmed == null) return false;
            return int.TryParse(trimmed, out id) && id > 0;
        }
        // End of TryParsePositiveInt

        // Added by Vishal.M on 12-08-2026 - First mapped row from GetAsyncSP ExpandoObject
        private static T? FirstRowOf<T>(dynamic? result) where T : class
        {
            if (result == null) return null;
            if (result is IDictionary<string, object> bag)
            {
                foreach (var kv in bag)
                {
                    if (kv.Value is IEnumerable<T> list)
                        return list.FirstOrDefault();
                }
            }
            return null;
        }
        // End of FirstRowOf

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

        private static ResponseEntity Failure(string message)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message }
            };
        }

        #endregion

        // Added by Vikas T on 28-07-2026 - Populate the five cascading filter dropdowns
        public async Task<ResponseEntity> GetFilterMasters(PlanVsActualFilterMastersRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PlanVsActualFilterMastersRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@LoginID", (object?)request.LoginID ?? DBNull.Value),
                    new SqlParameter("@TimesheetStatusFlags",
                        (object?)NullIfBlank(request.TimesheetStatusFlags) ?? DBNull.Value),
                    new SqlParameter("@IncludeOvertime", request.IncludeOvertime),
                    new SqlParameter("@PlanSpreadMethod",
                        request.PlanSpreadMethod == 2 ? (byte)2 : (byte)1)
                };

                // 5 result sets -> 5 generic arguments
                var result = await _repository.GetAsyncSP<
                        BusinessGroupMasterModel,
                        OrganizationUnitMasterModel,
                        ProjectMasterModel,
                        FinancialYearMasterModel,
                        MonthMasterModel>(
                    "usp_Whizible2_Sel_PlanVsActual_FilterMasters", sqlParams);

                return Success("Plan vs Actual filter masters", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetFilterMasters

        // Added by Vikas T on 28-07-2026 - KPI cards for the Plan vs Actual dashboard
        public async Task<ResponseEntity> GetKPISummary(PlanVsActualFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PlanVsActualFilterRequest();

                var sqlParams = BuildFilterParameters(request);

                // 1 result set, always exactly 1 row
                var result = await _repository.GetAsyncSP<PlanVsActualKpiModel>(
                    "usp_Whizible2_Sel_PlanVsActual_KPISummary", sqlParams);

                return Success("Plan vs Actual KPI summary", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetKPISummary

        // Added by Vikas T on 28-07-2026 - The three overview bar charts in one call
        public async Task<ResponseEntity> GetSummaryByLevel(PlanVsActualFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PlanVsActualFilterRequest();

                var sqlParams = BuildFilterParameters(request);

                // 3 result sets -> 3 generic arguments
                var result = await _repository.GetAsyncSP<
                        PlanVsActualBusinessGroupSummaryModel,
                        PlanVsActualOrganizationUnitSummaryModel,
                        PlanVsActualProjectSummaryModel>(
                    "usp_Whizible2_Sel_PlanVsActual_SummaryByLevel", sqlParams);

                return Success("Plan vs Actual summary by level", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetSummaryByLevel

        // Added by Vikas T on 28-07-2026 - Trend line chart and variance heatmap (shared grid)
        public async Task<ResponseEntity> GetTrendData(PlanVsActualTrendRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PlanVsActualTrendRequest();

                if (request.LevelFlag < 1 || request.LevelFlag > 3)
                {
                    return Failure("LevelFlag must be 1 (Business Group), "
                                 + "2 (Organization Unit) or 3 (Project).");
                }

                var sqlParams = BuildFilterParameters(request);
                sqlParams.Add(new SqlParameter("@LevelFlag", request.LevelFlag));

                var trendProjectId = request.TrendProjectID.GetValueOrDefault() > 0
                    ? (object)request.TrendProjectID!.Value
                    : DBNull.Value;

                sqlParams.Add(new SqlParameter("@TrendProjectID", trendProjectId));
                sqlParams.Add(new SqlParameter("@PageNumber", request.PageNumber));
                sqlParams.Add(new SqlParameter("@PageSize", request.PageSize));

                var result = await _repository.GetAsyncSP<PlanVsActualTrendModel>(
                    "usp_Whizible2_Sel_PlanVsActual_TrendData", sqlParams);

                return Success("Plan vs Actual trend data", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetTrendData

        // Added by Vikas T on 28-07-2026 - Analysis table, re-grouped by LevelFlag 1..5
        public async Task<ResponseEntity> GetAnalysisTable(PlanVsActualAnalysisRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PlanVsActualAnalysisRequest();

                if (request.LevelFlag < 1 || request.LevelFlag > 5)
                {
                    return Failure("LevelFlag must be between 1 and 5.");
                }

                var sqlParams = BuildFilterParameters(request);
                sqlParams.Add(new SqlParameter("@LevelFlag", request.LevelFlag));
                sqlParams.Add(new SqlParameter("@SortColumn",
                    (object?)NullIfBlank(request.SortColumn) ?? "PlannedHours"));
                sqlParams.Add(new SqlParameter("@SortDirection",
                    (object?)NullIfBlank(request.SortDirection) ?? "DESC"));
                sqlParams.Add(new SqlParameter("@PageNumber", request.PageNumber));
                sqlParams.Add(new SqlParameter("@PageSize", request.PageSize));

                var result = await _repository.GetAsyncSP<
                        PlanVsActualAnalysisRowModel,
                        PlanVsActualAnalysisTotalModel>(
                    "usp_Whizible2_Sel_PlanVsActual_AnalysisTable", sqlParams);

                return Success("Plan vs Actual analysis", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalysisTable

        // Added by Vishal.M on 12-08-2026 - Persist default filters from Plan_VS_Actual Save button
        /// <summary>
        /// Upserts the page's default filter JSON (WhereClause) for the logged-in employee.
        /// SP: usp_InsUpd_Whizible2_PlanVsActual_DefaultFilter
        /// Params: @SessionEmployeeID, @SessionProjectID, @WhereClause
        /// Returns: FilterID, Result ('1' insert / '2' update), WhereClause
        /// </summary>
        public async Task<ResponseEntity> SaveDefaultFilter(PlanVsActualDefaultFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PlanVsActualDefaultFilterRequest();

                // SP requires @SessionEmployeeID INT (one default filter per employee)
                if (!TryParsePositiveInt(request.SessionEmployeeID, out var sessionEmployeeId))
                {
                    return Failure("SessionEmployeeID is required and must be a valid integer.");
                }

                // UI sends WhereClause and FilterJson as the same JSON string; accept either
                var whereClause = NullIfBlank(request.WhereClause);
                if (whereClause == null)
                {
                    return Failure("WhereClause (filter JSON) is required.");
                }

                // @SessionProjectID INT = NULL when blank / 0
                object sessionProjectIdParam = DBNull.Value;
                if (TryParsePositiveInt(request.DashboardID, out var sessionProjectId))
                    sessionProjectIdParam = sessionProjectId;

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@SessionEmployeeID", sessionEmployeeId),
                    new SqlParameter("@DashboardID", request.DashboardID),
                    new SqlParameter("@WhereClause", whereClause)
                };

                // SP SELECTs FilterID, Result, WhereClause — read via GetAsyncSP
                var result = await _repository.GetAsyncSP<PlanVsActualDefaultFilterSaveResultModel>(
                    "usp_Whizible2_InsUpd_Dashboard_DefaultFilter", sqlParams);

                var row = FirstRowOf<PlanVsActualDefaultFilterSaveResultModel>(result);

                // Echo SP columns so UI / callers get FilterID, Result, WhereClause
                return Success("Default filter saved", new
                {
                    FilterID = row?.FilterID ?? 0,
                    Result = row?.Result ?? string.Empty,
                    WhereClause = NullIfBlank(row?.WhereClause) ?? whereClause,
                    //FilterJson = NullIfBlank(row?.WhereClause) ?? whereClause
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of SaveDefaultFilter

        // Added by Vishal.M on 12-08-2026 - Load saved default filters on Plan_VS_Actual page open
        /// <summary>
        /// Returns the saved WhereClause JSON so the UI can re-apply dropdown selections on load.
        /// SP: usp_Whizible2_Sel_tbl_Whizible2_PlanVsActual_DefaultFilter
        /// Params: @SessionEmployeeID
        /// Returns same Success data shape as Save: FilterID, WhereClause (also FilterJson).
        /// </summary>
        public async Task<ResponseEntity> GetDefaultFilter(PlanVsActualDefaultFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PlanVsActualDefaultFilterRequest();

                // Same employee key as InsUpd SP (one default filter per employee)
                if (!TryParsePositiveInt(request.SessionEmployeeID, out var sessionEmployeeId))
                {
                    return Failure("SessionEmployeeID is required and must be a valid integer.");
                }

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@SessionEmployeeID", sessionEmployeeId),
                    new SqlParameter("@DashboardID", request.DashboardID)
                };

                var result = await _repository.GetAsyncSP<PlanVsActualDefaultFilterModel>(
                    "usp_Whizible2_Sel_PlanVsActual_DefaultFilter", sqlParams);

                var row = FirstRowOf<PlanVsActualDefaultFilterModel>(result);
                var whereClause = NullIfBlank(row?.WhereClause)
                               ?? NullIfBlank(row?.FilterJson);

                // Same Success(...) wrapper as SaveDefaultFilter for extractDefaultFilterResponseData()
                return Success("Plan vs Actual default filter", new
                {
                    //FilterID = row?.FilterID ?? 0,
                    Result = whereClause != null ? "1" : "0",
                    WhereClause = whereClause,
                    FilterJson = whereClause,
                    SessionEmployeeID = row?.EmployeeID ?? sessionEmployeeId,
                    //SessionProjectID = row?.SessionProjectID
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetDefaultFilter
    }
}

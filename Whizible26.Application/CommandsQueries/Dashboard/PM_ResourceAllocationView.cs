using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Whizible26.Application.Repository.DashboardRepo;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.Application.CommandsQueries.Dashboard
{
    // Added by Vyankat B. on 11-08-2026 for the Resource Allocation View
    // Added By Vyankat B. on 25th Aug 2026
    // Kept only methods used by ResourceAllocationView.aspx. Unused APIs removed.
    // End of Added By Vyankat B. on 25th Aug 2026
    public class PM_ResourceAllocationView
    {
        private readonly IConfiguration _configuration;
        private readonly PM_ResourceAllocationViewRepo _repository;
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PM_ResourceAllocationView() { }

        public PM_ResourceAllocationView(
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment = null)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;

            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(
                    _configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);

                _repository = new PM_ResourceAllocationViewRepo(_connectionString);
            }
        }

        private ResponseEntity RepositoryNotConfigured()
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message = "Database connection is not configured." }
            };
        }

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

        #region Helpers

        private static object NullIfZero(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim() == "0")
                return DBNull.Value;
            return value.Trim();
        }

        private static object NullIfEmpty(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DBNull.Value;
            return value.Trim();
        }

        private static object ToDbDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DBNull.Value;

            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out var dt))
                return dt;

            if (DateTime.TryParse(value, out dt))
                return dt;

            return DBNull.Value;
        }

        private static object ToDbIntOrNull(string? value)
        {
            var v = NullIfZero(value);
            if (v == DBNull.Value) return DBNull.Value;
            if (int.TryParse(v.ToString(), out int i)) return i;
            return DBNull.Value;
        }

        private static object ToDbFloatOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim() == "0")
                return DBNull.Value;
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                return d;
            return DBNull.Value;
        }

        private static string? GetString(DataRow row, string col)
            => !row.Table.Columns.Contains(col) || row[col] == DBNull.Value
                ? null
                : Convert.ToString(row[col]);

        private static int GetInt(DataRow row, string col)
            => !row.Table.Columns.Contains(col) || row[col] == DBNull.Value
                ? 0
                : Convert.ToInt32(row[col]);

        private static int? GetNullableInt(DataRow row, string col)
            => !row.Table.Columns.Contains(col) || row[col] == DBNull.Value
                ? null
                : Convert.ToInt32(row[col]);

        private static decimal GetDecimal(DataRow row, string col)
            => !row.Table.Columns.Contains(col) || row[col] == DBNull.Value
                ? 0m
                : Convert.ToDecimal(row[col]);

        private static double GetDouble(DataRow row, string col)
            => !row.Table.Columns.Contains(col) || row[col] == DBNull.Value
                ? 0d
                : Convert.ToDouble(row[col]);

        private static DateTime? GetNullableDate(DataRow row, string col)
            => !row.Table.Columns.Contains(col) || row[col] == DBNull.Value
                ? null
                : Convert.ToDateTime(row[col]);

        private static bool? GetNullableBool(DataRow row, string col)
        {
            if (!row.Table.Columns.Contains(col) || row[col] == DBNull.Value) return null;
            var val = row[col];
            if (val is bool b) return b;
            if (val is byte bt) return bt != 0;
            if (val is short s) return s != 0;
            if (val is int i) return i != 0;
            return Convert.ToBoolean(val);
        }

        private static int ToInt(string? value)
        {
            int.TryParse(value, out int i);
            return i;
        }

        private static List<Dictionary<string, object?>> ToDictionaryList(DataTable? table)
        {
            var list = new List<Dictionary<string, object?>>();
            if (table == null) return list;

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn col in table.Columns)
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                list.Add(dict);
            }
            return list;
        }

        private async Task<ResponseEntity> ExecuteSelect(
            string storedProcedure,
            List<SqlParameter> sqlParams,
            string successMessage)
        {
            if (_repository == null) return RepositoryNotConfigured();
            var ds = await _repository.GetDataSetAsync(storedProcedure, sqlParams);
            var data = ToDictionaryList(ds?.Tables.Count > 0 ? ds.Tables[0] : null);
            return Success(successMessage, data);
        }

        private static List<SqlParameter> BuildProjectEmployeeParams(ProjectEmployeeIdsRequest request)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@intProjectID", ToInt(request.ProjectID)),
                new SqlParameter("@intEmployeeID", ToInt(request.EmployeeID))
            };
        }

        #endregion

        // Added by Vyankat B. on 13-08-2026 - usp_Whizible2_Sel_GetAll_Dropdown
        public async Task<ResponseEntity> GetAllDropdown(GetAllDropdownRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new GetAllDropdownRequest();

                if (string.IsNullOrWhiteSpace(request.FieldName))
                    return Failure(new Exception("FieldName is required."));

                string? json = null;
                if (request.InputParameterJson != null)
                {
                    json = JsonSerializer.Serialize(
                        request.InputParameterJson,
                        new JsonSerializerOptions
                        {
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        });
                }

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@FieldName", request.FieldName.Trim()),
                    new SqlParameter("@InputParameterJson", (object?)json ?? DBNull.Value)
                };

                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_GetAll_Dropdown", sqlParams);

                var list = new List<GetAllDropdownResponse>();
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new GetAllDropdownResponse
                        {
                            ID = GetString(row, "ID"),
                            FieldName = GetString(row, "FieldName")
                        });
                    }
                }

                return Success("Dropdown list retrieved successfully", list);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_FinancialType
        public async Task<ResponseEntity> GetFinancialType()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_FinancialType", new List<SqlParameter>());

                var list = new List<FinancialTypeResponse>();
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new FinancialTypeResponse
                        {
                            Code = GetString(row, "Code"),
                            FinancialType = GetString(row, "FinancialType"),
                            OrderBy = GetInt(row, "OrderBy")
                        });
                    }
                }

                return Success("Financial types retrieved successfully", list);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_FromAndToDates_ForReasAllocation
        public async Task<ResponseEntity> GetFromAndToDates(GetFromAndToDatesRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new GetFromAndToDatesRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@FinancialPeriod",
                        string.IsNullOrWhiteSpace(request.FinancialPeriod)
                            ? "M"
                            : request.FinancialPeriod.Trim()),
                    new SqlParameter("@Period", request.Period),
                    new SqlParameter("@SpecificDate", ToDbDate(request.SpecificDate))
                };

                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_FromAndToDates_ForReasAllocation", sqlParams);

                var data = new FromAndToDatesResponse();
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null && table.Rows.Count > 0)
                {
                    var row = table.Rows[0];
                    data.FromDate = GetNullableDate(row, "FromDate");
                    data.ToDate = GetNullableDate(row, "ToDate");
                    data.DistinctDays = GetInt(row, "DistinctDays");
                }

                return Success("From and To dates retrieved successfully", data);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_ResourceAllocation_Dashboard
        public async Task<ResponseEntity> GetResourceAllocationDashboard(
            GetResourceAllocationDashboardRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new GetResourceAllocationDashboardRequest();

                int.TryParse(request.UserID, out int userId);
                int.TryParse(request.LoginId, out int loginId);

                var fromDate = ToDbDate(request.FromDate);
                var toDate = ToDbDate(request.ToDate);
                if (fromDate == DBNull.Value || toDate == DBNull.Value)
                    return Failure(new Exception("FromDate and ToDate are required."));

                var pageNo = request.PageNo.GetValueOrDefault(1);
                if (pageNo < 1) pageNo = 1;
                var pageSize = request.PageSize.GetValueOrDefault(10);
                if (pageSize < 1) pageSize = 10;

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@LoginType",
                        string.IsNullOrWhiteSpace(request.LoginType)
                            ? "E"
                            : request.LoginType.Trim()),
                    new SqlParameter("@UserName", request.UserName ?? string.Empty),
                    new SqlParameter("@LoginId", loginId),
                    new SqlParameter("@FinancialPeriod",
                        string.IsNullOrWhiteSpace(request.FinancialPeriod)
                            ? "M"
                            : request.FinancialPeriod.Trim()),
                    new SqlParameter("@FromDate", fromDate),
                    new SqlParameter("@ToDate", toDate),
                    new SqlParameter("@businessGroupID", ToDbIntOrNull(request.BusinessGroupID)),
                    new SqlParameter("@LocationID", ToDbIntOrNull(request.LocationID)),
                    new SqlParameter("@RoleId", ToDbIntOrNull(request.RoleId)),
                    new SqlParameter("@GradeId", ToDbIntOrNull(request.GradeId)),
                    new SqlParameter("@EmployeeName", NullIfEmpty(request.EmployeeName)),
                    new SqlParameter("@Deployable", NullIfEmpty(request.Deployable)),
                    new SqlParameter("@AllocationPercentage", ToDbFloatOrNull(request.AllocationPercentage)),
                    new SqlParameter("@PageNo", pageNo),
                    new SqlParameter("@PageSize", pageSize)
                };

                var dashboardList = await _repository.GetAsyncSP<
                    ResourceAllocationDashboardResponse,
                    ResourceAllocationPaginationEntity>(
                    "usp_Whizible2_Sel_ResourceAllocation_Dashboard",
                    sqlParams);

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Resource allocation dashboard retrieved successfully",
                        data = dashboardList
                    }
                };
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added by Vyankat B. on 13-08-2026 - usp_SEL_ResourceAllocation_Summary
        public async Task<ResponseEntity> GetResourceAllocationSummary(
            GetResourceAllocationSummaryRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new GetResourceAllocationSummaryRequest();

                int.TryParse(request.UserID, out int userId);
                int.TryParse(request.LoginId, out int loginId);

                var fromDate = ToDbDate(request.FromDate);
                var toDate = ToDbDate(request.ToDate);

                var aggregation = string.IsNullOrWhiteSpace(request.AggregationMethod)
                    ? "AVG"
                    : request.AggregationMethod.Trim().ToUpperInvariant();
                if (aggregation != "AVG" && aggregation != "MAX" && aggregation != "SUM")
                    aggregation = "AVG";

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@LoginType",
                        string.IsNullOrWhiteSpace(request.LoginType)
                            ? "E"
                            : request.LoginType.Trim()),
                    new SqlParameter("@UserName", request.UserName ?? string.Empty),
                    new SqlParameter("@LoginId", loginId),
                    new SqlParameter("@FinancialPeriod",
                        string.IsNullOrWhiteSpace(request.FinancialPeriod)
                            ? "M"
                            : request.FinancialPeriod.Trim()),
                    new SqlParameter("@FromDate", fromDate),
                    new SqlParameter("@ToDate", toDate),
                    new SqlParameter("@businessGroupID", ToDbIntOrNull(request.BusinessGroupID)),
                    new SqlParameter("@LocationID", ToDbIntOrNull(request.LocationID)),
                    new SqlParameter("@RoleId", ToDbIntOrNull(request.RoleId)),
                    new SqlParameter("@GradeId", ToDbIntOrNull(request.GradeId)),
                    new SqlParameter("@EmployeeName", NullIfEmpty(request.EmployeeName)),
                    new SqlParameter("@Deployable", NullIfEmpty(request.Deployable)),
                    new SqlParameter("@AllocationPercentage", ToDbFloatOrNull(request.AllocationPercentage)),
                    new SqlParameter("@AggregationMethod", aggregation)
                };

                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_SEL_ResourceAllocation_Summary", sqlParams);

                var data = new ResourceAllocationSummaryResponse();
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null && table.Rows.Count > 0)
                {
                    var row = table.Rows[0];
                    data.TotalEmployees = GetInt(row, "TotalEmployees");
                    data.FullyAllocated = GetInt(row, "FullyAllocated");
                    data.FullyAllocatedPct = GetDecimal(row, "FullyAllocatedPct");
                    data.PartiallyAllocated = GetInt(row, "PartiallyAllocated");
                    data.PartiallyAllocatedPct = GetDecimal(row, "PartiallyAllocatedPct");
                    data.Unallocated = GetInt(row, "Unallocated");
                    data.UnallocatedPct = GetDecimal(row, "UnallocatedPct");
                }

                return Success("Resource allocation summary retrieved successfully", data);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_Res_tbl_PM_EmployeeName
        public async Task<ResponseEntity> GetEmployeeName(GetEmployeeNameRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new GetEmployeeNameRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intEmployeeID", ToDbIntOrNull(request.EmployeeID))
                };

                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_Res_tbl_PM_EmployeeName", sqlParams);

                var list = new List<EmployeeNameResponse>();
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new EmployeeNameResponse
                        {
                            EmployeeName = GetString(row, "EmployeeName")
                        });
                    }
                }

                return Success("Employee name(s) retrieved successfully", list);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_Res_tbl_PM_ProjectEmployeeRole
        public async Task<ResponseEntity> GetProjectEmployeeRole(GetProjectEmployeeRoleRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new GetProjectEmployeeRoleRequest();

                int.TryParse(request.ProjectEmployeeRoleID, out int perId);

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectEmployeeRoleID", perId)
                };

                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_Res_tbl_PM_ProjectEmployeeRole", sqlParams);

                var list = new List<ProjectEmployeeRoleResponse>();
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new ProjectEmployeeRoleResponse
                        {
                            ActualEndDate = GetNullableDate(row, "ActualEndDate"),
                            ActualHours = GetDecimal(row, "ActualHours"),
                            ActualStartDate = GetNullableDate(row, "ActualStartDate"),
                            BilledHours = GetDecimal(row, "BilledHours"),
                            BillingPercentage = GetDecimal(row, "BillingPercentage"),
                            BillingType = GetString(row, "BillingType"),
                            BudgetedHours = GetDecimal(row, "BudgetedHours"),
                            Cost = GetDecimal(row, "Cost"),
                            CostWithCurrency = GetString(row, "CostWithCurrency"),
                            EmployeeID = GetInt(row, "EmployeeID"),
                            EmployeeName = GetString(row, "EmployeeName"),
                            ExpectedEndDate = GetNullableDate(row, "ExpectedEndDate"),
                            ExpectedStartDate = GetNullableDate(row, "ExpectedStartDate"),
                            IsApprover = GetInt(row, "IsApprover"),
                            IsDefaultApprover = GetNullableBool(row, "IsDefaultApprover"),
                            IsExpenseApprover = GetInt(row, "IsExpenseApprover"),
                            IsResourceActive = GetNullableBool(row, "IsResourceActive"),
                            IsResourceBillable = GetNullableBool(row, "IsResourceBillable"),
                            Location = GetString(row, "Location"),
                            MonthlyFee = GetDecimal(row, "MonthlyFee"),
                            PlannedCostInProjectCurrency = GetString(row, "PlannedCostInProjectCurrency"),
                            PlannedCostInResourceCurrency = GetString(row, "PlannedCostInResourceCurrency"),
                            ProjectEmployeeRoleId = GetInt(row, "ProjectEmployeeRoleId"),
                            ProjectID = GetInt(row, "ProjectID"),
                            Rate = GetDecimal(row, "Rate"),
                            ReportingTo = GetInt(row, "ReportingTo"),
                            ReportingToName = GetString(row, "ReportingToName"),
                            ResourcePercentage = GetDecimal(row, "ResourcePercentage"),
                            ResourceStatus = GetString(row, "ResourceStatus"),
                            Responsibility = GetString(row, "Responsibility"),
                            Role = GetInt(row, "Role"),
                            RoleDescription = GetString(row, "RoleDescription"),
                            Used = GetInt(row, "Used"),
                            UserName = GetString(row, "UserName")
                        });
                    }
                }

                return Success("Project employee role retrieved successfully", list);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_ResourceAllocation_Dashboard_ResourceDetails
        public async Task<ResponseEntity> GetResourceAllocationDetails(
            GetResourceAllocationDetailsRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new GetResourceAllocationDetailsRequest();

                int.TryParse(request.UserID, out int userId);
                int.TryParse(request.LoginId, out int loginId);

                var fromDate = ToDbDate(request.FromDate);
                var toDate = ToDbDate(request.ToDate);
                if (fromDate == DBNull.Value || toDate == DBNull.Value)
                    return Failure(new Exception("FromDate and ToDate are required."));

                var pageNo = request.PageNo.GetValueOrDefault(1);
                if (pageNo < 1) pageNo = 1;
                var pageSize = request.PageSize.GetValueOrDefault(5);
                if (pageSize < 1) pageSize = 5;

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@LoginType",
                        string.IsNullOrWhiteSpace(request.LoginType)
                            ? "E"
                            : request.LoginType.Trim()),
                    new SqlParameter("@UserName", request.UserName ?? string.Empty),
                    new SqlParameter("@LoginId", loginId),
                    new SqlParameter("@FinancialPeriod",
                        string.IsNullOrWhiteSpace(request.FinancialPeriod)
                            ? "M"
                            : request.FinancialPeriod.Trim()),
                    new SqlParameter("@FromDate", fromDate),
                    new SqlParameter("@ToDate", toDate),
                    new SqlParameter("@EmployeeID", ToDbIntOrNull(request.EmployeeID)),
                    new SqlParameter("@PageNo", pageNo),
                    new SqlParameter("@PageSize", pageSize)
                };

                var detailsList = await _repository.GetAsyncSP<
                    ResourceAllocationDetailsResponse,
                    ResourceAllocationPaginationEntity>(
                    "usp_Whizible2_Sel_ResourceAllocation_Dashboard_ResourceDetails",
                    sqlParams);

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Resource allocation details retrieved successfully",
                        data = detailsList
                    }
                };
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Upd_Res_tbl_PM_ProjectEmployeeRole
        public async Task<ResponseEntity> UpdateProjectEmployeeRole(
            UpdateProjectEmployeeRoleRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new UpdateProjectEmployeeRoleRequest();

                int.TryParse(request.ProjectEmployeeRoleID, out int perId);
                int.TryParse(request.EmployeeID, out int employeeId);
                int.TryParse(request.ProjectID, out int projectId);
                int.TryParse(request.RoleID, out int roleId);
                int.TryParse(request.ReportingTo, out int reportingTo);

                var startDate = ToDbDate(request.ExpectedStartDate);
                var endDate = ToDbDate(request.ExpectedEndDate);
                if (startDate == DBNull.Value || endDate == DBNull.Value)
                    return Failure(new Exception("ExpectedStartDate and ExpectedEndDate are required."));

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectEmployeeRoleID", perId),
                    new SqlParameter("@intEmployeeID", employeeId),
                    new SqlParameter("@intProjectID", projectId),
                    new SqlParameter("@Cost", request.Cost),
                    new SqlParameter("@Rate", request.Rate),
                    new SqlParameter("@intRoleID", roleId),
                    new SqlParameter("@ResourcePercentage", request.ResourcePercentage),
                    new SqlParameter("@ExpectedStartDate", startDate),
                    new SqlParameter("@ExpectedEndDate", endDate),
                    new SqlParameter("@ResourceStatus", request.ResourceStatus ?? string.Empty),
                    new SqlParameter("@IsResourceBillable", request.IsResourceBillable),
                    new SqlParameter("@Responsibility",
                        (object?)request.Responsibility ?? DBNull.Value),
                    new SqlParameter("@intReportingTo", reportingTo),
                    new SqlParameter("@IsDefaultApprover", request.IsDefaultApprover),
                    new SqlParameter("@ModifiedBy", request.ModifiedBy ?? string.Empty)
                };

                var rows = await _repository.UpdateAsyncSP(
                    "usp_Whizible2_Upd_Res_tbl_PM_ProjectEmployeeRole", sqlParams);

                if (rows < 0) rows = 1;

                return Success(
                    "Project employee role updated successfully",
                    new ResourceAllocationUpdateResponse { RowsAffected = rows });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added By Vyankat B. on 24th Aug 2026 - usp_Whizible2_Sel_tbl_PM_Res_RowWiseExternalApprovers
        public async Task<ResponseEntity> GetRowWiseExternalApprovers(RowWiseExternalApproversRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new RowWiseExternalApproversRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@ProjectID", ToInt(request.ProjectID)),
                    new SqlParameter("@blnPaging", request.Paging),
                    new SqlParameter("@strPaging",
                        string.IsNullOrWhiteSpace(request.StrPaging) ? "-1" : request.StrPaging.Trim()),
                    new SqlParameter("@SortBy",
                        string.IsNullOrWhiteSpace(request.SortBy) ? "RoleDescription" : request.SortBy.Trim()),
                    new SqlParameter("@SortOrder",
                        string.IsNullOrWhiteSpace(request.SortOrder) ? "DESC" : request.SortOrder.Trim())
                };

                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_tbl_PM_Res_RowWiseExternalApprovers", sqlParams);

                var list = new List<RowWiseExternalApproversResponse>();
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new RowWiseExternalApproversResponse
                        {
                            EmployeeID = GetNullableInt(row, "EmployeeID"),
                            EmployeeName = GetString(row, "EmployeeName"),
                            ResourceName = GetString(row, "Resource Name")
                                ?? GetString(row, "ResourceName"),
                            RoleDescription = GetString(row, "RoleDescription"),
                            IsExternal = GetString(row, "IsExternal"),
                            OrderNo = GetNullableInt(row, "OrderNo")
                        });
                    }
                }

                return Success("Reporting To list retrieved successfully", list);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        public Task<ResponseEntity> GetIsWorkflowApprover(IsWorkflowApproverRequest request)
        {
            request ??= new IsWorkflowApproverRequest();
            return ExecuteSelect(
                "usp_Whizible2_Sel_Res_IsWorkflowApprover",
                new List<SqlParameter>
                {
                    new SqlParameter("@intUserID", ToInt(request.UserID)),
                    new SqlParameter("@intProjectID", ToInt(request.ProjectID))
                },
                "Workflow approver check completed successfully");
        }

        public Task<ResponseEntity> GetMppTasks(ProjectEmployeeIdsRequest request)
        {
            request ??= new ProjectEmployeeIdsRequest();
            return ExecuteSelect(
                "usp_Whizible2_Sel_Res_MPPTask_tbl_PM_ProjectTasks",
                BuildProjectEmployeeParams(request),
                "MPP tasks retrieved successfully");
        }

        public Task<ResponseEntity> GetProjectTools(ProjectEmployeeIdsRequest request)
        {
            request ??= new ProjectEmployeeIdsRequest();
            return ExecuteSelect(
                "usp_Whizible2_Sel_Res_tbl_PM_ProjectTools",
                new List<SqlParameter>
                {
                    new SqlParameter("@ProjectID", ToInt(request.ProjectID)),
                    new SqlParameter("@EmployeeID", ToInt(request.EmployeeID))
                },
                "Project tools retrieved successfully");
        }

        public Task<ResponseEntity> GetTasksForCompletion(ProjectEmployeeIdsRequest request)
        {
            request ??= new ProjectEmployeeIdsRequest();
            return ExecuteSelect(
                "usp_Whizible2_Sel_Res_tbl_PM_TasksForCompletion_ReleaseResource",
                BuildProjectEmployeeParams(request),
                "Tasks for completion retrieved successfully");
        }

        public Task<ResponseEntity> GetTasksForVoiding(ProjectEmployeeIdsRequest request)
        {
            request ??= new ProjectEmployeeIdsRequest();
            return ExecuteSelect(
                "usp_Whizible2_Sel_Res_tbl_PM_TasksForVoiding_ReleaseResource",
                BuildProjectEmployeeParams(request),
                "Tasks for voiding retrieved successfully");
        }

        public Task<ResponseEntity> GetTimesheetAndExpenseApproveeList(ProjectEmployeeIdsRequest request)
        {
            request ??= new ProjectEmployeeIdsRequest();
            return ExecuteSelect(
                "usp_Whizible2_Sel_Res_TimesheetandExpenseApproveelist_ReleaseResource",
                BuildProjectEmployeeParams(request),
                "Timesheet and expense approvee list retrieved successfully");
        }

        // Added by Vyankat B. on 12-08-2026 - usp_Whizible2_Sel_ResourceAllocation_AuditTrail
        public async Task<ResponseEntity> GetResourceAllocationAuditTrail(
            GetResourceAllocationAuditTrailRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new GetResourceAllocationAuditTrailRequest();

                var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
                var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@PageNumber", pageNumber),
                    new SqlParameter("@PageSize", pageSize),
                    new SqlParameter("@ProjectEmployeeRoleID",
                        ToDbIntOrNull(request.ProjectEmployeeRoleID)),
                    new SqlParameter("@ModifiedBy", NullIfEmpty(request.ModifiedBy)),
                    new SqlParameter("@ModifiedFieldName", NullIfEmpty(request.ModifiedFieldName))
                };

                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_ResourceAllocation_AuditTrail", sqlParams);

                var result = new ResourceAllocationAuditTrailPagedResponse
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                };

                var recordsTable = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (recordsTable != null)
                {
                    foreach (DataRow row in recordsTable.Rows)
                    {
                        result.Records.Add(new ResourceAllocationAuditTrailResponse
                        {
                            AuditID = GetInt(row, "AuditID"),
                            ProjectEmployeeRoleID = GetInt(row, "ProjectEmployeeRoleID"),
                            ModifiedFieldName = GetString(row, "ModifiedFieldName"),
                            OldValue = GetString(row, "OldValue"),
                            NewValue = GetString(row, "NewValue"),
                            ModifiedDate = GetNullableDate(row, "ModifiedDate"),
                            ModifiedBy = GetString(row, "ModifiedBy")
                        });
                    }
                }

                var pagingTable = ds?.Tables.Count > 1 ? ds.Tables[1] : null;
                if (pagingTable != null && pagingTable.Rows.Count > 0)
                {
                    var pagingRow = pagingTable.Rows[0];
                    result.TotalRecords = GetInt(pagingRow, "TotalRecords");
                    result.TotalPages = GetDouble(pagingRow, "TotalPages");
                    result.CurrentPage = GetInt(pagingRow, "CurrentPage");
                    result.PageSize = GetInt(pagingRow, "PageSize");
                }

                return Success("Resource allocation audit trail retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added by Vyankat B. on 12-08-2026 - usp_Whizible2_Sel_ResourceAllocationAudit_ModifiedField
        public async Task<ResponseEntity> GetResourceAllocationAuditModifiedField(
            GetResourceAllocationAuditLookupRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new GetResourceAllocationAuditLookupRequest();

                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_ResourceAllocationAudit_ModifiedField",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@ProjectEmployeeRoleID",
                            ToDbIntOrNull(request.ProjectEmployeeRoleID))
                    });

                var list = new List<ResourceAllocationAuditModifiedFieldResponse>();
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new ResourceAllocationAuditModifiedFieldResponse
                        {
                            ModifiedFieldName = GetString(row, "ModifiedFieldName")
                        });
                    }
                }

                return Success("Audit modified fields retrieved successfully", list);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        // Added by Vyankat B. on 12-08-2026 - usp_Whizible2_Sel_ResourceAllocationAudit_ModifiedBy
        public async Task<ResponseEntity> GetResourceAllocationAuditModifiedBy(
            GetResourceAllocationAuditLookupRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                request ??= new GetResourceAllocationAuditLookupRequest();

                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_ResourceAllocationAudit_ModifiedBy",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@ProjectEmployeeRoleID",
                            ToDbIntOrNull(request.ProjectEmployeeRoleID))
                    });

                var list = new List<ResourceAllocationAuditModifiedByResponse>();
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new ResourceAllocationAuditModifiedByResponse
                        {
                            ModifiedBy = GetString(row, "ModifiedBy")
                        });
                    }
                }

                return Success("Audit modified by list retrieved successfully", list);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
    }
}

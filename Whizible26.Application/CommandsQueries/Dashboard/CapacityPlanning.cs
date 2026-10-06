using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Whizible26.Application.Repository.DashboardRepo;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleTeams.Application.Repository;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.Application.CommandsQueries.RM
{
    public class CapacityPlanning
    {
        private readonly IConfiguration _configuration;
        private readonly RM_CapacityPlanningRepo _RM_CapacityPlanningRepo;

        public CapacityPlanning(IConfiguration configuration)
        {
            _configuration = configuration;

            if (_configuration != null)
            {
                _RM_CapacityPlanningRepo = new RM_CapacityPlanningRepo(
                    CommonFunctions.General.BuildConnectionString(
                        _configuration
                            .GetSection("ConnectionStrings:WhizibleDbConnection")
                            .Value));
            }
        }

        public async Task<ResponseEntity> GetCapacityPlanHeaderCounts(
            CapacityPlanning_Params parameter)
        {
            try
            {
                if (parameter == null || parameter.UserID <= 0)
                {
                    return Failure("Valid User ID is required");
                }

                string periodType = string.IsNullOrWhiteSpace(parameter.PeriodType)
                    ? "MONTH"
                    : parameter.PeriodType.Trim().ToUpperInvariant();

                if (periodType != "QUARTER")
                    periodType = "MONTH";

                var result =
                    await _RM_CapacityPlanningRepo.GetAsyncSP<RM_CapacityPlanning_Header>(
                        "usp_Whizible2_Sel_CapacityPlanHeader",
                        new List<SqlParameter>
                        {
                            new SqlParameter("@UserID", parameter.UserID),
                            new SqlParameter("@PeriodType", periodType)
                        });

                var headerData =
                    ((dynamic)result).RM_CapacityPlanning_Header
                    as List<RM_CapacityPlanning_Header>;

                return Success(new
                {
                    message = "Capacity Plan Header",
                    data = headerData?.FirstOrDefault()
                });
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        public async Task<ResponseEntity> GetCapacityPlanBGFilterList(
            CapacityPlanning_Params parameter)
        {
            try
            {
                if (parameter == null || parameter.UserID <= 0)
                {
                    return Failure("Valid User ID is required");
                }

                var result =
                    await _RM_CapacityPlanningRepo.GetAsyncSP<CapacityPlanBGDto>(
                        "usp_Whizible2_Sel_BusinessGroups_LevelWise_Dropdown",
                        new List<SqlParameter>
                        {
                            new SqlParameter("@intUserID", parameter.UserID)
                        });

                var bgList =
                    ((dynamic)result).CapacityPlanBGDto as List<CapacityPlanBGDto>;

                return Success(bgList);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        public async Task<ResponseEntity> GetCapacityPlanOUFilterList(
            CapacityPlanning_Params parameter)
        {
            try
            {
                if (parameter == null || parameter.UserID <= 0)
                {
                    return Failure("Valid User ID is required");
                }

                var result =
                    await _RM_CapacityPlanningRepo.GetAsyncSP<CapacityPlanOUDto>(
                        "usp_Whizible2_sel_OrganizationUnits_LevelWise_Dropdown",
                        new List<SqlParameter>
                        {
                            new SqlParameter("@intBGID", DBNull.Value),
                            new SqlParameter("@intUserID", parameter.UserID)
                        });

                var data =
                    ((dynamic)result).CapacityPlanOUDto as List<CapacityPlanOUDto>;

                return Success(data);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        public async Task<ResponseEntity> GetCapacityPlanSkillFilterList(
            CapacityPlanning_Params parameter)
        {
            try
            {
                var result =
                    await _RM_CapacityPlanningRepo.GetAsyncSP<RM_CapacityPlanning_Skill>(
                        "usp_Whizible2_Sel_CapacityPlanning_SkillList",
                        new List<SqlParameter>());

                var skillData =
                    ((dynamic)result).RM_CapacityPlanning_Skill
                    as List<RM_CapacityPlanning_Skill>;

                return Success(new
                {
                    message = "Skill Filter List",
                    data = skillData
                });
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        public async Task<ResponseEntity> GetCapacityPlanRoleFilterList(
            CapacityPlanning_Params parameter)
        {
            try
            {
                var result =
                    await _RM_CapacityPlanningRepo.GetAsyncSP<RM_CapacityPlanning_Role>(
                        "usp_Whizible2_Sel_CapacityPlanning_RoleList",
                        new List<SqlParameter>());

                var roleData =
                    ((dynamic)result).RM_CapacityPlanning_Role
                    as List<RM_CapacityPlanning_Role>;

                return Success(new
                {
                    message = "Role Filter List",
                    data = roleData
                });
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        public async Task<ResponseEntity> GetCP_RoleWise_MonthHeader(Cp_Params parameter)
        {
            return await ExecuteGridSP(
                "usp_Whizible2_Sel_Cp_RoleWise_MonthHeader",
                BuildMonthHeaderParams(parameter, includeRoleList: true),
                "Role Wise Month Headers",
                includeWeekHeaders: true,
                includePagination: true);
        }

        public async Task<ResponseEntity> GetCP_RoleWise_MonthDetails(Cp_Params parameter)
        {
            try
            {
                var result = await _RM_CapacityPlanningRepo.GetCpDynamicAsyncSP(
                    "usp_Whizible2_Sel_Cp_RoleWise_MonthDetail",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                        new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                        new SqlParameter("@SkillList", parameter.SkillList ?? ""),
                        new SqlParameter("@strCurrentDate", ToSqlDate(parameter.CurrentDate)),
                        new SqlParameter("@RoleID", parameter.RoleID)
                    });

                return Success(BuildCpPayload(
                    result,
                    "Role Wise Month Details",
                    includeWeekHeaders: true,
                    includePagination: false,
                    mergeMonthHeadersIntoPagination: false));
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        public async Task<ResponseEntity> GetCP_SkillWise_MonthHeader(Cp_Params parameter)
        {
            return await ExecuteGridSP(
                "usp_Whizible2_Sel_CP_SkillWise_MonthHeader",
                BuildMonthHeaderParams(parameter, includeRoleList: false),
                "Skill Wise Month Headers",
                includeWeekHeaders: true,
                includePagination: true);
        }

        public async Task<ResponseEntity> GetCP_SkillWise_MonthDetails(Cp_Params parameter)
        {
            try
            {
                var result = await _RM_CapacityPlanningRepo.GetCpDynamicAsyncSP(
                    "usp_Whizible2_Sel_CP_SkillWise_MonthDetail",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                        new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                        new SqlParameter("@SkillList", parameter.SkillList ?? ""),
                        new SqlParameter("@strCurrentDate", ToSqlDate(parameter.CurrentDate)),
                        new SqlParameter("@ToolID", parameter.SkillID)
                    });

                return Success(BuildCpPayload(
                    result,
                    "Skill Wise Month Details",
                    includeWeekHeaders: true,
                    includePagination: false,
                    mergeMonthHeadersIntoPagination: false));
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        public async Task<ResponseEntity> GetCP_RoleWise_QtrHeader(Cp_Params parameter)
        {
            int pageNumber = NormalizePage(parameter.PageNumber);
            int pageSize = NormalizePageSize(parameter.PageSize);

            return await ExecuteGridSP(
                "usp_Whizible2_Sel_CP_RoleWise_QtrHeader",
                new List<SqlParameter>
                {
                    new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                    new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                    new SqlParameter("@SkillList", parameter.SkillList ?? ""),
                    new SqlParameter("@RoleList", parameter.RoleList ?? ""),
                    new SqlParameter("@PageNumber", pageNumber),
                    new SqlParameter("@PageSize", pageSize)
                },
                "Role Wise Quarter Headers",
                includeWeekHeaders: false,
                includePagination: true,
                mergeMonthHeadersIntoPagination: true);
        }

        public async Task<ResponseEntity> GetCP_RoleWise_QtrDetails(Cp_Params parameter)
        {
            try
            {
                var result = await _RM_CapacityPlanningRepo.GetCpDynamicAsyncSP(
                    "usp_Whizible2_Sel_CP_RoleWise_QtrDetails",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                        new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                        new SqlParameter("@SkillList", parameter.SkillList ?? ""),
                        new SqlParameter("@intRoleID", parameter.RoleID)
                    });

                return Success(BuildCpPayload(
                    result,
                    "Role Wise Quarter Details",
                    includeWeekHeaders: false,
                    includePagination: false,
                    mergeMonthHeadersIntoPagination: true));
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        public async Task<ResponseEntity> GettotalstrengthbyRoleorskill(CP_Details parameter)
        {
            return await ExecutePagedSP(
                "usp_Whizible2_GetTotalStrengthBySkillOrRoleID",
                parameter,
                "Total Strength Details");
        }

        public async Task<ResponseEntity> GetProjectAllocationDetails(CP_Details parameter)
        {
            return await ExecutePagedSP(
                "usp_Whizible2_GetProjectAllocationBySkillOrRoleID",
                parameter,
                "Project Allocation Details");
        }

        public async Task<ResponseEntity> GetProjectRequestByRole(CP_Details parameter)
        {
            return await ExecutePagedSP(
                "usp_Whizible2_GetProjectRequestsBySkillorRoleID",
                parameter,
                "Project Resources Details");
        }

        public async Task<ResponseEntity> GetApportunityRequestByRole(CP_Details parameter)
        {
            return await ExecutePagedSP(
                "usp_Whizible2_GetOpportunityRequestBySkillorRole",
                parameter,
                "Opportunity Request Details");
        }

        public async Task<ResponseEntity> GetBenchByRole(CP_Details parameter)
        {
            return await ExecuteBenchPagedSP(
                "usp_Whizible2_Sel_BenchByRollorSkill",
                parameter,
                "Bench Details");
        }

        public async Task<ResponseEntity> GetAnticipatedExitByRole(CP_Details parameter)
        {
            return await ExecutePagedSP(
                "usp_Whizible2_GetAnticipatedExitsBySkillOrRoleID",
                parameter,
                "Anticipated Exit Details");
        }

        public async Task<ResponseEntity> GetJoiningPoolByRole(CP_Details parameter)
        {
            return await ExecuteJoiningPoolPagedSP(
                "usp_Whizible2_GetJoiningPoolBySkillorRoleID",
                parameter,
                "Joining Pool Details");
        }

        public async Task<ResponseEntity> GetCP_SW_QuarterHeader(Cp_Params parameter)
        {
            int pageNumber = NormalizePage(parameter.PageNumber);
            int pageSize = NormalizePageSize(parameter.PageSize);

            return await ExecuteGridSP(
                "usp_Whizible2_Sel_CP_SkillWise_QtrHeader",
                new List<SqlParameter>
                {
                    new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                    new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                    new SqlParameter("@SkillList", parameter.SkillList ?? ""),
                    new SqlParameter("@PageNumber", pageNumber),
                    new SqlParameter("@PageSize", pageSize)
                },
                "Skill Wise Quarter Headers",
                includeWeekHeaders: false,
                includePagination: true,
                mergeMonthHeadersIntoPagination: true);
        }

        public async Task<ResponseEntity> GetCP_Sw_QuarterDetails(Cp_Params parameter)
        {
            try
            {
                var result = await _RM_CapacityPlanningRepo.GetCpDynamicAsyncSP(
                    "usp_Whizible2_Sel_CP_SkillWise_QtrDetails",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                        new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                        new SqlParameter("@SkillList", parameter.SkillList ?? ""),
                        new SqlParameter("@intToolID", parameter.SkillID)
                    });

                return Success(BuildCpPayload(
                    result,
                    "Skill Wise Quarter Details",
                    includeWeekHeaders: false,
                    includePagination: false,
                    mergeMonthHeadersIntoPagination: true));
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        public async Task<ResponseEntity> GetRoleWiseTrendAnalysis(
            CP_RoleorSkilWiseTrendAnalysisRequest parameter)
        {
            return await ExecuteTrendSP(
                "usp_Whizible2_Sel_CP_RoleWiseTrendAnalysis",
                parameter,
                "Role Wise Trend Analysis");
        }

        public async Task<ResponseEntity> GetSkillWiseTrendAnalysis(
            CP_RoleorSkilWiseTrendAnalysisRequest parameter)
        {
            return await ExecuteTrendSP(
                "usp_Whizible2_Sel_CP_SkillWiseTrendAnalysis",
                parameter,
                "Skill Wise Trend Analysis");
        }

        public async Task<ResponseEntity> GetCpExportFile(CP_ExportRequest parameter)
        {
            try
            {
                var dt = await GetCpExportData(parameter);
                // Keep legacy Report SP column layout (RoleDescription, Name, Week1..);
                // apply selected Role/Skill name filter on RoleDescription / Description.
                dt = ApplyCpExportNameFilter(dt, parameter?.RoleNames, parameter?.RoleList, parameter?.ReportTab);

                string format = parameter.ReportFormat?.ToUpperInvariant() ?? "EXCEL";

                byte[] fileBytes;
                string fileName;

                if (format == "PDF")
                {
                    var (logoBytes, logoExtension) = await GetCompanyLogoBytes();
                    fileBytes = GeneratePdf(dt, logoBytes, logoExtension);
                    fileName = $"CP_Export_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                }
                else
                {
                    fileBytes = GenerateExcel(dt);
                    fileName = $"CP_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                }

                return Success(new
                {
                    FileName = fileName,
                    FileBytes = fileBytes
                });
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        public async Task<CompanyLogoModel> GetCompanyLogoFileName()
        {
            try
            {
                var result =
                    await _RM_CapacityPlanningRepo.GetAsyncSP<CompanyLogoModel>(
                        "usp_Whizible2_Sel_Logo_tbl_PM_CompanyInformation",
                        new List<SqlParameter>());

                if (result?.CompanyLogoModel != null && result.CompanyLogoModel.Count > 0)
                    return result.CompanyLogoModel[0];

                return new CompanyLogoModel();
            }
            catch
            {
                return new CompanyLogoModel();
            }
        }

        private async Task<ResponseEntity> ExecuteGridSP(
            string storedProcedure,
            List<SqlParameter> sqlParameters,
            string message,
            bool includeWeekHeaders,
            bool includePagination,
            bool mergeMonthHeadersIntoPagination = false)
        {
            try
            {
                var result = await _RM_CapacityPlanningRepo.GetCpDynamicAsyncSP(
                    storedProcedure,
                    sqlParameters);

                return Success(BuildCpPayload(
                    result,
                    message,
                    includeWeekHeaders,
                    includePagination,
                    mergeMonthHeadersIntoPagination));
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        private async Task<ResponseEntity> ExecutePagedSP(
            string storedProcedure,
            CP_Details parameter,
            string message)
        {
            int pageNumber = NormalizePage(parameter.PageNumber);
            int pageSize = NormalizePageSize(parameter.PageSize);

            try
            {
                var result = await _RM_CapacityPlanningRepo.GetCpDynamicAsyncSP(
                    storedProcedure,
                    new List<SqlParameter>
                    {
                        new SqlParameter("@intRoleID", parameter.RoleID ?? (object)DBNull.Value),
                        new SqlParameter("@intSkillID", parameter.SkillID ?? (object)DBNull.Value),
                        new SqlParameter("@UserID", parameter.UserID ?? (object)DBNull.Value),
                        new SqlParameter("@dtStart", parameter.StartDate ?? (object)DBNull.Value),
                        new SqlParameter("@dtEnd", parameter.EndDate ?? (object)DBNull.Value),
                        new SqlParameter("@RoleOrSkill", parameter.RoleOrSkill),
                        new SqlParameter("@WeekOrMonth", parameter.WeekOrMonth),
                        new SqlParameter("@SkillList", parameter.SkillList ?? ""),
                        new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                        new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                        new SqlParameter("@PageNumber", pageNumber),
                        new SqlParameter("@PageSize", pageSize)
                    });

                return Success(BuildCpPayload(
                    result,
                    message,
                    includeWeekHeaders: false,
                    includePagination: true,
                    mergeMonthHeadersIntoPagination: false));
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        private async Task<ResponseEntity> ExecuteJoiningPoolPagedSP(
            string storedProcedure,
            CP_Details parameter,
            string message)
        {
            int pageNumber = NormalizePage(parameter.PageNumber);
            int pageSize = NormalizePageSize(parameter.PageSize);

            try
            {
                var result = await _RM_CapacityPlanningRepo.GetCpDynamicAsyncSP(
                    storedProcedure,
                    new List<SqlParameter>
                    {
                        new SqlParameter("@intRoleID", parameter.RoleID ?? (object)DBNull.Value),
                        new SqlParameter("@intSkillID", parameter.SkillID ?? (object)DBNull.Value),
                        new SqlParameter("@UserID", parameter.UserID ?? (object)DBNull.Value),
                        new SqlParameter("@dtStart", parameter.StartDate ?? (object)DBNull.Value),
                        new SqlParameter("@dtEnd", parameter.EndDate ?? (object)DBNull.Value),
                        new SqlParameter("@RoleOrSkill", parameter.RoleOrSkill),
                        new SqlParameter("@WeekOrMonth", parameter.WeekOrMonth),
                        new SqlParameter("@PageNumber", pageNumber),
                        new SqlParameter("@PageSize", pageSize),
                        new SqlParameter("@DEBUG", false)
                    });

                return Success(BuildCpPayload(
                    result,
                    message,
                    includeWeekHeaders: false,
                    includePagination: true,
                    mergeMonthHeadersIntoPagination: false));
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        private async Task<ResponseEntity> ExecuteBenchPagedSP(
            string storedProcedure,
            CP_Details parameter,
            string message)
        {
            int pageNumber = NormalizePage(parameter.PageNumber);
            int pageSize = NormalizePageSize(parameter.PageSize);

            try
            {
                var result = await _RM_CapacityPlanningRepo.GetCpDynamicAsyncSP(
                    storedProcedure,
                    new List<SqlParameter>
                    {
                        new SqlParameter("@RoleID", parameter.RoleID ?? (object)DBNull.Value),
                        new SqlParameter("@SkillID", parameter.SkillID ?? (object)DBNull.Value),
                        new SqlParameter("@intUserID", parameter.UserID ?? (object)DBNull.Value),
                        new SqlParameter("@strDate", parameter.StartDate ?? (object)DBNull.Value),
                        new SqlParameter("@endDate", parameter.EndDate ?? (object)DBNull.Value),
                        new SqlParameter("@RoleOrSkill", parameter.RoleOrSkill),
                        new SqlParameter("@SkillList", parameter.SkillList ?? ""),
                        new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                        new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                        new SqlParameter("@WeekOrMonth", parameter.WeekOrMonth),
                        new SqlParameter("@PageNumber", pageNumber),
                        new SqlParameter("@PageSize", pageSize)
                    });

                return Success(BuildCpPayload(
                    result,
                    message,
                    includeWeekHeaders: false,
                    includePagination: true,
                    mergeMonthHeadersIntoPagination: false));
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        private async Task<ResponseEntity> ExecuteTrendSP(
            string storedProcedure,
            CP_RoleorSkilWiseTrendAnalysisRequest parameter,
            string message)
        {
            try
            {
                var result = await _RM_CapacityPlanningRepo.GetCpDynamicAsyncSP(
                    storedProcedure,
                    new List<SqlParameter>
                    {
                        new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                        new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                        new SqlParameter("@SkillList", parameter.SkillList ?? ""),
                        new SqlParameter("@ViewType", parameter.ViewType ?? "UTILIZATION")
                    });

                return Success(BuildCpPayload(
                    result,
                    message,
                    includeWeekHeaders: false,
                    includePagination: false,
                    mergeMonthHeadersIntoPagination: false));
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
        }

        private List<SqlParameter> BuildMonthHeaderParams(Cp_Params parameter, bool includeRoleList)
        {
            int pageNumber = NormalizePage(parameter.PageNumber);
            int pageSize = NormalizePageSize(parameter.PageSize);

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                new SqlParameter("@SkillList", parameter.SkillList ?? "")
            };

            if (includeRoleList)
                sqlParameters.Add(new SqlParameter("@RoleList", parameter.RoleList ?? ""));

            sqlParameters.Add(new SqlParameter("@strCurrentDate", ToSqlDate(parameter.CurrentDate)));
            sqlParameters.Add(new SqlParameter("@PageNumber", pageNumber));
            sqlParameters.Add(new SqlParameter("@PageSize", pageSize));

            return sqlParameters;
        }

        private static object BuildCpPayload(
            dynamic result,
            string message,
            bool includeWeekHeaders,
            bool includePagination,
            bool mergeMonthHeadersIntoPagination)
        {
            var dictionary = result as IDictionary<string, object>;
            if (dictionary == null)
            {
                return new
                {
                    message,
                    data = (object)null,
                    WeekHeaders = Array.Empty<object>(),
                    MonthHeaders = Array.Empty<object>(),
                    PaginationEntities = Array.Empty<object>()
                };
            }

            object data = GetDictValue(dictionary, "Data");
            object weekHeaders = GetDictValue(dictionary, "WeekHeaders") ?? Array.Empty<object>();
            object monthHeaders = GetDictValue(dictionary, "MonthHeaders") ?? Array.Empty<object>();
            object pagination = GetDictValue(dictionary, "Pagination") ?? Array.Empty<object>();

            object paginationEntities = pagination;
            if (mergeMonthHeadersIntoPagination)
            {
                paginationEntities = ConcatLists(monthHeaders, pagination);
            }
            else if (!includePagination)
            {
                paginationEntities = Array.Empty<object>();
            }

            return new
            {
                message,
                data,
                WeekHeaders = includeWeekHeaders ? weekHeaders : Array.Empty<object>(),
                MonthHeaders = monthHeaders,
                PaginationEntities = paginationEntities
            };
        }

        private static object ConcatLists(object first, object second)
        {
            var list = new List<object>();

            if (first is IEnumerable firstEnum)
            {
                foreach (var item in firstEnum)
                    list.Add(item);
            }

            if (second is IEnumerable secondEnum)
            {
                foreach (var item in secondEnum)
                    list.Add(item);
            }

            return list;
        }

        private static object GetDictValue(IDictionary<string, object> dictionary, string key)
        {
            if (dictionary == null || string.IsNullOrWhiteSpace(key))
                return null;

            if (dictionary.TryGetValue(key, out var value))
                return value;

            var match = dictionary.Keys.FirstOrDefault(
                k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

            return match != null ? dictionary[match] : null;
        }

        private static object ToSqlDate(DateTime value)
        {
            return value == default ? (object)DBNull.Value : value;
        }

        private static int NormalizePage(int pageNumber)
        {
            return pageNumber <= 0 ? 1 : pageNumber;
        }

        private static int NormalizePageSize(int pageSize)
        {
            return pageSize <= 0 ? 10 : pageSize;
        }

        private static ResponseEntity Success(object data)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.SUCCESS,
                Data = data
            };
        }

        private static ResponseEntity Failure(object data)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = data
            };
        }

        private async Task<DataTable> GetCpExportData(CP_ExportRequest parameter)
        {
            // Same Report SPs / same columns as old usp_Whizible2_Sel_CpExportReport.
            // RoleList is applied in ApplyCpExportRoleFilter after fetch.
            string reportTab = parameter.ReportTab ?? "MonthWiseRole";
            DateTime currentDate = parameter.CurrentDate.HasValue && parameter.CurrentDate.Value != default
                ? parameter.CurrentDate.Value
                : DateTime.Now;

            return await _RM_CapacityPlanningRepo.GetDataTableFromStoredProcedureAsync(
                "usp_Whizible2_Sel_CpExportReport_New",
                new List<SqlParameter>
                {
                    new SqlParameter("@BGOUType", parameter.BGOUType ?? ""),
                    new SqlParameter("@BGOUFilter", parameter.BGOUFilter ?? ""),
                    new SqlParameter("@SkillList", parameter.SkillList ?? ""),
                    new SqlParameter("@RoleList", parameter.RoleList ?? ""),
                    new SqlParameter("@ReportTab", reportTab),
                    new SqlParameter("@strCurrentDate", ToSqlDate(currentDate)),
                    new SqlParameter("@DEBUG", false)
                });
        }

        /// <summary>
        /// Legacy *_View_Report returns RoleDescription + Name + Week1..WeekN (no RoleID).
        /// Filter rows by selected RoleDescription / Description / Skill name.
        /// </summary>
        private static DataTable ApplyCpExportNameFilter(
            DataTable source,
            string roleNames,
            string roleList,
            string reportTab)
        {
            if (source == null || source.Rows.Count == 0)
                return source ?? new DataTable();

            var selectedNames = ParseExportNameList(roleNames);
            if (selectedNames.Count == 0)
                return source;

            DataColumn? nameCol = FindExportRoleOrSkillNameColumn(source, reportTab);
            if (nameCol == null)
            {
                // Fallback: old path if RoleID column ever present
                return ApplyCpExportRoleFilter(source, roleList, reportTab);
            }

            var filtered = source.Clone();
            foreach (DataRow row in source.Rows)
            {
                if (row[nameCol] == null || row[nameCol] == DBNull.Value)
                    continue;

                string rowName = Convert.ToString(row[nameCol])?.Trim() ?? "";
                if (rowName.Length == 0)
                    continue;

                // Skip org Summary when a specific Role/Skill filter is applied
                if (rowName.Equals("Summary", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (MatchesAnySelectedName(rowName, selectedNames))
                    filtered.ImportRow(row);
            }

            return filtered;
        }

        private static List<string> ParseExportNameList(string roleNames)
        {
            var names = new List<string>();
            if (string.IsNullOrWhiteSpace(roleNames))
                return names;

            foreach (string part in roleNames.Split(new[] { '\n', '\r', '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string name = NormalizeExportName(part);
                if (name.Length > 0)
                    names.Add(name);
            }

            return names;
        }

        private static bool MatchesAnySelectedName(string rowName, List<string> selectedNames)
        {
            if (selectedNames == null || selectedNames.Count == 0)
                return false;

            string row = NormalizeExportName(rowName);
            foreach (string selected in selectedNames)
            {
                if (string.IsNullOrWhiteSpace(selected)) continue;
                if (row.Equals(selected, StringComparison.OrdinalIgnoreCase)) return true;
                if (row.IndexOf(selected, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (selected.IndexOf(row, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private static string NormalizeExportName(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            string value = text.Trim();
            value = System.Text.RegularExpressions.Regex.Replace(value, @"\s{2,}", " ");
            return value;
        }

        private static DataColumn? FindExportRoleOrSkillNameColumn(DataTable table, string reportTab)
        {
            bool isSkill = !string.IsNullOrWhiteSpace(reportTab)
                && reportTab.IndexOf("Skill", StringComparison.OrdinalIgnoreCase) >= 0;

            // Header SP shape: RoleName / SkillName (same export layout for Role + Skill)
            string[] preferred = isSkill
                ? new[] { "SkillName", "Description", "SkillDescription", "RoleDescription", "RoleName" }
                : new[] { "RoleName", "RoleDescription", "Role Description", "Description" };

            foreach (string name in preferred)
            {
                if (table.Columns.Contains(name))
                    return table.Columns[name];
            }

            foreach (DataColumn col in table.Columns)
            {
                string n = (col.ColumnName ?? "").Replace(" ", "").Replace("_", "");
                if (!isSkill && (n.Equals("RoleName", StringComparison.OrdinalIgnoreCase)
                    || n.Equals("RoleDescription", StringComparison.OrdinalIgnoreCase)))
                    return col;
                if (isSkill && (n.Equals("SkillName", StringComparison.OrdinalIgnoreCase)
                    || n.Equals("Description", StringComparison.OrdinalIgnoreCase)
                    || n.Equals("SkillDescription", StringComparison.OrdinalIgnoreCase)))
                    return col;
            }

            return null;
        }

        /// <summary>
        /// Legacy Role *_View_Report SPs have no @RoleList. Keep their result shape,
        /// then keep only rows for selected RoleIDs from UI RoleList:
        /// e.g. (Role.RoleID IN (12,15)).
        /// </summary>
        private static DataTable ApplyCpExportRoleFilter(DataTable source, string roleList, string reportTab)
        {
            if (source == null || source.Rows.Count == 0)
                return source ?? new DataTable();

            if (string.IsNullOrWhiteSpace(roleList))
                return source;

            var roleIds = ParseIdsFromSqlInClause(roleList);
            if (roleIds.Count == 0)
                return source;

            DataColumn? roleIdCol = FindExportRoleIdColumn(source);
            if (roleIdCol == null)
                return source;

            var filtered = source.Clone();
            foreach (DataRow row in source.Rows)
            {
                if (row[roleIdCol] == null || row[roleIdCol] == DBNull.Value)
                    continue;

                if (!int.TryParse(Convert.ToString(row[roleIdCol]), out int rowRoleId))
                    continue;

                if (roleIds.Contains(rowRoleId))
                    filtered.ImportRow(row);
            }

            return filtered;
        }

        private static HashSet<int> ParseIdsFromSqlInClause(string clause)
        {
            var ids = new HashSet<int>();
            if (string.IsNullOrWhiteSpace(clause))
                return ids;

            int open = clause.LastIndexOf('(');
            int close = clause.LastIndexOf(')');
            if (open < 0 || close <= open)
                return ids;

            string inner = clause.Substring(open + 1, close - open - 1);
            foreach (string part in inner.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part.Trim(), out int id))
                    ids.Add(id);
            }

            return ids;
        }

        private static DataColumn? FindExportRoleIdColumn(DataTable table)
        {
            string[] preferred =
            {
                "RoleID", "RoleId", "Role_ID", "roleid", "ROLEID", "Role ID"
            };

            foreach (string name in preferred)
            {
                if (table.Columns.Contains(name))
                    return table.Columns[name];
            }

            foreach (DataColumn col in table.Columns)
            {
                string n = (col.ColumnName ?? "").Replace(" ", "").Replace("_", "");
                if (n.Equals("RoleID", StringComparison.OrdinalIgnoreCase))
                    return col;
            }

            return null;
        }

        private static string GetCpExportDisplayHeader(string rawName)
        {
            string name = rawName ?? string.Empty;
            if (name.Length == 0) return string.Empty;

            name = name.Trim().TrimStart('_');

            // Week metric columns: __TS__September Week 1 → Total Strength Sep W1
            var metricMatch = System.Text.RegularExpressions.Regex.Match(
                name,
                @"^(TS|AL|BN|RQ)__(.+)$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (metricMatch.Success)
            {
                return FormatCpExportWeekMetricHeader(
                    metricMatch.Groups[1].Value,
                    metricMatch.Groups[2].Value);
            }

            switch (name)
            {
                case "RoleDescription": return "Role Description";
                case "RoleName": return "Role Name";
                case "SkillName": return "Skill Name";
                case "ToolID": return "Skill ID";
                case "RoleID": return "Role ID";
                case "TotalStrength": return "Total Strength";
                case "Bench": return "Bench";
                case "Requests": return "Requests";
                case "Allocated": return "Allocated";
                case "CurrentBench": return "Current Bench";
                case "ProjectRequests": return "Project Requests";
                case "OpportunityRequests": return "Opportunity Requests";
                case "UtilizationPercentage":
                case "UtilizationPerc":
                case "UtilizationPercent":
                case "UtilizationPer": return "Utilization %";
                case "AllocatedtoProjects":
                case "AllocatedToProjects": return "Allocated";
            }

            var sb = new System.Text.StringBuilder(name.Length + 8);
            for (int i = 0; i < name.Length; i++)
            {
                char ch = name[i];
                if (ch == '_')
                {
                    sb.Append(' ');
                    continue;
                }

                bool split =
                    i > 0 &&
                    char.IsUpper(ch) &&
                    (char.IsLower(name[i - 1]) || char.IsDigit(name[i - 1]));

                if (split) sb.Append(' ');
                sb.Append(ch);
            }

            string header = sb.ToString().Trim();
            header = AbbreviateMonthWords(header);
            header = System.Text.RegularExpressions.Regex.Replace(
                header,
                @"\bWeek\s+(\d+)\b",
                "W$1",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            header = System.Text.RegularExpressions.Regex.Replace(header, @"\s{2,}", " ");
            return header;
        }

        private static string FormatCpExportWeekMetricHeader(string metricCode, string weekPart)
        {
            string metric = metricCode.ToUpperInvariant() switch
            {
                "TS" => "Total Strength",
                "AL" => "Allocated",
                "BN" => "Bench",
                "RQ" => "Requests",
                _ => metricCode
            };

            weekPart = weekPart.Replace('_', ' ').Trim();
            weekPart = AbbreviateMonthWords(weekPart);
            weekPart = System.Text.RegularExpressions.Regex.Replace(
                weekPart,
                @"\bWeek\s+(\d+)\b",
                "W$1",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            weekPart = System.Text.RegularExpressions.Regex.Replace(weekPart, @"\s{2,}", " ");
            return $"{metric} {weekPart}".Trim();
        }

        private static string AbbreviateMonthWords(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value ?? string.Empty;

            var monthMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "January", "Jan" },
                { "February", "Feb" },
                { "March", "Mar" },
                { "April", "Apr" },
                { "May", "May" },
                { "June", "Jun" },
                { "July", "Jul" },
                { "August", "Aug" },
                { "September", "Sep" },
                { "October", "Oct" },
                { "November", "Nov" },
                { "December", "Dec" }
            };

            string output = value;
            foreach (var kv in monthMap)
            {
                output = System.Text.RegularExpressions.Regex.Replace(
                    output,
                    $@"\b{kv.Key}\b",
                    kv.Value,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            return output;
        }

        private byte[] GenerateExcel(DataTable dt)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Export");

            for (int c = 0; c < dt.Columns.Count; c++)
            {
                ws.Cell(1, c + 1).Value = GetCpExportDisplayHeader(dt.Columns[c].ColumnName);
                ws.Cell(1, c + 1).Style.Font.Bold = true;
            }

            for (int r = 0; r < dt.Rows.Count; r++)
            {
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    ws.Cell(r + 2, c + 1).Value = dt.Rows[r][c]?.ToString();
                }
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private async Task<(byte[] logoBytes, string extension)> GetCompanyLogoBytes()
        {
            try
            {
                var logoInfo = await GetCompanyLogoFileName();
                string currentDir = Directory.GetCurrentDirectory();
                string webRoot = Path.Combine(currentDir, "wwwroot");
                string logoFolder = Path.Combine(webRoot, "Uploads", "Logo");

                if (logoInfo != null && !string.IsNullOrWhiteSpace(logoInfo.OriginalFileName))
                {
                    string logoPath = Path.Combine(
                        logoFolder,
                        Path.GetFileName(logoInfo.OriginalFileName));

                    if (File.Exists(logoPath))
                    {
                        return (File.ReadAllBytes(logoPath), Path.GetExtension(logoPath));
                    }
                }

                string fallbackLogo = Path.Combine(logoFolder, "no-photo.png");
                if (File.Exists(fallbackLogo))
                {
                    return (File.ReadAllBytes(fallbackLogo), ".png");
                }
            }
            catch
            {
            }

            return (Array.Empty<byte>(), ".png");
        }

        private byte[] GeneratePdf(DataTable dt, byte[] logoBytes, string logoExtension)
        {
            if (dt == null || dt.Rows.Count == 0)
                return Array.Empty<byte>();

            string tempLogoPath = string.Empty;

            try
            {
                var document = new MigraDoc.DocumentObjectModel.Document();
                document.Info.Title = "CP Export Report";
                document.Styles["Normal"].Font.Name = "Arial";
                document.Styles["Normal"].Font.Size = 6.5;

                var section = document.AddSection();
                section.PageSetup.PageFormat = PageFormat.A3;
                section.PageSetup.Orientation = Orientation.Landscape;
                section.PageSetup.LeftMargin = Unit.FromCentimeter(0.8);
                section.PageSetup.RightMargin = Unit.FromCentimeter(0.8);
                section.PageSetup.TopMargin = Unit.FromCentimeter(0.8);
                section.PageSetup.BottomMargin = Unit.FromCentimeter(0.8);

                const double usableWidthCm = 40.4;

                var headerTable = section.AddTable();
                headerTable.Borders.Visible = false;
                headerTable.AddColumn(Unit.FromCentimeter(4.5));
                headerTable.AddColumn(Unit.FromCentimeter(usableWidthCm - 4.5));

                var headerRow = headerTable.AddRow();
                headerRow.VerticalAlignment = VerticalAlignment.Center;

                if (logoBytes != null && logoBytes.Length > 0)
                {
                    string ext = string.IsNullOrWhiteSpace(logoExtension) ? ".png" : logoExtension;
                    if (!ext.StartsWith("."))
                        ext = "." + ext;

                    tempLogoPath = Path.Combine(
                        Path.GetTempPath(),
                        $"logo_{Guid.NewGuid()}{ext}");

                    File.WriteAllBytes(tempLogoPath, logoBytes);

                    var logoPara = headerRow.Cells[0].AddParagraph();
                    logoPara.Format.Alignment = ParagraphAlignment.Left;
                    var image = logoPara.AddImage(tempLogoPath);
                    image.Width = Unit.FromCentimeter(3.2);
                    image.LockAspectRatio = true;
                    headerRow.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                }
                else
                {
                    headerRow.Cells[0].AddParagraph("");
                }

                var title = headerRow.Cells[1].AddParagraph("CP Export Report");
                title.Format.Font.Size = 16;
                title.Format.Font.Bold = true;
                title.Format.Alignment = ParagraphAlignment.Center;

                section.AddParagraph();

                var table = section.AddTable();
                table.Borders.Width = 0.4;
                table.Rows.LeftIndent = Unit.Zero;

                int colCount = dt.Columns.Count;
                double[] weights = new double[colCount];
                double totalWeight = 0;

                for (int i = 0; i < colCount; i++)
                {
                    string col = (dt.Columns[i].ColumnName ?? "").ToUpperInvariant();
                    double weight;
                    if (col == "NAME")
                        weight = 3.2;
                    else if (col.Contains("ROLE"))
                        weight = 4.2;
                    else if (col.Contains("DATE"))
                        weight = 3.4;
                    else
                        weight = 1.7;

                    weights[i] = weight;
                    totalWeight += weight;
                }

                for (int i = 0; i < colCount; i++)
                {
                    table.AddColumn(
                        Unit.FromCentimeter(usableWidthCm * weights[i] / totalWeight));
                }

                var header = table.AddRow();
                header.HeadingFormat = true;
                header.Shading.Color = Colors.LightGray;
                header.Format.Font.Bold = true;
                header.Format.Font.Size = 6;

                for (int i = 0; i < colCount; i++)
                {
                    var p = header.Cells[i].AddParagraph(GetCpExportDisplayHeader(dt.Columns[i].ColumnName));
                    p.Format.SpaceBefore = 0;
                    p.Format.SpaceAfter = 0;
                    p.Format.Alignment = ParagraphAlignment.Center;
                    header.Cells[i].Format.Alignment = ParagraphAlignment.Center;
                    header.Cells[i].VerticalAlignment = VerticalAlignment.Center;
                    header.Cells[i].Format.Font.Size = 6;
                }

                foreach (DataRow dr in dt.Rows)
                {
                    var row = table.AddRow();
                    for (int i = 0; i < colCount; i++)
                    {
                        row.Cells[i].AddParagraph(
                            dr[i] == DBNull.Value ? "" : Convert.ToString(dr[i]));
                        row.Cells[i].Format.Font.Size = 6.5;
                        row.Cells[i].VerticalAlignment = VerticalAlignment.Center;
                        row.Cells[i].Format.Alignment =
                            i <= 1 ? ParagraphAlignment.Left : ParagraphAlignment.Center;
                    }
                }

                var renderer = new PdfDocumentRenderer(true)
                {
                    Document = document
                };
                renderer.RenderDocument();

                using var stream = new MemoryStream();
                renderer.PdfDocument.Save(stream);
                return stream.ToArray();
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(tempLogoPath) && File.Exists(tempLogoPath))
                    File.Delete(tempLogoPath);
            }
        }
    }
}

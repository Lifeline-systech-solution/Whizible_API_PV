using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whizible26.Application.Repository.DashboardRepo;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.Application.CommandsQueries.Dashboard
{
    public class ProjectHealthDashboard
    {
        private readonly IConfiguration _configuration;
        private readonly ProjectHealthRepo _repository;
        private readonly string _connectionString;

        public ProjectHealthDashboard() { }

        public ProjectHealthDashboard(IConfiguration configuration)
        {
            _configuration = configuration;
            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(
                    _configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);
                _repository = new ProjectHealthRepo(_connectionString);
            }
        }

        #region Helpers

        private static string GetMetricColor(int rangeId, double value, IEnumerable<PHSSqertRangeModel> ranges)
        {
            var r = ranges.FirstOrDefault(x => x.RangeID == rangeId);
            if (r == null) return "Green"; // Fallback if no range found

            if (value >= r.UpperLow && value <= r.UpperHigh) return "Red";
            if (value >= r.MiddleLow && value <= r.MiddleHigh) return "Yellow";
            return "Green";
        }
        private ResponseEntity RepositoryNotConfigured() => new ResponseEntity { Status = ResponseStatus.FAILURE, Data = new { message = "Database connection is not configured." } };
        private static string? NullIfBlank(string? value) { if (string.IsNullOrWhiteSpace(value)) return null; var trimmed = value.Trim(); return trimmed.Equals("string", StringComparison.OrdinalIgnoreCase) ? null : trimmed; }
        private static string NormalizeLoginType(string? loginType) => string.IsNullOrWhiteSpace(loginType) ? "E" : loginType.Trim();
        private static DateTime ReportingDateOrToday(DateTime? value) => value ?? DateTime.Today;
        private static ResponseEntity Success(string message, object data) => new ResponseEntity { Status = ResponseStatus.SUCCESS, Data = new { message, data } };
        private static ResponseEntity Failure(Exception ex) => new ResponseEntity { Status = ResponseStatus.FAILURE, Data = new { message = ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "") } };
        private static ResponseEntity Failure(string message) => new ResponseEntity { Status = ResponseStatus.FAILURE, Data = new { message } };

        private static List<T> Extract<T>(object dynamicResult)
        {
            var dict = dynamicResult as IDictionary<string, object>;
            if (dict != null && dict.TryGetValue(typeof(T).Name, out var value) && value is List<T> list)
            {
                return list;
            }
            return new List<T>();
        }

        private static bool IsMetricAtRisk(int rangeId, double value, IEnumerable<PHSSqertRangeModel> ranges)
        {
            var range = ranges.FirstOrDefault(r => r.RangeID == rangeId);
            return range != null && value >= range.UpperLow && value <= range.UpperHigh;
        }

        private static List<SqlParameter> BuildScopeParameters(PHSProjectScopeRequest request)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@intProgramID", (object?)request.ProgramID ?? DBNull.Value),
                
                // FIX 1: Convert 0 to NULL so global dashboard search works
                new SqlParameter("@intProjectId", request.ProjectID > 0 ? request.ProjectID : DBNull.Value),

                new SqlParameter("@intProjectCategoeyID", (object?)request.ProjectTypeID ?? DBNull.Value),
                new SqlParameter("@intBusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
                new SqlParameter("@intOrganizationUnitID", (object?)request.OrganizationUnitID ?? DBNull.Value),
                new SqlParameter("@inputDate", ReportingDateOrToday(request.ReportingEndDate)),
                new SqlParameter("@intEmployeeID", request.EmployeeID.GetValueOrDefault(0)),
                
                // FIX 2: Add LoginType so the SP can calculate Level 2 access natively
                new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType))
            };
        }

        #endregion

        #region My Filters (Save/Apply)

        public async Task<ResponseEntity> SaveFilter(PHSFilterSaveRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@TagID", request.TagID),
                    new SqlParameter("@ProjectID", request.ProjectID),
                    new SqlParameter("@EmployeeID", request.EmployeeID),
                    new SqlParameter("@FilterName", request.FilterName),
                    new SqlParameter("@LoginType", NormalizeLoginType(request.LoginType)),
                    new SqlParameter("@WhereClause", request.WhereClause),
                    new SqlParameter("@CreatedBy", request.CreatedBy),
                    new SqlParameter("@Flag", request.Flag),
                    new SqlParameter("@FilterID", request.FilterID.GetValueOrDefault(0))
                };

                var result = await _repository.GetScalarAsyncSP<int>("usp_Whizible2_Ins_tbl_Whizible2_Filter_Query_dashboard", sqlParams);
                return Success("Filter saved successfully", new { filterId = result });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetMyFilterList(PHSFilterListRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@ProjectID", request.ProjectID),
                    new SqlParameter("@TagID", request.TagID),
                    new SqlParameter("@LoginType", NormalizeLoginType(request.LoginType)),
                    new SqlParameter("@EmployeeID", request.EmployeeID)
                };

                object raw = await _repository.GetAsyncSP<PHSFilterListModel>("usp_Whizible2_sel_tbl_Whizible2_Filter_Query_dashboard", sqlParams);
                return Success("Filter list retrieved", Extract<PHSFilterListModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetFilterByID(PHSFilterByIdRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                var sqlParams = new List<SqlParameter> { new SqlParameter("@FilterID", request.FilterID) };

                object raw = await _repository.GetAsyncSP<PHSFilterListModel>("usp_sel_ByFilterID_tbl_Whizible2_Filter_Query_dashboard", sqlParams);
                return Success("Filter details retrieved", Extract<PHSFilterListModel>(raw).FirstOrDefault() ?? new PHSFilterListModel());
            }
            catch (Exception ex) { return Failure(ex); }
        }

        // FIX: Now accepts a string like "12 Aug 2026" and safely parses it
        private static DateTime ReportingDateOrToday(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return DateTime.Today;
            if (DateTime.TryParse(value, out var parsedDate)) return parsedDate;
            return DateTime.Today;
        }

        public async Task<ResponseEntity> GetReportingFrequency()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                object raw = await _repository.GetAsyncSP<PHSReportingFrequencyModel>("usp_Whizible2_SEL_Tbl_PM_companyInformation_ResourceTimeSheetFrequency_dashboard", new List<SqlParameter>());
                return Success("Project Health reporting frequency", Extract<PHSReportingFrequencyModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> SetDefaultFilter(PHSSetDefaultFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@strProjectID", request.ProjectID),
                    new SqlParameter("@LoginType", NormalizeLoginType(request.LoginType)),
                    new SqlParameter("@UserID", request.UserID),
                    new SqlParameter("@TagID", request.TagID),
                    new SqlParameter("@ChangeDefaultFilterID", request.ChangeDefaultFilterID),
                    new SqlParameter("@Flag", request.Flag)
                };

                await _repository.ExecuteSPAsync("usp_Whizible2_Upd_tbl_Whizible2_Filter_Query_SetDefaultFilter_dashboard", sqlParams);
                return Success("Default filter updated", new { });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> DeleteFilter(PHSFilterByIdRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                var sqlParams = new List<SqlParameter> { new SqlParameter("@FilterID", request.FilterID) };
                await _repository.ExecuteSPAsync("usp_Whizible2_del_tbl_Whizible2_Filter_Query_dashboard", sqlParams);
                return Success("Filter deleted", new { });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> CheckDefaultFilterSetOrNot(PHSDefaultFilterCheckRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@TagID", request.TagID),
                    new SqlParameter("@LoginType", NormalizeLoginType(request.LoginType)),
                    new SqlParameter("@UserID", request.UserID)
                };

                object raw = await _repository.GetAsyncSP<PHSDefaultFilterCheckModel>("usp_Whizible2_ProjectHealthSheet_GetDefaultFilter_dashboard", sqlParams);
                return Success("Default filter status retrieved", Extract<PHSDefaultFilterCheckModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> CheckFilterNameExists(PHSFilterExistsRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@vFilterID", request.FilterName),
                    new SqlParameter("@intTagID", request.TagID),
                    new SqlParameter("@intProjectID", request.ProjectID),
                    new SqlParameter("@intEmployeeID", request.EmployeeID)
                };

                var result = await _repository.GetScalarAsyncSP<int>("usp_Whizible2_chk_FilterName_Exists_dashboard", sqlParams);
                return Success("Filter existence verified", new { exists = result == 1 });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetPHSDefaultFilter(PHSDefaultFilterDetailsRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@strProjectID", request.ProjectID),
                    new SqlParameter("@TagID", request.TagID),
                    new SqlParameter("@LoginType", NormalizeLoginType(request.LoginType)),
                    
                    // FIX: Changed request.UserID to request.EmployeeID
                    new SqlParameter("@UserID", request.EmployeeID)
                };

                object raw = await _repository.GetAsyncSP<PHSDefaultFilterDetailsModel>(
                    "usp_Whizible2_Sel_tbl_Whizible2_Filter_Query_GetDefaultFilter_dashboard", sqlParams);

                return Success("Default filter details retrieved", Extract<PHSDefaultFilterDetailsModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        #endregion

        #region Filter masters

        public async Task<ResponseEntity> GetFilterMasters(PHSFilterMastersRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PHSFilterMastersRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intUserID", request.UserID),
                    new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType))
                };

                object rawPractice = await _repository.GetAsyncSP<PHSPracticeMasterModel>("usp_Whizible2_ProjectHealthSheet_Sel_tbl_CNF_Practice_dashboard", sqlParams);

                var businessGroupParams = new List<SqlParameter>
                {
                    new SqlParameter("@userID", request.UserID),
                    new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType))
                };

                object rawBusiness = await _repository.GetAsyncSP<PHSBusinessGroupMasterModel>("usp_Whizible2_ProjectHealthSheet_Sel_tbl_CNF_BusinessGroup_dashboard", businessGroupParams);
                object rawProject = await _repository.GetAsyncSP<PHSProjectGroupMasterModel>("usp_Whizible2_ProjectHealthSheet_Sel_tbl_CNF_ProjectGroup_dashboard", sqlParams);

                return Success("Project Health filter masters", new
                {
                    practice = Extract<PHSPracticeMasterModel>(rawPractice),
                    businessGroup = Extract<PHSBusinessGroupMasterModel>(rawBusiness),
                    projectGroup = Extract<PHSProjectGroupMasterModel>(rawProject)
                });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetOrganizationUnits(PHSOrganizationUnitRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PHSOrganizationUnitRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intBusinessGroupID", request.BusinessGroupID.GetValueOrDefault(0)),
                    new SqlParameter("@intUserID", request.UserID),
                    new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType))
                };

                object raw = await _repository.GetAsyncSP<PHSOrganizationUnitMasterModel>("usp_Whizible2_ProjectHealthSheet_Sel_pm_LocationList_dashboard", sqlParams);
                return Success("Project Health organization units", Extract<PHSOrganizationUnitMasterModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetFilteredProjects(PHSFilterProjectListRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PHSFilterProjectListRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectTypeId", (object?)request.PracticeID ?? DBNull.Value),
                    new SqlParameter("@intBusinessgroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
                    new SqlParameter("@intOraganizationUnitID", (object?)request.OrganizationUnitID ?? DBNull.Value),
                    new SqlParameter("@intProjectGroupId", (object?)request.ProjectGroupID ?? DBNull.Value),
                    new SqlParameter("@intEmployeeID", request.UserID),
                    new SqlParameter("@strProjectIDs", DBNull.Value),
                    new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType))
                };

                object raw = await _repository.GetAsyncSP<PHSProjectListModel>("usp_Whizible2_ProjectHealthSheet_Sel_GetProjectNameList_dashboard", sqlParams);
                return Success("Project Health filtered project list", Extract<PHSProjectListModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetProjectList(PHSTopProjectListRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PHSTopProjectListRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intEmployeeID", request.UserID),
                    new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType))
                };

                object raw = await _repository.GetAsyncSP<PHSProjectListModel>("usp_Whizible2_ProjectHealthSheet_GetProjectList_dashboard", sqlParams);
                return Success("Project Health project list", Extract<PHSProjectListModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        #endregion

        #region PHS information + KPI

        public async Task<ResponseEntity> GetPHSInformation(PHSInformationRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null || request.ProjectID <= 0) return Failure("ProjectID is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intUserID", request.UserID),
                    new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType)),
                    new SqlParameter("@intProjectID", request.ProjectID),
                    new SqlParameter("@inputDate", ReportingDateOrToday(request.ReportingDate))
                };

                object raw = await _repository.GetAsyncSP<PHSInformationModel>("usp_Whizible2_ProjectHealthSheet_GetProjectHealthSheetInformation_dashboard", sqlParams);
                return Success("Project Health information", Extract<PHSInformationModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetKPISummary(PHSKpiSummaryRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) return Failure("Request body is required.");

                DateTime reportingDate = ReportingDateOrToday(request.ReportingDate);

                // Fetch SQERT List - We pass PageSize=1000000 to guarantee ALL records for KPI
                var sqertParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProgramID", (object?)request.ProjectGroupID ?? DBNull.Value),
                    new SqlParameter("@intProjectId", DBNull.Value),
                    new SqlParameter("@intProjectCategoeyID", (object?)request.PracticeID ?? DBNull.Value),
                    new SqlParameter("@intBusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
                    new SqlParameter("@intOrganizationUnitID", (object?)request.OrganizationUnitID ?? DBNull.Value),
                    new SqlParameter("@dtmReportedStartDate", reportingDate),
                    new SqlParameter("@dtmReportedEndDate", reportingDate),
                    new SqlParameter("@intFlag", 0),
                    new SqlParameter("@intEmployeeID", request.UserID),
                    new SqlParameter("@strProjectIDs", DBNull.Value),
                    new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType)),
                    new SqlParameter("@PageNumber", 1),
                    new SqlParameter("@PageSize", 1000000) // FIX: Explicitly fetch a million rows
                };
                object rawSqert = await _repository.GetAsyncSP<PHSSqertListModel>("usp_Whizible2_ProjectHealthSheet_GetSQERTList_dashboard", sqertParams);
                List<PHSSqertListModel> sqertList = Extract<PHSSqertListModel>(rawSqert);

                // Fetch Pending Lock List
                var pendingParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProgramID", (object?)request.ProjectGroupID ?? DBNull.Value),
                    new SqlParameter("@intProjectId", DBNull.Value),
                    new SqlParameter("@intProjectCategoeyID", (object?)request.PracticeID ?? DBNull.Value),
                    new SqlParameter("@intBusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
                    new SqlParameter("@intOrganizationUnitID", (object?)request.OrganizationUnitID ?? DBNull.Value),
                    new SqlParameter("@dtmReportedStartDate", reportingDate),
                    new SqlParameter("@dtmReportedEndDate", reportingDate),
                    new SqlParameter("@intFlag", 0),
                    new SqlParameter("@intEmployeeID", request.UserID),
                    new SqlParameter("@strProjectIDs", DBNull.Value),
                    new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType)),
                    new SqlParameter("@PageNumber", 1),
                    new SqlParameter("@PageSize", 1000000) // FIX: Explicitly fetch a million rows
                };
                object rawPending = await _repository.GetAsyncSP<PHSProjectPendingLockModel>("usp_Whizible2_ProjectHealthSheet_Sel_GetProjectNameList_Lock_dashboard", pendingParams);
                List<PHSProjectPendingLockModel> pendingList = Extract<PHSProjectPendingLockModel>(rawPending);

                // Fetch Threshold Ranges
                object rawRanges = await _repository.GetAsyncSP<PHSSqertRangeModel>("usp_Whizible2_ProjectHealthSheet_sel_SQERT_tbl_PRS_SQERT_Ranges_dashboard", new List<SqlParameter>());
                List<PHSSqertRangeModel> ranges = Extract<PHSSqertRangeModel>(rawRanges);

                // Calculate KPI
                int onTrackCount = 0;
                int atRiskCount = 0;
                string firstOnTrack = "No project found";
                List<string> riskNotes = new List<string>();

                foreach (var proj in sqertList)
                {
                    List<string> failedMetrics = new List<string>();

                    if (IsMetricAtRisk(1, proj.Scope, ranges)) failedMetrics.Add("Scope");
                    if (IsMetricAtRisk(2, proj.Quality, ranges)) failedMetrics.Add("Quality");
                    if (IsMetricAtRisk(3, proj.Effort, ranges)) failedMetrics.Add("Effort");
                    if (IsMetricAtRisk(4, proj.Risk, ranges)) failedMetrics.Add("Risk");
                    if (IsMetricAtRisk(5, proj.Time, ranges)) failedMetrics.Add("Time");

                    if (failedMetrics.Any())
                    {
                        atRiskCount++;
                        if (riskNotes.Count == 0) riskNotes.AddRange(failedMetrics);
                    }
                    else
                    {
                        onTrackCount++;
                        if (firstOnTrack == "No project found") firstOnTrack = proj.ProjectName;
                    }
                }

                var summary = new PHSKpiSummaryModel
                {
                    // FIX: Safely read the TotalRecords count directly from the SQL output if available
                    TotalProjects = sqertList.FirstOrDefault()?.TotalRecords ?? sqertList.Count,
                    OnTrackProjects = onTrackCount,
                    OnTrackProjectName = firstOnTrack,
                    AtRiskProjects = atRiskCount,
                    AtRiskNote = riskNotes.Any() ? string.Join(" / ", riskNotes.Distinct()) + " issues" : "No current risks",
                    UnlockedProjects = pendingList.FirstOrDefault()?.TotalRecords ?? pendingList.Count,
                    AsOfDate = reportingDate.ToString("dd-MMM-yyyy")
                };

                return Success("Project Health KPI summary", summary);
            }
            catch (Exception ex) { return Failure(ex); }
        }

        #endregion

        #region SQERT Grids

        public async Task<ResponseEntity> GetProjectsPendingLock(PHSProjectPendingLockRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) return Failure("Request body is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProgramID", (object?)request.ProgramID ?? DBNull.Value),
                    new SqlParameter("@intProjectId", request.ProjectID > 0 ? request.ProjectID : DBNull.Value),
                    new SqlParameter("@intProjectCategoeyID", (object?)request.ProjectTypeID ?? DBNull.Value),
                    new SqlParameter("@intBusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
                    new SqlParameter("@intOrganizationUnitID", (object?)request.OrganizationUnitID ?? DBNull.Value),
                    new SqlParameter("@dtmReportedStartDate", ReportingDateOrToday(request.ReportingStartDate)),
                    new SqlParameter("@dtmReportedEndDate", ReportingDateOrToday(request.ReportingEndDate)),
                    new SqlParameter("@intFlag", request.Flag),
                    new SqlParameter("@intEmployeeID", request.EmployeeID.GetValueOrDefault(0)),
                    new SqlParameter("@strProjectIDs", DBNull.Value),
                    new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType)),
                    new SqlParameter("@PageNumber", request.PageNumber),
                    new SqlParameter("@PageSize", request.PageSize)
                };

                object raw = await _repository.GetAsyncSP<PHSProjectPendingLockModel>("usp_Whizible2_ProjectHealthSheet_Sel_GetProjectNameList_Lock_dashboard", sqlParams);
                var list = Extract<PHSProjectPendingLockModel>(raw);
                int total = list.FirstOrDefault()?.TotalRecords ?? 0;

                return Success("Project Health pending lock list", new { totalRecords = total, data = list });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetSQERTList(PHSSqertListRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new PHSSqertListRequest();

                var reportingDate = ReportingDateOrToday(request.ReportingDate);
                var sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@intProgramID", (object?)request.ProgramID ?? DBNull.Value),
            new SqlParameter("@intProjectId", request.ProjectID > 0 ? request.ProjectID : DBNull.Value),
            new SqlParameter("@intProjectCategoeyID", (object?)request.ProjectTypeID ?? DBNull.Value),
            new SqlParameter("@intBusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
            new SqlParameter("@intOrganizationUnitID", (object?)request.OrganizationUnitID ?? DBNull.Value),
            new SqlParameter("@dtmReportedStartDate", reportingDate),
            new SqlParameter("@dtmReportedEndDate", reportingDate),
            new SqlParameter("@intFlag", request.Flag),
            new SqlParameter("@intEmployeeID", request.UserID),
            new SqlParameter("@strProjectIDs", DBNull.Value),
            new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType)),
            new SqlParameter("@PageNumber", request.PageNumber),
            new SqlParameter("@PageSize", request.PageSize)
        };

                // 1. Fetch the raw paginated list from the database
                object raw = await _repository.GetAsyncSP<PHSSqertListModel>("usp_Whizible2_ProjectHealthSheet_GetSQERTList_dashboard", sqlParams);
                var list = Extract<PHSSqertListModel>(raw);

                // 2. Fetch the SQERT Threshold Ranges so we can calculate the colors
                object rawRanges = await _repository.GetAsyncSP<PHSSqertRangeModel>("usp_Whizible2_ProjectHealthSheet_sel_SQERT_tbl_PRS_SQERT_Ranges_dashboard", new List<SqlParameter>());
                List<PHSSqertRangeModel> ranges = Extract<PHSSqertRangeModel>(rawRanges);

                // 3. Calculate "Worst-Case" RAG for the Project Overview
                foreach (var proj in list)
                {
                    // Evaluate all 5 metrics
                    string[] colors = new string[]
                    {
                        GetMetricColor(1, proj.Scope, ranges),
                        GetMetricColor(2, proj.Quality, ranges),
                        GetMetricColor(3, proj.Effort, ranges),
                        GetMetricColor(4, proj.Risk, ranges),
                        GetMetricColor(5, proj.Time, ranges)
                    };

                    // Worst-case calculation
                    if (colors.Contains("Red"))
                    {
                        proj.ProjectOverview = "Red";
                    }
                    else if (colors.Contains("Yellow"))
                    {
                        proj.ProjectOverview = "Yellow";
                    }
                    else
                    {
                        proj.ProjectOverview = "Green";
                    }
                }

                int total = list.FirstOrDefault()?.TotalRecords ?? 0;

                return Success("Project Health SQERT list", new { totalRecords = total, data = list });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetSQERTRange()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                object raw = await _repository.GetAsyncSP<PHSSqertRangeModel>("usp_Whizible2_ProjectHealthSheet_sel_SQERT_tbl_PRS_SQERT_Ranges_dashboard", new List<SqlParameter>());
                return Success("Project Health SQERT ranges", Extract<PHSSqertRangeModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetSQERTSection(PHSSqertSectionRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) return Failure("Request body is required.");
                if (request.ProjectID <= 0) return Failure("ProjectID is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectId", request.ProjectID),
                    new SqlParameter("@dtmReportedEndDate", ReportingDateOrToday(request.ReportingDate)),
                    new SqlParameter("@intEmployeeID", request.UserID),
                    new SqlParameter("@strLoginType", NormalizeLoginType(request.LoginType))
                };

                object raw = await _repository.GetAsyncSP<PHSSqertSectionModel>("usp_Whizible2_ProjectHealthSheet_SQERT_dashboard", sqlParams);
                return Success("Project Health SQERT section", Extract<PHSSqertSectionModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        #endregion

        #region Key achievement / Issue details / Milestone / Active resource

        public async Task<ResponseEntity> GetKeyAchievement(PHSProjectScopeRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null || request.ProjectID <= 0) return Failure("ProjectID is required.");

                var sqlParams = BuildScopeParameters(request);
                sqlParams.Add(new SqlParameter("@PageNumber", request.PageNumber));
                sqlParams.Add(new SqlParameter("@PageSize", request.PageSize));

                object raw = await _repository.GetAsyncSP<PHSKeyAchievementModel>("usp_Whizible2_ProjectHealthSheet_GetNoOfTaskCompletedForPeriod_Count_dashboard", sqlParams);
                var list = Extract<PHSKeyAchievementModel>(raw);

                // FIX: The SQL SP returns 1 aggregated record, but the UI explicitly 
                // renders it as 2 separate rows ("Task" and "Deliverables").
                // We override the total count to 2 so the UI paginator is accurate.
                int total = list.Any() ? 2 : 0;
                if (list.Any()) list[0].TotalRecords = total;
                return Success("Project Health key achievement", new { totalRecords = total, data = list });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetIssueDetails(PHSProjectScopeRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null || request.ProjectID <= 0) return Failure("ProjectID is required.");

                var sqlParams = BuildScopeParameters(request);
                sqlParams.Add(new SqlParameter("@PageNumber", request.PageNumber));
                sqlParams.Add(new SqlParameter("@PageSize", request.PageSize));

                object raw = await _repository.GetAsyncSP<PHSIssueDetailModel>("usp_Whizible2_ProjectHealthSheet_Get_OpenAndClosedIssuestypes_dashboard", sqlParams);
                var list = Extract<PHSIssueDetailModel>(raw);
                int total = list.FirstOrDefault()?.TotalRecords ?? 0;

                return Success("Project Health issue details", new { totalRecords = total, data = list });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetMilestoneDetails(PHSProjectScopeRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null || request.ProjectID <= 0) return Failure("ProjectID is required.");

                var sqlParams = BuildScopeParameters(request);

                // Add pagination params which were not in the shared builder
                sqlParams.Add(new SqlParameter("@PageNumber", request.PageNumber));
                sqlParams.Add(new SqlParameter("@PageSize", request.PageSize));

                object raw = await _repository.GetAsyncSP<PHSMilestoneModel>("usp_Whizible2_Sel_ProjectHealthSheet_MilestoneDetails_dashboard", sqlParams);
                var list = Extract<PHSMilestoneModel>(raw);
                int total = list.FirstOrDefault()?.TotalRecords ?? 0;

                return Success("Project Health milestone details", new { totalRecords = total, data = list });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetActiveResources(PHSActiveResourceRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null || request.ProjectID <= 0) return Failure("ProjectID is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@ProjectID", request.ProjectID),
                    new SqlParameter("@PageNumber", request.PageNumber),
                    new SqlParameter("@PageSize", request.PageSize)
                };

                object raw = await _repository.GetAsyncSP<PHSActiveResourceModel>("usp_Whizible2_Sel_ResourceDetailsForProject_dashboard", sqlParams);
                var list = Extract<PHSActiveResourceModel>(raw);
                int total = list.FirstOrDefault()?.TotalRecords ?? 0;

                return Success("Project Health active resources", new { totalRecords = total, data = list });
            }
            catch (Exception ex) { return Failure(ex); }
        }

        #endregion

        #region Graphs

        public async Task<ResponseEntity> GetTaskVsCompletionGraph(PHSTaskVsCompletionRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null || request.ProjectID <= 0) return Failure("ProjectID is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectID", request.ProjectID),
                    new SqlParameter("@dtToDate", ReportingDateOrToday(request.CurrentDate))
                };

                object raw = await _repository.GetAsyncSP<PHSTaskVsCompletionModel>("usp_Whizible2_ProjectHealthSheet_TotalTasksVsTaskStatus_dashboard", sqlParams);
                return Success("Project Health task-vs-completion graph", Extract<PHSTaskVsCompletionModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetDelayInDaysGraph(PHSDelayInDaysRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null || request.ProjectID <= 0) return Failure("ProjectID is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectID", request.ProjectID),
                    new SqlParameter("@dtmReportedEndDate", ReportingDateOrToday(request.CurrentDate))
                };

                object raw = await _repository.GetAsyncSP<PHSDelayInDaysModel>("usp_Whizible2_ProjectHealthSheet_TotalTasksVSCompletionTime_Graph_dashboard", sqlParams);
                return Success("Project Health delay-in-days graph", Extract<PHSDelayInDaysModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        public async Task<ResponseEntity> GetMonthlyResourceCostGraph(PHSMonthlyResourceCostRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null || request.ProjectID <= 0) return Failure("ProjectID is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectID", request.ProjectID),
                    new SqlParameter("@dtmReportedEndDate", ReportingDateOrToday(request.CurrentDate))
                };

                object raw = await _repository.GetAsyncSP<PHSMonthlyResourceCostModel>("usp_Whizible2_ProjectHealthSheet_Sel_TotalEffortsMonthVSResourceCost_Graph_dashboard", sqlParams);
                return Success("Project Health monthly resource cost graph", Extract<PHSMonthlyResourceCostModel>(raw));
            }
            catch (Exception ex) { return Failure(ex); }
        }

        #endregion
    }
}
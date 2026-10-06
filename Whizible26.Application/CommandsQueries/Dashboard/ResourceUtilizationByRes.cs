using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.Repository.DashboardRepo;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.Application.CommandsQueries.Dashboard
{
    // Added for Resource Utilization By Resource API
    public class ResourceUtilizationByRes
    {
        private readonly IConfiguration _configuration;
        private readonly ResourceUtilizationByResRepo _repository;
        private readonly string _connectionString;
        //Added by Aditya J. on 25-08-2026 for Resource Utilization Report
        private readonly IWebHostEnvironment? _webHostEnvironment;
        private const string MonthlyReportSpName = "usp_Whizible2_Sel_ResourceUtilization_Monthly_Report";
        //End of Added by Aditya J. on 25-08-2026 for Resource Utilization Report

        private const string DataSpName = "usp_SEL_Whizible2_ResourceUtilizationByResource";
        private const string DateRangeSpName = "usp_SEL_Whizible2_ResourceUtilizationDateRange";

        //Added by Aditya J. on 03-09-2026 for adding Business Groups/Organization Unit/Delivery Unit/Resource multi-select filters
        private const string BusinessGroupSpName = "usp_Whizible2_Sel_GetBusinessGroups_DB";
        private const string OrganizationUnitSpName = "usp_Whizible2_Sel_GetOrganizationUnit_DB";
        private const string DeliveryUnitSpName = "usp_Whizible2_Sel_GetDeliveryUnit_DB";
        private const string ResourceSpName = "usp_Whizible2_Sel_GetResources_DB";
        //End of Added by Aditya J. on 03-09-2026 for adding Business Groups/Organization Unit/Delivery Unit/Resource multi-select filters

        //Added by Aditya J. on 09-09-2026<Added generic cascading filter stored procedure name>
        private const string DependentFilterSpName = "usp_Whizible2_Sel_GetResourceUtilizationDependentFilters_DB";
        //End of Added by Aditya J. on 09-09-2026<Added generic cascading filter stored procedure name>

        // Added for Resource Utilization By Resource API - default constructor required by the repository pattern
        public ResourceUtilizationByRes() { }
        // End of Added for Resource Utilization By Resource API

        // Added for Resource Utilization By Resource API - initialise the service with configuration
        public ResourceUtilizationByRes(
            IConfiguration configuration,
            IWebHostEnvironment? webHostEnvironment = null)
        {
            _configuration = configuration;
            //Added by Aditya J. on 25-08-2026 for Resource Utilization Report
            _webHostEnvironment = webHostEnvironment;
            //End of Added by Aditya J. on 25-08-2026 for Resource Utilization Report
            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(
                    _configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);
                _repository = new ResourceUtilizationByResRepo(_connectionString);
            }
        }
        // End of Added for Resource Utilization By Resource API

        #region Helpers

        // Added for Resource Utilization By Resource API - guard against an unconfigured repository
        private ResponseEntity RepositoryNotConfigured()
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message = "Database connection is not configured." }
            };
        }
        // End of Added for Resource Utilization By Resource API

        // Added for Resource Utilization By Resource API - Empty / Swagger "string" -> no filter
        private static string? NullIfBlank(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            if (trimmed.Equals("string", StringComparison.OrdinalIgnoreCase)) return null;
            return trimmed;
        }
        // End of Added for Resource Utilization By Resource API

        // Added for Resource Utilization By Resource API - CSV of ints; no valid int -> NULL (no filter)
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
        // End of Added for Resource Utilization By Resource API

        // Added for Resource Utilization By Resource API - '0'/'1' flag; blank -> '0' (matches SP default)
        private static string ShowDeployableOnlyFlag(string? value)
        {
            var trimmed = NullIfBlank(value);
            return trimmed == "1" ? "1" : "0";
        }
        // End of Added for Resource Utilization By Resource API

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

        // Added for Resource Utilization By Resource API - shared parameter list for Graph/Details/Summary
        //Updated by Aditya J. on 07-09-2026 for Business Groups/Organization Unit/Delivery Unit/Resource
        //multi-select filters - @intBUID/@intOUID/@intDUID/@intEmployeeID are now sent as CSV strings
        //(same NullIfBlankIntCsv helper already used for @strProjectIDs) so usp_SEL_Whizible2_ResourceUtilizationByResource
        //can match every checked value via an IN-list instead of a single scalar id.
        private List<SqlParameter> BuildDataParams(ResourceUtilizationRequest request, int intDetails)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@intBUID", (object?)NullIfBlankIntCsv(request.BUID) ?? DBNull.Value),
                new SqlParameter("@intOUID", (object?)NullIfBlankIntCsv(request.OUID) ?? DBNull.Value),
                new SqlParameter("@intDUID", (object?)NullIfBlankIntCsv(request.DUID) ?? DBNull.Value),
                new SqlParameter("@intEmployeeID", (object?)NullIfBlankIntCsv(request.EmployeeID) ?? DBNull.Value),
                new SqlParameter("@DateRange", (object?)request.DateRange ?? 10),
                new SqlParameter("@intDetails", intDetails),
                new SqlParameter("@intUserID", request.UserID),
                new SqlParameter("@strProjectIDs", (object?)NullIfBlankIntCsv(request.ProjectIDs) ?? DBNull.Value),
                new SqlParameter("@ShowDeployableOnly", ShowDeployableOnlyFlag(request.ShowDeployableOnly))
            };
        }
        //End of Updated by Aditya J. on 07-09-2026
        // End of Added for Resource Utilization By Resource API

        #endregion

        // Added for Resource Utilization By Resource API - Graph tab (@intDetails = 0)
        public async Task<ResponseEntity> GetGraph(ResourceUtilizationRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ResourceUtilizationRequest();

                var sqlParams = BuildDataParams(request, intDetails: 0);

                // 1 result set -> monthly graph rows (Available/Planned/Actual/Billable/Bench % and ratios)
                var result = await _repository.GetAsyncSP<ResourceUtilizationGraphModel>(DataSpName, sqlParams);

                return Success("Resource utilization graph", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added for Resource Utilization By Resource API

        // Added for Resource Utilization By Resource API - Display Details tab (@intDetails = 1)
        public async Task<ResponseEntity> GetDetails(ResourceUtilizationRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ResourceUtilizationRequest();

                var sqlParams = BuildDataParams(request, intDetails: 1);

                // 1 result set -> one row per resource per month
                var result = await _repository.GetAsyncSP<ResourceUtilizationDetailModel>(DataSpName, sqlParams);

                return Success("Resource utilization details", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added for Resource Utilization By Resource API

        // Added for Resource Utilization By Resource API - Display Summary tab (@intDetails = 2)
        public async Task<ResponseEntity> GetSummary(ResourceUtilizationRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ResourceUtilizationRequest();

                var sqlParams = BuildDataParams(request, intDetails: 2);

                // 1 result set -> one row per month, aggregated across resources
                var result = await _repository.GetAsyncSP<ResourceUtilizationSummaryModel>(DataSpName, sqlParams);

                return Success("Resource utilization summary", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added for Resource Utilization By Resource API

        // Added for Resource Utilization By Resource API - Date Range filter lookup (standalone SP)
        public async Task<ResponseEntity> GetDateRanges(ResourceUtilizationDateRangeRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ResourceUtilizationDateRangeRequest();

                var sqlParams = new List<SqlParameter>
                {
                    //new SqlParameter("@intDateRangeID", (object?)request.DateRangeID ?? 10),
                    new SqlParameter("@intDateRangeID", (object?)request.DateRangeID ?? DBNull.Value),
                    new SqlParameter("@ShowProjectFromTo", (object?)request.ShowProjectFromTo ?? DBNull.Value)
                };

                // 1 result set -> date range filter options
                var result = await _repository.GetAsyncSP<ResourceUtilizationDateRangeModel>(DateRangeSpName, sqlParams);

                return Success("Resource utilization date ranges", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added for Resource Utilization By Resource API

        //Added by Aditya J. on 03-09-2026 for adding Business Groups multi-select filter
        public async Task<ResponseEntity> GetBusinessGroups()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var sqlParams = new List<SqlParameter>();

                // 1 result set -> Business Group filter options
                var result = await _repository.GetAsyncSP<ResourceUtilizationBusinessGroupModel>(BusinessGroupSpName, sqlParams);

                return Success("Resource utilization business groups", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        //End of Added by Aditya J. on 03-09-2026 for adding Business Groups multi-select filter

        //Added by Aditya J. on 03-09-2026 for adding Organization Unit multi-select filter
        public async Task<ResponseEntity> GetOrganizationUnits()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var sqlParams = new List<SqlParameter>();

                // 1 result set -> Organization Unit filter options
                var result = await _repository.GetAsyncSP<ResourceUtilizationOrganizationUnitModel>(OrganizationUnitSpName, sqlParams);

                return Success("Resource utilization organization units", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        //End of Added by Aditya J. on 03-09-2026 for adding Organization Unit multi-select filter

        //Added by Aditya J. on 03-09-2026 for adding Delivery Unit multi-select filter
        public async Task<ResponseEntity> GetDeliveryUnits()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var sqlParams = new List<SqlParameter>();

                // 1 result set -> Delivery Unit filter options
                var result = await _repository.GetAsyncSP<ResourceUtilizationDeliveryUnitModel>(DeliveryUnitSpName, sqlParams);

                return Success("Resource utilization delivery units", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        //End of Added by Aditya J. on 03-09-2026 for adding Delivery Unit multi-select filter

        //Added by Aditya J. on 03-09-2026 for adding Resource multi-select filter
        public async Task<ResponseEntity> GetResources()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var sqlParams = new List<SqlParameter>();

                // 1 result set -> Resource filter options
                var result = await _repository.GetAsyncSP<ResourceUtilizationResourceModel>(ResourceSpName, sqlParams);

                return Success("Resource utilization resources", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        //End of Added by Aditya J. on 03-09-2026 for adding Resource multi-select filter

        //Added by Aditya J. on 09-09-2026<Added generic cascading filter lookup for BG/OU/DU/Resource dropdowns>
        public async Task<ResponseEntity> GetDependentFilters(ResourceUtilizationDependentFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ResourceUtilizationDependentFilterRequest();

                var filterType = NullIfBlank(request.FilterType);
                if (filterType == null)
                {
                    return new ResponseEntity
                    {
                        Status = ResponseStatus.FAILURE,
                        Data = new { message = "FilterType is required." }
                    };
                }

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@strFilterType", filterType),
                    new SqlParameter("@strBUIDs", (object?)NullIfBlankIntCsv(request.BUID) ?? DBNull.Value),
                    new SqlParameter("@strOUIDs", (object?)NullIfBlankIntCsv(request.OUID) ?? DBNull.Value),
                    new SqlParameter("@strDUIDs", (object?)NullIfBlankIntCsv(request.DUID) ?? DBNull.Value),
                    new SqlParameter("@intUserID", request.UserID > 0 ? request.UserID : (object)DBNull.Value)
                };

                var result = await _repository.GetAsyncSP<ResourceUtilizationDependentFilterModel>(
                    DependentFilterSpName, sqlParams);

                return Success("Resource utilization dependent filters", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        //End of Added by Aditya J. on 09-09-2026<Added generic cascading filter lookup for BG/OU/DU/Resource dropdowns>

        //Added by Aditya J. on 25-08-2026 for Resource Utilization Report
        public async Task<(byte[]? PdfBytes, string FileName, string? ErrorMessage)> ExportResourceUtilizationReport(
            ResourceUtilizationRequest request)
        {
            if (_repository == null)
                return (null, string.Empty, "Database connection is not configured.");

            request ??= new ResourceUtilizationRequest();

            //Updated by Aditya J. on 07-09-2026 for Business Groups/Organization Unit/Delivery Unit/Resource
            //multi-select filters - @intBUID/@intOUID/@intDUID/@intEmployeeID now go to
            //usp_Whizible2_Sel_ResourceUtilization_Monthly_Report as CSV strings (see the SP's 07-09-2026
            //revision), matching every checked filter value instead of only the first one (ToDbInt removed).
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@intBUID", (object?)NullIfBlankIntCsv(request.BUID) ?? DBNull.Value),
                new SqlParameter("@intOUID", (object?)NullIfBlankIntCsv(request.OUID) ?? DBNull.Value),
                new SqlParameter("@intDUID", (object?)NullIfBlankIntCsv(request.DUID) ?? DBNull.Value),
                new SqlParameter("@intEmployeeID", (object?)NullIfBlankIntCsv(request.EmployeeID) ?? DBNull.Value),
                new SqlParameter("@DateRange", request.DateRange > 0 ? request.DateRange : 3),
                new SqlParameter("@intDetails", request.Details),
                new SqlParameter("@intUserID", request.UserID > 0 ? request.UserID : 61),
                new SqlParameter("@strProjectIDs", (object?)NullIfBlankIntCsv(request.ProjectIDs) ?? DBNull.Value),
                new SqlParameter("@ShowDeployableOnly", ShowDeployableOnlyFlag(request.ShowDeployableOnly))
            };
            //End of Updated by Aditya J. on 07-09-2026

            var ds = await _repository.GetDataSetAsync(MonthlyReportSpName, sqlParams);
            //Modified by Aditya J. on 03-09-2026 for Resource Utilization Report - carry ResourceName
            //through so the PDF can show per-resource rows plus a per-month total, matching the base
            //site's "Total Work(Hrs) for<Month>" row
            var rows = MapMonthlyReportRows(ds?.Tables.Count > 0 ? ds.Tables[0] : null);
            //End of Modified by Aditya J. on 03-09-2026 for Resource Utilization Report
            if (rows.Count == 0)
                return (null, string.Empty, "No data found for given input.");

            var companyName = await GetCompanyName();
            var (logoBytes, logoExtension) = await LoadCompanyLogo();
            var filters = await ResolveReportFilters(request);
            var pdfBytes = BuildResourceUtilizationPdf(rows, companyName, logoBytes, logoExtension, filters);
            var fileName = $"ResourceUtilizationSummary_{DateTime.Now:yyyyMMdd}.pdf";
            return (pdfBytes, fileName, null);
        }

        private async Task<(string BusinessGroup, string OrganizationUnit, string DeliveryUnit, string Resource, string Period)>
            ResolveReportFilters(ResourceUtilizationRequest request)
        {
            var businessGroup = DisplayNameOrDash(request.BusinessGroupName);
            var organizationUnit = DisplayNameOrDash(request.OrganizationUnitName);
            var deliveryUnit = DisplayNameOrDash(request.DeliveryUnitName);
            var resource = DisplayNameOrDash(request.ResourceName);
            var period = DisplayNameOrDash(request.PeriodName);

            //Updated by Aditya J. on 07-09-2026 for Business Groups/Organization Unit/Delivery Unit/Resource
            //multi-select filters - BUID/OUID are now CSV lists, so resolve every selected id's name and
            //join with ", " for the PDF header instead of looking up a single id.
            //Updated by Aditya J. on 09-09-2026<Resolve all selected BG/OU/DU/Resource names on the PDF header>
            //Updated by Aditya J. on 10-09-2026<Use page-selected names and the cascading filter SP so PDF BG/OU match the dropdowns>
            var buidCsv = NullIfBlankIntCsv(request.BUID);
            var ouidCsv = NullIfBlankIntCsv(request.OUID);
            var duidCsv = NullIfBlankIntCsv(request.DUID);
            var employeeCsv = NullIfBlankIntCsv(request.EmployeeID);

            if (businessGroup == "-" && buidCsv != null)
            {
                try
                {
                    businessGroup = JoinSelectedFilterNames(
                        buidCsv,
                        await LookupDependentFilterNames("BG", null, null, null, request.UserID));
                }
                catch { }
            }

            if (organizationUnit == "-" && ouidCsv != null)
            {
                try
                {
                    organizationUnit = JoinSelectedFilterNames(
                        ouidCsv,
                        await LookupDependentFilterNames("OU", buidCsv, null, null, request.UserID));
                }
                catch { }
            }

            if (deliveryUnit == "-" && duidCsv != null)
            {
                try
                {
                    deliveryUnit = JoinSelectedFilterNames(
                        duidCsv,
                        await LookupDependentFilterNames("DU", buidCsv, ouidCsv, null, request.UserID));
                }
                catch { }
            }

            if (resource == "-" && employeeCsv != null)
            {
                try
                {
                    resource = JoinSelectedFilterNames(
                        employeeCsv,
                        await LookupDependentFilterNames("RES", buidCsv, ouidCsv, duidCsv, request.UserID));
                }
                catch { }
            }
            //End of Updated by Aditya J. on 10-09-2026<Use page-selected names and the cascading filter SP so PDF BG/OU match the dropdowns>
            //End of Updated by Aditya J. on 09-09-2026<Resolve all selected BG/OU/DU/Resource names on the PDF header>
            //End of Updated by Aditya J. on 07-09-2026

            try
            {
                if (period == "-")
                {
                    var dateRangeId = request.DateRange > 0 ? request.DateRange : 3;
                    var rangeParams = new List<SqlParameter>
                    {
                        new SqlParameter("@intDateRangeID", dateRangeId),
                        new SqlParameter("@ShowProjectFromTo", DBNull.Value)
                    };
                    var rangeResult = await _repository.GetAsyncSP<ResourceUtilizationDateRangeModel>(
                        DateRangeSpName, rangeParams);
                    List<ResourceUtilizationDateRangeModel> ranges = rangeResult.ResourceUtilizationDateRangeModel;
                    var match = ranges?.FirstOrDefault(r => r.UniqueId == dateRangeId) ?? ranges?.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(match?.Description))
                        period = match.Description;
                    else if (dateRangeId == 3)
                        period = "Current Financial Year";
                }
            }
            catch
            {
                if (period == "-" && (request.DateRange <= 0 || request.DateRange == 3))
                    period = "Current Financial Year";
            }

            return (businessGroup, organizationUnit, deliveryUnit, resource, period);
        }

        private async Task<string> GetCompanyName()
        {
            try
            {
                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_sel_tbl_PM_CompanyInformation",
                    new List<SqlParameter>());
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null && table.Rows.Count > 0)
                    return GetString(table.Rows[0], "CompanyName") ?? string.Empty;
            }
            catch { }
            return string.Empty;
        }

        private async Task<(byte[]? Bytes, string Extension)> LoadCompanyLogo()
        {
            try
            {
                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_Logo_tbl_PM_CompanyInformation",
                    new List<SqlParameter>());
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;

                //Added by Aditya J. on 09-09-2026<Load company logo from image bytes or from Uploads/Logo using OriginalFileName/SystemFileName>
                if (table != null && table.Rows.Count > 0)
                {
                    var imageBytes = GetBytes(table.Rows[0],
                        "Logo", "CompanyLogo", "LogoImage", "Image", "Photo", "FileData", "ImageData");
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        var extFromName = Path.GetExtension(GetString(table.Rows[0],
                            "OriginalFileName", "SystemFileName", "FileName", "LogoFileName") ?? string.Empty);
                        return (imageBytes, string.IsNullOrWhiteSpace(extFromName) ? ".png" : extFromName);
                    }
                }

                var fileName = table != null && table.Rows.Count > 0
                    ? GetString(table.Rows[0],
                        "OriginalFileName", "SystemFileName", "FileName", "LogoFileName", "ImageName")
                    : null;
                fileName = string.IsNullOrWhiteSpace(fileName) ? null : Path.GetFileName(fileName.Trim());

                foreach (var folder in GetLogoFolders())
                {
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        var logoPath = Path.Combine(folder, fileName);
                        if (File.Exists(logoPath))
                            return (File.ReadAllBytes(logoPath), Path.GetExtension(logoPath));
                    }

                    var fallbackLogo = Path.Combine(folder, "no-photo.png");
                    if (File.Exists(fallbackLogo))
                        return (File.ReadAllBytes(fallbackLogo), ".png");

                    var clogo = Path.Combine(folder, "Clogo.png");
                    if (File.Exists(clogo))
                        return (File.ReadAllBytes(clogo), ".png");
                }
                //End of Added by Aditya J. on 09-09-2026<Load company logo from image bytes or from Uploads/Logo using OriginalFileName/SystemFileName>

                return (null, ".png");
            }
            catch
            {
                return (null, ".png");
            }
        }

        //Added by Aditya J. on 09-09-2026<Search API wwwroot and content-root Uploads/Logo folders for the company logo>
        private IEnumerable<string> GetLogoFolders()
        {
            var folders = new List<string>();
            void add(string? path)
            {
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
                if (!folders.Contains(path, StringComparer.OrdinalIgnoreCase))
                    folders.Add(path);
            }

            add(_webHostEnvironment?.WebRootPath is string webRoot
                ? Path.Combine(webRoot, "Uploads", "Logo") : null);
            add(_webHostEnvironment?.WebRootPath is string webRoot2
                ? Path.Combine(webRoot2, "ProductImage") : null);
            add(_webHostEnvironment?.ContentRootPath is string contentRoot
                ? Path.Combine(contentRoot, "wwwroot", "Uploads", "Logo") : null);
            add(_webHostEnvironment?.ContentRootPath is string contentRoot2
                ? Path.Combine(contentRoot2, "wwwroot", "ProductImage") : null);
            add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Logo"));
            add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImage"));

            var parent = Directory.GetParent(Directory.GetCurrentDirectory());
            if (parent != null)
            {
                add(Path.Combine(parent.FullName, "wwwroot", "Uploads", "Logo"));
                add(Path.Combine(parent.FullName, "wwwroot", "ProductImage"));
            }

            return folders;
        }
        //End of Added by Aditya J. on 09-09-2026<Search API wwwroot and content-root Uploads/Logo folders for the company logo>

        //Modified by Aditya J. on 03-09-2026 for Resource Utilization Report - PDF-only row shape that
        //adds ResourceName (the monthly report SP's @intDetails = 1 result is one row per resource per
        //month) so the report can list resources and total each month, without touching the shared
        //ResourceUtilizationMonthlyReportModel entity used elsewhere
        private class MonthlyReportPdfRow
        {
            public string Month { get; set; } = string.Empty;
            public string ResourceName { get; set; } = string.Empty;
            public string InstallCapacityHrs { get; set; }
            public string AvailableHrs { get; set; }
            public double AvailablePercent { get; set; }
            public string PlannedHrs { get; set; }
            public double PlannedPercent { get; set; }
            public string ActualHrs { get; set; }
            public double ActualPercent { get; set; }
            public string BillableHrs { get; set; }
            public double BillablePercent { get; set; }
        }
        //End of Modified by Aditya J. on 03-09-2026 for Resource Utilization Report

        private static List<MonthlyReportPdfRow> MapMonthlyReportRows(DataTable? table)
        {
            var list = new List<MonthlyReportPdfRow>();
            if (table == null) return list;

            foreach (DataRow row in table.Rows)
            {
                var month = ReadMonth(row);
                if (string.IsNullOrWhiteSpace(month))
                    continue;
                if (month.IndexOf("total", StringComparison.OrdinalIgnoreCase) >= 0
                    || month.IndexOf("grand", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                //list.Add(new MonthlyReportPdfRow
                //{
                //    Month = month,
                //    //Added by Aditya J. on 03-09-2026 for Resource Utilization Report
                //    ResourceName = GetString(row,
                //        "ResourceName", "EmployeeName", "Resource Name", "Resource") ?? string.Empty,
                //    //End of Added by Aditya J. on 03-09-2026 for Resource Utilization Report
                //    InstallCapacityHrs = GetDouble(row,
                //        "InstallCapacityHrs", "InstalledCapacityHrs", "InstalledCapacity", "InstalledHrs",
                //        "InstallCapacity", "InstalledHours", "Install Capacity (Hrs)"),
                //    AvailableHrs = GetDouble(row,
                //        "AvailableHrs", "CapacityHrs", "AvailableHours", "Available", "Available (Hrs)"),
                //    AvailablePercent = GetDouble(row,
                //        "AvailablePercent", "CapacityPercent", "AvailableHrsPercent", "AvailablePer", "Available %"),
                //    PlannedHrs = GetDouble(row,
                //        "PlannedHrs", "AllocatedHrs", "PlannedHours", "Planned", "Planned (Hrs)"),
                //    PlannedPercent = GetDouble(row,
                //        "PlannedPercent", "AllocatedPercent", "PlannedHrsPercent", "PlannedPer", "Planned %"),
                //    ActualHrs = GetDouble(row,
                //        "ActualHrs", "ActualHours", "Actual", "Actual (Hrs)"),
                //    ActualPercent = GetDouble(row,
                //        "ActualPercent", "ActualHrsPercent", "ActualPer", "Actual %"),
                //    BillableHrs = GetDouble(row,
                //        "BillableHrs", "BillableHours", "Billable", "Billable (Hrs)"),
                //    BillablePercent = GetDouble(row,
                //        "BillablePercent", "BillableHrsPercent", "BillablePer", "Billable %")
                //});
                list.Add(new MonthlyReportPdfRow
                {
                    Month = month,

                    // Added by Aditya J. on 03-09-2026 for Resource Utilization Report
                    ResourceName = GetString(row,
        "ResourceName", "EmployeeName", "Resource Name", "Resource") ?? string.Empty,
                    // End of Added by Aditya J. on 03-09-2026 for Resource Utilization Report

                    InstallCapacityHrs = GetString(row,
        "InstallCapacityHrs", "InstalledCapacityHrs", "InstalledCapacity", "InstalledHrs",
        "InstallCapacity", "InstalledHours", "Install Capacity (Hrs)") ?? "0:00",

                    AvailableHrs = GetString(row,
        "AvailableHrs", "CapacityHrs", "AvailableHours", "Available", "Available (Hrs)") ?? "0:00",

                    AvailablePercent = GetDouble(row,
        "AvailablePercent", "CapacityPercent", "AvailableHrsPercent", "AvailablePer", "Available %", "Capacity%"),

                    PlannedHrs = GetString(row,
        "PlannedHrs", "AllocatedHrs", "PlannedHours", "Planned", "Planned (Hrs)") ?? "0:00",

                    PlannedPercent = GetDouble(row,
        "PlannedPercent", "AllocatedPercent", "PlannedHrsPercent", "PlannedPer", "Planned %","Allocated%"),

                    ActualHrs = GetString(row,
        "ActualHrs", "ActualHours", "Actual", "Actual (Hrs)") ?? "0:00",

                    ActualPercent = GetDouble(row,
        "ActualPercent", "ActualHrsPercent", "ActualPer", "Actual %"),

                    BillableHrs = GetString(row,
        "BillableHrs", "BillableHours", "Billable", "Billable (Hrs)") ?? "0:00",

                    BillablePercent = GetDouble(row,
        "BillablePercent", "BillableHrsPercent", "BillablePer", "Billable %")
                });
            }

            return list;
        }

        private static byte[] BuildResourceUtilizationPdf(
    List<MonthlyReportPdfRow> rows,
    string companyName,
    byte[]? logoBytes,
    string logoExtension,
    (string BusinessGroup, string OrganizationUnit, string DeliveryUnit, string Resource, string Period) filters)
        {
            var culture = CultureInfo.InvariantCulture;
            string? tempLogoPath = null;

            // Added by Aditya J. on 03-09-2026 for Resource Utilization Report
            // Converts the string returned by fn_Whizible2_ConvertDecimalToHourViceVersa
            // back to decimal hours ONLY for PDF total/percentage calculations.
            // Example:
            // "270,452,049:46" -> 270452049.7667
            static double ParseHourValue(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return 0;

                value = value.Trim();

                // Handle HH:MM format returned by the SQL function.
                if (value.Contains(':'))
                {
                    var parts = value.Split(':');

                    if (parts.Length >= 2 &&
                        double.TryParse(
                            parts[0].Replace(",", ""),
                            NumberStyles.Float | NumberStyles.AllowThousands,
                            CultureInfo.InvariantCulture,
                            out var hours) &&
                        double.TryParse(
                            parts[1].Replace(",", ""),
                            NumberStyles.Float | NumberStyles.AllowThousands,
                            CultureInfo.InvariantCulture,
                            out var minutes))
                    {
                        return hours + (minutes / 60.0);
                    }
                }

                // Fallback in case the value is already a decimal number.
                if (double.TryParse(
                    value.Replace(",", ""),
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture,
                    out var decimalHours))
                {
                    return decimalHours;
                }

                return 0;
            }

            // Converts decimal hours to the same HH:MM representation
            // used by fn_Whizible2_ConvertDecimalToHourViceVersa.
            static string FormatHours(double hours)
            {
                if (double.IsNaN(hours) || double.IsInfinity(hours))
                    return "0:00";

                if (hours < 0)
                    hours = 0;

                var totalMinutes = (long)Math.Round(hours * 60.0, MidpointRounding.AwayFromZero);

                var wholeHours = totalMinutes / 60;
                var minutes = totalMinutes % 60;

                //return $"{wholeHours:N0}:{minutes:00}";
                return $"{wholeHours.ToString("#,##0", CultureInfo.InvariantCulture)}:{minutes:00}";
            }

            var document = new Document();
            document.Info.Title = "Resource Utilization Summary (By Resource)";
            document.Info.Author = companyName;

            var style = document.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 8;

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.TopMargin = Unit.FromCentimeter(3.4);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.4);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.0);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.0);
            section.PageSetup.HeaderDistance = Unit.FromCentimeter(0.4);
            section.PageSetup.FooterDistance = Unit.FromCentimeter(0.35);

            var banner = section.Headers.Primary.AddTable();
            banner.Borders.Width = 0;
            banner.AddColumn(Unit.FromCentimeter(4.5));
            banner.AddColumn(Unit.FromCentimeter(18.7));
            banner.AddColumn(Unit.FromCentimeter(4.5));

            var bannerRow = banner.AddRow();
            bannerRow.VerticalAlignment = VerticalAlignment.Top;

            if (logoBytes != null && logoBytes.Length > 0)
            {
                try
                {
                    var extension = string.IsNullOrWhiteSpace(logoExtension)
                        ? ".png"
                        : logoExtension;

                    if (!extension.StartsWith("."))
                        extension = "." + extension;

                    tempLogoPath = Path.Combine(
                        Path.GetTempPath(),
                        $"logo_{Guid.NewGuid()}{extension}");

                    File.WriteAllBytes(tempLogoPath, logoBytes);

                    var logoPara = bannerRow.Cells[0].AddParagraph();
                    var logo = logoPara.AddImage(tempLogoPath);

                    logo.LockAspectRatio = true;
                    logo.Width = Unit.FromPoint(90);
                }
                catch
                {
                    bannerRow.Cells[0].AddParagraph();
                }
            }

            var companyPara =
                bannerRow.Cells[1].AddParagraph(companyName ?? string.Empty);

            companyPara.Format.Alignment = ParagraphAlignment.Center;
            companyPara.Format.Font.Size = 12;
            companyPara.Format.Font.Bold = true;
            companyPara.Format.Font.Underline = Underline.Single;
            companyPara.Format.SpaceAfter = Unit.FromPoint(2);

            var headerTitle =
                bannerRow.Cells[1].AddParagraph(
                    "Resource Utilization Summary (By Resource)");

            headerTitle.Format.Alignment = ParagraphAlignment.Center;
            headerTitle.Format.Font.Size = 11;
            headerTitle.Format.Font.Bold = true;

            AddFilterLine(
                section,
                "Business Group : ",
                filters.BusinessGroup);

            AddFilterLine(
                section,
                "Organization Unit : ",
                filters.OrganizationUnit);

            AddFilterLine(
                section,
                "Delivery Unit : ",
                filters.DeliveryUnit);

            AddFilterLine(
                section,
                "Resource : ",
                filters.Resource);

            AddFilterLine(
                section,
                "Period : ",
                filters.Period,
                valueInBlue: true);

            var spacer = section.AddParagraph();
            spacer.Format.SpaceAfter = Unit.FromPoint(6);

            var body = section.AddTable();
            body.Borders.Width = 0;
            ApplyUtilizationColumns(body);

            // Modified by Aditya J. on 03-09-2026 for Resource Utilization Report
            // Added Resource Name column so month-wise subtotal rows match
            // the base site's per-resource layout.
            string[] headers =
            {
        "Month",
        "Resource Name",
        "Install Capacity (Hrs)",
        "Available (Hrs)",
        "Available %",
        "Planned (Hrs)",
        "Planned %",
        "Actual (Hrs)",
        "Actual %",
        "Billable (Hrs)",
        "Billable %"
    };

            var headerRow = body.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Format.Font.Size = 8;
            headerRow.Borders.Bottom.Width = 0.75;
            headerRow.Borders.Bottom.Color = Colors.Black;
            headerRow.TopPadding = 3;
            headerRow.BottomPadding = 4;

            for (int i = 0; i < headers.Length; i++)
            {
                var align =
                    i <= 1
                        ? ParagraphAlignment.Left
                        : ParagraphAlignment.Right;

                SetCell(
                    headerRow.Cells[i],
                    headers[i],
                    align);
            }

            // These remain numeric because they are used only for
            // calculations inside the PDF.
            double totInstall = 0;
            double totAvailable = 0;
            double totPlanned = 0;
            double totActual = 0;
            double totBillable = 0;

            // Added by Aditya J. on 03-09-2026
            // Running month totals.
            double monthInstall = 0;
            double monthAvailable = 0;
            double monthPlanned = 0;
            double monthActual = 0;
            double monthBillable = 0;

            string? currentMonth = null;

            foreach (var item in rows)
            {
                // Convert string HH:MM values to decimal hours
                // ONLY for calculations.
                var itemInstall =
                    ParseHourValue(item.InstallCapacityHrs);

                var itemAvailable =
                    ParseHourValue(item.AvailableHrs);

                var itemPlanned =
                    ParseHourValue(item.PlannedHrs);

                var itemActual =
                    ParseHourValue(item.ActualHrs);

                var itemBillable =
                    ParseHourValue(item.BillableHrs);

                // Added by Aditya J. on 03-09-2026
                // Flush previous month totals when month changes.
                if (currentMonth != null &&
                    !string.Equals(
                        currentMonth,
                        item.Month,
                        StringComparison.OrdinalIgnoreCase))
                {
                    AddMonthTotalRow(
                        body,
                        currentMonth,
                        monthInstall,
                        monthAvailable,
                        monthPlanned,
                        monthActual,
                        monthBillable,
                        culture);

                    monthInstall = 0;
                    monthAvailable = 0;
                    monthPlanned = 0;
                    monthActual = 0;
                    monthBillable = 0;
                }

                currentMonth = item.Month;

                monthInstall += itemInstall;
                monthAvailable += itemAvailable;
                monthPlanned += itemPlanned;
                monthActual += itemActual;
                monthBillable += itemBillable;

                totInstall += itemInstall;
                totAvailable += itemAvailable;
                totPlanned += itemPlanned;
                totActual += itemActual;
                totBillable += itemBillable;

                var dataRow = body.AddRow();

                dataRow.VerticalAlignment =
                    VerticalAlignment.Center;

                dataRow.TopPadding = 2;
                dataRow.BottomPadding = 2;

                SetCell(
                    dataRow.Cells[0],
                    item.Month,
                    ParagraphAlignment.Left);

                SetCell(
                    dataRow.Cells[1],
                    item.ResourceName,
                    ParagraphAlignment.Left);

                // IMPORTANT:
                // Display the string returned by SQL/function directly.
                // Do NOT call GetDouble/FormatNumber for hour values.
                SetCell(
                    dataRow.Cells[2],
                    item.InstallCapacityHrs ?? "0:00",
                    ParagraphAlignment.Right);

                SetCell(
                    dataRow.Cells[3],
                    item.AvailableHrs ?? "0:00",
                    ParagraphAlignment.Right);

                SetCell(
                    dataRow.Cells[4],
                    FormatNumber(
                        item.AvailablePercent,
                        culture),
                    ParagraphAlignment.Right);

                SetCell(
                    dataRow.Cells[5],
                    item.PlannedHrs ?? "0:00",
                    ParagraphAlignment.Right);

                SetCell(
                    dataRow.Cells[6],
                    FormatNumber(
                        item.PlannedPercent,
                        culture),
                    ParagraphAlignment.Right);

                SetCell(
                    dataRow.Cells[7],
                    item.ActualHrs ?? "0:00",
                    ParagraphAlignment.Right);

                SetCell(
                    dataRow.Cells[8],
                    FormatNumber(
                        item.ActualPercent,
                        culture),
                    ParagraphAlignment.Right);

                SetCell(
                    dataRow.Cells[9],
                    item.BillableHrs ?? "0:00",
                    ParagraphAlignment.Right);

                SetCell(
                    dataRow.Cells[10],
                    FormatNumber(
                        item.BillablePercent,
                        culture),
                    ParagraphAlignment.Right);
            }

            // Added by Aditya J. on 03-09-2026
            // Flush the last month's running total.
            if (currentMonth != null)
            {
                AddMonthTotalRow(
                    body,
                    currentMonth,
                    monthInstall,
                    monthAvailable,
                    monthPlanned,
                    monthActual,
                    monthBillable,
                    culture);
            }

            // Grand Total
            var totalRow = body.AddRow();

            totalRow.Format.Font.Bold = true;

            totalRow.Borders.Top.Width = 1.5;
            totalRow.Borders.Top.Color = Colors.Black;

            totalRow.Borders.Bottom.Width = 0.75;
            totalRow.Borders.Bottom.Color = Colors.Black;

            totalRow.TopPadding = 3;
            totalRow.BottomPadding = 3;

            totalRow.Cells[0].MergeRight = 1;

            SetCell(
                totalRow.Cells[0],
                "Grand Total",
                ParagraphAlignment.Left);

            // IMPORTANT:
            // Hours are formatted as HH:MM, not as decimal numbers.
            SetCell(
                totalRow.Cells[2],
                FormatHours(totInstall),
                ParagraphAlignment.Right);

            SetCell(
                totalRow.Cells[3],
                FormatHours(totAvailable),
                ParagraphAlignment.Right);

            SetCell(
                totalRow.Cells[4],
                FormatNumber(
                    PercentOf(totAvailable, totInstall),
                    culture),
                ParagraphAlignment.Right);

            SetCell(
                totalRow.Cells[5],
                FormatHours(totPlanned),
                ParagraphAlignment.Right);

            SetCell(
                totalRow.Cells[6],
                FormatNumber(
                    PercentOf(totPlanned, totAvailable),
                    culture),
                ParagraphAlignment.Right);

            SetCell(
                totalRow.Cells[7],
                FormatHours(totActual),
                ParagraphAlignment.Right);

            SetCell(
                totalRow.Cells[8],
                FormatNumber(
                    PercentOf(totActual, totAvailable),
                    culture),
                ParagraphAlignment.Right);

            SetCell(
                totalRow.Cells[9],
                FormatHours(totBillable),
                ParagraphAlignment.Right);

            SetCell(
                totalRow.Cells[10],
                FormatNumber(
                    PercentOf(totBillable, totAvailable),
                    culture),
                ParagraphAlignment.Right);

            var footer = section.Footers.Primary.AddTable();

            footer.Borders.Width = 0;
            footer.Borders.Top.Width = 0.5;
            footer.Borders.Top.Color = Colors.Black;

            footer.AddColumn(Unit.FromCentimeter(11.0));
            footer.AddColumn(Unit.FromCentimeter(5.7));
            footer.AddColumn(Unit.FromCentimeter(11.0));

            var footerRow = footer.AddRow();

            footerRow.TopPadding = 3;
            footerRow.Format.Font.Size = 8;

            SetCell(
                footerRow.Cells[0],
                $"{companyName} Confidential",
                ParagraphAlignment.Left);

            SetCell(
                footerRow.Cells[1],
                DateTime.Now.ToString(
                    "dd-MMM-yyyy",
                    culture),
                ParagraphAlignment.Center);

            var pagePara =
                footerRow.Cells[2].AddParagraph();

            pagePara.Format.Alignment =
                ParagraphAlignment.Right;

            pagePara.AddText("Page ");
            pagePara.AddPageField();
            pagePara.AddText(" of ");
            pagePara.AddNumPagesField();

            footerRow.Cells[2].VerticalAlignment =
                VerticalAlignment.Center;

            try
            {
                var renderer =
                    new PdfDocumentRenderer(true)
                    {
                        Document = document
                    };

                renderer.RenderDocument();

                using var stream = new MemoryStream();

                renderer.PdfDocument.Save(
                    stream,
                    false);

                return stream.ToArray();
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(tempLogoPath) &&
                    File.Exists(tempLogoPath))
                {
                    try
                    {
                        File.Delete(tempLogoPath);
                    }
                    catch
                    {
                        // Ignore cleanup failures.
                    }
                }
            }
        }

        //Modified by Aditya J. on 03-09-2026 for Resource Utilization Report - added the Resource Name
        //column and narrowed the rest slightly to keep the table within the landscape A4 print area
        private static void ApplyUtilizationColumns(Table table)
        {
            table.AddColumn(Unit.FromCentimeter(2.2));  // Month
            table.AddColumn(Unit.FromCentimeter(3.5));  // Resource Name
            table.AddColumn(Unit.FromCentimeter(2.6));  // Install Capacity (Hrs)
            table.AddColumn(Unit.FromCentimeter(2.3));  // Available (Hrs)
            table.AddColumn(Unit.FromCentimeter(2.0));  // Available %
            table.AddColumn(Unit.FromCentimeter(2.3));  // Planned (Hrs)
            table.AddColumn(Unit.FromCentimeter(2.0));  // Planned %
            table.AddColumn(Unit.FromCentimeter(2.3));  // Actual (Hrs)
            table.AddColumn(Unit.FromCentimeter(2.0));  // Actual %
            table.AddColumn(Unit.FromCentimeter(2.4));  // Billable (Hrs)
            table.AddColumn(Unit.FromCentimeter(2.0));  // Billable %
        }
        //End of Modified by Aditya J. on 03-09-2026 for Resource Utilization Report

        //Added by Aditya J. on 03-09-2026 for Resource Utilization Report - "Total Work(Hrs) for<Month>"
        //subtotal row, matching RM_ResourceUtilizationReport.aspx (base site) formatting; % columns are
        //computed against that month's Install Capacity total, same convention as the Grand Total row
        //private static void AddMonthTotalRow(
        //    Table body, string month, double install, double available, double planned, double actual, double billable, CultureInfo culture)
        //{
        //    var row = body.AddRow();
        //    row.Format.Font.Bold = true;
        //    row.Borders.Top.Width = 1.0;
        //    row.Borders.Top.Color = Colors.Black;
        //    row.Borders.Bottom.Width = 0.5;
        //    row.Borders.Bottom.Color = Colors.Black;
        //    row.TopPadding = 2;
        //    row.BottomPadding = 2;
        //    row.Cells[0].MergeRight = 1;
        //    SetCell(row.Cells[0], $"Total Work(Hrs) for{month}", ParagraphAlignment.Left);
        //    SetCell(row.Cells[2], FormatNumber(install, culture), ParagraphAlignment.Right);
        //    SetCell(row.Cells[3], FormatNumber(available, culture), ParagraphAlignment.Right);
        //    SetCell(row.Cells[4], FormatNumber(PercentOf(available, install), culture), ParagraphAlignment.Right);
        //    SetCell(row.Cells[5], FormatNumber(planned, culture), ParagraphAlignment.Right);
        //    SetCell(row.Cells[6], FormatNumber(PercentOf(planned, install), culture), ParagraphAlignment.Right);
        //    SetCell(row.Cells[7], FormatNumber(actual, culture), ParagraphAlignment.Right);
        //    SetCell(row.Cells[8], FormatNumber(PercentOf(actual, install), culture), ParagraphAlignment.Right);
        //    SetCell(row.Cells[9], FormatNumber(billable, culture), ParagraphAlignment.Right);
        //    SetCell(row.Cells[10], FormatNumber(PercentOf(billable, install), culture), ParagraphAlignment.Right);
        //}
        // Added by Aditya J. on 03-09-2026 for Resource Utilization Report
        // Updated to match RM_ResourceUtilizationReport.aspx/base UI calculations.
        //
        // Hour values are displayed in HH:MM format.
        // Available %  = Available / Install Capacity * 100
        // Planned %    = Planned / Available * 100
        // Actual %     = Actual / Available * 100
        // Billable %   = Billable / Available * 100
        private static void AddMonthTotalRow(
            Table body,
            string month,
            double install,
            double available,
            double planned,
            double actual,
            double billable,
            CultureInfo culture)
        {
            var row = body.AddRow();

            row.Format.Font.Bold = true;

            row.Borders.Top.Width = 1.0;
            row.Borders.Top.Color = Colors.Black;

            row.Borders.Bottom.Width = 0.5;
            row.Borders.Bottom.Color = Colors.Black;

            row.TopPadding = 2;
            row.BottomPadding = 2;

            row.Cells[0].MergeRight = 1;

            SetCell(
                row.Cells[0],
                $"Total Work(hrs) for {month}",
                ParagraphAlignment.Left);

            // ------------------------------------------------------------
            // Hours
            // IMPORTANT:
            // Do NOT use FormatNumber() here.
            // UI displays hours as HH:MM.
            // ------------------------------------------------------------

            SetCell(
                row.Cells[2],
                FormatHours(install),
                ParagraphAlignment.Right);

            SetCell(
                row.Cells[3],
                FormatHours(available),
                ParagraphAlignment.Right);

            // ------------------------------------------------------------
            // Available %
            // UI calculation:
            // Available / Install Capacity * 100
            // ------------------------------------------------------------

            SetCell(
                row.Cells[4],
                FormatNumber(
                    PercentOf(available, install),
                    culture),
                ParagraphAlignment.Right);

            // ------------------------------------------------------------
            // Planned Hours
            // ------------------------------------------------------------

            SetCell(
                row.Cells[5],
                FormatHours(planned),
                ParagraphAlignment.Right);

            // ------------------------------------------------------------
            // Planned %
            //
            // IMPORTANT FIX:
            // UI uses Planned / Available * 100
            //
            // Previously this was:
            // Planned / Install * 100
            //
            // That caused:
            // 183,366.43 instead of 183,787.00
            // ------------------------------------------------------------

            SetCell(
                row.Cells[6],
                FormatNumber(
                    PercentOf(planned, available),
                    culture),
                ParagraphAlignment.Right);

            // ------------------------------------------------------------
            // Actual Hours
            // ------------------------------------------------------------

            SetCell(
                row.Cells[7],
                FormatHours(actual),
                ParagraphAlignment.Right);

            // ------------------------------------------------------------
            // Actual %
            //
            // UI uses Actual / Available * 100
            // ------------------------------------------------------------

            SetCell(
                row.Cells[8],
                FormatNumber(
                    PercentOf(actual, available),
                    culture),
                ParagraphAlignment.Right);

            // ------------------------------------------------------------
            // Billable Hours
            // ------------------------------------------------------------

            SetCell(
                row.Cells[9],
                FormatHours(billable),
                ParagraphAlignment.Right);

            // ------------------------------------------------------------
            // Billable %
            //
            // UI uses Billable / Available * 100
            // ------------------------------------------------------------

            SetCell(
                row.Cells[10],
                FormatNumber(
                    PercentOf(billable, available),
                    culture),
                ParagraphAlignment.Right);
        }
        // End of Updated AddMonthTotalRow
        //End of Added by Aditya J. on 03-09-2026 for Resource Utilization Report

        private static void AddFilterLine(Section section, string label, string value, bool valueInBlue = false)
        {
            var para = section.AddParagraph();
            para.Format.Font.Size = 9;
            para.Format.SpaceAfter = Unit.FromPoint(1);
            para.AddText(label);
            var valueText = para.AddFormattedText(string.IsNullOrWhiteSpace(value) ? "-" : value);
            if (valueInBlue)
                valueText.Color = Colors.Blue;
        }

        private static void SetCell(Cell cell, string? text, ParagraphAlignment align)
        {
            var para = cell.AddParagraph(text ?? string.Empty);
            para.Format.Alignment = align;
            cell.VerticalAlignment = VerticalAlignment.Center;
            cell.Format.Alignment = align;
        }

        private static string FormatNumber(double value, CultureInfo culture)
            => value.ToString("N2", culture);

        private static double PercentOf(double part, double total)
            => total == 0 ? 0 : (part / total) * 100.0;

        //Updated by Aditya J. on 07-09-2026 for Business Groups/Organization Unit/Delivery Unit/Resource
        //multi-select filters - replaces ToDbInt(int?), which no longer compiles now that
        //BUID/OUID/DUID/EmployeeID are CSV strings. Returns the first valid id from a CSV list (or DBNull)
        //for the few call sites that still need a single int (e.g. the OU-for-business-group lookup).
        private static object FirstIdOrDbNull(string? csv)
        {
            var normalized = NullIfBlankIntCsv(csv);
            if (normalized == null) return DBNull.Value;
            var first = normalized.Split(',')[0];
            return int.TryParse(first, out var id) ? id : DBNull.Value;
        }
        //End of Updated by Aditya J. on 07-09-2026

        //Added by Aditya J. on 10-09-2026<Use selected filter labels from the page on the PDF header>
        private static string DisplayNameOrDash(string? value)
        {
            var trimmed = NullIfBlank(value);
            return trimmed == null || trimmed == "-" ? "-" : trimmed;
        }

        private async Task<IEnumerable<(int Id, string Name)>?> LookupDependentFilterNames(
            string filterType,
            string? buidCsv,
            string? ouidCsv,
            string? duidCsv,
            int userId)
        {
            if (_repository == null) return null;

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@strFilterType", filterType),
                new SqlParameter("@strBUIDs", (object?)NullIfBlankIntCsv(buidCsv) ?? DBNull.Value),
                new SqlParameter("@strOUIDs", (object?)NullIfBlankIntCsv(ouidCsv) ?? DBNull.Value),
                new SqlParameter("@strDUIDs", (object?)NullIfBlankIntCsv(duidCsv) ?? DBNull.Value),
                new SqlParameter("@intUserID", userId > 0 ? userId : (object)DBNull.Value)
            };

            var result = await _repository.GetAsyncSP<ResourceUtilizationDependentFilterModel>(
                DependentFilterSpName, sqlParams);
            List<ResourceUtilizationDependentFilterModel> rows = result.ResourceUtilizationDependentFilterModel;
            return rows?.Select(r => (r.ID, r.Name));
        }
        //End of Added by Aditya J. on 10-09-2026<Use selected filter labels from the page on the PDF header>

        //Added by Aditya J. on 09-09-2026<Join every selected filter id to its dropdown name for the PDF header>
        private static string JoinSelectedFilterNames(
            string csv,
            IEnumerable<(int Id, string Name)>? lookup)
        {
            var selectedIds = csv.Split(',')
                .Select(p => int.TryParse(p.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
            if (selectedIds.Count == 0 || lookup == null)
                return "-";

            var map = lookup
                .Where(x => x.Id > 0 && !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First().Name);

            var names = selectedIds
                .Where(id => map.ContainsKey(id))
                .Select(id => map[id])
                .ToList();

            return names.Count == 0 ? "-" : string.Join(", ", names);
        }
        //End of Added by Aditya J. on 09-09-2026<Join every selected filter id to its dropdown name for the PDF header>

        private static string ReadMonth(DataRow row)
        {
            var text = GetString(row, "Month", "MonthName", "strMonth", "MonthYear", "MonthDesc");
            if (!string.IsNullOrWhiteSpace(text))
                return text;

            var date = GetNullableDate(row, "MonthDate", "FromDate", "StartDate");
            return date?.ToString("MMMM", CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string? GetString(DataRow row, params string[] names)
        {
            var col = FindColumn(row.Table, names);
            if (col == null || row[col] == DBNull.Value)
                return null;
            return Convert.ToString(row[col]);
        }

        //Added by Aditya J. on 09-09-2026<Read binary company logo columns from the logo stored procedure>
        private static byte[]? GetBytes(DataRow row, params string[] names)
        {
            var col = FindColumn(row.Table, names);
            if (col == null || row[col] == DBNull.Value)
                return null;
            return row[col] as byte[];
        }
        //End of Added by Aditya J. on 09-09-2026<Read binary company logo columns from the logo stored procedure>

        private static double GetDouble(DataRow row, params string[] names)
        {
            var col = FindColumn(row.Table, names);
            if (col == null || row[col] == DBNull.Value)
                return 0;
            return Convert.ToDouble(row[col]);
        }

        private static DateTime? GetNullableDate(DataRow row, params string[] names)
        {
            var col = FindColumn(row.Table, names);
            if (col == null || row[col] == DBNull.Value)
                return null;
            return Convert.ToDateTime(row[col]);
        }

        private static DataColumn? FindColumn(DataTable table, params string[] names)
        {
            foreach (var name in names)
            {
                foreach (DataColumn col in table.Columns)
                {
                    if (string.Equals(col.ColumnName, name, StringComparison.OrdinalIgnoreCase))
                        return col;

                    var compactCol = col.ColumnName.Replace(" ", string.Empty).Replace("_", string.Empty);
                    var compactName = name.Replace(" ", string.Empty).Replace("_", string.Empty);
                    if (string.Equals(compactCol, compactName, StringComparison.OrdinalIgnoreCase))
                        return col;
                }
            }
            return null;
        }
        //End of Added by Aditya J. on 25-08-2026 for Resource Utilization Report

        private static string FormatHours(double hours)
        {
            if (double.IsNaN(hours) || double.IsInfinity(hours))
                return "0:00";

            if (hours < 0)
                hours = 0;

            var totalMinutes = (long)Math.Round(
                hours * 60.0,
                MidpointRounding.AwayFromZero);

            var wholeHours = totalMinutes / 60;
            var minutes = totalMinutes % 60;

            return $"{wholeHours.ToString("#,##0", CultureInfo.InvariantCulture)}:{minutes:00}";
        }
    }
}

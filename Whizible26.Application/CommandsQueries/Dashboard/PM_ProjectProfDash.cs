using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ClosedXML.Excel;
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
using System.Threading.Tasks;
using Whizible26.Application.Repository.DashboardRepo;
using Whizible26.Domain.Entity.DashboardEntities;
using Whizible26.Domain.Entity.ProjectEntities.ProjectProfiEntity;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.Application.CommandsQueries.Dashboard
{
    // Added by Vyankat B. on 07-08-2026 - Project Profitability Dashboard
    public class PM_ProjectProfDash
    {
        private readonly IConfiguration _configuration;
        private readonly PM_ProjectProfDashRepo _repository;
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PM_ProjectProfDash() { }

        public PM_ProjectProfDash(
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment = null)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(
                    _configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);
                _repository = new PM_ProjectProfDashRepo(_connectionString);
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

        // Copied from ProjectProfitability.GetProjectProfitability — renamed for Dashboard
        public async Task<ResponseEntity> GetProjectProfDash(ProjectProfitabilityRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@ProjectID", request.ProjectID),
                    new SqlParameter("@FromDate", request.FromDate ?? (object)DBNull.Value),
                    new SqlParameter("@ToDate", request.ToDate ?? (object)DBNull.Value),
                    new SqlParameter("@ReportFlag", request.ReportFlag)
                };

                object result;

                if (request.ReportFlag == 1)
                {
                    result = await _repository.GetAsyncSP<ProjectProfitabilityPeriodicResponse>(
                        "usp_Whizible2_Sel_ProjProfitability", sqlParams);
                }
                else if (request.ReportFlag == 2)
                {
                    result = await _repository.GetAsyncSP<ProjectProfitabilityDetailedResponse>(
                        "usp_Whizible2_Sel_ProjProfitability", sqlParams);
                }
                else if (request.ReportFlag == 3)
                {
                    result = await _repository.GetAsyncSP<ProjectProfitabilityCumulativeResponse>(
                        "usp_Whizible2_Sel_ProjProfitability", sqlParams);
                }
                else
                {
                    return new ResponseEntity
                    {
                        Status = ResponseStatus.FAILURE,
                        Data = "Invalid ReportFlag provided."
                    };
                }

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        Message = "Project profitability data retrieved successfully",
                        ProjectProfitability = result
                    }
                };
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetProjectProfDash

        // Added by Vyankat B. on 07-08-2026 - usp_Whizible2_Sel_BusinessGroup
        // Updated 10-08-2026 - @userID / @strLoginType (Employee vs Customer)
        public async Task<ResponseEntity> GetBusinessGroup(ProfDashBusinessGroupRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProfDashBusinessGroupRequest();

                var loginType = string.IsNullOrWhiteSpace(request.LoginType)
                    ? "E"
                    : request.LoginType.Trim().Substring(0, 1).ToUpperInvariant();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@userID", request.UserID ?? (object)DBNull.Value),
                    new SqlParameter("@strLoginType", loginType)
                };

                var result = await _repository.GetAsyncSP<ProfDashBusinessGroupModel>(
                    "usp_Whizible2_Sel_BusinessGroup", sqlParams);

                return Success("Business Group list retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetBusinessGroup

        // Added by Vyankat B. on 07-08-2026 - usp_Whizible2_Sel_ProjectGroupForProfitability
        public async Task<ResponseEntity> GetProjectGroupForProfitability()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var result = await _repository.GetAsyncSP<ProfDashProjectGroupModel>(
                    "usp_Whizible2_Sel_ProjectGroupForProfitability",
                    new List<SqlParameter>());

                return Success("Project Group list retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetProjectGroupForProfitability

        // Added by Vyankat B. on 07-08-2026 - usp_Whizible2_Sel_Profitability_Currency
        public async Task<ResponseEntity> GetProfitabilityCurrency()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var result = await _repository.GetAsyncSP<ProfDashCurrencyModel>(
                    "usp_Whizible2_Sel_Profitability_Currency",
                    new List<SqlParameter>());

                return Success("Currency list retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetProfitabilityCurrency

        // Added by Vyankat B. on 07-08-2026 - usp_Whizible2_Sel_Profitability_Locations
        // Updated 10-08-2026 - @intUserID / @strLoginType (Employee vs Customer)
        public async Task<ResponseEntity> GetProfitabilityLocations(ProfDashLocationsRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProfDashLocationsRequest();

                var loginType = string.IsNullOrWhiteSpace(request.LoginType)
                    ? "E"
                    : request.LoginType.Trim().Substring(0, 1).ToUpperInvariant();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intBusinessGroupID", request.BusinessGroupID ?? (object)DBNull.Value),
                    new SqlParameter("@intUserID", request.UserID ?? (object)DBNull.Value),
                    new SqlParameter("@strLoginType", loginType)
                };

                var result = await _repository.GetAsyncSP<ProfDashLocationModel>(
                    "usp_Whizible2_Sel_Profitability_Locations", sqlParams);

                return Success("Location list retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetProfitabilityLocations

        // Added by Vyankat B. on 07-08-2026 - usp_Whizible2_Sel_Profitability_AccessibleProjects
        // Updated 10-08-2026 - aligned to usp_Sel_GetProjectNameList params / logic
        public async Task<ResponseEntity> GetProfitabilityAccessibleProjects(ProfDashAccessibleProjectsRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProfDashAccessibleProjectsRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectTypeId", request.ProjectTypeId ?? (object)DBNull.Value),
                    new SqlParameter("@intBusinessGroupID", request.BusinessGroupID ?? (object)DBNull.Value),
                    new SqlParameter("@intOrganizationUnitID", request.OrganizationUnitID ?? (object)DBNull.Value),
                    new SqlParameter("@intProjectGroupId", request.ProjectGroupId ?? (object)DBNull.Value),
                    new SqlParameter("@intEmployeeID", request.EmployeeID ?? 0),
                    new SqlParameter("@strProjectIDs",
                        string.IsNullOrWhiteSpace(request.ProjectIDs)
                            ? (object)DBNull.Value
                            : request.ProjectIDs.Trim()),
                    new SqlParameter("@strLoginType",
                        string.IsNullOrWhiteSpace(request.LoginType)
                            ? (object)DBNull.Value
                            : request.LoginType.Trim().Substring(0, 1).ToUpperInvariant())
                };

                var result = await _repository.GetAsyncSP<ProfDashAccessibleProjectModel>(
                    "usp_Whizible2_Sel_Profitability_AccessibleProjects", sqlParams);

                return Success("Accessible projects retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetProfitabilityAccessibleProjects

        // Added by Vyankat B. on 10-08-2026 - usp_Whizible2_Sel_Profitability_ProjectProfitByProjectGroup
        public async Task<ResponseEntity> GetProfitabilityProjectProfitByProjectGroup(
            ProfDashProjectProfitByProjectGroupRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProfDashProjectProfitByProjectGroupRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@strBusinessGroupID", ToDbCsvOrNull(request.BusinessGroupID)),
                    new SqlParameter("@strLocationID", ToDbCsvOrNull(request.LocationID)),
                    new SqlParameter("@strProjectID", ToDbCsvOrNull(request.ProjectIDs)),
                    new SqlParameter("@intProjectID",
                        !request.ProjectID.HasValue || request.ProjectID.Value <= 0
                            ? (object)DBNull.Value
                            : request.ProjectID.Value),
                    new SqlParameter("@GPMTrend", request.GPMTrend),
                    new SqlParameter("@strProjectGroupID", ToDbCsvOrNull(request.ProjectGroupID)),
                    new SqlParameter("@CurrencyID",
                        !request.CurrencyID.HasValue || request.CurrencyID.Value <= 0
                            ? (object)DBNull.Value
                            : request.CurrencyID.Value)
                };

                var result = await _repository.GetAsyncSP<ProfDashProjectProfitByProjectGroupModel>(
                    "usp_Whizible2_Sel_Profitability_ProjectProfitByProjectGroup", sqlParams);

                return Success("Project profitability by project group retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetProfitabilityProjectProfitByProjectGroup

        // Added by Vyankat B. on 14-08-2026 - PDF from usp_Whizible2_CRW_seL_tbl_pm_ProjectProfitability_ProjectProfit_forProjectGroup_Total
        public async Task<(byte[]? PdfBytes, string FileName, string? ErrorMessage)> ExportProjectProfitByProjectGroupPdf(
            ProfDashProjectProfitByProjectGroupRequest request)
        {
            if (_repository == null)
                return (null, string.Empty, "Database connection is not configured.");

            request ??= new ProfDashProjectProfitByProjectGroupRequest();

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@strBusinessGroupID", ToDbCsvOrNull(request.BusinessGroupID)),
                new SqlParameter("@strLocationID", ToDbCsvOrNull(request.LocationID)),
                new SqlParameter("@strProjectID", ToDbCsvOrNull(request.ProjectIDs)),
                new SqlParameter("@intProjectID",
                    !request.ProjectID.HasValue || request.ProjectID.Value <= 0
                        ? (object)DBNull.Value
                        : request.ProjectID.Value),
                new SqlParameter("@GPMTrend", request.GPMTrend),
                new SqlParameter("@strProjectGroupID", ToDbCsvOrNull(request.ProjectGroupID))
            };

            var ds = await _repository.GetDataSetAsync(
                "usp_Whizible2_CRW_seL_tbl_pm_ProjectProfitability_ProjectProfit_forProjectGroup_Total",
                sqlParams);

            var rows = MapProjectProfitGroupTotal(ds?.Tables.Count > 0 ? ds.Tables[0] : null);
            if (rows.Count == 0)
                return (null, string.Empty, "No data found for given input.");

            var companyName = await GetCompanyName();
            var (logoBytes, logoExtension) = await LoadCompanyLogo();
            var pdfBytes = BuildProjectProfitGroupPdf(rows, companyName, logoBytes, logoExtension);
            var fileName = $"ProjectProfitability_{DateTime.Now:yyyyMMdd}.pdf";
            return (pdfBytes, fileName, null);
        }

        // Added by Vyankat B. on 14-08-2026 - Excel from the same CRW SP (no logo)
        public async Task<(byte[]? ExcelBytes, string FileName, string? ErrorMessage)> ExportProjectProfitByProjectGroupExcel(
            ProfDashProjectProfitByProjectGroupRequest request)
        {
            if (_repository == null)
                return (null, string.Empty, "Database connection is not configured.");

            request ??= new ProfDashProjectProfitByProjectGroupRequest();

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@strBusinessGroupID", ToDbCsvOrNull(request.BusinessGroupID)),
                new SqlParameter("@strLocationID", ToDbCsvOrNull(request.LocationID)),
                new SqlParameter("@strProjectID", ToDbCsvOrNull(request.ProjectIDs)),
                new SqlParameter("@intProjectID",
                    !request.ProjectID.HasValue || request.ProjectID.Value <= 0
                        ? (object)DBNull.Value
                        : request.ProjectID.Value),
                new SqlParameter("@GPMTrend", request.GPMTrend),
                new SqlParameter("@strProjectGroupID", ToDbCsvOrNull(request.ProjectGroupID))
            };

            var ds = await _repository.GetDataSetAsync(
                "usp_Whizible2_CRW_seL_tbl_pm_ProjectProfitability_ProjectProfit_forProjectGroup_Total",
                sqlParams);

            var rows = MapProjectProfitGroupTotal(ds?.Tables.Count > 0 ? ds.Tables[0] : null);
            if (rows.Count == 0)
                return (null, string.Empty, "No data found for given input.");

            var companyName = await GetCompanyName();
            var excelBytes = BuildProjectProfitGroupExcel(rows, companyName);
            var fileName = $"ProjectProfitability_{DateTime.Now:yyyyMMdd}.xlsx";
            return (excelBytes, fileName, null);
        }

        private static byte[] BuildProjectProfitGroupExcel(
            List<ProfDashProjectProfitGroupTotalModel> rows,
            string companyName)
        {
            var first = rows[0];
            string[] headers =
            {
                "Business Group", "Project Name", "Customer", "Organization Unit",
                "As on Date", "Accrued Revenue", "Accrued Cost", "Accrued GPM",
                "Accrued GPM %", "Invoice Revenue"
            };

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Project Profitability");
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;

            ws.Range(1, 1, 1, 10).Merge();
            ws.Cell(1, 1).Value = companyName ?? string.Empty;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.Underline = XLFontUnderlineValues.Single;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, 10).Merge();
            ws.Cell(2, 1).Value = "Project Profitability";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 12;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            const int headerRow = 4;
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                if (i >= 5)
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }

            var headerRange = ws.Range(headerRow, 1, headerRow, 10);
            headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            //int excelRow = headerRow + 1;
            //foreach (var group in rows.GroupBy(r => r.BusinessGroup ?? string.Empty))
            //{
            //    var isFirstInGroup = true;
            //    foreach (var item in group)
            //    {
            //        ws.Cell(excelRow, 1).Value = isFirstInGroup ? item.BusinessGroup : null;
            //        ws.Cell(excelRow, 2).Value = item.ProjectName;
            //        ws.Cell(excelRow, 3).Value = item.CustomerName;
            //        ws.Cell(excelRow, 4).Value = item.Location;
            //        if (item.ToDate.HasValue)
            //        {
            //            ws.Cell(excelRow, 5).Value = item.ToDate.Value;
            //            ws.Cell(excelRow, 5).Style.DateFormat.Format = "dd-MMM-yyyy";
            //        }
            //        SetAmountCell(ws.Cell(excelRow, 6), item.AccruedRevenue);
            //        SetAmountCell(ws.Cell(excelRow, 7), item.AccruedCost);
            //        SetAmountCell(ws.Cell(excelRow, 8), item.AccruedGPM);
            //        SetAmountCell(ws.Cell(excelRow, 9), item.AccruedGPMPercent);
            //        SetAmountCell(ws.Cell(excelRow, 10), item.InvoiceRevenue);
            //        ws.Range(excelRow, 2, excelRow, 4).Style.Alignment.WrapText = true;
            //        isFirstInGroup = false;
            //        excelRow++;
            //    }
            //}

            int excelRow = headerRow + 1;

            foreach (var group in rows.GroupBy(r => r.BusinessGroup ?? string.Empty))
            {
                var isFirstInGroup = true;
                var groupFirst = group.First();

                foreach (var item in group)
                {
                    ws.Cell(excelRow, 1).Value =
                        isFirstInGroup ? item.BusinessGroup : null;

                    ws.Cell(excelRow, 2).Value = item.ProjectName;
                    ws.Cell(excelRow, 3).Value = item.CustomerName;
                    ws.Cell(excelRow, 4).Value = item.Location;

                    if (item.ToDate.HasValue)
                    {
                        ws.Cell(excelRow, 5).Value = item.ToDate.Value;
                        ws.Cell(excelRow, 5).Style.DateFormat.Format = "dd-MMM-yyyy";
                    }

                    SetAmountCell(ws.Cell(excelRow, 6), item.AccruedRevenue);
                    SetAmountCell(ws.Cell(excelRow, 7), item.AccruedCost);
                    SetAmountCell(ws.Cell(excelRow, 8), item.AccruedGPM);
                    SetAmountCell(ws.Cell(excelRow, 9), item.AccruedGPMPercent);
                    SetAmountCell(ws.Cell(excelRow, 10), item.InvoiceRevenue);

                    ws.Range(excelRow, 2, excelRow, 4)
                        .Style.Alignment.WrapText = true;

                    isFirstInGroup = false;
                    excelRow++;
                }

                // Business Group Total
                ws.Cell(excelRow, 1).Value = "Total";

                ws.Cell(excelRow, 1).Style.Font.Bold = true;

                SetAmountCell(
                    ws.Cell(excelRow, 6),
                    groupFirst.AccruedRevenueTotal,
                    true);

                SetAmountCell(
                    ws.Cell(excelRow, 7),
                    groupFirst.AccruedCostTotal,
                    true);

                SetAmountCell(
                    ws.Cell(excelRow, 8),
                    groupFirst.AccruedGPMTotal,
                    true);

                SetAmountCell(
                    ws.Cell(excelRow, 9),
                    groupFirst.AccruedGPMPercentTotal,
                    true);

                SetAmountCell(
                    ws.Cell(excelRow, 10),
                    groupFirst.InvoiceRevenueTotal,
                    true);

                var groupTotalRange =
                    ws.Range(excelRow, 1, excelRow, 10);

                groupTotalRange.Style.Font.Bold = true;

                groupTotalRange.Style.Border.TopBorder =
                    XLBorderStyleValues.Thin;

                groupTotalRange.Style.Border.BottomBorder =
                    XLBorderStyleValues.Thin;

                excelRow++;
            }


            ws.Cell(excelRow, 1).Value = "Grand total";
            ws.Cell(excelRow, 1).Style.Font.Bold = true;
            SetAmountCell(ws.Cell(excelRow, 6), first.AccruedRevenueGrandTotal, true);
            SetAmountCell(ws.Cell(excelRow, 7), first.AccruedCostGrandTotal, true);
            SetAmountCell(ws.Cell(excelRow, 8), first.AccruedGPMGrandTotal, true);
            SetAmountCell(ws.Cell(excelRow, 9), first.AccruedGPMPercentGrandTotal, true);
            SetAmountCell(ws.Cell(excelRow, 10), first.InvoiceRevenueGrandTotal, true);
            var totalRange = ws.Range(excelRow, 1, excelRow, 10);
            totalRange.Style.Font.Bold = true;
            totalRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            totalRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            ws.Column(1).Width = 16;
            ws.Column(2).Width = 28;
            ws.Column(3).Width = 26;
            ws.Column(4).Width = 20;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 16;
            ws.Column(7).Width = 15;
            ws.Column(8).Width = 15;
            ws.Column(9).Width = 14;
            ws.Column(10).Width = 16;
            ws.Row(headerRow).Height = 30;
            ws.SheetView.FreezeRows(headerRow);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }

        private static void SetAmountCell(IXLCell cell, decimal value, bool bold = false)
        {
            cell.Value = value;
            cell.Style.NumberFormat.Format = "#,##,##0.00";
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            if (bold)
                cell.Style.Font.Bold = true;
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
            catch
            {
                // Company name is only used in the footer; continue without it.
            }
            return string.Empty;
        }

        /* Added By Vyankat B. on 02nd Sep 2026 - PDF company logo load (OriginalFileName / SystemFileName, multiple logo paths) */
        private async Task<(byte[]? Bytes, string Extension)> LoadCompanyLogo()
        {
            try
            {
                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_Logo_tbl_PM_CompanyInformation",
                    new List<SqlParameter>());
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                var originalFileName = table != null && table.Rows.Count > 0
                    ? GetStringAny(table.Rows[0], "OriginalFileName")
                    : null;
                var systemFileName = table != null && table.Rows.Count > 0
                    ? GetStringAny(table.Rows[0], "SystemFileName")
                    : null;

                foreach (var uploadPath in GetLogoFolders())
                {
                    var logoPath = ResolveLogoFilePath(uploadPath, originalFileName, systemFileName);
                    if (logoPath != null && File.Exists(logoPath))
                        return (File.ReadAllBytes(logoPath), Path.GetExtension(logoPath));
                }

                return (null, ".png");
            }
            catch
            {
                return (null, ".png");
            }
        }

        /* Previous LoadCompanyLogo / GetLogoFolder:
        private async Task<(byte[]? Bytes, string Extension)> LoadCompanyLogo()
        {
            try
            {
                var ds = await _repository.GetDataSetAsync(
                    "usp_Whizible2_Sel_Logo_tbl_PM_CompanyInformation",
                    new List<SqlParameter>());
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                var fileName = table != null && table.Rows.Count > 0
                    ? GetString(table.Rows[0], "OriginalFileName")
                    : null;

                var uploadPath = GetLogoFolder();
                var logoPath = !string.IsNullOrWhiteSpace(fileName)
                    ? Path.Combine(uploadPath, fileName)
                    : Path.Combine(uploadPath, "no-photo.png");

                if (!File.Exists(logoPath))
                    logoPath = Path.Combine(uploadPath, "no-photo.png");

                if (!File.Exists(logoPath))
                    return (null, ".png");

                return (File.ReadAllBytes(logoPath), Path.GetExtension(logoPath));
            }
            catch
            {
                return (null, ".png");
            }
        }

        private string GetLogoFolder()
        {
            var webRoot = _webHostEnvironment?.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot) || !Directory.Exists(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(webRoot))
                {
                    var parent = Directory.GetParent(Directory.GetCurrentDirectory());
                    if (parent != null)
                        webRoot = Path.Combine(parent.FullName, "wwwroot");
                }
            }

            return Path.Combine(webRoot, "Uploads", "Logo");
        }
        */

        private IEnumerable<string> GetLogoFolders()
        {
            var folders = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddFolder(string? path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    return;

                var logoFolder = Path.Combine(path, "Uploads", "Logo");
                if (seen.Add(logoFolder))
                    folders.Add(logoFolder);
            }

            var webRoot = _webHostEnvironment?.WebRootPath;
            if (!string.IsNullOrWhiteSpace(webRoot))
                AddFolder(webRoot);

            var contentRoot = _webHostEnvironment?.ContentRootPath;
            if (!string.IsNullOrWhiteSpace(contentRoot))
                AddFolder(Path.Combine(contentRoot, "wwwroot"));

            var currentDir = Directory.GetCurrentDirectory();
            AddFolder(Path.Combine(currentDir, "wwwroot"));

            var parent = Directory.GetParent(currentDir);
            if (parent != null)
                AddFolder(Path.Combine(parent.FullName, "wwwroot"));

            return folders;
        }

        private static string? ResolveLogoFilePath(string uploadPath, params string?[] fileNames)
        {
            if (!Directory.Exists(uploadPath))
                return null;

            foreach (var fileName in fileNames)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    continue;

                var logoPath = Path.Combine(uploadPath, fileName);
                if (File.Exists(logoPath))
                    return logoPath;
            }

            var fallback = Path.Combine(uploadPath, "no-photo.png");
            if (File.Exists(fallback))
                return fallback;

            var imageFile = Directory
                .EnumerateFiles(uploadPath)
                .FirstOrDefault(path =>
                {
                    var ext = Path.GetExtension(path);
                    return ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
                        || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                        || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                        || ext.Equals(".gif", StringComparison.OrdinalIgnoreCase)
                        || ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase);
                });

            return imageFile;
        }

        private string GetLogoFolder()
        {
            return GetLogoFolders().FirstOrDefault()
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Logo");
        }

        private static string? GetStringAny(DataRow row, params string[] columnNames)
        {
            if (row?.Table?.Columns == null)
                return null;

            foreach (var columnName in columnNames)
            {
                foreach (DataColumn column in row.Table.Columns)
                {
                    if (!column.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (row[column] == DBNull.Value)
                        return null;

                    return Convert.ToString(row[column]);
                }
            }

            return null;
        }
        /* End of Added By Vyankat B. on 02nd Sep 2026 */

        private static List<ProfDashProjectProfitGroupTotalModel> MapProjectProfitGroupTotal(DataTable? table)
        {
            var list = new List<ProfDashProjectProfitGroupTotalModel>();
            if (table == null) return list;

            foreach (DataRow row in table.Rows)
            {
                list.Add(new ProfDashProjectProfitGroupTotalModel
                {
                    ProjectProfitabilityID = GetInt(row, "ProjectProfitabilityID"),
                    ProjectID = GetInt(row, "ProjectID"),
                    ProjectName = GetString(row, "ProjectName"),
                    FromDate = GetNullableDate(row, "FromDate"),
                    ToDate = GetNullableDate(row, "ToDate"),
                    ProjectCurrencyID = GetNullableInt(row, "ProjectCurrencyID"),
                    BaseCurrencyID = GetNullableInt(row, "BaseCurrencyID"),
                    AccruedRevenue = GetDecimal(row, "AccruedRevenue"),
                    InvoiceRevenue = GetDecimal(row, "InvoiceRevenue"),
                    AccruedCost = GetDecimal(row, "AccruedCost"),
                    AccruedGPM = GetDecimal(row, "AccruedGPM"),
                    AccruedGPMPercent = GetDecimal(row, "AccruedGPMPercent"),
                    BusinessGroupID = GetNullableInt(row, "BusinessGroupID"),
                    LocationID = GetNullableInt(row, "LocationID"),
                    Location = GetString(row, "Location"),
                    BusinessGroup = GetString(row, "BusinessGroup"),
                    CustomerName = GetString(row, "CustomerName"),
                    CurrencySymbol = GetString(row, "CurrencySymbol"),
                    AccruedRevenueTotal = GetDecimal(row, "AccruedRevenueTotal"),
                    InvoiceRevenueTotal = GetDecimal(row, "InvoiceRevenueTotal"),
                    AccruedCostTotal = GetDecimal(row, "AccruedCostTotal"),
                    AccruedGPMTotal = GetDecimal(row, "AccruedGPMTotal"),
                    AccruedGPMPercentTotal = GetDecimal(row, "AccruedGPMPercentTotal"),
                    AccruedRevenueGrandTotal = GetDecimal(row, "AccruedRevenueGrandTotal"),
                    InvoiceRevenueGrandTotal = GetDecimal(row, "InvoiceRevenueGrandTotal"),
                    AccruedCostGrandTotal = GetDecimal(row, "AccruedCostGrandTotal"),
                    AccruedGPMGrandTotal = GetDecimal(row, "AccruedGPMGrandTotal"),
                    AccruedGPMPercentGrandTotal = GetDecimal(row, "AccruedGPMPercentGrandTotal")
                });
            }

            return list;
        }

        private static byte[] BuildProjectProfitGroupPdf(
            List<ProfDashProjectProfitGroupTotalModel> rows,
            string companyName,
            byte[]? logoBytes,
            string logoExtension)
        {
            var culture = new CultureInfo("en-IN");
            var first = rows[0];
            string? tempLogoPath = null;

            var document = new Document();
            document.Info.Title = "Project Profitability";
            document.Info.Author = companyName;

            var style = document.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 8;

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.TopMargin = Unit.FromCentimeter(3.0);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.0);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(0.6);
            section.PageSetup.RightMargin = Unit.FromCentimeter(0.6);
            section.PageSetup.HeaderDistance = Unit.FromCentimeter(0.4);
            section.PageSetup.FooterDistance = Unit.FromCentimeter(0.3);

            var banner = section.Headers.Primary.AddTable();
            banner.Borders.Width = 0;
            banner.AddColumn(Unit.FromCentimeter(3.8));
            banner.AddColumn(Unit.FromCentimeter(19.8));
            banner.AddColumn(Unit.FromCentimeter(3.8));
            var bannerRow = banner.AddRow();
            bannerRow.VerticalAlignment = VerticalAlignment.Top;

            /* Added By Vyankat B. on 02nd Sep 2026 - PDF header logo cell alignment */
            if (logoBytes != null && logoBytes.Length > 0)
            {
                try
                {
                    var extension = string.IsNullOrWhiteSpace(logoExtension) ? ".png" : logoExtension;
                    if (!extension.StartsWith("."))
                        extension = "." + extension;
                    tempLogoPath = Path.Combine(Path.GetTempPath(), $"logo_{Guid.NewGuid()}{extension}");
                    File.WriteAllBytes(tempLogoPath, logoBytes);
                    var logoCell = bannerRow.Cells[0];
                    logoCell.VerticalAlignment = VerticalAlignment.Center;
                    var logoPara = logoCell.AddParagraph();
                    logoPara.Format.Alignment = ParagraphAlignment.Left;
                    var logo = logoPara.AddImage(tempLogoPath);
                    logo.LockAspectRatio = true;
                    logo.Width = Unit.FromPoint(90);
                }
                catch
                {
                    bannerRow.Cells[0].AddParagraph();
                }
            }
            else
            {
                bannerRow.Cells[0].AddParagraph();
            }

            bannerRow.Cells[2].AddParagraph();
            /* Previous:
            if (logoBytes != null && logoBytes.Length > 0)
            {
                try
                {
                    var extension = string.IsNullOrWhiteSpace(logoExtension) ? ".png" : logoExtension;
                    if (!extension.StartsWith("."))
                        extension = "." + extension;
                    tempLogoPath = Path.Combine(Path.GetTempPath(), $"logo_{Guid.NewGuid()}{extension}");
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
            */
            /* End of Added By Vyankat B. on 02nd Sep 2026 */

            var companyPara = bannerRow.Cells[1].AddParagraph(companyName ?? string.Empty);
            companyPara.Format.Alignment = ParagraphAlignment.Center;
            companyPara.Format.Font.Size = 12;
            companyPara.Format.Font.Bold = true;
            companyPara.Format.Font.Underline = Underline.Single;
            companyPara.Format.SpaceAfter = Unit.FromPoint(2);

            var titlePara = bannerRow.Cells[1].AddParagraph("Project Profitability");
            titlePara.Format.Alignment = ParagraphAlignment.Center;
            titlePara.Format.Font.Size = 11;
            titlePara.Format.Font.Bold = true;

            var body = section.AddTable();
            ApplyProfitColumns(body);
            body.Borders.Width = 0;

            string[] headers =
            {
                "Business Group", "Project Name", "Customer", "Organization Unit",
                "As on Date", "Accrued Revenue", "Accrued Cost", "Accrued GPM",
                "Accrued GPM %", "Invoice Revenue"
            };

            var headerRow = body.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Font.Bold = true;
            headerRow.Format.Font.Size = 7;
            headerRow.Borders.Top.Width = 0.75;
            headerRow.Borders.Bottom.Width = 0.75;
            headerRow.Borders.Top.Color = Colors.Black;
            headerRow.Borders.Bottom.Color = Colors.Black;
            headerRow.TopPadding = 3;
            headerRow.BottomPadding = 4;

            for (int i = 0; i < headers.Length; i++)
            {
                var align = i >= 5 ? ParagraphAlignment.Right : ParagraphAlignment.Left;
                SetCell(headerRow.Cells[i], headers[i], align);
            }

            //foreach (var group in rows.GroupBy(r => r.BusinessGroup ?? string.Empty))
            //{
            //    var isFirstInGroup = true;
            //    foreach (var item in group)
            //    {
            //        var dataRow = body.AddRow();
            //        dataRow.VerticalAlignment = VerticalAlignment.Center;
            //        dataRow.TopPadding = 1;
            //        dataRow.BottomPadding = 1;
            //        SetCell(dataRow.Cells[0], isFirstInGroup ? item.BusinessGroup : string.Empty, ParagraphAlignment.Left);
            //        SetCell(dataRow.Cells[1], item.ProjectName, ParagraphAlignment.Left);
            //        SetCell(dataRow.Cells[2], item.CustomerName, ParagraphAlignment.Left);
            //        SetCell(dataRow.Cells[3], item.Location, ParagraphAlignment.Left);
            //        SetCell(dataRow.Cells[4], item.ToDate?.ToString("dd-MMM-yyyy"), ParagraphAlignment.Left);
            //        SetCell(dataRow.Cells[5], FormatAmount(item.AccruedRevenue, culture), ParagraphAlignment.Right);
            //        SetCell(dataRow.Cells[6], FormatAmount(item.AccruedCost, culture), ParagraphAlignment.Right);
            //        SetCell(dataRow.Cells[7], FormatAmount(item.AccruedGPM, culture), ParagraphAlignment.Right);
            //        SetCell(dataRow.Cells[8], FormatAmount(item.AccruedGPMPercent, culture), ParagraphAlignment.Right);
            //        SetCell(dataRow.Cells[9], FormatAmount(item.InvoiceRevenue, culture), ParagraphAlignment.Right);
            //        isFirstInGroup = false;
            //    }
            //}


            foreach (var group in rows.GroupBy(r => r.BusinessGroup ?? string.Empty))
            {
                var isFirstInGroup = true;
                var groupFirst = group.First();

                foreach (var item in group)
                {
                    var dataRow = body.AddRow();

                    dataRow.VerticalAlignment = VerticalAlignment.Center;
                    dataRow.TopPadding = 1;
                    dataRow.BottomPadding = 1;

                    SetCell(
                        dataRow.Cells[0],
                        isFirstInGroup ? item.BusinessGroup : string.Empty,
                        ParagraphAlignment.Left);

                    SetCell(
                        dataRow.Cells[1],
                        item.ProjectName,
                        ParagraphAlignment.Left);

                    SetCell(
                        dataRow.Cells[2],
                        item.CustomerName,
                        ParagraphAlignment.Left);

                    SetCell(
                        dataRow.Cells[3],
                        item.Location,
                        ParagraphAlignment.Left);

                    SetCell(
                        dataRow.Cells[4],
                        item.ToDate?.ToString("dd-MMM-yyyy"),
                        ParagraphAlignment.Left);

                    SetCell(
                        dataRow.Cells[5],
                        FormatAmount(item.AccruedRevenue, culture),
                        ParagraphAlignment.Right);

                    SetCell(
                        dataRow.Cells[6],
                        FormatAmount(item.AccruedCost, culture),
                        ParagraphAlignment.Right);

                    SetCell(
                        dataRow.Cells[7],
                        FormatAmount(item.AccruedGPM, culture),
                        ParagraphAlignment.Right);

                    SetCell(
                        dataRow.Cells[8],
                        FormatAmount(item.AccruedGPMPercent, culture),
                        ParagraphAlignment.Right);

                    SetCell(
                        dataRow.Cells[9],
                        FormatAmount(item.InvoiceRevenue, culture),
                        ParagraphAlignment.Right);

                    isFirstInGroup = false;
                }

                // Business Group Total
                var groupTotalRow = body.AddRow();

                groupTotalRow.Format.Font.Bold = true;
                groupTotalRow.TopPadding = 2;
                groupTotalRow.BottomPadding = 2;

                groupTotalRow.Borders.Top.Width = 0.5;
                groupTotalRow.Borders.Bottom.Width = 0.5;

                SetCell(
                    groupTotalRow.Cells[0],
                    "Total",
                    ParagraphAlignment.Left);

                SetCell(
                    groupTotalRow.Cells[1],
                    string.Empty,
                    ParagraphAlignment.Left);

                SetCell(
                    groupTotalRow.Cells[2],
                    string.Empty,
                    ParagraphAlignment.Left);

                SetCell(
                    groupTotalRow.Cells[3],
                    string.Empty,
                    ParagraphAlignment.Left);

                SetCell(
                    groupTotalRow.Cells[4],
                    string.Empty,
                    ParagraphAlignment.Left);

                SetCell(
                    groupTotalRow.Cells[5],
                    FormatAmount(groupFirst.AccruedRevenueTotal, culture),
                    ParagraphAlignment.Right);

                SetCell(
                    groupTotalRow.Cells[6],
                    FormatAmount(groupFirst.AccruedCostTotal, culture),
                    ParagraphAlignment.Right);

                SetCell(
                    groupTotalRow.Cells[7],
                    FormatAmount(groupFirst.AccruedGPMTotal, culture),
                    ParagraphAlignment.Right);

                SetCell(
                    groupTotalRow.Cells[8],
                    FormatAmount(groupFirst.AccruedGPMPercentTotal, culture),
                    ParagraphAlignment.Right);

                SetCell(
                    groupTotalRow.Cells[9],
                    FormatAmount(groupFirst.InvoiceRevenueTotal, culture),
                    ParagraphAlignment.Right);
            }

            var totalRow = body.AddRow();
            totalRow.Format.Font.Bold = true;
            totalRow.Borders.Top.Width = 0.75;
            totalRow.Borders.Bottom.Width = 0.75;
            totalRow.TopPadding = 3;
            totalRow.BottomPadding = 3;
            SetCell(totalRow.Cells[0], "Grand total", ParagraphAlignment.Left);
            SetCell(totalRow.Cells[1], string.Empty, ParagraphAlignment.Left);
            SetCell(totalRow.Cells[2], string.Empty, ParagraphAlignment.Left);
            SetCell(totalRow.Cells[3], string.Empty, ParagraphAlignment.Left);
            SetCell(totalRow.Cells[4], string.Empty, ParagraphAlignment.Left);
            SetCell(totalRow.Cells[5], FormatAmount(first.AccruedRevenueGrandTotal, culture), ParagraphAlignment.Right);
            SetCell(totalRow.Cells[6], FormatAmount(first.AccruedCostGrandTotal, culture), ParagraphAlignment.Right);
            SetCell(totalRow.Cells[7], FormatAmount(first.AccruedGPMGrandTotal, culture), ParagraphAlignment.Right);
            SetCell(totalRow.Cells[8], FormatAmount(first.AccruedGPMPercentGrandTotal, culture), ParagraphAlignment.Right);
            SetCell(totalRow.Cells[9], FormatAmount(first.InvoiceRevenueGrandTotal, culture), ParagraphAlignment.Right);

            try
            {
                var renderer = new PdfDocumentRenderer(true) { Document = document };
                renderer.RenderDocument();
                using var stream = new MemoryStream();
                renderer.PdfDocument.Save(stream, false);
                return stream.ToArray();
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(tempLogoPath) && File.Exists(tempLogoPath))
                {
                    try { File.Delete(tempLogoPath); } catch { }
                }
            }
        }

        private static void ApplyProfitColumns(Table table)
        {
            // A4 landscape usable width ~ 28.5 cm with 0.6 cm side margins.
            table.AddColumn(Unit.FromCentimeter(2.4)); // Business Group
            table.AddColumn(Unit.FromCentimeter(3.5)); // Project Name
            table.AddColumn(Unit.FromCentimeter(3.3)); // Customer
            table.AddColumn(Unit.FromCentimeter(2.6)); // Organization Unit
            table.AddColumn(Unit.FromCentimeter(2.1)); // As on Date
            table.AddColumn(Unit.FromCentimeter(2.5)); // Accrued Revenue
            table.AddColumn(Unit.FromCentimeter(2.3)); // Accrued Cost
            table.AddColumn(Unit.FromCentimeter(2.3)); // Accrued GPM
            table.AddColumn(Unit.FromCentimeter(2.1)); // Accrued GPM %
            table.AddColumn(Unit.FromCentimeter(2.5)); // Invoice Revenue
        }

        private static void SetCell(Cell cell, string? text, ParagraphAlignment align)
        {
            var para = cell.AddParagraph(text ?? string.Empty);
            para.Format.Alignment = align;
            cell.VerticalAlignment = VerticalAlignment.Center;
            cell.Format.Alignment = align;
        }

        private static string FormatAmount(decimal value, CultureInfo culture)
            => value.ToString("N2", culture);

        private static object ToDbCsvOrNull(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv))
                return DBNull.Value;

            var trimmed = csv.Trim().Replace(" ", string.Empty);
            if (trimmed == "0")
                return DBNull.Value;

            return trimmed;
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

        private static DateTime? GetNullableDate(DataRow row, string col)
            => !row.Table.Columns.Contains(col) || row[col] == DBNull.Value
                ? null
                : Convert.ToDateTime(row[col]);
    }
    // End of Added by Vyankat B. on 07-08-2026
}

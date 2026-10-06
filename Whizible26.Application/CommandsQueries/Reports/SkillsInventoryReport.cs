using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.Repository.ReportsRepo;
using Whizible26.Domain.Entity.ReportEntities.SkillsInventory;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.Application.CommandsQueries
{
    public class SkillsInventoryReport
    {
        private readonly IConfiguration? _configuration;
        private readonly SkillsInventoryRepo? _repository;
        private readonly string? _connectionString;
        private readonly IWebHostEnvironment? _webHostEnvironment;

        private const string SP_FILTER_MASTERS = "usp_Whizible2_Sel_SkillsInventory_FilterMasters";
        private const string SP_REPORT = "usp_Whizible2_Sel_SkillsInventory_Report";
        private const string SP_RESOURCE_DETAIL = "usp_Whizible2_Sel_SkillsInventory_ResourceDetail";
        private const string SP_COMPANY_LOGO = "usp_Whizible2_Sel_Logo_tbl_PM_CompanyInformation";
        private const string SP_COMPANY_INFO = "usp_Whizible2_sel_tbl_PM_CompanyInformation";

       
        private static readonly string[] BandLabels =
            { "< 1 Year", "1 - 2 Years", "3 - 5 Years", "5 - 7 Years", "> 7 Years" };

       
        public SkillsInventoryReport() { }
        // End of default constructor

        // Added by Vyankat on 07-08-2026 - Initialise the report service with configuration
        // Updated By Vyankat B. on 27th Aug 2026 - IWebHostEnvironment for company logo on PDF.
        public SkillsInventoryReport(
            IConfiguration configuration,
            IWebHostEnvironment? webHostEnvironment = null)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(
                    _configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);
                _repository = new SkillsInventoryRepo(_connectionString);
            }
        }
        // End of Added by Vyankat on 07-08-2026

        #region Helpers

        // Added by Vyankat on 07-08-2026 - Guard against an unconfigured repository
        private ResponseEntity RepositoryNotConfigured()
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message = "Database connection is not configured." }
            };
        }
        // End of RepositoryNotConfigured

        // Added by Vyankat on 07-08-2026 - Build the shared report parameters once
        private static List<SqlParameter> BuildReportParameters(SkillsInventoryFilterRequest request)
        {
            return new List<SqlParameter>
            {
                 new SqlParameter("@ToolIDs",
                    (object?)NullIfBlank(request.ToolIDs) ?? DBNull.Value),
                 new SqlParameter("@OrgUnitIDs",
                    (object?)NullIfBlank(request.OrgUnitIDs) ?? DBNull.Value),
                  new SqlParameter("@PageNo",
            (object?)request.PageNo ?? DBNull.Value),

        new SqlParameter("@PageSize",
            (object?)request.PageSize ?? DBNull.Value)
            };
        }
        // End of BuildReportParameters

        /* Added By Vyankat B. on 02nd Sep 2026 - map SP columns like [1-2YearsExperience] to model fields */
        private async Task<(List<SkillInventoryModel> Rows, List<PaginationModel> Pagination)> LoadSkillInventoryReportAsync(
            SkillsInventoryFilterRequest request)
        {
            if (_repository == null)
                return (new List<SkillInventoryModel>(), new List<PaginationModel>());

            var ds = await _repository.GetDataSetAsync(SP_REPORT, BuildReportParameters(request));
            var rows = MapSkillInventoryTable(ds?.Tables.Count > 0 ? ds.Tables[0] : null);
            var pagination = MapPaginationTable(ds?.Tables.Count > 1 ? ds.Tables[1] : null);
            return (rows, pagination);
        }

        private static List<SkillInventoryModel> MapSkillInventoryTable(DataTable? table)
        {
            var list = new List<SkillInventoryModel>();
            if (table == null) return list;

            foreach (DataRow row in table.Rows)
            {
                list.Add(new SkillInventoryModel
                {
                    Location = GetRowString(row, "Location"),
                    Description = GetRowString(row, "Description"),
                    Below1YearExperience = GetRowNullableInt(row, "below1YearExperience", "Below1YearExperience"),
                    OneToTwoYearsExperience = GetRowNullableInt(row, "1-2YearsExperience", "OneToTwoYearsExperience"),
                    ThreeToFiveYearsExperience = GetRowNullableInt(row, "3-5YearsExperience", "ThreeToFiveYearsExperience"),
                    FiveToSevenYearsExperience = GetRowNullableInt(row, "5-7YearsExperience", "FiveToSevenYearsExperience"),
                    Above7YearsExperience = GetRowNullableInt(row, "Above7YearsExperience", "above7YearsExperience")
                });
            }

            return list;
        }

        private static List<PaginationModel> MapPaginationTable(DataTable? table)
        {
            var list = new List<PaginationModel>();
            if (table == null || table.Rows.Count == 0) return list;

            var row = table.Rows[0];
            list.Add(new PaginationModel
            {
                TotalRecords = GetRowInt(row, "totalRecords", "TotalRecords"),
                CurrentPage = GetRowInt(row, "currentPage", "CurrentPage"),
                PageSize = GetRowInt(row, "pageSize", "PageSize"),
                TotalPages = GetRowInt(row, "totalPages", "TotalPages")
            });

            return list;
        }

        private static string? GetRowString(DataRow row, params string[] columnNames)
        {
            var value = GetRowValue(row, columnNames);
            return value == null || value == DBNull.Value ? null : Convert.ToString(value);
        }

        private static int GetRowInt(DataRow row, params string[] columnNames)
            => GetRowNullableInt(row, columnNames) ?? 0;

        private static int? GetRowNullableInt(DataRow row, params string[] columnNames)
        {
            var value = GetRowValue(row, columnNames);
            if (value == null || value == DBNull.Value) return null;
            return Convert.ToInt32(value);
        }

        private static object? GetRowValue(DataRow row, params string[] columnNames)
        {
            if (row?.Table?.Columns == null) return null;

            foreach (var columnName in columnNames)
            {
                foreach (DataColumn column in row.Table.Columns)
                {
                    if (!column.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    return row[column];
                }
            }

            return null;
        }
        /* End of Added By Vyankat B. on 02nd Sep 2026 */

        // Added by Vyankat on 07-08-2026 - Treat an empty filter string as "no filter"
      
        private static string? NullIfBlank(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        // End of NullIfBlank

        // Added by Vyankat on 07-08-2026 - Pull one typed list out of the GetAsyncSP ExpandoObject
       
        private static List<T> Extract<T>(dynamic result)
        {
            var dict = result as IDictionary<string, object>;
            if (dict == null) return new List<T>();

            string key = typeof(T).Name;
            if (!dict.ContainsKey(key) || dict[key] == null) return new List<T>();

            return dict[key] as List<T> ?? new List<T>();
        }
        // End of Extract

        // Added by Vyankat on 07-08-2026 - The page enables Generate only when a filter is chosen
       
        private static bool HasAnyFilter(SkillsInventoryFilterRequest request)
        {
            return !string.IsNullOrWhiteSpace(request.OrgUnitIDs)
                || !string.IsNullOrWhiteSpace(request.ToolIDs);
        }
        // End of HasAnyFilter

        // Added by Vyankat on 07-08-2026 - Strip characters Windows will not accept in a file name
        private static string SafeFileNamePart(string? value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            var cleaned = new string(value.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
            cleaned = cleaned.Trim();
            if (cleaned.Length == 0) return fallback;
            return cleaned.Length > 40 ? cleaned.Substring(0, 40) : cleaned;
        }
        // End of SafeFileNamePart

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

        #region Queries

        // Added by Vyankat on 07-08-2026 - Populate the Organization Unit and Skills filter cards
        public async Task<ResponseEntity> GetFilterMasters(SkillsInventoryFilterMastersRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new SkillsInventoryFilterMastersRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@LoginID", (object?)request.LoginID ?? DBNull.Value),
                    new SqlParameter("@OrgUnitParameterGroupID",
                        (object?)request.OrgUnitParameterGroupID ?? DBNull.Value),
                    new SqlParameter("@IncludeInactiveEmployees", request.IncludeInactiveEmployees),
                    new SqlParameter("@OnlySkillsInUse", request.OnlySkillsInUse)
                };

                // 3 result sets -> 3 generic arguments
                var result = await _repository.GetAsyncSP<
                        SkillsInventoryOrgUnitModel,
                        SkillsInventorySkillModel,
                        SkillsInventoryBandModel>(SP_FILTER_MASTERS, sqlParams);

                return Success("Skills inventory filter masters", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetFilterMasters

        // Added by Vyankat on 07-08-2026 - The report itself (Generate Report button)
        public async Task<ResponseEntity> GetSkillsInventory(SkillsInventoryFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new SkillsInventoryFilterRequest();

                if (!HasAnyFilter(request))
                {
                    return Failure("Select at least one Organization Unit or Skill "
                                 + "before generating the report.");
                }

                /* Added By Vyankat B. on 02nd Sep 2026 - map SP experience columns correctly */
                var (rows, pagination) = await LoadSkillInventoryReportAsync(request);
                /* Previous:
                var result = await _repository.GetAsyncSP<
                     SkillInventoryModel, PaginationModel>(SP_REPORT, BuildReportParameters(request));
                return Success("Skills inventory report", result);
                */

                return Success("Skills inventory report", new
                {
                    SkillInventoryModel = rows,
                    PaginationModel = pagination
                });
                /* End of Added By Vyankat B. on 02nd Sep 2026 */
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetSkillsInventory

        // Added by Vyankat on 07-08-2026 - Named resources behind the counts
        public async Task<ResponseEntity> GetResourceDetail(SkillsInventoryFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new SkillsInventoryFilterRequest();

                if (!HasAnyFilter(request))
                {
                    return Failure("Select at least one Organization Unit or Skill "
                                 + "before generating the report.");
                }

                // 1 result set
                var result = await _repository.GetAsyncSP<SkillsInventoryResourceModel>(
                    SP_RESOURCE_DETAIL, BuildReportParameters(request));

                return Success("Skills inventory resource detail", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetResourceDetail

        #endregion

        #region Excel export

        // Added by Vyankat on 07-08-2026 - Excel export of the generated report (ClosedXML)       
        public async Task<(byte[]? Bytes, string FileName)> ExportSkillsInventoryExcel(SkillsInventoryExportRequest request)
        {
            if (_repository == null) return (Array.Empty<byte>(), BuildExcelFileName("Whizible"));
            if (request == null) request = new SkillsInventoryExportRequest();

            // Added By Vyankat B. on 27th Aug 2026
           
            request.PageNo = null;
            request.PageSize = null;
            // End of Added By Vyankat B. on 27th Aug 2026

            var (rows, _) = await LoadSkillInventoryReportAsync(request);

            if (rows == null || rows.Count == 0) return (Array.Empty<byte>(), BuildExcelFileName("Whizible"));

            /* Added By Vyankat B. on 02nd Sep 2026 - company name from company/logo SP (filename only) */
            var (_, _, companyName) = await LoadCompanyBrandingAsync();
            /* Previous:
            string companyName = string.IsNullOrWhiteSpace(request.CompanyName)
                ? "Whizible" : request.CompanyName;
            */
            /* End of Added By Vyankat B. on 02nd Sep 2026 */

            // ---------------- Grouping / totals ----------------
            var skillGroups = rows
                .GroupBy(r => r.Description ?? "")
                .ToList();

            int Band1(SkillInventoryModel r) => r.Below1YearExperience.GetValueOrDefault();
            int Band2(SkillInventoryModel r) => r.OneToTwoYearsExperience.GetValueOrDefault();
            int Band3(SkillInventoryModel r) => r.ThreeToFiveYearsExperience.GetValueOrDefault();
            int Band4(SkillInventoryModel r) => r.FiveToSevenYearsExperience.GetValueOrDefault();
            int Band5(SkillInventoryModel r) => r.Above7YearsExperience.GetValueOrDefault();

            const int colCount = 7;

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Skills Inventory");

            /* Added By Vyankat B. on 02nd Sep 2026 - simple Excel layout (matches inventory page template) */
            ws.Cell(1, 1).Value = "Skills Inventory Report";
            ws.Range(1, 1, 1, colCount).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            const int headerRow = 2;
            ws.Cell(headerRow, 1).Value = "Skill";
            ws.Cell(headerRow, 2).Value = "Organization Unit";
            for (int b = 0; b < BandLabels.Length; b++)
            {
                ws.Cell(headerRow, 3 + b).Value = BandLabels[b] + " Experience";
            }

            var headerRange = ws.Range(headerRow, 1, headerRow, colCount);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.WrapText = true;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            int r = headerRow;

            foreach (var skillGroup in skillGroups)
            {
                string skillName = skillGroup.Key;

                r++;
                ws.Cell(r, 1).Value = skillName;

                foreach (var row in skillGroup)
                {
                    r++;
                    ws.Cell(r, 2).Value = row.Location ?? "";
                    ws.Cell(r, 3).Value = Band1(row);
                    ws.Cell(r, 4).Value = Band2(row);
                    ws.Cell(r, 5).Value = Band3(row);
                    ws.Cell(r, 6).Value = Band4(row);
                    ws.Cell(r, 7).Value = Band5(row);
                    ws.Range(r, 3, r, colCount).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;
                }

                int subBand1 = skillGroup.Sum(Band1);
                int subBand2 = skillGroup.Sum(Band2);
                int subBand3 = skillGroup.Sum(Band3);
                int subBand4 = skillGroup.Sum(Band4);
                int subBand5 = skillGroup.Sum(Band5);

                r++;
                var subRange = ws.Range(r, 1, r, colCount);
                subRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E7E6E6");
                subRange.Style.Font.Bold = true;
                ws.Cell(r, 2).Value = "Subtotal";
                ws.Cell(r, 3).Value = subBand1;
                ws.Cell(r, 4).Value = subBand2;
                ws.Cell(r, 5).Value = subBand3;
                ws.Cell(r, 6).Value = subBand4;
                ws.Cell(r, 7).Value = subBand5;
                ws.Range(r, 3, r, colCount).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;
            }

            if (r > headerRow)
            {
                var tableRange = ws.Range(headerRow, 1, r, colCount);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            ws.SheetView.FreezeRows(headerRow);
            ws.Column(1).Width = 38;
            ws.Column(2).Width = 28;
            ws.Columns(3, colCount).Width = 16;
            ws.Row(headerRow).Height = 30;
            /* Previous Excel layout:
               - Skills Inventory Report + company + generated meta rows
               - Skill repeated on every OU row, Total column, Grand Total, footnote
            */
            /* End of Added By Vyankat B. on 02nd Sep 2026 */

            // ---------------- Sheet 2 : resource detail ----------------
            // Added By Vyankat B. on 27th Aug 2026
            // Detail sheet must not fail the whole export (missing SP / SQL error).
            if (request.IncludeResourceDetail)
            {
                try
                {
                    List<SkillsInventoryResourceModel> detail = Extract<SkillsInventoryResourceModel>(
                        await _repository.GetAsyncSP<SkillsInventoryResourceModel>(
                            SP_RESOURCE_DETAIL, BuildReportParameters(request)))
                        ?? new List<SkillsInventoryResourceModel>();

                    var ws2 = wb.Worksheets.Add("Resource Detail");
                    string[] headers =
                    {
                        "Skill", "Organization Unit", "Employee Code", "Employee Name",
                        "Experience (Years)", "Experience Band", "Proficiency",
                        "Core Competency", "Training Hours"
                    };

                    for (int c = 0; c < headers.Length; c++)
                    {
                        ws2.Cell(1, c + 1).Value = headers[c];
                    }

                    var h2 = ws2.Range(1, 1, 1, headers.Length);
                    h2.Style.Font.Bold = true;
                    h2.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    h2.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    int dr = 1;
                    foreach (var d in detail)
                    {
                        dr++;
                        ws2.Cell(dr, 1).Value = d.SkillName;
                        ws2.Cell(dr, 2).Value = d.OrgUnitName;
                        ws2.Cell(dr, 3).Value = d.EmployeeCode;
                        ws2.Cell(dr, 4).Value = d.EmployeeName;
                        ws2.Cell(dr, 5).Value = d.ExperienceYears;
                        ws2.Cell(dr, 6).Value = d.BandLabel;
                        ws2.Cell(dr, 7).Value = d.Proficiency;
                        ws2.Cell(dr, 8).Value = d.HasCoreCompetency == 1 ? "Yes" : "No";
                        ws2.Cell(dr, 9).Value = d.TrainingHours;
                    }

                    if (dr > 1)
                    {
                        var d2 = ws2.Range(1, 1, dr, headers.Length);
                        d2.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        d2.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

                    ws2.SheetView.FreezeRows(1);
                    ws2.Columns(1, 4).Width = 30;
                    ws2.Columns(5, headers.Length).Width = 16;
                }
                catch (Exception detailEx)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"ExportSkillsInventoryExcel ResourceDetail skipped: {detailEx}");
                }
            }
            // End of Added By Vyankat B. on 27th Aug 2026

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return (ms.ToArray(), BuildExcelFileName(companyName));
        }
        // End of ExportSkillsInventoryExcel

        // Added by Vyankat on 07-08-2026 - File name for the Excel export
        public static string BuildExcelFileName(string? companyName)
        {
            // commented and added by vyankat b. on 03th Sep 2026 for name correcting

            //string company = SafeFileNamePart(companyName, "Whizible");
            //return $"SkillsInventory_{company}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return $"{Guid.NewGuid().ToString("N").Substring(0, 8)}.xlsx";

            // End of commented and added by vyankat b. on 03th Sep 2026 for name correcting

        }
        /* Previous:
        public static string BuildExcelFileName(SkillsInventoryExportRequest request)
        {
            string company = SafeFileNamePart(request?.CompanyName, "Whizible");
            return $"SkillsInventory_{company}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        }
        */
        // End of BuildExcelFileName

        #endregion

        public async Task<(byte[]? Bytes, string FileName)> ExportSkillsInventoryPdf(SkillsInventoryExportRequest request)
        {
            if (_repository == null) return (Array.Empty<byte>(), BuildPdfFileName("Whizible"));
            if (request == null) request = new SkillsInventoryExportRequest();

            // Added By Vyankat B. on 27th Aug 2026
            request.PageNo = null;
            request.PageSize = null;
            // End of Added By Vyankat B. on 27th Aug 2026

            var (rows, _) = await LoadSkillInventoryReportAsync(request);

            if (rows == null || rows.Count == 0) return (Array.Empty<byte>(), BuildPdfFileName("Whizible"));

            /* Added By Vyankat B. on 02nd Sep 2026 - company name + logo from company SPs */
            var (logoBytes, logoExtension, companyName) = await LoadCompanyBrandingAsync();
            /* Previous:
            string companyName = string.IsNullOrWhiteSpace(request.CompanyName)
                ? "Whizible" : request.CompanyName;
            var (logoBytes, logoExtension) = await LoadCompanyLogo();
            */
            /* End of Added By Vyankat B. on 02nd Sep 2026 */

            // ---------------- Grouping / totals (no separate subtotal or total result set anymore) ----------------
            // Group by Description (skill/tool) - preserves SP's "order by tools.description, loc.location"
            var skillGroups = rows
                .GroupBy(r => r.Description ?? "")
                .ToList();

            int Band1(SkillInventoryModel r) => r.Below1YearExperience.GetValueOrDefault();
            int Band2(SkillInventoryModel r) => r.OneToTwoYearsExperience.GetValueOrDefault();
            int Band3(SkillInventoryModel r) => r.ThreeToFiveYearsExperience.GetValueOrDefault();
            int Band4(SkillInventoryModel r) => r.FiveToSevenYearsExperience.GetValueOrDefault();
            int Band5(SkillInventoryModel r) => r.Above7YearsExperience.GetValueOrDefault();

            string tempLogoPath = null;

            try
            {
                var document = new MigraDoc.DocumentObjectModel.Document();
                document.Info.Title = "Skills Inventory Report";
                document.Info.Author = companyName;
                document.Info.Subject = "Skills Inventory Report";

                Style normal = document.Styles["Normal"];
                normal.Font.Name = "Arial";
                normal.Font.Size = 8;
                normal.Font.Color = Colors.Black;

                Section section = document.AddSection();
                section.PageSetup.Orientation = Orientation.Landscape;
                section.PageSetup.PageFormat = PageFormat.A4;
                section.PageSetup.TopMargin = Unit.FromCentimeter(1.2);
                section.PageSetup.BottomMargin = Unit.FromCentimeter(1.0);
                section.PageSetup.LeftMargin = Unit.FromCentimeter(0.6);
                section.PageSetup.RightMargin = Unit.FromCentimeter(0.6);
                section.PageSetup.HeaderDistance = Unit.FromCentimeter(0.4);
                section.PageSetup.FooterDistance = Unit.FromCentimeter(0.3);

                /* Added By Vyankat B. on 02nd Sep 2026 - PDF banner with logo (page 1 only) */
                Table banner = section.AddTable();
                banner.Borders.Width = 0;
                banner.AddColumn(Unit.FromCentimeter(4.0));
                banner.AddColumn(Unit.FromCentimeter(19.4));
                banner.AddColumn(Unit.FromCentimeter(4.0));

                Row bannerRow = banner.AddRow();
                bannerRow.VerticalAlignment = VerticalAlignment.Center;
                bannerRow.HeightRule = RowHeightRule.AtLeast;
                bannerRow.Height = Unit.FromCentimeter(2.0);

                if (logoBytes != null && logoBytes.Length > 0)
                {
                    try
                    {
                        string extension = string.IsNullOrWhiteSpace(logoExtension) ? ".png" : logoExtension;
                        if (!extension.StartsWith("."))
                            extension = "." + extension;
                        tempLogoPath = Path.Combine(Path.GetTempPath(), $"logo_{Guid.NewGuid()}{extension}");
                        File.WriteAllBytes(tempLogoPath, logoBytes);

                        var logoCell = bannerRow.Cells[0];
                        logoCell.VerticalAlignment = VerticalAlignment.Center;
                        Paragraph logoPara = logoCell.AddParagraph();
                        logoPara.Format.Alignment = ParagraphAlignment.Left;
                        MigraDoc.DocumentObjectModel.Shapes.Image logo = logoPara.AddImage(tempLogoPath);
                        logo.LockAspectRatio = true;
                        logo.Width = Unit.FromPoint(82);
                    }
                    catch (Exception logoEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Skills Inventory PDF logo error: {logoEx.Message}");
                        bannerRow.Cells[0].AddParagraph();
                    }
                }
                else
                {
                    bannerRow.Cells[0].AddParagraph();
                }

                var titleCell = bannerRow.Cells[1];
                titleCell.VerticalAlignment = VerticalAlignment.Center;

                Paragraph companyPara = titleCell.AddParagraph(companyName);
                companyPara.Format.Alignment = ParagraphAlignment.Center;
                companyPara.Format.Font.Size = 12;
                companyPara.Format.Font.Bold = true;
                companyPara.Format.Font.Underline = Underline.Single;
                companyPara.Format.SpaceAfter = Unit.FromPoint(2);

                Paragraph titlePara = titleCell.AddParagraph("Skills Inventory Report");
                titlePara.Format.Alignment = ParagraphAlignment.Center;
                titlePara.Format.Font.Size = 11;
                titlePara.Format.Font.Bold = true;
                titlePara.Format.SpaceAfter = Unit.FromPoint(0);

                bannerRow.Cells[2].AddParagraph();

                Paragraph bannerSpacer = section.AddParagraph();
                bannerSpacer.Format.SpaceAfter = Unit.FromPoint(8);
                /* End of Added By Vyankat B. on 02nd Sep 2026 */

                /* Added By Vyankat B. on 02nd Sep 2026 - simple PDF table (matches Excel / inventory template) */
                Table table = section.AddTable();
                table.Borders.Width = 0.75;
                table.Borders.Color = Colors.Black;
                table.Rows.LeftIndent = 0;

                table.AddColumn(Unit.FromCentimeter(6.2));
                table.AddColumn(Unit.FromCentimeter(4.8));
                for (int b = 0; b < BandLabels.Length; b++)
                {
                    table.AddColumn(Unit.FromCentimeter(3.1));
                }

                Row header = table.AddRow();
                header.HeadingFormat = true;
                header.Format.Font.Bold = true;
                header.VerticalAlignment = VerticalAlignment.Center;
                header.Height = Unit.FromPoint(24);
                header.Borders.Bottom.Width = 0.75;

                SetPdfTextCell(header.Cells[0], "Skill", ParagraphAlignment.Left);
                SetPdfTextCell(header.Cells[1], "Organization Unit", ParagraphAlignment.Left);
                for (int b = 0; b < BandLabels.Length; b++)
                {
                    SetPdfTextCell(header.Cells[2 + b], BandLabels[b] + " Experience", ParagraphAlignment.Right);
                }

                foreach (var skillGroup in skillGroups)
                {
                    string skillName = skillGroup.Key;

                    Row skillRow = table.AddRow();
                    skillRow.VerticalAlignment = VerticalAlignment.Center;
                    SetPdfTextCell(skillRow.Cells[0], skillName, ParagraphAlignment.Left);

                    foreach (var row in skillGroup)
                    {
                        Row dataRow = table.AddRow();
                        dataRow.VerticalAlignment = VerticalAlignment.Center;
                        SetPdfTextCell(dataRow.Cells[1], row.Location ?? string.Empty, ParagraphAlignment.Left);
                        AddPdfNumberCell(dataRow, 2, Band1(row));
                        AddPdfNumberCell(dataRow, 3, Band2(row));
                        AddPdfNumberCell(dataRow, 4, Band3(row));
                        AddPdfNumberCell(dataRow, 5, Band4(row));
                        AddPdfNumberCell(dataRow, 6, Band5(row));
                    }

                    Row subRow = table.AddRow();
                    subRow.VerticalAlignment = VerticalAlignment.Center;
                    subRow.Format.Font.Bold = true;
                    subRow.Shading.Color = Colors.LightGray;
                    subRow.Borders.Top.Width = 0.75;
                    subRow.Borders.Bottom.Width = 0.75;
                    SetPdfTextCell(subRow.Cells[1], "Subtotal", ParagraphAlignment.Left);
                    AddPdfNumberCell(subRow, 2, skillGroup.Sum(Band1), true);
                    AddPdfNumberCell(subRow, 3, skillGroup.Sum(Band2), true);
                    AddPdfNumberCell(subRow, 4, skillGroup.Sum(Band3), true);
                    AddPdfNumberCell(subRow, 5, skillGroup.Sum(Band4), true);
                    AddPdfNumberCell(subRow, 6, skillGroup.Sum(Band5), true);
                }
                /* Previous PDF layout:
                   - meta line, Total column, skill on every OU row, Grand Total, footnote
                */
                /* End of Added By Vyankat B. on 02nd Sep 2026 */

                Paragraph footer = section.Footers.Primary.AddParagraph();
                footer.AddText(companyName);
                footer.AddTab();
                footer.AddText(DateTime.Now.ToString("dd-MMM-yyyy"));
                footer.AddTab();
                footer.AddText("Page ");
                footer.AddPageField();
                footer.AddText(" of ");
                footer.AddNumPagesField();
                footer.Format.Font.Size = 7;
                footer.Format.Font.Color = Colors.Black;
                footer.Format.Alignment = ParagraphAlignment.Left;
                // End of Added By Vyankat B. on 27th Aug 2026

                // ---------------- Render ----------------
                var pdfRenderer = new PdfDocumentRenderer(true);
                pdfRenderer.Document = document;
                pdfRenderer.RenderDocument();

                using var ms = new MemoryStream();
                pdfRenderer.PdfDocument.Save(ms, false);
                return (ms.ToArray(), BuildPdfFileName(companyName));
            }
            finally
            {
                // Added By Vyankat B. on 27th Aug 2026
                if (!string.IsNullOrEmpty(tempLogoPath) && File.Exists(tempLogoPath))
                {
                    try { File.Delete(tempLogoPath); } catch { /* ignore */ }
                }
                // End of Added By Vyankat B. on 27th Aug 2026
            }
        }
        // End of ExportSkillsInventoryPdf

        /* Added By Vyankat B. on 02nd Sep 2026 - company name + logo from company SPs */
        private async Task<(byte[]? LogoBytes, string LogoExtension, string CompanyName)> LoadCompanyBrandingAsync()
        {
            string companyName = string.Empty;
            byte[]? logoBytes = null;
            string logoExtension = ".png";

            try
            {
                if (_repository != null)
                {
                    var ds = await _repository.GetDataSetAsync(
                        SP_COMPANY_LOGO,
                        new List<SqlParameter>());
                    var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;

                    if (table != null && table.Rows.Count > 0)
                    {
                        var row = table.Rows[0];
                        companyName = GetStringAny(row, "CompanyName") ?? string.Empty;
                        var originalFileName = GetStringAny(row, "OriginalFileName");
                        var systemFileName = GetStringAny(row, "SystemFileName");

                        foreach (var uploadPath in GetLogoFolders())
                        {
                            var logoPath = ResolveLogoFilePath(uploadPath, originalFileName, systemFileName);
                            if (logoPath == null || !File.Exists(logoPath))
                                continue;

                            logoBytes = File.ReadAllBytes(logoPath);
                            logoExtension = Path.GetExtension(logoPath);
                            break;
                        }
                    }
                }
            }
            catch
            {
                // Logo/company name are optional in the export header.
            }

            if (string.IsNullOrWhiteSpace(companyName))
                companyName = await GetCompanyNameFromCompanyInfoAsync();

            if (string.IsNullOrWhiteSpace(companyName))
                companyName = "Whizible";

            return (logoBytes, logoExtension, companyName);
        }

        private async Task<string> GetCompanyNameFromCompanyInfoAsync()
        {
            try
            {
                if (_repository == null)
                    return string.Empty;

                var ds = await _repository.GetDataSetAsync(
                    SP_COMPANY_INFO,
                    new List<SqlParameter>());
                var table = ds?.Tables.Count > 0 ? ds.Tables[0] : null;
                if (table != null && table.Rows.Count > 0)
                    return GetStringAny(table.Rows[0], "CompanyName") ?? string.Empty;
            }
            catch
            {
                // Company name is optional in the export header.
            }

            return string.Empty;
        }

        /* Previous LoadCompanyLogo:
        private async Task<(byte[]? Bytes, string Extension)> LoadCompanyLogo()
        {
            try
            {
                if (_repository == null)
                    return (null, ".png");

                var ds = await _repository.GetDataSetAsync(
                    SP_COMPANY_LOGO,
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
        */
        /* End of Added By Vyankat B. on 02nd Sep 2026 */

        /* Previous LoadCompanyLogo / GetLogoFolder:
        private async Task<(byte[]? Bytes, string Extension)> LoadCompanyLogo()
        {
            try
            {
                if (_repository == null)
                    return (null, ".png");

                var ds = await _repository.GetDataSetAsync(
                    SP_COMPANY_LOGO,
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

            return Directory
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

        private static void SetPdfTextCell(Cell cell, string text, ParagraphAlignment align)
        {
            cell.VerticalAlignment = VerticalAlignment.Center;
            var para = cell.AddParagraph(text ?? string.Empty);
            para.Format.Alignment = align;
        }

        private static void AddPdfNumberCell(Row row, int cellIndex, int value, bool bold = false)
        {
            var cell = row.Cells[cellIndex];
            cell.VerticalAlignment = VerticalAlignment.Center;
            var para = cell.AddParagraph(value.ToString());
            para.Format.Alignment = ParagraphAlignment.Right;
            if (bold)
                para.Format.Font.Bold = true;
        }

        // Added by Vyankat on 07-08-2026 - Centre-aligned numeric cell for the PDF table
        private static void AddNumberCell(Row row, int cellIndex, int value)
        {
            row.Cells[cellIndex].AddParagraph(value.ToString());
            row.Cells[cellIndex].Format.Alignment = ParagraphAlignment.Center;
        }
        // End of AddNumberCell

        // Added by Vyankat on 07-08-2026 - File name for the PDF export
        public static string BuildPdfFileName(string? companyName)
        {
            // commented and added by vyankat b. on 03th Sep 2026 for name correcting
            //string company = SafeFileNamePart(companyName, "Whizible");
            //return $"SkillsInventory_{company}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            return $"{Guid.NewGuid().ToString("N").Substring(0, 8)}.pdf";
            // End of commented and added by vyankat b. on 03th Sep 2026 for name correcting
        }
        /* Previous:
        public static string BuildPdfFileName(SkillsInventoryExportRequest request)
        {
            string company = SafeFileNamePart(request?.CompanyName, "Whizible");
            return $"SkillsInventory_{company}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
        }
        */
        // End of BuildPdfFileName

    }
}
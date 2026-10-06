using ClosedXML.Excel;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Whizible26.Application.Repository.DashboardRepo;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.Application.CommandsQueries.Dashboard
{
    public class ForecastingDashboard
    {
        private readonly IConfiguration _configuration;
        private readonly ForecastingRepo _repository;
        private readonly string _connectionString;

        private const int ExportAllRowsPageSize = 100000;

        public ForecastingDashboard() { }

        public ForecastingDashboard(IConfiguration configuration)
        {
            _configuration = configuration;
            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(
                    _configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);
                _repository = new ForecastingRepo(_connectionString);
            }
        }

        #region Helpers

        private ResponseEntity RepositoryNotConfigured()
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message = "Database connection is not configured." }
            };
        }

        private static string? NullIfBlank(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            return trimmed.Equals("string", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
        }

        private static ResponseEntity Success(string message, object data)
        {
            return new ResponseEntity { Status = ResponseStatus.SUCCESS, Data = new { message, data } };
        }

        private static ResponseEntity Failure(Exception ex)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message = ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "") }
            };
        }

        private static List<SqlParameter> BuildFilterParameters(
            ForecastingFilterRequest request, int? userId, int? pageSizeOverride = null)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@intUserID", (object?)userId ?? DBNull.Value),
                new SqlParameter("@RoleID", (object?)request.RoleID ?? DBNull.Value),
                new SqlParameter("@BusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
                new SqlParameter("@LocationID", (object?)request.LocationID ?? DBNull.Value),
                new SqlParameter("@DepartmentID", (object?)request.DepartmentID ?? DBNull.Value),
                new SqlParameter("@SearchText", (object?)NullIfBlank(request.SearchText) ?? DBNull.Value),
                new SqlParameter("@PageNumber", request.PageNumber <= 0 ? 1 : request.PageNumber),
                new SqlParameter("@PageSize", pageSizeOverride ?? (request.PageSize <= 0 ? 10 : request.PageSize)),
            };
        }

        private static string BandUtilization(double utilizationPercent)
        {
            if (utilizationPercent < 70.0) return "Under-utilized";
            if (utilizationPercent > 100.0) return "Over-utilized";
            return "Healthy";
        }

        // FIX: The parameter is now 'object' instead of 'dynamic' to stop dynamic poisoning
        private static List<T> Extract<T>(object result)
        {
            var dict = (IDictionary<string, object>)result;
            return dict.TryGetValue(typeof(T).Name, out var value) && value is List<T> list
                ? list
                : new List<T>();
        }

        private static ForecastingKpiSummaryModel BuildKpiSummary(ForecastingKpiRawModel raw)
        {
            var utilization = raw.TotalPlannedHours > 0
                ? Math.Round(raw.TotalActualHours / raw.TotalPlannedHours * 100.0, 2)
                : 0.0;

            return new ForecastingKpiSummaryModel
            {
                TotalResources = raw.TotalResources,
                TotalResourcesCompanyWide = raw.TotalResourcesCompanyWide,
                TotalPlannedHours = raw.TotalPlannedHours,
                TotalActualHours = raw.TotalActualHours,
                UtilizationPercent = utilization,
                UtilizationLabel = BandUtilization(utilization),
                TotalRecords = raw.TotalRecords
            };
        }

        #endregion

        public async Task<ResponseEntity> GetFilterMasters()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var result = await _repository.GetAsyncSP<
                        ForecastingBusinessGroupModel,
                        ForecastingOriginationUnitModel,
                        ForecastingDepartmentModel,
                        ForecastingRoleModel>(
                    "usp_Whizible2_Sel_ForeCastingFilterMasters_dashboard", new List<SqlParameter>());

                return Success("Forecasting filter masters", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        public async Task<ResponseEntity> GetMonthlyForecast(ForecastingFilterRequest request, int? userId = null)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ForecastingFilterRequest();

                var sqlParams = BuildFilterParameters(request, userId);

                dynamic raw = await _repository.GetAsyncSP<
                        ForecastingKpiRawModel,
                        ForecastingMonthColumnModel,
                        ForecastingMonthRowModel>(
                    "usp_Whizible2_ForeCastingReport_dashboard", sqlParams);

                // FIX: Explicit strong typing and casting to object prevents dynamic dispatch cascade
                List<ForecastingKpiRawModel> kpiRaw = Extract<ForecastingKpiRawModel>((object)raw);
                List<ForecastingMonthColumnModel> monthColumns = Extract<ForecastingMonthColumnModel>((object)raw);
                List<ForecastingMonthRowModel> rows = Extract<ForecastingMonthRowModel>((object)raw);

                var kpi = BuildKpiSummary(kpiRaw.FirstOrDefault() ?? new ForecastingKpiRawModel());

                var response = new ForecastingMonthlyResponseModel
                {
                    Kpi = kpi,
                    MonthColumns = monthColumns,
                    Grid = new PagedResult<ForecastingMonthRowModel>
                    {
                        Rows = rows,
                        PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber,
                        PageSize = request.PageSize <= 0 ? 10 : request.PageSize,
                        TotalRecords = kpi.TotalRecords
                    }
                };

                return Success("Forecasting monthly grid", response);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        public async Task<ResponseEntity> GetWeeklyForecast(ForecastingFilterRequest request, int? userId = null)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ForecastingFilterRequest();

                var sqlParams = BuildFilterParameters(request, userId);

                dynamic raw = await _repository.GetAsyncSP<
                        ForecastingKpiRawModel,
                        ForecastingWeekColumnModel,
                        ForecastingWeekRowModel>(
                    "usp_Whizible2_ForeCastingReportWeeks_dashboard", sqlParams);

                // FIX: Explicit strong typing and casting to object prevents dynamic dispatch cascade
                List<ForecastingKpiRawModel> kpiRaw = Extract<ForecastingKpiRawModel>((object)raw);
                List<ForecastingWeekColumnModel> weekColumns = Extract<ForecastingWeekColumnModel>((object)raw);
                List<ForecastingWeekRowModel> rows = Extract<ForecastingWeekRowModel>((object)raw);

                var kpi = BuildKpiSummary(kpiRaw.FirstOrDefault() ?? new ForecastingKpiRawModel());

                var response = new ForecastingWeeklyResponseModel
                {
                    Kpi = kpi,
                    WeekColumns = weekColumns,
                    Grid = new PagedResult<ForecastingWeekRowModel>
                    {
                        Rows = rows,
                        PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber,
                        PageSize = request.PageSize <= 0 ? 10 : request.PageSize,
                        TotalRecords = kpi.TotalRecords
                    }
                };

                return Success("Forecasting weekly grid", response);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        public async Task<byte[]?> ExportForecastingExcel(ForecastingExportRequest request, bool isWeekly, int? userId = null)
        {
            if (_repository == null) throw new InvalidOperationException("Database connection is not configured.");
            if (request == null) request = new ForecastingExportRequest();

            var sqlParams = BuildFilterParameters(request, userId, ExportAllRowsPageSize);

            using var workbook = new XLWorkbook();

            if (isWeekly)
            {
                dynamic raw = await _repository.GetAsyncSP<
                        ForecastingKpiRawModel, ForecastingWeekColumnModel, ForecastingWeekRowModel>(
                    "usp_Whizible2_ForeCastingReportWeeks_dashboard", sqlParams);

                List<ForecastingWeekColumnModel> weekColumns = Extract<ForecastingWeekColumnModel>((object)raw);
                List<ForecastingWeekRowModel> rows = Extract<ForecastingWeekRowModel>((object)raw);

                if (rows == null || rows.Count == 0) return null;

                var ws = workbook.AddWorksheet("Forecasting Weekly");
                int col = 1;
                ws.Cell(1, col++).Value = "Resource Name";
                ws.Cell(1, col++).Value = "Role";
                ws.Cell(1, col++).Value = "Department";
                foreach (var wk in weekColumns)
                {
                    ws.Cell(1, col++).Value = $"W{wk.WKNo} Actual";
                    ws.Cell(1, col++).Value = $"W{wk.WKNo} Planned";
                }
                ws.Row(1).Style.Font.Bold = true;

                int row = 2;
                foreach (var r in rows)
                {
                    col = 1;
                    ws.Cell(row, col++).Value = r.ResourceName;
                    ws.Cell(row, col++).Value = r.RoleDescription;
                    ws.Cell(row, col++).Value = r.Department;
                    var actuals = GetWeekValues(r);

                    // FIX: This will now compile because `actuals` is an IEnumerable<(double, double)> not dynamic
                    foreach (var (actual, planned) in actuals)
                    {
                        ws.Cell(row, col++).Value = actual;
                        ws.Cell(row, col++).Value = planned;
                    }
                    row++;
                }

                ws.SheetView.FreezeRows(1);
                ws.Columns().AdjustToContents();
            }
            else
            {
                dynamic raw = await _repository.GetAsyncSP<
                        ForecastingKpiRawModel, ForecastingMonthColumnModel, ForecastingMonthRowModel>(
                    "usp_Whizible2_ForeCastingReport_dashboard", sqlParams);

                List<ForecastingMonthColumnModel> monthColumns = Extract<ForecastingMonthColumnModel>((object)raw);
                List<ForecastingMonthRowModel> rows = Extract<ForecastingMonthRowModel>((object)raw);

                if (rows == null || rows.Count == 0) return null;

                var ws = workbook.AddWorksheet("Forecasting Monthly");
                int col = 1;
                ws.Cell(1, col++).Value = "Resource Name";
                ws.Cell(1, col++).Value = "Role";
                ws.Cell(1, col++).Value = "Department";
                var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                foreach (var m in monthNames)
                {
                    ws.Cell(1, col++).Value = $"{m} Actual";
                    ws.Cell(1, col++).Value = $"{m} Planned";
                }
                ws.Row(1).Style.Font.Bold = true;

                int row = 2;
                foreach (var r in rows)
                {
                    col = 1;
                    ws.Cell(row, col++).Value = r.ResourceName;
                    ws.Cell(row, col++).Value = r.RoleDescription;
                    ws.Cell(row, col++).Value = r.Department;

                    // FIX: This compiles correctly now because GetMonthValues returns strongly typed tuples
                    foreach (var (actual, planned) in GetMonthValues(r))
                    {
                        ws.Cell(row, col++).Value = actual;
                        ws.Cell(row, col++).Value = planned;
                    }
                    row++;
                }

                ws.SheetView.FreezeRows(1);
                ws.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]?> ExportForecastingPdf(ForecastingExportRequest request, bool isWeekly, int? userId = null)
        {
            if (_repository == null) throw new InvalidOperationException("Database connection is not configured.");
            if (request == null) request = new ForecastingExportRequest();

            var sqlParams = BuildFilterParameters(request, userId, ExportAllRowsPageSize);

            var document = new Document();
            document.Info.Title = isWeekly ? "Forecasting Report - Weekly" : "Forecasting Report - Monthly";

            var section = document.AddSection();
            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.TopMargin = Unit.FromCentimeter(5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1);

            var headerTable = section.Headers.Primary.AddTable();
            headerTable.AddColumn(Unit.FromCentimeter(3));
            headerTable.AddColumn(Unit.FromCentimeter(20));

            var headerRow = headerTable.AddRow();
            headerRow.VerticalAlignment = VerticalAlignment.Center;

            byte[]? logoBytes = null;
            if (!string.IsNullOrEmpty(request.CompanyLogo))
            {
                var base64 = request.CompanyLogo.Substring(request.CompanyLogo.IndexOf(",") + 1);
                logoBytes = Convert.FromBase64String(base64);
            }
            if (logoBytes != null)
            {
                var imagePath = SaveTempImage(logoBytes);
                headerRow.Cells[0].AddImage(imagePath).LockAspectRatio = true;
            }

            headerRow.Cells[1].AddParagraph(document.Info.Title).Format.Font.Size = 12;
            headerRow.Cells[1].Format.Alignment = ParagraphAlignment.Center;
            section.AddParagraph().AddLineBreak();

            if (isWeekly)
            {
                dynamic raw = await _repository.GetAsyncSP<
                        ForecastingKpiRawModel, ForecastingWeekColumnModel, ForecastingWeekRowModel>(
                    "usp_Whizible2_ForeCastingReportWeeks_dashboard", sqlParams);

                List<ForecastingWeekColumnModel> weekColumns = Extract<ForecastingWeekColumnModel>((object)raw);
                List<ForecastingWeekRowModel> rows = Extract<ForecastingWeekRowModel>((object)raw);

                if (rows == null || rows.Count == 0) return null;

                var table = section.AddTable();
                table.Borders.Width = 0.5;
                table.AddColumn(Unit.FromCentimeter(3.5));
                table.AddColumn(Unit.FromCentimeter(2.5));
                foreach (var _ in weekColumns) table.AddColumn(Unit.FromCentimeter(1.4));

                var header = table.AddRow();
                header.Shading.Color = Colors.LightGray;
                header.Format.Font.Bold = true;
                header.Format.Font.Size = 8;
                header.Cells[0].AddParagraph("Resource Name");
                header.Cells[1].AddParagraph("Role");
                var wc = 2;
                foreach (var wk in weekColumns)
                    header.Cells[wc++].AddParagraph($"W{wk.WKNo}");

                foreach (var r in rows)
                {
                    var row = table.AddRow();
                    row.Format.Font.Size = 8;
                    row.Cells[0].AddParagraph(r.ResourceName);
                    row.Cells[1].AddParagraph(r.RoleDescription);
                    var c = 2;

                    // FIX: Strongly typed tuple avoids CS8133
                    foreach (var (actual, _) in GetWeekValues(r))
                        row.Cells[c++].AddParagraph(actual.ToString("0.0"));
                }
            }
            else
            {
                dynamic raw = await _repository.GetAsyncSP<
                        ForecastingKpiRawModel, ForecastingMonthColumnModel, ForecastingMonthRowModel>(
                    "usp_Whizible2_ForeCastingReport_dashboard", sqlParams);

                List<ForecastingMonthColumnModel> monthColumns = Extract<ForecastingMonthColumnModel>((object)raw);
                List<ForecastingMonthRowModel> rows = Extract<ForecastingMonthRowModel>((object)raw);

                if (rows == null || rows.Count == 0) return null;

                var table = section.AddTable();
                table.Borders.Width = 0.5;
                table.AddColumn(Unit.FromCentimeter(4));
                table.AddColumn(Unit.FromCentimeter(2.5));
                for (int i = 0; i < 12; i++) table.AddColumn(Unit.FromCentimeter(1.3));

                var header = table.AddRow();
                header.Shading.Color = Colors.LightGray;
                header.Format.Font.Bold = true;
                header.Format.Font.Size = 8;
                header.Cells[0].AddParagraph("Resource Name");
                header.Cells[1].AddParagraph("Role");
                var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                for (int i = 0; i < monthNames.Length; i++)
                    header.Cells[2 + i].AddParagraph(monthNames[i]);

                foreach (var r in rows)
                {
                    var row = table.AddRow();
                    row.Format.Font.Size = 8;
                    row.Cells[0].AddParagraph(r.ResourceName);
                    row.Cells[1].AddParagraph(r.RoleDescription);

                    // FIX: Lambda works correctly now because GetMonthValues returns a known type
                    var values = GetMonthValues(r).Select(v => v.actual).ToArray();
                    for (int i = 0; i < values.Length; i++)
                        row.Cells[2 + i].AddParagraph(values[i].ToString("0.0"));
                }
            }

            var footer = section.Footers.Primary.AddParagraph();
            footer.Format.Font.Size = 8;
            footer.AddText(DateTime.Now.ToString("dd/MM/yyyy"));
            footer.AddTab();
            footer.AddText("Page ");
            footer.AddPageField();
            footer.AddText(" of ");
            footer.AddNumPagesField();
            footer.Format.Alignment = ParagraphAlignment.Right;

            var renderer = new PdfDocumentRenderer(true) { Document = document };
            renderer.RenderDocument();

            using var stream = new MemoryStream();
            renderer.PdfDocument.Save(stream, false);
            return stream.ToArray();
        }

        private static string SaveTempImage(byte[] imageBytes)
        {
            var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
            File.WriteAllBytes(path, imageBytes);
            return path;
        }

        // FIX: Return type is double
        private static IEnumerable<(double actual, double planned)> GetMonthValues(ForecastingMonthRowModel r)
        {
            yield return (r.January, r.January_Ph);
            yield return (r.February, r.February_Ph);
            yield return (r.March, r.March_Ph);
            yield return (r.April, r.April_Ph);
            yield return (r.May, r.May_Ph);
            yield return (r.June, r.June_Ph);
            yield return (r.July, r.July_Ph);
            yield return (r.August, r.August_Ph);
            yield return (r.September, r.September_Ph);
            yield return (r.October, r.October_Ph);
            yield return (r.November, r.November_Ph);
            yield return (r.December, r.December_Ph);
        }

        // FIX: Return type is double
        private static IEnumerable<(double actual, double planned)> GetWeekValues(ForecastingWeekRowModel r)
        {
            yield return (r.W1, r.W1_Ah); yield return (r.W2, r.W2_Ah); yield return (r.W3, r.W3_Ah);
            yield return (r.W4, r.W4_Ah); yield return (r.W5, r.W5_Ah); yield return (r.W6, r.W6_Ah);
            yield return (r.W7, r.W7_Ah); yield return (r.W8, r.W8_Ah); yield return (r.W9, r.W9_Ah);
            yield return (r.W10, r.W10_Ah); yield return (r.W11, r.W11_Ah); yield return (r.W12, r.W12_Ah);
            yield return (r.W13, r.W13_Ah); yield return (r.W14, r.W14_Ah); yield return (r.W15, r.W15_Ah);
            yield return (r.W16, r.W16_Ah); yield return (r.W17, r.W17_Ah); yield return (r.W18, r.W18_Ah);
            yield return (r.W19, r.W19_Ah); yield return (r.W20, r.W20_Ah); yield return (r.W21, r.W21_Ah);
            yield return (r.W22, r.W22_Ah); yield return (r.W23, r.W23_Ah); yield return (r.W24, r.W24_Ah);
            yield return (r.W25, r.W25_Ah); yield return (r.W26, r.W26_Ah); yield return (r.W27, r.W27_Ah);
            yield return (r.W28, r.W28_Ah); yield return (r.W29, r.W29_Ah); yield return (r.W30, r.W30_Ah);
            yield return (r.W31, r.W31_Ah); yield return (r.W32, r.W32_Ah); yield return (r.W33, r.W33_Ah);
            yield return (r.W34, r.W34_Ah); yield return (r.W35, r.W35_Ah); yield return (r.W36, r.W36_Ah);
            yield return (r.W37, r.W37_Ah); yield return (r.W38, r.W38_Ah); yield return (r.W39, r.W39_Ah);
            yield return (r.W40, r.W40_Ah); yield return (r.W41, r.W41_Ah); yield return (r.W42, r.W42_Ah);
            yield return (r.W43, r.W43_Ah); yield return (r.W44, r.W44_Ah); yield return (r.W45, r.W45_Ah);
            yield return (r.W46, r.W46_Ah); yield return (r.W47, r.W47_Ah); yield return (r.W48, r.W48_Ah);
            yield return (r.W49, r.W49_Ah); yield return (r.W50, r.W50_Ah); yield return (r.W51, r.W51_Ah);
            yield return (r.W52, r.W52_Ah); yield return (r.W53, r.W53_Ah);
        }
    }
}
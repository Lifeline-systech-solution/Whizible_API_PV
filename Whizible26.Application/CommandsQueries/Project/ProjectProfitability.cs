using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Reflection.Metadata; // Commented out to avoid conflict with MigraDoc.DocumentObjectModel.Document
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.Repository.ProjectRepo.Execute;
using Whizible26.Domain.Entity.ProjectEntities.Execute;
using Whizible26.Domain.Entity.ProjectEntities.ProjectProfiEntity;
using WhizibleTeams.Domain.Entity;


using ClosedXML.Excel;
using System.Data;
using Whizible26.Application.Repository.ProjectRepo;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using System.IO;


namespace Whizible26.Application.CommandsQueries
{
    public class ProjectProfitability
    {
        private readonly IConfiguration _configuration;
        private readonly ProjectProfiRepo _repository;
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Added By Vyankat B On 15-12-2025 For default constructor
        public ProjectProfitability() { }
        // End of Added By Vyankat B On 15-12-2025 For default constructor

        // Added By Vyankat B On 15-12-2025 For initializing Project Profitability with configuration
        public ProjectProfitability(IConfiguration configuration, IWebHostEnvironment webHostEnvironment = null)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(_configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);
                _repository = new ProjectProfiRepo(_connectionString);
            }
        }
        // End of Added By Vyankat B On 15-12-20255 For initializing Project Profitability with configuration

        public async Task<ResponseEntity> GetProjectDetail(ProjectDetailRequest request)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
                {
                new SqlParameter("@intProjectId", request.ProjectID ?? (object)DBNull.Value)
                };


                var result = await _repository.GetAsyncSP<ProjectDetailModel>("usp_Whizible2_Sel_PROJECTDETAIL", sqlParams);


                //var list = result.ProjectDetailModel; // Repo returns dataset -> map name accordingly in repo


                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Project Detail",
                        data = result//list
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = new { message = ex.Message }
                };
            }
        }

        // Added by Vyankat Bhure on 12-12-2025 - Get Project Profitability Date Range (FromDate and ToDate from Status = 1 rows)
        public async Task<ResponseEntity> GetProjectProfitabilityDateRange(ProjectDateRangeRequest request)
        {
            try
            {
                if (request == null || request.ProjectID <= 0)
                {
                    return new ResponseEntity
                    {
                        Status = ResponseStatus.FAILURE,
                        Data = new { message = "ProjectID is required and must be greater than 0." }
                    };
                }

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@ProjectID", request.ProjectID)
                };

                var result = await _repository.GetAsyncSP<ProjectDateRangeResponse>(
                    "usp_Whizible2_SEL_Date_Tbl_PM_ProjectProfitability",
                    sqlParams
                );

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Project Profitability Date Range retrieved successfully",
                        data = result
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = new { message = ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "") }
                };
            }
        }
        // End of Added by Vyankat Bhure on 12-12-2025 - Get Project Profitability Date Range
     
        // Added by Vyankat Bhure on 15-12-2025
        // Business logic to fetch Project Profitability report data (Flag-wise models)
        public async Task<ResponseEntity> GetProjectProfitability(ProjectProfitabilityRequest request)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
{
     new SqlParameter("@ProjectID", request.ProjectID),
     new SqlParameter("@FromDate", request.FromDate ?? (object)DBNull.Value),
     new SqlParameter("@ToDate", request.ToDate ?? (object)DBNull.Value),
     new SqlParameter("@ReportFlag", request.ReportFlag)
};

                object result;

                // Flag 1 – Periodic Summary View
                if (request.ReportFlag == 1)
                {
                    result = await _repository.GetAsyncSP<ProjectProfitabilityPeriodicResponse>(
                        "usp_Whizible2_Sel_ProjProfitability",
                        sqlParams
                    );
                }
                // Flag 2 – Detailed (Periodic + Cumulative) View
                else if (request.ReportFlag == 2)
                {
                    result = await _repository.GetAsyncSP<ProjectProfitabilityDetailedResponse>(
                        "usp_Whizible2_Sel_ProjProfitability",
                        sqlParams
                    );
                }
                // Flag 3 – Cumulative Revenue / Cost / GPM View
                else if (request.ReportFlag == 3)
                {
                    result = await _repository.GetAsyncSP<ProjectProfitabilityCumulativeResponse>(
                        "usp_Whizible2_Sel_ProjProfitability",
                        sqlParams
                    );
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
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error fetching project profitability data: " + ex.Message +
                           (ex.InnerException != null
                               ? " Inner: " + ex.InnerException.Message
                               : string.Empty)
                };
            }
        }
        // End of GetProjectProfitability business logic

        // Added by Vyankat Bhure on 22-12-2025
        // Business logic for usp_Whizible2_Sel_ProjectProfitability_PeridGraphData
        public async Task<ResponseEntity> GetPeridGraphData(int projectId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@ProjectID", projectId),
                    new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
                    new SqlParameter("@ToDate",   toDate   ?? (object)DBNull.Value)
                };

                var result = await _repository.GetAsyncSP<ProjectProfitabilityPeriodGraphDataModel>(
                    "usp_Whizible2_Sel_ProjectProfitability_PeridGraphData",
                    sqlParams
                );

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Project Profitability Period Graph Data",
                        data = result
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = new { message = ex.Message }
                };
            }
        }

        // Added by Vyankat Bhure on 22-12-2025 - Business logic for usp_Whizible2_Sel_ProjectProfitability_FinacialData
        // Updated: SP now only requires @ProjectID parameter (removed @FromDate and @ToDate)
        public async Task<ResponseEntity> GetFinancialData(int projectId)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@ProjectID", projectId)
                    // Note: @FromDate and @ToDate removed - SP now returns aggregated summary for all periods
                };

                var result = await _repository.GetAsyncSP<ProjectProfitabilityFinancialDataModel>(
                    "usp_Whizible2_Sel_ProjectProfitability_FinacialData",
                    sqlParams
                );

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Project Profitability Financial Data",
                        data = result
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = new { message = ex.Message }
                };
            }
        }

        // Added by Vyankat Bhure on 24-12-2025 - Business logic for usp_Whizible2_Sel_ProjectProfitability_Periods
        public async Task<ResponseEntity> GetProjectProfitabilityPeriods(ProjectProfitabilityPeriodsRequest request)
        {
            try
            {
                if (request.ProjectID <= 0)
                {
                    return new ResponseEntity
                    {
                        Status = ResponseStatus.FAILURE,
                        Data = "ProjectID is required and must be greater than 0."
                    };
                }

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@ProjectID", request.ProjectID),
                    new SqlParameter("@WhichDate", string.IsNullOrWhiteSpace(request.WhichDate) ? "F" : request.WhichDate)
                };

                var result = await _repository.GetAsyncSP<ProjectProfitabilityPeriodsResponse>(
                    "usp_Whizible2_Sel_ProjectProfitability_Periods",
                    sqlParams
                );

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Project Profitability Periods retrieved successfully",
                        data = result
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error retrieving project profitability periods: " + ex.Message +
                           (ex.InnerException != null
                               ? " Inner: " + ex.InnerException.Message
                               : string.Empty)
                };
            }
        }
        // End of Added by Vyankat Bhure on 24-12-2025 - Business logic for usp_Whizible2_Sel_ProjectProfitability_Periods

        public async Task<ResponseEntity> GetTrends(int projectId, DateTime? fromDate, DateTime? toDate, int? graphId)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@ProjectID", projectId),
                    new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
                    new SqlParameter("@ToDate",   toDate   ?? (object)DBNull.Value),
                    new SqlParameter("@GraphId",  graphId  ?? (object)DBNull.Value)
                };

                //var result = await _repo.GetAsyncSP<ProjectProfitabilityTrendModel>(
                //    "usp_Whizible2_Sel_ProjectProfitabilityTrends",
                //    sqlParams
                //);

                object data = null;

                // pick entity based on graphId
                if (graphId == 1)
                {
                    var result = await _repository.GetAsyncSP<ProjectProfitabilityTrendGraph1Model>(
                        "usp_Whizible2_Sel_ProjectProfitabilityTrends",
                        sqlParams
                    );
                    data = result; // keep same shape — repository result assigned to data
                }
                else if (graphId == 2)
                {
                    var result = await _repository.GetAsyncSP<ProjectProfitabilityTrendGraph2Model>(
                        "usp_Whizible2_Sel_ProjectProfitabilityTrends",
                        sqlParams
                    );
                    data = result;
                }
                else // graphId == 3 or other
                {
                    var result = await _repository.GetAsyncSP<ProjectProfitabilityTrendGraph3Model>(
                        "usp_Whizible2_Sel_ProjectProfitabilityTrends",
                        sqlParams
                    );
                    data = result;
                }


                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Project Profitability Trends",
                        data = data // adjust property according to your repo return shape
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = new { message = ex.Message }
                };
            }
        }

        // Added by Vyankat Bhure - Generate Profitability PDF using MigraDoc
        public async Task<byte[]> GenerateProfitabilityPdf(int projectId, DateTime? fromDate, DateTime? toDate, byte[] logoBytes, string logoExtension = ".png")
        {
            // 1. Prepare Parameters
            var param = new List<SqlParameter>
    {
        new SqlParameter("@ProjectID", projectId),
        new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
        new SqlParameter("@ToDate",   toDate   ?? (object)DBNull.Value)
    };

            // 2. Repo Call
            var result = await _repository.GetAsyncSP<ProfitabilityReportData>(
                "usp_RPT_Tbl_PM_ProjectProfitability",
                param
            );

            // 3. Get Company Info
            var companyInfo = await _repository.GetAsyncSP<CompanyDataEntity>(
                            "usp_Whizible2_sel_tbl_PM_CompanyInformation",
                           new List<SqlParameter>()
                        );

            List<CompanyDataEntity> lst = companyInfo.CompanyDataEntity;
            String? CompanyName = lst[0].CompanyName;

            List<ProfitabilityReportData> data = result.ProfitabilityReportData;
            if (data == null || !data.Any())
                return null;

            var headerInfo = data.First();

            // Added by Vyankat Bhure on 20-02-2025 - Get ActualStartDate and ActualEndDate from GetProjectDetail
            DateTime? actualStartDate = null;
            DateTime? actualEndDate = null;
            try
            {
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectId", projectId)
                };
                var projectDetailResult = await _repository.GetAsyncSP<ProjectDetailModel>("usp_Whizible2_Sel_PROJECTDETAIL", sqlParams);
                if (projectDetailResult != null && projectDetailResult.ProjectDetailModel != null && projectDetailResult.ProjectDetailModel.Count > 0)
                {
                    var projectDetail = projectDetailResult.ProjectDetailModel[0];
                    actualStartDate = projectDetail.ActualStartDate;
                    actualEndDate = projectDetail.ActualEndDate;
                }
            }
            catch (Exception ex)
            {
                // If GetProjectDetail fails, continue with existing dates
                System.Diagnostics.Debug.WriteLine($"Error getting project detail: {ex.Message}");
            }
            // End of Added by Vyankat Bhure on 20-02-2025

            // 4. Create MigraDoc Document
            MigraDoc.DocumentObjectModel.Document document = new MigraDoc.DocumentObjectModel.Document();
            document.Info.Title = "Project Profitability Report";
            document.Info.Author = CompanyName;
            document.Info.Subject = "Project Profitability Report";

            // Define Styles
            Style style = document.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 7; // Reduced font size to fit all content

            Style headerStyle = document.Styles.AddStyle("Header", "Normal");
            headerStyle.Font.Size = 12; // Reduced from 16 to fit better
            headerStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            Style titleStyle = document.Styles.AddStyle("Title", "Normal");
            titleStyle.Font.Size = 10; // Reduced from 12 to fit better
            titleStyle.Font.Bold = true;
            titleStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            // 5. Create Section (Page) - Configure for Landscape orientation
            Section section = document.AddSection();
            // Use A3 landscape for more width to accommodate all columns
            // A3 Landscape: Width = 1191pt (42cm), Height = 842pt (29.7cm) - much wider than A4
            section.PageSetup.PageFormat = PageFormat.A3;
            section.PageSetup.Orientation = Orientation.Landscape;
            // Explicitly set landscape dimensions to ensure correct orientation
            section.PageSetup.PageWidth = Unit.FromPoint(1191); // A3 Landscape width (horizontal - wider)
            section.PageSetup.PageHeight = Unit.FromPoint(842); // A3 Landscape height (vertical)
            section.PageSetup.TopMargin = Unit.FromPoint(20);
            section.PageSetup.BottomMargin = Unit.FromPoint(20);
            section.PageSetup.LeftMargin = Unit.FromPoint(20);
            section.PageSetup.RightMargin = Unit.FromPoint(20);
            // Set header distance to prevent overlap with body content
            section.PageSetup.HeaderDistance = Unit.FromPoint(10);

            // 6. Add Header Content with Logo
            string tempLogoPath = null;
            
            // Create a header table for better layout control
            Table headerTable = section.Headers.Primary.AddTable();
            headerTable.AddColumn(Unit.FromPoint(200)); // Logo column - A3 has more width
            headerTable.AddColumn(Unit.FromPoint(950)); // Company name and title column - A3 landscape width ~1151pt
            
            Row headerRow = headerTable.AddRow();
            headerRow.VerticalAlignment = VerticalAlignment.Top;
            
            // Left cell: Logo
            if (logoBytes != null && logoBytes.Length > 0)
            {
                try
                {
                    // Save logo to temporary file for MigraDoc (it requires a file path)
                    string extension = !string.IsNullOrWhiteSpace(logoExtension) ? logoExtension : ".png";
                    if (!extension.StartsWith("."))
                        extension = "." + extension;
                    tempLogoPath = Path.Combine(Path.GetTempPath(), $"logo_{Guid.NewGuid()}{extension}");
                    File.WriteAllBytes(tempLogoPath, logoBytes);
                    
                    Paragraph logoPara = headerRow.Cells[0].AddParagraph();
                    MigraDoc.DocumentObjectModel.Shapes.Image logo = logoPara.AddImage(tempLogoPath);
                    logo.Width = Unit.FromPoint(120);
                    logo.Height = Unit.FromPoint(50);
                    logo.LockAspectRatio = true;
                    headerRow.Cells[0].VerticalAlignment = VerticalAlignment.Top;
                    headerRow.Cells[0].Format.LeftIndent = Unit.FromPoint(5);
                }
                catch (Exception ex)
                {
                    // If logo fails to load, continue without it
                    System.Diagnostics.Debug.WriteLine($"Logo loading error: {ex.Message}");
                    headerRow.Cells[0].AddParagraph(""); // Empty cell if logo fails
                }
            }
            else
            {
                headerRow.Cells[0].AddParagraph(""); // Empty cell if no logo
            }
            
            // Right cell: Company name and title - centered
            Paragraph companyPara = headerRow.Cells[1].AddParagraph();
            companyPara.AddText(CompanyName ?? "");
            companyPara.Format.Alignment = ParagraphAlignment.Center;
            companyPara.Format.Font.Size = 11; // Reduced but readable
            companyPara.Format.Font.Bold = true;
            companyPara.Format.Font.Underline = Underline.Single;
            companyPara.Format.SpaceAfter = Unit.FromPoint(5);
            companyPara.Format.SpaceBefore = Unit.FromPoint(0);
            
            Paragraph titlePara = headerRow.Cells[1].AddParagraph();
            titlePara.AddText("Project Profitability Report");
            titlePara.Format.Alignment = ParagraphAlignment.Center;
            titlePara.Format.Font.Size = 9; // Reduced but readable
            titlePara.Format.Font.Bold = true;
            titlePara.Format.SpaceAfter = Unit.FromPoint(0);
            
            headerRow.Cells[1].VerticalAlignment = VerticalAlignment.Top;
            headerRow.Cells[1].Format.LeftIndent = Unit.FromPoint(0);
            headerRow.Cells[1].Format.Alignment = ParagraphAlignment.Center;

            // 7. Add proper spacing after header table - significantly increased to prevent overlap with logo
            Paragraph headerSpacer = section.AddParagraph();
            headerSpacer.Format.SpaceAfter = Unit.FromPoint(50); // Significantly increased to push Project Details below logo

            // 8. Add Project Details Section Title
            Paragraph projectDetailsTitle = section.AddParagraph();
            projectDetailsTitle.AddText("Project Details");
            projectDetailsTitle.Format.Alignment = ParagraphAlignment.Left;
            projectDetailsTitle.Format.Font.Size = 9; // Reduced from 12
            projectDetailsTitle.Format.Font.Bold = true;
            projectDetailsTitle.Format.SpaceAfter = Unit.FromPoint(5);
            projectDetailsTitle.Format.SpaceBefore = Unit.FromPoint(15); // Increased spacing before title

            // 9. Add Project Details Table
            Table projectTable = section.AddTable();
            projectTable.Style = "Table";
            projectTable.Borders.Color = Colors.Black;
            projectTable.Borders.Width = 0.25;
            projectTable.Borders.Left.Width = 0.5;
            projectTable.Borders.Right.Width = 0.5;
            projectTable.Rows.LeftIndent = 0;
            projectTable.Format.SpaceAfter = Unit.FromPoint(15);

            // Define columns - adjust widths for better fit
            Column col1 = projectTable.AddColumn(Unit.FromPoint(130));
            Column col2 = projectTable.AddColumn(Unit.FromPoint(200));
            Column col3 = projectTable.AddColumn(Unit.FromPoint(130));
            Column col4 = projectTable.AddColumn(Unit.FromPoint(200));

            // Add rows with proper formatting
            Row row = projectTable.AddRow();
            row.Cells[0].AddParagraph("Project Name:").Format.Font.Bold = true;
            row.Cells[0].Format.LeftIndent = Unit.FromPoint(5);
            row.Cells[1].AddParagraph(headerInfo.ProjectName ?? "");
            row.Cells[2].AddParagraph("Project Value:").Format.Font.Bold = true;
            row.Cells[2].Format.LeftIndent = Unit.FromPoint(5);
            row.Cells[3].AddParagraph($"{headerInfo.CurrencySymbol} {headerInfo.ProjectValue:N2}");

            row = projectTable.AddRow();
            row.Cells[0].AddParagraph("Start Date:").Format.Font.Bold = true;
            row.Cells[0].Format.LeftIndent = Unit.FromPoint(5);
            row.Cells[1].AddParagraph(headerInfo.ExpectedStartDate?.ToString("dd-MMM-yyyy") ?? "");
            row.Cells[2].AddParagraph("End Date:").Format.Font.Bold = true;
            row.Cells[2].Format.LeftIndent = Unit.FromPoint(5);
            row.Cells[3].AddParagraph(headerInfo.ExpectedEndDate?.ToString("dd-MMM-yyyy") ?? "");

            row = projectTable.AddRow();
            row.Cells[0].AddParagraph("Actual Start Date:").Format.Font.Bold = true;
            row.Cells[0].Format.LeftIndent = Unit.FromPoint(5);
            // Added by Vyankat Bhure on 20-02-2025 - Use ActualStartDate from GetProjectDetail instead of headerInfo.ActualStartDate
            row.Cells[1].AddParagraph(actualStartDate?.ToString("dd-MMM-yyyy") ?? headerInfo.ActualStartDate?.ToString("dd-MMM-yyyy") ?? "");
            // End of Added by Vyankat Bhure on 20-02-2025
            row.Cells[2].AddParagraph("Actual End Date:").Format.Font.Bold = true;
            row.Cells[2].Format.LeftIndent = Unit.FromPoint(5);
            // Added by Vyankat Bhure on 20-02-2025 - Use ActualEndDate from GetProjectDetail instead of headerInfo.ActualEndDate
            row.Cells[3].AddParagraph(actualEndDate?.ToString("dd-MMM-yyyy") ?? headerInfo.ActualEndDate?.ToString("dd-MMM-yyyy") ?? "");
            // End of Added by Vyankat Bhure on 20-02-2025

            row = projectTable.AddRow();
            row.Cells[0].AddParagraph("Project Currency:").Format.Font.Bold = true;
            row.Cells[0].Format.LeftIndent = Unit.FromPoint(5);
            row.Cells[1].AddParagraph(headerInfo.CurrencySymbol ?? "");
            row.Cells[2].AddParagraph("");
            row.Cells[3].AddParagraph("");

            // 10. Add spacing before main data table
            Paragraph dataTableSpacer = section.AddParagraph();
            dataTableSpacer.Format.SpaceAfter = Unit.FromPoint(10);

            // 11. Add Main Data Table
            Table dataTable = section.AddTable();
            dataTable.Style = "Table";
            dataTable.Borders.Color = Colors.Black;
            dataTable.Borders.Width = 0.25;
            dataTable.Borders.Left.Width = 0.5;
            dataTable.Borders.Right.Width = 0.5;
            dataTable.Rows.LeftIndent = 0;

            // Define 15 columns (As On + 14 data columns) - matching desired format
            // Landscape A3 width: 1191pt, usable width: 1151pt (with 20pt margins)
            // As On = 50pt, Data columns = 50pt each
            // Total: 50 + (14 × 50) = 50 + 700 = 750pt (fits within 1151pt)
            dataTable.AddColumn(Unit.FromPoint(53)); // As On column
            for (int i = 0; i < 14; i++)
            {
                dataTable.AddColumn(Unit.FromPoint(53)); // Data columns
            }

            // Add Header Row
            Row dataHeaderRow = dataTable.AddRow();
            dataHeaderRow.HeadingFormat = true;
            dataHeaderRow.Format.Alignment = ParagraphAlignment.Center;
            dataHeaderRow.Format.Font.Bold = true;
            dataHeaderRow.Format.Font.Size = 7; // Reduced font size for table headers
            dataHeaderRow.Shading.Color = Colors.LightGray;
            // Increase header row height
            dataHeaderRow.HeightRule = RowHeightRule.AtLeast;
            dataHeaderRow.Height = Unit.FromPoint(25); // Increased row height
            // Add vertical padding to header cells
            for (int i = 0; i < 15; i++)
            {
                dataHeaderRow.Cells[i].Format.SpaceBefore = Unit.FromPoint(5);
                dataHeaderRow.Cells[i].Format.SpaceAfter = Unit.FromPoint(5);
                dataHeaderRow.Cells[i].VerticalAlignment = VerticalAlignment.Center;
            }

            dataHeaderRow.Cells[0].AddParagraph("As On");
            dataHeaderRow.Cells[1].AddParagraph("Periodic\nActual Hours");
            dataHeaderRow.Cells[2].AddParagraph("Cumulative\nActual Hours");
            dataHeaderRow.Cells[3].AddParagraph("Periodic\nPeople Cost");
            dataHeaderRow.Cells[4].AddParagraph("Cumulative\nPeople Cost");
            dataHeaderRow.Cells[5].AddParagraph("Periodic\nExpenses");
            dataHeaderRow.Cells[6].AddParagraph("Cumulative\nExpenses");
            dataHeaderRow.Cells[7].AddParagraph("Periodic Accrued\nPeople Revenue");
            dataHeaderRow.Cells[8].AddParagraph("Cumulative Accrued\nPeople Revenue");
            dataHeaderRow.Cells[9].AddParagraph("Periodic Accrued\nBillable Expenses");
            dataHeaderRow.Cells[10].AddParagraph("Cumulative Accrued\nBillable Expenses");
            dataHeaderRow.Cells[11].AddParagraph("Periodic\nGPM");
            dataHeaderRow.Cells[12].AddParagraph("Cumulative\nGPM");
            dataHeaderRow.Cells[13].AddParagraph("Periodic\nInvoice Revenue");
            dataHeaderRow.Cells[14].AddParagraph("Cumulative\nInvoice Revenue");

            // Add Data Rows
                        foreach (var r in data)
                        {
                Row dataRow = dataTable.AddRow();
                // Increase data row height
                dataRow.HeightRule = RowHeightRule.AtLeast;
                dataRow.Height = Unit.FromPoint(20); // Increased row height
                // Set reduced font size and add vertical padding for all data cells
                for (int i = 0; i < 15; i++)
                {
                    dataRow.Cells[i].Format.Font.Size = 7; // Reduced font size for all data columns
                    dataRow.Cells[i].Format.SpaceBefore = Unit.FromPoint(4);
                    dataRow.Cells[i].Format.SpaceAfter = Unit.FromPoint(4);
                    dataRow.Cells[i].VerticalAlignment = VerticalAlignment.Center;
                }
                // As On date column - Use ToDate since stored procedure returns ToDate as the period end date
                // Format: dd-MMM-yyyy (e.g., "03-Aug-2025") to match project details format
                string asOnDate = (r.ToDate != default(DateTime) && r.ToDate != DateTime.MinValue) 
                    ? r.ToDate.ToString("dd-MMM-yyyy") 
                    : (r.AsOn != default(DateTime) && r.AsOn != DateTime.MinValue) 
                        ? r.AsOn.ToString("dd-MMM-yyyy") 
                        : "";
                dataRow.Cells[0].AddParagraph(asOnDate);
                dataRow.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                // Actual Hours
                dataRow.Cells[1].AddParagraph(r.PeriodicAccruedActualHoursTotal.ToString("N2"));
                dataRow.Cells[1].Format.Alignment = ParagraphAlignment.Right;

                dataRow.Cells[2].AddParagraph(r.CumulativeAccruedActualHoursTotal.ToString("N2"));
                dataRow.Cells[2].Format.Alignment = ParagraphAlignment.Right;

                // People Cost
                dataRow.Cells[3].AddParagraph(r.PeriodicAccruedResourceCostTotal.ToString("N2"));
                dataRow.Cells[3].Format.Alignment = ParagraphAlignment.Right;

                dataRow.Cells[4].AddParagraph(r.CumulativeAccruedResourceCostTotal.ToString("N2"));
                dataRow.Cells[4].Format.Alignment = ParagraphAlignment.Right;

                // Expenses (moved before Accrued People Revenue to match desired format)
                dataRow.Cells[5].AddParagraph(r.PeriodicOtherCostTotal.ToString("N2"));
                dataRow.Cells[5].Format.Alignment = ParagraphAlignment.Right;

                dataRow.Cells[6].AddParagraph(r.CumulativeOtherCostTotal.ToString("N2"));
                dataRow.Cells[6].Format.Alignment = ParagraphAlignment.Right;

                // Accrued People Revenue
                dataRow.Cells[7].AddParagraph(r.PeriodicAccruedResourceBillingTotal.ToString("N2"));
                dataRow.Cells[7].Format.Alignment = ParagraphAlignment.Right;

                dataRow.Cells[8].AddParagraph(r.CumulativeAccruedResourceBillingTotal.ToString("N2"));
                dataRow.Cells[8].Format.Alignment = ParagraphAlignment.Right;

                // Accrued Billable Expenses
                dataRow.Cells[9].AddParagraph(r.PeriodicBillableOtherCostTotal.ToString("N2"));
                dataRow.Cells[9].Format.Alignment = ParagraphAlignment.Right;

                dataRow.Cells[10].AddParagraph(r.CumulativeBillableOtherCostTotal.ToString("N2"));
                dataRow.Cells[10].Format.Alignment = ParagraphAlignment.Right;

                // GPM with color (red if negative)
                Paragraph gpmPara1 = dataRow.Cells[11].AddParagraph(r.PeriodicGPM_Project.ToString("N2"));
                gpmPara1.Format.Alignment = ParagraphAlignment.Right;
                if (r.PeriodicGPM_Project < 0)
                {
                    gpmPara1.Format.Font.Color = Colors.Red;
                }

                Paragraph gpmPara2 = dataRow.Cells[12].AddParagraph(r.CumulativeGPM_Project.ToString("N2"));
                gpmPara2.Format.Alignment = ParagraphAlignment.Right;
                if (r.CumulativeGPM_Project < 0)
                {
                    gpmPara2.Format.Font.Color = Colors.Red;
                }

                // Invoice Revenue
                dataRow.Cells[13].AddParagraph(r.PeriodicInvoiceBillingTotal.ToString("N2"));
                dataRow.Cells[13].Format.Alignment = ParagraphAlignment.Right;

                dataRow.Cells[14].AddParagraph(r.CumulativeInvoiceBillingTotal.ToString("N2"));
                dataRow.Cells[14].Format.Alignment = ParagraphAlignment.Right;
            }

            // 10. Add Footer
            Paragraph footer = section.Footers.Primary.AddParagraph();
            footer.AddText(CompanyName ?? "");
            footer.AddTab();
            footer.AddText(DateTime.Now.ToString("dd-MMM-yyyy"));
            footer.AddTab();
            footer.AddText("Page ");
            footer.AddPageField();
            footer.AddText(" of ");
            footer.AddNumPagesField();
            footer.Format.Font.Size = 7; // Reduced font size for footer
            footer.Format.Alignment = ParagraphAlignment.Left;

            // 11. Render PDF
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();

            // 12. Clean up temporary logo file after rendering
            if (!string.IsNullOrEmpty(tempLogoPath) && File.Exists(tempLogoPath))
            {
                try
                {
                    File.Delete(tempLogoPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            // 13. Convert to byte array
            using (MemoryStream stream = new MemoryStream())
            {
                pdfRenderer.PdfDocument.Save(stream);
                return stream.ToArray();
            }
        }

        // Added by Vyankat Bhure on 18-Feb-2026 - Get Company Logo filename from stored procedure
        public async Task<CompanyLogoModel> GetCompanyLogoFileName()
        {
            try
            {
                var result = await _repository.GetAsyncSP<CompanyLogoModel>(
                    "usp_Whizible2_Sel_Logo_tbl_PM_CompanyInformation",
                    new List<SqlParameter>()
                );

                if (result != null && result.CompanyLogoModel != null && result.CompanyLogoModel.Count > 0)
                {
                    return result.CompanyLogoModel[0];
                }

                return new CompanyLogoModel();
            }
            catch
            {
                return new CompanyLogoModel();
            }
        }

        public async Task<(byte[] excelBytes, string projectName)> GenerateProfitabilityExcel(int projectId, DateTime? fromDate, DateTime? toDate)
        {
            var param = new List<SqlParameter>
    {
        new SqlParameter("@ProjectID", projectId),
        new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
        new SqlParameter("@ToDate",   toDate   ?? (object)DBNull.Value)
    };

            var result = await _repository.GetAsyncSP<ProfitabilityReportData>(
                "usp_RPT_Tbl_PM_ProjectProfitability",
                param
            );

            List<ProfitabilityReportData> data = result.ProfitabilityReportData;
            if (data == null || !data.Any())
                return (null, null);


            var header = data.First();

            // Added by Vyankat Bhure on 20-02-2025 - Get ActualStartDate and ActualEndDate from GetProjectDetail for Excel
            DateTime? actualStartDate = null;
            DateTime? actualEndDate = null;
            try
            {
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intProjectId", projectId)
                };
                var projectDetailResult = await _repository.GetAsyncSP<ProjectDetailModel>("usp_Whizible2_Sel_PROJECTDETAIL", sqlParams);
                if (projectDetailResult != null && projectDetailResult.ProjectDetailModel != null && projectDetailResult.ProjectDetailModel.Count > 0)
                {
                    var projectDetail = projectDetailResult.ProjectDetailModel[0];
                    actualStartDate = projectDetail.ActualStartDate;
                    actualEndDate = projectDetail.ActualEndDate;
                }
            }
            catch (Exception ex)
            {
                // If GetProjectDetail fails, continue with existing dates
                System.Diagnostics.Debug.WriteLine($"Error getting project detail for Excel: {ex.Message}");
            }
            // End of Added by Vyankat Bhure on 20-02-2025

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Project Profitability");

            // ---------------- PROJECT INFO (Rows 1-4) ----------------
            // Added by Vyankat Bhure on 20-02-2025 - Updated to show project details similar to PDF format with complete table borders
            // Create a range for the project details table (A1:E4) and apply borders
            var projectDetailsRange = ws.Range("A1:E4");
            projectDetailsRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            projectDetailsRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            
            // Row 1: Project Name and Project Value
            ws.Cell("A1").Value = "Project Name:";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("B1").Value = header.ProjectName ?? "";
            ws.Cell("B1").Style.Alignment.WrapText = true;
            ws.Cell("B1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell("C1").Value = ""; // Empty cell
            ws.Cell("D1").Value = "Project Value:";
            ws.Cell("D1").Style.Font.Bold = true;
            ws.Cell("E1").Value = $"{header.CurrencySymbol} {header.ProjectValue:N2}";
            ws.Cell("E1").Style.Alignment.WrapText = true;
            ws.Cell("E1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // Row 2: Start Date and End Date
            ws.Cell("A2").Value = "Start Date:";
            ws.Cell("A2").Style.Font.Bold = true;
            ws.Cell("B2").Value = header.ExpectedStartDate?.ToString("dd-MMM-yyyy") ?? "";
            ws.Cell("B2").Style.Alignment.WrapText = true;
            ws.Cell("B2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            if (header.ExpectedStartDate.HasValue)
            {
                ws.Cell("B2").Style.DateFormat.Format = "dd-MMM-yyyy";
            }
            ws.Cell("C2").Value = ""; // Empty cell
            ws.Cell("D2").Value = "End Date:";
            ws.Cell("D2").Style.Font.Bold = true;
            ws.Cell("E2").Value = header.ExpectedEndDate?.ToString("dd-MMM-yyyy") ?? "";
            ws.Cell("E2").Style.Alignment.WrapText = true;
            ws.Cell("E2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            if (header.ExpectedEndDate.HasValue)
            {
                ws.Cell("E2").Style.DateFormat.Format = "dd-MMM-yyyy";
            }

            // Row 3: Actual Start Date and Actual End Date
            ws.Cell("A3").Value = "Actual Start Date:";
            ws.Cell("A3").Style.Font.Bold = true;
            // Use ActualStartDate from GetProjectDetail instead of headerInfo.ActualStartDate
            string actualStartDateStr = actualStartDate?.ToString("dd-MMM-yyyy") ?? header.ActualStartDate?.ToString("dd-MMM-yyyy") ?? "";
            ws.Cell("B3").Value = actualStartDateStr;
            ws.Cell("B3").Style.Alignment.WrapText = true;
            ws.Cell("B3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            if (actualStartDate.HasValue || header.ActualStartDate.HasValue)
            {
                ws.Cell("B3").Style.DateFormat.Format = "dd-MMM-yyyy";
            }
            ws.Cell("C3").Value = ""; // Empty cell
            ws.Cell("D3").Value = "Actual End Date:";
            ws.Cell("D3").Style.Font.Bold = true;
            // Use ActualEndDate from GetProjectDetail instead of headerInfo.ActualEndDate
            string actualEndDateStr = actualEndDate?.ToString("dd-MMM-yyyy") ?? header.ActualEndDate?.ToString("dd-MMM-yyyy") ?? "";
            ws.Cell("E3").Value = actualEndDateStr;
            ws.Cell("E3").Style.Alignment.WrapText = true;
            ws.Cell("E3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            if (actualEndDate.HasValue || header.ActualEndDate.HasValue)
            {
                ws.Cell("E3").Style.DateFormat.Format = "dd-MMM-yyyy";
            }

            // Row 4: Project Currency
            ws.Cell("A4").Value = "Project Currency:";
            ws.Cell("A4").Style.Font.Bold = true;
            ws.Cell("B4").Value = header.CurrencySymbol ?? "";
            ws.Cell("B4").Style.Alignment.WrapText = true;
            ws.Cell("B4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell("C4").Value = ""; // Empty cell
            ws.Cell("D4").Value = ""; // Empty cell
            ws.Cell("E4").Value = ""; // Empty cell
            // End of Added by Vyankat Bhure on 20-02-2025

            // ---------------- EMPTY ROW (Row 5) ----------------
            // Leave row 5 empty for spacing

            // ---------------- COLUMN HEADERS (Row 6) ----------------
            // Added by Vyankat Bhure on 20-02-2025 - Removed FromDate and ToDate columns, updated column names to match PDF format
            int col = 1;
            ws.Cell(6, col++).Value = "As On";
            ws.Cell(6, col++).Value = "Periodic Actual Hours";
            ws.Cell(6, col++).Value = "Cumulative Actual Hours";
            ws.Cell(6, col++).Value = "Periodic People Cost";
            ws.Cell(6, col++).Value = "Cumulative People Cost";
            ws.Cell(6, col++).Value = "Periodic Expenses";
            ws.Cell(6, col++).Value = "Cumulative Expenses";
            ws.Cell(6, col++).Value = "Periodic Accrued People Revenue";
            ws.Cell(6, col++).Value = "Cumulative Accrued People Revenue";
            ws.Cell(6, col++).Value = "Periodic Accrued Billable Expenses";
            ws.Cell(6, col++).Value = "Cumulative Accrued Billable Expenses";
            ws.Cell(6, col++).Value = "Periodic GPM";
            ws.Cell(6, col++).Value = "Cumulative GPM";
            ws.Cell(6, col++).Value = "Periodic Invoice Revenue";
            ws.Cell(6, col++).Value = "Cumulative Invoice Revenue";
            // End of Added by Vyankat Bhure on 20-02-2025

            // Style all headers - Normal horizontal text
            ws.Row(6).Height = 30; // Standard row height for headers
            for (int headerCol = 1; headerCol < col; headerCol++)
            {
                ws.Cell(6, headerCol).Style.Font.Bold = true;
                ws.Cell(6, headerCol).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(6, headerCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(6, headerCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(6, headerCol).Style.Alignment.WrapText = true; // Enable wrap text for long header names
            }

            // ---------------- DATA ----------------
            int row = 7;

            foreach (var r in data)
            {
                col = 1;
                
                // Added by Vyankat Bhure on 20-02-2025 - Removed FromDate and ToDate columns
                // "As On" column - Use ToDate directly (SP shows "As On" equals ToDate)
                // If ToDate is invalid, try AsOn, otherwise use a fallback
                DateTime asOnValue = r.ToDate;
                if (asOnValue == DateTime.MinValue || asOnValue.Year < 1900)
                {
                    if (r.AsOn != DateTime.MinValue && r.AsOn.Year >= 1900)
                        asOnValue = r.AsOn;
                    else
                        asOnValue = header.ExpectedStartDate ?? DateTime.Now;
                }
                ws.Cell(row, col++).Value = asOnValue;
                if (asOnValue.Year >= 1900)
                {
                    ws.Cell(row, col - 1).Style.DateFormat.Format = "dd-MM-yyyy";
                }
                ws.Cell(row, col - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // Commented by Vyankat Bhure on 20-02-2025 - Removed FromDate and ToDate columns to match PDF format
                //// FromDate
                //DateTime fromDateValue = r.FromDate;
                //if (fromDateValue == DateTime.MinValue || fromDateValue.Year < 1900)
                //{
                //    fromDateValue = header.ExpectedStartDate ?? DateTime.Now;
                //}
                //ws.Cell(row, col++).Value = fromDateValue;
                //if (fromDateValue.Year >= 1900)
                //{
                //    ws.Cell(row, col - 1).Style.DateFormat.Format = "dd-MM-yyyy";
                //}
                //ws.Cell(row, col - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                //// ToDate
                //DateTime toDateValue = r.ToDate;
                //if (toDateValue == DateTime.MinValue || toDateValue.Year < 1900)
                //{
                //    toDateValue = header.ExpectedEndDate ?? DateTime.Now;
                //}
                //ws.Cell(row, col++).Value = toDateValue;
                //if (toDateValue.Year >= 1900)
                //{
                //    ws.Cell(row, col - 1).Style.DateFormat.Format = "dd-MM-yyyy";
                //}
                //ws.Cell(row, col - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                // End of Commented by Vyankat Bhure on 20-02-2025

                // Numeric columns - Updated order to match PDF format
                ws.Cell(row, col++).Value = r.PeriodicAccruedActualHoursTotal;
                ws.Cell(row, col++).Value = r.CumulativeAccruedActualHoursTotal;
                ws.Cell(row, col++).Value = r.PeriodicAccruedResourceCostTotal;
                ws.Cell(row, col++).Value = r.CumulativeAccruedResourceCostTotal;
                ws.Cell(row, col++).Value = r.PeriodicOtherCostTotal;
                ws.Cell(row, col++).Value = r.CumulativeOtherCostTotal;
                ws.Cell(row, col++).Value = r.PeriodicAccruedResourceBillingTotal;
                ws.Cell(row, col++).Value = r.CumulativeAccruedResourceBillingTotal;
                ws.Cell(row, col++).Value = r.PeriodicBillableOtherCostTotal;
                ws.Cell(row, col++).Value = r.CumulativeBillableOtherCostTotal;
                ws.Cell(row, col++).Value = r.PeriodicGPM_Project;
                ws.Cell(row, col++).Value = r.CumulativeGPM_Project;
                ws.Cell(row, col++).Value = r.PeriodicInvoiceBillingTotal;
                ws.Cell(row, col++).Value = r.CumulativeInvoiceBillingTotal;

                // RIGHT ALIGN NUMBERS (columns 2-15 are numeric, column 1 is date)
                // Column 1: As On (date)
                // Columns 2-15: Numeric values
                for (int numCol = 2; numCol <= 15; numCol++)
                {
                    ws.Cell(row, numCol).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, numCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
                // End of Added by Vyankat Bhure on 20-02-2025

                row++;
            }

            // ---------------- COLUMN WIDTHS ----------------
            // Added by Vyankat Bhure on 20-02-2025 - Updated column widths after removing FromDate and ToDate columns
            // Adjusted widths for better header visibility with horizontal text
            ws.Column(1).Width = 22;  // As On
            ws.Column(2).Width = 28;  // Periodic Actual Hours
            ws.Column(3).Width = 30;  // Cumulative Actual Hours
            ws.Column(4).Width = 28;  // Periodic People Cost
            ws.Column(5).Width = 30;  // Cumulative People Cost
            ws.Column(6).Width = 24;  // Periodic Expenses
            ws.Column(7).Width = 26;  // Cumulative Expenses
            ws.Column(8).Width = 32;  // Periodic Accrued People Revenue
            ws.Column(9).Width = 34;  // Cumulative Accrued People Revenue
            ws.Column(10).Width = 36; // Periodic Accrued Billable Expenses
            ws.Column(11).Width = 38; // Cumulative Accrued Billable Expenses
            ws.Column(12).Width = 22; // Periodic GPM
            ws.Column(13).Width = 24; // Cumulative GPM
            ws.Column(14).Width = 28; // Periodic Invoice Revenue
            ws.Column(15).Width = 30; // Cumulative Invoice Revenue
            // End of Added by Vyankat Bhure on 20-02-2025

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return (stream.ToArray(), header.ProjectName);
        }

        // Added by Vyankat Bhure on 15-12-20255 – Fetch cost trend data for Project Profitability
        public async Task<ResponseEntity> GetCostTrend(

            int projectID,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@intProjectID", projectID),
                new SqlParameter("@dtFromDate", (object?)fromDate ?? DBNull.Value),
                new SqlParameter("@dtToDate", (object?)toDate ?? DBNull.Value)
            };

                var result = await _repository.GetAsyncSP<CostTrendResponse>(
                    "usp_Whizible2_Sel_CRW_Profitability_ResourceCost",
                    sqlParams);

                var costTrendList = result ?? new List<CostTrendResponse>();

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Cost trend retrieved successfully",
                        costTrend = costTrendList
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error retrieving cost trend: " + ex.Message +
                           (ex.InnerException != null
                               ? " Inner: " + ex.InnerException.Message
                               : "")
                };
            }
        }
        //End of Added by Vyankat Bhure on 15-12-20255 – Fetch cost trend data for Project Profitability

        // Added by Vyankat Bhure on 15-12-2025 to fetch revenue trend data for a project Profitability
        public async Task<ResponseEntity> GetRevenueTrend(int projectID, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@intProjectID", projectID),
            new SqlParameter("@dtFromDate", (object?)fromDate ?? DBNull.Value),
            new SqlParameter("@dtToDate", (object?)toDate ?? DBNull.Value)
        };

                var result = await _repository.GetAsyncSP<RevenueTrendResponse>(
                    "usp_Whizible2_Sel_ProjectProfitability_RevenueTrend",
                    sqlParams);

                var revenueTrendList = result ?? new List<RevenueTrendResponse>();

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Revenue trend retrieved successfully",
                        revenueTrend = revenueTrendList
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error retrieving revenue trend: " + ex.Message +
                           (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")
                };
            }
        }
        //End of Added by Vyankat Bhure on 15-12-2025 to fetch revenue trend data for a project Profitability


        // Added by Vyankat Bhure on 15-12-2025 to fetch latest GPM (Gross Profit Margin) data for a project
        public async Task<ResponseEntity> GetGPMTrend(int projectID)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@intProjectID", projectID)
        };

                var result = await _repository.GetAsyncSP<GPMTrendResponse>(
                    "usp_Whizible2_Sel_Tbl_PM_ProjectProfitability_GPM",
                    sqlParams);

                var gpmTrend = result ?? new List<GPMTrendResponse>();


                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "GPM data retrieved successfully",
                        gpmTrend
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error retrieving GPM data: " + ex.Message +
                           (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")
                };
            }
        }
        // End of GetGPMTrend service method

        // Added by Vyankat Bhure on 15-12-2025 to fetch Project Profitability snapshots
        public async Task<ResponseEntity> GetProjectSnapshots(int projectID)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@ProjectID", projectID)
        };

                var result = await _repository.GetAsyncSP<SnapshotTrendResponse>(
                    "usp_Whizible2_SEL_Tbl_PM_ProjectProfitability_forSnapshot",
                    sqlParams);

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Project snapshots retrieved successfully",
                        snapshots = result
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error retrieving Project snapshots: " + ex.Message +
                           (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")
                };
            }
        }
        // End of Added by Vyankat Bhure on 15-12-2025 to fetch Project Profitability snapshots 

        // Added by Vyankat Bhure on 15-12-2025 - Business logic to fetch Resource Profitability Details based on Cost Type
        public async Task<ResponseEntity> GetResourceProfitabilityDetails(ResourceProfitabilityRequest request)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@intProjectID", request.ProjectID),
            new SqlParameter("@FromDate",
                string.IsNullOrWhiteSpace(request.FromDate)
                    ? (object)DBNull.Value
                    : request.FromDate),
            new SqlParameter("@ToDate",
                string.IsNullOrWhiteSpace(request.ToDate)
                    ? (object)DBNull.Value
                    : request.ToDate),
            new SqlParameter("@CostType", request.CostType)
        };

                // This will hold final data from any branch
                object? finalResult = null;

                // EXPENSE (LIKE '%EXPENSE%')
                if (!string.IsNullOrWhiteSpace(request.CostType) &&
                    request.CostType.Contains("EXPENSE", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await _repository.GetAsyncSP<ProjectExpenseTrendModel>(
                        "usp_Whizible2_sel_Resource_Profitability_Details",
                        sqlParams
                    );

                    finalResult = result.ProjectExpenseTrendModel;
                }
                // MILESTONE
                else if (!string.IsNullOrWhiteSpace(request.CostType) &&
                         request.CostType.Contains("MILESTONE", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await _repository.GetAsyncSP<ProjectMilestoneCostResponse>(
                        "usp_Whizible2_sel_Resource_Profitability_Details",
                        sqlParams
                    );

                    finalResult = result.ProjectMilestoneCostResponse;
                }
                // DEFAULT → RESOURCE / HOURS / PEOPLE COST
                else
                {
                    var result = await _repository.GetAsyncSP<ResourceProfResponse>(
                        "usp_Whizible2_sel_Resource_Profitability_Details",
                        sqlParams
                    );

                    finalResult = result.ResourceProfResponse;
                }

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        Message = "Resource profitability details retrieved successfully",
                        ResourceProfitabilityDetails = finalResult
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error retrieving resource profitability details: " +
                           ex.Message +
                           (ex.InnerException != null
                                ? " InnerException: " + ex.InnerException.Message
                                : string.Empty)
                };
            }
        }
        // End of Added by Vyankat Bhure on 15-12-2025 - Business logic to fetch Resource Profitability Details based on Cost Type


        // Added by Vyankat Bhure on 15-12-2025 - Process Project Profitability for Generate and ReGenerate
        public async Task<ResponseEntity> ProcessProjectProfitability(
            ProjectProfiRequest request)
        {
            try
            {

                var sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@intProjectId", request.ProjectID),

            // Handle optional dates: pass DBNull.Value if the string is empty/whitespace
            new SqlParameter("@dtFromDate", string.IsNullOrWhiteSpace(request.FromDate)
                                            ? (object)DBNull.Value
                                            : request.FromDate),
            new SqlParameter("@dtToDate", string.IsNullOrWhiteSpace(request.ToDate)
                                            ? (object)DBNull.Value
                                            : request.ToDate),
             // Handle flag (Generate / ReGenerate)
    new SqlParameter("@dtFlag", request.DtFlag)
        };

                var result = await _repository.GetAsyncSP<ProjectProfitabilityCumulativeResponse>(
                       "usp_CDE_Whizible2_ProjectProfitability",
                       sqlParams
                   );

                // Return a success response
                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        Message = "Project profitability snapshot process started successfully for Project ID: " + request.ProjectID
                    }
                };
            }
            catch (Exception ex)
            {
                // Return a failure response with exception details
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error processing project profitability snapshot: " + ex.Message +
                           (ex.InnerException != null
                                ? " InnerException: " + ex.InnerException.Message
                                : string.Empty)
                };
            }
        }
        // End of Added by Vyankat Bhure on 15-12-2025 - Process Project Profitability for Generate and ReGenerate

        // Added by Vyankat Bhure on 16-12-2025 - - Get Project Contract Type and Currency
        public async Task<ResponseEntity> GetProjectContractTypeCurrency(
              ProjectDetailRequest request)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@ProjectID", request.ProjectID)
        };

                var result = await _repository.GetAsyncSP<ProjectContractTypeCurrencyModel>(
                    "usp_Whizible2_Sel_ProjectContractTypeCurrency",
                    sqlParams
                );

                var data = result.ProjectContractTypeCurrencyModel;

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Project contract type and billing currency details",
                        result = data
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error " + ex.Message +
                           (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")
                };
            }
        }
        // End of Added by Vyankat Bhure on 16-12-2025 - Get Project Contract Type and Currency
     
        // Added by Vyankat Bhure on 26-12-2025 - Update CostMethod for Company Information
        public async Task<ResponseEntity> UpdateCompanyInformation(int costMethod)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intCostMethod", costMethod)
                };

                var result = await _repository.UpdateAsyncSP("usp_Whizible2_Upd_CompanyInformation", sqlParams);
               
                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        Message = "Cost Method updated successfully" 
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error updating company information: " + ex.Message +
                           (ex.InnerException != null
                                ? " InnerException: " + ex.InnerException.Message
                                : string.Empty)
                };
            }
        }
        // End of Added by Vyankat Bhure on 26-12-2025 - Update CostMethod for Company Information

        // Added by Vyankat Bhure on 26-12-2025 - Update Reporting Frequency for Company Information
        public async Task<ResponseEntity> UpdateCompanyInformationReportingFrequency(int reportingFrequency)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intReportingFrequency", reportingFrequency)
                };
             
                var result = await _repository.UpdateAsyncSP("usp_Whizible2_Upd_CompanyInformation_ReportingFrequency", sqlParams);

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        Message = "Company Information updated successfully. Reporting Frequency set to: " + reportingFrequency +
                                  (reportingFrequency == 1 ? " (Weekly)" : reportingFrequency == 2 ? " (Monthly)" : " (Quarterly)")
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error updating company information reporting frequency: " + ex.Message +
                           (ex.InnerException != null
                                ? " InnerException: " + ex.InnerException.Message
                                : string.Empty)
                };
            }
        }
        // End of Added by Vyankat Bhure on 26-12-2025 - Update Reporting Frequency for Company Information

        // Added by Vyankat Bhure on 13-01-2025 - Get Project Profitability ToDate
        public async Task<ResponseEntity> GetProjectProfitabilityToDate(
            ProjectProfitabilityToDateRequest request)
        {
            try
            {
                var sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@ProjectID", request.ProjectID),
            new SqlParameter("@FromDate", request.FromDate)
        };

                var result = await _repository.GetAsyncSP<ProjectProfitabilityToDateModel>(
                    "usp_Whizible2_Sel_ProjectProfitability_Todate",
                    sqlParams
                );

                var data = result.ProjectProfitabilityToDateModel;

                return new ResponseEntity
                {
                    Status = ResponseStatus.SUCCESS,
                    Data = new
                    {
                        message = "Project profitability ToDate fetched successfully",
                        result = data
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseEntity
                {
                    Status = ResponseStatus.FAILURE,
                    Data = "Error " + ex.Message +
                           (ex.InnerException != null
                                ? " Inner: " + ex.InnerException.Message
                                : string.Empty)
                };
            }
        }
        // End of Added by Vyankat Bhure on 13-01-2025 - Get Project Profitability ToDate

    }

}

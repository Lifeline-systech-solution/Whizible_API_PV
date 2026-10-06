using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using Whizible26.Application.CommandsQueries;
//using Whizible26.Application.CommandsQueries.Dashboard.MIS;
using Whizible26.Domain.Entity.ProjectEntities.ProjectProfiEntity;
using WhizibleAPI.API.Filters;
using WhizibleTeams.Application.Helpers;
using WhizibleTeams.API.Attributes;


namespace Whizible26.API.Controllers
{
    // Added by Vyankat B. on 15-12-2025 for the PM_Project Profitability

    [Route("api/[controller]")]
    [ApiController]
    public class ProjectProfitabilityController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        //public PM_ProjectProfitabilityController(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}

        private readonly IWebHostEnvironment environment;

        public ProjectProfitabilityController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            this.environment = webHostEnvironment;

        }

        // Vaibhav k 

        [HttpPost("GetProjectDetail")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectDetail([FromBody] ProjectDetailRequest request)
        {
            try
            {
                var svc = new ProjectProfitability(_configuration);
                var response = await svc.GetProjectDetail(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("GetProjectProfitabilityTrends")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectProfitabilityTrends([FromBody] TrendsRequest request)
        {
            try
            {
                var svc = new ProjectProfitability(_configuration);
                var response = await svc.GetTrends(request.ProjectID, request.FromDate, request.ToDate, request.GraphId);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat Bhure on 22-12-2025 - Fetch Project Financial Data for Project Profitability
        [HttpPost("GetProjectFinancialData")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectFinancialData([FromBody] ProjProfFinancialRequest request)
        {
            try
            {
                if (request.ProjectID <= 0)
                    return BadRequest("ProjectID is required.");

                var svc = new ProjectProfitability(_configuration);
                var response = await svc.GetFinancialData(request.ProjectID);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProjectFinancialData controller method

        [HttpPost("GenerateProfitabilityPdf")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GenerateProfitabilityPdf([FromBody] ProfitabilityRequest req)
        {
            try
            {
                var svc = new ProjectProfitability(_configuration);

                // Get logo filename from stored procedure
                var (logoBytes, logoExtension) = await GetCompanyLogoBytes(svc);

                // Call your service method that generates the PDF bytes
                var pdfBytes = await svc.GenerateProfitabilityPdf(req.ProjectID, req.FromDate, req.ToDate,
                        logoBytes, logoExtension);

                if (pdfBytes == null || pdfBytes.Length == 0)
                    return NotFound("No data found for given input.");

                // Sanitize project name for filename (remove invalid characters)
                var sanitizedProjectName = string.IsNullOrWhiteSpace(req.ProjectName)
                    ? $"Project_{req.ProjectID}"
                    : string.Join("_", req.ProjectName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))
                        .Replace(" ", "_")
                        .Trim();

                // Limit length to avoid very long filenames
                if (sanitizedProjectName.Length > 50)
                    sanitizedProjectName = sanitizedProjectName.Substring(0, 50);

                // Build the file name dynamically
                var fileName = $"ProjectProfitability_{sanitizedProjectName}_{DateTime.Now:yyyyMMdd}.pdf";

                // Return file download result
                return File(
                    pdfBytes,
                    "application/pdf",
                    fileName
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("GenerateProfitabilityExcel")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GenerateProfitabilityExcel([FromBody] ProfitabilityRequest req)
        {
            try
            {
                var svc = new ProjectProfitability(_configuration);

                // Call service method that generates Excel bytes
                (byte[] excelBytes, string projectName) = await svc.GenerateProfitabilityExcel(
                    req.ProjectID,
                    req.FromDate,
                    req.ToDate
                );

                if (excelBytes == null || excelBytes.Length == 0)
                    return NotFound("No data found for given input.");

                // Sanitize project name for filename (remove invalid characters)
                var sanitizedProjectName = string.IsNullOrWhiteSpace(projectName)
                    ? $"Project_{req.ProjectID}"
                    : string.Join("_", projectName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))
                        .Replace(" ", "_")
                        .Trim();

                // Limit length to avoid very long filenames
                if (sanitizedProjectName.Length > 50)
                    sanitizedProjectName = sanitizedProjectName.Substring(0, 50);

                // Build file name dynamically
                var fileName = $"ProjectProfitability_{sanitizedProjectName}_{DateTime.Now:yyyyMMdd}.xlsx";

                // Return file download result
                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat Bhure on 15-12-2025 - Fetch Project Profitability report based on selected report flag
        [HttpPost("GetProjectProfitability")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectProfitability(
            [FromBody] ProjectProfitabilityRequest request)
        {
            try
            {
                if (request.ProjectID <= 0)
                    return BadRequest("ProjectID is required.");

                var response = await new ProjectProfitability(_configuration)
                    .GetProjectProfitability(request);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProjectProfitability controller method



        //Added by Vyankat Bhure on 15-12-2025 - Fetch Cost Trend data for Project Profitability
        [HttpPost("GetCostTrend")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetCostTrend(
            [FromBody] CostTrendRequest request)
        {
            try
            {
                var response = await new ProjectProfitability(_configuration)
             .GetCostTrend(request.ProjectID, request.FromDate, request.ToDate);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        //End of Added by Vyankat Bhure on 15-12-2025 - Fetch Cost Trend data for Project Profitability


        // Added by Vyankat Bhure on 15-12-2025 - Fetch the Revenue Trend for Project Profitability
        [HttpPost("GetRevenueTrend")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetRevenueTrend([FromBody] CostTrendRequest request)
        {
            try
            {
                var response = await new ProjectProfitability(_configuration)
                    .GetRevenueTrend(request.ProjectID, request.FromDate, request.ToDate);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        //End of Added by Vyankat Bhure on 15-12-2025 - Fetch the Revenue Trend for Project Profitability

        // Added by Vyankat Bhure on 15-12-2025 to handle API call for Project Profitability GPM data via query string
        [HttpPost("GetGPMTrend")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetGPMTrend([FromBody] GPMRequest request)
        {
            try
            {
                var response = await new ProjectProfitability(_configuration)
            .GetGPMTrend(request.ProjectID);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetGPMTrend controller method


        // Added by Vyankat Bhure on 15-12-2025 - Fetch Project SnapShot for Project Profitability snapshots
        [HttpPost("GetProjectSnapshots")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectSnapshots([FromBody] GPMRequest request)
        {
            try
            {

                if (request.ProjectID <= 0)
                {
                    return BadRequest("ProjectID is required");
                }

                var response = await new ProjectProfitability(_configuration)
                    .GetProjectSnapshots(request.ProjectID);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        //End of Added by Vyankat Bhure on 15-12-2025 - Fetch Project SnapShot for Project Profitability snapshots

        [NonAction]
        // Updated by Vyankat Bhure on 18-Feb-2026 - Get logo filename from stored procedure
        private async Task<(byte[] logoBytes, string fileExtension)> GetCompanyLogoBytes(ProjectProfitability svc)
        {
            // Resolve wwwroot path robustly to ensure it works in all hosting scenarios
            string baseWebRoot = environment.WebRootPath;

            // Fallback if WebRootPath is not set or directory does not exist
            if (string.IsNullOrWhiteSpace(baseWebRoot) || !Directory.Exists(baseWebRoot))
            {
                var currentDir = Directory.GetCurrentDirectory();
                var wwwrootPath = Path.Combine(currentDir, "wwwroot");

                if (!Directory.Exists(wwwrootPath))
                {
                    var parentDir = Directory.GetParent(currentDir);
                    wwwrootPath = parentDir != null
                        ? Path.Combine(parentDir.FullName, "wwwroot")
                        : wwwrootPath;
                }

                baseWebRoot = wwwrootPath;
            }

            var uploadPath = Path.Combine(baseWebRoot, "Uploads", "Logo");
            var fallbackLogo = Path.Combine(uploadPath, "no-photo.png");

            try
            {
                // Get logo filename from stored procedure
                var logoInfo = await svc.GetCompanyLogoFileName();
                
                if (logoInfo != null && !string.IsNullOrWhiteSpace(logoInfo.OriginalFileName))
                {
                    // Use OriginalFileName to find the logo file in Uploads/Logo directory
                    var companyLogo = Path.Combine(uploadPath, logoInfo.OriginalFileName);
                    
                    System.Diagnostics.Debug.WriteLine($"Logo path: {companyLogo}");
                    System.Diagnostics.Debug.WriteLine($"Logo file exists: {System.IO.File.Exists(companyLogo)}");
                    
                    if (System.IO.File.Exists(companyLogo))
                    {
                        var bytes = System.IO.File.ReadAllBytes(companyLogo);
                        // Get file extension from OriginalFileName
                        var extension = Path.GetExtension(logoInfo.OriginalFileName);
                        if (string.IsNullOrWhiteSpace(extension))
                            extension = ".png"; // Default to png if no extension
                        System.Diagnostics.Debug.WriteLine($"Logo loaded successfully. Size: {bytes.Length} bytes, Extension: {extension}");
                        return (bytes, extension);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Logo file not found at: {companyLogo}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Logo info is null or OriginalFileName is empty");
                }
            }
            catch (Exception ex)
            {
                // Log error but continue to fallback
                System.Diagnostics.Debug.WriteLine($"Error loading logo from SP: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            // Fallback to default logo if SP fails or file doesn't exist
            if (System.IO.File.Exists(fallbackLogo))
            {
                var bytes = System.IO.File.ReadAllBytes(fallbackLogo);
                return (bytes, ".png");
            }

            // Return empty array if no logo found
            return (new byte[0], ".png");
        }

        // Added by Vyankat Bhure on 15-12-2025 - Fetch Resource Profitability Details based on Cost Type
        [HttpPost("GetResourceProfitabilityDetails")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceProfitabilityDetails(
            [FromBody] ResourceProfitabilityRequest request)
        {
            try
            {
                if (request.ProjectID <= 0)
                {
                    return BadRequest("ProjectID is required");
                }
                var response = await new ProjectProfitability(_configuration)
                   .GetResourceProfitabilityDetails(request);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        //End of Added by Vyankat Bhure on 15-12-2025 - Fetch Resource Profitability Details based on Cost Type

        // Added by Vyankat Bhure on 15-12-2025 - Process Project Profitability for Generate and ReGenerate
        [HttpPost("GenerateProjProfi")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> ProcessProjectProfitability(
             [FromBody] ProjectProfiRequest request)
        {
            try
            {
                var response = await new ProjectProfitability(_configuration)
                    .ProcessProjectProfitability(request);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of Added by Vyankat Bhure on 15-12-2025 - Process Project Profitability for Generate and ReGenerate


        // Added by Vyankat Bhure on 22-12-2025 - Fetch Period Graph data for Project Profitability
        [HttpPost("GetProjPeridGraphData")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectProfitabilityPeridGraphData(
            [FromBody] ProfitabilityRequest request)
        {
            try
            {
                if (request.ProjectID <= 0)
                    return BadRequest("ProjectID is required.");

                var svc = new ProjectProfitability(_configuration);
                var response = await svc.GetPeridGraphData(request.ProjectID, request.FromDate, request.ToDate);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProjectProfitabilityPeridGraphData controller method

        // Added by Vyankat Bhure on 24-12-2025 - Fetch Project Profitability Periods
        [HttpPost("GetProjectProfitabilityPeriods")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectProfitabilityPeriods(
            [FromBody] ProjectProfitabilityPeriodsRequest request)
        {
            try
            {
                if (request == null || request.ProjectID <= 0)
                    return BadRequest("ProjectID is required and must be greater than 0.");

                var svc = new ProjectProfitability(_configuration);
                var response = await svc.GetProjectProfitabilityPeriods(request);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProjectProfitabilityPeriods controller method


        [HttpPost("GetProjectContractTypeCurrency")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectContractTypeCurrency(
        [FromBody] ProjectDetailRequest request)
        {
            try
            {
                var response = await new ProjectProfitability(_configuration)
                    .GetProjectContractTypeCurrency(request);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        // Added by Vyankat Bhure on 26-12-2025 - Update CostMethod for Company Information
        [HttpPost("UpdateCompanyInformation")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        //[ServiceFilter(typeof(ValidateRateLimitAttribute))]   //Added by Vishal Mane on 03/06/2026 for Rate Limiting 
        public async Task<IActionResult> UpdateCompanyInformation([FromBody] UpdateCompanyInformationRequest request)
        {
            try
            {
                if (request == null || request.CostMethod <= 0)
                    return BadRequest("CostMethod is required and must be greater than 0.");

                var svc = new ProjectProfitability(_configuration);
                var response = await svc.UpdateCompanyInformation(request.CostMethod);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of UpdateCompanyInformation controller method

        // Added by Vyankat Bhure on 26-12-2025 - Update Reporting Frequency for Company Information
        [HttpPost("UpdReportingFrequency")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        //[ServiceFilter(typeof(ValidateRateLimitAttribute))]   //Commented by Vishal Mane on 03/06/2026 for Rate Limiting 
        public async Task<IActionResult> UpdateCompanyInformationReportingFrequency([FromBody] UpdateCompanyInformationReportingFrequencyRequest request)
        {
            try
            {
                if (request == null || request.ReportingFrequency <= 0 || request.ReportingFrequency > 3)
                    return BadRequest("ReportingFrequency is required and must be between 1 and 3 (1=Weekly, 2=Monthly, 3=Quarterly).");

                var svc = new ProjectProfitability(_configuration);
                var response = await svc.UpdateCompanyInformationReportingFrequency(request.ReportingFrequency);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of UpdateCompanyInformationReportingFrequency controller method

        // Added by Vyankat Bhure on 12-12-2025 - Get Project Profitability Date Range
        [HttpPost("GetProjectProfitabilityDateRange")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectProfitabilityDateRange([FromBody] ProjectDateRangeRequest request)
        {
            try
            {
                if (request == null || request.ProjectID <= 0)
                    return BadRequest("ProjectID is required and must be greater than 0.");

                var svc = new ProjectProfitability(_configuration);
                var response = await svc.GetProjectProfitabilityDateRange(request);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProjectProfitabilityDateRange controller method

        // Added by Vyankat Bhure on 13-01-2025 - API endpoint for Get Project Profitability ToDate
        [HttpPost("GetProjectProfitabilityToDate")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectProfitabilityToDate(
            [FromBody] ProjectProfitabilityToDateRequest request)
        {
            var response = await new ProjectProfitability(_configuration)
                                 .GetProjectProfitabilityToDate(request);

            return Ok(response);
        }
        // End of Added by Vyankat Bhure on 13-01-2025 - API endpoint for Get Project Profitability ToDate


    }

}

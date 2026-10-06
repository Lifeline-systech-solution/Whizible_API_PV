
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Whizible26.Application.CommandsQueries;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleAPI.API.Filters;
using WhizibleTeams.API.Attributes;
using WhizibleTeams.Application.Helpers;
using Whizible26.Application.CommandsQueries.Dashboard;

namespace Whizible26.API.Controllers
{
    // Added for Resource Utilization By Resource API
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceUtilizationByResController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        //Added by Aditya J. on 25-08-2026 for Resource Utilization Report
        private readonly IWebHostEnvironment _webHostEnvironment;
        //End of Added by Aditya J. on 25-08-2026 for Resource Utilization Report

        public ResourceUtilizationByResController(
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            //Added by Aditya J. on 25-08-2026 for Resource Utilization Report
            _webHostEnvironment = webHostEnvironment;
            //End of Added by Aditya J. on 25-08-2026 for Resource Utilization Report
        }

        // Added for Resource Utilization By Resource API - Graph (top of screen), @intDetails = 0
        [HttpGet("GetResourceUtilizationGraph")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceUtilizationGraph(
            [FromQuery] ResourceUtilizationRequest request)
        {
            try
            {
                var svc = new ResourceUtilizationByRes(_configuration);
                var response = await svc.GetGraph(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                // Deviation from the reference controller, which returns ex.Message and
                // leaks stored-procedure and parameter names to the client. KNOWN_ISSUES #12.
                System.Diagnostics.Debug.WriteLine($"GetResourceUtilizationGraph Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added for Resource Utilization By Resource API

        // Added for Resource Utilization By Resource API - Display Details tab, @intDetails = 1
        [HttpGet("GetResourceUtilizationDetails")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceUtilizationDetails(
            [FromQuery] ResourceUtilizationRequest request)
        {
            try
            {
                var svc = new ResourceUtilizationByRes(_configuration);
                var response = await svc.GetDetails(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetResourceUtilizationDetails Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added for Resource Utilization By Resource API

        // Added for Resource Utilization By Resource API - Display Summary tab, @intDetails = 2
        [HttpGet("GetResourceUtilizationSummary")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceUtilizationSummary(
            [FromQuery] ResourceUtilizationRequest request)
        {
            try
            {
                var svc = new ResourceUtilizationByRes(_configuration);
                var response = await svc.GetSummary(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetResourceUtilizationSummary Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added for Resource Utilization By Resource API

        // Added for Resource Utilization By Resource API - Date Range filter dropdown (funnel icon), @intMode = 1
        [HttpGet("GetResourceUtilizationDateRanges")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceUtilizationDateRanges(
            [FromQuery] ResourceUtilizationDateRangeRequest request)
        {
            try
            {
                var svc = new ResourceUtilizationByRes(_configuration);
                var response = await svc.GetDateRanges(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetResourceUtilizationDateRanges Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added for Resource Utilization By Resource API

        //Added by Aditya J. on 03-09-2026 for adding Business Groups multi-select filter
        [HttpGet("GetResourceUtilizationBusinessGroups")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceUtilizationBusinessGroups()
        {
            try
            {
                var svc = new ResourceUtilizationByRes(_configuration);
                var response = await svc.GetBusinessGroups();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetResourceUtilizationBusinessGroups Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        //End of Added by Aditya J. on 03-09-2026 for adding Business Groups multi-select filter

        //Added by Aditya J. on 03-09-2026 for adding Organization Unit multi-select filter
        [HttpGet("GetResourceUtilizationOrganizationUnits")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceUtilizationOrganizationUnits()
        {
            try
            {
                var svc = new ResourceUtilizationByRes(_configuration);
                var response = await svc.GetOrganizationUnits();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetResourceUtilizationOrganizationUnits Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        //End of Added by Aditya J. on 03-09-2026 for adding Organization Unit multi-select filter

        //Added by Aditya J. on 03-09-2026 for adding Delivery Unit multi-select filter
        [HttpGet("GetResourceUtilizationDeliveryUnits")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceUtilizationDeliveryUnits()
        {
            try
            {
                var svc = new ResourceUtilizationByRes(_configuration);
                var response = await svc.GetDeliveryUnits();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetResourceUtilizationDeliveryUnits Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        //End of Added by Aditya J. on 03-09-2026 for adding Delivery Unit multi-select filter

        //Added by Aditya J. on 03-09-2026 for adding Resource multi-select filter
        [HttpGet("GetResourceUtilizationResources")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceUtilizationResources()
        {
            try
            {
                var svc = new ResourceUtilizationByRes(_configuration);
                var response = await svc.GetResources();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetResourceUtilizationResources Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        //End of Added by Aditya J. on 03-09-2026 for adding Resource multi-select filter

        //Added by Aditya J. on 09-09-2026<Added generic cascading filter API endpoint for BG/OU/DU/Resource>
        [HttpGet("GetResourceUtilizationDependentFilters")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceUtilizationDependentFilters(
            [FromQuery] ResourceUtilizationDependentFilterRequest request)
        {
            try
            {
                var svc = new ResourceUtilizationByRes(_configuration);
                var response = await svc.GetDependentFilters(request ?? new ResourceUtilizationDependentFilterRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetResourceUtilizationDependentFilters Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        //End of Added by Aditya J. on 09-09-2026<Added generic cascading filter API endpoint for BG/OU/DU/Resource>

        //Added by Aditya J. on 25-08-2026 for Resource Utilization Report
        [HttpGet("ExportResourceUtilizationReport")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> ExportResourceUtilizationReport(
            [FromQuery] ResourceUtilizationRequest request)
        {
            try
            {
                var svc = new ResourceUtilizationByRes(_configuration, _webHostEnvironment);
                var (pdfBytes, fileName, errorMessage) = await svc.ExportResourceUtilizationReport(
                    request ?? new ResourceUtilizationRequest());

                if (!string.IsNullOrWhiteSpace(errorMessage))
                    return BadRequest(errorMessage);

                if (pdfBytes == null || pdfBytes.Length == 0)
                    return NotFound("No data found for given input.");

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExportResourceUtilizationReport Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("ExportResourceUtilizationReport")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> ExportResourceUtilizationReportPost(
            [FromBody] ResourceUtilizationRequest request)
        {
            return await ExportResourceUtilizationReport(request ?? new ResourceUtilizationRequest());
        }
        //End of Added by Aditya J. on 25-08-2026 for Resource Utilization Report
    }
    // End of Added for Resource Utilization By Resource API
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Whizible26.Application.CommandsQueries.Dashboard;
using Whizible26.Domain.Entity.DashboardEntities;
using Whizible26.Domain.Entity.ProjectEntities.ProjectProfiEntity;
using WhizibleAPI.API.Filters;
using WhizibleTeams.API.Attributes;
using WhizibleTeams.Application.Helpers;

namespace Whizible26.API.Controllers
{
    // Added by Vyankat B. on 07-08-2026 for the Project Profitability Dashboard
    [Route("api/[controller]")]
    [ApiController]
    public class PM_ProjectProfDashController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PM_ProjectProfDashController(
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        // Copied from ProjectProfitabilityController.GetProjectProfitability — renamed endpoint for Dashboard
        [HttpPost("GetProjectProfDash")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectProfDash(
            [FromBody] ProjectProfitabilityRequest request)
        {
            try
            {
                if (request.ProjectID <= 0)
                    return BadRequest("ProjectID is required.");

                var response = await new PM_ProjectProfDash(_configuration)
                    .GetProjectProfDash(request);

                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProjectProfDash controller method

        // Added by Vyankat B. on 07-08-2026 - Business Group dropdown
        // Updated 10-08-2026 - UserID / LoginType (Employee vs Customer)
        [HttpPost("GetBusinessGroup")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetBusinessGroup(
            [FromBody] ProfDashBusinessGroupRequest request)
        {
            try
            {
                var response = await new PM_ProjectProfDash(_configuration)
                    .GetBusinessGroup(request ?? new ProfDashBusinessGroupRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetBusinessGroup

        // Added by Vyankat B. on 07-08-2026 - Project Group dropdown
        [HttpPost("GetProjectGroupForProfitability")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectGroupForProfitability()
        {
            try
            {
                var response = await new PM_ProjectProfDash(_configuration)
                    .GetProjectGroupForProfitability();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProjectGroupForProfitability

        // Added by Vyankat B. on 07-08-2026 - Currency dropdown
        [HttpPost("GetProfitabilityCurrency")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProfitabilityCurrency()
        {
            try
            {
                var response = await new PM_ProjectProfDash(_configuration)
                    .GetProfitabilityCurrency();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProfitabilityCurrency

        // Added by Vyankat B. on 07-08-2026 - Location / OU dropdown by Business Group
        [HttpPost("GetProfitabilityLocations")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProfitabilityLocations(
            [FromBody] ProfDashLocationsRequest request)
        {
            try
            {
                var response = await new PM_ProjectProfDash(_configuration)
                    .GetProfitabilityLocations(request ?? new ProfDashLocationsRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProfitabilityLocations

        // Added by Vyankat B. on 07-08-2026 - Accessible projects dropdown
        [HttpPost("GetProfitabilityAccessibleProjects")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProfitabilityAccessibleProjects(
            [FromBody] ProfDashAccessibleProjectsRequest request)
        {
            try
            {
                var response = await new PM_ProjectProfDash(_configuration)
                    .GetProfitabilityAccessibleProjects(request ?? new ProfDashAccessibleProjectsRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProfitabilityAccessibleProjects

        // Added by Vyankat B. on 10-08-2026 - Project profitability list by project group (renamed SP)
        [HttpPost("GetProjectProfitability")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProfitabilityProjectProfitByProjectGroup(
            [FromBody] ProfDashProjectProfitByProjectGroupRequest request)
        {
            try
            {
                var response = await new PM_ProjectProfDash(_configuration)
                    .GetProfitabilityProjectProfitByProjectGroup(
                        request ?? new ProfDashProjectProfitByProjectGroupRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of GetProfitabilityProjectProfitByProjectGroup

        // Added by Vyankat B. on 14-08-2026 - PDF export for Project Profit by Project Group
        [HttpPost("ExportPdf")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> ExportProjectProfitByProjectGroupPdf(
            [FromBody] ProfDashProjectProfitByProjectGroupRequest request)
        {
            try
            {
                var (pdfBytes, fileName, errorMessage) = await new PM_ProjectProfDash(_configuration, _webHostEnvironment)
                    .ExportProjectProfitByProjectGroupPdf(
                        request ?? new ProfDashProjectProfitByProjectGroupRequest());

                if (!string.IsNullOrWhiteSpace(errorMessage))
                    return BadRequest(errorMessage);

                if (pdfBytes == null || pdfBytes.Length == 0)
                    return NotFound("No data found for given input.");

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of ExportProjectProfitByProjectGroupPdf

        // Added by Vyankat B. on 14-08-2026 - Excel export for Project Profit by Project Group (no logo)
        [HttpPost("ExportExcel")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> ExportProjectProfitByProjectGroupExcel(
            [FromBody] ProfDashProjectProfitByProjectGroupRequest request)
        {
            try
            {
                var (excelBytes, fileName, errorMessage) = await new PM_ProjectProfDash(_configuration)
                    .ExportProjectProfitByProjectGroupExcel(
                        request ?? new ProfDashProjectProfitByProjectGroupRequest());

                if (!string.IsNullOrWhiteSpace(errorMessage))
                    return BadRequest(errorMessage);

                if (excelBytes == null || excelBytes.Length == 0)
                    return NotFound("No data found for given input.");

                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // End of ExportProjectProfitByProjectGroupExcel
    }
    // End of Added by Vyankat B. on 07-08-2026
}

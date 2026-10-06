

using Microsoft.AspNetCore.Authorization;
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
    // Added by Vikas T on 28-07-2026 for the Plan vs Actual Hours Dashboard
    [Route("api/[controller]")]
    [ApiController]
    public class PlanVsActualDashboardController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PlanVsActualDashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Added by Vikas T on 28-07-2026 - Populate the five cascading filter dropdowns
        [HttpPost("GetFilterMasters")]
       
        public async Task<IActionResult> GetFilterMasters(
            [FromBody] PlanVsActualFilterMastersRequest request)
        {
            try
            {
                var svc = new PlanVsActualDashboard(_configuration);
                var response = await svc.GetFilterMasters(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                // Deviation from the reference controller, which returns ex.Message and
                // leaks stored-procedure and parameter names to the client. KNOWN_ISSUES #12.
                System.Diagnostics.Debug.WriteLine($"GetFilterMasters Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetFilterMasters controller method

        // Added by Vikas T on 28-07-2026 - The four KPI cards
        [HttpPost("GetKPISummary")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetKPISummary(
            [FromBody] PlanVsActualFilterRequest request)
        {
            try
            {
                var svc = new PlanVsActualDashboard(_configuration);
                var response = await svc.GetKPISummary(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetKPISummary Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetKPISummary controller method

        // Added by Vikas T on 28-07-2026 - The three overview bar charts in one call
        [HttpPost("GetSummaryByLevel")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetSummaryByLevel(
            [FromBody] PlanVsActualFilterRequest request)
        {
            try
            {
                var svc = new PlanVsActualDashboard(_configuration);
                var response = await svc.GetSummaryByLevel(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSummaryByLevel Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetSummaryByLevel controller method

        // Added by Vikas T on 28-07-2026 - Trend line chart and variance heatmap
        [HttpPost("GetTrendData")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetTrendData(
            [FromBody] PlanVsActualTrendRequest request)
        {
            try
            {
                if (request == null || request.LevelFlag < 1 || request.LevelFlag > 3)
                    return BadRequest(new { message = "LevelFlag must be 1, 2 or 3." });

                var svc = new PlanVsActualDashboard(_configuration);
                var response = await svc.GetTrendData(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTrendData Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetTrendData controller method

        // Added by Vikas T on 28-07-2026 - Analysis table, re-grouped by LevelFlag 1..5
        [HttpPost("GetAnalysisTable")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalysisTable(
            [FromBody] PlanVsActualAnalysisRequest request)
        {
            try
            {
                if (request == null || request.LevelFlag < 1 || request.LevelFlag > 5)
                    return BadRequest(new { message = "LevelFlag must be between 1 and 5." });

                var svc = new PlanVsActualDashboard(_configuration);
                var response = await svc.GetAnalysisTable(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalysisTable Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalysisTable controller method

        // Added by Vishal.M on 12-08-2026 - Save default filters (Plan_VS_Actual Save button)
        // UI: POST /api/PlanVsActualDashboard/SaveDefaultFilter
        // SP: usp_InsUpd_Whizible2_PlanVsActual_DefaultFilter
        // Body: SessionEmployeeID (required), SessionProjectID, WhereClause / FilterJson
        // Response data: FilterID, Result ('1' insert / '2' update), WhereClause
        [HttpPost("SaveDefaultFilter")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> SaveDefaultFilter(
            [FromBody] PlanVsActualDefaultFilterRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is required." });

                if (string.IsNullOrWhiteSpace(request.SessionEmployeeID)
                    || !int.TryParse(request.SessionEmployeeID.Trim(), out var empId)
                    || empId <= 0)
                {
                    return BadRequest(new { message = "SessionEmployeeID is required and must be a valid integer." });
                }

                var whereClause = string.IsNullOrWhiteSpace(request.WhereClause)
                    ? request.WhereClause
                    : request.WhereClause;
                if (string.IsNullOrWhiteSpace(whereClause))
                    return BadRequest(new { message = "WhereClause (filter JSON) is required." });

                var svc = new PlanVsActualDashboard(_configuration);
                var response = await svc.SaveDefaultFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveDefaultFilter Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of SaveDefaultFilter controller method

        // Added by Vishal.M on 12-08-2026 - Load default filters on Plan_VS_Actual page open
        // UI: POST /api/PlanVsActualDashboard/GetDefaultFilter
        // SP: usp_Whizible2_Sel_tbl_Whizible2_PlanVsActual_DefaultFilter
        // Body: SessionEmployeeID (required); SessionProjectID / LoginID / PageKey accepted by API but not passed to Sel SP
        // Response data: FilterID, WhereClause / FilterJson (same Success shape as SaveDefaultFilter)
        [HttpPost("GetDefaultFilter")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetDefaultFilter(
            [FromBody] PlanVsActualDefaultFilterRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is required." });

                if (string.IsNullOrWhiteSpace(request.SessionEmployeeID)
                    || !int.TryParse(request.SessionEmployeeID.Trim(), out var empId)
                    || empId <= 0)
                {
                    return BadRequest(new { message = "SessionEmployeeID is required and must be a valid integer." });
                }

                var svc = new PlanVsActualDashboard(_configuration);
                var response = await svc.GetDefaultFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDefaultFilter Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetDefaultFilter controller method
    }
}

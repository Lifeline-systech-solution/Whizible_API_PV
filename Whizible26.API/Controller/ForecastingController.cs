
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
    // Added by <Name> on 12-08-2026 for the Forecasting Report Dashboard
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(ValidateHeadersAttribute))]
    public class ForecastingController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ForecastingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Added by <Name> on 12-08-2026 - Business Group / Origination Unit / Department / Role dropdowns
        [HttpPost("GetFilterMasters")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetFilterMasters()
        {
            try
            {
                var svc = new ForecastingDashboard(_configuration);
                var response = await svc.GetFilterMasters();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetFilterMasters Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetFilterMasters controller method

        // Added by <Name> on 12-08-2026 - Monthly forecast grid (KPI cards + paged rows)
        [HttpPost("GetMonthlyForecast")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetMonthlyForecast(
            [FromBody] ForecastingFilterRequest request)
        {
            try
            {
                var svc = new ForecastingDashboard(_configuration);
                var response = await svc.GetMonthlyForecast(request, CurrentUserId());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetMonthlyForecast Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetMonthlyForecast controller method

        // Added by <Name> on 12-08-2026 - Weekly forecast grid (KPI cards + paged rows)
        [HttpPost("GetWeeklyForecast")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetWeeklyForecast(
            [FromBody] ForecastingFilterRequest request)
        {
            try
            {
                var svc = new ForecastingDashboard(_configuration);
                var response = await svc.GetWeeklyForecast(request, CurrentUserId());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetWeeklyForecast Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetWeeklyForecast controller method

        // Added by <Name> on 12-08-2026 - Excel export (ClosedXML), full filtered set.
        // isWeekly picks the grid; defaults to the monthly view to match GetMonthlyForecast.
        [HttpPost("ExportForecastingExcel")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> ExportForecastingExcel(
            [FromBody] ForecastingExportRequest request,
            [FromQuery] bool isWeekly = false)
        {
            try
            {
                var svc = new ForecastingDashboard(_configuration);
                var bytes = await svc.ExportForecastingExcel(request, isWeekly, CurrentUserId());

                if (bytes == null || bytes.Length == 0)
                    return NoContent();

                var fileName = $"Forecasting_{(isWeekly ? "Weekly" : "Monthly")}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExportForecastingExcel Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of ExportForecastingExcel controller method

        // Added by <Name> on 12-08-2026 - PDF export (MigraDoc/PdfSharp), full filtered set
        [HttpPost("ExportForecastingPdf")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> ExportForecastingPdf(
            [FromBody] ForecastingExportRequest request,
            [FromQuery] bool isWeekly = false)
        {
            try
            {
                var svc = new ForecastingDashboard(_configuration);
                var bytes = await svc.ExportForecastingPdf(request, isWeekly, CurrentUserId());

                if (bytes == null || bytes.Length == 0)
                    return NoContent();

                var fileName = $"Forecasting_{(isWeekly ? "Weekly" : "Monthly")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExportForecastingPdf Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of ExportForecastingPdf controller method

        // Added by <Name> on 12-08-2026 - Pulls the authenticated user's EmployeeID claim,
        // matching how the legacy RR_ForeCastingController sourced intUserID from the
        // logged-in session rather than trusting a value in the request body.
        // TODO: confirm the actual claim type used elsewhere in Whizible26.API (this
        // assumes "EmployeeID"; adjust to match whatever TokenSessionValidator issues).
        private int? CurrentUserId()
        {
            var claim = User?.FindFirst("EmployeeID");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : (int?)null;
        }
    }
}

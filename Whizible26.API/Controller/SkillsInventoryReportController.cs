using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Whizible26.Application.CommandsQueries;
using Whizible26.Domain.Entity.ReportEntities.SkillsInventory;
using WhizibleAPI.API.Filters;
using WhizibleTeams.API.Attributes;
using WhizibleTeams.Application.Helpers;

namespace Whizible26.API.Controllers
{
    // Added by Vyankat on 07-08-2026 for the Skills Inventory Report
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsInventoryReportController : ControllerBase
    {
        private const string ExcelContentType =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Updated By Vyankat B. on 27th Aug 2026 - IWebHostEnvironment for PDF company logo.
        public SkillsInventoryReportController(
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        private SkillsInventoryReport CreateService()
            => new SkillsInventoryReport(_configuration, _webHostEnvironment);

        // Added by Vyankat on 07-08-2026 - Populate the Organization Unit and Skills filter cards
        [HttpPost("GetFilterMasters")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetFilterMasters(
            [FromBody] SkillsInventoryFilterMastersRequest request)
        {
            try
            {
                var svc = CreateService();
                var response = await svc.GetFilterMasters(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {          
                System.Diagnostics.Debug.WriteLine($"GetFilterMasters Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetFilterMasters controller method

        // Added by Vyankat on 07-08-2026 - The Generate Report button
        [HttpPost("GetSkillsInventory")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetSkillsInventory(
            [FromBody] SkillsInventoryFilterRequest request)
        {
            try
            {
              
                if (request == null
                    || (string.IsNullOrWhiteSpace(request.OrgUnitIDs)
                        && string.IsNullOrWhiteSpace(request.ToolIDs)))
                {
                    return BadRequest(new
                    {
                        message = "Select at least one Organization Unit or Skill "
                                + "before generating the report."
                    });
                }

                var svc = CreateService();
                var response = await svc.GetSkillsInventory(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSkillsInventory Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetSkillsInventory controller method

        // Added by Vyankat on 07-08-2026 - Named resources behind the counts (drill-down)
        [HttpPost("GetResourceDetail")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceDetail(
            [FromBody] SkillsInventoryFilterRequest request)
        {
            try
            {
                if (request == null
                    || (string.IsNullOrWhiteSpace(request.OrgUnitIDs)
                        && string.IsNullOrWhiteSpace(request.ToolIDs)))
                {
                    return BadRequest(new
                    {
                        message = "Select at least one Organization Unit or Skill "
                                + "before generating the report."
                    });
                }

                var svc = CreateService();
                var response = await svc.GetResourceDetail(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetResourceDetail Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetResourceDetail controller method

        [HttpPost("ExportSkillsInventoryExcel")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
       
        public async Task<IActionResult> ExportSkillsInventoryExcel(
            [FromBody] SkillsInventoryExportRequest request)
        {
            try
            {
                if (request == null
                    || (string.IsNullOrWhiteSpace(request.OrgUnitIDs)
                        && string.IsNullOrWhiteSpace(request.ToolIDs)))
                {
                    return BadRequest(new
                    {
                        message = "Select at least one Organization Unit or Skill "
                                + "before exporting the report."
                    });
                }

                var svc = CreateService();
                var (excelBytes, fileName) = await svc.ExportSkillsInventoryExcel(request);

                if (excelBytes == null || excelBytes.Length == 0)
                    return NotFound(new { message = "No data found for the selected filters." });

                return File(
                    excelBytes,
                    ExcelContentType,
                    fileName,
                    enableRangeProcessing: false);
                // End of Added By Vyankat B. on 27th Aug 2026
            }
            catch (Exception ex)
            {
               
                System.Diagnostics.Debug.WriteLine($"ExportSkillsInventoryExcel Error: {ex}");
                return StatusCode(500, new
                {
                    message = ex.Message
                        + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")
                });
                // End of Added By Vyankat B. on 27th Aug 2026
            }
        }
        // End of ExportSkillsInventoryExcel controller method

        // Added by Vyankat on 07-08-2026 - PDF export (#btnPdf)
        [HttpPost("ExportSkillsInventoryPdf")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> ExportSkillsInventoryPdf(
            [FromBody] SkillsInventoryExportRequest request)
        {
            try
            {
                if (request == null
                    || (string.IsNullOrWhiteSpace(request.OrgUnitIDs)
                        && string.IsNullOrWhiteSpace(request.ToolIDs)))
                {
                    return BadRequest(new
                    {
                        message = "Select at least one Organization Unit or Skill "
                                + "before exporting the report."
                    });
                }

                var svc = CreateService();
                var (pdfBytes, fileName) = await svc.ExportSkillsInventoryPdf(request);

                if (pdfBytes == null || pdfBytes.Length == 0)
                    return NotFound(new { message = "No data found for the selected filters." });

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                // Added By Vyankat B. on 27th Aug 2026
                System.Diagnostics.Debug.WriteLine($"ExportSkillsInventoryPdf Error: {ex}");
                return StatusCode(500, new
                {
                    message = ex.Message
                        + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")
                });
                // End of Added By Vyankat B. on 27th Aug 2026
            }
        }
        // End of ExportSkillsInventoryPdf controller method
    }
}

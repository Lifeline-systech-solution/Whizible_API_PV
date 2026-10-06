using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Whizible26.Application.CommandsQueries.RM;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleAPI.API.Filters;
using WhizibleTeams.API.Attributes;
using WhizibleTeams.Application.Helpers;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RM_CapacityPlanningController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RM_CapacityPlanningController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("GetCapacityPlanHeaderCounts")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetCapacityPlanHeaderCounts(
            [FromBody] CapacityPlanning_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCapacityPlanHeaderCounts(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCapacityPlanHeaderCounts Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCapacityPlanBGFilterList")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCapacityPlanBGFilterList(
            [FromBody] CapacityPlanning_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCapacityPlanBGFilterList(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCapacityPlanBGFilterList Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCapacityPlanOUFilterList")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCapacityPlanOUFilterList(
            [FromBody] CapacityPlanning_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCapacityPlanOUFilterList(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCapacityPlanOUFilterList Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCapacityPlanSkillFilterList")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCapacityPlanSkillFilterList(
            [FromBody] CapacityPlanning_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCapacityPlanSkillFilterList(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCapacityPlanSkillFilterList Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCapacityPlanRoleFilterList")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCapacityPlanRoleFilterList(
            [FromBody] CapacityPlanning_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCapacityPlanRoleFilterList(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCapacityPlanRoleFilterList Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_RoleWise_MonthHeader")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_RoleWise_MonthHeader(
            [FromBody] Cp_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCP_RoleWise_MonthHeader(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_RoleWise_MonthHeader Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_RoleWise_MonthDetails")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_RoleWise_MonthDetails(
            [FromBody] Cp_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCP_RoleWise_MonthDetails(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_RoleWise_MonthDetails Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_SkillWise_MonthHeader")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_SkillWise_MonthHeader(
            [FromBody] Cp_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCP_SkillWise_MonthHeader(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_SkillWise_MonthHeader Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_SkillWise_MonthDetails")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_SkillWise_MonthDetails(
            [FromBody] Cp_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCP_SkillWise_MonthDetails(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_SkillWise_MonthDetails Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_RoleWise_QtrHeader")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_RoleWise_QtrHeader(
            [FromBody] Cp_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCP_RoleWise_QtrHeader(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_RoleWise_QtrHeader Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_RoleWise_QtrDetails")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_RoleWise_QtrDetails(
            [FromBody] Cp_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCP_RoleWise_QtrDetails(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_RoleWise_QtrDetails Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_GettotalstrengthbyRoleorskill")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_GettotalstrengthbyRoleorskill(
            [FromBody] CP_Details parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GettotalstrengthbyRoleorskill(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_GettotalstrengthbyRoleorskill Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_GetProjectAllocationByRole")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_GetProjectAllocationByRole(
            [FromBody] CP_Details parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetProjectAllocationDetails(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_GetProjectAllocationByRole Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_GetProjectRequestByRole")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_GetProjectRequestByRole(
            [FromBody] CP_Details parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetProjectRequestByRole(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_GetProjectRequestByRole Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        // Intentionally keeps typo "Apportunity" — UI calls this exact route.
        [HttpPost("GetCP_ApportunityRequestByRole")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_ApportunityRequestByRole(
            [FromBody] CP_Details parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetApportunityRequestByRole(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_ApportunityRequestByRole Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_GetBenchByRole")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_GetBenchByRole(
            [FromBody] CP_Details parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetBenchByRole(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_GetBenchByRole Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_AnticipatedExitByRole")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_AnticipatedExitByRole(
            [FromBody] CP_Details parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetAnticipatedExitByRole(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_AnticipatedExitByRole Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_JoiningPoolByRole")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_JoiningPoolByRole(
            [FromBody] CP_Details parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetJoiningPoolByRole(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_JoiningPoolByRole Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_SkillWise_QuarterHeader")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_SkillWise_QuarterHeader(
            [FromBody] Cp_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCP_SW_QuarterHeader(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_SkillWise_QuarterHeader Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("GetCP_SkillWise_QuarterDetails")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetCP_SkillWise_QuarterDetails(
            [FromBody] Cp_Params parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCP_Sw_QuarterDetails(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCP_SkillWise_QuarterDetails Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("RoleWiseTrendAnalysis")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> RoleWiseTrendAnalysis(
            [FromBody] CP_RoleorSkilWiseTrendAnalysisRequest parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetRoleWiseTrendAnalysis(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RoleWiseTrendAnalysis Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("SkillWiseTrendAnalysis")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> SkillWiseTrendAnalysis(
            [FromBody] CP_RoleorSkilWiseTrendAnalysisRequest parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetSkillWiseTrendAnalysis(parameter);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SkillWiseTrendAnalysis Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        [HttpPost("DownloadCpExport")]
        [Authorize]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> DownloadCpExport(
            [FromBody] CP_ExportRequest parameter)
        {
            try
            {
                var svc = new CapacityPlanning(_configuration);
                var response = await svc.GetCpExportFile(parameter);

                if (response.Status != ResponseStatus.SUCCESS)
                    return BadRequest(response.Data);

                var data = response.Data;

                var fileName = data.GetType()
                    .GetProperty("FileName")?
                    .GetValue(data)?
                    .ToString();

                var fileBytes = data.GetType()
                    .GetProperty("FileBytes")?
                    .GetValue(data) as byte[];

                if (fileBytes == null || fileBytes.Length == 0)
                    return BadRequest("File data not available.");

                string contentType =
                    parameter.ReportFormat?.ToUpperInvariant() == "PDF"
                        ? "application/pdf"
                        : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DownloadCpExport Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
    }
}

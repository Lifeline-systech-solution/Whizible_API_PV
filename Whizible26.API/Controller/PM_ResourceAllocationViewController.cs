using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Whizible26.Application.CommandsQueries.Dashboard;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleAPI.API.Filters;
using WhizibleTeams.API.Attributes;
using WhizibleTeams.Application.Helpers;

namespace Whizible26.API.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class PM_ResourceAllocationViewController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PM_ResourceAllocationViewController(
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        private PM_ResourceAllocationView CreateService()
            => new PM_ResourceAllocationView(_configuration, _webHostEnvironment);

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_FinancialType
        [HttpPost("GetFinancialType")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        public async Task<IActionResult> GetFinancialType()
        {
            try
            {
                var response = await CreateService().GetFinancialType();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_FromAndToDates_ForReasAllocation
        [HttpPost("GetFromAndToDates")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetFromAndToDates(
            [FromBody] GetFromAndToDatesRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetFromAndToDates(request ?? new GetFromAndToDatesRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_ResourceAllocation_Dashboard
        [HttpPost("GetResourceAllocationDashboard")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceAllocationDashboard(
            [FromBody] GetResourceAllocationDashboardRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetResourceAllocationDashboard(
                        request ?? new GetResourceAllocationDashboardRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 13-08-2026 - usp_Whizible2_SEL_ResourceAllocation_Summary
        [HttpPost("GetResourceAllocationSummary")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceAllocationSummary(
            [FromBody] GetResourceAllocationSummaryRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetResourceAllocationSummary(
                        request ?? new GetResourceAllocationSummaryRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_ResourceAllocation_Dashboard_ResourceDetails
        [HttpPost("GetResourceAllocationDetails")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceAllocationDetails(
            [FromBody] GetResourceAllocationDetailsRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetResourceAllocationDetails(
                        request ?? new GetResourceAllocationDetailsRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 13-08-2026 - usp_Whizible2_Sel_GetAll_Dropdown
        [HttpPost("GetAllDropdown")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAllDropdown([FromBody] GetAllDropdownRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetAllDropdown(request ?? new GetAllDropdownRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_Res_tbl_PM_EmployeeName
        [HttpPost("GetEmployeeName")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetEmployeeName(
            [FromBody] GetEmployeeNameRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetEmployeeName(request ?? new GetEmployeeNameRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Sel_Res_tbl_PM_ProjectEmployeeRole
        [HttpPost("GetProjectEmployeeRole")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectEmployeeRole(
            [FromBody] GetProjectEmployeeRoleRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetProjectEmployeeRole(request ?? new GetProjectEmployeeRoleRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added By Vyankat B. on 24th Aug 2026 - usp_Whizible2_Sel_tbl_PM_Res_RowWiseExternalApprovers
        [HttpPost("GetRowWiseExternalApprovers")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetRowWiseExternalApprovers(
            [FromBody] RowWiseExternalApproversRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetRowWiseExternalApprovers(request ?? new RowWiseExternalApproversRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 11-08-2026 - usp_Whizible2_Upd_Res_tbl_PM_ProjectEmployeeRole
        [HttpPost("UpdateProjectEmployeeRole")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> UpdateProjectEmployeeRole(
            [FromBody] UpdateProjectEmployeeRoleRequest request)
        {
            try
            {
                var response = await CreateService()
                    .UpdateProjectEmployeeRole(
                        request ?? new UpdateProjectEmployeeRoleRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("GetIsWorkflowApprover")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetIsWorkflowApprover(
            [FromBody] IsWorkflowApproverRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetIsWorkflowApprover(request ?? new IsWorkflowApproverRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("GetMppTasks")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetMppTasks([FromBody] ProjectEmployeeIdsRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetMppTasks(request ?? new ProjectEmployeeIdsRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("GetProjectTools")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectTools(
            [FromBody] ProjectEmployeeIdsRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetProjectTools(request ?? new ProjectEmployeeIdsRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("GetTasksForCompletion")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetTasksForCompletion(
            [FromBody] ProjectEmployeeIdsRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetTasksForCompletion(request ?? new ProjectEmployeeIdsRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("GetTasksForVoiding")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetTasksForVoiding(
            [FromBody] ProjectEmployeeIdsRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetTasksForVoiding(request ?? new ProjectEmployeeIdsRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("GetTimesheetAndExpenseApproveeList")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetTimesheetAndExpenseApproveeList(
            [FromBody] ProjectEmployeeIdsRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetTimesheetAndExpenseApproveeList(request ?? new ProjectEmployeeIdsRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 12-08-2026 - usp_Whizible2_Sel_ResourceAllocation_AuditTrail
        [HttpPost("GetResourceAllocationAuditTrail")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceAllocationAuditTrail(
            [FromBody] GetResourceAllocationAuditTrailRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetResourceAllocationAuditTrail(
                        request ?? new GetResourceAllocationAuditTrailRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 12-08-2026 - usp_Whizible2_Sel_ResourceAllocationAudit_ModifiedField
        [HttpPost("GetResourceAllocationAuditModifiedField")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceAllocationAuditModifiedField(
            [FromBody] GetResourceAllocationAuditLookupRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetResourceAllocationAuditModifiedField(
                        request ?? new GetResourceAllocationAuditLookupRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Added by Vyankat B. on 12-08-2026 - usp_Whizible2_Sel_ResourceAllocationAudit_ModifiedBy
        [HttpPost("GetResourceAllocationAuditModifiedBy")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetResourceAllocationAuditModifiedBy(
            [FromBody] GetResourceAllocationAuditLookupRequest request)
        {
            try
            {
                var response = await CreateService()
                    .GetResourceAllocationAuditModifiedBy(
                        request ?? new GetResourceAllocationAuditLookupRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

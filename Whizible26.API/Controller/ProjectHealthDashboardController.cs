using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Whizible26.Application.CommandsQueries.Dashboard;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleAPI.API.Filters;
using WhizibleTeams.API.Attributes;
using WhizibleTeams.Application.Helpers;

namespace Whizible26.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(AuthorizeAuditAttribute))]
    [ServiceFilter(typeof(ValidateHeadersAttribute))]
    public class ProjectHealthDashboardController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProjectHealthDashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static IActionResult Fail(ControllerBase controller, string action, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"{action} Error: {ex}");
            return controller.StatusCode(500, new { message = "Something went wrong" });
        }

        #region My Filters (Save/Apply)

        [HttpPost("SaveFilter")]
        public async Task<IActionResult> SaveFilter([FromBody] PHSFilterSaveRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.SaveFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(SaveFilter), ex); }
        }

        [HttpPost("GetMyFilterList")]
        public async Task<IActionResult> GetMyFilterList([FromBody] PHSFilterListRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetMyFilterList(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetMyFilterList), ex); }
        }

        [HttpPost("GetFilterByID")]
        public async Task<IActionResult> GetFilterByID([FromBody] PHSFilterByIdRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetFilterByID(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetFilterByID), ex); }
        }

        [HttpPost("SetDefaultFilter")]
        public async Task<IActionResult> SetDefaultFilter([FromBody] PHSSetDefaultFilterRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.SetDefaultFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(SetDefaultFilter), ex); }
        }

        [HttpPost("DeleteFilter")]
        public async Task<IActionResult> DeleteFilter([FromBody] PHSFilterByIdRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.DeleteFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(DeleteFilter), ex); }
        }

        [HttpPost("CheckDefaultFilterSetOrNot")]
        public async Task<IActionResult> CheckDefaultFilterSetOrNot([FromBody] PHSDefaultFilterCheckRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.CheckDefaultFilterSetOrNot(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(CheckDefaultFilterSetOrNot), ex); }
        }

        [HttpPost("CheckFilterNameExists")]
        public async Task<IActionResult> CheckFilterNameExists([FromBody] PHSFilterExistsRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.CheckFilterNameExists(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(CheckFilterNameExists), ex); }
        }

        [HttpPost("GetPHSDefaultFilter")]
        public async Task<IActionResult> GetPHSDefaultFilter([FromBody] PHSDefaultFilterDetailsRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetPHSDefaultFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetPHSDefaultFilter), ex); }
        }

        #endregion

        #region Filters

        [HttpPost("GetReportingFrequency")]
        public async Task<IActionResult> GetReportingFrequency()
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetReportingFrequency();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetReportingFrequency), ex); }
        }

        [HttpPost("GetFilterMasters")]
        public async Task<IActionResult> GetFilterMasters([FromBody] PHSFilterMastersRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetFilterMasters(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetFilterMasters), ex); }
        }

        [HttpPost("GetOrganizationUnits")]
        public async Task<IActionResult> GetOrganizationUnits([FromBody] PHSOrganizationUnitRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetOrganizationUnits(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetOrganizationUnits), ex); }
        }

        [HttpPost("GetFilteredProjects")]
        public async Task<IActionResult> GetFilteredProjects([FromBody] PHSFilterProjectListRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetFilteredProjects(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetFilteredProjects), ex); }
        }

        [HttpPost("GetProjectList")]
        public async Task<IActionResult> GetProjectList([FromBody] PHSTopProjectListRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetProjectList(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetProjectList), ex); }
        }

        #endregion

        #region PHS information + KPI

        [HttpPost("GetPHSInformation")]
        public async Task<IActionResult> GetPHSInformation([FromBody] PHSInformationRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetPHSInformation(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetPHSInformation), ex); }
        }

        [HttpPost("GetKPISummary")]
        public async Task<IActionResult> GetKPISummary([FromBody] PHSKpiSummaryRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetKPISummary(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetKPISummary), ex); }
        }

        #endregion

        #region SQERT Grids

        [HttpPost("GetProjectsPendingLock")]
        public async Task<IActionResult> GetProjectsPendingLock([FromBody] PHSProjectPendingLockRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetProjectsPendingLock(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetProjectsPendingLock), ex); }
        }

        [HttpPost("GetSQERTList")]
        public async Task<IActionResult> GetSQERTList([FromBody] PHSSqertListRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetSQERTList(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetSQERTList), ex); }
        }

        [HttpPost("GetSQERTRange")]
        public async Task<IActionResult> GetSQERTRange()
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetSQERTRange();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetSQERTRange), ex); }
        }

        [HttpPost("GetSQERTSection")]
        public async Task<IActionResult> GetSQERTSection([FromBody] PHSSqertSectionRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetSQERTSection(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetSQERTSection), ex); }
        }

        #endregion

        #region Key achievement / Issue details / Milestone / Active resource

        [HttpPost("GetKeyAchievement")]
        public async Task<IActionResult> GetKeyAchievement([FromBody] PHSProjectScopeRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetKeyAchievement(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetKeyAchievement), ex); }
        }

        [HttpPost("GetIssueDetails")]
        public async Task<IActionResult> GetIssueDetails([FromBody] PHSProjectScopeRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetIssueDetails(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetIssueDetails), ex); }
        }

        [HttpPost("GetMilestoneDetails")]
        public async Task<IActionResult> GetMilestoneDetails([FromBody] PHSProjectScopeRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetMilestoneDetails(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetMilestoneDetails), ex); }
        }

        [HttpPost("GetActiveResources")]
        public async Task<IActionResult> GetActiveResources([FromBody] PHSActiveResourceRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetActiveResources(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetActiveResources), ex); }
        }

        #endregion

        #region Graphs

        [HttpPost("GetTaskVsCompletionGraph")]
        public async Task<IActionResult> GetTaskVsCompletionGraph([FromBody] PHSTaskVsCompletionRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetTaskVsCompletionGraph(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetTaskVsCompletionGraph), ex); }
        }

        [HttpPost("GetDelayInDaysGraph")]
        public async Task<IActionResult> GetDelayInDaysGraph([FromBody] PHSDelayInDaysRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetDelayInDaysGraph(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetDelayInDaysGraph), ex); }
        }

        [HttpPost("GetMonthlyResourceCostGraph")]
        public async Task<IActionResult> GetMonthlyResourceCostGraph([FromBody] PHSMonthlyResourceCostRequest request)
        {
            try
            {
                var svc = new ProjectHealthDashboard(_configuration);
                var response = await svc.GetMonthlyResourceCostGraph(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex) { return Fail(this, nameof(GetMonthlyResourceCostGraph), ex); }
        }

        #endregion
    }
}
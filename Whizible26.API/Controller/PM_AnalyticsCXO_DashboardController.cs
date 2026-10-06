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
    // Added by Dipali V. on 08-09-2026 for the Analytics CXO Dashboard
    [Route("api/[controller]")]
    [ApiController]
    public class PM_AnalyticsCXO_DashboardController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PM_AnalyticsCXO_DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Added by Dipali V. on 08-09-2026 - Flag-wise filter dropdown (ID, Name)
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBFilterFlagWise
        // SP: usp_Whizible2_Sel_AnalyticsDBFilterFlagWise
        // Body: DashboardID required. Flag optional — omit to load all chips (multi-select bind).
        //       Flag = Portfolio | Customer | Region | BillingType | Health | ProjectManager for one chip.
        // Returns: { FlagName: [ { id, name }, ... ], ... }
        [HttpPost("GetAnalyticsDBFilterFlagWise")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBFilterFlagWise(
            [FromBody] AnalyticsDBFilterFlagWiseRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBFilterFlagWise(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBFilterFlagWise Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBFilterFlagWise controller method

        // Added by Dipali - Portfolio-dependent chip masters
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBFilterFlagWiseDependent
        // SP: usp_Whizible2_Sel_AnalyticsDBFilterFlagWise_Dependent
        // Body: DashboardID, PortfolioIDs (optional), Flag optional
        [HttpPost("GetAnalyticsDBFilterFlagWiseDependent")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBFilterFlagWiseDependent(
            [FromBody] AnalyticsDBFilterFlagWiseDependentRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBFilterFlagWiseDependent(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBFilterFlagWiseDependent Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBFilterFlagWiseDependent controller method — Added by Dipali

        // Added by Dipali V. on 10-09-2026 - HighLevelRole greeting employee
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBRoleGreeting
        // SP: usp_Whizible2_Sel_AnalyticsDBRoleGreeting
        // Body: DashboardID, RoleID, UserID (SessionEmployeeID)
        [HttpPost("GetAnalyticsDBRoleGreeting")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBRoleGreeting(
            [FromBody] AnalyticsDBRoleGreetingRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBRoleGreeting(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBRoleGreeting Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBRoleGreeting controller method

        // Added by Dipali V. on 08-09-2026 - Date filter options from tbl_Whizible2_AnalyticsDBFilter
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetPageGenericfilters
        // SP: usp_Whizible2_Sel_PageGenericFilters
        [HttpPost("GetPageGenericfilters")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetPageGenericfilters(
            [FromBody] AnalyticsDBPageGenericFilterRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetPageGenericfilters(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPageGenericfilters Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetPageGenericfilters controller method

        // Added by Dipali V. on 08-09-2026 - Date picker (no comparison)
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBFilterDateRange
        // SP: usp_Whizible2_Sel_AnalyticsDBFilterDateRange
        // Function: udf_Whizible2_GetAnalyticsDBFilterDateRange
        // Body: DashboardID, Flag, StartDate, EndDate (Custom only)
        [HttpPost("GetAnalyticsDBFilterDateRange")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBFilterDateRange(
            [FromBody] AnalyticsDBFilterDateRangeRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBFilterDateRange(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBFilterDateRange Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBFilterDateRange controller method

        // Added by Dipali V. on 09-09-2026 - Comparison date range (IsComparison = 1 only)
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBFilterDateRangeUpdated
        // SP: usp_Whizible2_Sel_AnalyticsDBFilterDateRange_Updated
        // Function: udf_Whizible2_GetAnalyticsDBFilterDateRange_Updated
        // Body: DashboardID, FilterID (from first SP), IsComparison=true, ComparisonID,
        //       CustomCompStartDate / CustomCompEndDate = first SP CurrentStartDate / CurrentEndDate
        [HttpPost("GetAnalyticsDBFilterDateRangeUpdated")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBFilterDateRangeUpdated(
            [FromBody] AnalyticsDBFilterDateRangeUpdatedRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBFilterDateRangeUpdated(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBFilterDateRangeUpdated Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBFilterDateRangeUpdated controller method

        // Added by Dipali V. on 10-09-2026 - KPI cards (one endpoint, Flag-wise SPs)
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBKPICards
        // Body: DashboardID, calendar dates, chip ID lists; Flag optional (omit = all 12 cards)
        // Flag → SP: Revenue, GrossProfit, GrossMargin, EBIT, NetProfit, Utilization,
        //            Bench, ActiveProjects, DelayedProjects, BudgetVariance, CSAT, PortfolioHealth
        //            each maps to usp_Whizible2_Sel_AnalyticsDBKPI_{Flag}
        // Added by Vikas T on 16-09-2026 - Bind Resource Utilization, Bench, Active Projects cards
        // via same endpoint: SPs called at @DrillLevel=0; L0 columns mapped to CurrentValue/DeltaPct.
        [HttpPost("GetAnalyticsDBKPICards")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBKPICards(
            [FromBody] AnalyticsDBKPICardsRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBKPICards(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBKPICards Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBKPICards controller method

        // Added by Dipali V. on 15-09-2026 - Greeting project count (Portfolio + Customer)
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBConsolidatedProjectCount
        // SP: usp_Whizible2_Sel_AnalyticsDB_ConsolidatedProjectCount
        [HttpPost("GetAnalyticsDBConsolidatedProjectCount")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBConsolidatedProjectCount(
            [FromBody] AnalyticsDBConsolidatedProjectCountRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBConsolidatedProjectCount(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBConsolidatedProjectCount Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBConsolidatedProjectCount controller method

        // Added by Dipali V. on 11-09-2026 - Revenue KPI drill-through
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBRevenueDrillDown
        // SP: usp_Whizible2_Sel_AnalyticsDB_RevenueDrillDown
        // Body: DashboardID, Level (1/2/4 implemented), CurrentFromDate, CurrentToDate,
        //       PortfolioID (L2), ProjectID (L3), InvoiceID (L4+),
        //       PortfolioIDs, CustomerIDs, ProjectManagerIDs, RegionIDs, BillingTypeIDs, HealthIDs
        [HttpPost("GetAnalyticsDBRevenueDrillDown")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBRevenueDrillDown(
            [FromBody] AnalyticsDBRevenueDrillDownRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBRevenueDrillDown(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBRevenueDrillDown Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBRevenueDrillDown controller method

        // Added By Vyankat B. on the 16th sep 2026 for the Delayed Projects KPI drill-through
       
        [HttpPost("GetAnalyticsDBDelayedProjectsDrillDown")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBDelayedProjectsDrillDown(
            [FromBody] AnalyticsDBDelayedProjectsDrillDownRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBDelayedProjectsDrillDown(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBDelayedProjectsDrillDown Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of added By Vyankat B. on the 16th sep 2026 for the Delayed Projects KPI drill-through

        // Added By Vyankat B. on 21th Sep 2026 for the Budget Variance KPI drill-through
        [HttpPost("GetAnalyticsDBBudgetVarianceDrillDown")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBBudgetVarianceDrillDown(
            [FromBody] AnalyticsDBDelayedProjectsDrillDownRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBBudgetVarianceDrillDown(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBBudgetVarianceDrillDown Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added By Vyankat B. on 21th Sep 2026 for the Budget Variance KPI drill-through

        // Commented and Added By Vyankat b. on 24th Sep 2026 for the Customer CSAT KPI drill-through
        [HttpPost("GetAnalyticsDBCSATDrillDown")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBCSATDrillDown(
            [FromBody] AnalyticsDBCSATDrillDownRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBCSATDrillDown(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBCSATDrillDown Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Commented and Added By Vyankat b. on 24th Sep 2026 for the Customer CSAT KPI drill-through

        // Added by Vikas T on 17-09-2026 - Revenue vs Cost trend & forecast chart (SRS §4.1.2)
        // Updated by Vikas T on 23-09-2026 - Months follow CurrentFrom–CurrentTo; forecast from chart actuals
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBRevenueCostTrend
        // SP: usp_Whizible2_Sel_AnalyticsDB_RevenueCostTrend (inline money logic — no nested EXEC)
        // Body: DashboardID, CurrentFromDate, CurrentToDate, optional Previous*, chip ID lists
        // Returns: { Meta, Months } — Meta = chart subtitle/scope; Months = period months + trend forecast
        [HttpPost("GetAnalyticsDBRevenueCostTrend")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBRevenueCostTrend(
            [FromBody] AnalyticsDBRevenueCostTrendRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBRevenueCostTrend(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBRevenueCostTrend Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBRevenueCostTrend controller method

        // Added by Aditya J. on 15-09-2026 - Portfolio Revenue Mix (chart)
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBPortfolioRevenueMix
        // SP: usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMix
        // Body: same dates + chip IDs as GetAnalyticsDBKPICards
        [HttpPost("GetAnalyticsDBPortfolioRevenueMix")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBPortfolioRevenueMix(
            [FromBody] AnalyticsDBPortfolioRevenueMixRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBPortfolioRevenueMix(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBPortfolioRevenueMix Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBPortfolioRevenueMix controller method

        // Added by Vikas T on 21-09-2026 - Revenue vs Cost chart drill-through
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBPortfolioRevenueMixDrillDown
        // SP: usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMixDrillDown
        [HttpPost("GetAnalyticsDBPortfolioRevenueMixDrillDown")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBPortfolioRevenueMixDrillDown(
            [FromBody] AnalyticsDBPortfolioRevenueMixDrillDownRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBPortfolioRevenueMixDrillDown(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBPortfolioRevenueMixDrillDown Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBPortfolioRevenueMixDrillDown controller method

        // Added by Aditya J. on 17-09-2026 - Portfolio Revenue Mix Excel export.
        [HttpPost("ExportAnalyticsDBPortfolioRevenueMixExcel")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> ExportAnalyticsDBPortfolioRevenueMixExcel(
            [FromBody] AnalyticsDBPortfolioRevenueMixRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var bytes = await svc.ExportAnalyticsDBPortfolioRevenueMixExcel(request);
                return File(bytes, "application/vnd.ms-excel", "Portfolio-Revenue-Mix.xls");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExportAnalyticsDBPortfolioRevenueMixExcel Error: {ex}");
                return BadRequest(new { message = ex.Message });
            }
        }
        // End of ExportAnalyticsDBPortfolioRevenueMixExcel controller method

        // Added by Aditya J. on 17-09-2026 - Portfolio Revenue Mix drill-down Excel export.
        [HttpPost("ExportAnalyticsDBPortfolioRevenueMixDrillDownExcel")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> ExportAnalyticsDBPortfolioRevenueMixDrillDownExcel(
            [FromBody] AnalyticsDBPortfolioRevenueMixDrillDownRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var bytes = await svc.ExportAnalyticsDBPortfolioRevenueMixDrillDownExcel(request);
                var level = request != null && request.Level > 0 ? request.Level : 1;
                return File(bytes, "application/vnd.ms-excel", "Portfolio-Revenue-DrillDown-L" + level + ".xls");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExportAnalyticsDBPortfolioRevenueMixDrillDownExcel Error: {ex}");
                return BadRequest(new { message = ex.Message });
            }
        }
        // End of ExportAnalyticsDBPortfolioRevenueMixDrillDownExcel controller method

        //Added by Aditya J. on 21-09-2026 Cost & budget variance by project
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBCostBudgetVarianceByProject
        // SP: usp_Whizible2_Sel_AnalyticsDB_CostBudgetVarianceByProject
        // Body: DashboardID, CurrentFromDate, CurrentToDate + chip ID lists
        [HttpPost("GetAnalyticsDBCostBudgetVarianceByProject")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBCostBudgetVarianceByProject(
            [FromBody] AnalyticsDBCostBudgetVarianceByProjectRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBCostBudgetVarianceByProject(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBCostBudgetVarianceByProject Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        //End of Added by Aditya J. on 21-09-2026 Cost & budget variance by project

        // Added by Vikas T on 16-09-2026 - Utilization / Bench / Active Projects drill-through
        // Updated by Vikas T on 23-09-2026 - Util/Bench: Groups → Projects → People; Bench L3+ uses ProjectID (RoleID fallback)
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetAnalyticsDBKPIDrillDown
        // Body: DashboardID, Flag (Utilization|Bench|ActiveProjects), Level 1–4,
        //       dates, chip IDs, PortfolioID / ProjectID / EmployeeID / RoleID / MilestoneID / TaskID
        // SP: usp_Whizible2_Sel_AnalyticsDBKPI_{Flag} @DrillLevel = Level
        [HttpPost("GetAnalyticsDBKPIDrillDown")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetAnalyticsDBKPIDrillDown(
            [FromBody] AnalyticsDBKPIDrillDownRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetAnalyticsDBKPIDrillDown(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsDBKPIDrillDown Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetAnalyticsDBKPIDrillDown controller method

        // Added by Dipali V. on 09-09-2026 - Save view (user + DashboardID)
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/SaveUserFilter
        // SP: usp_Whizible2_InsUpd_AnalyticsDBUserFilter
        // Body: DashboardID (21036), SessionEmployeeID, FilterJson, IsDefaultFilter, optional UserFilterID / FilterName
        [HttpPost("SaveUserFilter")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> SaveUserFilter(
            [FromBody] AnalyticsDBUserFilterRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.SaveUserFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveUserFilter Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of SaveUserFilter controller method

        // Added by Dipali V. on 09-09-2026 - Get saved views / default for login persist
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/GetUserFilter
        // SP: usp_Whizible2_Sel_AnalyticsDBUserFilter
        // Body: DashboardID, SessionEmployeeID; GetDefaultOnly=true on page load
        [HttpPost("GetUserFilter")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetUserFilter(
            [FromBody] AnalyticsDBUserFilterRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.GetUserFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserFilter Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of GetUserFilter controller method

        // Added by Dipali V. on 09-09-2026 - Set / remove default saved view
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/SetDefaultUserFilter
        // SP: usp_Whizible2_Upd_AnalyticsDBUserFilter_SetDefault
        // Body: DashboardID, SessionEmployeeID, UserFilterID, IsDefaultFilter (true=1 / false=0)
        [HttpPost("SetDefaultUserFilter")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> SetDefaultUserFilter(
            [FromBody] AnalyticsDBUserFilterRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.SetDefaultUserFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetDefaultUserFilter Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of SetDefaultUserFilter controller method

        // Added by Dipali V. on 10-09-2026 - Soft-delete saved view (login + dashboard)
        // UI: POST /api/PM_AnalyticsCXO_Dashboard/DeleteUserFilter
        // SP: usp_Whizible2_Del_AnalyticsDBUserFilter
        // Body: DashboardID, SessionEmployeeID, UserFilterID
        [HttpPost("DeleteUserFilter")]
        //[Authorize]
        //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
        //[ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> DeleteUserFilter(
            [FromBody] AnalyticsDBUserFilterRequest request)
        {
            try
            {
                var svc = new PM_AnalyticsCXO_Dashboard(_configuration);
                var response = await svc.DeleteUserFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteUserFilter Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of DeleteUserFilter controller method
    }
}

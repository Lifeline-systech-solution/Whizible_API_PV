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
    // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectProfitByCustomerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProjectProfitByCustomerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API
        // Modified by Aditya J. on 12-08-2026 for FromQuery -> FromBody conversion
        [HttpPost("GetProjectProfitByCustomer")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectProfitByCustomer(
            [FromBody] ProjectProfitByCustomerRequest request)
        {
            try
            {
                var svc = new ProjectProfitByCustomer(_configuration);
                var response = await svc.GetProjectProfitByCustomer(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                // Deviation from the reference controller, which returns ex.Message and
                // leaks stored-procedure and parameter names to the client. KNOWN_ISSUES #12.
                System.Diagnostics.Debug.WriteLine($"GetProjectProfitByCustomer Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API

        // Added by Aditya J. on 07-08-2026 for Gross Profit Margin API
        // Modified by Aditya J. on 12-08-2026 for FromQuery -> FromBody conversion
        [HttpPost("GetGpm")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetGpm([FromBody] ProjectProfitabilityGpmRequest request)
        {
            try
            {
                var svc = new ProjectProfitByCustomer(_configuration);
                var response = await svc.GetGpm(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetGpm Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        //End of Added by Aditya J. on 07-08-2026 for Gross Profit Margin API

        // Added by Aditya J. on 07-08-2026 for Project Task Case Structure API
        // Modified by Aditya J. on 12-08-2026 for FromQuery -> FromBody conversion
        [HttpPost("GetTaskCaseStructure")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetTaskCaseStructure([FromBody] ProjectTaskCaseStructureRequest request)
        {
            try
            {
                var svc = new ProjectProfitByCustomer(_configuration);
                var response = await svc.GetTaskCaseStructure(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTaskCaseStructure Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 07-08-2026 for Project Task Case Structure API

        // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer Access Filter API
        [HttpGet("GetRoleLevelAccessFilter")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetRoleLevelAccessFilter(
            [FromQuery] ProjectProfitByCustomerAccessFilterRequest request)
        {
            try
            {
                var svc = new ProjectProfitByCustomer(_configuration);
                var response = await svc.GetRoleLevelAccessFilter(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetRoleLevelAccessFilter Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer Access Filter API

        // Added by Aditya J. on 12-08-2026 for Customer Dropdown API
        [HttpPost("GetCustomerDropdown")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetCustomerDropdown([FromBody] CustomerDropdownRequest request)
        {
            try
            {
                var svc = new ProjectProfitByCustomer(_configuration);
                var response = await svc.GetCustomerDropdown(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCustomerDropdown Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 12-08-2026 for Customer Dropdown API

        // Added by Aditya J. on 12-08-2026 for Project Dropdown API
        [HttpPost("GetProjectDropdown")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectDropdown([FromBody] ProjectDropdownRequest request)
        {
            try
            {
                var svc = new ProjectProfitByCustomer(_configuration);
                var response = await svc.GetProjectDropdown(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetProjectDropdown Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 12-08-2026 for Project Dropdown API

        // Added By Vyankat B. on 24th Aug 2026 for the Project Profit Filter Masters API
        [HttpPost("GetFilterMasters")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetFilterMasters(
            [FromBody] ProjectProfitFilterMastersRequest request)
        {
            try
            {
                var svc = new ProjectProfitByCustomer(_configuration);
                var response = await svc.GetFilterMasters(
                    request ?? new ProjectProfitFilterMastersRequest());
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetFilterMasters Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added By Vyankat B. on 24th Aug 2026 for the Project Profit Filter Masters API
    }
    // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API
}

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
    // Added by Aditya J. on 11-08-2026 for Project Billing dashboard API
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectBillingController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProjectBillingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Added by Aditya J. on 11-08-2026 for Project Billing Sales Period dropdown API
        [HttpPost("GetSalesPeriods")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetSalesPeriods([FromBody] ProjectBillingSalesPeriodRequest request)
        {
            try
            {
                var svc = new ProjectBilling(_configuration);
                var response = await svc.GetSalesPeriods(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                // Deviation from the reference controller, which returns ex.Message and
                // leaks stored-procedure and parameter names to the client. KNOWN_ISSUES #12.
                System.Diagnostics.Debug.WriteLine($"GetSalesPeriods Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing Sales Period dropdown API

        // Added by Aditya J. on 11-08-2026 for Project Billing Business Group dropdown API
        [HttpPost("GetBusinessGroups")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetBusinessGroups()
        {
            try
            {
                var svc = new ProjectBilling(_configuration);
                var response = await svc.GetBusinessGroups();
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetBusinessGroups Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing Business Group dropdown API

        // Added by Aditya J. on 11-08-2026 for Project Billing Organization Unit dropdown API
        [HttpPost("GetLocationsForBusinessGroup")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetLocationsForBusinessGroup([FromBody] ProjectBillingLocationRequest request)
        {
            try
            {
                var svc = new ProjectBilling(_configuration);
                var response = await svc.GetLocationsForBusinessGroup(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLocationsForBusinessGroup Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing Organization Unit dropdown API

        // Added by Aditya J. on 11-08-2026 for Project Billing main grid API
        [HttpPost("GetProjectBilling")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetProjectBilling([FromBody] ProjectBillingRequest request)
        {
            try
            {
                var svc = new ProjectBilling(_configuration);
                var response = await svc.GetProjectBilling(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetProjectBilling Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing main grid API

        // Added by Aditya J. on 11-08-2026 for Project Billing customer-details popup API
        // Called when the user clicks a "No." value (e.g. IRs Made, Invoices Made) on the main grid.
        [HttpPost("GetCustomerDetails")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetCustomerDetails([FromBody] ProjectBillingCustomerDetailsRequest request)
        {
            try
            {
                var svc = new ProjectBilling(_configuration);
                var response = await svc.GetCustomerDetails(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCustomerDetails Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing customer-details popup API

        // Added by Aditya J. on 11-08-2026 for Project Billing invoice-details popup API
        // Called when the user clicks a Customer link inside the customer-details popup.
        [HttpPost("GetInvoiceDetails")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetInvoiceDetails([FromBody] ProjectBillingInvoiceDetailsRequest request)
        {
            try
            {
                var svc = new ProjectBilling(_configuration);
                var response = await svc.GetInvoiceDetails(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetInvoiceDetails Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing invoice-details popup API

        // Added by Aditya J. on 07-08-2026 for Project Billing Access Filter API        
        [HttpPost("GetRoleLevelAccessFilter")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetRoleLevelAccessFilter(
            [FromBody] ProjectProfitByCustomerAccessFilterRequest request)
        {
            try
            {
                //'ProjectID',61,'E',2,1,NULL
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
        // End of Added by Aditya J. on 07-08-2026 for Project Billing Access Filter API

        // Added by Aditya J. on 11-08-2026 for Project Billing company base currencies dropdown API
        [HttpPost("GetCompanyBaseCurrencies")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetCompanyBaseCurrencies([FromBody] ProjectBillingCompanyBaseCurrencyRequest request)
        {
            try
            {
                var svc = new ProjectBilling(_configuration);
                var response = await svc.GetCompanyBaseCurrencies(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCompanyBaseCurrencies Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing company base currencies dropdown API

        // Added by Aditya J. on 11-08-2026 for Project Billing company base currency amount API
        // Called by dashboard tiles (IRs Made, Invoices Made, PDF Files Sent, Physical Invoices Dispatched,
        // Softex Forms Required, Softex Forms Sent) to fetch a single amount converted into the selected company base currency.
        [HttpPost("GetCompanyBaseCurrencyAmount")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeAuditAttribute))]
        [ServiceFilter(typeof(ValidateHeadersAttribute))]
        public async Task<IActionResult> GetCompanyBaseCurrencyAmount([FromBody] ProjectBillingCompanyBaseCurrencyAmountRequest request)
        {
            try
            {
                var svc = new ProjectBilling(_configuration);
                var response = await svc.GetCompanyBaseCurrencyAmount(request);
                return ResponseHelper.BuildResponse(this, response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCompanyBaseCurrencyAmount Error: {ex}");
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing company base currency amount API
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing dashboard API
}

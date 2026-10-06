using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.Repository.DashboardRepo;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.Application.CommandsQueries.Dashboard
{
    // Added by Aditya J. on 11-08-2026 for Project Billing dashboard API
    public class ProjectBilling
    {
        private readonly IConfiguration _configuration;
        private readonly ProjectBillingRepo _repository;
        private readonly string _connectionString;

        // Added by Aditya J. on 11-08-2026 for Project Billing dashboard API - default constructor required by the repository pattern
        public ProjectBilling() { }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing dashboard API

        // Added by Aditya J. on 11-08-2026 for Project Billing dashboard API - initialise the service with configuration
        public ProjectBilling(IConfiguration configuration)
        {
            _configuration = configuration;
            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(
                    _configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);
                _repository = new ProjectBillingRepo(_connectionString);
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing dashboard API

        #region Helpers

        // Added by Aditya J. on 11-08-2026 for Project Billing dashboard API - guard against an unconfigured repository
        private ResponseEntity RepositoryNotConfigured()
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message = "Database connection is not configured." }
            };
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing dashboard API

        // Added by Aditya J. on 11-08-2026 for Project Billing dashboard API - Empty / Swagger "string" -> no filter
        private static string? NullIfBlank(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            if (trimmed.Equals("string", StringComparison.OrdinalIgnoreCase)) return null;
            return trimmed;
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing dashboard API

        // Added by Aditya J. on 11-08-2026 for Project Billing dashboard API - CSV of ints; no valid int -> NULL (no filter)
        private static string? NullIfBlankIntCsv(string? value)
        {
            var trimmed = NullIfBlank(value);
            if (trimmed == null) return null;

            var valid = trimmed.Split(',')
                .Select(p => p.Trim())
                .Where(p => p.Length > 0 && int.TryParse(p, out _))
                .ToList();

            return valid.Count == 0 ? null : string.Join(",", valid);
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing dashboard API

        private static ResponseEntity Success(string message, object data)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.SUCCESS,
                Data = new { message, data }
            };
        }

        private static ResponseEntity Failure(Exception ex)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new
                {
                    message = ex.Message
                        + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")
                }
            };
        }

        #endregion

        // Added by Aditya J. on 11-08-2026 for Project Billing Sales Period dropdown API
        public async Task<ResponseEntity> GetSalesPeriods(ProjectBillingSalesPeriodRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectBillingSalesPeriodRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intSalesPeriodID", (object?)request.SalesPeriodID ?? DBNull.Value)
                };

                var result = await _repository.GetAsyncSP<ProjectBillingSalesPeriodModel>(
                    "usp_SEL_Whizible2_SalesPeriodMaster_Dashboard", sqlParams);

                return Success("Sales periods", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing Sales Period dropdown API

        // Added by Aditya J. on 11-08-2026 for Project Billing Business Group dropdown API
        public async Task<ResponseEntity> GetBusinessGroups()
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();

                var result = await _repository.GetAsyncSP<ProjectBillingBusinessGroupModel>(
                    "usp_SEL_Whizible2_BusinessGroup_Dashboard", new List<SqlParameter>());

                return Success("Business groups", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing Business Group dropdown API

        // Added by Aditya J. on 11-08-2026 for Project Billing Organization Unit dropdown API
        public async Task<ResponseEntity> GetLocationsForBusinessGroup(ProjectBillingLocationRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectBillingLocationRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intBusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value)
                };

                var result = await _repository.GetAsyncSP<ProjectBillingLocationModel>(
                    "usp_SEL_Whizible2_Location_For_BG_Dashboard", sqlParams);

                return Success("Organization units", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing Organization Unit dropdown API

        // Added by Aditya J. on 11-08-2026 for Project Billing main grid API
        public async Task<ResponseEntity> GetProjectBilling(ProjectBillingRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectBillingRequest();

                var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
                var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intSalesPeriodID", request.SalesPeriodID),
                    new SqlParameter("@intBusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
                    new SqlParameter("@intLocationID", (object?)request.LocationID ?? DBNull.Value),
                    new SqlParameter("@strProjectID", (object?)NullIfBlankIntCsv(request.ProjectIDs) ?? DBNull.Value),
                    new SqlParameter("@intPageNumber", pageNumber),
                    new SqlParameter("@intPageSize", pageSize)
                };

                // 1 result set -> the main grid rows for the requested page, each carrying TotalRecords
                var result = await _repository.GetAsyncSP<ProjectBillingModel>(
                    "usp_SEL_Whizible2_ProjectBilling", sqlParams);
                var resultList = result as IEnumerable<ProjectBillingModel>;
                //var totalRecords = result?.FirstOrDefault()?.TotalRecords ?? 0;
                var totalRecords = resultList?.FirstOrDefault()?.TotalRecords ?? 0;
                var totalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize);

                return Success("Project billing", new
                {
                    rows = result,
                    pageNumber,
                    pageSize,
                    totalRecords,
                    totalPages
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing main grid API

        // Added by Aditya J. on 11-08-2026 for Project Billing customer-details popup API
        public async Task<ResponseEntity> GetCustomerDetails(ProjectBillingCustomerDetailsRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectBillingCustomerDetailsRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intSalesPeriodID", request.SalesPeriodID),
                    new SqlParameter("@intCompanyID", request.CompanyID),
                    new SqlParameter("@intBusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
                    new SqlParameter("@intLocationID", (object?)request.LocationID ?? DBNull.Value),
                    new SqlParameter("@intType", request.Type),
                    new SqlParameter("@intCompanyBaseCurrencyID", request.CompanyBaseCurrencyID),
                    new SqlParameter("@strProjectID", (object?)NullIfBlankIntCsv(request.ProjectIDs) ?? DBNull.Value)
                };

                var result = await _repository.GetAsyncSP<ProjectBillingCustomerDetailModel>(
                    "usp_SEL_Whizible2_ProjectBilling_CustomerDetails", sqlParams);

                return Success("Project billing customer details", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing customer-details popup API

        // Added by Aditya J. on 11-08-2026 for Project Billing invoice-details popup API
        public async Task<ResponseEntity> GetInvoiceDetails(ProjectBillingInvoiceDetailsRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectBillingInvoiceDetailsRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intCustomerID", request.CustomerID),
                    new SqlParameter("@intSalesPeriodID", request.SalesPeriodID),
                    new SqlParameter("@intCompanyID", request.CompanyID),
                    new SqlParameter("@intBusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
                    new SqlParameter("@intLocationID", (object?)request.LocationID ?? DBNull.Value),
                    new SqlParameter("@intType", request.Type),
                    new SqlParameter("@intCompanyBaseCurrencyID", request.CompanyBaseCurrencyID),
                    new SqlParameter("@strProjectID", (object?)NullIfBlankIntCsv(request.ProjectIDs) ?? DBNull.Value)
                };

                var result = await _repository.GetAsyncSP<ProjectBillingInvoiceDetailModel>(
                    "usp_SEL_Whizible2_ProjectBilling_InvoiceDetails", sqlParams);

                return Success("Project billing invoice details", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing invoice-details popup API

        // Added by Aditya J. on 07-08-2026 for Project Billing Access Filter API
        //public async Task<ResponseEntity> GetRoleLevelAccessFilter(ProjectProfitByCustomerAccessFilterRequest request)
        //{
        //    try
        //    {
        //        if (_repository == null) return RepositoryNotConfigured();
        //        if (request == null) request = new ProjectProfitByCustomerAccessFilterRequest();

        //        var sqlParams = new List<SqlParameter>
        //{
        //    new SqlParameter("@strAccessParameter", request.AccessParameter),
        //    new SqlParameter("@intUserID", request.UserID),
        //    new SqlParameter("@strLoginType", request.LoginType),
        //    new SqlParameter("@intRoleLevel", request.RoleLevel),
        //    new SqlParameter("@blnShowReleasedProjects", request.ShowReleasedProjects),
        //    new SqlParameter("@intLoginID", (object?)request.LoginID ?? DBNull.Value)
        //};

        //        var result = await _repository.GetAsyncSP<ProjectProfitByCustomerAccessFilterModel>(
        //            "usp_QRB_Whizible2_ProjectProfitByCustomer_AccessFilter", sqlParams);

        //        return Success("Project billing access filter", result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Failure(ex);
        //    }
        //}
        // End of Added by Aditya J. on 07-08-2026 for Project Billing Access Filter API

        // Added by Aditya J. on 11-08-2026 for Project Billing company base currencies dropdown API
        public async Task<ResponseEntity> GetCompanyBaseCurrencies(ProjectBillingCompanyBaseCurrencyRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectBillingCompanyBaseCurrencyRequest();

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@intCompanyID", (object?)request.CompanyID ?? DBNull.Value)
                };

                var result = await _repository.GetAsyncSP<ProjectBillingCompanyBaseCurrencyModel>(
                    "usp_SEL_Whizible2_CompanyBaseCurrencies", sqlParams);

                return Success("Company base currencies", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing company base currencies dropdown API

        // Added by Aditya J. on 11-08-2026 for Project Billing company base currency amount API
        public async Task<ResponseEntity> GetCompanyBaseCurrencyAmount(ProjectBillingCompanyBaseCurrencyAmountRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new ProjectBillingCompanyBaseCurrencyAmountRequest();

                var sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@intSalesPeriodID", request.SalesPeriodID),
            new SqlParameter("@intBusinessGroupID", (object?)request.BusinessGroupID ?? DBNull.Value),
            new SqlParameter("@intLocationID", (object?)request.LocationID ?? DBNull.Value),
            new SqlParameter("@strProjectID", (object?)NullIfBlankIntCsv(request.ProjectIDs) ?? DBNull.Value),
            new SqlParameter("@intCompanyID", request.CompanyID),
            new SqlParameter("@intCompanyBaseCurrencyID", request.CompanyBaseCurrencyID),
            // @strType has no default in the SP (NOT NULL) - fall back to empty string rather than DBNull
            new SqlParameter("@strType", NullIfBlank(request.Type) ?? string.Empty)
        };

                // 1 result set: single row, single Amount column
                var result = await _repository.GetAsyncSP<ProjectBillingCompanyBaseCurrencyAmountModel>(
                    "usp_SEL_Whizible2_ProjectBilling_CompanyBaseCurrencyAmount", sqlParams);

                return Success("Project billing company base currency amount", result);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added by Aditya J. on 11-08-2026 for Project Billing company base currency amount API
    }
}

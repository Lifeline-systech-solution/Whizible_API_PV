
using System;
using System.Collections.Generic;

namespace Whizible26.Domain.Entity.DashboardEntities
{
    #region Requests

    // Added by Aditya J. on 11-08-2026 for Project Billing Sales Period dropdown API
    public class ProjectBillingSalesPeriodRequest
    {
        /// <summary>tbl_PM_SalesPeriodMaster.SalesPeriodID. null = all periods (plus the two rolling financial-year options).</summary>
        public int? SalesPeriodID { get; set; }
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing Sales Period dropdown API

    // Added by Aditya J. on 11-08-2026 for Project Billing Organization Unit dropdown API
    public class ProjectBillingLocationRequest
    {
        /// <summary>tbl_CNF_BusinessGroups.BusinessGroupID. null = all locations.</summary>
        public int? BusinessGroupID { get; set; }
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing Organization Unit dropdown API

    // Added by Aditya J. on 11-08-2026 for Project Billing main grid API
    /// <summary>
    /// Request payload for the Project Billing main grid.
    /// Mirrors the parameters of usp_SEL_Whizible2_ProjectBilling.
    /// </summary>
    public class ProjectBillingRequest
    {
        /// <summary>tbl_PM_SalesPeriodMaster.SalesPeriodID. -1 = previous financial year, -2 = current financial year, &gt;0 = a specific period.</summary>
        public string? SalesPeriodID { get; set; }

        /// <summary>tbl_CNF_BusinessGroups.BusinessGroupID. null = all business groups.</summary>
        public string? BusinessGroupID { get; set; }

        /// <summary>tbl_PM_Location.LocationID. null = all organization units.</summary>
        public string? LocationID { get; set; }

        /// <summary>CSV of tbl_PM_Project.ProjectID. null/empty = no rows (matches legacy CharIndex filter).</summary>
        public string? ProjectIDs { get; set; }

        /// <summary>1-based page number.</summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>Rows per page.</summary>
        public int PageSize { get; set; } = 10;
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing main grid API

    // Added by Aditya J. on 11-08-2026 for Project Billing customer-details popup API
    /// <summary>
    /// Request payload for the "Invoice Dashboard Details" popup showing the customer-wise
    /// breakdown of a single billing metric. Mirrors usp_SEL_Whizible2_ProjectBilling_CustomerDetails.
    /// </summary>
    public class ProjectBillingCustomerDetailsRequest
    {
        public int SalesPeriodID { get; set; }
        public int CompanyID { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? LocationID { get; set; }

        /// <summary>1=IRs Made, 2=Invoices Made, 3=PDF Files Sent, 4=Physical Invoices Dispatched, 5=Softex Forms Required, 6=Softex Forms Sent.</summary>
        public int Type { get; set; }

        /// <summary>0 = use each row's base currency amount as-is; otherwise convert to this currency.</summary>
        public int CompanyBaseCurrencyID { get; set; } = 0;

        public string? ProjectIDs { get; set; }
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing customer-details popup API

    // Added by Aditya J. on 11-08-2026 for Project Billing invoice-details popup API
    /// <summary>
    /// Request payload for the "Invoice Dashboard Details" popup showing the document-level
    /// (IR/Invoice) list for a single customer. Mirrors usp_SEL_Whizible2_ProjectBilling_InvoiceDetails.
    /// </summary>
    public class ProjectBillingInvoiceDetailsRequest
    {
        /// <summary>tbl_PM_Customer.Customer.</summary>
        public int CustomerID { get; set; }
        public int SalesPeriodID { get; set; }
        public int CompanyID { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? LocationID { get; set; }

        /// <summary>1=IRs, 2=Invoices, 3=PDF Files Sent, 4=Physical Invoices Dispatched, 5=Softex Forms Required, 6=Softex Forms Sent.</summary>
        public int Type { get; set; }

        public int CompanyBaseCurrencyID { get; set; } = 0;

        public string? ProjectIDs { get; set; }
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing invoice-details popup API
    // Added by Aditya J. on 11-08-2026 for Project Billing company base currencies dropdown API
    public class ProjectBillingCompanyBaseCurrencyRequest
    {
        /// <summary>tbl_PM_CompanyMaster.CompanyID.</summary>
        public int? CompanyID { get; set; }
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing company base currencies dropdown API
    #endregion

    #region Response

    // Added by Aditya J. on 11-08-2026 for Project Billing Sales Period dropdown API
    public class ProjectBillingSalesPeriodModel
    {
        public int SalesPeriodID { get; set; }
        public string SalesPeriod { get; set; } = string.Empty;
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing Sales Period dropdown API

    // Added by Aditya J. on 11-08-2026 for Project Billing Business Group dropdown API
    public class ProjectBillingBusinessGroupModel
    {
        public int BusinessGroupID { get; set; }
        public string BusinessGroup { get; set; } = string.Empty;
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing Business Group dropdown API

    // Added by Aditya J. on 11-08-2026 for Project Billing Organization Unit dropdown API
    public class ProjectBillingLocationModel
    {
        public int LocationID { get; set; }
        public string Location { get; set; } = string.Empty;
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing Organization Unit dropdown API

    // Added by Aditya J. on 11-08-2026 for Project Billing main grid API
    /// <summary>
    /// Result-set row for the Project Billing main grid (one row per Company).
    /// </summary>
    public class ProjectBillingModel
    {
        public int CompanyId { get; set; }
        public string Company { get; set; } = string.Empty;

        public int? TotalRFI { get; set; }
        public double? RFIAmount { get; set; }

        public int? InvoiceCount { get; set; }
        public double? InvoiceAmount { get; set; }

        public int? PDFFilesSent { get; set; }
        public double? PDFFilesSentAmount { get; set; }

        public int? PhysicalInvoicesCount { get; set; }
        public double? PhysicalInvoicesAmount { get; set; }

        public int? SoftexFormsRequired { get; set; }
        public double? SoftexFormsAmount { get; set; }

        public int? SoftexFormsSent { get; set; }
        public double? SoftexFormsSentAmount { get; set; }
        public string? BaseCurrencyCode { get; set; }

        /// <summary>Full (pre-paging) row count, repeated on every row so the caller can compute total pages.</summary>
        public int TotalRecords { get; set; }
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing main grid API

    // Added by Aditya J. on 11-08-2026 for Project Billing customer-details popup API
    public class ProjectBillingCustomerDetailModel
    {
        /// <summary>tbl_PM_Customer.Customer.</summary>
        public int Customer { get; set; }
        public string? CustomerName { get; set; }
        public int NoOfDocuments { get; set; }
        public double? Amount { get; set; }
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing customer-details popup API

    // Added by Aditya J. on 11-08-2026 for Project Billing invoice-details popup API
    public class ProjectBillingInvoiceDetailModel
    {
        public int InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? RFITypeName { get; set; }
        public string? CurrencyCode { get; set; }
        public double? Amount { get; set; }
        public double? BaseAmount { get; set; }
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing invoice-details popup API

    // Added by Aditya J. on 11-08-2026 for Project Billing company base currencies dropdown API
    public class ProjectBillingCompanyBaseCurrencyModel
    {
        public int CurrencyID { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing company base currencies dropdown API

    // Added by Aditya J. on 11-08-2026 for Project Billing company base currency amount API
    /// <summary>
    /// Request payload for a single billing-metric dashboard tile (RFI Amount, Invoice Amount,
    /// PDF Files Sent Amount, Physical Invoices Amount, Softex Forms Amount, Softex Forms Sent Amount),
    /// converted to the company's base currency. Mirrors usp_SEL_Whizible2_ProjectBilling_CompanyBaseCurrencyAmount.
    /// </summary>
    public class ProjectBillingCompanyBaseCurrencyAmountRequest
    {
        /// <summary>tbl_PM_SalesPeriodMaster.SalesPeriodID. -1 = previous financial year, -2 = current financial year, &gt;0 = a specific period.</summary>
        public int SalesPeriodID { get; set; }

        /// <summary>tbl_CNF_BusinessGroups.BusinessGroupID. null = all business groups.</summary>
        public int? BusinessGroupID { get; set; }

        /// <summary>tbl_PM_Location.LocationID. null = all organization units.</summary>
        public int? LocationID { get; set; }

        /// <summary>CSV of tbl_PM_Project.ProjectID. null/empty = NO project restriction (legacy semantics -
        /// unlike GetProjectBilling, where null/empty means no rows).</summary>
        public string? ProjectIDs { get; set; }

        /// <summary>tbl_PM_CompanyMaster.CompanyID.</summary>
        public int CompanyID { get; set; }

        /// <summary>Currency the amount is converted into.</summary>
        public int CompanyBaseCurrencyID { get; set; }

        /// <summary>RFIAmount, InvoiceAmount, PDFFilesSentAmount, PhysicalInvoicesAmount, SoftexFormsAmount, or SoftexFormsSentAmount.
        /// Any other value falls back to SoftexFormsSentAmount (legacy CASE ELSE behaviour, preserved as-is).</summary>
        public string Type { get; set; } = string.Empty;
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing company base currency amount API

    // Added by Aditya J. on 11-08-2026 for Project Billing company base currency amount API
    public class ProjectBillingCompanyBaseCurrencyAmountModel
    {
        public double? Amount { get; set; }
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing company base currency amount API
    #endregion
}

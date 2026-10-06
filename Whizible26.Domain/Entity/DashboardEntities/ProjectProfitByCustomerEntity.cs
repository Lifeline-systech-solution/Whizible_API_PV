
using System;
using System.Collections.Generic;

namespace Whizible26.Domain.Entity.DashboardEntities
{
    #region Requests

    // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API
    /// <summary>
    /// Request payload for the Project Profitability By Customer grid.
    /// Mirrors the parameters of usp_SEL_Whizible2_ProjectProfitByCustomer.
    /// </summary>
    public class ProjectProfitByCustomerRequest
    {
        /// <summary>tbl_PM_Customer.Customer. null = all customers.</summary>
        public int? CustomerID { get; set; }

        /// <summary>CSV of tbl_PM_Project.ProjectID. null/empty = all projects.</summary>
        public string? ProjectIDs { get; set; }

        /// <summary>Single ProjectID filter. null = no additional filter.</summary>
        public int? ProjectID { get; set; }

        /// <summary>1 = GPM Positive/Zero trend only, -1 = GPM Negative trend only, 0 = all.</summary>
        public int GPMTrend { get; set; } = 0;

        // Added By Vyankat B. on 24th Aug 2026
        /// <summary>CSV of tbl_CNF_BusinessGroups.BusinessGroupID. null/empty = all.</summary>
        public string? BusinessGroupIDs { get; set; }

        /// <summary>CSV of tbl_PM_Location.LocationID (Organization Unit). null/empty = all.</summary>
        public string? OrganizationUnitIDs { get; set; }

        /// <summary>
        /// CSV of GetFilterMasters PeriodValue: current_fy, previous_fy, next_fy,
        /// current_month, previous_month, next_month, current_quarter,
        /// previous_quarter, next_quarter, current_year, previous_year, next_year.
        /// null/empty = latest snapshot (no period filter).
        /// </summary>
        public string? Period { get; set; }
        // End of Added By Vyankat B. on 24th Aug 2026
    }
    // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API

    #endregion

    #region Response

    // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API
    /// <summary>
    /// Result-set row for the Project Profitability By Customer main grid.
    /// </summary>
    public class ProjectProfitByCustomerModel
    {
        public int ProjectProfitabilityID { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? ProjectCurrencyID { get; set; }
        public int? BaseCurrencyID { get; set; }

        public double? AccruedRevenue { get; set; }
        public double? InvoiceRevenue { get; set; }
        public double? AccruedCost { get; set; }
        public double? AccruedGPM { get; set; }
        public double? AccruedGPMPercent { get; set; }

        public int? BusinessGroupID { get; set; }
        public int? LocationID { get; set; }
        public string? Location { get; set; }
        public string? BusinessGroup { get; set; }

        public string? CustomerName { get; set; }
        public string? CurrencySymbol { get; set; }
        public int? Customer { get; set; }
        public string? BaseCurrencySymbol { get; set; }
    }
    // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer main grid API

    // Added by Aditya J. on 07-08-2026 for Gross Profit Margin API
    public class ProjectProfitabilityGpmRequest
    {
        public int ProjectID { get; set; }
    }

    public class ProjectProfitabilityGpmModel
    {
        public int ProjectProfitabilityID { get; set; }
        public int ProjectID { get; set; }
        public double? PeopleRevenue { get; set; }
        public double? AccruedBillableExpenses { get; set; }
        public double? Cost { get; set; }
        public double? Total { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double? PeopleCost { get; set; }
        public double? TotalCost { get; set; }
        public double? BasePeopleRevenue { get; set; }
        public double? BaseAccruedBillableExpenses { get; set; }
        public double? BaseCost { get; set; }
        public double? BaseTotal { get; set; }
        public double? BasePeopleCost { get; set; }
        public double? BaseTotalCost { get; set; }
        public string? Revenue { get; set; }
        public string? CurrencySymbol { get; set; }
    }
    // End of Added by Aditya J. on 07-08-2026 for Gross Profit Margin API

    // Added by Aditya J. on 07-08-2026 for Project Task Case Structure API
    public class ProjectTaskCaseStructureRequest
    {
        public int ProjectId { get; set; }
    }

    public class ProjectTaskCaseStructureModel
    {
        public bool? HaveSubTaskTypes { get; set; }
        public bool? ApplyEffortDistribution { get; set; }
        public int? LocationID { get; set; }
        public string? ExpectedStartDate { get; set; }
        public string? ExpectedEndDate { get; set; }
        public bool? Over { get; set; }
        public bool ResourceValidation { get; set; }
        public bool? Billable { get; set; }
        public int? RevisionNo { get; set; }
        public bool? ResourceLevelTaskCompletion { get; set; }
        public bool? EnforceConstraints { get; set; }
        public string? ProjectName { get; set; } = string.Empty;
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int? ContractType { get; set; }
        public int? BaseCurrency { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? NodeLabel { get; set; }
        public decimal? ContractValue { get; set; }
        public double EstimatedEfforts { get; set; }
        public double WorkingHours { get; set; }
        public string? CostMethod { get; set; }
    }
    // End of Added by Aditya J. on 07-08-2026 for Project Task Case Structure API

    // Added by Aditya J. on 12-08-2026 for Customer Dropdown API
    /// <summary>
    /// Request payload for the Customer filter dropdown.
    /// Mirrors the parameters of usp_SEL_Whizible2_ProjectProfitByCustomer_CustomerDropdown.
    /// </summary>
    public class CustomerDropdownRequest
    {
        /// <summary>tbl_PM_Customer.Customer. null = all customers.</summary>
        public int? CustomerID { get; set; }
    }

    /// <summary>
    /// Result-set row for the Customer filter dropdown.
    /// </summary>
    public class CustomerDropdownModel
    {
        public int Customer { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }
    // End of Added by Aditya J. on 12-08-2026 for Customer Dropdown API

    // Added by Aditya J. on 12-08-2026 for Project Dropdown API
    /// <summary>
    /// Request payload for the Project filter dropdown.
    /// Mirrors the parameters of usp_SEL_Whizible2_ProjectProfitByCustomer_ProjectDropdown.
    /// </summary>
    public class ProjectDropdownRequest
    {
        /// <summary>tbl_PM_Project.CustomerID. null = all customers.</summary>
        public int? ProjectTypeId { get; set; }

        /// <summary>tbl_PM_Project.BusinessGroupID. null = no filter.</summary>
        public int? BusinessgroupID { get; set; }

        /// <summary>tbl_PM_Project.LocationID. null = no filter.</summary>
        public int? OraganizationUnitID { get; set; }

        /// <summary>tbl_PM_Project.ProjectGroupId. null = no filter.</summary>
        public int? ProjectGroupID { get; set; }

        /// <summary>tbl_PM_Project.ProjectTypeId. null = no filter.</summary>
        public int? EmployeeID { get; set; }

        /// <summary>CSV of tbl_PM_Project.ProjectID. null/empty = no additional filter.</summary>
        public string? ProjectIDs { get; set; }
        public string? LoginType { get; set; }
        public int? CustomerID { get; set; }
    }

    /// <summary>
    /// Result-set row for the Project filter dropdown.
    /// </summary>
    public class ProjectDropdownModel
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
    }
    // End of Added by Aditya J. on 12-08-2026 for Project Dropdown API

    // Added By Vyankat B. on 24th Aug 2026 for the Project Profit Filter Masters API
    /// <summary>
    /// Request for usp_Whizible2_Sel_ProjectProfit_FilterMasters.
    /// Result sets: Business Group → Organization Unit → Project → Customer → Period.
    /// </summary>
    public class ProjectProfitFilterMastersRequest
    {
        public int? LoginID { get; set; }

        /// <summary>tbl_PM_Customer.Customer. null or 0 = all customers.</summary>
        public int? CustomerID { get; set; }
    }

    public class ProjectProfitFilterBusinessGroupModel
    {
        public int BusinessGroupID { get; set; }
        public string BusinessGroupName { get; set; } = string.Empty;
    }

    public class ProjectProfitFilterOrganizationUnitModel
    {
        public int OrganizationUnitID { get; set; }
        public string OrganizationUnitName { get; set; } = string.Empty;
        public int? BusinessGroupID { get; set; }
    }

    public class ProjectProfitFilterProjectModel
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int? OrganizationUnitID { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? GlobalProject { get; set; }
    }

    public class ProjectProfitFilterCustomerModel
    {
        public int Customer { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }

    public class ProjectProfitFilterPeriodModel
    {
        public string PeriodValue { get; set; } = string.Empty;
        public string PeriodText { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
    // End of Added By Vyankat B. on 24th Aug 2026 for the Project Profit Filter Masters API

    // Added by Aditya J. on 07-08-2026 for Project Profitability By Customer Access Filter API
    public class ProjectProfitByCustomerAccessFilterRequest
    {
        public string AccessParameter { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string LoginType { get; set; } = "E";
        public int RoleLevel { get; set; } = 3;
        public bool ShowReleasedProjects { get; set; } = true;
        public int? LoginID { get; set; }
    }

    public class ProjectProfitByCustomerAccessFilterModel
    {
        public int? ProjectID { get; set; }
    }
    // End of Added by Aditya J. on 07-08-2026 for Project Profitability By Customer Access Filter API
    #endregion
}

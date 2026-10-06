using System;
using System.Collections.Generic;

namespace Whizible26.Domain.Entity.DashboardEntities
{
    #region Requests

    // Added by Dipali V. on 08-09-2026 - Flag-wise filter master for PM_AnalyticsCXO_Dashboard
    /// <summary>
    /// Request for GetAnalyticsDBFilterFlagWise.
    /// Flag optional: omit / blank = load all chip masters in one call (multi-select bind).
    /// One Flag = that chip only. Values: Portfolio, Customer, Region, BillingType, Health, ProjectManager.
    /// HighLevelRole is for the CXO role pill (single-select); omit Flag still returns chip masters only.
    /// </summary>
    public class AnalyticsDBFilterFlagWiseRequest
    {
        /// <summary>
        /// Page key. CXO Dashboard = 21036. Required for generic reuse.
        /// </summary>
        public int DashboardID { get; set; }

        /// <summary>
        /// One chip name, or omit to return every chip list. Spaces ignored (Billing Type = BillingType).
        /// HighLevelRole = corporate role list for execRoleBtn.
        /// </summary>
        public string? Flag { get; set; }
    }
    // End of AnalyticsDBFilterFlagWiseRequest

    // Added by Dipali - Portfolio-dependent chip masters
    /// <summary>
    /// Request for GetAnalyticsDBFilterFlagWiseDependent.
    /// SP: usp_Whizible2_Sel_AnalyticsDBFilterFlagWise_Dependent
    /// When PortfolioIDs blank → same as FlagWise (all). When set → options for projects under those portfolios.
    /// </summary>
    public class AnalyticsDBFilterFlagWiseDependentRequest
    {
        /// <summary>Page key. CXO Dashboard = 21036.</summary>
        public int DashboardID { get; set; }

        /// <summary>
        /// One chip name, or omit to return dependent chip lists
        /// (Customer, ProjectManager, Health, Region, BillingType).
        /// </summary>
        public string? Flag { get; set; }

        /// <summary>Comma-separated ProjectGroupIDs. Blank = all (FlagWise behaviour).</summary>
        public string? PortfolioIDs { get; set; }
    }
    // End of AnalyticsDBFilterFlagWiseDependentRequest — Added by Dipali

    // Added by Dipali V. on 10-09-2026 - Role greeting (Top 1 employee for HighLevelRole)
    /// <summary>
    /// Request for GetAnalyticsDBRoleGreeting.
    /// SP: usp_Whizible2_Sel_AnalyticsDBRoleGreeting — RoleID + UserID (SessionEmployeeID).
    /// </summary>
    public class AnalyticsDBRoleGreetingRequest
    {
        /// <summary>Page key. CXO Dashboard = 21036.</summary>
        public int DashboardID { get; set; }

        /// <summary>Selected HighLevelRole RoleID.</summary>
        public int RoleID { get; set; }

        /// <summary>Session user EmployeeID — preferred when that user holds the role.</summary>
        public string? UserID { get; set; }
    }
    // End of AnalyticsDBRoleGreetingRequest

    // Added by Dipali V. on 08-09-2026 - Request for GetAnalyticsDBFilterDateRange
    /// <summary>
    /// Date picker only. Calls usp_Whizible2_Sel_AnalyticsDBFilterDateRange
    /// (udf_Whizible2_GetAnalyticsDBFilterDateRange). No comparison.
    /// Flag is FilterID or FilterName. StartDate / EndDate required only for Custom (FilterID = 0).
    /// </summary>
    public class AnalyticsDBFilterDateRangeRequest
    {
        /// <summary>Page key. CXO Dashboard = 21036.</summary>
        public int DashboardID { get; set; }
        public string? Flag { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
    // End of AnalyticsDBFilterDateRangeRequest

    // Added by Dipali V. on 09-09-2026 - Request for GetAnalyticsDBFilterDateRangeUpdated
    /// <summary>
    /// Comparison only. Call when IsComparison = 1.
    /// FilterID / CustomCompStartDate / CustomCompEndDate come from the first date-range SP
    /// (FilterID, CurrentStartDate, CurrentEndDate). Those current dates are the Start / End textboxes.
    /// </summary>
    public class AnalyticsDBFilterDateRangeUpdatedRequest
    {
        /// <summary>Page key. CXO Dashboard = 21036.</summary>
        public int DashboardID { get; set; }

        /// <summary>FilterID from GetAnalyticsDBFilterDateRange.</summary>
        public int FilterID { get; set; }

        /// <summary>Must be true / 1. This endpoint is comparison only.</summary>
        public bool IsComparison { get; set; }

        /// <summary>Comparison option FilterID (15-20) or 1-5.</summary>
        public int ComparisonID { get; set; }

        /// <summary>CurrentStartDate from first SP / Start date textbox.</summary>
        public string? CustomCompStartDate { get; set; }

        /// <summary>CurrentEndDate from first SP / End date textbox.</summary>
        public string? CustomCompEndDate { get; set; }
    }
    // End of AnalyticsDBFilterDateRangeUpdatedRequest

    // Added by Dipali V. on 09-09-2026 - Request for GetPageGenericfilters
    public class AnalyticsDBPageGenericFilterRequest
    {
        /// <summary>Page key. CXO Dashboard = 21036.</summary>
        public int DashboardID { get; set; }

        /// <summary>
        /// null = date + comparison rows.
        /// false = date options only (IsComparison = 0).
        /// true = comparison options only (IsComparison = 1).
        /// </summary>
        public bool? IsComparison { get; set; }
    }
    // End of AnalyticsDBPageGenericFilterRequest

    // Added by Dipali V. on 09-09-2026 - Save / get / set-default user filter view
    /// <summary>
    /// User-specific saved view. FilterJson holds date-range + flag-wise selections.
    /// IsDefaultFilter = true is applied when the same user opens the dashboard.
    /// </summary>
    public class AnalyticsDBUserFilterRequest
    {
        /// <summary>Page key. CXO Dashboard = 21036.</summary>
        public int DashboardID { get; set; }
        public string? SessionEmployeeID { get; set; }
        public int? UserFilterID { get; set; }
        public string? FilterName { get; set; }
        public string? FilterJson { get; set; }
        public bool IsDefaultFilter { get; set; }
        public bool GetDefaultOnly { get; set; }
    }
    // End of AnalyticsDBUserFilterRequest

    // Added by Dipali V. on 10-09-2026 - Shared filters + Flag for KPI cards
    /// <summary>
    /// One endpoint for all CXO KPI cards. Same filter bag for every Flag.
    /// Flag omitted = all registered cards; Flag = one card (e.g. Revenue).
    /// Multi-select IDs are comma-separated (from top chips).
    /// </summary>
    public class AnalyticsDBKPICardsRequest
    {
        /// <summary>Page key. CXO Dashboard = 21036.</summary>
        public int DashboardID { get; set; }

        /// <summary>Optional. Revenue, GrossProfit, … or omit for all.</summary>
        public string? Flag { get; set; }

        public string? CurrentFromDate { get; set; }
        public string? CurrentToDate { get; set; }
        public string? PreviousFromDate { get; set; }
        public string? PreviousToDate { get; set; }

        public string? PortfolioIDs { get; set; }
        public string? CustomerIDs { get; set; }
        public string? ProjectManagerIDs { get; set; }
        public string? RegionIDs { get; set; }
        public string? BillingTypeIDs { get; set; }
        public string? HealthIDs { get; set; }

        /// <summary>Optional HighLevelRole selection for KPI scoping.</summary>
        public int? RoleID { get; set; }

        /// <summary>Session user EmployeeID when RoleID is passed.</summary>
        public string? UserID { get; set; }
    }
    // End of AnalyticsDBKPICardsRequest

    // Added by Dipali V. on 15-09-2026 - Greeting consolidated project count
    /// <summary>
    /// Project count for CXO greeting subline from Portfolio + Customer combination.
    /// SP: usp_Whizible2_Sel_AnalyticsDB_ConsolidatedProjectCount
    /// </summary>
    public class AnalyticsDBConsolidatedProjectCountRequest
    {
        public int DashboardID { get; set; }
        /// <summary>Selected period start (same as KPI / Revenue drill).</summary>
        public string? CurrentFromDate { get; set; }
        /// <summary>Selected period end (same as KPI / Revenue drill).</summary>
        public string? CurrentToDate { get; set; }
        /// <summary>Comma-separated ProjectGroupIDs (selected or all options).</summary>
        public string? PortfolioIDs { get; set; }
        /// <summary>Comma-separated CustomerIDs (selected or all options).</summary>
        public string? CustomerIDs { get; set; }
        public string? ProjectManagerIDs { get; set; }
        public string? RegionIDs { get; set; }
        public string? BillingTypeIDs { get; set; }
        public string? HealthIDs { get; set; }
    }
    // End of AnalyticsDBConsolidatedProjectCountRequest

    /// <summary>Greeting consolidated counts (align Revenue drill L1/L2 scope).</summary>
    public class AnalyticsDBConsolidatedProjectCountModel
    {
        public int ProjectCount { get; set; }
        public int PortfolioCount { get; set; }
        public int CustomerCount { get; set; }
    }
    // End of AnalyticsDBConsolidatedProjectCountModel

    #endregion

    #region Responses

    // Added by Dipali V. on 08-09-2026 - Unified ID/Name row for all filter flags
    /// <summary>
    /// Result set from usp_Whizible2_Sel_AnalyticsDBFilterFlagWise.
    /// </summary>
    public class AnalyticsDBFilterFlagWiseModel
    {
        public string ID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
    // End of AnalyticsDBFilterFlagWiseModel

    // Added by Dipali V. on 10-09-2026 - Role greeting row
    /// <summary>
    /// Result set from usp_Whizible2_Sel_AnalyticsDBRoleGreeting.
    /// </summary>
    public class AnalyticsDBRoleGreetingModel
    {
        public string EmployeeID { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string RoleID { get; set; } = string.Empty;
        public string RoleDescription { get; set; } = string.Empty;
    }
    // End of AnalyticsDBRoleGreetingModel

    // Added by Dipali V. on 08-09-2026 - GetPageGenericfilters result set
    /// <summary>
    /// Active rows from tbl_Whizible2_AnalyticsDBFilter.
    /// </summary>
    public class PageGenericFilterModel
    {
        public int FilterID { get; set; }
        public string FilterName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int? DashboardID { get; set; }
        public bool IsComparison { get; set; }
    }
    // End of PageGenericFilterModel

    // Added by Dipali V. on 08-09-2026 - GetAnalyticsDBFilterDateRange result set
    /// <summary>
    /// Same columns from both date-range SPs:
    /// FilterID, FilterName, GetDate, CurrentStartDate, CurrentEndDate,
    /// PreviousStartDate, PreviousEndDate.
    /// </summary>
    public class AnalyticsDBFilterDateRangeModel
    {
        public int FilterID { get; set; }
        public string FilterName { get; set; } = string.Empty;
        public DateTime? GetDate { get; set; }
        public DateTime? CurrentStartDate { get; set; }
        public DateTime? CurrentEndDate { get; set; }
        public DateTime? PreviousStartDate { get; set; }
        public DateTime? PreviousEndDate { get; set; }
    }
    // End of AnalyticsDBFilterDateRangeModel

    // Added by Dipali V. on 09-09-2026 - Saved user filter view
    public class AnalyticsDBUserFilterModel
    {
        public int UserFilterID { get; set; }
        public int DashboardID { get; set; }
        public int EmployeeID { get; set; }
        public string? FilterName { get; set; }
        public string? FilterJson { get; set; }
        public bool IsDefaultFilter { get; set; }
        public string? Result { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    // End of AnalyticsDBUserFilterModel

    // Added by Dipali V. on 10-09-2026 - Common KPI card shape for UI bind
    /// <summary>
    /// Unified row for every CXO KPI card.
    /// Common SP columns: CurrentValue, PreviousValue, ValueChange, PercentageChange,
    /// TrendDirection, TrendColorCode, Note, BaseCurrencyCode.
    /// </summary>
    public class AnalyticsDBKPICardModel
    {
        public string Flag { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal PreviousValue { get; set; }
        public decimal ValueChange { get; set; }
        public decimal DeltaPct { get; set; }
        public string Trend { get; set; } = "FLAT";
        public string? TrendColorCode { get; set; }
        public string? Note { get; set; }
        /// <summary>Currency symbol from SP (e.g. ₹, S$).</summary>
        public string? BaseCurrencyCode { get; set; }
        /// <summary>ISO / master currency code (INR, SGD, USD) when available.</summary>
        public string? CurrencyCode { get; set; }
        public bool GoodUp { get; set; } = true;
        public string Unit { get; set; } = string.Empty;
        public string? Insight { get; set; }
        // Added By Vyankat B. on the 16th sep 2026 for the Delayed Projects watch badge
        public string? Badge { get; set; }
        // End of added By Vyankat B. on the 16th sep 2026 for the Delayed Projects watch badge
        public string? SpName { get; set; }
        public bool IsImplemented { get; set; }
    }
    // End of AnalyticsDBKPICardModel

    // Added by Dipali V. on 10-09-2026 - Common SP result for every KPI Flag
    /// <summary>
    /// SP numerics mapped as string so AutoMapper.DataReader does not fail on
    /// FLOAT / DECIMAL → decimal|double (Destination Member: CurrentValue).
    /// Parsed to decimal in MapKpiCard.
    /// Added by Vikas T on 16-09-2026 - L0 aliases also read into these properties:
    /// UtilizationPct / BenchFTE / ActiveProjectsCount → CurrentValue;
    /// PreviousUtilizationPct / PreviousBenchFTE / PreviousActiveProjectsCount → PreviousValue;
    /// DeltaPct → PercentageChange.
    /// </summary>
    public class AnalyticsDBKPISpRowModel
    {
        public string? CurrentValue { get; set; }
        public string? PreviousValue { get; set; }
        public string? ValueChange { get; set; }
        public string? PercentageChange { get; set; }
        public string? TrendDirection { get; set; }
        public string? TrendColorCode { get; set; }
        public string? Note { get; set; }
        // Commented and Added By Vyankat b. on 24th Sep 2026 for the CSAT Goal Below badge
        public string? Badge { get; set; }
        // End of Commented and Added By Vyankat b. on 24th Sep 2026 for the CSAT Goal Below badge
        public string? BaseCurrencyCode { get; set; }

        /// <summary>Money scale from the KPI SP (k / L / Cr / M / Abs). Blank when current value is 0.</summary>
        public string? Unit { get; set; }

        // Legacy Revenue SP column names (optional)
        public string? CurrentRevenue { get; set; }
        public string? PreviousRevenue { get; set; }
        public string? RevenueChange { get; set; }

        /// <summary>ISO / master currency code (INR, SGD, USD) when SP returns it.</summary>
        public string? CurrencyCode { get; set; }
    }
    // End of AnalyticsDBKPISpRowModel

    // Added by Dipali V. on 11-09-2026 - Revenue drill-through (6 levels)
    /// <summary>
    /// Request for GetAnalyticsDBRevenueDrillDown.
    /// SP: usp_Whizible2_Sel_AnalyticsDB_RevenueDrillDown
    /// Implemented SP levels today: 1 Portfolios, 2 Projects, 4 Invoice lines.
    /// Pass chip multi-select IDs the same way as KPI cards.
    /// </summary>
    public class AnalyticsDBRevenueDrillDownRequest
    {
        public int DashboardID { get; set; }

        /// <summary>1–6 (see SP). Current SP implements 1, 2, 4.</summary>
        public int Level { get; set; } = 1;

        public string? CurrentFromDate { get; set; }
        public string? CurrentToDate { get; set; }

        /// <summary>Selected portfolio for Level 2.</summary>
        public int? PortfolioID { get; set; }

        /// <summary>Selected project for Level 3+.</summary>
        public int? ProjectID { get; set; }

        /// <summary>InvoiceID for Level 4+ (tbl_PM_RFI_Items.InvoiceID).</summary>
        public string? InvoiceID { get; set; }

        /// <summary>
        /// Level 5+: selected line-item source ID from L4.
        /// Pass ProjectTimesheetID, or ExpensesEntryID, or DeliverableID, or MilestoneID, or EmployeeID
        /// (whichever was set on the clicked invoice line). SP: @InvoiceItemIDs.
        /// </summary>
        public string? InvoiceItemIDs { get; set; }

        /// <summary>Chip multi-select ID lists (comma-separated), same as KPI cards.</summary>
        public string? PortfolioIDs { get; set; }
        public string? CustomerIDs { get; set; }
        public string? ProjectManagerIDs { get; set; }
        public string? RegionIDs { get; set; }
        public string? BillingTypeIDs { get; set; }
        public string? HealthIDs { get; set; }
    }
    // End of AnalyticsDBRevenueDrillDownRequest

    /// <summary>
    /// Payload: Level + rows.
    /// L1: PortfolioID, Portfolio, Projects, TotalRevenue, TotalCost, RevenueFormatted, CostFormatted, GrossMarginPct, GrossMarginFormatted
    /// L2: ProjectID, Project, ProjectCode, Customer, PM, Billing, Revenue, RevenueFormatted, Margin, MarginFormatted, Health
    /// L3: InvoiceID, Invoice Number, RawInvoiceAmount, Amount, Status, Invoice Date, RawInvoiceDate, PO Ref
    /// L4: Line, ItemDescription, Qty (h), Rate, Amount, BaseCurrencyAmount, CurrencySymbol + source IDs
    /// L5: SourceType + Timesheet OR Milestone OR Deliverable columns (@InvoiceItemIDs = mapped IR id)
    /// </summary>
    public class AnalyticsDBRevenueDrillDownModel
    {
        public int Level { get; set; }
        public string LevelLabel { get; set; } = string.Empty;
        public List<Dictionary<string, string?>> Rows { get; set; } = new();
    }
    // End of AnalyticsDBRevenueDrillDownModel

    // Added By Vyankat B. on the 16th sep 2026 for the Delayed Projects KPI drill-through
    /// <summary>
    /// Request for GetAnalyticsDBDelayedProjectsDrillDown.
    /// L1 delayed project grid, L2 milestones, L3 tasks, L4 activity.
    /// </summary>
    public class AnalyticsDBDelayedProjectsDrillDownRequest
    {
        public int DashboardID { get; set; }
        public int Level { get; set; } = 1;

        public string? CurrentFromDate { get; set; }
        public string? CurrentToDate { get; set; }
        public string? PreviousFromDate { get; set; }
        public string? PreviousToDate { get; set; }

        public int? ProjectID { get; set; }
        public int? ItemID { get; set; }
        public string? ItemType { get; set; }
        public int? TaskID { get; set; }

        public string? PortfolioIDs { get; set; }
        public string? CustomerIDs { get; set; }
        public string? ProjectManagerIDs { get; set; }
        public string? RegionIDs { get; set; }
        public string? BillingTypeIDs { get; set; }
        public string? HealthIDs { get; set; }
        public string? EmployeeID { get; set; }
    }
    // End of added By Vyankat B. on the 16th sep 2026 for the Delayed Projects KPI drill-through

    // Commented and Added By Vyankat b. on 24th Sep 2026 for the Customer CSAT KPI drill-through
    public class AnalyticsDBCSATDrillDownRequest
    {
        public int DashboardID { get; set; }
        public int Level { get; set; } = 1;
        public int? QueryID { get; set; }

        public string? CurrentFromDate { get; set; }
        public string? CurrentToDate { get; set; }
        public string? PreviousFromDate { get; set; }
        public string? PreviousToDate { get; set; }

        public string? PortfolioIDs { get; set; }
        public string? CustomerIDs { get; set; }
        public string? ProjectManagerIDs { get; set; }
        public string? RegionIDs { get; set; }
        public string? BillingTypeIDs { get; set; }
        public string? HealthIDs { get; set; }
    }
    // End of Commented and Added By Vyankat b. on 24th Sep 2026 for the Customer CSAT KPI drill-through

    // Added by Vikas T on 16-09-2026 - Utilization / Bench / Active Projects KPI drill-through
    /// <summary>
    /// Request for GetAnalyticsDBKPIDrillDown.
    /// Flag → SP: Utilization | Bench | ActiveProjects (same SPs as KPI cards, DrillLevel 1–4).
    /// </summary>
    public class AnalyticsDBKPIDrillDownRequest
    {
        public int DashboardID { get; set; }

        /// <summary>Utilization | Bench | ActiveProjects</summary>
        public string? Flag { get; set; }

        /// <summary>1–4 (SP DrillLevel). Card uses 0 via GetAnalyticsDBKPICards.</summary>
        public int Level { get; set; } = 1;

        public string? CurrentFromDate { get; set; }
        public string? CurrentToDate { get; set; }
        public string? PreviousFromDate { get; set; }
        public string? PreviousToDate { get; set; }

        /// <summary>Utilization L2+ / Bench L2+ (ProjectGroupID; Bench allows 0 = Unallocated).</summary>
        public int? PortfolioID { get; set; }

        /// <summary>Utilization L3+ / Bench L3+ / Active L2+ (Bench 0 = Unallocated OK).</summary>
        public int? ProjectID { get; set; }

        /// <summary>Utilization L4 / Bench L4.</summary>
        public int? EmployeeID { get; set; }

        /// <summary>Added by Vikas T on 23-09-2026 - fallback for Bench L3+ when ProjectID is missing (old API).</summary>
        public int? RoleID { get; set; }

        /// <summary>Active L3+.</summary>
        public int? MilestoneID { get; set; }

        /// <summary>Active L4.</summary>
        public int? TaskID { get; set; }

        public string? PortfolioIDs { get; set; }
        public string? CustomerIDs { get; set; }
        public string? ProjectManagerIDs { get; set; }
        public string? RegionIDs { get; set; }
        public string? BillingTypeIDs { get; set; }
        public string? HealthIDs { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 500;
    }
    // End of AnalyticsDBKPIDrillDownRequest

    /// <summary>
    /// Same shape as Revenue drill — Level + flexible SP rows.
    /// </summary>
    public class AnalyticsDBKPIDrillDownModel
    {
        public string Flag { get; set; } = string.Empty;
        public int Level { get; set; }
        public string LevelLabel { get; set; } = string.Empty;
        public List<Dictionary<string, string?>> Rows { get; set; } = new();
    }
    // End of AnalyticsDBKPIDrillDownModel

    // Added by Vikas T on 17-09-2026 - Revenue vs Cost trend & forecast chart (SRS §4.1.2)
    /// <summary>
    /// Request for GetAnalyticsDBRevenueCostTrend.
    /// SP: usp_Whizible2_Sel_AnalyticsDB_RevenueCostTrend (inline Revenue/Cost logic — no nested EXEC).
    /// Same date + chip filters as KPI cards.
    /// </summary>
    public class AnalyticsDBRevenueCostTrendRequest
    {
        public int DashboardID { get; set; }
        public string? CurrentFromDate { get; set; }
        public string? CurrentToDate { get; set; }
        public string? PreviousFromDate { get; set; }
        public string? PreviousToDate { get; set; }
        public string? PortfolioIDs { get; set; }
        public string? CustomerIDs { get; set; }
        public string? ProjectManagerIDs { get; set; }
        public string? RegionIDs { get; set; }
        public string? BillingTypeIDs { get; set; }
        public string? HealthIDs { get; set; }
    }
    // End of AnalyticsDBRevenueCostTrendRequest

    /// <summary>
    /// Chart payload: Meta (result set 1) + Months (result set 2, CurrentFrom–CurrentTo).
    /// Updated by Vikas T on 23-09-2026 - month count follows the selected period, not a fixed 8-row spine.
    /// </summary>
    public class AnalyticsDBRevenueCostTrendModel
    {
        public Dictionary<string, string?> Meta { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<Dictionary<string, string?>> Months { get; set; } = new();
    }
    // End of AnalyticsDBRevenueCostTrendModel

    // Added by Aditya J. on 15-09-2026 - Portfolio Revenue Mix chart
    /// <summary>
    /// Request for GetAnalyticsDBPortfolioRevenueMix.
    /// Same filter bag as GetAnalyticsDBKPICards (Revenue KPI).
    /// SP: usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMix
    /// </summary>
    public class AnalyticsDBPortfolioRevenueMixRequest
    {
        public int DashboardID { get; set; }

        public string? CurrentFromDate { get; set; }
        public string? CurrentToDate { get; set; }
        public string? PreviousFromDate { get; set; }
        public string? PreviousToDate { get; set; }

        public string? PortfolioIDs { get; set; }
        public string? CustomerIDs { get; set; }
        public string? ProjectManagerIDs { get; set; }
        public string? RegionIDs { get; set; }
        public string? BillingTypeIDs { get; set; }
        public string? HealthIDs { get; set; }

        public int? RoleID { get; set; }
        public string? UserID { get; set; }
    }
    // End of AnalyticsDBPortfolioRevenueMixRequest

    /// <summary>
    /// One Portfolio / Project Group slice for the mix chart.
    /// PortfolioID = 0 and PortfolioName = Unmapped when ProjectGroupID is NULL.
    /// </summary>
    public class AnalyticsDBPortfolioRevenueMixItemModel
    {
        public int PortfolioID { get; set; }
        public string PortfolioName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public string? RevenueFormatted { get; set; }
        public string? BaseCurrencyCode { get; set; }
        /// <summary>Same money scale as the Revenue KPI (k / L / Cr / M). Blank when the mix total is 0.</summary>
        public string? Unit { get; set; }
        public string? CurrencyCode { get; set; }
        //Added by Aditya J. on 28-09-2026 Projects in this mix slice, including Unmapped
        public int ProjectCount { get; set; }
        //End of Added by Aditya J. on 28-09-2026 Projects in this mix slice, including Unmapped
    }
    // End of AnalyticsDBPortfolioRevenueMixItemModel

    // Added by Vikas T on 21-09-2026 - Revenue vs Cost chart drill-through
    /// <summary>
    /// Same filter/level shape as AnalyticsDBRevenueDrillDownRequest.
    /// SP: usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMixDrillDown
    /// </summary>
    public class AnalyticsDBPortfolioRevenueMixDrillDownRequest : AnalyticsDBRevenueDrillDownRequest
    {
    }
    // End of AnalyticsDBPortfolioRevenueMixDrillDownRequest

    //Added by Aditya J. on 21-09-2026 Cost & budget variance by project
    /// <summary>
    /// Request for GetAnalyticsDBCostBudgetVarianceByProject.
    /// Same chip filters as KPI cards; current period only (no previous dates).
    /// SP: usp_Whizible2_Sel_AnalyticsDB_CostBudgetVarianceByProject
    /// </summary>
    public class AnalyticsDBCostBudgetVarianceByProjectRequest
    {
        public int DashboardID { get; set; }

        public string? CurrentFromDate { get; set; }
        public string? CurrentToDate { get; set; }

        public string? PortfolioIDs { get; set; }
        public string? CustomerIDs { get; set; }
        public string? ProjectManagerIDs { get; set; }
        public string? RegionIDs { get; set; }
        public string? BillingTypeIDs { get; set; }
        public string? HealthIDs { get; set; }
    }
    //End of Added by Aditya J. on 21-09-2026 Cost & budget variance by project

    //Added by Aditya J. on 21-09-2026 Cost & budget variance by project
    /// <summary>
    /// One project bar for Cost &amp; Budget Variance.
    /// BudgetBurnPercentage is plotted; IsOverBudget flags bars beyond 100% burn.
    /// </summary>
    public class AnalyticsDBCostBudgetVarianceByProjectItemModel
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public decimal PlannedBudget { get; set; }
        public decimal ActualCost { get; set; }
        public decimal VarianceAmount { get; set; }
        public decimal BudgetBurnPercentage { get; set; }
        public decimal BudgetVariancePercentage { get; set; }
        public bool IsOverBudget { get; set; }
    }
    //End of Added by Aditya J. on 21-09-2026 Cost & budget variance by project

    #endregion
}

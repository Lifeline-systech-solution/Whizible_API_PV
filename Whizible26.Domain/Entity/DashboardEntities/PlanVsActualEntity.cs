using System;
using System.Collections.Generic;

namespace Whizible26.Domain.Entity.DashboardEntities
{
    #region Requests

    // Added by <Name> on 28-07-2026 - Shared filter payload for the dashboard endpoints
    /// <summary>
    /// Multi-select filters travel as comma-separated ID strings ("1,4,7").
    /// null or empty means "no filter / all", matching the page's behaviour where
    /// an empty selection Set is skipped by applyFilters().
    /// </summary>
    public class PlanVsActualFilterRequest
    {
        /// <summary>CSV of tbl_CNF_BusinessGroups.BusinessGroupID. null = all.</summary>
        public string? BusinessGroupIDs { get; set; }

        /// <summary>CSV of tbl_PM_Location.LocationID. null = all.</summary>
        public string? OrganizationUnitIDs { get; set; }

        /// <summary>CSV of tbl_PM_Project.ProjectID. null = all.</summary>
        public string? ProjectIDs { get; set; }

        /// <summary>CSV of fiscal-year start years. FY 2026-27 is 2026. null = all.</summary>
        public string? FinancialYears { get; set; }

        /// <summary>CSV of yyyyMM month keys, e.g. "202604,202605". null = all.</summary>
        public string? MonthKeys { get; set; }

        /// <summary>
        /// CSV of tbl_PM_DailyActivity.TimesheetStatusFlag values that count as
        /// actual hours. null = count every timesheet entry.
        /// </summary>
        public string? TimesheetStatusFlags { get; set; }

        /// <summary>Add OvertimeDuration to Duration. Default false.</summary>
        public bool IncludeOvertime { get; set; } = false;

        /// <summary>
        /// How EstimatedEfforts is spread across the project's months.
        /// 1 = equal monthly (default): EstimatedEfforts / TotalMonths.
        /// 2 = calendar-day pro-rata (partial first/last months).
        /// </summary>
        public byte PlanSpreadMethod { get; set; } = 1;
    }
    // End of PlanVsActualFilterRequest

    // Added by <Name> on 28-07-2026 - Request for the filter dropdowns
    public class PlanVsActualFilterMastersRequest
    {
        /// <summary>
        /// Reserved for row-level scoping. Currently unused -- if the dashboard must
        /// be restricted per user, read the LoginId claim from HttpContext.User on
        /// the server rather than trusting this value from the body.
        /// </summary>
        public int? LoginID { get; set; }

        public string? TimesheetStatusFlags { get; set; }
        public bool IncludeOvertime { get; set; } = false;
        public byte PlanSpreadMethod { get; set; } = 1;
    }
    // End of PlanVsActualFilterMastersRequest

    // Added by <Name> on 28-07-2026 - Request for the trend chart and heatmap
    public class PlanVsActualTrendRequest : PlanVsActualFilterRequest
    {
        /// <summary>1 = Business Group, 2 = Organization Unit, 3 = Project.</summary>
        public int LevelFlag { get; set; } = 1;

        /// <summary>
        /// Honoured only when LevelFlag = 3.
        /// null or 0 = all projects in scope; &gt; 0 = that ProjectID only.
        /// </summary>
        public int? TrendProjectID { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
    // End of PlanVsActualTrendRequest

    // Added by <Name> on 28-07-2026 - Request for the analysis table
    public class PlanVsActualAnalysisRequest : PlanVsActualFilterRequest
    {
        /// <summary>
        /// 1 = Project (default tab), 2 = Business Group, 3 = Organization Unit,
        /// 4 = Resource, 5 = resourceWise full detail (no UI tab today).
        /// </summary>
        public int LevelFlag { get; set; } = 1;

        /// <summary>
        /// PlannedHours | ActualHours | VarianceHours | VariancePercent | Label.
        /// Whitelisted inside the procedure; anything else falls back to PlannedHours.
        /// </summary>
        public string? SortColumn { get; set; } = "PlannedHours";

        /// <summary>ASC or DESC. Anything else falls back to DESC.</summary>
        public string? SortDirection { get; set; } = "DESC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
    // End of PlanVsActualAnalysisRequest

    // Added by Vishal.M on 12-08-2026 - Save / Get default filter for Plan_VS_Actual.aspx
    /// <summary>
    /// UI posts the five static dropdown selections as one JSON string in
    /// WhereClause (also mirrored in FilterJson).
    /// SP usp_InsUpd_Whizible2_PlanVsActual_DefaultFilter keys by SessionEmployeeID.
    /// </summary>
    public class PlanVsActualDefaultFilterRequest
    {
        /// <summary>Session("intLoginID"). Optional; not used by InsUpd SP.</summary>
        public int? LoginID { get; set; }
        public string? SessionEmployeeID { get; set; }
        public string? DashboardID { get; set; }
        public string? WhereClause { get; set; }
        //public string? FilterJson { get; set; }
    }
    // End of PlanVsActualDefaultFilterRequest

    // Added by Vishal.M on 12-08-2026 - Row returned by GetDefaultFilter SP
    /// <summary>
    /// Result set from usp_Whizible2_Sel_tbl_Whizible2_PlanVsActual_DefaultFilter.
    /// </summary>
    public class PlanVsActualDefaultFilterModel
    {
        public int? EmployeeID { get; set; }
        public int? DashboardID { get; set; }
        public string? WhereClause { get; set; }

    }
    // End of PlanVsActualDefaultFilterModel

    // Added by Vishal.M on 12-08-2026 - Result set from usp_InsUpd_Whizible2_PlanVsActual_DefaultFilter
    /// <summary>
    /// SP returns FilterID, Result ('1' = insert, '2' = update), and WhereClause.
    /// </summary>
    public class PlanVsActualDefaultFilterSaveResultModel
    {
        public int FilterID { get; set; }

        /// <summary>'1' = inserted, '2' = updated.</summary>
        public string Result { get; set; } = string.Empty;

        public string? WhereClause { get; set; }
    }
    // End of PlanVsActualDefaultFilterSaveResultModel

    #endregion

    #region Filter master responses

    // Added by <Name> on 28-07-2026 - GetFilterMasters result set 1
    public class BusinessGroupMasterModel
    {
        public int BusinessGroupID { get; set; }
        public string BusinessGroupName { get; set; } = string.Empty;
    }

    // Added by <Name> on 28-07-2026 - GetFilterMasters result set 2
    public class OrganizationUnitMasterModel
    {
        public int OrganizationUnitID { get; set; }
        public string OrganizationUnitName { get; set; } = string.Empty;
        public int? BusinessGroupID { get; set; }
    }

    // Added by <Name> on 28-07-2026 - GetFilterMasters result set 3
    public class ProjectMasterModel
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int? OrganizationUnitID { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? GlobalProject { get; set; }
    }

    // Added by <Name> on 28-07-2026 - GetFilterMasters result set 4
    public class FinancialYearMasterModel
    {
        public int FinancialYear { get; set; }
        public string FinancialYearLabel { get; set; } = string.Empty;
    }

    // Added by <Name> on 28-07-2026 - GetFilterMasters result set 5
    public class MonthMasterModel
    {
        public int MonthKey { get; set; }
        public string MonthLabel { get; set; } = string.Empty;
        public int FinancialYear { get; set; }
        public int FiscalQuarter { get; set; }
        public int QuarterKey { get; set; }
    }

    #endregion

    #region KPI response

    // Added by <Name> on 28-07-2026 - GetKPISummary result set (always exactly one row)
    public class PlanVsActualKpiModel
    {
        public string PlannedHours { get; set; } = string.Empty;
        public string ActualHours { get; set; } = string.Empty;
        public string VarianceHours { get; set; } = string.Empty;
        public decimal? VariancePercent { get; set; }
        public decimal? UtilizationPercent { get; set; }

        /// <summary>Fact rows in scope -- powers the "N data points" KPI sub-text.</summary>
        public long DataPointCount { get; set; }

        /// <summary>Distinct projects where actual &gt; planned * 1.1 (the mock's overCount).</summary>
        public int OverPlanProjectCount { get; set; }

        /// <summary>Returned so the UI, exports and SQL never disagree on the bands.</summary>
        public decimal? OnPlanThresholdPercent { get; set; }
        public decimal? UnderUtilizedThresholdPercent { get; set; }
        public decimal? OverUtilizedThresholdPercent { get; set; }
    }

    #endregion

    #region Summary-by-level responses

    // Added by <Name> on 28-07-2026 - GetSummaryByLevel result set 1 (#chartDept)
    public class PlanVsActualBusinessGroupSummaryModel
    {
        public int? BusinessGroupID { get; set; }
        public string BusinessGroupName { get; set; } = string.Empty;
        public string PlannedHours { get; set; } = string.Empty;
        public string ActualHours { get; set; } = string.Empty;
    }

    // Added by <Name> on 28-07-2026 - GetSummaryByLevel result set 2 (#chartDomain)
    public class PlanVsActualOrganizationUnitSummaryModel
    {
        public int? OrganizationUnitID { get; set; }
        public string OrganizationUnitName { get; set; } = string.Empty;
        public string BusinessGroupName { get; set; } = string.Empty;
        public string PlannedHours { get; set; } = string.Empty;
        public string ActualHours { get; set; } = string.Empty;
    }

    // Added by <Name> on 28-07-2026 - GetSummaryByLevel result set 3 (#chartProject)
    public class PlanVsActualProjectSummaryModel
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string OrganizationUnitName { get; set; } = string.Empty;
        public string BusinessGroupName { get; set; } = string.Empty;
        public string PlannedHours { get; set; } = string.Empty;
        public string ActualHours { get; set; } = string.Empty;
    }

    #endregion

    #region Trend response

    // Added by <Name> on 28-07-2026 - GetTrendData result set (line chart + heatmap)
    public class PlanVsActualTrendModel
    {
        public int? EntityID { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public int MonthKey { get; set; }

        /// <summary>Chart x-axis form, e.g. Apr '26.</summary>
        public string MonthLabel { get; set; } = string.Empty;

        public string PlannedHours { get; set; } = string.Empty;
        public string ActualHours { get; set; } = string.Empty;

        /// <summary>Precomputed server-side so heatmap and exports agree exactly.</summary>
        public decimal? VariancePercent { get; set; }
        public int TotalRecords { get; set; }
    }

    #endregion

    #region Analysis-table responses

    // Added by <Name> on 28-07-2026 - GetAnalysisTable result set 1
    /// <summary>
    /// Fixed column set across all five levels. Dimension columns that do not apply
    /// to the requested LevelFlag come back null and the client does not render them.
    /// </summary>
    public class PlanVsActualAnalysisRowModel
    {
        public string? BusinessGroupName { get; set; }
        public string? OrganizationUnitName { get; set; }
        public string? ProjectName { get; set; }
        public string? ResourceName { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }

        /// <summary>Concatenated dimension text, used for the Label sort.</summary>
        public string? Label { get; set; }

        /// <summary>HH:MM from SP (fn_Whizible2_ConvertDecimalToHourViceVersa flag 1).</summary>
        public string PlannedHours { get; set; } = string.Empty;
        public string ActualHours { get; set; } = string.Empty;
        public string VarianceHours { get; set; } = string.Empty;
        public decimal? VariancePercent { get; set; }
    }

    // Added by <Name> on 28-07-2026 - GetAnalysisTable result set 2 (footer totals)
    public class PlanVsActualAnalysisTotalModel
    {
        public long TotalRowCount { get; set; }
        /// <summary>HH:MM from SP (fn_Whizible2_ConvertDecimalToHourViceVersa flag 1).</summary>
        public string PlannedHours { get; set; } = string.Empty;
        public string ActualHours { get; set; } = string.Empty;
        public string VarianceHours { get; set; } = string.Empty;
        public decimal? VariancePercent { get; set; }
    }

    #endregion
}
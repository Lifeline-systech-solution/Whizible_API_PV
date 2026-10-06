using System;
using System.Collections.Generic;

namespace Whizible26.Domain.Entity.DashboardEntities
{
    #region Filter Masters & Project Lists
    public class PHSFilterMastersRequest { public int UserID { get; set; } public string LoginType { get; set; } = "E"; }
    public class PHSPracticeMasterModel { public int TypeId { get; set; } public string ProjectType { get; set; } = string.Empty; }
    public class PHSBusinessGroupMasterModel { public int BusinessGroupID { get; set; } public string BusinessGroup { get; set; } = string.Empty; }
    public class PHSProjectGroupMasterModel { public int ProjectGroupID { get; set; } public string ProjectGroupName { get; set; } = string.Empty; }
    public class PHSOrganizationUnitRequest { public int? BusinessGroupID { get; set; } public int UserID { get; set; } public string LoginType { get; set; } = "E"; }
    public class PHSOrganizationUnitMasterModel { public int LocationID { get; set; } public string Location { get; set; } = string.Empty; }
    public class PHSFilterProjectListRequest { public int? PracticeID { get; set; } public int? BusinessGroupID { get; set; } public int? OrganizationUnitID { get; set; } public int? ProjectGroupID { get; set; } public int UserID { get; set; } public string LoginType { get; set; } = "E"; }
    public class PHSTopProjectListRequest { public int UserID { get; set; } public string LoginType { get; set; } = "E"; }
    public class PHSProjectListModel { public int ProjectID { get; set; } public string ProjectName { get; set; } = string.Empty; public string? ExpectedStartDate { get; set; } public string? ExpectedEndDate { get; set; } }

    public class PHSReportingFrequencyModel { public string Frequency { get; set; } = string.Empty; public int FrequencyID { get; set; } }
    #endregion

    #region My Filters (Save/Apply)
    public class PHSFilterSaveRequest { public int TagID { get; set; } public int ProjectID { get; set; } public int EmployeeID { get; set; } public string FilterName { get; set; } = string.Empty; public string LoginType { get; set; } = "E"; public string WhereClause { get; set; } = string.Empty; public string CreatedBy { get; set; } = string.Empty; public int Flag { get; set; } = 0; public int? FilterID { get; set; } }
    public class PHSFilterListRequest { public int ProjectID { get; set; } public int TagID { get; set; } public string LoginType { get; set; } = "E"; public int EmployeeID { get; set; } }
    public class PHSFilterListModel { public int FilterId { get; set; } public string FilterName { get; set; } = string.Empty; public bool SetDefault { get; set; } public string QueryText { get; set; } = string.Empty; public int EmployeeID { get; set; } }
    public class PHSFilterByIdRequest { public int FilterID { get; set; } }
    public class PHSSetDefaultFilterRequest { public string ProjectID { get; set; } = string.Empty; public string LoginType { get; set; } = "E"; public string UserID { get; set; } = string.Empty; public string TagID { get; set; } = string.Empty; public string ChangeDefaultFilterID { get; set; } = string.Empty; public int Flag { get; set; } = 0; }
    public class PHSDefaultFilterCheckRequest { public int TagID { get; set; } public string LoginType { get; set; } = "E"; public int UserID { get; set; } }
    public class PHSDefaultFilterCheckModel { public int FilterID { get; set; } public int ProjectID { get; set; } public bool SetDefault { get; set; } }
    public class PHSFilterExistsRequest { public string FilterName { get; set; } = string.Empty; public int TagID { get; set; } public int ProjectID { get; set; } public int EmployeeID { get; set; } }
    public class PHSDefaultFilterDetailsRequest { public int ProjectID { get; set; } public int TagID { get; set; } public string LoginType { get; set; } = "E"; public int EmployeeID { get; set; } }
    public class PHSDefaultFilterDetailsModel { public int FilterID { get; set; } public string FilterName { get; set; } = string.Empty; public string QueryText { get; set; } = string.Empty; }
    #endregion

    #region Shared Scope & PHS Info
    public class PHSProjectScopeRequest
    {
        public int? ProgramID { get; set; }
        public int ProjectID { get; set; }
        public int? ProjectTypeID { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? OrganizationUnitID { get; set; }
        public string? ReportingEndDate { get; set; }
        public int? EmployeeID { get; set; }
        public string LoginType { get; set; } = "E";

        // ADDED FOR PAGINATION
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PHSInformationRequest { public int UserID { get; set; } public string LoginType { get; set; } = "E"; public int ProjectID { get; set; } public string? ReportingDate { get; set; } }
    public class PHSInformationModel
    {
        public string RequestId { get; set; } = string.Empty;
        public string RequestedOn { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string BusinessGroup { get; set; } = string.Empty;
        public string FromDate { get; set; } = string.Empty;
        public string OrganizationUnit { get; set; } = string.Empty;
        public string WorkhoursPercent { get; set; } = string.Empty;
    }
    #endregion

    #region KPI Summary
    public class PHSKpiSummaryRequest { public int? PracticeID { get; set; } public int? BusinessGroupID { get; set; } public int? OrganizationUnitID { get; set; } public int? ProjectGroupID { get; set; } public string? ReportingDate { get; set; } public int UserID { get; set; } public string LoginType { get; set; } = "E"; }
    public class PHSKpiSummaryModel
    {
        public int TotalProjects { get; set; }
        public int OnTrackProjects { get; set; }
        public string OnTrackProjectName { get; set; } = string.Empty;
        public int AtRiskProjects { get; set; }
        public string AtRiskNote { get; set; } = string.Empty;
        public int UnlockedProjects { get; set; }
        public string AsOfDate { get; set; } = string.Empty;
    }
    #endregion

    #region SQERT Tables & Thresholds
    public class PHSProjectPendingLockRequest : PHSProjectScopeRequest { public int Flag { get; set; } = 0; public string? ReportingStartDate { get; set; } }

    public class PHSProjectPendingLockModel
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ExpectedStartDate { get; set; } = string.Empty;
        public string ExpectedEndDate { get; set; } = string.Empty;
        public int TotalRecords { get; set; } // ADDED FOR PAGINATION
    }

    public class PHSSqertListRequest
    {
        public int? ProgramID { get; set; }
        public int? ProjectID { get; set; }
        public int? ProjectTypeID { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? OrganizationUnitID { get; set; }
        public string? ReportingDate { get; set; }
        public int Flag { get; set; } = 0; public int UserID { get; set; }
        public string LoginType { get; set; } = "E";
        public int PageNumber { get; set; } = 1; // ADDED FOR PAGINATION
        public int PageSize { get; set; } = 10;  // ADDED FOR PAGINATION
    }

    public class PHSSqertListModel
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ReportingDate { get; set; } = string.Empty;
        public double Scope { get; set; }
        public double Quality { get; set; }
        public double Effort { get; set; }
        public double Risk { get; set; }
        public double Time { get; set; }
        public string ExpectedStartDate { get; set; } = string.Empty;
        public string ExpectedEndDate { get; set; } = string.Empty;
        public int TotalRecords { get; set; }

        // NEW: Property to hold the calculated Overview color
        public string ProjectOverview { get; set; } = string.Empty;
    }

    public class PHSSqertRangeModel { public int RangeID { get; set; } public int LowerLow { get; set; } public int LowerHigh { get; set; } public int MiddleLow { get; set; } public int MiddleHigh { get; set; } public int UpperLow { get; set; } public int UpperHigh { get; set; } }

    public class PHSSqertSectionRequest { public int ProjectID { get; set; } public string? ReportingDate { get; set; } public int UserID { get; set; } public string LoginType { get; set; } = "E"; }
    public class PHSSqertSectionModel
    {
        public double Scope { get; set; }
        public double Quality { get; set; }
        public double Effort { get; set; }
        public double Risk { get; set; }
        public double Time { get; set; }
        public string ScopeDesc { get; set; } = string.Empty;
        public string QualityDesc { get; set; } = string.Empty;
        public string EffortDesc { get; set; } = string.Empty;
        public string RiskDesc { get; set; } = string.Empty;
        public string TimeDesc { get; set; } = string.Empty;
    }
    #endregion

    #region Tab details (Key Achievement, Issues, Milestone, Active Resources)
    public class PHSKeyAchievementModel
    {
        public int CompletedTasks { get; set; }
        public int TobeCompletedTasks { get; set; }
        public int SlippingTasks { get; set; }
        public int TotalPlannedTasks { get; set; }
        public int CompletedDeliverables { get; set; }
        public int SlippingDeliverables { get; set; }
        public int TobeCompletedDeliverables { get; set; }
        public int TotalPlannedDeliverables { get; set; }

        public int TotalRecords { get; set; } // ADDED FOR PAGINATION
    }
    public class PHSIssueDetailModel
    {
        public string Type { get; set; } = string.Empty;
        public int TotalIssues { get; set; }
        public int OpenIssues { get; set; }
        public int CloseIssues { get; set; }
        public int OthersIssues { get; set; }
        public int LessThanFive { get; set; }
        public int BetweenFiveAndTen { get; set; }
        public int MoreThanTen { get; set; }
        public int OverDueIssues { get; set; }
        public int ShownToCustomer { get; set; }

        public int TotalRecords { get; set; } // ADDED FOR PAGINATION
    }
    public class PHSMilestoneModel
    {
        public string ProjectName { get; set; } = string.Empty;
        public string Milestone { get; set; } = string.Empty;
        public bool IsReadyForBilling { get; set; }
        // CHANGE THIS FROM double TO double? (Nullable)
        public double? BillAmount { get; set; }
        public string PlannedStartDate { get; set; } = string.Empty;
        public string PlannedEndDate { get; set; } = string.Empty;
        public string ActualStartDate { get; set; } = string.Empty;
        public string ActualEndDate { get; set; } = string.Empty;
        public int Slippage { get; set; }
        public string MilestoneStatus { get; set; } = string.Empty;
        public bool IsSlipping { get; set; }
        public int TotalRecords { get; set; }
    }

    public class PHSActiveResourceRequest
    {
        public int ProjectID { get; set; }
        public int PageNumber { get; set; } = 1; // ADDED FOR PAGINATION
        public int PageSize { get; set; } = 10;  // ADDED FOR PAGINATION
    }

    public class PHSActiveResourceModel
    {
        public string Resource { get; set; } = string.Empty; public string StartDate { get; set; } = string.Empty; public string EndDate { get; set; } = string.Empty; public string Work { get; set; } = string.Empty; public string ActualWork { get; set; } = string.Empty;
        public int TotalRecords { get; set; } // ADDED FOR PAGINATION
    }
    #endregion

    #region Graph Requests & Models
    public class PHSTaskVsCompletionRequest { public int ProjectID { get; set; } public string? CurrentDate { get; set; } }
    public class PHSTaskVsCompletionModel { public string ActualPercentComplete { get; set; } = string.Empty; public int TotalTasks { get; set; } }
    public class PHSDelayInDaysRequest { public int ProjectID { get; set; } public string? CurrentDate { get; set; } }
    public class PHSDelayInDaysModel { public string DelayInterval { get; set; } = string.Empty; public int DelayCount { get; set; } }
    public class PHSMonthlyResourceCostRequest { public int ProjectID { get; set; } public string? CurrentDate { get; set; } }
    public class PHSMonthlyResourceCostModel { public string Month { get; set; } = string.Empty; public double ResourceCost { get; set; } }
    #endregion
}
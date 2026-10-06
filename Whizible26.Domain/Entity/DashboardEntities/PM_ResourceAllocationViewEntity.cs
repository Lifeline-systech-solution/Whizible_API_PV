using System;
using System.Collections.Generic;

namespace Whizible26.Domain.Entity.DashboardEntities
{
    // Added by Vyankat B. on 11-08-2026 for the Resource Allocation View
    // Added By Vyankat B. on 25th Aug 2026
    // Kept only request/response types used by ResourceAllocationView.aspx APIs.
    // End of Added By Vyankat B. on 25th Aug 2026

    #region Requests

    // usp_Whizible2_Sel_FromAndToDates_ForReasAllocation
    public class GetFromAndToDatesRequest
    {
        public string? FinancialPeriod { get; set; }
        public int Period { get; set; }
        public string? SpecificDate { get; set; }
    }

    // usp_Whizible2_Sel_ResourceAllocation_Dashboard
    public class GetResourceAllocationDashboardRequest
    {
        public string? UserID { get; set; }
        public string? LoginType { get; set; }
        public string? UserName { get; set; }
        public string? LoginId { get; set; }
        public string? FinancialPeriod { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? BusinessGroupID { get; set; }
        public string? LocationID { get; set; }
        public string? RoleId { get; set; }
        public string? GradeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? Deployable { get; set; }
        public string? AllocationPercentage { get; set; }
        public int? PageNo { get; set; }
        public int? PageSize { get; set; }
    }

    // usp_Whizible2_Sel_Res_tbl_PM_EmployeeName
    public class GetEmployeeNameRequest
    {
        public string? EmployeeID { get; set; }
    }

    // usp_Whizible2_Sel_Res_tbl_PM_ProjectEmployeeRole
    public class GetProjectEmployeeRoleRequest
    {
        public string? ProjectEmployeeRoleID { get; set; }
    }

    // usp_Whizible2_Sel_ResourceAllocation_Dashboard_ResourceDetails
    public class GetResourceAllocationDetailsRequest
    {
        public string? UserID { get; set; }
        public string? LoginType { get; set; }
        public string? UserName { get; set; }
        public string? LoginId { get; set; }
        public string? FinancialPeriod { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? EmployeeID { get; set; }
        public int? PageNo { get; set; }
        public int? PageSize { get; set; }
    }

    // usp_Whizible2_Upd_Res_tbl_PM_ProjectEmployeeRole
    public class UpdateProjectEmployeeRoleRequest
    {
        public string? ProjectEmployeeRoleID { get; set; }
        public string? EmployeeID { get; set; }
        public string? ProjectID { get; set; }
        public decimal Cost { get; set; }
        public decimal Rate { get; set; }
        public string? RoleID { get; set; }
        public decimal ResourcePercentage { get; set; }
        public string? ExpectedStartDate { get; set; }
        public string? ExpectedEndDate { get; set; }
        public string? ResourceStatus { get; set; }
        public bool IsResourceBillable { get; set; }
        public string? Responsibility { get; set; }
        public string? ReportingTo { get; set; }
        public bool IsDefaultApprover { get; set; }
        public string? ModifiedBy { get; set; }
    }

    // Added By Vyankat B. on 24th Aug 2026
    public class RowWiseExternalApproversRequest
    {
        public string? ProjectID { get; set; }
        public bool Paging { get; set; }
        public string? StrPaging { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }

    public class IsWorkflowApproverRequest
    {
        public string? UserID { get; set; }
        public string? ProjectID { get; set; }
    }

    public class ProjectEmployeeIdsRequest
    {
        public string? ProjectID { get; set; }
        public string? EmployeeID { get; set; }
    }

    // usp_Whizible2_Sel_ResourceAllocation_AuditTrail
    public class GetResourceAllocationAuditTrailRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? ProjectEmployeeRoleID { get; set; }
        public string? ModifiedBy { get; set; }
        public string? ModifiedFieldName { get; set; }
    }

    // Added By Vyankat B. on 24th Aug 2026
    public class GetResourceAllocationAuditLookupRequest
    {
        public string? ProjectEmployeeRoleID { get; set; }
    }

    // usp_Whizible2_Sel_GetAll_Dropdown
    public class GetAllDropdownRequest
    {
        public string? FieldName { get; set; }
        public GetAllDropdownInputParameter? InputParameterJson { get; set; }
    }

    public class GetAllDropdownInputParameter
    {
        public int? IntUniqueID { get; set; }
        public int? BlnIsAddNewMode { get; set; }
        public string? StrFromWhere { get; set; }
        public int? IntRoleID { get; set; }
        public int? IntProjectID { get; set; }
        public bool? BlnShowInActive { get; set; }
        public bool? BlnShowCurrent { get; set; }
        public int? IntApproverID { get; set; }
        public bool? BlnIsProjLevel { get; set; }
    }

    // usp_Whizible2_SEL_ResourceAllocation_Summary
    public class GetResourceAllocationSummaryRequest
    {
        public string? UserID { get; set; }
        public string? LoginType { get; set; }
        public string? UserName { get; set; }
        public string? LoginId { get; set; }
        public string? FinancialPeriod { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? BusinessGroupID { get; set; }
        public string? LocationID { get; set; }
        public string? RoleId { get; set; }
        public string? GradeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? Deployable { get; set; }
        public string? AllocationPercentage { get; set; }
        public string? AggregationMethod { get; set; }
    }

    #endregion

    #region Responses

    public class FinancialTypeResponse
    {
        public string? Code { get; set; }
        public string? FinancialType { get; set; }
        public int OrderBy { get; set; }
    }

    public class FromAndToDatesResponse
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int DistinctDays { get; set; }
    }

    public class ResourceAllocationDashboardResponse
    {
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? Title { get; set; }
        public DateTime? StartDate { get; set; }
        public double ResourcePercentage { get; set; }
    }

    public class ResourceAllocationPaginationEntity
    {
        public int TotalRecords { get; set; }
        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class EmployeeNameResponse
    {
        public string? EmployeeName { get; set; }
    }

    public class ResourceAllocationDetailsResponse
    {
        public int EmployeeID { get; set; }
        public int ProjectID { get; set; }
        public int ProjectEmployeeroleId { get; set; }
        public string? EmployeeName { get; set; }
        public string? ProjectName { get; set; }
        public DateTime? ExpectedStartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public string? Title { get; set; }
        public DateTime? StartDate { get; set; }
        public double ResourcePercentage { get; set; }
        public string? AllocationType { get; set; }
    }

    public class ProjectEmployeeRoleResponse
    {
        public DateTime? ActualEndDate { get; set; }
        public decimal ActualHours { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public decimal BilledHours { get; set; }
        public decimal BillingPercentage { get; set; }
        public string? BillingType { get; set; }
        public decimal BudgetedHours { get; set; }
        public decimal Cost { get; set; }
        public string? CostWithCurrency { get; set; }
        public int EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ExpectedStartDate { get; set; }
        public int IsApprover { get; set; }
        public bool? IsDefaultApprover { get; set; }
        public int IsExpenseApprover { get; set; }
        public bool? IsResourceActive { get; set; }
        public bool? IsResourceBillable { get; set; }
        public string? Location { get; set; }
        public decimal MonthlyFee { get; set; }
        public string? PlannedCostInProjectCurrency { get; set; }
        public string? PlannedCostInResourceCurrency { get; set; }
        public int ProjectEmployeeRoleId { get; set; }
        public int ProjectID { get; set; }
        public decimal Rate { get; set; }
        public int ReportingTo { get; set; }
        public string? ReportingToName { get; set; }
        public decimal ResourcePercentage { get; set; }
        public string? ResourceStatus { get; set; }
        public string? Responsibility { get; set; }
        public int Role { get; set; }
        public string? RoleDescription { get; set; }
        public int Used { get; set; }
        public string? UserName { get; set; }
    }

    public class RowWiseExternalApproversResponse
    {
        public int? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public string? ResourceName { get; set; }
        public string? RoleDescription { get; set; }
        public string? IsExternal { get; set; }
        public int? OrderNo { get; set; }
    }

    public class ResourceAllocationUpdateResponse
    {
        public int RowsAffected { get; set; }
    }

    public class ResourceAllocationAuditTrailResponse
    {
        public int AuditID { get; set; }
        public int ProjectEmployeeRoleID { get; set; }
        public string? ModifiedFieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class ResourceAllocationAuditTrailPagedResponse
    {
        public List<ResourceAllocationAuditTrailResponse> Records { get; set; }
            = new List<ResourceAllocationAuditTrailResponse>();
        public int TotalRecords { get; set; }
        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class ResourceAllocationAuditModifiedFieldResponse
    {
        public string? ModifiedFieldName { get; set; }
    }

    public class ResourceAllocationAuditModifiedByResponse
    {
        public string? ModifiedBy { get; set; }
    }

    public class GetAllDropdownResponse
    {
        public string? ID { get; set; }
        public string? FieldName { get; set; }
    }

    public class ResourceAllocationSummaryResponse
    {
        public int TotalEmployees { get; set; }
        public int FullyAllocated { get; set; }
        public decimal FullyAllocatedPct { get; set; }
        public int PartiallyAllocated { get; set; }
        public decimal PartiallyAllocatedPct { get; set; }
        public int Unallocated { get; set; }
        public decimal UnallocatedPct { get; set; }
    }

    #endregion
}

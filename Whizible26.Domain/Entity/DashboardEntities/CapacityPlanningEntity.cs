using System;

namespace Whizible26.Domain.Entity.DashboardEntities
{
    public class CapacityPlanningEntity
    {
        public int RoleID { get; set; }

        public string RoleDescription { get; set; }
    }

    public class RM_CapacityPlanning_Header
    {
        public string PeriodType { get; set; }

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        public int TotalStrength { get; set; }

        public string AvailableHours { get; set; }

        public string AllocatedHours { get; set; }

        public string BenchHours { get; set; }

        public decimal Allocated { get; set; }

        public decimal AllocatedPercentage { get; set; }

        public decimal Bench { get; set; }

        public decimal BenchPercentage { get; set; }

        public decimal ProjectRequests { get; set; }

        public decimal OpportunityRequests { get; set; }
    }

    public class CapacityPlanning_Params
    {
        public int UserID { get; set; }

        public string PeriodType { get; set; } = "MONTH";
    }

    public class CapacityPlanBGDto
    {
        public int BusinessGroupID { get; set; }

        public string BusinessGroup { get; set; }
    }

    public class CapacityPlanOUDto
    {
        public int LocationID { get; set; }

        public string Location { get; set; }
    }

    public class RM_CapacityPlanning_Skill
    {
        public int ToolID { get; set; }

        public string Description { get; set; }
    }

    public class RM_CapacityPlanning_Role
    {
        public int RoleID { get; set; }

        public string RoleDescription { get; set; }
    }

    public class Cp_Params
    {
        public string BGOUType { get; set; }

        public string BGOUFilter { get; set; }

        public string SkillList { get; set; }

        public string RoleList { get; set; }

        public DateTime CurrentDate { get; set; }

        public int SkillID { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int RoleID { get; set; }
    }

    public class CP_Details
    {
        public int? RoleID { get; set; }

        public int? SkillID { get; set; }

        public int? UserID { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int Days { get; set; }

        public bool RoleOrSkill { get; set; }

        public bool WeekOrMonth { get; set; }

        public string BGOUType { get; set; }

        public string BGOUFilter { get; set; }

        public string SkillList { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }

    public class CP_ProjectAllocation
    {
        public string ProjectName { get; set; }

        public string EmployeeName { get; set; }

        public string ProjectRole { get; set; }

        public double CostPerHour { get; set; }

        public double RatePerHour { get; set; }
    }

    public class CP_TotalStrengthDetails
    {
        public string ResourceName { get; set; }

        public decimal? Experience { get; set; }

        public string PrimarySkill { get; set; }

        public string Location { get; set; }

        public string EmployeeStatus { get; set; }
    }

    public class CP_ProjectRequest
    {
        public int RequestId { get; set; }

        public string Project { get; set; }

        public string BusinessGroup { get; set; }

        public DateTime RequiredFrom { get; set; }

        public int HeadCount { get; set; }

        public string Status { get; set; }
    }

    public class Cp_BenchByRoleOrSkill
    {
        public string Employee { get; set; }

        public decimal Experience { get; set; }

        public string PrimarySkill { get; set; }

        public DateTime? AvailableFrom { get; set; }

        public int Aging { get; set; }

        public string Location { get; set; }

        public string Status { get; set; }
    }

    public class CP_AnticipatedExit
    {
        public string Employee { get; set; }

        public decimal Experience { get; set; }

        public string PrimarySkill { get; set; }

        public string CurrentProject { get; set; }

        public DateTime? ExitDate { get; set; }

        public string Reason { get; set; }

        public string Location { get; set; }
    }

    public class CP_JoiningPool
    {
        public string Candidate { get; set; }

        public decimal Experience { get; set; }

        public string PrimarySkill { get; set; }

        public string OfferedRole { get; set; }

        public DateTime? JoiningDate { get; set; }

        public string Location { get; set; }
    }

    public class CP_OpportunityRequest
    {
        public string OpportunityName { get; set; }

        public string Client { get; set; }

        public string Stage { get; set; }

        public decimal? Probability { get; set; }

        public decimal? Headcount { get; set; }

        public DateTime? RequestStartDate { get; set; }
    }

    public class CP_RoleorSkilWiseTrendAnalysisRequest
    {
        public string BGOUType { get; set; }

        public string BGOUFilter { get; set; }

        public string SkillList { get; set; }

        public string ViewType { get; set; }
    }

    public class CP_ExportRequest
    {
        public string BGOUType { get; set; }

        public string BGOUFilter { get; set; }

        public string SkillList { get; set; }

        public string RoleList { get; set; }

        /// <summary>
        /// Selected Role/Skill display names (newline-separated).
        /// Month/Qtr Role report uses RoleDescription column (no RoleID).
        /// </summary>
        public string RoleNames { get; set; }

        public string ReportTab { get; set; }

        public string ReportFormat { get; set; }

        public DateTime? CurrentDate { get; set; }
    }

    public class CompanyLogoModel
    {
        public string? OriginalFileName { get; set; }

        public string? SystemFileName { get; set; }
    }
}

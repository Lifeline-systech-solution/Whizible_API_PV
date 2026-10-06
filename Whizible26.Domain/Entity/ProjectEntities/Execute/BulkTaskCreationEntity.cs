using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// code added by Vaibhav K on 31-10-25
namespace Whizible26.Domain.Entity.ProjectEntities.Execute
{
    //Added by Divya J on 30-06-2025 for QuickTask entities

    public class QuickTaskCurrentTeamMemberEntity
    {
        public int? EmployeeId { get; set; }
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public string? EmailID { get; set; }
        public string? RoleDescription { get; set; }
        public string? Responsibility { get; set; }
        public DateTime? ExpectedStartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
    }

    // Entity for mapping IB Priorities for QuickTask
    public class QuickTaskIBPriorityEntity
    {
        public string? Priority { get; set; }
    }

    // Entity for mapping Project Task Types for QuickTask
    public class QuickTaskProjectTaskTypeEntity
    {
        public int? TaskTypeID { get; set; }
        public string? TaskType { get; set; }
    }

    public class GetDefaultTaskTypeRequestbulk
    {
        public int? ProjectID { get; set; }
        public string? DeliverableType { get; set; }
    }
    public class Deliveriablemodelbulk
    {
        public int? TaskTypeID { get; set; }
        public int? ScheduleID { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Title { get; set; }
        public DateTime? LatestCompletionDate { get; set; } = null;
        public string? DeliverableLCE { get; set; }
        public string? LabelSchedule { get; set; }
    }
    public class DrawCustomFieldRequest
    {
        public int RowID { get; set; }
        public int? ProjectID { get; set; }
        public int? UserID { get; set; }
    }

    // Custom Field Data Model (returned from stored procedure)
    // Matches usp_Sel_tbl_PM_CustomFields_Master columns plus computed fields
    public class CustomFieldData
    {
        // Columns from usp_Sel_tbl_PM_CustomFields_Master (in same order)
        public int UniqueID { get; set; }
        public int ProjectID { get; set; }
        public string UserGivenCaption { get; set; } = "";
        public string DatabaseFieldName { get; set; } = "";
        public string? ValidationRules { get; set; }
        public bool Active { get; set; }
        public string DataType { get; set; } = "0";
        public int? ControlHeight { get; set; }
        public string? ControlWidth { get; set; }
        public int RowNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string? DefaultValue { get; set; }
        public int? MaxLength { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public bool IsCorporate { get; set; }
        public bool IsQueryValue { get; set; }
        public string? QueryText { get; set; }
        public string? DefaultType { get; set; }
        public string? EntityName { get; set; }
        public int IsCustomFieldAssigned { get; set; }

        // Additional computed fields for frontend
        public string FieldType { get; set; } = ""; // 'Text', 'TextArea', 'Combo', 'Date'
        public string? ComboOptions { get; set; } // JSON string for combo options
    }

    // Combo Option Model
    public class ComboOption
    {
        public string value { get; set; } = "";
        public string text { get; set; } = "";
    }

    // Based on QuickTask.aspx.vb - GetAccessToCustomField method (lines 151-218)
    public class GetAccessToCustomFieldRequest
    {
        public int? TaskTypeID { get; set; }
        public string? TaskType { get; set; } // TaskType name (will be converted to TaskTypeID if provided)
        public int? ProjectID { get; set; }
        public int? UserID { get; set; }
        public int? RoleID { get; set; }
    }

    public class TaskTypeModelbulk
    {
        public int? TaskTypeID { get; set; }
        public string? TaskType { get; set; }
        //public string? StartDate { get; set; }
        //public string? Title { get; set; }
        //public string? LatestCompletionDate { get; set; } = null;
        //public string? DeliverableLCE { get; set; }
    }

    // Entity for mapping Project Phases for SQA QuickTask
    public class QuickTaskProjectPhaseEntity
    {
        public int? ProjectPhaseID { get; set; }
        public string? Phase { get; set; }
        public int? NoOfReviews { get; set; }
        public int? FrequencyID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // Entity for mapping Milestones for QuickTask
    public class QuickTaskMilestoneEntity
    {
        public int? MilestoneID { get; set; }
        public string? Milestone { get; set; }
        public string? MilestoneDetails { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // Entity for mapping Modules for QuickTask
    public class QuickTaskModuleEntity
    {
        public int? ModuleID { get; set; }
        public string? ModuleName { get; set; }
        public string? ModuleDetails { get; set; } // Used for mode 'T' where ModuleID|ModuleName is returned
    }

    // Entity for mapping SubProjects for QuickTask
    public class QuickTaskSubProjectEntity
    {
        public int? SubProjectId { get; set; }
        public string? SubProjectName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // Entity for mapping Project Features for QuickTask
    public class QuickTaskProjectFeatureEntity
    {
        public int? ProjectFeatureID { get; set; }
        public string? FeatureName { get; set; }
    }

    // Entity for mapping Project Estimation Types for QuickTask
    public class QuickTaskProjectEstimationTypeEntity
    {
        public int? ProjectEstimationTypeID { get; set; }
        public string? EstimationTypeName { get; set; }
    }

    // Entity for mapping Change Requests for QuickTask
    public class QuickTaskChangeRequestEntity
    {
        public int? ChangeRequestID { get; set; }
        public string? ChangeRequestSummary { get; set; }
    }

    // Entity for mapping Deliverables for QuickTask
    public class QuickTaskDeliverableEntity
    {
        public int? DeliverableID { get; set; }
        public string? Deliverable { get; set; }
        public int? DeliverableTypeID { get; set; } // ScheduleTypeID from tbl_PM_OtherSchedules (ID)
        public int? ScheduleTypeID { get; set; } // ScheduleTypeID from tbl_PM_OtherSchedules (ID for join)
        public string? DeliverableTypeName { get; set; } // The type name from tbl_PM_CompanySchedules.LabelSchedule
        public string? DeliverableType { get; set; } // The type name from tbl_PM_CompanySchedules.LabelSchedule (via join)
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public string? ExpectedCompletionTime { get; set; } // Stored as nvarchar in table
        public DateTime? EndDate { get; set; }
        public int? TotalCount { get; set; } // Total count for paging
    }

    public class GetResourcesRequest
    {
        public int? ProjectID { get; set; }
    }

    public class GetTaskTypesRequest
    {
        public int? ProjectID { get; set; }
        public bool? GetDefault { get; set; }
    }

    public class GetPhasesRequest
    {
        public int? ProjectID { get; set; }
    }

    public class GetMilestonesRequest
    {
        public int? ProjectID { get; set; }
        public string? Mode { get; set; }
        public int? TaskID { get; set; }
    }

    public class GetModulesRequest
    {
        public int? ProjectID { get; set; }
        public string? Mode { get; set; }
        public int? TaskID { get; set; }
        public int? ModuleID { get; set; }
    }

    public class GetSubprojectsRequest
    {
        public int? ProjectID { get; set; }
        public string? Mode { get; set; }
        public int? TaskID { get; set; }
        public int? SubProjectID { get; set; }
    }

    public class GetFeaturesRequest
    {
        public int? ProjectID { get; set; }
        public int? ProjectFeatureID { get; set; }
    }

    public class GetEstimationTypesRequest
    {
        public int? ProjectID { get; set; }
        public int? ProjectEstimationTypeID { get; set; }
    }

    public class GetChangeRequestsRequest
    {
        public int? ProjectID { get; set; }
    }

    public class GetDeliverablesRequest
    {
        public int? ProjectID { get; set; }
        public string? q { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }

    public class GetDeliverableByIdRequest
    {
        public int DeliverableID { get; set; }
    }
    //End of Added by Divya J on 30-06-2025 for QuickTask entities

    // Added for Bulk Task Creation
    public class ResultFlag {
    
    public int ResultFlagID { get; set; } // 1 for success, 0 for failure
     }

  
    public class TaskValidationRequest
    {
        // For Step 1 & 3 & 4
        public int ProjectID { get; set; }
        public int? ProjectTypeID { get; set; }
        public int UserID { get; set; }

        // For Step 2 - INSERT
        public int? ProcessGroupID { get; set; }
        public string? TaskName { get; set; }
        public string? TaskNotes { get; set; }
        public int? DeliverableID { get; set; }
        public int? EmployeeID { get; set; }
        public string? Work { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int? ChangeRequestID { get; set; }
        public bool? Billable { get; set; }
        public int? ModuleID { get; set; }
        public int? SubProjectID { get; set; }
        public int? MilestoneID { get; set; }
        public string? Priority { get; set; }
        public string? TaskTypes { get; set; }
        public int? PhaseID { get; set; }
        public int IsHoliday { get; set; }
        public int IsLeave { get; set; }
        public int? UserStoryID { get; set; }
        public string? FeatureID { get; set; }
        public string? EstimationTypeID { get; set; }

        // For Step 3 - Validation
        public int IsPhaseMandatory { get; set; }
        public int IsModuleMandatory { get; set; }
        public int IsMilestoneMandatory { get; set; }
        public int IsSubProjectMandatory { get; set; }
        public int IsCRMandatory { get; set; }
        public int IsFeatureMandatory { get; set; }
        public int IsEstimationTypeMandatory { get; set; }

        // For Step 4
        public int? TMSTaskID { get; set; }
    }


    public class TaskValidationResultEntity
    {
        public int TaskReasonID { get; set; }
        public int TMSTaskID { get; set; }
        public string Task_Name { get; set; }
        public string Resource { get; set; }
        public int IsNotvalidate { get; set; }
        public string ValidationReason { get; set; }
        public int IsErrorWarning { get; set; }
        public string ValidationType { get; set; }
    }

    public class BulkTaskRequest
    {
        // For Step 1 & 3 & 4
        public int ProjectID { get; set; }
        public int? ProjectTypeID { get; set; }
        public int UserID { get; set; }

        // For Step 2 - INSERT
        public int? ProcessGroupID { get; set; }
        public string? TaskName { get; set; }
        public string? TaskNotes { get; set; }
        public int? DeliverableID { get; set; }
        public int? EmployeeID { get; set; }
        public string? Work { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int? ChangeRequestID { get; set; }
        public bool? Billable { get; set; }
        public int? ModuleID { get; set; }
        public int? SubProjectID { get; set; }
        public int? MilestoneID { get; set; }
        public string? Priority { get; set; }
        public string? TaskTypes { get; set; }
        public int? PhaseID { get; set; }
        public int IsHoliday { get; set; }
        public int IsLeave { get; set; }
        public int? UserStoryID { get; set; }
        public string? FeatureID { get; set; }
        public string? EstimationTypeID { get; set; }

        // For Step 3 - Validation
        public int IsPhaseMandatory { get; set; }
        public int IsModuleMandatory { get; set; }
        public int IsMilestoneMandatory { get; set; }
        public int IsSubProjectMandatory { get; set; }
        public int IsCRMandatory { get; set; }
        public int IsFeatureMandatory { get; set; }
        public int IsEstimationTypeMandatory { get; set; }

        // For Step 4
        public int? TMSTaskID { get; set; }

        // Additional properties from query parameters
        public int? TaskID { get; set; }
        public string? WhichTask { get; set; }
        public DateTime? BaselineStart { get; set; }
        public DateTime? BaselineEnd { get; set; }
        public double? BaselineWork { get; set; }
        public string? Phase { get; set; }
        public string? Module { get; set; }
        public string? SubProject { get; set; }
        public string? Milestone { get; set; }
        public int? OtherTaskID { get; set; }
        public int? ProjectFeatureID { get; set; }
        public int? ProjectEstimationTypeID { get; set; }
        public int? MitigationPlanID { get; set; }
        public int? TrainingResourceID { get; set; }
        public int? TrainingID { get; set; }
        public bool? Void { get; set; }
        public bool? OnHold { get; set; }

        public string? CreatedBy { get; set; }

        public bool? IsUserStoryTask { get; set; }
        public int? StoryPoints { get; set; }
        public string? DeliverableStageID { get; set; }

        
        // Added By Dipali V On 23rd Jan 2026 For W26 - Custom Fields support
        public Dictionary<string, object>? CustomFields { get; set; }
    }

    public class BulkTaskValidationRequest
    {
        // Common project-level fields
        public int ProjectID { get; set; }
        
        // List of tasks (each task has individual values)
        public List<BulkTaskRequest> Tasks { get; set; }
    }

    //for individual sp call
    public class BulkTaskResult
    {
        public int ParentTaskID { get; set; }
        public int SubTaskID { get; set; }
        public string? TaskName { get; set; }
        public string? Status { get; set; }
    }
    // End of Added for Bulk Task Creation

    // user story fileds
    public class UserStoryRequests
    {
      
        
        public int ProjectID { get; set; }

        public int? UserStoryID { get; set; }


        public string? UserStory { get; set; }
    }
    public class ScrumUserStoryDetailEntity
        {
        public int UserStoryID { get; set; }
        public string? UserStoryName { get; set; }
        public DateTime? ScrumUserStoryStartDate { get; set; }
        public DateTime? ScrumUserStoryEndDate { get; set; }
        public string? ReleaseName { get; set; }
        public DateTime? ScrumReleaseStartDate { get; set; }
        public DateTime? ScrumReleaseEndDate { get; set; }
        public string? Sprint { get; set; }
        public int? StoryPoints { get; set; }
        public double? InitialEstimate { get; set; }
    }

    // end of user story fileds

    public class ProjectTypeRequest
    {
        public int? ProjectTypeID { get; set; }
        public int? ProjectID { get; set; }
    }
    //Added for flags of visibility
    public class ProjectTypeEntity
    {
        public int TypeID { get; set; }
        public string? ProjectType { get; set; }

        public bool? ShowPhaseInAT { get; set; }
        public bool? PhaseMandatoryInAT { get; set; }

        public bool? ShowModuleInAT { get; set; }
        public bool? ModuleMandatoryInAT { get; set; }

        public bool? ShowSubProjectInAT { get; set; }
        public bool? SubProjectMandatoryInAT { get; set; }

        public bool? ShowMilestoneInAT { get; set; }
        public bool? MilestoneMandatoryInAT { get; set; }

        public bool? ShowChangeRequestInAT { get; set; }
        public bool? ChangeRequestMandatoryInAT { get; set; }

        public bool? ShowFeatureInAT { get; set; }
        public bool? FeatureMandatoryInAT { get; set; }

        public bool? ShowEstimationTypeInAT { get; set; }
        public bool? EstimationTypeMandatoryInAT { get; set; }

        public int? OUPoolID { get; set; }
        public int? ProjectTypeID { get; set; }

        public bool? IsProjectCreationWorkflowReqd { get; set; }
        public bool? IsActive { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool? IsProductExecutionProject { get; set; }
        public bool? IsAgileMethodFollowed { get; set; }
        public bool? Billable { get; set; }
        public decimal? EstimatedEfforts { get; set; }   // for DB & calculations
        public string? EstimatedEffortsDisplay { get; set; } // "02:30" for UI

        public int? MetricsTypeID { get; set; }
    }

}
// End of code added by Vaibhav K on 31-10-25

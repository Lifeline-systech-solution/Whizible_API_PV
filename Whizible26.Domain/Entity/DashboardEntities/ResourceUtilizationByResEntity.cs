
using System;
using System.Collections.Generic;

namespace Whizible26.Domain.Entity.DashboardEntities
{
    #region Requests

    // Added for Resource Utilization By Resource API
    /// <summary>
    /// Request payload for the Resource Utilization By Resource report (Graph / Details / Summary).
    /// Mirrors the parameters of usp_SEL_Whizible2_ResourceUtilizationByResource when @intMode = 0.
    /// </summary>
    public class ResourceUtilizationRequest
    {
        //Updated by Aditya J. on 07-09-2026 for Business Groups/Organization Unit/Delivery Unit/Resource
        //multi-select filters - BUID/OUID/DUID/EmployeeID are now CSV lists of ids (e.g. "1,3") instead of
        //a single int, since the front-end filters allow selecting more than one checkbox. int? rejected a
        //CSV value with a 400 "value '1,3' is not valid for BUID" validation error, so these are now string.
        /// <summary>CSV of tbl_PM_Employee.BusinessGroupID values, e.g. "1,3". null/blank = all business groups.</summary>
        public string? BUID { get; set; }

        /// <summary>CSV of tbl_PM_Employee.LocationID values. null/blank = all organization units.</summary>
        public string? OUID { get; set; }

        /// <summary>CSV of tbl_PM_Employee.ResourcePoolID values. null/blank = all delivery units.</summary>
        public string? DUID { get; set; }

        /// <summary>CSV of tbl_PM_Employee.EmployeeID values. null/blank = all resources (subject to access check).</summary>
        public string? EmployeeID { get; set; }
        //End of Updated by Aditya J. on 07-09-2026

        /// <summary>tbl_CRW_DateRanges.UniqueId reporting period. Defaults to 3 (matches legacy default).</summary>
        public int DateRange { get; set; } = 3;

        //Added by Aditya J. on 25-08-2026 for Resource Utilization Report
        /// <summary>usp_Sel_ResourceUtilization_Monthly @intDetails. 0 = graph/report, 1 = details, 2 = summary.</summary>
        public int Details { get; set; } = 0;
        //End of Added by Aditya J. on 25-08-2026 for Resource Utilization Report

        /// <summary>Logged-in user, used for the resource access/visibility check.</summary>
        public int UserID { get; set; }

        /// <summary>CSV of accessible ProjectIDs. null = all projects.</summary>
        public string? ProjectIDs { get; set; }

        /// <summary>'1' = only Deployable resources, '0' = all resources.</summary>
        public string? ShowDeployableOnly { get; set; } = "0";

        //Added by Aditya J. on 10-09-2026<PDF header display names from the selected BG/OU/DU/Resource/Period filters>
        /// <summary>Selected Business Group names for the PDF header, e.g. "Bench Hrs BG".</summary>
        public string? BusinessGroupName { get; set; }

        /// <summary>Selected Organization Unit names for the PDF header.</summary>
        public string? OrganizationUnitName { get; set; }

        /// <summary>Selected Delivery Unit names for the PDF header.</summary>
        public string? DeliveryUnitName { get; set; }

        /// <summary>Selected Resource names for the PDF header.</summary>
        public string? ResourceName { get; set; }

        /// <summary>Selected Period description for the PDF header, e.g. "This Month".</summary>
        public string? PeriodName { get; set; }
        //End of Added by Aditya J. on 10-09-2026<PDF header display names from the selected BG/OU/DU/Resource/Period filters>
    }
    // End of Added for Resource Utilization By Resource API

    // Added for Resource Utilization By Resource API
    /// <summary>
    /// Request payload for the Resource Utilization Date Range filter lookup.
    /// Mirrors the parameters of usp_SEL_Whizible2_ResourceUtilizationByResource when @intMode = 1.
    /// </summary>
    public class ResourceUtilizationDateRangeRequest
    {
        /// <summary>tbl_CRW_DateRanges.UniqueId. null = all date ranges.</summary>
        public int? DateRangeID { get; set; }

        /// <summary>1 = include the "Project Start Month to Current Month" option.</summary>
        public int? ShowProjectFromTo { get; set; }
    }
    // End of Added for Resource Utilization By Resource API

    #endregion

    #region Response

    // Added for Resource Utilization By Resource API
    /// <summary>
    /// Result-set row for the Resource Utilization graph (@intDetails = 0).
    /// </summary>
    public class ResourceUtilizationGraphModel
    {
        public string Month { get; set; } = string.Empty;
        public double? AvailableHrsPercent { get; set; }
        public double? PlannedHrsPercent { get; set; }
        public double? ActualHrsPercent { get; set; }
        public double? BillableHrsPercent { get; set; }
        public double? BenchHrsPercent { get; set; }
        public double? AvailableToBillableRatio { get; set; }
        public double? AvailableToPlannedRatio { get; set; }
        public double? AllocationToBillableRatio { get; set; }
    }

    /// <summary>
    /// Result-set row for the Resource Utilization Details tab (@intDetails = 1), one row per resource/month.
    /// </summary>
    public class ResourceUtilizationDetailModel
    {
        public string Month { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;
        public string? InstallCapacityHrs { get; set; }
        public string? CapacityHrs { get; set; }
        public double? CapacityPercent { get; set; }
        public string? AllocatedHrs { get; set; }
        public double? AllocatedPercent { get; set; }
        public string? ActualHrs { get; set; }
        public double? ActualPercent { get; set; }
        public string? BillableHrs { get; set; }
        public double? BillablePercent { get; set; }
        public string? SnapshotDate { get; set; }
        //commented and added by Aditya J. on 10-09-2026 for Removing Probable Bench Hrs % from Details; it is shown only on the graph
        //public double? ProbableBenchHrsPercent { get; set; }
        //End of commented and added by Aditya J. on 10-09-2026 for Removing Probable Bench Hrs % from Details; it is shown only on the graph
    }

    /// <summary>
    /// Result-set row for the Resource Utilization Summary tab (@intDetails = 2), one row per month.
    /// </summary>
    public class ResourceUtilizationSummaryModel
    {
        public string Month { get; set; } = string.Empty;
        public string? InstallCapacityHrs { get; set; }
        public string? CapacityHrs { get; set; }
        public double? CapacityPercent { get; set; }
        public string? AllocatedHrs { get; set; }
        public double? AllocatedPercent { get; set; }
        public string? ActualHrs { get; set; }
        public double? ActualPercent { get; set; }
        public string? BillableHrs { get; set; }
        public double? BillablePercent { get; set; }
        //commented and added by Aditya J. on 10-09-2026 for Removing Probable Bench Hrs % from Summary; it is shown only on the graph
        //public double? ProbableBenchHrsPercent { get; set; }
        //End of commented and added by Aditya J. on 10-09-2026 for Removing Probable Bench Hrs % from Summary; it is shown only on the graph
    }

    /// <summary>
    /// Result-set row for the Resource Utilization Date Range filter lookup (@intMode = 1).
    /// </summary>
    public class ResourceUtilizationDateRangeModel
    {
        public int UniqueId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    //Added by Aditya J. on 25-08-2026 for Resource Utilization Report
    /// <summary>
    /// Monthly row used by the Resource Utilization Summary (By Resource) PDF,
    /// sourced from usp_Sel_ResourceUtilization_Monthly.
    /// </summary>
    public class ResourceUtilizationMonthlyReportModel
    {
        public string Month { get; set; } = string.Empty;
        public double InstallCapacityHrs { get; set; } 
        public double AvailableHrs { get; set; }
        public double AvailablePercent { get; set; }
        public double PlannedHrs { get; set; } 
        public double PlannedPercent { get; set; }
        public double ActualHrs { get; set; } 
        public double ActualPercent { get; set; }
        public double BillableHrs { get; set; } 
        public double BillablePercent { get; set; }
    }
    //End of Added by Aditya J. on 25-08-2026 for Resource Utilization Report

    //Added by Aditya J. on 03-09-2026 for adding Business Groups multi-select filter
    /// <summary>
    /// Result-set row for the Resource Utilization Business Group filter dropdown,
    /// sourced from usp_Whizible2_Sel_GetBusinessGroups_DB.
    /// </summary>
    public class ResourceUtilizationBusinessGroupModel
    {
        public int BusinessGroupID { get; set; }
        public string BusinessGroup { get; set; } = string.Empty;
    }
    //End of Added by Aditya J. on 03-09-2026 for adding Business Groups multi-select filter

    //Added by Aditya J. on 03-09-2026 for adding Organization Unit multi-select filter
    /// <summary>
    /// Result-set row for the Resource Utilization Organization Unit filter dropdown,
    /// sourced from usp_Whizible2_Sel_GetOrganizationUnit_DB.
    /// </summary>
    public class ResourceUtilizationOrganizationUnitModel
    {
        public int LocationID { get; set; }
        public string Location { get; set; } = string.Empty;
    }
    //End of Added by Aditya J. on 03-09-2026 for adding Organization Unit multi-select filter

    //Added by Aditya J. on 03-09-2026 for adding Delivery Unit multi-select filter
    /// <summary>
    /// Result-set row for the Resource Utilization Delivery Unit filter dropdown,
    /// sourced from usp_Whizible2_Sel_GetDeliveryUnit_DB.
    /// </summary>
    public class ResourceUtilizationDeliveryUnitModel
    {
        public int ResourcePoolID { get; set; }
        public string ResourcePoolName { get; set; } = string.Empty;
    }
    //End of Added by Aditya J. on 03-09-2026 for adding Delivery Unit multi-select filter

    //Added by Aditya J. on 03-09-2026 for adding Resource multi-select filter
    /// <summary>
    /// Result-set row for the Resource Utilization Resource filter dropdown,
    /// sourced from usp_Whizible2_Sel_GetResources_DB.
    /// </summary>
    public class ResourceUtilizationResourceModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
    }
    //End of Added by Aditya J. on 03-09-2026 for adding Resource multi-select filter

    //Added by Aditya J. on 09-09-2026<Added request and response models for the generic cascading filter endpoint>
    /// <summary>
    /// Request payload for cascading Resource Utilization filter lookups
    /// (Business Groups / Organization Units / Delivery Units / Resources).
    /// BUID/OUID/DUID are CSV lists when the parent filter is multi-select.
    /// </summary>
    public class ResourceUtilizationDependentFilterRequest
    {
        /// <summary>BG / OU / DU / RES (also accepts longer aliases).</summary>
        public string? FilterType { get; set; }

        /// <summary>CSV of selected Business Group IDs. null/blank = no Business Group filter.</summary>
        public string? BUID { get; set; }

        /// <summary>CSV of selected Organization Unit (Location) IDs. null/blank = no OU filter.</summary>
        public string? OUID { get; set; }

        /// <summary>CSV of selected Delivery Unit (Resource Pool) IDs. null/blank = no DU filter.</summary>
        public string? DUID { get; set; }

        /// <summary>Logged-in user, used by the Resource list access check.</summary>
        public int UserID { get; set; }
    }

    /// <summary>
    /// Unified dropdown row from usp_Whizible2_Sel_GetResourceUtilizationDependentFilters_DB.
    /// ID 0 is the "Select ..." placeholder.
    /// </summary>
    public class ResourceUtilizationDependentFilterModel
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    //End of Added by Aditya J. on 09-09-2026<Added request and response models for the generic cascading filter endpoint>
    // End of Added for Resource Utilization By Resource API

    #endregion
}

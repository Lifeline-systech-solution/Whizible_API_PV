using System;
using System.Collections.Generic;

namespace Whizible26.Domain.Entity.ReportEntities.SkillsInventory
{

    // Added by Vyankat on 07-08-2026 - Shared filter payload for the report
  
    public class SkillsInventoryFilterRequest
    {
        public string? OrgUnitIDs { get; set; }
        public string? ToolIDs { get; set; }
        public int? PageNo { get; set; } 
        public int? PageSize { get; set; }

    }
    // End of SkillsInventoryFilterRequest

    // Added by Vyankat on 07-08-2026 - Request for the two filter cards
    public class SkillsInventoryFilterMastersRequest
    {
      
        public int? LoginID { get; set; }
        public int? OrgUnitParameterGroupID { get; set; }
        public bool IncludeInactiveEmployees { get; set; } = true;
        public bool OnlySkillsInUse { get; set; } = false;
    }
    // End of SkillsInventoryFilterMastersRequest

    // Added by Vyankat on 07-08-2026 - Request for the PDF / Excel exports
    public class SkillsInventoryExportRequest : SkillsInventoryFilterRequest
    {
        public string? CompanyName { get; set; }
        public bool IncludeResourceDetail { get; set; } = true;
    }
    // End of SkillsInventoryExportRequest


    // Added by Vyankat on 07-08-2026 - GetFilterMasters result set 1
    public class SkillsInventoryOrgUnitModel
    {
        public int OrgUnitID { get; set; }
        public string OrgUnitName { get; set; } = string.Empty;
        public int? ParameterGroupID { get; set; }
        public int EmployeeCount { get; set; }
    }

    // Added by Vyankat on 07-08-2026 - GetFilterMasters result set 2
    public class SkillsInventorySkillModel
    {
        public int ToolID { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
    }

    // Added by Vyankat on 07-08-2026 - GetFilterMasters result set 3
    public class SkillsInventoryBandModel
    {
        public int BandNumber { get; set; }
        public string BandLabel { get; set; } = string.Empty;
        public int? LowerMonths { get; set; }
        public int? UpperMonths { get; set; }
    }

    // Added by Vyankat on 07-08-2026 - GetSkillsInventory result set 1
   
    public class SkillsInventoryRowModel
    {
        public int ToolID { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public int OrgUnitID { get; set; }
        public string OrgUnitName { get; set; } = string.Empty;

        public int Band1Count { get; set; }   // < 1 Year
        public int Band2Count { get; set; }   // 1 - 2 Years
        public int Band3Count { get; set; }   // 3 - 5 Years
        public int Band4Count { get; set; }   // 5 - 7 Years
        public int Band5Count { get; set; }   // > 7 Years
        public int TotalCount { get; set; }
    }

    // Added by Vyankat on 07-08-2026 - GetSkillsInventory result set 2 (the "Subtotal" line)
    public class SkillsInventorySubtotalModel
    {
        public int ToolID { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public int OrgUnitCount { get; set; }

        public int Band1Count { get; set; }
        public int Band2Count { get; set; }
        public int Band3Count { get; set; }
        public int Band4Count { get; set; }
        public int Band5Count { get; set; }
        public int TotalCount { get; set; }
    }

    // Added by Vyankat on 07-08-2026 - GetSkillsInventory result set 3 (always one row)
    public class SkillsInventoryTotalModel
    {
        public int SkillCount { get; set; }
        public int OrgUnitCount { get; set; }

        public int Band1Count { get; set; }
        public int Band2Count { get; set; }
        public int Band3Count { get; set; }
        public int Band4Count { get; set; }
        public int Band5Count { get; set; }
        public int TotalCount { get; set; }
    }

    // Added by Vyankat on 07-08-2026 - The named resources behind the counts
    public class SkillsInventoryResourceModel
    {
        public int OrgUnitID { get; set; }
        public string OrgUnitName { get; set; } = string.Empty;
        public int ToolID { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int ExperienceMonths { get; set; }
        public decimal ExperienceYears { get; set; }
        public int ExperienceBand { get; set; }
        public string BandLabel { get; set; } = string.Empty;
        public int Proficiency { get; set; }
        public int HasCoreCompetency { get; set; }
        public decimal TrainingHours { get; set; }
    }


    public class SkillInventoryModel
    {
        public string? Location { get; set; }
        public string? Description { get; set; }

        public int? Below1YearExperience { get; set; }
        public int? OneToTwoYearsExperience { get; set; }
        public int? ThreeToFiveYearsExperience { get; set; }
        public int? FiveToSevenYearsExperience { get; set; }
        public int? Above7YearsExperience { get; set; }
    }


    public class PaginationModel
    {
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

}

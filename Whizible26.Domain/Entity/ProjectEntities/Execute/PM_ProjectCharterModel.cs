using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whizible26.Domain.Entity.Projects.Execute
{
    public class PM_ProjectCharterModel
    {
        public int ProjectID { get; set; }

        //public string ProjectName { get; set; }

        public string Objectives { get; set; }

        // Scope AS Background
        public string Background { get; set; }

        // ApplicationArchitecture AS StatementOfWork
        public string StatementOfWork { get; set; }

        public string Assumptions { get; set; }

        public string InitiatedBy { get; set; }

        public string? InitiationDate { get; set; }

        public string ApprovedBy { get; set; }

        public string? ApprovalDate { get; set; }

        public int PeakTeamSize { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }
    }

    public class GetProjectCharterDetailsRequest
    {
        public int? ProjectID { get; set; }
    }

     public class UpdateProjectCharterRequest
    {
        public int ProjectID { get; set; }

        public string Objectives { get; set; }

        // Maps to Scope
        public string Background { get; set; }

        // Maps to ApplicationArchitecture
        public string StatementOfWork { get; set; }

        public string Assumptions { get; set; }

        public string InitiatedBy { get; set; }

        public  string? InitiationDate { get; set; }

        public string ApprovedBy { get; set; }

        public string? ApprovalDate { get; set; }

        public int? PeakTeamSize { get; set; }

        //public string ModifiedBy { get; set; }
        //Added By sandhyarani M ON 17.02.2026 Modified by
        public int ModifiedBy { get; set; }
    }

    public class ProjectPinAuditTrailModel
    {
        public int PinHistoryID { get; set; }
        public int ProjectID { get; set; }
        public string? ModifiedField { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public string? Action { get; set; }
    }

    public class GetProjectPinAuditTrailRequest
    {
        public int? ProjectID { get; set; }
    }

    public class ProjectPinAuditTrailModifiedByModel
    {
        public string? ModifiedBy { get; set; }
    }
    public class GetProjectPinAuditTrailModifiedByRequest
    {
        public int? ProjectID { get; set; }
    }

    public class ProjectPinAuditTrailFieldNameModel
    {
        public string? FieldName { get; set; }
    }

    public class GetProjectPinAuditTrailFieldNameRequest
    {
        public int? ProjectID { get; set; }
    }

    public class GetEmailSubjectBodyRequest
    {
        public int MessageID { get; set; }
        public int? ProjectID { get; set; }
    }

    public class PM_EmailSubjectBodyModel
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class GetEmployeeEmailRequest
    {
        public int EmployeeID { get; set; }
    }

    public class PM_EmployeeEmailModel
    {
        //public string EmailID { get; set; }
        public string AdminEmail { get; set; } = string.Empty;    // EmployeeID 61
        public string EmployeeEmail { get; set; } = string.Empty; // Email of the requested EmployeeID
    }

    public class ProjectEmailRequest
    {
        public int ProjectID { get; set; }
        public string ToEmailID { get; set; } = string.Empty;
        public string CCEmailID { get; set; } = string.Empty;
        public string FromEmailID { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}

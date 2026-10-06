using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Added By Vyankat B On 18-11-2025 For PM Releases Entity and Request DTOs
namespace Whizible26.Domain.Entity.ProjectEntities.Execute
{
    // Added By Vyankat B On 18-11-2025 For Releases Entity class
    public class ReleasesEntity
    {
        public int ReleaseID { get; set; }
        public int ProjectID { get; set; }
        public string? Subject { get; set; }
        public string? ReleaseNote { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? MilestoneID { get; set; }
        public string? Objectives { get; set; }
        public string? Details { get; set; }
        public string? KnownProblems { get; set; }
        public string? Installation { get; set; }
        public string? Issues { get; set; }
        public string? TestingSummary { get; set; }
    }
    // End of Added By Vyankat B On 18-11-2025 For Releases Entity class

    // Added By Vyankat B On 18-11-2025 For Get Releases Request DTO
   
    public class GetReleasesRequest
    {
        public int ProjectID { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchText { get; set; }
    }
    // End of Added By Vyankat B On 18-11-2025 For Get Releases Request DTO

    // Added By Vyankat B On 18-11-2025 For Get Release By ID Request DTO
    public class GetReleaseByIDRequest
    {
        public int ReleaseID { get; set; }
    }
    // End of Added By Vyankat B On 18-11-2025 For Get Release By ID Request DTO

    // Added By Vyankat B On 18-11-2025 For Insert Release Request DTO
    public class InsertReleaseRequest
    {
        public int ProjectID { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public int MilestoneID { get; set; }
        public string Objectives { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string KnownProblems { get; set; } = string.Empty;
        public string Installation { get; set; } = string.Empty;
        public string Issues { get; set; } = string.Empty;
        public string TestingSummary { get; set; } = string.Empty;
    }
    // End of Added By Vyankat B On 18-11-2025 For Insert Release Request DTO

    // Added By Vyankat B On 18-11-2025 For Update Release Request DTO
    public class UpdateReleaseRequest
    {
        public int ReleaseID { get; set; }
        public int ProjectID { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public int MilestoneID { get; set; }
        public string Objectives { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string KnownProblems { get; set; } = string.Empty;
        public string Installation { get; set; } = string.Empty;
        public string Issues { get; set; } = string.Empty;
        public string TestingSummary { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
    }
    // End of Added By Vyankat B On 18-11-2025 For Update Release Request DTO

    // Added By Vyankat B On 18-11-2025 For Delete Release Request DTO
    public class DeleteReleaseRequest
    {
        public int ReleaseID { get; set; }
        public int ProjectID { get; set; }
    }
    // End of Added By Vyankat B On 18-11-2025 For Delete Release Request DTO

    // Added By Vyankat B On 18-11-2025 For Delete Multiple Releases Request DTO
    public class DeleteMultipleReleasesRequest
    {

        // Commenteda and Added By Vaibhav K On 03-02-2025 For Delete Multiple Releases issues
        //public string ReleaseIDs { get; set; } = string.Empty;
        public int? ProjectID { get; set; }            // Required if IsDeleteAll = true
        public bool IsDeleteAll { get; set; } = false; // 0 = Manual, 1 = Select All
        public string? SelectedReleaseIDs { get; set; } // CSV for Manual Selection
        public string? ExcludedReleaseIDs { get; set; } // CSV for Exclusions (when Select All is true)
        public string? SearchText { get; set; }        // Search filter to apply during deletion
        // End of Commenteda and Added By Vaibhav K  On 03-02-2025 For Delete Multiple Releases issues

    }
    // End of Added By Vyankat B On 18-11-2025 For Delete Multiple Releases Request DTO

    // Added By Vyankat B On 18-11-2025 For Get Projects Request DTO
    public class GetProjectsForReleases
    {
        public int UserId { get; set; }
        public string LoginType { get; set; } = "E";
    }
    // End of Added By Vyankat B On 18-11-2025 For Get Projects Request DTO

    // Added By Vyankat B On 20-11-2025 For Release Files Entity
    public class ReleaseFilesEntity
    {
        public int FileID { get; set; }
        public int ReleaseID { get; set; }
        public string? FileName { get; set; }
        public string? AttachedBy { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Description { get; set; }
        public string? SystemFileName { get; set; }
        public string? DirName { get; set; } 
        public long FileSize { get; set; }
    }
    // End of Added By Vyankat B On 20-11-2025 For Release Files Entity
   
    // Added By Vyankat B On 20-11-2025 For Get Release Files Request DTO
    public class GetReleaseFilesRequest
    {
        public int ReleaseID { get; set; }
        public int PageNumber { get; set; } = 1; 
        public int PageSize { get; set; } = 5; 
    }
    // End of Added By Vyankat B On 20-11-2025 For Get Release Files Request DTO

    // Added By Vyankat B On 20-11-2025 For Insert Release File Request DTO
    public class InsertReleaseFileRequest
    {
        public int ReleaseID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? SystemFilename { get; set; } 
        public string AttachedBy { get; set; } = string.Empty;
        public string LoginType { get; set; } = "E";
        public DateTime ReleaseDate { get; set; }
        public string? Description { get; set; } 
        public int ProjectID { get; set; }
    }
    // End of Added By Vyankat B On 20-11-2025 For Insert Release File Request DTO

    // Added By Vyankat B On 20-11-2025 For Update Release File Request DTO
    public class UpdateReleaseFileRequest
    {
        public int FileID { get; set; }
        public string Folder { get; set; } = string.Empty;
        public int ProjectID { get; set; }
        public string AttachmentFolderPath { get; set; } = string.Empty; 
    }
    // End of Added By Vyankat B On 20-11-2025 For Update Release File Request DTO

    // Added By Vyankat B On 20-11-2025 For Delete Release Files Request DTO
    public class DeleteReleaseFilesRequest
    {
        public string FileIDs { get; set; } = string.Empty; 
    }
    // End of Added By Vyankat B On 20-11-2025 For Delete Release Files Request DTO

    // Added By Vyankat B On 20-11-2025 For Download Release File Request DTO
    public class DownloadReleaseFileRequest
    {
        public int ReleaseID { get; set; }
        public int FileID { get; set; }
    }
    // End of Added By Vyankat B On 20-11-2025 For Download Release File Request DTO

    // Added By Vyankat B On 20-11-2025 For Get Project Code Request DTO
    public class GetProjectCodeRequest
    {
        public int ProjectID { get; set; }
    }
    // End of Added By Vyankat B On 20-11-2025 For Get Project Code Request DTO

    // Added By Vyankat B On 20-11-2025 For Generate DirName Request DTO
    public class GenerateDirNameRequest
    {
        public string ProjectCode { get; set; } = string.Empty;
        public DateTime? DateTime { get; set; }
    }
    // End of Added By Vyankat B On 20-11-2025 For Generate DirName Request DTO

    // Added By Vyankat B On 20-11-2025 For Save File To Disk Request DTO
    public class SaveFileToDiskRequest
    {
        public string DirName { get; set; } = string.Empty;
        public string SystemFilename { get; set; } = string.Empty;
        public string BasePath { get; set; } = "../../Releases/";
    }
    // End of Added By Vyankat B On 20-11-2025 For Save File To Disk Request DTO

    // Added By Vyankat B On 21-11-2025 For Get Employee and Project Info Request DTO
    public class GetEmployeeProjectInfoRequest
    {
        public int ProjectID { get; set; }
        public int EmployeeID { get; set; }
    }
    // End of Added By Vyankat B On 21-11-2025 For Get Employee and Project Info Request DTO

    // Added By Vyankat B On 21-11-2025 For Employee and Project Info Response DTO
    public class EmployeeProjectInfoResponse
    {
        public string? EmailID { get; set; }
        public string? ProjectName { get; set; }
        public string? UserName { get; set; }
    }
    // End of Added By Vyankat B On 21-11-2025 For Employee and Project Info Response DTO

    // Added By Vyankat B On 21-11-2025 For Get Email Message Request DTO
    public class GetEmailMessageRequest
    {
        public int MessageID { get; set; }
        public int? ProjectID { get; set; }
    }
    // End of Added By Vyankat B On 21-11-2025 For Get Email Message Request DTO

    // Added By Vyankat B On 21-11-2025 For Email Message Response DTO
    public class EmailMessageResponse
    {
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
    // End of Added By Vyankat B On 21-11-2025 For Email Message Response DTO

    // Added By Vyankat B On 21-11-2025 For Get Customer Email Request DTO
    public class GetCustomerEmailRequest
    {
        public int EmployeeID { get; set; }
        public int ProjectID { get; set; }

    }
    // End of Added By Vyankat B On 21-11-2025 For Get Customer Email Request DTO

    // Added By Vyankat B On 21-11-2025 For Customer Email Response DTO
    public class CustomerEmailResponse
    {
        public string? EmailID { get; set; }
        public string? ContactPerson { get; set; }

    }
    // End of Added By Vyankat B On 21-11-2025 For Customer Email Response DTO

    // Added By Vyankat B On 21-11-2025 For Send Email Request DTO
    public class SendEmailRequest
    {
        public int ReleaseID { get; set; }
        public string ToEmailID { get; set; } = string.Empty;
        public string CCEmailID { get; set; } = string.Empty;
        public string FromEmailID { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
    // End of Added By Vyankat B On 21-11-2025 For Send Email Request DTO

    // Added By Vyankat B On 21-11-2025 For Mail Request DTO (EmailService pattern)
    public class Mailrequest
    {
        public string? FromEmail { get; set; }
        public string? ToEmail { get; set; }
        public string? CcEmail { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public int? isHtml { get; set; }
    }
    // End of Added By Vyankat B On 21-11-2025 For Mail Request DTO (EmailService pattern)

    // Added By Vyankat B On 24-11-2025 For Get Release Feedback Request DTO
    public class GetReleaseFeedbackRequest
    {
        public int ReleaseID { get; set; }
    }
    // End of Added By Vyankat B On 24-11-2025 For Get Release Feedback Request DTO

    // Added By Vyankat B On 24-11-2025 For Release Feedback Response DTO
    public class ReleaseFeedbackResponse
    {
        public int? ReleaseID { get; set; }
        public string? Feedback { get; set; }
    }
    // End of Added By Vyankat B On 24-11-2025 For Release Feedback Response DTO

    // Added By Vyankat B On 25-11-2025 For Employee Info Entity (for repository pattern)
    public class EmployeeInfoEntity
    {
        public string? EmailID { get; set; }
        public string? UserName { get; set; }
    }
    // End of Added By Vyankat B On 25-11-2025 For Employee Info Entity

    // Added By Vyankat B On 25-11-2025 For Project Task Case Entity (for repository pattern)
    public class ProjectTaskCaseEntity
    {
        public string? ProjectName { get; set; }
    }
    // End of Added By Vyankat B On 25-11-2025 For Project Task Case Entity

    // Added By Vyankat B On 25-11-2025 For Project Code Entity (for repository pattern scalar operations)
    public class ProjectCodeEntity
    {
        public string? ProjectCode { get; set; }
    }
    // End of Added By Vyankat B On 25-11-2025 For Project Code Entity

    // Added By Vyankat B On 25-11-2025 For Email Message Entity (for repository pattern)
    public class EmailMessageEntity
    {
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
    // End of Added By Vyankat B On 25-11-2025 For Email Message Entity

    // Added By Vyankat B On 25-11-2025 For Releases PK Entity (for repository pattern scalar operations)
    public class ReleasesPKEntity
    {
        public int PK { get; set; }
    }
    // End of Added By Vyankat B On 25-11-2025 For Releases PK Entity

    // Added By Vyankat B On 25-11-2025 For DirName Entity (for repository pattern)
    public class DirNameEntity
    {
        public string? DirName { get; set; }
    }
    // End of Added By Vyankat B On 25-11-2025 For DirName Entity

    // Added By Vyankat B On 25-11-2025 For Release File By FileID Entity (for repository pattern)
    public class ReleaseFileByFileIDEntity
    {
        public string? FileName { get; set; }
        public string? SystemFilename { get; set; }
    }
    // End of Added By Vyankat B On 25-11-2025 For Release File By FileID Entity


    // Added By Vaibhav K On 04-02-26 for Milestone dropdown
    // Added for Milestone Dropdown
    public class MilestoneEntity
    {
        public int MilestoneID { get; set; }
        public string? Milestone { get; set; }
    }

    public class GetMilestonesDrpRequest
    {
        public int ProjectID { get; set; }
        public int SavedMilestoneID { get; set; } = 0; // Default 0 means only active
    }
    // End of Added By Vaibhav K On 04-02-26 for Milestone dropdown

}
// End of Added By Vyankat B On 18-11-2025 For PM Releases Entity and Request DTOs


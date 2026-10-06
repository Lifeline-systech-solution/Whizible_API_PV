using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Added By Vyankat B On 11-01-2025 For PM Lessons Learnt Entity and Request DTOs
namespace Whizible26.Domain.Entity.ProjectEntities.Execute
{
    // Added By Vyankat B On 11-01-2025 For Lessons Learnt Entity class
    public class LessonsLearntEntity
    {
        public int LessonId { get; set; }
        public int ProjectID { get; set; }
        public string? ProblemDescription { get; set; }
        public string? Solution { get; set; }
        public string? PreventiveAction { get; set; }
        public string? ReferedDocument { get; set; }
        public string? ProblemType { get; set; }
        public bool? PublishToKM { get; set; }
    }
    // End of Added By Vyankat B On 11-01-2025 For Lessons Learnt Entity class

    // Added By Vyankat B On 11-01-2025 For Get Lessons Learned Request 
    public class GetLessonsLearnedRequest
    {
        public int ProjectID { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchText { get; set; }
    }
    // End of Added By Vyankat B On 11-01-2025 For Get Lessons Learned Request DTO

    // Added By Vyankat B On 17-11-2025 For Pagination Info Entity
    public class PaginationInfoEntity
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public double TotalPages { get; set; }
    }
    // End of Added By Vyankat B On 17-11-2025 For Pagination Info Entity

    // Added By Vyankat B On 11-01-2025 For Get Lessons Learnt By ID Request DTO
    public class GetLessonsLearntByIDRequest
    {
        public int LessonId { get; set; }
    }
    // End of Added By Vyankat B On 11-01-2025 For Get Lessons Learnt By ID Request DTO

    // Added By Vyankat B On 11-01-2025 For Insert Lessons Learnt Request DTO
    public class InsertLessonsLearntRequest
    {
        public int ProjectID { get; set; }
        public string ProblemDescription { get; set; } = string.Empty;
        public string Solution { get; set; } = string.Empty;
        public string PreventiveAction { get; set; } = string.Empty;
        public string? ReferedDocument { get; set; }  
        public string ProblemType { get; set; } = string.Empty;  
        public bool PublishToKM { get; set; }  
        public string CreatedBy { get; set; } = string.Empty;
    }
    // End of Added By Vyankat B On 11-01-2025 For Insert Lessons Learnt Request DTO

    // Added By Vyankat B On 11-01-2025 For Update Lessons Learnt Request DTO
    public class UpdateLessonsLearntRequest
    {
        public int LessonId { get; set; }  // Changed from LessonsLearntID to LessonId
        public int ProjectID { get; set; }
        public string ProblemDescription { get; set; } = string.Empty;
        public string Solution { get; set; } = string.Empty;
        public string PreventiveAction { get; set; } = string.Empty;
        public string? ReferedDocument { get; set; }  
        public string ProblemType { get; set; } = string.Empty; 
        public bool PublishToKM { get; set; }  
        public string ModifiedBy { get; set; } = string.Empty;
    }
    // End of Added By Vyankat B On 11-01-2025 For Update Lessons Learnt Request DTO

   

    // Added By Vyankat B On 11-01-2025 For Delete Multiple Lessons Learnt Request DTO
    public class DeleteMultipleLessonsLearntRequest
    {

        //commented and added by Vaibhav K on 30-01-25
        //public string LessonIds { get; set; } = string.Empty;

        public int ProjectID { get; set; }
        public bool IsDeleteAll { get; set; }
        public string? SelectedLessonIds { get; set; } // CSV string
        public string? ExcludedLessonIds { get; set; } // CSV string
        public string? SearchText { get; set; }

        //End of commented and added by Vaibhav K on 30-01-25




    }
    // End of Added By Vyankat B On 11-01-2025 For Delete Multiple Lessons Learnt Request DTO

    // Added By Vyankat B On 11-01-2025 For Get Projects Request DTO
    public class GetProjects
    {
        public int UserId { get; set; }
        public string LoginType { get; set; } = "E";
    }
    // End of Added By Vyankat B On 11-01-2025 For Get Projects Request DTO

    // Added By Vyankat B On 11-01-2025 For Project Entity for Lessons Learnt
    public class ProjectEntity
    {
        public int? ProjectID { get; set; }
        public string? ProjectName { get; set; }
    }
    // End of Added By Vyankat B On 11-01-2025 For Project Entity for Lessons Learnt

    // Added By Vyankat B On 25-11-2025 For Lessons Learnt PK Entity (for repository pattern scalar operations)
    public class LessonsLearntPKEntity
    {
        public int PK { get; set; }
    }
    // End of Added By Vyankat B On 25-11-2025 For Lessons Learnt PK Entity
}
// End of Added By Vyankat B On 11-01-2025 For PM Lessons Learnt Entity and Request DTOs

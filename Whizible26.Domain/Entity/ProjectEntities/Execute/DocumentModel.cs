using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whizible26.Domain.Entity.ProjectEntities.Execute
{
    public class DocumentModel
    {
    }

    // Request Models - Added by Vyankat B. on 27-10-2025
    
    public class GetDocumentTypeRequest
    {
        public int? RoleID { get; set; }
        public int? ProjectID { get; set; }
    }

    public class GetSubDocTypeRequest
    {
        public int? CategoryID { get; set; }
        public int? SubCategoryID { get; set; }
        public int? ProjectID { get; set; }
    }

    public class GetAllDocumentsRequest
    {
        public int? ProjectID { get; set; }
        public int? DocumentID { get; set; }
        public string? WhatToSelect { get; set; }  // char(1) in SP
        public int? LoginRoleID { get; set; }
        public string? FilePaging { get; set; }  // VARCHAR(2) in SP
        public string? SearchText { get; set; }  // VARCHAR(4000) in SP
        public int? CategoryID { get; set; }
        public int? SubCategoryID { get; set; }
    }

    public class GetChangeReTypeRequest
    {
        public int? ProjectID { get; set; }
    }

    public class UploadDocumentRequest
    {
        public int? CategoryID { get; set; }
        public int? SubCategoryID { get; set; }
        public int? ProjectID { get; set; }
        public string? DirectoryName { get; set; }
        public string? UploadedFileName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? Description { get; set; }
        public decimal? FileSize { get; set; }
        public string? Extension { get; set; }
        public string? FileName { get; set; }
        public int? LogInID { get; set; }
        public string? LoginType { get; set; }
        public int? ChangeRID { get; set; }
        public int? TagID { get; set; }
        public int? UniqueID { get; set; }
        public string? CodeTemplate { get; set; }
    }

    // Response Models - Added by Vyankat B. on 27-10-2025
    
    public class DocumentTypeModel
    {
        public int? CategoryID { get; set; }
        public string? Category { get; set; }
    }

    public class SubDocumentTypeModel
    {
        public int? SubCategoryID { get; set; }
        public string? SubCategory { get; set; }
        public int? CategoryID { get; set; }
        public string? Description { get; set; }
    }

    public class ProjectDocumentModel
    {
        // Exact column names and types from SP: usp_Sel_tbl_PM_ProjectDocuments
        public int DocumentID { get; set; }                      // int NOT NULL
        public DateTime? UploadedDate { get; set; }              // datetime NULL
        public int ProjectID { get; set; }                       // int NOT NULL
        public string? DirectoryName { get; set; }               // nvarchar(200) NULL
        public string FileName { get; set; } = string.Empty;     // varchar(500) NOT NULL
        public DateTime? UpdatedDate { get; set; }               // datetime NULL
        public double? FileSize { get; set; }                    // float NULL
        public int? CategoryID { get; set; }                     // int NULL
        public string? Description { get; set; }                 // varchar(3000) NULL
        public DateTime? CreatedDate { get; set; }               // datetime NULL
        public bool Original { get; set; }                       // bit NOT NULL
        public int? DocumentRefID { get; set; }                  // int NULL
        public string? UploadedBy { get; set; }                  // varchar(50) NULL
        public string? ReviewedBy { get; set; }                  // varchar(50) NULL
        public DateTime? ReviewedDate { get; set; }              // datetime NULL
        public string? ReviewNotes { get; set; }                 // varchar(4000) NULL
        public bool IsURL { get; set; }                          // bit NOT NULL
        public int? ChangeRequestID { get; set; }                // int NULL
        public int? SubCategoryID { get; set; }                  // int NULL
        public int? TagID { get; set; }                          // int NULL
        public int? UniqueID { get; set; }                       // int NULL
        public string? DocumentCode { get; set; }                // varchar(200) NULL
        
        // Computed/Joined columns (not in base table)
        public string? Category { get; set; }                    // varchar - from join
        public string? CategoryDirectoryName { get; set; }       // varchar - from join
        public int? CheckBoxID { get; set; }                     // int - computed
        public string? SubCategory { get; set; }                 // varchar - from join
        public string? SubCategoryDirectoryName { get; set; }    // varchar - from join
        //Added by Vishal Mane on 15/01/2026 to get employee profile pic
        public string? ProfilePicURL { get; set; }              // varchar - from join
        //AEnd of Added by Vishal Mane on 15/01/2026 to get employee profile pic
    }

    public class ChangeRequestTypeModel
    {
        public int? ChangeRequestID { get; set; }
        public int? ProjectID { get; set; }
        public string? ChangeRequestType { get; set; }
        public string? ChangeRequestSummary { get; set; }  // Added to match stored procedure output
        public string? ChangeRequestDescription { get; set; }  // Added to match stored procedure output
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class UploadDocumentResponseModel
    {
        public string? strResult { get; set; }
        public string? strMessage { get; set; }
        public int? intID { get; set; }
    }

    // CheckIfDirectoryStructureExists Request/Response Models
    public class CheckDirectoryStructureRequest
    {
        public int? ProjectID { get; set; }
        public int? CategoryID { get; set; }
        public int? SubCategoryID { get; set; }
    }

    public class CheckDirectoryStructureResponse
    {
        public bool Success { get; set; }
        public string? DirectoryPath { get; set; }
        public string? DocumentCode { get; set; }
        public string? Message { get; set; }
    }

    public class ProjectDirectoryModel
    {
        public string? DirectoryName { get; set; }
    }

    public class CategoryDirectoryModel
    {
        public string? DirectoryName { get; set; }
    }

    public class SubCategoryDirectoryModel
    {
        public string? DirectoryName { get; set; }
    }

    public class ProjectCodeModel
    {
        public string? ProjectCode { get; set; }
    }

    public class DocumentCodeModel
    {
        public string? DocumentCode { get; set; }
    }
    // End of Added by Vyankat B. on 27-10-2025

    // Added by Vyankat B. on 28-10-2025 for SaveFileToServer
    public class SaveFileToServerRequest
    {
        public string DirectoryName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        //Added by Vishal Mane on 19/01/2026 to fix file temporing issue
        public string RequestedFileName { get; set; } = string.Empty;
        //End of Added by Vishal Mane on 19/01/2026 to fix file temporing issue
    }

    // Form model for file upload (combines file + parameters)
    public class SaveFileToServerFormModel
    {
        public IFormFile? File { get; set; }
        public string DirectoryName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        

    }

    public class SaveFileToServerResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? FilePath { get; set; }
    }
    // End of Added by Vyankat B. on 28-10-2025

    // Added by Vyankat B. on 28-10-2025 for AttachUrl
    public class AttachUrlRequest
    {
        public int? CategoryID { get; set; }
        public int? ProjectID { get; set; }
        public string? DirectoryName { get; set; }  // Used as strFileName in SP
        public string? UploadedFileName { get; set; }  // Used as strDescription in SP
        public int? LogInID { get; set; }
        public string? LoginType { get; set; }
        public int? SubCategoryID { get; set; }
        public int? TagID { get; set; }
    }

    public class AttachUrlResponse
    {
        public int? DocumentID { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }
    }
    // End of Added by Vyankat B. on 28-10-2025

    // Added by Vyankat B. on 28-10-2025 for DeleSelDoc
    public class DeleteDocumentRequest
    {
        public int? ProjectID { get; set; }
        public string? DocumentID { get; set; }  // Comma-separated document IDs
        public string? WhatToDelete { get; set; }
    }

    public class DeleteDocumentResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int DeletedCount { get; set; }
    }

    // Model for document info from usp_Sel_tbl_PM_ProjectDocuments (for deletion)
    public class DocumentInfoForDeletion
    {
        public int DocumentID { get; set; }
        public string? DirectoryName { get; set; }
        public string? FileName { get; set; }
        public bool IsURL { get; set; }
    }
    // End of Added by Vyankat B. on 28-10-2025

    // Added by Vyankat B. on 28-10-2025 for ShowHistory
    public class ShowHistoryRequest
    {
        public int? ProjectID { get; set; }
        public int? DocumentID { get; set; }
    }
    // End of Added by Vyankat B. on 28-10-2025

    // Added by Vyankat B. on 28-10-2025 for UpdShowReview
    public class UpdateReviewRequest
    {
        public int? DocumentID { get; set; }
        public int? LogInID { get; set; }       // Used as @intReviewedBy
        public string? Comment { get; set; }     // Used as @strReviewNotes
        public string? LoginType { get; set; }   // Used as @strReviewType
    }

    public class UpdateReviewResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? DocumentID { get; set; }
    }
    // End of Added by Vyankat B. on 28-10-2025

    // Added by Vyankat B. on 28-10-2025 for DownloadDocument
    public class DownloadDocumentRequest
    {
        public int? ProjectID { get; set; }
        public int? DocumentID { get; set; }
        public string? WhatToDelete { get; set; }
    }

    public class DownloadDocumentInfo
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public byte[]? FileBytes { get; set; }
        public bool IsURL { get; set; }
    }
    // End of Added by Vyankat B. on 28-10-2025

    // Added by Nischal C on 3/11/2025 for GetFileType (MIME validation)
    public class GetFileTypeFormModel
    {
        public IFormFile? File { get; set; }
    }

    public class GetFileTypeResponse
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public string? DetectedMimeType { get; set; }
    }
    // End of Added by Nischal C on 3/11/2025

    // Added by Nischal C on 3/11/2025 for GetEmployeeInformation (similar to NavigationController)
    public class GetEmployeeInformationRequest
    {
        public int? EmployeeID { get; set; }
    }

    public class EmployeeInformationModel
    {
        public int EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public string? Role { get; set; }
        public string? EmployeeImage { get; set; }
        public int RoleID { get; set; }
    }
    // End of Added by Nischal C on 3/11/2025

    public class GetReviewNotes
    {
        public int? DocumentID { get; set; }
    }
    // Added by Vishal Mane on 19/01/2026 to fix Review Noe and Mime Type issue
    public class ReviewNotesInformationModel
    {
        public string? ReviewNotes { get; set; }
       
    }
    // End of Added by Vishal Mane on 19/01/2026 to fix Review Noe and Mime Type issue

}

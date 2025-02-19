using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.UNOPSBusiness.Interfaces;

public interface IGoogleDriveDocumentManager
{
    /// <summary>
    /// Uploads a file to Google Drive.
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="fileName">Name of the file in Google Drive</param>
    /// <param name="parentFolderId">ID of the parent folder in Google Drive</param>
    /// <param name="mimeType">MIME type of the file (defaults to application/octet-stream)</param>
    /// <returns>The web view link of the uploaded file</returns>
    Task<string> UploadFileAsync(IFormFile file, string fileName, string parentFolderId, string mimeType = "application/octet-stream");

    /// <summary>
    /// Creates a folder in Google Drive. If a folder with the same name exists, returns its ID.
    /// </summary>
    /// <param name="folderName">Name of the folder to create</param>
    /// <param name="parentFolderId">ID of the parent folder</param>
    /// <returns>The ID of the created or existing folder</returns>
    Task<string> CreateFolderAsync(string folderName, string parentFolderId);

    /// <summary>
    /// Updates the permissions for a file in Google Drive.
    /// </summary>
    /// <param name="fileId">ID of the file</param>
    /// <param name="email">Email address of the user to grant permissions to</param>
    /// <param name="role">Role to assign (defaults to "reader")</param>
    Task UpdateFilePermissionsAsync(string fileId, string email, string role = "reader");

    /// <summary>
    /// Moves a file to a different folder in Google Drive.
    /// </summary>
    /// <param name="fileId">ID of the file to move</param>
    /// <param name="targetFolderId">ID of the destination folder</param>
    /// <returns>The new web view link of the moved file</returns>
    Task<string> MoveFileAsync(string fileId, string targetFolderId);

    /// <summary>
    /// Deletes a file from Google Drive.
    /// </summary>
    /// <param name="fileId">ID of the file to delete</param>
    Task DeleteFileAsync(string fileId);
    
    /// <summary>
    /// Moves a file to the Archive folder in Drive.
    /// </summary>
    /// <param name="fileId">ID of the file to be archived</param>
    Task ArchiveFileAsync(string fileId, string archiveFolderId);
}
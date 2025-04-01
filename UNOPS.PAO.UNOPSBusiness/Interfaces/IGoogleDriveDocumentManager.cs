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
    Task<Dictionary<string, string>> UploadFileAsync(IFormFile file, string fileName, string parentFolderId, string mimeType = "application/octet-stream");

    /// <summary>
    /// Copies a file in Google Drive.
    /// </summary>
    /// <param name="fileId">ID of the file to copy</param>
    /// <param name="fileName">Name of the copied file in Google Drive</param>
    /// <param name="parentFolderId">ID of the parent folder in Google Drive</param>
    /// <param name="userToImpersonate">Email of the user to impersonate</param>
    /// <param name="mimeType">MIME type of the file (defaults to application/octet-stream)</param>
    /// <returns>The web view link of the copied file</returns>
    Dictionary<string, string> CopyFile(string fileId, string fileName, string parentFolderId, string userToImpersonate, string mimeType = "application/octet-stream");

    /// <summary>
    /// Exports a file from Google Drive.
    /// </summary>
    /// <param name="fileId">ID of the file to export</param>
    /// <param name="fileName">Name of the exported file in Google Drive</param>
    /// <param name="parentFolderId">ID of the parent folder in Google Drive</param>
    /// <param name="mimeType">MIME type of the file</param>
    /// <param name="userToImpersonate">Email of the user to impersonate</param>
    /// <returns>The web view link of the exported file</returns>
    Task<Dictionary<string, string>> ExportFileAsync(string fileId, string fileName, string parentFolderId, string userToImpersonate, string mimeType);

    /// <summary>
    /// Creates a folder in Google Drive. If a folder with the same name exists, returns its ID.
    /// </summary>
    /// <param name="folderName">Name of the folder to create</param>
    /// <param name="parentFolderId">ID of the parent folder</param>
    /// <param name="userToImpersonate">Email of the user to impersonate</param>
    /// <returns>The ID of the created or existing folder</returns>
    Task<Dictionary<string, string>> CreateFolderAsync(string folderName, string parentFolderId);

    /// <summary>
    /// Updates the permissions for a file in Google Drive.
    /// </summary>
    /// <param name="fileId">ID of the file</param>
    /// <param name="email">Email address of the user to grant permissions to</param>
    /// <param name="userToImpersonate">Email of the user to impersonate</param>
    /// <param name="role">Role to assign (defaults to "reader")</param>
    Task UpdateFilePermissionsAsync(string fileId, string email, string userToImpersonate, string role = "reader");

    /// <summary>
    /// Moves a file to a different folder in Google Drive.
    /// </summary>
    /// <param name="fileId">ID of the file to move</param>
    /// <param name="targetFolderId">ID of the destination folder</param>
    /// <returns>The new web view link of the moved file</returns>
    Task<Dictionary<string, string>> MoveFileAsync(string fileId, string targetFolderId);

    /// <summary>
    /// Deletes a file from Google Drive.
    /// </summary>
    /// <param name="fileId">ID of the file to delete</param>
    /// <param name="userToImpersonate">Email of the user to impersonate</param>
    Task DeleteFileAsync(string fileId, string userToImpersonate);

    /// <summary>
    /// Moves a file to the Archive folder in Drive.
    /// </summary>
    /// <param name="fileId">ID of the file to be archived</param>
    /// <param name="archiveFolderId">ID of the archive folder</param>
    Task ArchiveFileAsync(string fileId, string archiveFolderId);

    /// <summary>
    /// Gets the file stream from Google Drive.
    /// </summary>
    /// <param name="fileId">ID of the file</param>
    /// <param name="userToImpersonate">Email of the user to impersonate</param>
    /// <returns>The file stream</returns>
    Task<MemoryStream> GetFileStream(string fileId, string userToImpersonate);
}

namespace UNOPS.PAO.GoogleServices;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Cloud.BigQuery.V2;
using GoogleServices.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using File = Google.Apis.Drive.v3.Data.File;



public class GoogleDriveAPIHelper
{
    private readonly IConfiguration configuration;
    private DriveService? driveService;
    private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    public GoogleDriveAPIHelper(IConfiguration configuration)
    {
        this.configuration = configuration;
        // TODO
        // ConfigureClient(); 
    }
    public BigQueryClient GetBigQueryClient()
    {
        var secretManager = new GoogleSecretManagerConfigurationProvider();

        var credentialParams = configuration.GetSection("BigQuerySettings")
            .Get<JsonCredentialParameters>();
        
        if (credentialParams?.ProjectId == null)
        {
            throw new InvalidOperationException("BigQuery ProjectId is not configured");
        }
        var bqSecret = secretManager.GetSecretVersion("BigQueryConnectionKey");
        if (bqSecret != null)
        {
            credentialParams.PrivateKey = bqSecret
                .Replace("\\n", "\n");
        }
        else
        {
            // In localhost the secret manager will be null and so the PrivateKey is defined in appsettings.json
        }

#pragma warning disable CS0618 // Type or member is obsolete - FromJsonParameters still works; CredentialFactory requires file path
        var credentials = GoogleCredential.FromJsonParameters(credentialParams);
#pragma warning restore CS0618

        var client = BigQueryClient.Create(credentialParams.ProjectId, credentials);

        return client;
    }

    private void ConfigureClient()
    {
        var credentials = GetCredentials();

        if (credentials.IsCreateScopedRequired)
        {
            string[] scopes = { DriveService.Scope.Drive, DriveService.Scope.Drive, DriveService.Scope.DriveMetadata };
            credentials = credentials.CreateScoped(scopes);
        }

        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credentials,
            ApplicationName = "VCEP ExternalEntity"
        });

        driveService = service;
    }

    /// <summary>
    /// Retrieves the Google credentials for accessing Google Drive.
    /// 
    /// Attempts to fetch the credentials from the Google Cloud Secret Manager first. 
    /// If the credentials are not found in the Secret Manager, it then attempts to retrieve them 
    /// from the application settings file (appsettings.json) under the section 'GoogleDriveSettings'.
    /// 
    /// The method returns a <see cref="GoogleCredential"/> object that can be used for authenticating 
    /// requests to Google Drive.
    /// 
    /// </summary>
    private GoogleCredential GetCredentials()
    {
        var credentialParams = configuration.GetSection("GoogleDriveSettings")
            .Get<JsonCredentialParameters>();
        if (credentialParams == null)
            throw new Exception("GoogleDriveSettings needs to be setup in appsettings.");

        var secretManager = new GoogleSecretManagerConfigurationProvider(credentialParams.ProjectId);
        var secretName = configuration?.GetSection("GoogleDriveSettings")["GoogleDriveConnectionKeySecretId"];
        if (secretName != null)
        {
            var googleDriveSecret = secretManager.GetSecretVersion(secretName, "latest");

            if (googleDriveSecret != null)
            {
                credentialParams.PrivateKey = googleDriveSecret?
                    .Replace("\\n", "\n");
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete - FromJsonParameters still works; CredentialFactory requires file path
        return GoogleCredential.FromJsonParameters(credentialParams);
#pragma warning restore CS0618
    }

    //file Upload to the Google Drive.
    public async Task<string> FileUpload(IFormFile file, string fileName, UploadFileType type, string? mimeType = null)
    {
        var driveFolderId = GetTargetFolderId(type);

        var fileMetaData = GetFileMetaData(fileName, driveFolderId, mimeType);

        await using var stream = file.OpenReadStream();
        return await UploadStream(stream, fileMetaData);
    }

    public async Task<string> FileUploadToSubFolderById(IFormFile file, string fileName, UploadFileType type, string subFolderId, string? mimeType = null)
    {
        var fileMetaData = GetFileMetaDataForDriveId(fileName, subFolderId, mimeType);

        await using var stream = file.OpenReadStream();
        return await UploadStream(stream, fileMetaData);
    }

    public async Task<Stream> GetFileStream(string fileId)
    {
        if (driveService == null)
        {
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
        }
        
        var request = driveService.Files.Get(fileId);
        request.SupportsAllDrives = true;
        var stream = new MemoryStream();
        await request.DownloadAsync(stream);
        return stream;
    }

    private File GetFileMetaData(string fileName, string driveFolderId, string? mimeType = null)
    {
        var googleSettings = configuration.GetSection("GoogleDriveSettings");
        var folderId = googleSettings.GetSection(driveFolderId).Value;
        var fileMetaData = new File
        {
            Name = fileName,
            MimeType = mimeType ?? "application/pdf"
        };
        fileMetaData.Parents = new[] { folderId };
        return fileMetaData;
    }

    private File GetFileMetaDataForDriveId(string fileName, string driveFolderId, string? mimeType = null)
    {
        var fileMetaData = new File
        {
            Name = fileName,
            MimeType = mimeType ?? "application/pdf"
        };
        fileMetaData.Parents = new[] { driveFolderId };
        return fileMetaData;
    }

    public async Task<string> FileUpload(Stream stream, string fileName, UploadFileType type, string? mimeType = null)
    {
        var driveFolderId = GetTargetFolderId(type);
        var fileMetaData = GetFileMetaData(fileName, driveFolderId, mimeType);
        return await UploadStream(stream, fileMetaData);
    }

    private async Task<string> UploadStream(Stream stream, File fileMetaData)
    {
        if (driveService == null)
        {
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
        }
        
        var request = driveService.Files.Create(fileMetaData, stream, fileMetaData.MimeType);
        request.SupportsTeamDrives = true;
        request.SupportsAllDrives = true;
        request.Fields = "webViewLink";

        var progress = await request.UploadAsync();
        if (progress.Status != UploadStatus.Completed)
        {
            throw progress.Exception;
        }

        return request.ResponseBody?.WebViewLink ?? throw new InvalidOperationException("Failed to get web view link from upload response");
    }

    private async Task<string?> CopyFile(string newFileName, string fileIdToCopy)
    {
        try
        {
            if (driveService == null)
            {
                throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
            }
            
            File copiedFile = new File();
            copiedFile.Name = newFileName;

            FilesResource.CopyRequest copyRequest = driveService.Files.Copy(copiedFile, fileIdToCopy);

            copyRequest.SupportsAllDrives = true;
            copyRequest.SupportsTeamDrives = true;
            var response = await copyRequest.ExecuteAsync();
            return response.Id ?? throw new InvalidOperationException("Failed to get ID from copy response");
        }
        catch (Exception)
        {
            return null;
        }
    }
    public async Task<string> GetFileUrlFromGoogleDriveFileId(string fileId)
    {
        if (driveService == null)
        {
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
        }
        
        var driveFileRequest = driveService.Files.Get(fileId);
        driveFileRequest.SupportsTeamDrives = true;
        driveFileRequest.SupportsAllDrives = true;
        driveFileRequest.Fields = "id, webViewLink, size";
        var driveFile = await driveFileRequest.ExecuteAsync();
        var url = driveFile.WebViewLink;
        return url ?? throw new InvalidOperationException($"Web view link not found for file ID: {fileId}");
    }

    public async Task<string> CloneFileToNewFolder(string newFileName, string fileIdToCopy, string targetFolderId)
    {
        var clonedFiledId = await CopyFile(newFileName, fileIdToCopy);
        
        if (string.IsNullOrEmpty(clonedFiledId))
        {
            throw new InvalidOperationException($"Failed to copy file {fileIdToCopy} with name {newFileName}");
        }

        await MoveFileToFolder(clonedFiledId, targetFolderId);
        return clonedFiledId;
    }
    public async Task MoveFileToFolder(string fileId, string targetFolderId)
    {
        if (driveService == null)
        {
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
        }
        
        var driveFileRequest = driveService.Files.Get(fileId);
        driveFileRequest.SupportsTeamDrives = true;
        driveFileRequest.Fields = "parents";
        var driveFile = await driveFileRequest.ExecuteAsync();

        if (driveFile.Parents != null)
        {
            var updateRequest = driveService.Files.Update(new File(), fileId);
            updateRequest.SupportsTeamDrives = true;
            updateRequest.AddParents = targetFolderId;
            updateRequest.RemoveParents = driveFile.Parents[0];
            await updateRequest.ExecuteAsync();
        }
    }

    private string GetTargetFolderId(UploadFileType type)
    {
        switch (type)
        {
            case UploadFileType.Invoice: return "uploadInvoiceTargetFolder";
            case UploadFileType.Claim: return "uploadClaimTargetFolder";
            case UploadFileType.Contract: return "uploadContractTargetFolder";
            default: throw new NotSupportedException();
        }
    }

    public async Task UpdatePermissionForAll(string[] emailsToUpdatePermissionTo, string fileId)
    {
        emailsToUpdatePermissionTo = emailsToUpdatePermissionTo.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
        foreach (var emailAddress in emailsToUpdatePermissionTo) await UpdatePermission(emailAddress, fileId);
    }

    public async Task UpdatePermission(string email, string FileId)
    {
        if (driveService == null)
        {
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
        }
        
        var permission = new Permission
        {
            Type = "user",
            EmailAddress = email,
            Role = "reader"
        };

        var permissionResource = driveService.Permissions.Create(permission, FileId);
        permissionResource.SupportsTeamDrives = true;
        permissionResource.SupportsAllDrives = true;
        permissionResource.SendNotificationEmail = false;

        await permissionResource.ExecuteAsync();
    }

    public async Task RemovePermission(string email, string fileId)
    {
        if (driveService == null)
        {
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
        }
        
        var listRequest = driveService.Permissions.List(fileId);
        listRequest.SupportsAllDrives = true;
        listRequest.Fields = "permissions(id,emailAddress, type)";
        var response = await listRequest.ExecuteAsync();

        foreach (var permission in response.Permissions)
        {
            if (permission.Type.ToLower() == "user" && permission.EmailAddress.ToLower() == email)
            {
                var deleteRequest = driveService.Permissions.Delete(fileId, permission.Id);
                deleteRequest.SupportsAllDrives = true;
                await deleteRequest.ExecuteAsync();
            }
        }
    }

    public async Task Delete(string path)
    {
        if (driveService == null)
        {
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
        }
        
        var id = GetFileIdFromLink(path);
        var request = driveService.Files.Delete(id);
        request.SupportsTeamDrives = true;
        request.SupportsAllDrives = true;
        await request.ExecuteAsync();
    }

    public string GenerateFileName(string reference, UploadFileType type, string fileExtension = ".pdf")
    {
        return $"{type}-{reference}{fileExtension}";
    }

    public string GetFileIdFromLink(string webViewLink)
    {
        var start = webViewLink.IndexOf("d/") + 2;
        // excel files contain edit and not view in the link like pdfs
        var end = webViewLink.IndexOf("/view") != -1 ? webViewLink.IndexOf("/view") : webViewLink.IndexOf("/edit");
        if (end < 0)
        {
            start = webViewLink.IndexOf("id=") + 3;
            end = webViewLink.Length;
        }

        return webViewLink.Substring(start, end - start);
    }

    public async Task DeleteFile(string path)
    {
        var googleDriveHelper = new GoogleDriveAPIHelper(configuration);
        await googleDriveHelper.Delete(path);
    }

    public async Task<string> UploadFileToDrive(IFormFile file, string fileName, UploadFileType type,
        params string[]? emailsToShareFileWith)
    {
        var uploadedFileUrl = await FileUpload(file, fileName, type);

        var fileId = GetFileIdFromLink(uploadedFileUrl);
        if (emailsToShareFileWith != null)
            await UpdatePermissionForAll(emailsToShareFileWith, fileId);

        return uploadedFileUrl;
    }

    public async Task<string> UploadFileToDrive(IFormFile file, string fileName, string subFolderId, UploadFileType type,
    string mimeType, params string[]? emailsToShareFileWith)
    {
        var uploadedFileUrl = await FileUploadToSubFolderById(file, fileName, type, subFolderId, mimeType);

        var fileId = GetFileIdFromLink(uploadedFileUrl);
        if (emailsToShareFileWith != null)
            await UpdatePermissionForAll(emailsToShareFileWith, fileId);

        return uploadedFileUrl;
    }

    public async Task<IList<File>> ReadFiles(string[] sourceFolderIds)
    {
        if (driveService == null)
        {
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
        }
        
        var parentsQuery = "";
        for (var i = 0; i < sourceFolderIds.Length; i++)
        {
            var folder = sourceFolderIds[i];
            parentsQuery += $"'{folder}' in parents";
            if (i < sourceFolderIds.Length - 1) parentsQuery += " or ";
        }

        // Define parameters of request.
        var listRequest = driveService.Files.List();
        listRequest.SupportsTeamDrives = true;
        listRequest.IncludeTeamDriveItems = true;
        listRequest.Q = "mimeType != 'application/vnd.google-apps.folder' and (" + parentsQuery + ")";
        var fileList = await listRequest.ExecuteAsync();
        return fileList.Files;
    }

    public async Task ReadFilesContent(IList<File> files, List<object> list, Func<MemoryStream, File, object> callback)
    {
        if (driveService == null)
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient first.");

        if (files.Count > 0)
            foreach (var file in files)
            {
                var request = driveService.Files.Get(file.Id ?? "");
                request.SupportsTeamDrives = true;

                // Add a handler which will be notified on progress changes.
                // It will notify on each chunk download and when the
                // download is completed or failed.
                var stream = new MemoryStream();

                request.MediaDownloader.ProgressChanged +=
                    progress =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Completed:
                                {
                                    list.Add(callback(stream, file));

                                    break;
                                }
                            case DownloadStatus.Failed:
                                {
                                    break;
                                }
                        }
                    };
                await request.DownloadAsync(stream);
            }
    }

    public async Task MoveToFolder(File file, string targetFolderId)
    {
        if (driveService == null)
        {
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
        }

        var driveFileRequest = driveService.Files.Get(file.Id);
        driveFileRequest.SupportsTeamDrives = true;
        driveFileRequest.Fields = "parents";
        var driveFile = await driveFileRequest.ExecuteAsync();

        if (driveFile.Parents != null)
        {
            var updateRequest = driveService.Files.Update(new File(), file.Id);
            updateRequest.SupportsTeamDrives = true;
            updateRequest.AddParents = targetFolderId;
            updateRequest.RemoveParents = driveFile.Parents[0];
            await updateRequest.ExecuteAsync();
        }
    }

    public async Task<string> CreateFolderIfNotExists(string folderName, string parentFolderId)
    {
        if (driveService == null)
        {
            throw new InvalidOperationException("Drive service is not configured. Call ConfigureClient() first.");
        }

        await semaphore.WaitAsync();
        try
        {
            // Check if the folder exists
            var listRequest = driveService.Files.List();
            listRequest.Q = $"parents in '{parentFolderId}' " +
                "and mimeType = 'application/vnd.google-apps.folder' " +
                $"and name = '{folderName}' and trashed = false";
            listRequest.SupportsAllDrives = true;
            listRequest.IncludeItemsFromAllDrives = true;
            listRequest.Fields = "files(id, name)";

            var files = await listRequest.ExecuteAsync();

            if (files.Files.Any())
            {
                // Folder already exists
                return files.Files.First().Id;
            }

            // Create the folder
            var fileMetadata = new File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { parentFolderId }
            };

            var createRequest = driveService.Files.Create(fileMetadata);
            createRequest.SupportsAllDrives = true;
            createRequest.Fields = "id";

            var file = await createRequest.ExecuteAsync();

            return file.Id;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<string> FindOrCreateFolder(string folderName, string type, string? parentFolderId = null)
    {
        if (string.IsNullOrEmpty(parentFolderId))
        {
            parentFolderId = this.configuration.GetValue<string>($"GoogleDriveSettings:DefaultGoogleDriveFolderIds:{type}");
        }

        if (string.IsNullOrEmpty(parentFolderId))
        {
            throw new InvalidOperationException($"Parent folder ID is not configured for type: {type}");
        }

        return await CreateFolderIfNotExists(folderName, parentFolderId);

    }
}

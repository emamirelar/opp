using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using File = Google.Apis.Drive.v3.Data.File;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class GoogleDriveDocumentManager : IGoogleDriveDocumentManager
{
    private readonly IConfiguration _configuration;
    private readonly DriveService _driveService;
    private static readonly int MaxRetries = 5;
    private static readonly int InitialRetryDelayMs = 1000;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public GoogleDriveDocumentManager(IConfiguration configuration)
    {
        _configuration = configuration;
        var credentials = GetCredentials();
        //_driveService = InitializeDriveService();
    }

    private DriveService InitializeDriveService()
    {
        var credentials = GetCredentials();

        if (credentials.IsCreateScopedRequired)
        {
            string[] scopes =
                { DriveService.Scope.Drive, DriveService.Scope.DriveReadonly, DriveService.Scope.DriveMetadata };
            credentials = credentials.CreateScoped(scopes);
        }

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credentials,
            ApplicationName = "Grant+ Document Management"
        });
    }

    private GoogleCredential GetCredentials()
    {
        var credentialParams = _configuration.GetSection("GoogleDriveSettings")
            .Get<JsonCredentialParameters>();
        if (credentialParams == null)
            throw new Exception("GoogleDriveSettings configuration is missing.");

        
        var secretName = _configuration.GetValue<string>("GoogleDriveSettings:GoogleDriveConnectionKeySecretId");
        var secretManagerProvider = new GoogleSecretManagerConfigurationProvider(credentialParams.ProjectId, secretName);
        var secretValue = secretManagerProvider.GetSecretVersion(secretName, "latest");

        return GoogleCredential.FromJson(secretValue);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName, string parentFolderId,
        string mimeType = "application/octet-stream")
    {
        using var stream = file.OpenReadStream();
        var fileMetadata = new File
        {
            Name = fileName,
            Parents = new[] { parentFolderId },
            MimeType = mimeType
        };

        var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
        request.Fields = "webViewLink";
        request.SupportsAllDrives = true;

        var result = await RetryAsync(async () => await request.UploadAsync());
        if (result.Status != UploadStatus.Completed)
            throw new Exception("File upload failed.", result.Exception);

        return request.ResponseBody.WebViewLink;
    }

    public async Task<string> CreateFolderAsync(string folderName, string parentFolderId)
    {
        await _semaphore.WaitAsync();
        try
        {
            var folderId = await FindFolderIdAsync(folderName, parentFolderId);
            if (!string.IsNullOrEmpty(folderId))
                return folderId;

            var fileMetadata = new File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new[] { parentFolderId }
            };

            var request = _driveService.Files.Create(fileMetadata);
            request.Fields = "id";
            request.SupportsAllDrives = true;

            var createdFolder = await request.ExecuteAsync();
            return createdFolder.Id;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<string> FindFolderIdAsync(string folderName, string parentFolderId)
    {
        var query = $"name = '{folderName}' and mimeType = 'application/vnd.google-apps.folder' " +
                    $"and '{parentFolderId}' in parents and trashed = false";

        var request = _driveService.Files.List();
        request.Q = query;
        request.Fields = "files(id)";
        request.SupportsAllDrives = true;
        request.IncludeItemsFromAllDrives = true;
        request.IncludeTeamDriveItems = true;

        var result = await request.ExecuteAsync();
        return result.Files.FirstOrDefault()?.Id;
    }

    public async Task UpdateFilePermissionsAsync(string fileId, string email, string role = "reader")
    {
        var permission = new Permission
        {
            Type = "user",
            Role = role,
            EmailAddress = email
        };

        var request = _driveService.Permissions.Create(permission, fileId);
        request.SendNotificationEmail = false;
        request.SupportsAllDrives = true;

        await RetryAsync(async () => await request.ExecuteAsync());
    }

    public async Task<string> MoveFileAsync(string fileId, string targetFolderId)
    {
        var file = await GetFileAsync(fileId);
        if (file.Parents == null || file.Parents.Count == 0)
            throw new Exception("The file does not have a parent folder.");

        var request = _driveService.Files.Update(new File(), fileId);
        request.AddParents = targetFolderId;
        request.RemoveParents = file.Parents.First();
        request.Fields = "id, webViewLink";
        request.SupportsAllDrives = true;

        var result = await request.ExecuteAsync();
        return result.WebViewLink;
    }

    public async Task DeleteFileAsync(string fileId)
    {
        var request = _driveService.Files.Delete(fileId);
        request.SupportsAllDrives = true;

        await RetryAsync(async () => await request.ExecuteAsync());
    }

    public async Task ArchiveFileAsync(string fileId, string archiveFolderId)
    {
        await MoveFileAsync(fileId, archiveFolderId);
    }
    private async Task<File> GetFileAsync(string fileId)
    {
        var request = _driveService.Files.Get(fileId);
        request.Fields = "id, parents, webViewLink";
        request.SupportsAllDrives = true;

        return await request.ExecuteAsync();
    }

    private static async Task<T> RetryAsync<T>(Func<Task<T>> action)
    {
        var attempts = 0;
        var delay = InitialRetryDelayMs;

        while (true)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (attempts < MaxRetries)
            {
                attempts++;
                await Task.Delay(delay);
                delay *= 2;
            }
        }
    }
}
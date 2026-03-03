using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using File = Google.Apis.Drive.v3.Data.File;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class GoogleDriveDocumentManager : IGoogleDriveDocumentManager
{
    private readonly IConfiguration _configuration;
    private DriveService _driveService = null!;
    private static readonly int MaxRetries = 5;
    private static readonly int InitialRetryDelayMs = 1000;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public GoogleDriveDocumentManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private DriveService InitializeDriveService(string? userToImpersonate = null)
    {
        var credentials = GetCredentials(userToImpersonate);

        if (credentials.IsCreateScopedRequired)
        {
            string[] scopes =
                { DriveService.Scope.Drive, DriveService.Scope.DriveReadonly, DriveService.Scope.DriveMetadata };
            credentials = credentials.CreateScoped(scopes);
        }

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credentials,
            ApplicationName = "PAO Document Management"
        });
    }

    private GoogleCredential GetCredentials(string? userToImpersonate)
    {
        var credentialParams = _configuration.GetSection("GoogleDriveSettings")
            .Get<JsonCredentialParameters>();
        if (credentialParams == null)
            throw new Exception("GoogleDriveSettings configuration is missing.");

        var secretManagerProvider = new GoogleSecretManagerConfigurationProvider(credentialParams.ProjectId);
        var secretName = _configuration.GetValue<string>("GoogleDriveSettings:GoogleDriveConnectionKeySecretId");
        if (!string.IsNullOrEmpty(secretName))
        {
            var secretValue = secretManagerProvider.GetSecretVersion(secretName, "latest");
            credentialParams.PrivateKey = secretValue?.Replace("\\n", "\n");
        }

        userToImpersonate = userToImpersonate == null ? _configuration.GetValue<string>("GoogleDriveSettings:ManagedUserAccount") : userToImpersonate;

        var credential = GoogleCredential.FromJsonParameters(credentialParams)
            .CreateScoped(new[] { DriveService.Scope.Drive })
            .CreateWithUser(userToImpersonate);

        return credential;
    }

    public async Task<Dictionary<string,string>> UploadFileAsync(IFormFile file, string fileName, string parentFolderId, string mimeType = "application/octet-stream")
    {
        _driveService = InitializeDriveService();

        using var stream = file.OpenReadStream();
        var fileMetadata = new File
        {
            Name = fileName,
            Parents = new[] { parentFolderId },
            MimeType = mimeType
        };

        var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
        request.Fields = "id, webViewLink";
        request.SupportsAllDrives = true;

        var result = await RetryAsync(async () => await request.UploadAsync());

        if (result.Status != UploadStatus.Completed)
        {
            throw new Exception("File upload failed.", result.Exception);
        }

        return new Dictionary<string, string>
        {
            { "id", request.ResponseBody.Id },
            { "webViewLink", request.ResponseBody.WebViewLink }
        };
    }

    public Dictionary<string, string> CopyFile(string fileId, string fileName, string parentFolderId, string mimeType = "application/octet-stream", string? userToImpersonate = null)
    {
        _driveService = InitializeDriveService(userToImpersonate);

        var fileMetadata = new File
        {
            Name = fileName,
            Parents = new[] { parentFolderId },
            MimeType = mimeType
        };

        var request = _driveService.Files.Copy(fileMetadata, fileId);
        request.Fields = "id, webViewLink";
        request.SupportsAllDrives = true;

        var result = request.Execute();
              
        return new Dictionary<string, string>
        {
            { "id", result.Id },
            { "webViewLink", result.WebViewLink }
        };
    }

    public async Task<Dictionary<string, string>> ExportFileAsync(string fileId, string fileName, string parentFolderId, string mimeType, string userToImpersonate)
    {
        _driveService = InitializeDriveService(userToImpersonate);

        var strRequest = _driveService.Files.Export(fileId, mimeType);
        MemoryStream stream = new();
        strRequest.Download(stream);

        var fileMetadata = new File
        {
            Name = fileName,
            Parents = [parentFolderId],
            MimeType = mimeType
        };

        var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
        request.Fields = "id, webViewLink";
        request.SupportsAllDrives = true;

        var result = await RetryAsync(async () => await request.UploadAsync());
        if (result.Status != UploadStatus.Completed)
            throw new Exception("File upload failed.", result.Exception);

        return new Dictionary<string, string>
        {
            { "id", request.ResponseBody.Id },
            { "webViewLink", request.ResponseBody.WebViewLink }
        };
    }

    public async Task<Dictionary<string, string>> CreateFolderAsync(string folderName, string parentFolderId)
    {
        _driveService = InitializeDriveService();

        await _semaphore.WaitAsync();
        try
        {
            var folderDictionary = await FindFolderIdAsync(folderName, parentFolderId);
            if (folderDictionary != null)
                return folderDictionary;

            var fileMetadata = new File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new[] { parentFolderId }
            };

            var request = _driveService.Files.Create(fileMetadata);
            request.Fields = "id, webViewLink";
            request.SupportsAllDrives = true;

            var createdFolder = await request.ExecuteAsync();
            
            return new Dictionary<string, string>
            {
                { "id", createdFolder.Id },
                { "webViewLink", createdFolder.WebViewLink }
            };

        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<Dictionary<string, string>?> FindFolderIdAsync(string folderName, string parentFolderId)
    {
        var query = $"name = '{folderName}' and mimeType = 'application/vnd.google-apps.folder' " +
                    $"and '{parentFolderId}' in parents and trashed = false";

        var request = _driveService.Files.List();
        request.Q = query;
        request.Fields = "files(id,webViewLink)";
        request.SupportsAllDrives = true;
        request.IncludeItemsFromAllDrives = true;
        request.IncludeTeamDriveItems = true;

        var result = await request.ExecuteAsync();
        var foundFolder = result.Files.FirstOrDefault();
        if (foundFolder != null)
        {
            
            return new Dictionary<string, string>
            {
                { "id", foundFolder.Id },
                { "webViewLink", foundFolder.WebViewLink }
            };
        }
        return null;
    }

    public async Task UpdateFilePermissionsAsync(string fileId, string email, string userToImpersonate, string role = "reader")
    {
        _driveService = InitializeDriveService(userToImpersonate);

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

    public async Task<Dictionary<string, string>> MoveFileAsync(string fileId, string targetFolderId)
    {
        _driveService = InitializeDriveService();

        var file = await GetFileAsync(fileId);
        if (file.Parents == null || file.Parents.Count == 0)
            throw new Exception("The file does not have a parent folder.");

        var request = _driveService.Files.Update(new File(), fileId);
        request.AddParents = targetFolderId;
        request.RemoveParents = file.Parents.First();
        request.Fields = "id, webViewLink";
        request.SupportsAllDrives = true;

        var result = await request.ExecuteAsync();

        return new Dictionary<string, string>
        {
            { "id", result.Id },
            { "webViewLink", result.WebViewLink }
        };
    }

    public async Task DeleteFileAsync(string fileId, string userToImpersonate)
    {
        _driveService = InitializeDriveService(userToImpersonate);

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

    public async Task<MemoryStream> GetFileStream(string fileId, string userToImpersonate)
    {
        try
        {
            _driveService = InitializeDriveService(userToImpersonate);

            // Get the file metadata to determine its MIME type
            var file = await GetFileMetadataAsync(fileId);

            var stream = new MemoryStream();

            // Determine the export MIME type based on the file's MIME type
            string exportMimeType = file.MimeType switch
            {
                "application/vnd.google-apps.document" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // DOCX
                "application/vnd.google-apps.spreadsheet" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // XLSX
                _ => "application/pdf" // Default to PDF for other Google file types
            };

            if (file.MimeType.StartsWith("application/vnd.google-apps"))
            {
                // Export Google Docs or Google Sheets to the specified format
                var exportRequest = _driveService.Files.Export(fileId, exportMimeType);
                await exportRequest.DownloadAsync(stream);
            }
            else
            {
                // Download other file types directly
                var request = _driveService.Files.Get(fileId);
                request.SupportsAllDrives = true;
                await request.DownloadAsync(stream);
            }

            // Check if the stream has content
            if (stream.Length == 0)
            {
                throw new Exception("The stream is empty. The file might not exist or the user might not have access.");
            }

            // Reset the stream position to the beginning
            stream.Position = 0;
            return stream;
        }
        catch (Exception ex)
        {
            // Log the exception (use your preferred logging framework)
            Console.WriteLine($"Error getting file stream: {ex.Message}");
            throw;
        }
    }

    private async Task<File> GetFileMetadataAsync(string fileId)
    {
        var request = _driveService.Files.Get(fileId);
        request.Fields = "id, name, mimeType, size, parents";
        request.SupportsAllDrives = true;

        var file = await request.ExecuteAsync();
        Console.WriteLine($"File ID: {file.Id}, Name: {file.Name}, MIME Type: {file.MimeType}, Size: {file.Size}");
        return file;
    }
}

using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Models;
using UNOPS.PAO.Business.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.DataAccess.Context;
using AutoMapper;
using UNOPS.PAO.Business.Repositories.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using Google.Cloud.Vision.V1;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Google.Cloud.TextToSpeech.V1;
using Google.Apis.Storage.v1.Data;
using System.IO;
using System.Collections.Generic;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class GoogleCloudStorageService
{
    private readonly StorageClient _storageClient;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;
    
    // Cache for URL signer to avoid repeated Secret Manager calls
    private static UrlSigner? _cachedUrlSigner;
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly object _cacheLock = new object();

    public GoogleCloudStorageService(IConfiguration configuration) 
    {
        _storageClient = StorageClient.Create();
        _configuration = configuration;
        _bucketName = configuration.GetValue<string>("AISettings:GoogleCloudStorageBucketName");
    }

    private async Task<string> UploadToGCS(Stream stream, string objectName, string contentType)
    {
        try
        {
            stream.Position = 0; // Ensure the stream is at the beginning
            await _storageClient.UploadObjectAsync(_bucketName, objectName, contentType, stream);
            return $"https://storage.cloud.google.com/{_bucketName}/{objectName}";
        }
        catch (Exception ex)
        {
            return ""; // Handle errors as needed
        }
    }

    // Overload for IFormFile
    public async Task<string> UploadFileToGCS(IFormFile file)
    {
        string objectName = $"{Guid.NewGuid()}_{file.FileName}"; // Unique filename
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return await UploadToGCS(memoryStream, objectName, file.ContentType);
    }

    // Overload for byte array (TTS audio)
    public async Task<string> UploadAudioToGCS(byte[] audioBytes)
    {
        string objectName = $"tts_audio_{Guid.NewGuid()}.mp3"; // Unique filename
        using var memoryStream = new MemoryStream(audioBytes);
        return await UploadToGCS(memoryStream, objectName, "audio/mpeg");
    }
    
    // Method called by UNOPSContactManager and UNOPSPartnerManager
    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return await UploadToGCS(memoryStream, fileName, file.ContentType);
    }

    /// <summary>
    /// Checks if a file with the same name and MIME type already exists in GCS
    /// </summary>
    /// <param name="folder">Folder name (e.g., "opportunities", "partners")</param>
    /// <param name="entityId">Entity ID for organizing files</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="mimeType">MIME type of the file</param>
    /// <returns>Existing gs:// URI if duplicate found, null otherwise</returns>
    public async Task<string?> CheckForDuplicateFileAsync(string folder, int entityId, string fileName, string mimeType)
    {
        try
        {
            // Construct object prefix: folder/entityId/
            var prefix = $"{folder.ToLower()}/{entityId}/";
            
            // List all objects with the prefix
            var objects = _storageClient.ListObjectsAsync(_bucketName, prefix);
            
            await foreach (var obj in objects)
            {
                // Check if file name matches (ignoring the GUID suffix)
                // Original file format: filename_GUID.ext
                // We want to match: filename*.ext with same MIME type
                var originalFileName = Path.GetFileNameWithoutExtension(fileName);
                var originalExtension = Path.GetExtension(fileName);
                
                // Extract the file name from the object (remove folder path)
                var objectFileName = Path.GetFileName(obj.Name);
                
                // Check if it starts with the original filename and has the same extension
                if (objectFileName.StartsWith(originalFileName + "_") && 
                    objectFileName.EndsWith(originalExtension, StringComparison.OrdinalIgnoreCase) &&
                    obj.ContentType == mimeType)
                {
                    // Found a duplicate - return its gs:// URI
                    return $"gs://{_bucketName}/{obj.Name}";
                }
            }
            
            return null; // No duplicate found
        }
        catch (Exception ex)
        {
            // If checking fails, return null to proceed with upload
            return null;
        }
    }

    /// <summary>
    /// Uploads PDF bytes to Google Cloud Storage with organized folder structure.
    /// Used for backend-generated PDFs (e.g., markdown-to-PDF conversion).
    /// </summary>
    /// <param name="pdfBytes">PDF file content as byte array</param>
    /// <param name="folder">Folder name (e.g., "opportunities", "partners")</param>
    /// <param name="entityId">Entity ID for organizing files</param>
    /// <param name="fileName">File name (e.g., "statement.pdf")</param>
    /// <returns>Google Cloud Storage URI (gs://bucket/path)</returns>
    public async Task<string> UploadPdfBytesAsync(byte[] pdfBytes, string folder, int entityId, string fileName)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            throw new ArgumentException("PDF bytes cannot be null or empty", nameof(pdfBytes));
        }

        var uniqueId = Guid.NewGuid().ToString();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{fileNameWithoutExtension}_{uniqueId}{fileExtension}";
        var objectName = $"{folder.ToLower()}/{entityId}/{uniqueFileName}";

        using var stream = new MemoryStream(pdfBytes);
        await _storageClient.UploadObjectAsync(
            _bucketName,
            objectName,
            "application/pdf",
            stream
        );

        return $"gs://{_bucketName}/{objectName}";
    }

    /// <summary>
    /// Uploads a PDF file to Google Cloud Storage with organized folder structure
    /// </summary>
    /// <param name="file">PDF file to upload</param>
    /// <param name="folder">Folder name (e.g., "opportunities", "partners")</param>
    /// <param name="entityId">Entity ID for organizing files</param>
    /// <returns>Google Cloud Storage URI (gs://bucket/path)</returns>
    public async Task<string> UploadPdfAsync(IFormFile file, string folder, int entityId)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File cannot be null or empty", nameof(file));
        }

        // Check for duplicate file first
        var duplicateUri = await CheckForDuplicateFileAsync(folder, entityId, file.FileName, file.ContentType ?? "application/pdf");
        if (duplicateUri != null)
        {
            // Return existing file URI instead of uploading a duplicate
            return duplicateUri;
        }

        // Generate unique filename to avoid collisions
        var fileExtension = Path.GetExtension(file.FileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
        var uniqueId = Guid.NewGuid().ToString();
        var fileName = $"{fileNameWithoutExtension}_{uniqueId}{fileExtension}";
        
        // Construct object path: folder/entityId/filename
        var objectName = $"{folder.ToLower()}/{entityId}/{fileName}";

        // Upload to GCS
        using var stream = file.OpenReadStream();
        await _storageClient.UploadObjectAsync(
            _bucketName, 
            objectName, 
            file.ContentType ?? "application/pdf", 
            stream
        );

        // Return gs:// URI
        return $"gs://{_bucketName}/{objectName}";
    }

    /// <summary>
    /// Generates a signed URL from a gs:// URI
    /// </summary>
    /// <param name="gsUri">Google Cloud Storage URI (gs://bucket/path)</param>
    /// <param name="expirationMinutes">Number of minutes before the URL expires (default: 60)</param>
    /// <returns>Signed URL that can be used to access the file</returns>
    public async Task<string> GetSignedUrlFromGsUri(string gsUri, int expirationMinutes = 60)
    {
        if (string.IsNullOrEmpty(gsUri) || !gsUri.StartsWith("gs://"))
        {
            throw new ArgumentException("Invalid Google Cloud Storage URI. Must start with gs://", nameof(gsUri));
        }

        // Parse gs:// URI to extract bucket and object name
        // Format: gs://bucket-name/path/to/object
        var uriWithoutPrefix = gsUri.Replace("gs://", "");
        var parts = uriWithoutPrefix.Split('/', 2);
        
        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid Google Cloud Storage URI format", nameof(gsUri));
        }

        // Decode URL-encoded characters in the object name to match actual GCS object name
        var objectName = Uri.UnescapeDataString(parts[1]);

        // Generate signed URL using existing method
        return await GenerateSignedUrlAsync(objectName, TimeSpan.FromMinutes(expirationMinutes));
    }

    // Generate a signed URL for secure access to a private object
    public async Task<string> GenerateSignedUrlAsync(string objectName, TimeSpan expiration, HttpMethod httpMethod = null)
    {
        try
        {
            // OPTIMIZATION: Skip object existence check for better performance  
            // The signed URL will gracefully fail if object doesn't exist
            
            // Use cached or create URL signer from Secret Manager (OPTIMIZED)
            var urlSigner = await CreateUrlSignerAsync();

            if (urlSigner == null)
            {
                throw new InvalidOperationException("Unable to create URL signer. No valid service account credentials found. " +
                    "Please ensure Google Cloud credentials are properly configured.");
            }

            // Generate signed URL
            var signedUrl = await urlSigner.SignAsync(
                _bucketName,
                objectName,
                expiration,
                httpMethod ?? HttpMethod.Get
            );

            return signedUrl;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate signed URL: {ex.Message}", ex);
        }
    }

    // Generate a signed URL with default 1 hour expiration
    public async Task<string> GenerateSignedUrlAsync(string objectName)
    {
        return await GenerateSignedUrlAsync(objectName, TimeSpan.FromHours(1));
    }

    // Generate signed URLs for multiple objects
    public async Task<Dictionary<string, string>> GenerateSignedUrlsAsync(IEnumerable<string> objectNames, TimeSpan expiration)
    {
        var signedUrls = new Dictionary<string, string>();
        
        foreach (var objectName in objectNames)
        {
            try
            {
                var signedUrl = await GenerateSignedUrlAsync(objectName, expiration);
                signedUrls.Add(objectName, signedUrl);
            }
            catch (Exception ex)
            {
                // Log error but continue with other objects
                signedUrls.Add(objectName, null);
            }
        }

        return signedUrls;
    }

    // Helper method to extract object name from Google Cloud Storage URL
    public string ExtractObjectNameFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        try
        {
            var uri = new Uri(url);
            
            // Handle different URL formats:
            // 1. https://storage.cloud.google.com/bucket-name/object/path
            // 2. https://storage.googleapis.com/bucket-name/object/path
            
            if (uri.Host == "storage.cloud.google.com" || uri.Host == "storage.googleapis.com")
            {
                // Use Uri.UnescapeDataString to decode URL-encoded characters
                var decodedPath = Uri.UnescapeDataString(uri.AbsolutePath);
                var pathSegments = decodedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathSegments.Length >= 2)
                {
                    // Skip the bucket name (first segment) and return the rest as object path
                    return string.Join("/", pathSegments.Skip(1));
                }
            }
            
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Generate signed URL from a Google Cloud Storage URL
    public async Task<string> GenerateSignedUrlFromStorageUrl(string storageUrl, TimeSpan? expiration = null)
    {
        var objectName = ExtractObjectNameFromUrl(storageUrl);
        
        if (string.IsNullOrEmpty(objectName))
        {
            return storageUrl; // Return original URL if we can't extract object name
        }

        try
        {
            var signedUrl = await GenerateSignedUrlAsync(objectName, expiration ?? TimeSpan.FromHours(1));
            return signedUrl;
        }
        catch (Exception)
        {
            // Try alternative approach with temporary access token
            try
            {
                var tempUrl = await GenerateTemporaryAccessUrl(objectName, expiration);
                if (tempUrl != $"https://storage.googleapis.com/{_bucketName}/{objectName}")
                {
                    return tempUrl;
                }
            }
            catch (Exception)
            {
                // Temporary access URL also failed
            }
            
            return storageUrl; // Return original URL if both approaches fail
        }
    }

    // Alternative method for development: Generate a temporary access token URL
    public async Task<string> GenerateTemporaryAccessUrl(string objectName, TimeSpan? expiration = null)
    {
        try
        {
            // For development environment, we can use the Google Cloud Storage client
            // to get a temporary access token and build a URL
            var credential = GoogleCredential.GetApplicationDefault();
            var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                // Create a URL with access token (less secure but works for development)
                var tempUrl = $"https://storage.googleapis.com/{_bucketName}/{objectName}?access_token={accessToken}";
                return tempUrl;
            }
        }
        catch (Exception ex)
        {
        }
        
        return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
    }

    // Cache management for URL signer
    private UrlSigner? GetCachedUrlSigner()
    {
        lock (_cacheLock)
        {
            if (_cachedUrlSigner != null && DateTime.UtcNow < _cacheExpiry)
            {
                return _cachedUrlSigner;
            }
            return null;
        }
    }

    private void SetCachedUrlSigner(UrlSigner urlSigner)
    {
        lock (_cacheLock)
        {
            _cachedUrlSigner = urlSigner;
            _cacheExpiry = DateTime.UtcNow.AddMinutes(30); // Cache for 30 minutes
        }
    }

    // Optimized URL signer creation with caching (Secret Manager only)
    private async Task<UrlSigner?> CreateUrlSignerAsync()
    {
        // Try cache first - avoid Secret Manager calls
        var cachedSigner = GetCachedUrlSigner();
        if (cachedSigner != null)
        {
            return cachedSigner;
        }

        // Load service account from Google Secret Manager
        try
        {
            var projectId = _configuration.GetValue<string>("AppConfig:ProjectId");
            var secretName = _configuration.GetValue<string>("GoogleDriveSettings:GoogleDriveServiceAccountJSONSecretName");
            
            if (!string.IsNullOrEmpty(projectId) && !string.IsNullOrEmpty(secretName))
            {
                var secretManager = new GoogleSecretManagerConfigurationProvider(projectId);
                var serviceAccountJson = secretManager.GetSecretVersion(secretName, "latest");
                
                if (!string.IsNullOrEmpty(serviceAccountJson))
                {
                    var serviceAccountCredential = ServiceAccountCredential.FromServiceAccountData(
                        new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serviceAccountJson)));
                    
                    var urlSigner = UrlSigner.FromCredential(serviceAccountCredential);
                    SetCachedUrlSigner(urlSigner); // Cache for 30 minutes
                    return urlSigner;
                }
            }
        }
        catch (Exception)
        {
            // Secret Manager failed
        }

        return null;
    }
}
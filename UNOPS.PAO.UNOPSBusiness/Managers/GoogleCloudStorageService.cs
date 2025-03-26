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

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class GoogleCloudStorageService
{
    private readonly StorageClient _storageClient;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;

    public GoogleCloudStorageService(IConfiguration configuration) 
    {
        _storageClient = StorageClient.Create();
        _configuration = configuration;
        _bucketName = configuration.GetValue<string>("GoogleDriveSettings:GoogleCloudStorageBucketName");
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
}
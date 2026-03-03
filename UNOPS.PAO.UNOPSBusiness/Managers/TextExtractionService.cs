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

public class TextExtractionService
{
    private readonly ImageAnnotatorClient _visionClient;
    private readonly SpeechClient _speechClient;

    public TextExtractionService() 
    {
        _visionClient = ImageAnnotatorClient.Create();
        _speechClient = SpeechClient.Create();
    }

    public async Task<string> ExtractDataFromFile(IFormFile file) {
        string fileType = FindFileType(file);
        string extractedText = "";
        string fileTypeText = "";
            
        if (fileType == "image") {
            fileTypeText = "(Image uploaded by User)";
            extractedText = await ProcessImage(file);
        } else if (fileType == "audio") {
            fileTypeText = "(Audio uploaded by User)";
            extractedText = await ProcessAudio(file);
        }

        extractedText = "Extracted Text " + fileTypeText + ": " + extractedText;
        return extractedText;

    }

    public string FindFileType(IFormFile file) 
    {
        string type = file?.ContentType ?? "";
        if (type.StartsWith("image/")) 
        {
            return "image";
        }
        else if (type.StartsWith("audio/") || type == "application/octet-stream")
        {
            return "audio";
        }
        return "";
    }

    private async Task<string> ProcessImage(IFormFile file) {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        byte[] fileBytes = memoryStream.ToArray();

        var image = Image.FromBytes(fileBytes);
        var response = await _visionClient.DetectTextAsync(image);

        if (response == null || response.Count == 0)
        {
            return "";
        }

        return string.Join(" ", response.Select(r => r.Description));
    }

    private async Task<string> ProcessAudio(IFormFile file) {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        byte[] fileBytes = memoryStream.ToArray();


        // Detect encoding based on the file extension or content
        var encoding = GetAudioEncoding(file.FileName.ToLower());
        if (encoding == null)
        {
            return "Unsupported audio format";
        }

        var audio = RecognitionAudio.FromBytes(fileBytes);
        var config = new RecognitionConfig
        {
            Encoding = encoding.Value,
            SampleRateHertz = 16000,
            LanguageCode = "en-US",
            Model = "default",
            EnableAutomaticPunctuation = true
        };

        var response = await _speechClient.RecognizeAsync(config, audio);

        if (response == null || response.Results.Count == 0)
        {
            return "";
        }

        return string.Join(" ", response.Results.SelectMany(r => r.Alternatives).Select(a => a.Transcript));
    }

    // Helper method to get the encoding based on file extension
    private RecognitionConfig.Types.AudioEncoding? GetAudioEncoding(string fileName)
    {
        if (fileName.EndsWith(".wav"))
        {
            return RecognitionConfig.Types.AudioEncoding.Linear16;  // WAV files are usually uncompressed PCM (Linear16)
        }
        if (fileName.EndsWith(".mp3"))
        {
            return RecognitionConfig.Types.AudioEncoding.Mp3;  // MP3 files
        }
        if (fileName.EndsWith(".flac"))
        {
            return RecognitionConfig.Types.AudioEncoding.Flac;  // FLAC files
        }
        if (fileName.EndsWith(".ogg") || fileName.EndsWith(".opus"))
        {
            return RecognitionConfig.Types.AudioEncoding.OggOpus;  // OGG and OPUS files
        }
        if (fileName.EndsWith(".mulaw"))
        {
            return RecognitionConfig.Types.AudioEncoding.Mulaw;  // Mu-law encoded audio
        }

        // Return null if the format is not supported
        return null;
    }
}
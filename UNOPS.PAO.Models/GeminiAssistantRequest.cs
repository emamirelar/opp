namespace UNOPS.PAO.Models;

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

public class GeminiAssistantRequest
{
    public string? Message { get; set; }
    public string? sessionId { get; set; }
    public IFormFile? File { get; set; }
    public string? ExtractedText { get; set; }
    public string? ScreenUrl { get; set; }
}
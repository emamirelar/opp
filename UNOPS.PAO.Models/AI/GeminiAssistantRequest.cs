namespace UNOPS.PAO.Models.AI;

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
    
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "session_id")]
    public string? sessionId { get; set; }
    
    public IFormFile? File { get; set; }
    public IFormFileCollection? Files { get; set; }
    
    /// <summary>
    /// JSON string containing array of GCS file paths
    /// Each file object has: gcsPath, name, mimeType
    /// </summary>
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "gcs_files")]
    public string? GcsFiles { get; set; }
    
    public string? ExtractedText { get; set; }
    public string? ScreenUrl { get; set; }
    public string? State { get; set; }
    public bool Streaming { get; set; } = false;
}
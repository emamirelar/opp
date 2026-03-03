using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models.Documents;

public class DocumentUploadModel : DocumentBaseCreateModel
{
    public IFormFile? File { get; set; }
    
    /// <summary>
    /// Flag to indicate if file should be uploaded to Google Cloud Storage
    /// </summary>
    public bool UploadToGCS { get; set; } = false;
    
    /// <summary>
    /// Flag to skip database persistence (for temporary uploads like AI proposal generation)
    /// When true, only uploads to GCS and returns the storage path without creating a database record
    /// </summary>
    public bool SkipDatabaseSave { get; set; } = false;
    
    /// <summary>
    /// Google Drive link (optional - for files sourced from Drive)
    /// </summary>
    public new string? Link { get; set; }
    
    /// <summary>
    /// Google Drive file ID (optional - for files sourced from Drive)
    /// </summary>
    public new string? GoogleId { get; set; }

    public bool? AITranscribed { get; set; }
}
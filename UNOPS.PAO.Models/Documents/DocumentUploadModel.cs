using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models.Documents;

public class DocumentUploadModel: DocumentBaseCreateModel
{
    public IFormFile File { get; set; }
    
    /// <summary>
    /// Flag to indicate if file should be uploaded to Google Cloud Storage
    /// </summary>
    public bool UploadToGCS { get; set; } = false;
    
    /// <summary>
    /// Google Drive link (optional - for files sourced from Drive)
    /// </summary>
    public string? Link { get; set; }
    
    /// <summary>
    /// Google Drive file ID (optional - for files sourced from Drive)
    /// </summary>
    public string? GoogleId { get; set; }
}
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models;

public class DocumentUploadModel: DocumentBaseCreateModel
{
    public IFormFile File { get; set; }
}
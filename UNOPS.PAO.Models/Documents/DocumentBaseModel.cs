using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.Documents;
public class DocumentBaseModel : ExtensibleModel
{
    public string Name { get; set; } = null!;
    public string? Type { get; set; }
    public string? Link { get; set; }
    public string? GoogleId { get; set; }
    public byte[]? Blob { get; set; }
    public string? StoragePath { get; set; }
}
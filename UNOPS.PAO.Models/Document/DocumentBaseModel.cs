using Microsoft.AspNetCore.Http;

namespace UNOPS.PAO.Models;
public class DocumentBaseModel: ExtensibleModel
{
    public string Name { get; set; }
    public string? Type { get; set; }
}
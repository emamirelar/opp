using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.Documents;
public class DocumentBaseModel: ExtensibleModel
{
    public string Name { get; set; }
    public string? Type { get; set; }
}
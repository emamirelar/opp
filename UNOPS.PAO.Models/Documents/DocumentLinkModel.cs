using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models.Documents;
public class DocumentLinkModel: DocumentBaseCreateModel
{
    public string Link { get; set; } = null!;
    public string GoogleId { get; set; } = null!;
}
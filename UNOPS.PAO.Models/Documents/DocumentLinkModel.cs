using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models.Documents;
public class DocumentLinkModel : DocumentBaseCreateModel
{
    public new string Link { get; set; } = null!;
    public new string GoogleId { get; set; } = null!;
}
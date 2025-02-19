using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models;

public class DocumentBaseCreateModel: DocumentBaseModel
{
    public DocumentParentEntityType ParentEntityType { get; set; }
    public int ParentEntityId { get; set; }
}
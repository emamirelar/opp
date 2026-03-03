using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Identity.Security.Enums;

namespace UNOPS.PAO.Models.Documents;

public class DocumentBaseCreateModel : DocumentBaseModel
{
    private DocumentParentEntityType _parentEntityType;
    public DocumentParentEntityType ParentEntityType
    {
        get
        {
            return EntityNames.ByName(ParentEntityName ?? "") switch
            {
                EntityNames.Contact => DocumentParentEntityType.Contact,
                EntityNames.Partner => DocumentParentEntityType.Partner,
                EntityNames.Interaction => DocumentParentEntityType.Interaction,
                EntityNames.PartnerTree => DocumentParentEntityType.PartnerTree,
                EntityNames.Opportunity => DocumentParentEntityType.Opportunity,
                _ => DocumentParentEntityType.Drive
            };
        }
        set
        {
            _parentEntityType = value;
        }
    }
    public string? ParentEntityName { get; set; }
    public int ParentEntityId { get; set; }
    public int? DocumentTypeId { get; set; }
}

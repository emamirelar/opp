using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using AutoMapper;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Project, ProjectModel>();
        CreateMap<ContactRequest, UNOPSContact>();
        CreateMap<UNOPSContact, ContactModel>();
        CreateMap<ContactModel, UNOPSContact>();
        CreateMap<InteractionRequest, UNOPSInteraction>();
        CreateMap<UNOPSInteraction, InteractionModel>();
        CreateMap<InteractionModel, UNOPSInteraction>();
        CreateMap<PartnerTreeRequest, UNOPSPartnerTree>();
        CreateMap<UNOPSPartnerTree, PartnerTreeModel>();
        CreateMap<PartnerTreeModel, UNOPSPartnerTree>();
        CreateMap<GeminiProcessDataRequest, AiPromptModel>();
        CreateMap<UNOPSDocument, DocumentModel>();
        CreateMap<DocumentModel, UNOPSDocument>();
        CreateMap<DocumentUploadModel, UNOPSDocument>();
        CreateMap<DocumentLinkModel, UNOPSDocument>();
        CreateMap<UpdateDocumentRequest, UNOPSDocument>();
        CreateMap<UNOPSOrganizationUnit, OrganizationUnitModel>();
        CreateMap<OrganizationUnitModel, UNOPSOrganizationUnit>();
        CreateMap<UNOPSPartnerCategory, PartnerCategoryModel>();
        CreateMap<PartnerCategoryModel, UNOPSPartnerCategory>();
    }
}

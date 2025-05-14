namespace UNOPS.PAO.Business.Managers.Mapping;
using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Document;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GrantUser, ApplicantModel>();
        CreateMap<Currency, CurrencyModel>();
        CreateMap<EligibleEntity, EligibleEntityModel>();
        CreateMap<Country, CountryModel>();
        CreateMap<Interaction, InteractionModel>();
        CreateMap<InteractionRequest, Interaction>();
        CreateMap<PartnerRequest, Partner>();
        CreateMap<UpdatePartnerRequest, Partner>();
        CreateMap<Partner, PartnerModel>();
        CreateMap<AiPromptModel, AiPrompt>();
        CreateMap<AiScreenMappingModel, AiScreenMapping>();
        CreateMap<AiChatHistoryModel, AiChatHistory>();
        CreateMap<AiChatSessionModel, AiChatSession>();
        CreateMap<Document, DocumentModel>();
        CreateMap<DocumentModel, Document>();
        CreateMap<DocumentUploadModel, Document>();
        CreateMap<DocumentType, DocumentTypeModel>();
        CreateMap<UpdateDocumentRequest, Document>();
        CreateMap<Link, LinkModel>();
        CreateMap<LinkRequest, Link>();
        CreateMap<UpdateLinkRequest, Link>();

        // OrganizationHierarchy mappings
        CreateMap<OrganizationHierarchy, OrganizationHierarchyModel>()
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Id : (int?)null))
            .ReverseMap();

        CreateMap<OrganizationHierarchy, OrganizationHierarchyTreeModel>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src));

        CreateMap<OrganizationHierarchy, OrganizationHierarchyDataModel>()
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Id : (int?)null))
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));

        CreateMap<PartnerCategory, PartnerCategoryModel>();
        CreateMap<PartnerCategoryModel, PartnerCategory>();
        CreateMap<Partner, PartnerValueModel>();
        CreateMap<Contact, ContactValueModel>();
        CreateMap<GrantUser, UserValueModel>();
        CreateMap<UserProfile, UserProfileValueModel>();
        CreateMap<GrantUser, PAOUserModel>();
    }
}

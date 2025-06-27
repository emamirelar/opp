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
        CreateMap<Partner, PartnerModel>()
            .ForMember(dest => dest.First5ContactsByDate, opt => opt.MapFrom(src => src.First5ContactsByDate));
        CreateMap<PartnerModel, Partner>();
        CreateMap<Contact, ContactValueModel>();
        CreateMap<Contact, ContactModel>()
            .ForMember(dest => dest.Partner, opt => opt.MapFrom(src => src.Partner != null ? new PartnerSummaryModel { Id = src.Partner.Id, Name = src.Partner.Name } : null));
        CreateMap<ContactModel, Contact>()
            .ForMember(dest => dest.Partner, opt => opt.Ignore());
        
        // AI Prompt mappings
        CreateMap<AiPrompt, AiPromptModel>();
        CreateMap<AiPromptModel, AiPrompt>();
        
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
        CreateMap<Partner, PartnerValueModel>();
        CreateMap<PartnerTree, PartnerTreeModel>();
        CreateMap<PartnerTreeModel, PartnerTree>();
        CreateMap<GrantUser, UserValueModel>();
        CreateMap<GrantUser, PAOUserModel>();
        CreateMap<UserProfile, UserProfileValueModel>();
    }
}

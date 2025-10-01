namespace UNOPS.PAO.Business.Managers.Mapping;
using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Document;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PAOUser, ApplicantModel>();
        CreateMap<Currency, CurrencyModel>();
        CreateMap<EligibleEntity, EligibleEntityModel>();
        CreateMap<Country, CountryModel>();
        CreateMap<Interaction, InteractionModel>();
        CreateMap<InteractionRequest, Interaction>()
            .ForMember(dest => dest.OrganizationUnitRelationships, opt => opt.Ignore()); // Handle manually in manager
        CreateMap<PartnerRequest, Partner>()
            .ForMember(dest => dest.OrganizationUnitRelationships, opt => opt.Ignore()); // Handle manually in manager
        CreateMap<UpdatePartnerRequest, Partner>()
            .ForMember(dest => dest.OrganizationUnitRelationships, opt => opt.Ignore()); // Handle manually in manager
        CreateMap<Partner, PartnerModel>()
            .PreserveReferences()
            .MaxDepth(2)
            .ForMember(dest => dest.First5ContactsByDate, opt => opt.MapFrom(src => src.First5ContactsByDate));
        CreateMap<PartnerModel, Partner>();
        CreateMap<Contact, ContactValueModel>();
        CreateMap<Contact, ContactModel>()
            .PreserveReferences()
            .MaxDepth(2)
            .ForMember(dest => dest.Partner, opt => opt.MapFrom(src => src.Partner != null ? new PartnerSummaryModel { Id = src.Partner.Id, Name = src.Partner.Name } : null));
        CreateMap<ContactModel, Contact>()
            .ForMember(dest => dest.Partner, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationUnitRelationships, opt => opt.Ignore()); // Handle manually in manager

        // AI Prompt mappings
        CreateMap<AiPrompt, AiPromptModel>();
        CreateMap<AiPromptModel, AiPrompt>();

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
        CreateMap<PAOUser, UserValueModel>();
        CreateMap<PAOUser, PAOUserModel>();
        CreateMap<UserProfile, UserProfileValueModel>();
        
        // OrganizationUnitRelationship mappings
        CreateMap<OrganizationUnitRelationship, OrganizationUnitRelationshipModel>().ReverseMap();
    }
}

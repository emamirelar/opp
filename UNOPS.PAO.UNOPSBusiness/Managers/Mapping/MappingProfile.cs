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
        CreateMap<Project, ProjectSummaryModel>();
        CreateMap<UNOPSPartner, UNOPS.PAO.Models.PartnerSummaryModel>();
        CreateMap<ContactRequest, UNOPSContact>();
        CreateMap<UNOPSContact, ContactModel>()
            .ForMember(dest => dest.Partner, opt => opt.MapFrom(src => src.Partner != null ? new UNOPS.PAO.Models.PartnerSummaryModel { Id = src.Partner.Id, Name = src.Partner.Name } : null))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl))
            .ForMember(dest => dest.Interactions, opt => opt.MapFrom((src, dest, destMember, context) => 
                src.Interactions != null ? src.Interactions.Cast<UNOPSInteraction>().Select(interaction => context.Mapper.Map<UNOPSInteraction, InteractionModel>(interaction)).ToList() : null));
        CreateMap<ContactModel, UNOPSContact>()
            .ForMember(dest => dest.Partner, opt => opt.Ignore());
        CreateMap<InteractionRequest, UNOPSInteraction>()
            .ForMember(dest => dest.OrganizationUnitRelationships, opt => opt.Ignore()); // Handle manually in manager
        CreateMap<UNOPSInteraction, InteractionModel>()
            .ForMember(dest => dest.ContactId, opt => opt.MapFrom(src => 
                src.InteractionContacts != null && src.InteractionContacts.Any() 
                    ? src.InteractionContacts.First().ContactId 
                    : 0))
            .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => 
                src.InteractionContacts != null && src.InteractionContacts.Any() 
                    ? src.InteractionContacts.First().Contact.Name 
                    : null));
        CreateMap<InteractionModel, UNOPSInteraction>()
            .ForMember(dest => dest.InteractionContacts, opt => opt.Ignore()); // Handle via junction table processing
        CreateMap<PartnerTreeRequest, UNOPSPartnerTree>();
        

        CreateMap<UNOPSPartnerTree, PartnerTreeModel>()
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => new PartnerTreeDataModel 
            {
                Id = src.Id,
                Code = src.Code,
                Description = src.Description,
                Type = src.Type,
                PartnerCategoryCode = src.PartnerCategoryCode,
                PartnerGroupCode = src.PartnerGroupCode,
            }));
            
        CreateMap<PartnerTreeModel, UNOPSPartnerTree>()
            .ForMember(dest => dest.Partners, opt => opt.Ignore());
            
        CreateMap<GeminiProcessDataRequest, AiPromptModel>();
        CreateMap<UNOPSDocument, DocumentModel>();
        CreateMap<DocumentModel, UNOPSDocument>();
        CreateMap<DocumentUploadModel, UNOPSDocument>();
        CreateMap<DocumentLinkModel, UNOPSDocument>();
        CreateMap<UpdateDocumentRequest, UNOPSDocument>();
        
        CreateMap<PartnerRequest, UNOPSPartner>()
            .ForMember(dest => dest.OrganizationUnitRelationships, opt => opt.Ignore()); // Handle manually in manager
        CreateMap<UpdatePartnerRequest, UNOPSPartner>()
            .ForMember(dest => dest.OrganizationUnitRelationships, opt => opt.Ignore()); // Handle manually in manager
        
        CreateMap<UNOPSPartner, PartnerModel>()
            .ForMember(dest => dest.First5ContactsByDate, opt => opt.MapFrom((src, dest, destMember, context) => 
                src.First5ContactsByDate.Cast<UNOPSContact>().Select(contact => context.Mapper.Map<UNOPSContact, ContactModel>(contact)).ToList()));
    }
}

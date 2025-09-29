using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using AutoMapper;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSDomain.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UNOPSPartner, UNOPS.PAO.Models.PartnerSummaryModel>();
        CreateMap<ContactRequest, UNOPSContact>();
        CreateMap<UNOPSContact, ContactModel>()
            .PreserveReferences()
            .MaxDepth(2)
            .ForMember(dest => dest.Partner, opt => opt.MapFrom(src => src.Partner != null ? new UNOPS.PAO.Models.PartnerSummaryModel { Id = src.Partner.Id, Name = src.Partner.Name } : null))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl))
            .ForMember(dest => dest.Interactions, opt => opt.Ignore()); // Avoid circular reference - handle separately if needed
        CreateMap<ContactModel, UNOPSContact>()
            .ForMember(dest => dest.Partner, opt => opt.Ignore());
        CreateMap<InteractionRequest, UNOPSInteraction>()
            .ForMember(dest => dest.OrganizationUnitRelationships, opt => opt.Ignore()); // Handle manually in manager
        CreateMap<UNOPSInteraction, InteractionModel>()
            .PreserveReferences()
            .MaxDepth(2)
            .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => 
                src.InteractionContacts != null && src.InteractionContacts.Any() 
                    ? $"{src.InteractionContacts.First().Contact.FirstName} {src.InteractionContacts.First().Contact.LastName}".Trim()
                    : null))
            .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => 
                src.InteractionContacts != null 
                    ? src.InteractionContacts.Select(ic => new ContactModel 
                    { 
                        Id = ic.Contact.Id, 
                        FirstName = ic.Contact.FirstName, 
                        LastName = ic.Contact.LastName,
                        Email = ic.Contact.Email,
                        Phone = ic.Contact.Phone,
                        Title = ic.Contact.Title,
                        ProfilePictureUrl = ic.Contact.ProfilePictureUrl,
                        Partner = ic.Contact.Partner != null ? new UNOPS.PAO.Models.PartnerSummaryModel { Id = ic.Contact.Partner.Id, Name = ic.Contact.Partner.Name } : null,
                        Interactions = null // Explicitly break circular reference
                    }).ToList() 
                    : new List<ContactModel>()))
            .ForMember(dest => dest.Partners, opt => opt.MapFrom(src => 
                src.InteractionPartners != null
                    ? src.InteractionPartners.Select(ip => new PartnerModel 
                    { 
                        Id = ip.Partner.Id, 
                        Name = ip.Partner.Name,
                        PartnerShortDescription = ip.Partner.PartnerShortDescription,
                        PartnerLongDescription = ip.Partner.PartnerLongDescription,
                        LogoUrl = ip.Partner.LogoUrl,
                        First5ContactsByDate = null // Explicitly break circular reference
                    }).ToList() 
                    : new List<PartnerModel>()))
            .ForMember(dest => dest.Users, opt => opt.MapFrom((src, dest, destMember, context) => 
                src.InteractionUsers != null ? src.InteractionUsers.Select(iu => context.Mapper.Map<PAOUser, UserValueModel>(iu.User)).ToList() : new List<UserValueModel>()))
            .ForMember(dest => dest.ContactIds, opt => opt.MapFrom(src => 
                src.InteractionContacts != null ? src.InteractionContacts.Select(ic => ic.ContactId).ToList() : new List<int>()))
            .ForMember(dest => dest.PartnerIds, opt => opt.MapFrom(src => 
                src.InteractionPartners != null ? src.InteractionPartners.Select(ip => ip.PartnerId).ToList() : new List<int>()))
;
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
            }));
            
        CreateMap<PartnerTreeModel, UNOPSPartnerTree>()
            .ForMember(dest => dest.Partners, opt => opt.Ignore());
            
        CreateMap<GeminiProcessDataRequest, AiPromptModel>();
        CreateMap<UNOPSDocument, DocumentModel>();
        CreateMap<DocumentModel, UNOPSDocument>();
        CreateMap<DocumentUploadModel, UNOPSDocument>();
        CreateMap<DocumentLinkModel, UNOPSDocument>();
        CreateMap<UpdateDocumentRequest, UNOPSDocument>();
        
        // User mappings for interaction user resolution
        CreateMap<PAOUser, UserValueModel>();
        CreateMap<UserProfile, UserProfileValueModel>();
        
        CreateMap<PartnerRequest, UNOPSPartner>()
            .ForMember(dest => dest.OrganizationUnitRelationships, opt => opt.Ignore()); // Handle manually in manager
        CreateMap<UpdatePartnerRequest, UNOPSPartner>()
            .ForMember(dest => dest.OrganizationUnitRelationships, opt => opt.Ignore()); // Handle manually in manager
        
        CreateMap<UNOPSPartner, PartnerModel>()
            .PreserveReferences()
            .MaxDepth(2)
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.LogoUrl))
            .ForMember(dest => dest.First5ContactsByDate, opt => opt.MapFrom(src => 
                src.First5ContactsByDate != null
                    ? src.First5ContactsByDate.Cast<UNOPSContact>().Select(contact => new ContactModel 
                    {
                        Id = contact.Id,
                        FirstName = contact.FirstName,
                        LastName = contact.LastName,
                        Email = contact.Email,
                        Phone = contact.Phone,
                        Title = contact.Title,
                        Partner = contact.Partner != null ? new UNOPS.PAO.Models.PartnerSummaryModel { Id = contact.Partner.Id, Name = contact.Partner.Name } : null,
                        Interactions = null // Explicitly break circular reference
                    }).ToList()
                    : new List<ContactModel>()));
    }
}

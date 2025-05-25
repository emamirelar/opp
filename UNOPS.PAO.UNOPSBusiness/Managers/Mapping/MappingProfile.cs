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
        CreateMap<UNOPSContact, ContactModel>()
            .ForMember(dest => dest.Partner, opt => opt.Ignore())
            .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.Name : null))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl));
        CreateMap<ContactModel, UNOPSContact>()
            .ForMember(dest => dest.Partner, opt => opt.Ignore());
        CreateMap<InteractionRequest, UNOPSInteraction>();
        CreateMap<UNOPSInteraction, InteractionModel>();
        CreateMap<InteractionModel, UNOPSInteraction>();
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
        CreateMap<UNOPSOrganizationUnit, OrganizationUnitModel>();
        CreateMap<OrganizationUnitModel, UNOPSOrganizationUnit>();
        
        CreateMap<PartnerRequest, UNOPSPartner>();
        CreateMap<UpdatePartnerRequest, UNOPSPartner>();
        
        CreateMap<UNOPSPartner, PartnerModel>()
            .ForMember(dest => dest.First5ContactsByDate, opt => opt.MapFrom((src, dest, destMember, context) => 
                src.First5ContactsByDate.Cast<UNOPSContact>().Select(contact => context.Mapper.Map<UNOPSContact, ContactModel>(contact)).ToList()));
    }
}

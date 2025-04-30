using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Add mappings here
        CreateMap<PartnerTreeDataModel, PartnerTree>().ReverseMap();
        CreateMap<PartnerTree, PartnerTreeModel>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src));
        CreateMap<PartnerTree, PartnerTreeDataModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status)); // Ensure Status is mapped
        CreateMap<Link, LinkModel>();
        CreateMap<LinkRequest, Link>();
        CreateMap<UpdateLinkRequest, Link>();
        CreateMap<InteractionContact, InteractionContactModel>();
        CreateMap<InteractionContactModel, InteractionContact>();
        CreateMap<InteractionPartner, InteractionPartnerModel>();
        CreateMap<InteractionPartnerModel, InteractionPartner>();
        CreateMap<InteractionUser, InteractionUserModel>();
        CreateMap<InteractionUserModel, InteractionUser>();
        CreateMap<OrganizationUnit, OrganizationUnitModel>();
        CreateMap<OrganizationUnitModel, OrganizationUnit>();
    }
}

using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSDomain.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Add mappings here
        CreateMap<PartnerTreeDataModel, UNOPSPartnerTree>().ReverseMap();
        CreateMap<UNOPSPartnerTree, PartnerTreeModel>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src));
        CreateMap<UNOPSPartnerTree, PartnerTreeDataModel>();
    }
}

using AutoMapper;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Managers.Mapping;

public class BaseEngagementMappingProfile : Profile
{
    public BaseEngagementMappingProfile()
    {
        CreateMap<BaseEngagement, BaseEngagementModel>()
            .ForMember(dest => dest.Partners, opt => opt.MapFrom(src => src.EngagementPartners));
            
        CreateMap<BaseEngagementPartners, BaseEngagementPartnerModel>()
            .ForMember(dest => dest.EngagementDescription, opt => opt.MapFrom(src => 
                src.BaseEngagementEntity != null ? src.BaseEngagementEntity.EngagementDescription : string.Empty))
            .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => 
                src.PartnerEntity != null ? src.PartnerEntity.Name : string.Empty));
    }
}

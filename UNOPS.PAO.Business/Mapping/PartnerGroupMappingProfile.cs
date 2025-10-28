using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.PartnerTrees;

namespace UNOPS.PAO.Business.Mapping;

/// <summary>
/// AutoMapper profile for PartnerGroup entity and models
/// </summary>
public class PartnerGroupMappingProfile : Profile
{
    public PartnerGroupMappingProfile()
    {
        // PartnerGroup to PartnerGroupModel mapping
        CreateMap<PartnerGroup, PartnerGroupModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PartnerCategory, opt => opt.MapFrom(src => src.PartnerCategory))
            .ForMember(dest => dest.Partners, opt => opt.MapFrom(src => src.Partners))
            .ForMember(dest => dest.PartnerCount, opt => opt.MapFrom(src => src.PartnerCount))
            .ForMember(dest => dest.TotalPartnerCount, opt => opt.MapFrom(src => src.TotalPartnerCount))
            .ForMember(dest => dest.Permissions, opt => opt.Ignore()); // Will be populated separately

        // PartnerGroupModel to PartnerGroup mapping (if needed for updates)
        CreateMap<PartnerGroupModel, PartnerGroup>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<Domain.Entities.EntityStatus>(src.Status)))
            .ForMember(dest => dest.PartnerCategory, opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Partners, opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.PartnerCount, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.TotalPartnerCount, opt => opt.Ignore()); // Computed property
    }
}

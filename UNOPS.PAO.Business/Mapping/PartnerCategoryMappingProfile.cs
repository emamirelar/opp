using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Partners;

namespace UNOPS.PAO.Business.Mapping;

/// <summary>
/// AutoMapper profile for PartnerCategory entity and models
/// </summary>
public class PartnerCategoryMappingProfile : Profile
{
    public PartnerCategoryMappingProfile()
    {
        // PartnerCategory to PartnerCategoryModel mapping
        CreateMap<PartnerCategory, PartnerCategoryModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PartnerGroups, opt => opt.MapFrom(src => src.PartnerGroups))
            .ForMember(dest => dest.PartnerGroupCount, opt => opt.MapFrom(src => src.PartnerGroupCount))
            .ForMember(dest => dest.TotalPartnerCount, opt => opt.MapFrom(src => src.TotalPartnerCount))
            .ForMember(dest => dest.Permissions, opt => opt.Ignore()); // Will be populated separately

        // PartnerCategoryModel to PartnerCategory mapping (if needed for updates)
        CreateMap<PartnerCategoryModel, PartnerCategory>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<Domain.Entities.EntityStatus>(src.Status)))
            .ForMember(dest => dest.PartnerGroups, opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.PartnerGroupCount, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.TotalPartnerCount, opt => opt.Ignore()); // Computed property
    }
}

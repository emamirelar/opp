using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Locations;

namespace UNOPS.PAO.Business.Mapping;

/// <summary>
/// AutoMapper profile for Country entity and models
/// </summary>
public class CountryMappingProfile : Profile
{
    public CountryMappingProfile()
    {
        // Map Country entity to CountryModel with artifacts
        CreateMap<Country, CountryModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PartnerCount, opt => opt.MapFrom(src => src.PartnerCount))
            .ForMember(dest => dest.LiaisonOfficeCount, opt => opt.MapFrom(src => src.LiaisonOfficeCount))
            .ForMember(dest => dest.HasActiveUNCF, opt => opt.MapFrom(src => src.HasActiveUNCF))
            .ForMember(dest => dest.Permissions, opt => opt.Ignore()) // Will be populated separately
            .ForMember(dest => dest.Artifacts, opt => opt.MapFrom<EntityArtifactValueResolver>())
            .ForMember(dest => dest.Continent, opt => opt.MapFrom(src => src.ContinentDescription))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.RegionDescription));

        // CountryModel to Country mapping (if needed for updates)
        CreateMap<CountryModel, Country>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<Domain.Entities.EntityStatus>(src.Status)))
            .ForMember(dest => dest.PartnerCount, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.LiaisonOfficeCount, opt => opt.Ignore()); // Computed property
    }
}
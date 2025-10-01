using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Mapping;

/// <summary>
/// AutoMapper profile for Country entity and models
/// </summary>
public class CountryMappingProfile : Profile
{
    public CountryMappingProfile()
    {
        // Country to CountryModel mapping
        CreateMap<Country, CountryModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PartnerCount, opt => opt.MapFrom(src => src.PartnerCount))
            .ForMember(dest => dest.LiaisonOfficeCount, opt => opt.MapFrom(src => src.LiaisonOfficeCount))
            .ForMember(dest => dest.Permissions, opt => opt.Ignore()); // Will be populated separately

        // CountryModel to Country mapping (if needed for updates)
        CreateMap<CountryModel, Country>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<Domain.Entities.EntityStatus>(src.Status)))
            .ForMember(dest => dest.PartnerCount, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.LiaisonOfficeCount, opt => opt.Ignore()); // Computed property
    }
}
using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.LiaisonOffices;

namespace UNOPS.PAO.Business.Mapping;

/// <summary>
/// AutoMapper profile for LiaisonOffice entity and models
/// </summary>
public class LiaisonOfficeMappingProfile : Profile
{
    public LiaisonOfficeMappingProfile()
    {
        // LiaisonOffice to LiaisonOfficeModel mapping
        CreateMap<LiaisonOffice, LiaisonOfficeModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PartnerCount, opt => opt.MapFrom(src => src.PartnerCount))
            .ForMember(dest => dest.Permissions, opt => opt.Ignore()); // Will be populated separately

        // LiaisonOfficeModel to LiaisonOffice mapping (if needed for updates)
        CreateMap<LiaisonOfficeModel, LiaisonOffice>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<Domain.Entities.EntityStatus>(src.Status)))
            .ForMember(dest => dest.PartnerCount, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.Partners, opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedDate, opt => opt.Ignore());
    }
}

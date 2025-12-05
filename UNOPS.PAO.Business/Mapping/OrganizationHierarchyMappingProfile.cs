using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.OrganizationUnits;

namespace UNOPS.PAO.Business.Mapping;

/// <summary>
/// AutoMapper profile for OrganizationHierarchy entity and models
/// </summary>
public class OrganizationHierarchyMappingProfile : Profile
{
    public OrganizationHierarchyMappingProfile()
    {
        // OrganizationHierarchy to OrganizationHierarchyModel mapping
        CreateMap<OrganizationHierarchy, OrganizationHierarchyModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null))
            .ForMember(dest => dest.ParentCode, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Code : null))
            .ForMember(dest => dest.ChildrenCount, opt => opt.MapFrom(src => src.ChildrenCount))
            .ForMember(dest => dest.EntityRelationshipCount, opt => opt.MapFrom(src => src.EntityRelationshipCount))
            .ForMember(dest => dest.Permissions, opt => opt.Ignore()) // Will be populated separately
            .ForMember(dest => dest.Artifacts, opt => opt.MapFrom<EntityArtifactValueResolver>());

        // OrganizationHierarchyModel to OrganizationHierarchy mapping (if needed for updates)
        CreateMap<OrganizationHierarchyModel, OrganizationHierarchy>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<Domain.Entities.EntityStatus>(src.Status)))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<Domain.Enums.OrganizationUnitType>(src.Type)))
            .ForMember(dest => dest.Parent, opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Children, opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.EntityRelationships, opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.ParentName, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.ParentCode, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.ChildrenCount, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.EntityRelationshipCount, opt => opt.Ignore()) // Computed property
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
            .ForSourceMember(src => src.Artifacts, opt => opt.DoNotValidate()); // Computed from EntityArtifacts

        // OrganizationHierarchy to OrganizationHierarchyDataModel mapping (for tree view)
        CreateMap<OrganizationHierarchy, OrganizationHierarchyDataModel>()
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));
    }
}
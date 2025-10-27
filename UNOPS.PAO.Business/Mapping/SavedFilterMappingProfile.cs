using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Filters;

namespace UNOPS.PAO.Business.Mapping
{
    /// <summary>
    /// AutoMapper profile for SavedFilter entity and models
    /// </summary>
    public class SavedFilterMappingProfile : Profile
    {
        public SavedFilterMappingProfile()
        {
            // Entity to Model mapping
            CreateMap<SavedFilter, SavedFilterModel>()
                .ForMember(dest => dest.OrderBy, opt => opt.MapFrom(src => src.OrderByField))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(src => src.LastModifiedDate))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));

            // Request to Entity mappings are handled manually in the service
            // to avoid confusion with audit fields
        }
    }
} 
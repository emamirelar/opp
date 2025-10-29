using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Mapping;

/// <summary>
/// AutoMapper profile for Comment entity mappings
/// </summary>
public class CommentMappingProfile : Profile
{
    public CommentMappingProfile()
    {
        // Comment -> CommentModel
        CreateMap<Comment, CommentModel>()
            .ForMember(dest => dest.MentionedUserNames, opt => opt.Ignore()) // Will be populated by manager
            .ForMember(dest => dest.CreatedByName, opt => opt.Ignore()) // Will be populated by manager
            .ForMember(dest => dest.LastModifiedByName, opt => opt.Ignore()) // Will be populated by manager
            .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));

        // CommentRequest -> Comment
        CreateMap<CommentRequest, Comment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
            .ForMember(dest => dest.Replies, opt => opt.Ignore())
            .ForMember(dest => dest.IsEdited, opt => opt.Ignore())
            .ForMember(dest => dest.IsPinned, opt => opt.Ignore())
            .ForMember(dest => dest.MentionedUserIds, opt => opt.Ignore()) // Handled in manager
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedDate, opt => opt.Ignore());
    }
}


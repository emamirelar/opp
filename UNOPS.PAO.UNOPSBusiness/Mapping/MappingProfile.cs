using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PartnerTreeDataModel, UNOPSPartnerTree>().ReverseMap();
            CreateMap<UNOPSPartnerTree, PartnerTreeModel>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src));
            CreateMap<UNOPSPartnerTree, PartnerTreeDataModel>();
            CreateMap<AiPrompt, AiPromptModel>().ReverseMap();
            CreateMap<GeminiProcessDataRequest, AiPrompt>().ReverseMap();
            CreateMap<AiChatHistory, AiChatHistoryModel>().ReverseMap();
            CreateMap<AiChatSession, AiChatSessionModel>().ReverseMap();
            CreateMap<EntityEmbeddings, EntityEmbeddingsModel>().ReverseMap();
        }
    }
}
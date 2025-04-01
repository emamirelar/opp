namespace UNOPS.PAO.Business.Managers.Mapping;
using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Document;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GrantUser, ApplicantModel>();
        CreateMap<Currency, CurrencyModel>();
        CreateMap<EligibleEntity, EligibleEntityModel>();
        CreateMap<Country, CountryModel>();
        CreateMap<Interaction, InteractionModel>();
        CreateMap<InteractionRequest, Interaction>();
        CreateMap<PartnerRequest, Partner>();
        CreateMap<UpdatePartnerRequest, Partner>();
        CreateMap<Partner, PartnerModel>();
        CreateMap<AiPromptModel, AiPrompt>();
        CreateMap<AiScreenMappingModel, AiScreenMapping>();
        CreateMap<AiChatHistoryModel, AiChatHistory>();
        CreateMap<AiChatSessionModel, AiChatSession>();
        CreateMap<Document, DocumentModel>();
        CreateMap<DocumentModel, Document>();
        CreateMap<DocumentUploadModel, Document>();
        CreateMap<DocumentType, DocumentTypeModel>();
        CreateMap<UpdateDocumentRequest, Document>();
    }
}

namespace UNOPS.PAO.Business.Managers.Mapping;
using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GrantUser, ApplicantModel>();
        CreateMap<Document, DocumentModel>();
        CreateMap<DocumentModel, Document>();
        CreateMap<DocumentUploadModel, Document>();
        CreateMap<Currency, CurrencyModel>();
        CreateMap<EligibleEntity, EligibleEntityModel>();
        CreateMap<Country, CountryModel>();
        CreateMap<Interaction, InteractionModel>();
        CreateMap<InteractionRequest, Interaction>();
        CreateMap<PartnerRequest, Partner>();
        CreateMap<UpdatePartnerRequest, Partner>();
        CreateMap<Partner, PartnerModel>();
    }
}

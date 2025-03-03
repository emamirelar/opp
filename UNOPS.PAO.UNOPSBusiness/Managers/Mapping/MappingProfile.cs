using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using AutoMapper;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<FundingOpportunityRequest, UNOPSFundingOpportunity>();
        CreateMap<UNOPSFundingOpportunity, FundingOpportunityModel>();
        CreateMap<FundingOpportunityModel, UNOPSFundingOpportunity>();
        CreateMap<Project, ProjectModel>();
        CreateMap<UNOPSDocument, DocumentModel>();
        CreateMap<DocumentModel, UNOPSDocument>();
        CreateMap<DocumentUploadModel, UNOPSDocument>();
        CreateMap<DocumentLinkModel, UNOPSDocument>();
        CreateMap<ContactRequest, UNOPSContact>();
        CreateMap<UNOPSContact, ContactModel>();
        CreateMap<ContactModel, UNOPSContact>();
        CreateMap<InteractionRequest, UNOPSInteraction>();
        CreateMap<UNOPSInteraction, InteractionModel>();
        CreateMap<InteractionModel, UNOPSInteraction>();
    }
}

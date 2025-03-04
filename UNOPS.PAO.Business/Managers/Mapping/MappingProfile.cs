namespace UNOPS.PAO.Business.Managers.Mapping;
using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<FundingOpportunityRequest, FundingOpportunity>();
        CreateMap<UpdateFundingOpportunityRequest, FundingOpportunity>();
        CreateMap<FundingOpportunity, FundingOpportunityModel>();
        CreateMap<FundingOpportunity, ExternalFundingOpportunityModel>();
        CreateMap<SelectionMethodology, SelectonMethodologyModel>();
        CreateMap<ProposalRequest, Proposal>();
        CreateMap<Proposal, ProposalModel>();
        CreateMap<Proposal, ExternalProposalModel>();
        CreateMap<GrantUser, ApplicantModel>();
        CreateMap<Document, DocumentModel>();
        CreateMap<DocumentModel, Document>();
        CreateMap<DocumentUploadModel, Document>();
        CreateMap<Currency, CurrencyModel>();
        CreateMap<EligibleEntity, EligibleEntityModel>();
        CreateMap<SDG, SDGModel>();
        CreateMap<Country, CountryModel>();
        CreateMap<PartnerRequest, Partner>();
        CreateMap<UpdatePartnerRequest, Partner>();
        CreateMap<Partner, PartnerModel>();
    }
}

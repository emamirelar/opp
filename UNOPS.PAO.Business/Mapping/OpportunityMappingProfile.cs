using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Mapping;

public class OpportunityMappingProfile : Profile
{
    public OpportunityMappingProfile()
    {
        // Opportunity mappings
        CreateMap<Opportunity, OpportunityModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.WorkflowStageName, opt => opt.MapFrom(src => src.WorkflowStage != null ? src.WorkflowStage.Name : null))
            .ForMember(dest => dest.ResponsibleOrgUnitName, opt => opt.MapFrom(src => src.ResponsibleOrgUnit != null ? src.ResponsibleOrgUnit.Name : null))
            .ForMember(dest => dest.ProposedInitiativeTypeName, opt => opt.MapFrom(src => src.ProposedInitiativeType != null ? src.ProposedInitiativeType.Name : null));
            
        CreateMap<OpportunityRequest, Opportunity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EntityStatus.Draft))
            .ForMember(dest => dest.FundingPartners, opt => opt.Ignore())
            .ForMember(dest => dest.ClientPartners, opt => opt.Ignore())
            .ForMember(dest => dest.Stakeholders, opt => opt.Ignore())
            .ForMember(dest => dest.Deliverables, opt => opt.Ignore())
            .ForMember(dest => dest.Countries, opt => opt.Ignore());
            
        CreateMap<UpdateOpportunityRequest, Opportunity>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        
        // OpportunityFundingPartner mappings
        CreateMap<OpportunityFundingPartner, OpportunityFundingPartnerModel>()
            .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.Name : null))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Code : null));
            
        CreateMap<OpportunityFundingPartnerRequest, OpportunityFundingPartner>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());
        
        // OpportunityClientPartner mappings
        CreateMap<OpportunityClientPartner, OpportunityClientPartnerModel>()
            .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.Name : null));
            
        CreateMap<OpportunityClientPartnerRequest, OpportunityClientPartner>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());
        
        // OpportunityStakeholder mappings
        CreateMap<OpportunityStakeholder, OpportunityStakeholderModel>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : null))
            .ForMember(dest => dest.EntityRoleName, opt => opt.MapFrom(src => src.EntityRole != null ? src.EntityRole.Name : null));
            
        CreateMap<OpportunityStakeholderRequest, OpportunityStakeholder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());
        
        // OpportunityDeliverable mappings
        CreateMap<OpportunityDeliverable, OpportunityDeliverableModel>();
        CreateMap<OpportunityDeliverableRequest, OpportunityDeliverable>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());
        
        // OpportunityCountry mappings
        CreateMap<OpportunityCountry, OpportunityCountryModel>()
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null));
            
        CreateMap<OpportunityCountryRequest, OpportunityCountry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());
    }
}


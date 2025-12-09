using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.SDG;

namespace UNOPS.PAO.Business.Mapping;

public class OpportunityMappingProfile : Profile
{
    public OpportunityMappingProfile()
    {
        // =================================================================
        // Opportunity mappings
        // =================================================================
        CreateMap<Opportunity, OpportunityModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.WorkflowStageName, opt => opt.MapFrom(src => src.WorkflowStage != null ? src.WorkflowStage.Name : null))
            .ForMember(dest => dest.ResponsibleOrgUnitName, opt => opt.MapFrom(src => src.ResponsibleOrgUnit != null ? src.ResponsibleOrgUnit.Name : null))
            .ForMember(dest => dest.ResponsibleOrgUnit, opt => opt.MapFrom(src => src.ResponsibleOrgUnit))
            .ForMember(dest => dest.ProposedInitiativeTypeName, opt => opt.MapFrom(src => src.ProposedInitiativeType != null ? src.ProposedInitiativeType.Name : null))
            .ForMember(dest => dest.FundingPartners, opt => opt.MapFrom(src => src.FundingPartners))
            .ForMember(dest => dest.ClientPartners, opt => opt.MapFrom(src => src.ClientPartners))
            .ForMember(dest => dest.Stakeholders, opt => opt.MapFrom(src => src.Stakeholders))
            .ForMember(dest => dest.ExternalStakeholders, opt => opt.MapFrom(src => src.ExternalStakeholders))
            .ForMember(dest => dest.Deliverables, opt => opt.MapFrom(src => src.Deliverables))
            .ForMember(dest => dest.Countries, opt => opt.MapFrom(src => src.Countries))
            .ForMember(dest => dest.SDGs, opt => opt.MapFrom(src => src.SDGs))
            .ForMember(dest => dest.UNCFOutcomes, opt => opt.MapFrom(src => src.UNCFOutcomes))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => 
                src.CreatedByUser != null && src.CreatedByUser.UserProfile != null 
                    ? src.CreatedByUser.UserProfile.Name 
                    : (src.CreatedByUser != null ? src.CreatedByUser.Email : null)))
            .ForMember(dest => dest.LastModifiedByName, opt => opt.MapFrom(src => 
                src.LastModifiedByUser != null && src.LastModifiedByUser.UserProfile != null 
                    ? src.LastModifiedByUser.UserProfile.Name 
                    : (src.LastModifiedByUser != null ? src.LastModifiedByUser.Email : null)));
            
        CreateMap<OpportunityRequest, Opportunity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EntityStatus.Draft))
            .ForMember(dest => dest.FundingPartners, opt => opt.Ignore())
            .ForMember(dest => dest.ClientPartners, opt => opt.Ignore())
            .ForMember(dest => dest.Stakeholders, opt => opt.Ignore())
            .ForMember(dest => dest.Deliverables, opt => opt.Ignore())
            .ForMember(dest => dest.Countries, opt => opt.Ignore())
            .ForMember(dest => dest.SDGs, opt => opt.Ignore())
            .ForMember(dest => dest.UNCFOutcomes, opt => opt.Ignore());
            
        CreateMap<UpdateOpportunityRequest, Opportunity>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        
        // =================================================================
        // OpportunityFundingPartner mappings
        // =================================================================
        CreateMap<OpportunityFundingPartner, OpportunityFundingPartnerModel>()
            .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.Name : null))
            .ForMember(dest => dest.PartnerLogoUrl, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.LogoUrl : null))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Code : "USD"))
            .ForMember(dest => dest.DocumentName, opt => opt.MapFrom(src => src.Document != null ? src.Document.Name : null))
            .ForMember(dest => dest.PartnerStatus, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.Status.ToString() : null))
            .ForMember(dest => dest.PartnerApprovalStatus, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.PartnerApprovalStatus.ToString() : null))
            .ForMember(dest => dest.DDApproval, opt => opt.MapFrom(src => src.Partner != null && src.Partner.DueDiligenceApproval.HasValue ? src.Partner.DueDiligenceApproval.Value.ToString() : null))
            .ForMember(dest => dest.DDApprovalDate, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.DueDiligenceApprovalDate : null))
            .ForMember(dest => dest.DDExpiryDate, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.DueDiligenceExpiryDate : null))
            .ForMember(dest => dest.DDStatus, opt => opt.MapFrom(src => src.Partner != null ? CalculateDDStatus(src.Partner) : null))
            .ForMember(dest => dest.DDExpiresBeforeOpportunityEnd, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                if (src.Partner == null || src.Partner.DueDiligenceExpiryDate == null) return false;
                var opportunity = context.Items.ContainsKey("Opportunity") ? context.Items["Opportunity"] as Opportunity : null;
                if (opportunity?.TargetDeliveryDate == null) return false;
                return src.Partner.DueDiligenceExpiryDate < opportunity.TargetDeliveryDate;
            }))
            // Map both Amount and FundedAmount for backwards compatibility
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.FundedAmount, opt => opt.MapFrom(src => src.Amount)) // Use Amount for FundedAmount
            // Exchange rate and USD conversion fields
            .ForMember(dest => dest.PartnerPreferredCurrency, opt => opt.Ignore()) // Partner doesn't have PreferredCurrency yet
            .ForMember(dest => dest.AmountUSD, opt => opt.MapFrom(src => src.AmountUSD))
            .ForMember(dest => dest.ExchangeRate, opt => opt.MapFrom(src => src.ExchangeRate))
            .ForMember(dest => dest.ExchangeRateDate, opt => opt.MapFrom(src => src.ExchangeRateDate))
            .ForMember(dest => dest.ExchangeRateDisplay, opt => opt.MapFrom(src => 
                src.ExchangeRate.HasValue && src.ExchangeRateDate.HasValue 
                    ? $"{src.ExchangeRate.Value:F4} on {src.ExchangeRateDate.Value:MMM dd, yyyy}" 
                    : null));
            
        CreateMap<OpportunityFundingPartnerRequest, OpportunityFundingPartner>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());
        
        // =================================================================
        // OpportunityClientPartner mappings
        // =================================================================
        CreateMap<OpportunityClientPartner, OpportunityClientPartnerModel>()
            .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.Name : null))
            .ForMember(dest => dest.PartnerLogoUrl, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.LogoUrl : null))
            .ForMember(dest => dest.DocumentName, opt => opt.MapFrom(src => src.Document != null ? src.Document.Name : null))
            .ForMember(dest => dest.PartnerStatus, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.Status.ToString() : null))
            .ForMember(dest => dest.PartnerApprovalStatus, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.PartnerApprovalStatus.ToString() : null))
            .ForMember(dest => dest.DDApproval, opt => opt.MapFrom(src => src.Partner != null && src.Partner.DueDiligenceApproval.HasValue ? src.Partner.DueDiligenceApproval.Value.ToString() : null))
            .ForMember(dest => dest.DDApprovalDate, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.DueDiligenceApprovalDate : null))
            .ForMember(dest => dest.DDExpiryDate, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.DueDiligenceExpiryDate : null))
            .ForMember(dest => dest.DDStatus, opt => opt.MapFrom(src => src.Partner != null ? CalculateDDStatus(src.Partner) : null))
            .ForMember(dest => dest.DDExpiresBeforeOpportunityEnd, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                if (src.Partner == null || src.Partner.DueDiligenceExpiryDate == null) return false;
                var opportunity = context.Items.ContainsKey("Opportunity") ? context.Items["Opportunity"] as Opportunity : null;
                if (opportunity?.TargetDeliveryDate == null) return false;
                return src.Partner.DueDiligenceExpiryDate < opportunity.TargetDeliveryDate;
            }));
            
        CreateMap<OpportunityClientPartnerRequest, OpportunityClientPartner>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());
        
        // =================================================================
        // OpportunityStakeholder mappings
        // =================================================================
        CreateMap<OpportunityStakeholder, OpportunityStakeholderModel>()
            .ForMember(dest => dest.EntityRoleName, opt => opt.MapFrom(src => src.EntityRole != null ? src.EntityRole.Name : null))
            .ForMember(dest => dest.StakeholderType, opt => opt.MapFrom(src => src.IsInternal ? "Internal" : "External"))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null && src.User.UserProfile != null ? src.User.UserProfile.Name : (src.User != null ? src.User.Email : null)))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
            .ForMember(dest => dest.OrganizationHierarchyName, opt => opt.MapFrom(src => src.OrganizationHierarchy != null ? src.OrganizationHierarchy.Name : null))
            .ForMember(dest => dest.IsAutoPopulated, opt => opt.MapFrom(src => src.OrganizationHierarchyId.HasValue));
            
        CreateMap<OpportunityStakeholderRequest, OpportunityStakeholder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());
        
        // =================================================================
        // OpportunityExternalStakeholder mappings
        // =================================================================
        CreateMap<OpportunityExternalStakeholder, OpportunityExternalStakeholderModel>()
            .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => src.Contact != null ? src.Contact.Name : null))
            .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.Contact != null ? src.Contact.Email : null))
            .ForMember(dest => dest.ContactOrganization, opt => opt.MapFrom(src => src.Contact != null && src.Contact.Partner != null ? src.Contact.Partner.Name : null));
        
        CreateMap<OpportunityExternalStakeholderRequest, OpportunityExternalStakeholder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());
        
        // =================================================================
        // OpportunityDeliverable mappings
        // =================================================================
        CreateMap<OpportunityDeliverable, OpportunityDeliverableModel>()
            .ForMember(dest => dest.OutputName, opt => opt.MapFrom(src => src.Output != null ? src.Output.Name : null))
            .ForMember(dest => dest.Level0, opt => opt.MapFrom(src => src.Output != null ? src.Output.Level0 : null))
            .ForMember(dest => dest.Level1, opt => opt.MapFrom(src => src.Output != null ? src.Output.Level1 : null))
            .ForMember(dest => dest.Level2, opt => opt.MapFrom(src => src.Output != null ? src.Output.Level2 : null))
            .ForMember(dest => dest.Level3, opt => opt.MapFrom(src => src.Output != null ? src.Output.Level3 : null))
            .ForMember(dest => dest.Level4, opt => opt.MapFrom(src => src.Output != null ? src.Output.Level4 : null))
            .ForMember(dest => dest.ServiceLine, opt => opt.MapFrom(src => src.Output != null ? src.Output.ServiceLine : null));
        
        CreateMap<OpportunityDeliverableRequest, OpportunityDeliverable>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());

        // =================================================================
        // OpportunityCountry mappings
        // =================================================================
        CreateMap<OpportunityCountry, OpportunityCountryModel>()
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country));
            
        CreateMap<OpportunityCountryRequest, OpportunityCountry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore())
            .ForMember(dest => dest.Opportunity, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.ContextWarning, opt => opt.Ignore())
            .ForMember(dest => dest.RiskScore, opt => opt.Ignore());
        
        // =================================================================
        // OpportunitySDG mappings
        // =================================================================
        CreateMap<OpportunitySDG, OpportunitySDGModel>()
            .ForMember(dest => dest.SDGDatabaseId, opt => opt.MapFrom(src => src.SDGId))
            .ForMember(dest => dest.SDGId, opt => opt.MapFrom(src => 
                src.SDG != null ? src.SDG.SDGId : null))
            .ForMember(dest => dest.SDGNumber, opt => opt.MapFrom(src => 
                src.SDG != null ? src.SDG.SDGNumber : null))
            .ForMember(dest => dest.SDGName, opt => opt.MapFrom(src => src.SDG != null ? src.SDG.SDGDescription : null))
            .ForMember(dest => dest.SDGLogoUrl, opt => opt.MapFrom(src => 
                src.SDG != null ? $"https://sdgs.un.org/sites/default/files/goals/E_SDG_Icons-{src.SDG.SDGNumber.ToString().PadLeft(2, '0')}.jpg" : null))
            .ForMember(dest => dest.Targets, opt => opt.MapFrom(src => src.Targets));
            
        CreateMap<OpportunitySDGRequest, OpportunitySDG>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore());
        
        // =================================================================
        // OpportunitySDGTarget mappings
        // =================================================================
        CreateMap<OpportunitySDGTarget, OpportunitySDGTargetModel>()
            .ForMember(dest => dest.SDGTargetDatabaseId, opt => opt.MapFrom(src => src.SDGTargetId))
            .ForMember(dest => dest.SDGTargetId, opt => opt.MapFrom(src => 
                src.SDGTarget != null ? src.SDGTarget.SDGTargetId : null))
            .ForMember(dest => dest.TargetDescription, opt => opt.MapFrom(src => 
                src.SDGTarget != null ? src.SDGTarget.TargetDescription : null))
            .ForMember(dest => dest.TargetType, opt => opt.MapFrom(src => 
                src.SDGTarget != null ? src.SDGTarget.TargetType : null))
            .ForMember(dest => dest.Indicators, opt => opt.MapFrom(src => src.Indicators));
            
        CreateMap<OpportunitySDGTargetRequest, OpportunitySDGTarget>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore())
            .ForMember(dest => dest.SDGTargetId, opt => opt.MapFrom(src => src.SDGTargetDatabaseId));
        
        // =================================================================
        // OpportunitySDGIndicator mappings
        // =================================================================
        CreateMap<OpportunitySDGIndicator, OpportunitySDGIndicatorModel>()
            .ForMember(dest => dest.SDGIndicatorDatabaseId, opt => opt.MapFrom(src => src.SDGIndicatorId))
            .ForMember(dest => dest.SDGIndicatorId, opt => opt.MapFrom(src => 
                src.SDGIndicator != null ? src.SDGIndicator.SDGIndicatorId : null))
            .ForMember(dest => dest.SDGIndicatorLongDescription, opt => opt.MapFrom(src => 
                src.SDGIndicator != null ? src.SDGIndicator.SDGIndicatorLongDescription : null));
            
        CreateMap<OpportunitySDGIndicatorRequest, OpportunitySDGIndicator>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OpportunityId, opt => opt.Ignore())
            .ForMember(dest => dest.SDGIndicatorId, opt => opt.MapFrom(src => src.SDGIndicatorDatabaseId));
        
        // =================================================================
        // SDG reference data mappings
        // =================================================================
        CreateMap<SDGTarget, SDGTargetModel>();
        CreateMap<SDGIndicator, SDGIndicatorModel>();
        
        // =================================================================
        // OpportunityUNCFOutcome mappings
        // =================================================================
        CreateMap<OpportunityUNCFOutcome, OpportunityUNCFOutcomeModel>()
            .ForMember(dest => dest.UNCFOutcomeId, opt => opt.MapFrom(src => src.UNCFOutcomeId))
            .ForMember(dest => dest.UNCFOutcomeExternalId, opt => opt.MapFrom(src => 
                src.UNCFOutcome != null ? src.UNCFOutcome.UNCFOutcomeId : null))
            .ForMember(dest => dest.UNCFOutcomeName, opt => opt.MapFrom(src => 
                src.UNCFOutcome != null ? src.UNCFOutcome.Name : null))
            .ForMember(dest => dest.VersionNo, opt => opt.MapFrom(src => 
                src.UNCFOutcome != null ? src.UNCFOutcome.UNCooperationFrameworkVersionNo : null))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => 
                src.UNCFOutcome != null ? src.UNCFOutcome.Country : null))
            .ForMember(dest => dest.Indicators, opt => opt.MapFrom(src => src.Indicators));
        
        // =================================================================
        // OpportunityUNCFIndicator mappings
        // =================================================================
        CreateMap<OpportunityUNCFIndicator, OpportunityUNCFIndicatorModel>()
            .ForMember(dest => dest.UNCFIndicatorId, opt => opt.MapFrom(src => src.UNCFIndicatorId))
            .ForMember(dest => dest.UNCFIndicatorExternalId, opt => opt.MapFrom(src => 
                src.UNCFIndicator != null ? src.UNCFIndicator.UNCFIndicatorId : null))
            .ForMember(dest => dest.UNCFIndicatorName, opt => opt.MapFrom(src => 
                src.UNCFIndicator != null ? src.UNCFIndicator.Name : null));
        
        // =================================================================
        // UNCF reference data mappings
        // =================================================================
        CreateMap<UNCFOutcome, Models.UNCF.UNCFOutcomeModel>()
            .ForMember(dest => dest.UNCFOutcomeExternalId, opt => opt.MapFrom(src => src.UNCFOutcomeId))
            .ForMember(dest => dest.VersionNo, opt => opt.MapFrom(src => src.UNCooperationFrameworkVersionNo));
        CreateMap<UNCFIndicator, Models.UNCF.UNCFIndicatorModel>()
            .ForMember(dest => dest.UNCFIndicatorExternalId, opt => opt.MapFrom(src => src.UNCFIndicatorId))
            .ForMember(dest => dest.VersionNo, opt => opt.MapFrom(src => src.UNCooperationFrameworkVersionNo));
    }
    
    /// <summary>
    /// Calculate Due Diligence status based on partner DD information
    /// </summary>
    private static string CalculateDDStatus(Partner partner)
    {
        if (partner.DueDiligenceRequired == null || partner.DueDiligenceRequired == Domain.Enums.DueDiligenceRequired.NotRequired)
            return "Not Required";
            
        if (partner.DueDiligenceApproval == null || partner.DueDiligenceApproval == Domain.Enums.DueDiligenceApproval.NotApproved)
            return "Pending";
            
        if (partner.DueDiligenceExpiryDate == null)
            return "Approved";
            
        var now = DateTime.UtcNow;
        if (partner.DueDiligenceExpiryDate < now)
            return "Expired";
            
        if (partner.DueDiligenceExpiryDate <= now.AddMonths(6))
            return "Expiring Soon";
            
        return "Valid";
    }
}


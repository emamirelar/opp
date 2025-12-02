using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSBusiness.Repositories;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSOpportunityManager : BaseUNOPSManager, IOpportunityManager
{
    private readonly IMapper mapper;
    private readonly AppDbContext context;
    private readonly UNOPSAppDbContext uNOPSAppDbContext;
    private readonly BaseRepository<Opportunity> opportunityRepository;
    private readonly IServiceProvider _serviceProvider;

    public UNOPSOpportunityManager(
        IMapper mapper,
        AppDbContext context,
        IConfiguration configuration,
        IPermissionService permissionService = null,
        IHttpContextAccessor httpContextAccessor = null,
        IServiceProvider serviceProvider = null)
        : base(mapper, context as UNOPSAppDbContext, configuration, null, "Opportunity", permissionService, httpContextAccessor)
    {
        this.mapper = mapper;
        this.context = context;
        this.uNOPSAppDbContext = context as UNOPSAppDbContext;
        this._serviceProvider = serviceProvider;
        this.opportunityRepository = new BaseRepository<Opportunity>(this.uNOPSAppDbContext, configuration, serviceProvider);
    }

    public async Task<OpportunityModel> CreateOpportunityAsync(OpportunityRequest model)
    {
        var entity = mapper.Map<Opportunity>(model);

        // Set default workflow stage to 1 if not provided
        if (entity.WorkflowStageId == null || entity.WorkflowStageId == 0)
        {
            entity.WorkflowStageId = 1;
        }

        // Handle child entities
        if (model.FundingPartners != null && model.FundingPartners.Any())
        {
            // Get a valid currency ID (preferably USD, or the first available)
            var defaultCurrencyId = uNOPSAppDbContext.Currencies
                .Where(c => c.Code == "USD")
                .Select(c => c.Id)
                .FirstOrDefault();
            
            if (defaultCurrencyId == 0)
            {
                // Fallback to first available currency
                defaultCurrencyId = uNOPSAppDbContext.Currencies
                    .Select(c => c.Id)
                    .FirstOrDefault();
            }

            entity.FundingPartners = model.FundingPartners
                .Select(fp => {
                    var mapped = mapper.Map<OpportunityFundingPartner>(fp);
                    // Set default currency if not provided
                    if (mapped.CurrencyId == 0)
                    {
                        mapped.CurrencyId = defaultCurrencyId;
                    }
                    return mapped;
                })
                .ToList();
        }

        if (model.ClientPartners != null && model.ClientPartners.Any())
        {
            entity.ClientPartners = model.ClientPartners
                .Select(cp => mapper.Map<OpportunityClientPartner>(cp))
                .ToList();
        }

        if (model.Stakeholders != null && model.Stakeholders.Any())
        {
            entity.Stakeholders = model.Stakeholders
                .Select(s => mapper.Map<OpportunityStakeholder>(s))
                .ToList();
        }

        if (model.Deliverables != null && model.Deliverables.Any())
        {
            entity.Deliverables = model.Deliverables
                .Select(d => mapper.Map<OpportunityDeliverable>(d))
                .ToList();
        }

        if (model.Countries != null && model.Countries.Any())
        {
            entity.Countries = model.Countries
                .Select(c => mapper.Map<OpportunityCountry>(c))
                .ToList();
        }

        if (model.SDGs != null && model.SDGs.Any())
        {
            entity.SDGs = model.SDGs
                .Select(s => mapper.Map<OpportunitySDG>(s))
                .ToList();
        }

        await opportunityRepository.AddAsync(entity);

        var opportunityModel = mapper.Map<OpportunityModel>(entity);
        
        // Compute statistics
        opportunityModel.Stats = ComputeOpportunityStats(entity);

        return opportunityModel;
    }

    public async Task<OpportunityModel?> GetOpportunityAsync(int id)
    {
        var entity = await context.Opportunities
            .Include(o => o.WorkflowStage)
            .Include(o => o.ResponsibleOrgUnit)
            .Include(o => o.ProposedInitiativeType)
            .Include(o => o.FundingPartners)
                .ThenInclude(fp => fp.Partner)
            .Include(o => o.FundingPartners)
                .ThenInclude(fp => fp.Currency)
            .Include(o => o.ClientPartners)
                .ThenInclude(cp => cp.Partner)
            .Include(o => o.Stakeholders)
                .ThenInclude(s => s.EntityRole)
            .Include(o => o.Stakeholders)
                .ThenInclude(s => s.User)
                    .ThenInclude(u => u!.UserProfile)
            .Include(o => o.ExternalStakeholders)
                .ThenInclude(es => es.Contact)
                    .ThenInclude(c => c!.Partner)
            .Include(o => o.Deliverables)
                .ThenInclude(d => d.Output)
            .Include(o => o.Countries)
                .ThenInclude(c => c.Country)
            .Include(o => o.SDGs)
                .ThenInclude(s => s.SDG)
            .Include(o => o.SDGs)
                .ThenInclude(s => s.Targets)
                    .ThenInclude(t => t.SDGTarget)
            .Include(o => o.SDGs)
                .ThenInclude(s => s.Targets)
                    .ThenInclude(t => t.Indicators)
                        .ThenInclude(i => i.SDGIndicator)
            .Include(o => o.UNCFOutcomes)
                .ThenInclude(uo => uo.UNCFOutcome)
            .Include(o => o.UNCFOutcomes)
                .ThenInclude(uo => uo.OpportunityCountry)
                    .ThenInclude(oc => oc.Country)
            .Include(o => o.UNCFOutcomes)
                .ThenInclude(uo => uo.Indicators)
                    .ThenInclude(ui => ui.UNCFIndicator)
            .Include(o => o.UNOPSMissions)
                .ThenInclude(om => om.UNOPSMission)
            .AsSplitQuery() // Split into multiple queries to avoid Cartesian explosion
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

        if (entity == null)
        {
            return null;
        }

        var model = mapper.Map<OpportunityModel>(entity, opt => opt.Items["Opportunity"] = entity);
        
        // Populate associated documents and DD fields for funding partners
        if (model.FundingPartners != null && model.FundingPartners.Any())
        {
            // Get opportunity country IDs for agreement matching
            var opportunityCountryIds = entity.Countries?.Select(c => c.CountryId).ToList() ?? new List<int>();
            
            foreach (var fundingPartner in model.FundingPartners)
            {
                fundingPartner.AssociatedDocuments = await GetDocumentsForPartner(
                    id, 
                    fundingPartner.PartnerId, 
                    isFundingPartner: true
                );
                
                // Populate DD fields from the entity's Partner navigation property
                var fundingPartnerEntity = entity.FundingPartners?
                    .FirstOrDefault(fp => fp.Id == fundingPartner.Id);
                    
                if (fundingPartnerEntity?.Partner != null)
                {
                    var partner = fundingPartnerEntity.Partner;
                    
                    // DD Approval
                    fundingPartner.DDApproval = partner.DueDiligenceApproval?.ToString();
                    fundingPartner.DDApprovalDate = partner.DueDiligenceApprovalDate;
                    fundingPartner.DDExpiryDate = partner.DueDiligenceExpiryDate;
                    
                    // DD Status calculation
                    fundingPartner.DDStatus = CalculateDDStatus(partner);
                    
                    // DD Expires before opportunity end
                    if (partner.DueDiligenceExpiryDate != null && entity.TargetDeliveryDate != null)
                    {
                        fundingPartner.DDExpiresBeforeOpportunityEnd = 
                            partner.DueDiligenceExpiryDate < entity.TargetDeliveryDate;
                    }
                    
                    // Exchange Rate Display
                    if (fundingPartnerEntity.ExchangeRate != null && fundingPartnerEntity.ExchangeRateDate != null)
                    {
                        fundingPartner.ExchangeRateDisplay = 
                            $"{fundingPartnerEntity.ExchangeRate:F4} on {fundingPartnerEntity.ExchangeRateDate:MMM dd, yyyy}";
                    }
                    
                    // Load partner agreements
                    fundingPartner.AvailableAgreements = await LoadPartnerAgreementsAsync(
                        fundingPartner.PartnerId,
                        entity.CreatedDate, // Use created date as start
                        entity.TargetDeliveryDate,
                        opportunityCountryIds
                    );
                }
            }
        }
        
        // Populate associated documents and DD fields for client partners
        if (model.ClientPartners != null && model.ClientPartners.Any())
        {
            // Get opportunity country IDs for agreement matching
            var opportunityCountryIds = entity.Countries?.Select(c => c.CountryId).ToList() ?? new List<int>();
            
            foreach (var clientPartner in model.ClientPartners)
            {
                clientPartner.AssociatedDocuments = await GetDocumentsForPartner(
                    id, 
                    clientPartner.PartnerId, 
                    isFundingPartner: false
                );
                
                // Populate DD fields from the entity's Partner navigation property
                var clientPartnerEntity = entity.ClientPartners?
                    .FirstOrDefault(cp => cp.Id == clientPartner.Id);
                    
                if (clientPartnerEntity?.Partner != null)
                {
                    var partner = clientPartnerEntity.Partner;
                    
                    // DD Approval
                    clientPartner.DDApproval = partner.DueDiligenceApproval?.ToString();
                    clientPartner.DDApprovalDate = partner.DueDiligenceApprovalDate;
                    clientPartner.DDExpiryDate = partner.DueDiligenceExpiryDate;
                    
                    // DD Status calculation
                    clientPartner.DDStatus = CalculateDDStatus(partner);
                    
                    // DD Expires before opportunity end
                    if (partner.DueDiligenceExpiryDate != null && entity.TargetDeliveryDate != null)
                    {
                        clientPartner.DDExpiresBeforeOpportunityEnd = 
                            partner.DueDiligenceExpiryDate < entity.TargetDeliveryDate;
                    }
                    
                    // Load partner agreements
                    clientPartner.AvailableAgreements = await LoadPartnerAgreementsAsync(
                        clientPartner.PartnerId,
                        entity.CreatedDate, // Use created date as start
                        entity.TargetDeliveryDate,
                        opportunityCountryIds
                    );
                }
            }
        }
        
        // Enrich country models with organization unit hierarchy and UNCF status
        if (model.Countries != null && model.Countries.Any())
        {
            await EnrichCountriesWithOrgUnitHierarchyAsync(model.Countries);
            await EnrichCountriesWithActiveUNCFAsync(model.Countries);
            
            // Check which countries have Humanitarian, Peace & Security Framework
            await EnrichCountriesWithHumanitarianFrameworkAsync(model.Countries);
            
            // Check which countries have NDC (Nationally Determined Contributions)
            await EnrichCountriesWithNdcAsync(model.Countries);
            
            // Check which countries have NAP (National Adaptation Plan)
            await EnrichCountriesWithNapAsync(model.Countries);
            
            // Check which countries have Organization Unit Strategy (traverse hierarchy)
            await EnrichCountriesWithOrgUnitStrategyAsync(model.Countries);
        }
        
        // Enrich UNCF data with activity status and newer version checks
        EnrichUNCFDataWithActivityStatus(model);

        // Compute statistics
        model.Stats = ComputeOpportunityStats(entity);
        
        // Check if this is a new value range for the responsible org unit
        if (model.ResponsibleOrgUnitId.HasValue && model.Stats?.TotalFundingUSD != null && model.Stats.TotalFundingUSD > 0)
        {
            try
            {
                // Find the historical maximum budget for this org unit (excluding current opportunity)
                var historicalMax = await context.Opportunities
                    .Where(o => o.ResponsibleOrgUnitId == model.ResponsibleOrgUnitId
                             && o.Id != id
                             && !o.IsDeleted)
                    .SelectMany(o => o.FundingPartners)
                    .GroupBy(fp => fp.OpportunityId)
                    .Select(g => new { 
                        OpportunityId = g.Key, 
                        Total = g.Sum(fp => fp.AmountUSD ?? 0) 
                    })
                    .OrderByDescending(x => x.Total)
                    .FirstOrDefaultAsync();
                
                model.OrgUnitHistoricalMaxValue = historicalMax?.Total ?? 0;
                model.IsNewValueRangeForOrgUnit = model.Stats.TotalFundingUSD > (historicalMax?.Total ?? 0);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the entire request
                // Logger not available in this context - silently continue
                model.IsNewValueRangeForOrgUnit = null;
                model.OrgUnitHistoricalMaxValue = null;
            }
        }

        return model;
    }
    
    /// <summary>
    /// Gets an opportunity by ID with user-specific permissions
    /// Stakeholders (team members) on the opportunity can update it even if they don't have global update permission
    /// </summary>
    /// <param name="user">Current user context</param>
    /// <param name="id">Opportunity ID</param>
    /// <returns>Opportunity model with permissions, or null if not found</returns>
    public async Task<OpportunityModel?> GetOpportunityAsync(ClaimsPrincipal user, int id)
    {
        // Get the base opportunity model
        var model = await GetOpportunityAsync(id);
        if (model == null)
        {
            return null;
        }
        
        // Get entity for permission checking
        var entity = await context.Opportunities
            .Include(o => o.Stakeholders)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        
        if (entity == null)
        {
            return null;
        }
        
        // Check if user is a stakeholder on this opportunity
        var isStakeholder = await IsUserStakeholderOnOpportunityAsync(user, id);
        
        // Add permissions with stakeholder check
        model = await MapEntityToModelWithPermissionsAsync(model, user, entity);
        
        // If user is a stakeholder, they should be able to update the opportunity
        // even if they don't have global update permission
        if (isStakeholder && model.Permissions != null)
        {
            model.Permissions.CanUpdate = true;
            model.Permissions.Notes = "Stakeholder on this opportunity";
        }
        
        return model;
    }
    
    /// <summary>
    /// Checks if the current user is a stakeholder (team member) on the given opportunity
    /// </summary>
    /// <param name="user">Current user context</param>
    /// <param name="opportunityId">Opportunity ID</param>
    /// <returns>True if user is a stakeholder, false otherwise</returns>
    private async Task<bool> IsUserStakeholderOnOpportunityAsync(ClaimsPrincipal user, int opportunityId)
    {
        if (user == null)
        {
            return false;
        }
        
        // Get user ID from claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return false;
        }
        
        // Check if this user is an internal stakeholder on the opportunity
        var isStakeholder = await context.OpportunityStakeholders
            .AnyAsync(s => s.OpportunityId == opportunityId 
                        && s.UserId == userId 
                        && s.IsInternal);
        
        return isStakeholder;
    }
    
    /// <summary>
    /// Get all documents associated with a specific partner for an opportunity
    /// </summary>
    /// <param name="opportunityId">Opportunity ID</param>
    /// <param name="partnerId">Partner ID</param>
    /// <param name="isFundingPartner">True for funding partners, false for client partners</param>
    /// <returns>List of document details</returns>
    private async Task<List<UNOPS.PAO.Models.Documents.DocumentDetailModel>> GetDocumentsForPartner(
        int opportunityId, 
        int partnerId, 
        bool isFundingPartner)
    {
        var documents = new List<UNOPS.PAO.Models.Documents.DocumentDetailModel>();
        
        if (isFundingPartner)
        {
            // Get documents from OpportunityFundingPartner table
            var fundingPartnerDocs = await context.OpportunityFundingPartners
                .Where(fp => fp.OpportunityId == opportunityId && fp.PartnerId == partnerId && fp.DocumentId != null)
                .Include(fp => fp.Document)
                .Select(fp => fp.Document)
                .Where(d => d != null && !d.IsDeleted)
                .Distinct()
                .ToListAsync();
            
            foreach (var doc in fundingPartnerDocs)
            {
                if (doc != null)
                {
                    documents.Add(new UNOPS.PAO.Models.Documents.DocumentDetailModel
                    {
                        Id = doc.Id,
                        Name = doc.Name,
                        Type = doc.Type,
                        StoragePath = doc.StoragePath,
                        Link = doc.Link
                    });
                }
            }
        }
        else
        {
            // Get documents from OpportunityClientPartner table
            var clientPartnerDocs = await context.OpportunityClientPartners
                .Where(cp => cp.OpportunityId == opportunityId && cp.PartnerId == partnerId && cp.DocumentId != null)
                .Include(cp => cp.Document)
                .Select(cp => cp.Document)
                .Where(d => d != null && !d.IsDeleted)
                .Distinct()
                .ToListAsync();
            
            foreach (var doc in clientPartnerDocs)
            {
                if (doc != null)
                {
                    documents.Add(new UNOPS.PAO.Models.Documents.DocumentDetailModel
                    {
                        Id = doc.Id,
                        Name = doc.Name,
                        Type = doc.Type,
                        StoragePath = doc.StoragePath,
                        Link = doc.Link
                    });
                }
            }
        }
        
        return documents;
    }
    
    /// <summary>
    /// Enriches country models with their organization unit hierarchy chains
    /// </summary>
    private async Task EnrichCountriesWithOrgUnitHierarchyAsync(IEnumerable<OpportunityCountryModel> countries)
    {
        foreach (var country in countries)
        {
            if (country.Country != null)
            {
                var hierarchy = await GetOrganizationUnitHierarchyForCountryAsync(country.Country.Id);
                country.Country.OrganizationUnitHierarchy = hierarchy;
            }
        }
    }
    
    /// <summary>
    /// Gets the organization unit hierarchy chain for a given country
    /// Returns the chain from root to the country's org unit (e.g., OPS → APR → B5101)
    /// </summary>
    private async Task<List<UNOPS.PAO.Models.Locations.OrganizationUnitHierarchyNode>?> GetOrganizationUnitHierarchyForCountryAsync(int countryId)
    {
        // Find the organization unit relationship for this country
        var orgUnitRelationship = await context.Set<OrganizationUnitRelationship>()
            .Include(r => r.OrganizationHierarchy)
                .ThenInclude(oh => oh!.Parent)
            .FirstOrDefaultAsync(r => 
                r.EntityType == "Country" && 
                r.EntityId == countryId && 
                !r.IsDeleted);
        
        if (orgUnitRelationship?.OrganizationHierarchy == null)
        {
            return null;
        }
        
        // Build the hierarchy chain from this org unit to the root
        var hierarchyChain = new List<UNOPS.PAO.Models.Locations.OrganizationUnitHierarchyNode>();
        var currentOrgUnit = orgUnitRelationship.OrganizationHierarchy;
        
        while (currentOrgUnit != null)
        {
            hierarchyChain.Add(new UNOPS.PAO.Models.Locations.OrganizationUnitHierarchyNode
            {
                Id = currentOrgUnit.Id,
                Code = currentOrgUnit.Code,
                Name = currentOrgUnit.Name,
                Type = currentOrgUnit.Type.ToString(),
                Description = currentOrgUnit.Description,
                ParentId = currentOrgUnit.ParentId,
                Level = 0 // Will be set after reversing
            });
            
            // Load the parent if it exists
            if (currentOrgUnit.ParentId.HasValue)
            {
                currentOrgUnit = await context.Set<OrganizationHierarchy>()
                    .FirstOrDefaultAsync(oh => oh.Id == currentOrgUnit.ParentId.Value && !oh.IsDeleted);
            }
            else
            {
                currentOrgUnit = null;
            }
        }
        
        // Reverse the chain so it goes from root to leaf (e.g., OPS → APR → B5101)
        hierarchyChain.Reverse();
        
        // Set levels after reversing (root = 0, leaf = highest)
        for (int i = 0; i < hierarchyChain.Count; i++)
        {
            hierarchyChain[i].Level = i;
        }
        
        return hierarchyChain.Any() ? hierarchyChain : null;
    }
    
    /// <summary>
    /// Enriches country models with active UNCF status based on UNCFMetadatas table
    /// Checks if there's at least one active UNCF metadata record for each country
    /// </summary>
    private async Task EnrichCountriesWithActiveUNCFAsync(IEnumerable<OpportunityCountryModel> opportunityCountries)
    {
        // Get all country ISO2 codes from the opportunity countries
        var iso2Codes = opportunityCountries
            .Where(oc => oc.Country != null && !string.IsNullOrEmpty(oc.Country.Iso2Code))
            .Select(oc => oc.Country!.Iso2Code)
            .Distinct()
            .ToList();
        
        if (!iso2Codes.Any())
        {
            return;
        }
        
        // Check which countries have active UNCF metadata
        var countriesWithActiveUNCF = await context.UNCFMetadatas
            .Where(m => m.Status == EntityStatus.Active 
                && iso2Codes.Contains(m.Country!))
            .Select(m => m.Country!)
            .Distinct()
            .ToListAsync();
        
        var countriesWithActiveUNCFSet = new HashSet<string>(
            countriesWithActiveUNCF, 
            StringComparer.OrdinalIgnoreCase
        );
        
        // Set HasActiveUNCF flag for each country
        foreach (var oppCountry in opportunityCountries)
        {
            if (oppCountry.Country != null && !string.IsNullOrEmpty(oppCountry.Country.Iso2Code))
            {
                oppCountry.Country.HasActiveUNCF = 
                    countriesWithActiveUNCFSet.Contains(oppCountry.Country.Iso2Code);
            }
        }
    }
    
    /// <summary>
    /// Enriches country models with Humanitarian, Peace & Security Framework availability
    /// Sets HasHumanitarianFramework flag for each country that has an active framework
    /// </summary>
    private async Task EnrichCountriesWithHumanitarianFrameworkAsync(IEnumerable<OpportunityCountryModel> opportunityCountries)
    {
        // Get all country IDs from the opportunity countries
        var countryIds = opportunityCountries
            .Select(oc => oc.CountryId)
            .Distinct()
            .ToList();
        
        if (!countryIds.Any())
        {
            return;
        }
        
        // Check which countries have the Humanitarian_Peace_Security_Framework artifact
        var countriesWithFramework = await context.EntityArtifacts
            .Where(ea => 
                ea.EntityType == "Country" 
                && countryIds.Contains(ea.EntityId)
                && ea.ArtifactType!.ArtifactTypeCode == "Humanitarian_Peace_Security_Framework"
                && ea.Status == EntityStatus.Active
                && !ea.IsDeleted
                && (ea.ExpiryDate == null || ea.ExpiryDate > DateTime.UtcNow))
            .Select(ea => ea.EntityId)
            .Distinct()
            .ToListAsync();
        
        var countriesWithFrameworkSet = new HashSet<int>(countriesWithFramework);
        
        // Set HasHumanitarianFramework flag for each country
        foreach (var oppCountry in opportunityCountries)
        {
            oppCountry.HasHumanitarianFramework = countriesWithFrameworkSet.Contains(oppCountry.CountryId);
        }
    }
    
    /// <summary>
    /// Enriches country models with NDC (Nationally Determined Contributions) availability
    /// Sets HasNdc flag for each country that has active NDC
    /// </summary>
    private async Task EnrichCountriesWithNdcAsync(IEnumerable<OpportunityCountryModel> opportunityCountries)
    {
        var countryIds = opportunityCountries
            .Select(oc => oc.CountryId)
            .Distinct()
            .ToList();
        
        if (!countryIds.Any())
        {
            return;
        }
        
        // Check which countries have the NDC artifact
        var countriesWithNdc = await context.EntityArtifacts
            .Where(ea => 
                ea.EntityType == "Country" 
                && countryIds.Contains(ea.EntityId)
                && ea.ArtifactType!.ArtifactTypeCode == "NDC"
                && ea.Status == EntityStatus.Active
                && !ea.IsDeleted
                && (ea.ExpiryDate == null || ea.ExpiryDate > DateTime.UtcNow))
            .Select(ea => ea.EntityId)
            .Distinct()
            .ToListAsync();
        
        var countriesWithNdcSet = new HashSet<int>(countriesWithNdc);
        
        // Set HasNdc flag for each country
        foreach (var oppCountry in opportunityCountries)
        {
            oppCountry.HasNdc = countriesWithNdcSet.Contains(oppCountry.CountryId);
        }
    }
    
    /// <summary>
    /// Enriches country models with NAP (National Adaptation Plan) availability
    /// Sets HasNap flag for each country that has active NAP
    /// </summary>
    private async Task EnrichCountriesWithNapAsync(IEnumerable<OpportunityCountryModel> opportunityCountries)
    {
        var countryIds = opportunityCountries
            .Select(oc => oc.CountryId)
            .Distinct()
            .ToList();
        
        if (!countryIds.Any())
        {
            return;
        }
        
        // Check which countries have the NAP artifact
        var countriesWithNap = await context.EntityArtifacts
            .Where(ea => 
                ea.EntityType == "Country" 
                && countryIds.Contains(ea.EntityId)
                && ea.ArtifactType!.ArtifactTypeCode == "NAP"
                && ea.Status == EntityStatus.Active
                && !ea.IsDeleted
                && (ea.ExpiryDate == null || ea.ExpiryDate > DateTime.UtcNow))
            .Select(ea => ea.EntityId)
            .Distinct()
            .ToListAsync();
        
        var countriesWithNapSet = new HashSet<int>(countriesWithNap);
        
        // Set HasNap flag for each country
        foreach (var oppCountry in opportunityCountries)
        {
            oppCountry.HasNap = countriesWithNapSet.Contains(oppCountry.CountryId);
        }
    }
    
    /// <summary>
    /// Enriches country models with Organization Unit Strategy information
    /// Traverses up the org hierarchy to find the most local org unit with a Strategy artifact
    /// Sets HasOrgUnitStrategy flag, OrgUnitWithStrategyId, and OrgUnitWithStrategyName
    /// Also detects if a more local strategy is now available compared to the stored one
    /// </summary>
    private async Task EnrichCountriesWithOrgUnitStrategyAsync(IEnumerable<OpportunityCountryModel> opportunityCountries)
    {
        var countryIds = opportunityCountries
            .Select(oc => oc.CountryId)
            .Distinct()
            .ToList();
        
        if (!countryIds.Any())
        {
            return;
        }
        
        // Get all org unit relationships for these countries
        var countryOrgRelationships = await context.Set<OrganizationUnitRelationship>()
            .Where(r => 
                r.EntityType == "Country" 
                && countryIds.Contains(r.EntityId)
                && !r.IsDeleted)
            .Include(r => r.OrganizationHierarchy)
            .ToListAsync();
        
        // Get all org units with Strategy artifacts
        var orgUnitsWithStrategy = await context.EntityArtifacts
            .Where(ea => 
                ea.EntityType == "OrganizationHierarchy"
                && ea.ArtifactType!.ArtifactTypeCode == "Strategy"
                && ea.Status == EntityStatus.Active
                && !ea.IsDeleted)
            .Select(ea => ea.EntityId)
            .Distinct()
            .ToListAsync();
        
        var orgUnitsWithStrategySet = new HashSet<int>(orgUnitsWithStrategy);
        
        // Get all unique current (stored) org unit IDs to load their details
        var currentOrgUnitIds = opportunityCountries
            .Where(oc => oc.CurrentOrgUnitWithStrategyId.HasValue)
            .Select(oc => oc.CurrentOrgUnitWithStrategyId!.Value)
            .Distinct()
            .ToList();
        
        // Load current org units details
        var currentOrgUnits = currentOrgUnitIds.Any()
            ? await context.Set<OrganizationHierarchy>()
                .Where(o => currentOrgUnitIds.Contains(o.Id) && !o.IsDeleted)
                .ToListAsync()
            : new List<OrganizationHierarchy>();
        
        var currentOrgUnitsDict = currentOrgUnits.ToDictionary(o => o.Id);
        
        // For each country, find the most local org unit with a Strategy
        foreach (var oppCountry in opportunityCountries)
        {
            var countryOrgRelationship = countryOrgRelationships
                .FirstOrDefault(r => r.EntityId == oppCountry.CountryId);
            
            if (countryOrgRelationship?.OrganizationHierarchy != null)
            {
                // Traverse up the hierarchy to find a Strategy
                var orgUnitWithStrategy = await FindOrgUnitWithStrategyAsync(
                    countryOrgRelationship.OrganizationHierarchyId, 
                    orgUnitsWithStrategySet);
                
                if (orgUnitWithStrategy != null)
                {
                    oppCountry.HasOrgUnitStrategy = true;
                    oppCountry.OrgUnitWithStrategyId = orgUnitWithStrategy.Id;
                    oppCountry.OrgUnitWithStrategyName = orgUnitWithStrategy.Name;
                    oppCountry.OrgUnitWithStrategyCode = orgUnitWithStrategy.Code;
                    
                    // Check if current stored org unit is different (indicating a more local strategy is available)
                    if (oppCountry.CurrentOrgUnitWithStrategyId.HasValue)
                    {
                        // Populate current org unit details
                        if (currentOrgUnitsDict.TryGetValue(oppCountry.CurrentOrgUnitWithStrategyId.Value, out var currentOrgUnit))
                        {
                            oppCountry.CurrentOrgUnitWithStrategyName = currentOrgUnit.Name;
                            oppCountry.CurrentOrgUnitWithStrategyCode = currentOrgUnit.Code;
                        }
                        
                        // Check if the new org unit is different (more local)
                        if (oppCountry.CurrentOrgUnitWithStrategyId.Value != orgUnitWithStrategy.Id)
                        {
                            oppCountry.HasMoreLocalStrategyAvailable = true;
                        }
                    }
                }
                else
                {
                    oppCountry.HasOrgUnitStrategy = false;
                    oppCountry.OrgUnitWithStrategyId = null;
                    oppCountry.OrgUnitWithStrategyName = null;
                    oppCountry.OrgUnitWithStrategyCode = null;
                }
            }
            else
            {
                oppCountry.HasOrgUnitStrategy = false;
                oppCountry.OrgUnitWithStrategyId = null;
                oppCountry.OrgUnitWithStrategyName = null;
                oppCountry.OrgUnitWithStrategyCode = null;
            }
        }
    }
    
    /// <summary>
    /// Recursively traverses up the OrganizationHierarchy to find the most local org unit with a Strategy artifact
    /// </summary>
    /// <param name="orgUnitId">Starting org unit ID</param>
    /// <param name="orgUnitsWithStrategy">Set of org unit IDs that have Strategy artifacts</param>
    /// <returns>The most local OrganizationHierarchy with a Strategy, or null if none found</returns>
    private async Task<OrganizationHierarchy?> FindOrgUnitWithStrategyAsync(int orgUnitId, HashSet<int> orgUnitsWithStrategy)
    {
        // Check if current org unit has a Strategy
        if (orgUnitsWithStrategy.Contains(orgUnitId))
        {
            // Load and return this org unit
            var orgUnit = await context.Set<OrganizationHierarchy>()
                .FirstOrDefaultAsync(o => o.Id == orgUnitId && !o.IsDeleted);
            return orgUnit;
        }
        
        // If not, check parent
        var currentOrgUnit = await context.Set<OrganizationHierarchy>()
            .FirstOrDefaultAsync(o => o.Id == orgUnitId && !o.IsDeleted);
        
        if (currentOrgUnit?.ParentId != null)
        {
            // Recursively check parent
            return await FindOrgUnitWithStrategyAsync(currentOrgUnit.ParentId.Value, orgUnitsWithStrategy);
        }
        
        // No strategy found in hierarchy
        return null;
    }
    
    /// <summary>
    /// Computes the OrgUnitWithStrategyId for a list of countries
    /// Returns a dictionary mapping CountryId to OrgUnitId (the most local org unit with a Strategy)
    /// </summary>
    private async Task<Dictionary<int, int>> ComputeOrgUnitWithStrategyForCountriesAsync(List<int> countryIds)
    {
        var result = new Dictionary<int, int>();
        
        if (!countryIds.Any())
        {
            return result;
        }
        
        // Get all org unit relationships for these countries
        var countryOrgRelationships = await context.Set<OrganizationUnitRelationship>()
            .Where(r => 
                r.EntityType == "Country" 
                && countryIds.Contains(r.EntityId)
                && !r.IsDeleted)
            .ToListAsync();
        
        // Get all org units with Strategy artifacts
        var orgUnitsWithStrategy = await context.EntityArtifacts
            .Where(ea => 
                ea.EntityType == "OrganizationHierarchy"
                && ea.ArtifactType!.ArtifactTypeCode == "Strategy"
                && ea.Status == EntityStatus.Active
                && !ea.IsDeleted
                && (ea.ExpiryDate == null || ea.ExpiryDate > DateTime.UtcNow))
            .Select(ea => ea.EntityId)
            .Distinct()
            .ToListAsync();
        
        var orgUnitsWithStrategySet = new HashSet<int>(orgUnitsWithStrategy);
        
        // For each country, find the most local org unit with a Strategy
        foreach (var countryId in countryIds)
        {
            var countryOrgRelationship = countryOrgRelationships
                .FirstOrDefault(r => r.EntityId == countryId);
            
            if (countryOrgRelationship != null)
            {
                // Traverse up the hierarchy to find a Strategy
                var orgUnitWithStrategy = await FindOrgUnitWithStrategyAsync(
                    countryOrgRelationship.OrganizationHierarchyId, 
                    orgUnitsWithStrategySet);
                
                if (orgUnitWithStrategy != null)
                {
                    result[countryId] = orgUnitWithStrategy.Id;
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Enriches UNCF outcome and indicator models with activity status and newer version availability
    /// </summary>
    private void EnrichUNCFDataWithActivityStatus(OpportunityModel model)
    {
        if (model.UNCFOutcomes == null || !model.UNCFOutcomes.Any())
        {
            return;
        }
        
        var valuesRepo = new Business.Repositories.ValuesRepository(context);
        
        foreach (var uncfOutcome in model.UNCFOutcomes)
        {
            // Check if this outcome is currently active
            bool isOutcomeActive = valuesRepo.IsUNCFOutcomeActive(uncfOutcome.UNCFOutcomeId);
            uncfOutcome.IsInactive = !isOutcomeActive;
            
            // If inactive, check for newer versions
            if (uncfOutcome.IsInactive && 
                !string.IsNullOrEmpty(uncfOutcome.Country) &&
                uncfOutcome.VersionNo.HasValue)
            {
                uncfOutcome.HasNewerVersion = valuesRepo.HasNewerUNCFOutcomeVersion(
                    uncfOutcome.Country,
                    uncfOutcome.VersionNo.Value
                );
            }
            
            // Check indicators for this outcome
            if (uncfOutcome.Indicators != null && uncfOutcome.Indicators.Any())
            {
                foreach (var indicator in uncfOutcome.Indicators)
                {
                    // Check if this indicator is currently active
                    bool isIndicatorActive = valuesRepo.IsUNCFIndicatorActive(indicator.UNCFIndicatorId);
                    indicator.IsInactive = !isIndicatorActive;
                    
                    // If inactive, check for newer versions
                    if (indicator.IsInactive &&
                        !string.IsNullOrEmpty(uncfOutcome.Country) &&
                        uncfOutcome.VersionNo.HasValue)
                    {
                        indicator.HasNewerVersion = valuesRepo.HasNewerUNCFIndicatorVersion(
                            uncfOutcome.Country,
                            uncfOutcome.VersionNo.Value
                        );
                    }
                }
            }
        }
    }

    public async Task<IEnumerable<OpportunityModel>> GetAllOpportunitiesAsync()
    {
        var entities = await context.Opportunities
            .Include(o => o.WorkflowStage)
            .Include(o => o.ResponsibleOrgUnit)
            .Include(o => o.ProposedInitiativeType)
            .Where(o => !o.IsDeleted)
            .ToListAsync();

        return entities.Select(e => mapper.Map<OpportunityModel>(e));
    }

    public async Task<OpportunityModel?> UpdateOpportunityAsync(UpdateOpportunityRequest model)
    {
        var entity = await context.Opportunities
            .Include(o => o.FundingPartners)
            .Include(o => o.ClientPartners)
            .Include(o => o.Stakeholders)
            .Include(o => o.Deliverables)
            .Include(o => o.Countries)
            .Include(o => o.SDGs)
            .FirstOrDefaultAsync(o => o.Id == model.Id && !o.IsDeleted);

        if (entity == null)
        {
            return null;
        }

        // Update main entity properties
        mapper.Map(model, entity);

        // Update child collections (full replacement approach)
        if (model.FundingPartners != null)
        {
            context.Set<OpportunityFundingPartner>().RemoveRange(entity.FundingPartners);
            
            entity.FundingPartners = model.FundingPartners
                .Select(fp =>
                {
                    var mapped = mapper.Map<OpportunityFundingPartner>(fp);
                    mapped.OpportunityId = entity.Id;
                    return mapped;
                })
                .ToList();
        }

        if (model.ClientPartners != null)
        {
            context.Set<OpportunityClientPartner>().RemoveRange(entity.ClientPartners);
            entity.ClientPartners = model.ClientPartners
                .Select(cp =>
                {
                    var mapped = mapper.Map<OpportunityClientPartner>(cp);
                    mapped.OpportunityId = entity.Id;
                    return mapped;
                })
                .ToList();
        }

        if (model.Stakeholders != null)
        {
            context.Set<OpportunityStakeholder>().RemoveRange(entity.Stakeholders);
            entity.Stakeholders = model.Stakeholders
                .Select(s =>
                {
                    var mapped = mapper.Map<OpportunityStakeholder>(s);
                    mapped.OpportunityId = entity.Id;
                    return mapped;
                })
                .ToList();
        }

        if (model.Deliverables != null)
        {
            context.Set<OpportunityDeliverable>().RemoveRange(entity.Deliverables);
            entity.Deliverables = model.Deliverables
                .Select(d =>
                {
                    var mapped = mapper.Map<OpportunityDeliverable>(d);
                    mapped.OpportunityId = entity.Id;
                    return mapped;
                })
                .ToList();
        }

        if (model.Countries != null)
        {
            context.Set<OpportunityCountry>().RemoveRange(entity.Countries);
            entity.Countries = model.Countries
                .Select(c =>
                {
                    var mapped = mapper.Map<OpportunityCountry>(c);
                    mapped.OpportunityId = entity.Id;
                    return mapped;
                })
                .ToList();
        }

        if (model.SDGs != null)
        {
            context.Set<OpportunitySDG>().RemoveRange(entity.SDGs);
            entity.SDGs = model.SDGs
                .Select(s =>
                {
                    var mapped = mapper.Map<OpportunitySDG>(s);
                    mapped.OpportunityId = entity.Id;
                    return mapped;
                })
                .ToList();
        }

        await opportunityRepository.UpdateAsync(entity);

        // Reload with all includes for complete response
        return await GetOpportunityAsync(entity.Id);
    }

    public async Task<OpportunityModel> UpdateOverviewSectionAsync(int id, OverviewSectionRequest request)
    {
        var entity = await opportunityRepository.GetByIdAsync(id);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update Overview section fields
        if (request.Name != null)
        {
            entity.Name = request.Name;
        }

        if (request.Description != null)
        {
            entity.Description = request.Description;
        }

        await opportunityRepository.UpdateAsync(entity);

        // Reload with all includes for complete response
        return await GetOpportunityAsync(entity.Id);
    }

    public async Task<OpportunityModel> UpdateWhatSectionAsync(int id, WhatSectionRequest request)
    {
        var entity = await opportunityRepository.GetByIdAsync(id, new[]
        {
            nameof(Opportunity.Deliverables)
        });

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update WHAT section fields
        if (request.Name != null)
        {
            entity.Name = request.Name;
        }

        if (request.Description != null)
        {
            entity.Description = request.Description;
        }

        if (request.ResponsibleOrgUnitId.HasValue)
        {
            entity.ResponsibleOrgUnitId = request.ResponsibleOrgUnitId.Value;
        }

        if (request.ProposedInitiativeTypeId.HasValue)
        {
            entity.ProposedInitiativeTypeId = request.ProposedInitiativeTypeId.Value;
        }
        
        // Update delivery modality (always update if provided, including null to clear)
        if (request.DeliveryModality.HasValue)
        {
            entity.DeliveryModality = (DeliveryModality)request.DeliveryModality.Value;
        }

        // Update deliverables
        if (request.Deliverables != null)
        {
            // Remove existing deliverables
            if (entity.Deliverables != null && entity.Deliverables.Any())
            {
                context.Set<OpportunityDeliverable>().RemoveRange(entity.Deliverables);
            }

            // Add new deliverables
            entity.Deliverables = request.Deliverables
                .Select(d => new OpportunityDeliverable
                {
                    OpportunityId = id,
                    OutputId = d.OutputId,
                    Quantity = d.Quantity,
                    Notes = d.Notes
                })
                .ToList();
        }

        await opportunityRepository.UpdateAsync(entity);

        // Reload with all includes for complete response
        return await GetOpportunityAsync(entity.Id);
    }

    public async Task<OpportunityModel> UpdateWhySectionAsync(int id, WhySectionRequest request)
    {
        var entity = await opportunityRepository.GetByIdAsync(id, new[]
        {
            nameof(Opportunity.SDGs),
            nameof(Opportunity.UNCFOutcomes),
            nameof(Opportunity.UNCFIndicators),
            nameof(Opportunity.UNOPSMissions)
        });

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update WHY section fields

        if (request.ExpectedBeneficiaries != null)
        {
            entity.ExpectedBeneficiaries = request.ExpectedBeneficiaries;
        }
        
        // Update beneficiary numbers
        entity.EstimatedDirectBeneficiaries = request.EstimatedDirectBeneficiaries;
        entity.EstimatedIndirectBeneficiaries = request.EstimatedIndirectBeneficiaries;
        entity.BeneficiariesToBeDetermined = request.BeneficiariesToBeDetermined;

        if (request.IntendedImpactOutcomes != null)
        {
            entity.IntendedImpactOutcomes = request.IntendedImpactOutcomes;
        }

        if (request.Challenges != null)
        {
            entity.Challenges = request.Challenges;
        }

        if (request.ResultsFocus != null)
        {
            entity.ResultsFocus = request.ResultsFocus;
        }

        // Update SDG alignments with differential update strategy
        if (request.SdGs != null)
        {
            // Load existing SDGs with their targets and indicators for comparison
            var existingSDGs = await context.OpportunitySDGs
                .Where(sdg => sdg.OpportunityId == id)
                .Include(sdg => sdg.Targets)
                    .ThenInclude(t => t.Indicators)
                .ToListAsync();

            var requestedSDGIds = request.SdGs.Select(s => s.SDGId).ToHashSet();
            var existingSDGIds = existingSDGs.Select(s => s.SDGId).ToHashSet();

            // Remove SDGs that are no longer in the request
            var sdgsToRemove = existingSDGs.Where(s => !requestedSDGIds.Contains(s.SDGId)).ToList();
            if (sdgsToRemove.Any())
            {
                context.OpportunitySDGs.RemoveRange(sdgsToRemove);
            }

            // Process each requested SDG
            foreach (var sdgRequest in request.SdGs)
            {
                var existingSDG = existingSDGs.FirstOrDefault(s => s.SDGId == sdgRequest.SDGId);

                if (existingSDG == null)
                {
                    // Add new SDG with its targets and indicators
                    var newSDG = new OpportunitySDG
                    {
                        OpportunityId = id,
                        SDGId = sdgRequest.SDGId,
                        IsPrimary = sdgRequest.IsPrimary,
                        SkipTargetsAndIndicators = sdgRequest.SkipTargetsAndIndicators,
                        Notes = sdgRequest.Notes
                    };

                    // Add targets only if not skipped
                    if (sdgRequest.SkipTargetsAndIndicators != true && sdgRequest.Targets != null && sdgRequest.Targets.Any())
                    {
                        foreach (var targetRequest in sdgRequest.Targets)
                        {
                            var newTarget = new OpportunitySDGTarget
                            {
                                OpportunityId = id,
                                SDGTargetId = targetRequest.SDGTargetDatabaseId,
                                Notes = targetRequest.Notes
                            };

                            // Add indicators
                            if (targetRequest.SDGIndicatorDatabaseIds != null && targetRequest.SDGIndicatorDatabaseIds.Any())
                            {
                                foreach (var indicatorId in targetRequest.SDGIndicatorDatabaseIds)
                                {
                                    newTarget.Indicators.Add(new OpportunitySDGIndicator
                                    {
                                        OpportunityId = id,
                                        SDGIndicatorId = indicatorId
                                    });
                                }
                            }

                            newSDG.Targets.Add(newTarget);
                        }
                    }

                    context.OpportunitySDGs.Add(newSDG);
                }
                else
                {
                    // Update existing SDG properties
                    existingSDG.IsPrimary = sdgRequest.IsPrimary;
                    existingSDG.SkipTargetsAndIndicators = sdgRequest.SkipTargetsAndIndicators;
                    existingSDG.Notes = sdgRequest.Notes;

                    // If user opted to skip targets and indicators, remove all existing ones
                    if (sdgRequest.SkipTargetsAndIndicators == true)
                    {
                        if (existingSDG.Targets.Any())
                        {
                            var allTargets = existingSDG.Targets.ToList();
                            foreach (var target in allTargets)
                            {
                                existingSDG.Targets.Remove(target);
                                context.OpportunitySDGTargets.Remove(target);
                            }
                        }
                    }
                    else
                    {
                        // Update targets with differential strategy only if not skipped
                        var requestedTargetIds = sdgRequest.Targets?.Select(t => t.SDGTargetDatabaseId).ToHashSet() ?? new HashSet<int>();

                        // Remove targets that are no longer in the request
                        var targetsToRemove = existingSDG.Targets.Where(t => !requestedTargetIds.Contains(t.SDGTargetId)).ToList();
                        if (targetsToRemove.Any())
                        {
                            foreach (var target in targetsToRemove)
                            {
                                existingSDG.Targets.Remove(target);
                                context.OpportunitySDGTargets.Remove(target);
                            }
                        }

                        // Process each requested target
                        if (sdgRequest.Targets != null)
                        {
                            foreach (var targetRequest in sdgRequest.Targets)
                            {
                                var existingTarget = existingSDG.Targets.FirstOrDefault(t => t.SDGTargetId == targetRequest.SDGTargetDatabaseId);

                                if (existingTarget == null)
                                {
                                    // Add new target with its indicators
                                    var newTarget = new OpportunitySDGTarget
                                    {
                                        OpportunityId = id,
                                        OpportunitySDGId = existingSDG.Id,
                                        SDGTargetId = targetRequest.SDGTargetDatabaseId,
                                        Notes = targetRequest.Notes
                                    };

                                    // Add indicators
                                    if (targetRequest.SDGIndicatorDatabaseIds != null && targetRequest.SDGIndicatorDatabaseIds.Any())
                                    {
                                        foreach (var indicatorId in targetRequest.SDGIndicatorDatabaseIds)
                                        {
                                            newTarget.Indicators.Add(new OpportunitySDGIndicator
                                            {
                                                OpportunityId = id,
                                                SDGIndicatorId = indicatorId
                                            });
                                        }
                                    }

                                    existingSDG.Targets.Add(newTarget);
                                }
                                else
                                {
                                    // Update existing target properties
                                    existingTarget.Notes = targetRequest.Notes;

                                    // Update indicators with differential strategy
                                    var requestedIndicatorIds = targetRequest.SDGIndicatorDatabaseIds?.ToHashSet() ?? new HashSet<int>();
                                    var existingIndicatorIds = existingTarget.Indicators.Select(i => i.SDGIndicatorId).ToHashSet();

                                    // Remove indicators that are no longer in the request
                                    var indicatorsToRemove = existingTarget.Indicators.Where(i => !requestedIndicatorIds.Contains(i.SDGIndicatorId)).ToList();
                                    if (indicatorsToRemove.Any())
                                    {
                                        foreach (var indicator in indicatorsToRemove)
                                        {
                                            existingTarget.Indicators.Remove(indicator);
                                            context.OpportunitySDGIndicators.Remove(indicator);
                                        }
                                    }

                                    // Add new indicators
                                    if (targetRequest.SDGIndicatorDatabaseIds != null)
                                    {
                                        foreach (var indicatorId in targetRequest.SDGIndicatorDatabaseIds)
                                        {
                                            if (!existingIndicatorIds.Contains(indicatorId))
                                            {
                                                existingTarget.Indicators.Add(new OpportunitySDGIndicator
                                                {
                                                    OpportunityId = id,
                                                    OpportunitySDGTargetId = existingTarget.Id,
                                                    SDGIndicatorId = indicatorId
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }  // End of else block for differential target update
                }
            }
        }

        // Update UNCF Outcome alignments with differential update strategy
        if (request.UncfOutcomes != null)
        {
            // Load existing UNCF outcomes with their indicators for comparison
            var existingUNCFOutcomes = await context.OpportunityUNCFOutcomes
                .Where(uo => uo.OpportunityId == id)
                .Include(uo => uo.Indicators)
                .ToListAsync();

            // Create composite keys for comparison (OpportunityCountryId + UNCFOutcomeId)
            var requestedOutcomeKeys = request.UncfOutcomes
                .Select(uo => new { uo.OpportunityCountryId, uo.UNCFOutcomeId })
                .ToHashSet();
            
            var existingOutcomeKeys = existingUNCFOutcomes
                .Select(uo => new { uo.OpportunityCountryId, uo.UNCFOutcomeId })
                .ToHashSet();

            // Remove UNCF outcomes that are no longer in the request
            var outcomesToRemove = existingUNCFOutcomes
                .Where(uo => !requestedOutcomeKeys.Contains(new { uo.OpportunityCountryId, uo.UNCFOutcomeId }))
                .ToList();
            
            if (outcomesToRemove.Any())
            {
                context.OpportunityUNCFOutcomes.RemoveRange(outcomesToRemove);
            }

            // Process each requested UNCF outcome
            foreach (var outcomeRequest in request.UncfOutcomes)
            {
                var existingOutcome = existingUNCFOutcomes.FirstOrDefault(uo => 
                    uo.OpportunityCountryId == outcomeRequest.OpportunityCountryId && 
                    uo.UNCFOutcomeId == outcomeRequest.UNCFOutcomeId);

                if (existingOutcome == null)
                {
                    // Add new UNCF outcome with its indicators
                    var newOutcome = new OpportunityUNCFOutcome
                    {
                        OpportunityId = id,
                        OpportunityCountryId = outcomeRequest.OpportunityCountryId,
                        UNCFOutcomeId = outcomeRequest.UNCFOutcomeId,
                        Notes = outcomeRequest.Notes
                    };

                    // Add indicators if provided
                    if (outcomeRequest.UNCFIndicatorIds != null && outcomeRequest.UNCFIndicatorIds.Any())
                    {
                        foreach (var indicatorId in outcomeRequest.UNCFIndicatorIds)
                        {
                            newOutcome.Indicators.Add(new OpportunityUNCFIndicator
                            {
                                OpportunityId = id,
                                UNCFIndicatorId = indicatorId
                            });
                        }
                    }

                    context.OpportunityUNCFOutcomes.Add(newOutcome);
                }
                else
                {
                    // Update existing UNCF outcome properties
                    existingOutcome.Notes = outcomeRequest.Notes;

                    // Update indicators with differential strategy
                    var requestedIndicatorIds = outcomeRequest.UNCFIndicatorIds?.ToHashSet() ?? new HashSet<int>();
                    var existingIndicatorIds = existingOutcome.Indicators.Select(i => i.UNCFIndicatorId).ToHashSet();

                    // Remove indicators that are no longer in the request
                    var indicatorsToRemove = existingOutcome.Indicators
                        .Where(i => !requestedIndicatorIds.Contains(i.UNCFIndicatorId))
                        .ToList();
                    
                    if (indicatorsToRemove.Any())
                    {
                        foreach (var indicator in indicatorsToRemove)
                        {
                            existingOutcome.Indicators.Remove(indicator);
                            context.OpportunityUNCFIndicators.Remove(indicator);
                        }
                    }

                    // Add new indicators
                    if (outcomeRequest.UNCFIndicatorIds != null)
                    {
                        foreach (var indicatorId in outcomeRequest.UNCFIndicatorIds)
                        {
                            if (!existingIndicatorIds.Contains(indicatorId))
                            {
                                existingOutcome.Indicators.Add(new OpportunityUNCFIndicator
                                {
                                    OpportunityId = id,
                                    OpportunityUNCFOutcomeId = existingOutcome.Id,
                                    UNCFIndicatorId = indicatorId
                                });
                            }
                        }
                    }
                }
            }
        }

        // Update UNOPS Mission alignments with differential update strategy
        if (request.UNOPSMissions != null)
        {
            // Load existing UNOPS mission alignments
            var existingMissions = await context.Set<OpportunityUNOPSMission>()
                .Where(m => m.OpportunityId == id)
                .ToListAsync();

            var requestedMissionIds = request.UNOPSMissions.Select(m => m.UNOPSMissionId).ToHashSet();
            var existingMissionIds = existingMissions.Select(m => m.UNOPSMissionId).ToHashSet();

            // Remove missions that are no longer in the request
            var missionsToRemove = existingMissions.Where(m => !requestedMissionIds.Contains(m.UNOPSMissionId)).ToList();
            if (missionsToRemove.Any())
            {
                context.Set<OpportunityUNOPSMission>().RemoveRange(missionsToRemove);
            }

            // Process each requested mission
            foreach (var missionRequest in request.UNOPSMissions)
            {
                var existingMission = existingMissions.FirstOrDefault(m => m.UNOPSMissionId == missionRequest.UNOPSMissionId);

                if (existingMission == null)
                {
                    // Add new mission alignment
                    var newMission = new OpportunityUNOPSMission
                    {
                        OpportunityId = id,
                        UNOPSMissionId = missionRequest.UNOPSMissionId
                    };
                    context.Set<OpportunityUNOPSMission>().Add(newMission);
                }
                // No update needed for existing missions - junction table only has IDs
            }
        }

        await opportunityRepository.UpdateAsync(entity);

        // Reload with all includes for complete response
        return await GetOpportunityAsync(entity.Id);
    }

    public async Task<OpportunityModel> UpdateWhoSectionAsync(int id, WhoSectionRequest request)
    {
        var opportunity = await context.Opportunities
            .Include(o => o.FundingPartners)
            .Include(o => o.ClientPartners)
            .Include(o => o.Stakeholders)
            .Include(o => o.ExternalStakeholders)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update pooled funding flag
        opportunity.IsPooledFunding = request.IsPooledFunding;

        // Update Funding Partners
        if (request.FundingPartners != null)
        {
            // Remove existing funding partners
            if (opportunity.FundingPartners != null && opportunity.FundingPartners.Any())
            {
                context.OpportunityFundingPartners.RemoveRange(opportunity.FundingPartners);
            }

            // Get a valid currency ID (preferably USD, or the first available)
            var defaultCurrencyId = context.Currencies
                .Where(c => c.Code == "USD")
                .Select(c => c.Id)
                .FirstOrDefault();
            
            if (defaultCurrencyId == 0)
            {
                // Fallback to first available currency
                defaultCurrencyId = context.Currencies
                    .Select(c => c.Id)
                    .FirstOrDefault();
            }

            // Add new funding partners
            var exchangeRateService = new ExchangeRateService(context);
            var fundingPartners = new List<OpportunityFundingPartner>();
            
            foreach (var fp in request.FundingPartners)
            {
                var partner = await context.Partners.FindAsync(fp.PartnerId);
                var currency = await context.Currencies.FindAsync(fp.CurrencyId ?? defaultCurrencyId);
                
                var fundingPartner = new OpportunityFundingPartner
                {
                    OpportunityId = id,
                    PartnerId = fp.PartnerId,
                    Amount = fp.Amount,
                    CurrencyId = fp.CurrencyId ?? defaultCurrencyId,
                    Percentage = fp.Percentage,
                    FeePercentage = fp.FeePercentage,
                    FeeAmount = fp.FeeAmount,
                    FeeAmountUSD = fp.FeeAmountUSD,
                    IsAmountBasedFee = fp.IsAmountBasedFee,
                    PartnershipAgreementReference = fp.PartnershipAgreementReference,
                    DocumentId = fp.DocumentId,
                    IsPooledContribution = fp.IsPooledContribution,
                    SelectedPartnerAgreementNumber = fp.SelectedPartnerAgreementNumber
                    // PartnerPreferredCurrency will remain null until Partner entity gets this field
                };
                
                // Convert amount to USD if amount is provided
                if (fp.Amount.HasValue && fp.Amount.Value > 0 && currency != null)
                {
                    try
                    {
                        var conversionResult = await exchangeRateService.ConvertToUSDAsync(
                            fp.Amount.Value, 
                            currency.Code ?? "USD"
                        );
                        
                        fundingPartner.AmountUSD = conversionResult.AmountUSD;
                        fundingPartner.ExchangeRate = conversionResult.ExchangeRate;
                        fundingPartner.ExchangeRateDate = conversionResult.ExchangeRateDate;
                        fundingPartner.ExchangeRateId = conversionResult.ExchangeRateId > 0 ? conversionResult.ExchangeRateId : null;
                    }
                    catch (Exception ex)
                    {
                        // Log warning but don't fail the operation
                        Console.WriteLine($"Warning: Could not convert amount to USD for partner {fp.PartnerId}: {ex.Message}");
                        // If conversion fails, just store the original amount as USD
                        fundingPartner.AmountUSD = fp.Amount.Value;
                        fundingPartner.ExchangeRate = 1.0m;
                        fundingPartner.ExchangeRateDate = DateTime.UtcNow;
                    }
                }
                
                fundingPartners.Add(fundingPartner);
            }
            
            opportunity.FundingPartners = fundingPartners;
        }

        // Update Client Partners
        if (request.ClientPartners != null)
        {
            // Remove existing client partners
            if (opportunity.ClientPartners != null && opportunity.ClientPartners.Any())
            {
                context.OpportunityClientPartners.RemoveRange(opportunity.ClientPartners);
            }

            // Add new client partners
            opportunity.ClientPartners = request.ClientPartners
                .Select(cp => new OpportunityClientPartner
                {
                    OpportunityId = id,
                    PartnerId = cp.PartnerId,
                    SelectedPartnerAgreementNumber = cp.SelectedPartnerAgreementNumber
                })
                .ToList();
        }

        // Update Stakeholders
        if (request.Stakeholders != null)
        {
            // Get entity roles to check AllowsMultiple property
            var entityRoleIds = request.Stakeholders.Select(s => s.EntityRoleId).Distinct().ToList();
            var entityRoles = await context.Set<EntityRole>()
                .Where(er => entityRoleIds.Contains(er.Id))
                .ToDictionaryAsync(er => er.Id);

            // Validate that single-assignment roles don't have duplicates
            var roleGroups = request.Stakeholders
                .GroupBy(s => s.EntityRoleId)
                .ToList();

            foreach (var roleGroup in roleGroups)
            {
                if (entityRoles.TryGetValue(roleGroup.Key, out var entityRole))
                {
                    if (!entityRole.AllowsMultiple && roleGroup.Count() > 1)
                    {
                        throw new BusinessException($"The role '{entityRole.Name}' does not allow multiple assignments. Only one person can be assigned to this role.");
                    }
                }
            }

            // Remove existing stakeholders
            if (opportunity.Stakeholders != null && opportunity.Stakeholders.Any())
            {
                context.Set<OpportunityStakeholder>().RemoveRange(opportunity.Stakeholders);
            }

            // Add new stakeholders
            opportunity.Stakeholders = request.Stakeholders
                .Select(s => new OpportunityStakeholder
                {
                    OpportunityId = id,
                    UserId = s.UserId,
                    EntityRoleId = s.EntityRoleId,
                    IsInternal = true, // Internal stakeholders only for now
                    StakeholderType = "Internal",
                    Notes = s.Notes
                })
                .ToList();
        }
        
        // Update External Stakeholders
        if (request.ExternalStakeholders != null && request.ExternalStakeholders.Any())
        {
            // Get all partner IDs from the opportunity
            var opportunityPartnerIds = new HashSet<int>();
            
            // Add funding partner IDs
            if (opportunity.FundingPartners != null)
            {
                foreach (var fp in opportunity.FundingPartners)
                {
                    opportunityPartnerIds.Add(fp.PartnerId);
                }
            }
            
            // Add client partner IDs
            if (opportunity.ClientPartners != null)
            {
                foreach (var cp in opportunity.ClientPartners)
                {
                    opportunityPartnerIds.Add(cp.PartnerId);
                }
            }
            
            // Validate that all contacts belong to the opportunity's partners
            var contactIds = request.ExternalStakeholders.Select(es => es.ContactId).Distinct().ToList();
            var contacts = await context.Contacts
                .Where(c => contactIds.Contains(c.Id))
                .ToListAsync();
            
            foreach (var contact in contacts)
            {
                if (contact.PartnerId == 0 || !opportunityPartnerIds.Contains(contact.PartnerId))
                {
                    throw new BusinessException("All external stakeholder contacts must belong to the opportunity's funding or client partners.");
                }
            }
            
            // Remove existing external stakeholders
            if (opportunity.ExternalStakeholders != null && opportunity.ExternalStakeholders.Any())
            {
                context.Set<OpportunityExternalStakeholder>().RemoveRange(opportunity.ExternalStakeholders);
            }

            // Add new external stakeholders
            opportunity.ExternalStakeholders = request.ExternalStakeholders
                .Select(es => new OpportunityExternalStakeholder
                {
                    OpportunityId = id,
                    ContactId = es.ContactId
                })
                .ToList();
        }
        else if (request.ExternalStakeholders != null && !request.ExternalStakeholders.Any())
        {
            // If empty list is sent, remove all external stakeholders
            if (opportunity.ExternalStakeholders != null && opportunity.ExternalStakeholders.Any())
            {
                context.Set<OpportunityExternalStakeholder>().RemoveRange(opportunity.ExternalStakeholders);
            }
        }
        
        // Update misc external stakeholders and notes
        opportunity.MiscExternalStakeholders = request.MiscExternalStakeholders;
        opportunity.ExternalStakeholderNotes = request.ExternalStakeholderNotes;

        await context.SaveChangesAsync();

        // Reload with all includes
        var result = await GetOpportunityAsync(id);
        return result ?? throw new KeyNotFoundException($"Failed to reload opportunity {id}");
    }

    public async Task<OpportunityModel> UpdateWhereSectionAsync(int id, WhereSectionRequest request)
    {
        var opportunity = await context.Opportunities
            .Include(o => o.Countries)
                .ThenInclude(c => c.Country)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update Countries with differential update strategy
        // CRITICAL: Do NOT remove and re-add countries as this will CASCADE DELETE all related
        // OpportunityUNCFOutcomes and OpportunityUNCFIndicators due to the foreign key relationship
        if (request.Countries != null)
        {
            // Initialize Countries collection if null
            if (opportunity.Countries == null)
            {
                opportunity.Countries = new List<OpportunityCountry>();
            }
            
            var existingCountries = opportunity.Countries.ToList();
            var requestedCountryIds = request.Countries.Select(c => c.CountryId).ToHashSet();
            var existingCountryIds = existingCountries.Select(c => c.CountryId).ToHashSet();

            // Compute OrgUnitWithStrategyId for each country
            var countryOrgUnitStrategyMap = await ComputeOrgUnitWithStrategyForCountriesAsync(
                request.Countries.Select(c => c.CountryId).ToList());

            // Remove countries that are no longer in the request
            var countriesToRemove = existingCountries.Where(c => !requestedCountryIds.Contains(c.CountryId)).ToList();
            if (countriesToRemove.Any())
            {
                context.OpportunityCountries.RemoveRange(countriesToRemove);
            }

            // Process each requested country
            foreach (var countryRequest in request.Countries)
            {
                var existingCountry = existingCountries.FirstOrDefault(c => c.CountryId == countryRequest.CountryId);

                if (existingCountry == null)
                {
                    // Add new country
                    var newCountry = new OpportunityCountry
                    {
                        OpportunityId = id,
                        CountryId = countryRequest.CountryId,
                        SpecificAreas = countryRequest.SpecificAreas,
                        HumanitarianFrameworkAlignment = countryRequest.HumanitarianFrameworkAlignment,
                        NdcAlignment = countryRequest.NdcAlignment,
                        NapAlignment = countryRequest.NapAlignment,
                        OrgUnitStrategyAlignment = countryRequest.OrgUnitStrategyAlignment,
                        OrgUnitWithStrategyId = countryOrgUnitStrategyMap.ContainsKey(countryRequest.CountryId) 
                            ? countryOrgUnitStrategyMap[countryRequest.CountryId] 
                            : null
                    };
                    opportunity.Countries.Add(newCountry);
                }
                else
                {
                    // Update existing country properties
                    existingCountry.SpecificAreas = countryRequest.SpecificAreas;
                    existingCountry.HumanitarianFrameworkAlignment = countryRequest.HumanitarianFrameworkAlignment;
                    existingCountry.NdcAlignment = countryRequest.NdcAlignment;
                    existingCountry.NapAlignment = countryRequest.NapAlignment;
                    existingCountry.OrgUnitStrategyAlignment = countryRequest.OrgUnitStrategyAlignment;
                    existingCountry.OrgUnitWithStrategyId = countryOrgUnitStrategyMap.ContainsKey(countryRequest.CountryId) 
                        ? countryOrgUnitStrategyMap[countryRequest.CountryId] 
                        : null;
                }
            }
        }

        await context.SaveChangesAsync();

        // Reload with all includes
        var result = await GetOpportunityAsync(id);
        return result ?? throw new KeyNotFoundException($"Failed to reload opportunity {id}");
    }

    public async Task<RelatedItemsModel> GetRelatedItemsAsync(int id)
    {
        var opportunity = await context.Opportunities
            .Include(o => o.FundingPartners)
                .ThenInclude(fp => fp.Partner)
            .Include(o => o.ClientPartners)
                .ThenInclude(cp => cp.Partner)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        var result = new RelatedItemsModel();

        // Get all partner IDs from funding and client partners
        var partnerIds = new List<int>();
        if (opportunity.FundingPartners != null)
        {
            partnerIds.AddRange(opportunity.FundingPartners.Select(fp => fp.PartnerId));
        }
        if (opportunity.ClientPartners != null)
        {
            partnerIds.AddRange(opportunity.ClientPartners.Select(cp => cp.PartnerId));
        }

        partnerIds = partnerIds.Distinct().ToList();

        if (partnerIds.Any())
        {
            // Get contacts for these partners
            var contacts = await context.Contacts
                .Where(c => partnerIds.Contains(c.PartnerId) && !c.IsDeleted)
                .Include(c => c.Partner)
                .OrderBy(c => c.Name)
                .Select(c => new RelatedContactModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    JobTitle = c.Title, // Title is the property name in Contact entity
                    LogoUrl = c.ProfilePictureUrl,
                    OrganizationId = c.PartnerId,
                    OrganizationName = c.Partner != null ? c.Partner.Name : null
                })
                .ToListAsync();

            result.Contacts = contacts;

            // Get partners (distinct from funding and client)
            var partners = await context.Partners
                .Where(p => partnerIds.Contains(p.Id) && !p.IsDeleted)
                .OrderBy(p => p.Name)
                .Select(p => new RelatedPartnerModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    LogoUrl = p.LogoUrl,
                    PartnerType = null, // Can add PartnerCategory if needed
                    Country = null // Can add country if available
                })
                .ToListAsync();

            result.Partners = partners;

            // Get interactions involving these partners
            var interactions = await context.Interactions
                .Where(i => i.InteractionPartners != null && 
                           i.InteractionPartners.Any(ip => partnerIds.Contains(ip.PartnerId)) && 
                           !i.IsDeleted)
                .Include(i => i.InteractionPartners)
                    .ThenInclude(ip => ip.Partner)
                .OrderByDescending(i => i.Date)
                .Take(50) // Limit to recent 50 interactions
                .Select(i => new RelatedInteractionModel
                {
                    Id = i.Id,
                    Subject = i.Name,
                    InteractionType = i.Type.ToString(), // Type is an enum
                    InteractionDate = i.Date,
                    Description = i.Description,
                    PartnerId = i.InteractionPartners != null && i.InteractionPartners.Any() 
                        ? i.InteractionPartners.First().PartnerId 
                        : null,
                    PartnerName = i.InteractionPartners != null && i.InteractionPartners.Any() 
                        ? i.InteractionPartners.First().Partner.Name 
                        : null
                })
                .ToListAsync();

            result.Interactions = interactions;
        }

        return result;
    }

    public async Task<OpportunityModel> UpdateWhenSectionAsync(int id, WhenSectionRequest request)
    {
        var opportunity = await context.Opportunities
            .Include(o => o.Deliverables)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update target dates
        opportunity.TargetSigningDate = request.TargetSigningDate;
        opportunity.ImplementationStartDate = request.ImplementationStartDate;
        opportunity.TargetDeliveryDate = request.TargetDeliveryDate;
        
        // Update signing date details (AC5)
        if (request.IsTargetSigningDateFirm.HasValue)
        {
            opportunity.IsTargetSigningDateFirm = request.IsTargetSigningDateFirm.Value;
        }
        opportunity.SigningDateNotes = request.SigningDateNotes;
        opportunity.SubmissionDeadline = request.SubmissionDeadline;

        // Update deliverable planned dates (Work Breakdown Structure)
        if (request.Deliverables != null && request.Deliverables.Any())
        {
            foreach (var deliverableUpdate in request.Deliverables)
            {
                var deliverable = opportunity.Deliverables?.FirstOrDefault(d => d.Id == deliverableUpdate.Id);
                if (deliverable != null)
                {
                    deliverable.PlannedStartDate = deliverableUpdate.PlannedStartDate;
                    deliverable.PlannedEndDate = deliverableUpdate.PlannedEndDate;
                }
            }
        }

        await context.SaveChangesAsync();

        // Reload with all includes
        var result = await GetOpportunityAsync(id);
        return result ?? throw new KeyNotFoundException($"Failed to reload opportunity {id}");
    }

    /// <summary>
    /// Apply AI-extracted changes to an opportunity across multiple sections
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    /// <param name="request">AI changes request containing fields to update</param>
    /// <returns>Updated opportunity model</returns>
    public async Task<OpportunityModel> ApplyAiChangesAsync(int id, ApplyOpportunityAiChangesRequest request)
    {
        // Load entity with all relevant navigation properties
        var entity = await opportunityRepository.GetByIdAsync(id, new[]
        {
            nameof(Opportunity.Deliverables),
            nameof(Opportunity.SDGs),
            nameof(Opportunity.FundingPartners),
            nameof(Opportunity.ClientPartners),
            nameof(Opportunity.Stakeholders),
            nameof(Opportunity.Countries)
        });

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // WHAT Section - Update basic properties
        if (request.Name != null)
        {
            entity.Name = request.Name;
        }

        if (request.Description != null)
        {
            entity.Description = request.Description;
        }

        if (request.ResponsibleOrgUnitId.HasValue)
        {
            entity.ResponsibleOrgUnitId = request.ResponsibleOrgUnitId.Value;
        }

        if (request.ProposedInitiativeTypeId.HasValue)
        {
            entity.ProposedInitiativeTypeId = request.ProposedInitiativeTypeId.Value;
        }

        // Update deliverables
        if (request.Deliverables != null)
        {
            // Remove existing deliverables
            if (entity.Deliverables != null && entity.Deliverables.Any())
            {
                context.Set<OpportunityDeliverable>().RemoveRange(entity.Deliverables);
            }

            // Add new deliverables
            entity.Deliverables = request.Deliverables
                .Select(d => new OpportunityDeliverable
                {
                    OpportunityId = id,
                    OutputId = d.OutputId,
                    Quantity = d.Quantity,
                    Notes = d.Notes
                })
                .ToList();
        }

        // WHY Section - Update strategic properties

        if (request.ResultsFocus != null)
        {
            entity.ResultsFocus = request.ResultsFocus;
        }

        if (request.IntendedImpactOutcomes != null)
        {
            entity.IntendedImpactOutcomes = request.IntendedImpactOutcomes;
        }

        if (request.ExpectedBeneficiaries != null)
        {
            entity.ExpectedBeneficiaries = request.ExpectedBeneficiaries;
        }

        // Update SDGs
        if (request.SdGs != null)
        {
            // Remove existing SDGs
            if (entity.SDGs != null && entity.SDGs.Any())
            {
                context.Set<OpportunitySDG>().RemoveRange(entity.SDGs);
            }

            // Add new SDGs
            entity.SDGs = request.SdGs
                .Select(sdgId => new OpportunitySDG
                {
                    OpportunityId = id,
                    SDGId = sdgId
                })
                .ToList();
        }

        // WHO Section - Update partnerships
        if (request.FundingPartners != null)
        {
            // Remove existing funding partners
            if (entity.FundingPartners != null && entity.FundingPartners.Any())
            {
                context.Set<OpportunityFundingPartner>().RemoveRange(entity.FundingPartners);
            }

            // Get a valid currency ID (preferably USD, or the first available)
            var defaultCurrencyId = context.Currencies
                .Where(c => c.Code == "USD")
                .Select(c => c.Id)
                .FirstOrDefault();
            
            if (defaultCurrencyId == 0)
            {
                // Fallback to first available currency
                defaultCurrencyId = context.Currencies
                    .Select(c => c.Id)
                    .FirstOrDefault();
            }

            // Add new funding partners
            entity.FundingPartners = request.FundingPartners
                .Select(partnerId => new OpportunityFundingPartner
                {
                    OpportunityId = id,
                    PartnerId = partnerId,
                    CurrencyId = defaultCurrencyId // Set default USD currency
                })
                .ToList();
        }

        if (request.ClientPartners != null)
        {
            // Remove existing client partners
            if (entity.ClientPartners != null && entity.ClientPartners.Any())
            {
                context.Set<OpportunityClientPartner>().RemoveRange(entity.ClientPartners);
            }

            // Add new client partners
            entity.ClientPartners = request.ClientPartners
                .Select(partnerId => new OpportunityClientPartner
                {
                    OpportunityId = id,
                    PartnerId = partnerId
                })
                .ToList();
        }

        if (request.Stakeholders != null)
        {
            // Remove existing stakeholders
            if (entity.Stakeholders != null && entity.Stakeholders.Any())
            {
                context.Set<OpportunityStakeholder>().RemoveRange(entity.Stakeholders);
            }

            // Add new stakeholders
            entity.Stakeholders = request.Stakeholders
                .Select(entityRoleId => new OpportunityStakeholder
                {
                    OpportunityId = id,
                    EntityRoleId = entityRoleId
                })
                .ToList();
        }

        // WHERE Section - Update countries
        if (request.Countries != null)
        {
            // Remove existing countries
            if (entity.Countries != null && entity.Countries.Any())
            {
                context.Set<OpportunityCountry>().RemoveRange(entity.Countries);
            }

            // Add new countries
            entity.Countries = request.Countries
                .Select(countryId => new OpportunityCountry
                {
                    OpportunityId = id,
                    CountryId = countryId
                })
                .ToList();
        }

        // WHEN Section - Update dates
        if (request.TargetSigningDate.HasValue)
        {
            entity.TargetSigningDate = request.TargetSigningDate.Value;
        }

        if (request.TargetDeliveryDate.HasValue)
        {
            entity.TargetDeliveryDate = request.TargetDeliveryDate.Value;
        }

        // Other properties
        if (request.PartnerReference != null)
        {
            entity.PartnerReference = request.PartnerReference;
        }

        if (request.Status != null)
        {
            if (Enum.TryParse<EntityStatus>(request.Status, true, out var status))
            {
                entity.Status = status;
            }
        }

        if (request.WorkflowStageId.HasValue)
        {
            entity.WorkflowStageId = request.WorkflowStageId.Value;
        }

        if (request.InitiativeBudgetUSD.HasValue)
        {
            entity.InitiativeBudgetUSD = request.InitiativeBudgetUSD.Value;
        }

        // Save changes
        await opportunityRepository.UpdateAsync(entity);

        // Reload with all includes for complete response
        return await GetOpportunityAsync(entity.Id);
    }

    public async Task<bool> DeleteOpportunityAsync(int id)
    {
        var entity = await opportunityRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return false;
        }

        await opportunityRepository.Delete(entity);
        return true;
    }

    private OpportunityStats ComputeOpportunityStats(Opportunity opportunity)
    {
        var stats = new OpportunityStats
        {
            FundingPartnerCount = opportunity.FundingPartners?.Count ?? 0,
            ClientPartnerCount = opportunity.ClientPartners?.Count ?? 0,
            StakeholderCount = opportunity.Stakeholders?.Count ?? 0,
            DeliverableCount = opportunity.Deliverables?.Count ?? 0,
            CountryCount = opportunity.Countries?.Count ?? 0,
            SDGCount = opportunity.SDGs?.Count ?? 0,
            InternalStakeholderCount = opportunity.Stakeholders?.Count(s => s.IsInternal) ?? 0,
            ExternalStakeholderCount = opportunity.Stakeholders?.Count(s => !s.IsInternal) ?? 0
        };

        // Calculate total funding from all funding partners
        if (opportunity.FundingPartners != null && opportunity.FundingPartners.Any())
        {
            // Sum all funding partner amounts in USD
            stats.TotalFundingUSD = opportunity.FundingPartners
                .Where(fp => fp.AmountUSD.HasValue)
                .Sum(fp => fp.AmountUSD.Value);
                
            stats.TotalFeeAmountUSD = opportunity.FundingPartners
                .Where(fp => fp.FeeAmountUSD.HasValue)
                .Sum(fp => fp.FeeAmountUSD.Value);
        }

        // Primary SDG
        stats.PrimarySDGId = opportunity.SDGs?.FirstOrDefault(s => s.IsPrimary)?.SDGId;

        return stats;
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

    /// <summary>
    /// Data retrieval method for AI prompts - Gets comprehensive opportunity details for keyword extraction
    /// This method is called via reflection by the BaseUNOPSManager
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    /// <returns>Dictionary containing all opportunity details formatted for AI prompt placeholders</returns>
    public async Task<Dictionary<string, object>> GetOpportunityDetailsForAIAsync(int id)
    {
        var opportunity = await context.Set<Opportunity>()
            .Include(o => o.ResponsibleOrgUnit)
            .Include(o => o.ProposedInitiativeType)
            .Include(o => o.FundingPartners).ThenInclude(fp => fp.Partner)
            .Include(o => o.ClientPartners).ThenInclude(cp => cp.Partner)
            .Include(o => o.Stakeholders).ThenInclude(s => s.User).ThenInclude(u => u.UserProfile)
            .Include(o => o.Deliverables).ThenInclude(d => d.Output)
            .Include(o => o.Countries).ThenInclude(c => c.Country)
            .Include(o => o.SDGs).ThenInclude(s => s.SDG)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        var stats = ComputeOpportunityStats(opportunity);

        // Format arrays as comma-separated strings for the AI prompt
        var fundingPartners = opportunity.FundingPartners?
            .Select(fp => fp.Partner?.Name ?? "Unknown")
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList() ?? new List<string>();

        var clientPartners = opportunity.ClientPartners?
            .Select(cp => cp.Partner?.Name ?? "Unknown")
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList() ?? new List<string>();

        var stakeholders = opportunity.Stakeholders?
            .Select(s => s.User?.Name ?? "Unknown")
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList() ?? new List<string>();

        var deliverables = opportunity.Deliverables?
            .Select(d => d.Output?.Name ?? d.Notes ?? "")
            .Where(desc => !string.IsNullOrEmpty(desc))
            .ToList() ?? new List<string>();

        var countries = opportunity.Countries?
            .Select(c => c.Country?.Name ?? "Unknown")
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList() ?? new List<string>();

        var sdgs = opportunity.SDGs?
            .Select(s => s.SDG?.Name ?? "Unknown")
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList() ?? new List<string>();

        // Return dictionary with all placeholders the AI prompt expects
        return new Dictionary<string, object>
        {
            ["id"] = opportunity.Id.ToString(),
            ["name"] = opportunity.Name ?? "",
            ["description"] = opportunity.Description ?? "",
            ["partnerReference"] = opportunity.PartnerReference ?? "",
            ["status"] = opportunity.Status.ToString(),
            ["responsibleOrgUnitName"] = opportunity.ResponsibleOrgUnit?.Name ?? "",
            ["proposedInitiativeTypeName"] = opportunity.ProposedInitiativeType?.Name ?? "",
            ["initiativeBudgetUSD"] = opportunity.InitiativeBudgetUSD?.ToString("N2") ?? "",
            ["targetSigningDate"] = opportunity.TargetSigningDate?.ToString("yyyy-MM-dd") ?? "",
            ["targetDeliveryDate"] = opportunity.TargetDeliveryDate?.ToString("yyyy-MM-dd") ?? "",
            ["resultsFocus"] = opportunity.ResultsFocus ?? "",
            ["intendedImpactOutcomes"] = opportunity.IntendedImpactOutcomes ?? "",
            ["expectedBeneficiaries"] = opportunity.ExpectedBeneficiaries ?? "",
            ["fundingPartners"] = string.Join(", ", fundingPartners),
            ["clientPartners"] = string.Join(", ", clientPartners),
            ["stakeholders"] = string.Join(", ", stakeholders),
            ["deliverables"] = string.Join(", ", deliverables),
            ["countries"] = string.Join(", ", countries),
            ["sdGs"] = string.Join(", ", sdgs),
            ["stats.totalFundingPartners"] = stats.FundingPartnerCount.ToString(),
            ["stats.totalClientPartners"] = stats.ClientPartnerCount.ToString(),
            ["stats.totalStakeholders"] = stats.StakeholderCount.ToString(),
            ["stats.totalDeliverables"] = stats.DeliverableCount.ToString(),
            ["stats.totalCountries"] = stats.CountryCount.ToString(),
            ["stats.totalSDGs"] = stats.SDGCount.ToString(),
            ["createdDate"] = opportunity.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
            ["lastModifiedDate"] = opportunity.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
        };
    }

    /// <summary>
    /// Gets basic entity information for authorization and permission checks
    /// Required by BaseUNOPSManager abstract method
    /// </summary>
    /// <param name="entityId">Opportunity ID</param>
    /// <param name="user">Current user context</param>
    /// <returns>Basic entity information as object</returns>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        var opportunity = await opportunityRepository.GetByIdAsync(entityId);
        return opportunity != null ? new { Id = opportunity.Id, Name = opportunity.Name } : null;
    }

    /// <summary>
    /// Gets comprehensive opportunity data for embedding generation and semantic search
    /// Includes all essential fields that define the opportunity's purpose, scope, and context
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    /// <returns>OpportunityModel with all semantic search-relevant data</returns>
    public override async Task<object> GetBasicEntityDataAsync(int id)
    {
        var opportunity = await context.Opportunities
            .Include(o => o.WorkflowStage)
            .Include(o => o.ResponsibleOrgUnit)
            .Include(o => o.ProposedInitiativeType)
            .Include(o => o.FundingPartners)
                .ThenInclude(fp => fp.Partner)
            .Include(o => o.FundingPartners)
                .ThenInclude(fp => fp.Currency)
            .Include(o => o.ClientPartners)
                .ThenInclude(cp => cp.Partner)
            .Include(o => o.Stakeholders)
                .ThenInclude(s => s.EntityRole)
            .Include(o => o.Stakeholders)
                .ThenInclude(s => s.User)
                    .ThenInclude(u => u!.UserProfile)
            .Include(o => o.Deliverables)
                .ThenInclude(d => d.Output)
            .Include(o => o.Countries)
                .ThenInclude(c => c.Country)
            .Include(o => o.SDGs)
                .ThenInclude(s => s.SDG)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

        if (opportunity != null)
        {
            var model = mapper.Map<OpportunityModel>(opportunity);
            
            // Compute statistics for completeness
            model.Stats = ComputeOpportunityStats(opportunity);
            
            return model;
        }
        
        return null;
    }

    /// <summary>
    /// Gets similar opportunities using semantic search based on embeddings
    /// </summary>
    public async Task<SimilarOpportunitiesResponse> GetSimilarOpportunitiesAsync(int id, int maxResults = 6, ClaimsPrincipal? user = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Call the semantic_search_entity PostgreSQL function
            using var connection = new Npgsql.NpgsqlConnection(uNOPSAppDbContext.Database.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            
            // Use the semantic_search_entity function
            // Parameters: entity_name, current_entity_id, max_results, similarity_threshold
            command.CommandText = "SELECT public.semantic_search_entity($1, $2, $3, $4)";
            command.Parameters.Add(new Npgsql.NpgsqlParameter { Value = "Opportunities" });
            command.Parameters.Add(new Npgsql.NpgsqlParameter { Value = id });
            command.Parameters.Add(new Npgsql.NpgsqlParameter { Value = maxResults });
            command.Parameters.Add(new Npgsql.NpgsqlParameter { Value = 0.15f }); // similarity_threshold

            var result = await command.ExecuteScalarAsync();
            var jsonResult = result?.ToString() ?? "{}";
            
            // Parse the JSON result
            var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonResult);
            var root = jsonDoc.RootElement;
            
            var similarOpportunities = new List<SimilarOpportunityModel>();
            
            // Check if embeddings exist
            if (root.TryGetProperty("hasEmbedding", out var hasEmbedding) && hasEmbedding.GetBoolean())
            {
                if (root.TryGetProperty("similarEntities", out var entities) && entities.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    // Get the entity IDs
                    var entityIds = new List<int>();
                    var relevanceScores = new Dictionary<int, double>();
                    
                    foreach (var entity in entities.EnumerateArray())
                    {
                        if (entity.TryGetProperty("entityId", out var entityId) && 
                            entity.TryGetProperty("relevancePercentage", out var relevance))
                        {
                            var oppId = entityId.GetInt32();
                            entityIds.Add(oppId);
                            relevanceScores[oppId] = relevance.GetDouble();
                        }
                    }
                    
                    // Fetch the full opportunity details from the database
                    if (entityIds.Any())
                    {
                        var opportunities = await context.Opportunities
                            .Include(o => o.WorkflowStage)
                            .Where(o => entityIds.Contains(o.Id) && !o.IsDeleted)
                            .ToListAsync();
                        
                        foreach (var opp in opportunities)
                        {
                            // Calculate duration in months if dates are available
                            int? durationMonths = null;
                            if (opp.TargetSigningDate.HasValue && opp.TargetDeliveryDate.HasValue)
                            {
                                var duration = opp.TargetDeliveryDate.Value - opp.TargetSigningDate.Value;
                                durationMonths = (int)Math.Round(duration.TotalDays / 30.0);
                            }
                            
                            similarOpportunities.Add(new SimilarOpportunityModel
                            {
                                OpportunityId = opp.Id,
                                Name = opp.Name,
                                Description = opp.Description,
                                Budget = opp.InitiativeBudgetUSD,
                                DurationMonths = durationMonths,
                                RelevanceScore = relevanceScores.GetValueOrDefault(opp.Id, 0),
                                WorkflowStage = opp.WorkflowStage?.Name
                            });
                        }
                        
                        // Sort by relevance score descending
                        similarOpportunities = similarOpportunities
                            .OrderByDescending(o => o.RelevanceScore)
                            .ToList();
                    }
                }
            }
            
            stopwatch.Stop();
            
            return new SimilarOpportunitiesResponse
            {
                SimilarOpportunities = similarOpportunities,
                TotalFound = similarOpportunities.Count,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            // Log the error and return empty result
            Console.WriteLine($"Error getting similar opportunities: {ex.Message}");
            stopwatch.Stop();
            
            return new SimilarOpportunitiesResponse
            {
                SimilarOpportunities = new List<SimilarOpportunityModel>(),
                TotalFound = 0,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <summary>
    /// Assigns the creator as the Opportunity Manager role for the opportunity
    /// </summary>
    /// <param name="opportunityId">The ID of the opportunity</param>
    /// <param name="userId">The ID of the user to assign as Opportunity Manager</param>
    public async Task AssignCreatorAsOpportunityManagerAsync(int opportunityId, int userId)
    {
        try
        {
            // Get the "Opportunity Manager" entity role
            var opportunityManagerRole = await uNOPSAppDbContext.EntityRoles
                .FirstOrDefaultAsync(er => er.EntityType == "Opportunity" && er.Name == "Opportunity Manager");

            if (opportunityManagerRole == null)
            {
                throw new InvalidOperationException("Opportunity Manager role not found in the system");
            }

            // Check if this user is already assigned as Opportunity Manager
            var existingAssignment = await uNOPSAppDbContext.Set<OpportunityStakeholder>()
                .AnyAsync(os => os.OpportunityId == opportunityId 
                    && os.UserId == userId 
                    && os.EntityRoleId == opportunityManagerRole.Id);

            if (existingAssignment)
            {
                // User is already assigned as Opportunity Manager
                return;
            }

            // Create the stakeholder assignment
            var stakeholder = new OpportunityStakeholder
            {
                OpportunityId = opportunityId,
                UserId = userId,
                EntityRoleId = opportunityManagerRole.Id,
                IsInternal = true,
                StakeholderType = "Internal",
                Notes = "Auto-assigned as Opportunity Manager (creator)"
            };

            await uNOPSAppDbContext.Set<OpportunityStakeholder>().AddAsync(stakeholder);
            await uNOPSAppDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log and rethrow so the controller can handle gracefully
            Console.WriteLine($"Error assigning creator as Opportunity Manager: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets all opportunities related to a specific partner (where partner is funding or client partner)
    /// </summary>
    /// <param name="partnerId">The ID of the partner</param>
    /// <returns>List of opportunities associated with the partner</returns>
    public async Task<IEnumerable<OpportunityModel>> GetOpportunitiesByPartnerIdAsync(int partnerId)
    {
        try
        {
            // Query opportunities where the partner is either a funding partner or client partner
            var opportunities = await uNOPSAppDbContext.Opportunities
                .Include(o => o.WorkflowStage)
                .Include(o => o.ResponsibleOrgUnit)
                .Include(o => o.ProposedInitiativeType)
                .Include(o => o.FundingPartners).ThenInclude(fp => fp.Partner)
                .Include(o => o.ClientPartners).ThenInclude(cp => cp.Partner)
                .Include(o => o.Stakeholders).ThenInclude(s => s.EntityRole)
                .Include(o => o.Deliverables)
                .Include(o => o.Countries).ThenInclude(c => c.Country)
                .Include(o => o.SDGs).ThenInclude(s => s.SDG)
                .Where(o => 
                    o.FundingPartners.Any(fp => fp.PartnerId == partnerId) ||
                    o.ClientPartners.Any(cp => cp.PartnerId == partnerId))
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            var models = opportunities.Select(o => mapper.Map<OpportunityModel>(o)).ToList();

            return models;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting opportunities for partner {partnerId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets multiple opportunities by their IDs with RBAC filtering
    /// Used by GlobalController for search results
    /// </summary>
    public override async Task<List<object>> GetByIdsAsync(int[] ids, ClaimsPrincipal user = null)
    {
        if (ids == null || ids.Length == 0)
            return new List<object>();

        var opportunities = opportunityRepository
            .GetAll([
                "WorkflowStage",
                "ResponsibleOrgUnit",
                "ProposedInitiativeType",
                "FundingPartners",
                "FundingPartners.Partner",
                "ClientPartners",
                "ClientPartners.Partner",
                "Stakeholders",
                "Stakeholders.EntityRole",
                "Stakeholders.User",
                "Deliverables",
                "Countries",
                "Countries.Country",
                "SDGs",
                "SDGs.SDG"
            ])
            .Where(o => ids.Contains(o.Id))
            .ToList();

        // Apply access control if user context is provided
        if (user != null)
        {
            var filteredData = await ApplyAccessControlFilters(opportunities.AsQueryable(), user, "read");
            if (filteredData is IEnumerable<Opportunity> opportunityList)
            {
                opportunities = opportunityList.ToList();
            }
        }

        // Map to models
        var opportunityModels = mapper.Map<List<OpportunityModel>>(opportunities);

        // Add permissions if user context is provided
        if (user != null)
        {
            foreach (var model in opportunityModels)
            {
                var sourceEntity = opportunities.FirstOrDefault(o => o.Id == model.Id);
                await MapEntityToModelWithPermissionsAsync(model, user, sourceEntity);
            }
        }

        return opportunityModels.Cast<object>().ToList();
    }

    public List<SearchFieldInfo> GetOpportunitySearchFields()
    {
        try
        {
            var fields = new List<SearchFieldInfo>
            {
                // Core Opportunity Identity fields
                new() { Field = "name", DisplayName = "label.opportunity.name", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "description", DisplayName = "label.opportunity.description", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "partnerReference", DisplayName = "label.opportunity.partnerReference", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },

                // Status field
                new() { 
                    Field = "status", 
                    DisplayName = "label.common.status", 
                    FieldType = "enum", 
                    AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" },
                    DropdownOptions = new List<DropdownOption>
                    {
                        new() { Value = "Inactive", Label = "enums.entityStatus.inactive" },
                        new() { Value = "Active", Label = "enums.entityStatus.active" },
                        new() { Value = "Closed", Label = "enums.entityStatus.closed" },
                        new() { Value = "Draft", Label = "enums.entityStatus.draft" },
                        new() { Value = "Archived", Label = "enums.entityStatus.archived" }
                    }
                },

                // Strategic Information fields
                new() { Field = "resultsFocus", DisplayName = "label.opportunity.resultsFocus", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "intendedImpactOutcomes", DisplayName = "label.opportunity.intendedImpactOutcomes", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "expectedBeneficiaries", DisplayName = "label.opportunity.expectedBeneficiaries", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },

                // Budget field
                new() { Field = "initiativeBudgetUSD", DisplayName = "label.opportunity.budgetUSD", FieldType = "number", AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq", "entityCards.operators.gt", "entityCards.operators.lt", "entityCards.operators.gte", "entityCards.operators.lte", "entityCards.operators.between" } },

                // Date fields
                new() { Field = "targetSigningDate", DisplayName = "label.opportunity.targetSigningDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "targetDeliveryDate", DisplayName = "label.opportunity.targetDeliveryDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "createdDate", DisplayName = "label.common.createdDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "lastModifiedDate", DisplayName = "label.common.lastModifiedDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },

                // Related entity fields
                new() { Field = "workflowStage.name", DisplayName = "label.opportunity.workflowStage", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "responsibleOrgUnit.name", DisplayName = "label.opportunity.responsibleOrgUnit", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "proposedInitiativeType.name", DisplayName = "label.opportunity.proposedInitiativeType", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
            };

            return fields;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting opportunity search fields: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Load partner agreements for a specific partner
    /// </summary>
    private async Task<List<PartnerAgreementInfo>> LoadPartnerAgreementsAsync(
        int partnerId, 
        DateTime? opportunityStartDate, 
        DateTime? opportunityEndDate,
        List<int> opportunityCountryIds)
    {
        var agreements = new List<PartnerAgreementInfo>();
        
        try
        {
            // ==========================================
            // SOURCE 1: Load agreements from BigQuery (via EDS)
            // ==========================================
            
            // Get partner's ERP dimension value to match with agreements
            var partner = await context.Partners
                .Where(p => p.Id == partnerId)
                .Select(p => new { p.ErpDimValue })
                .FirstOrDefaultAsync();
                
            if (partner != null && partner.ErpDimValue.HasValue)
            {
                // Convert ErpDimValue to string for matching with PartnerAgreementPartner
                var partnerNumber = partner.ErpDimValue.Value.ToString();
                
                // Load all active agreements for this partner from BigQuery
                var partnerAgreements = await context.PartnerAgreements
                    .Where(pa => pa.PartnerAgreementPartner == partnerNumber && !pa.IsDeleted)
                    .OrderByDescending(pa => pa.PartnerAgreementStartDate)
                    .ToListAsync();
                    
                foreach (var agreement in partnerAgreements)
                {
                    var agreementInfo = new PartnerAgreementInfo
                    {
                        PartnerAgreementNumber = agreement.PartnerAgreementNumber,
                        Name = agreement.Name,
                        PartnerAgreementType = agreement.PartnerAgreementType,
                        PartnerAgreementTypeDescription = agreement.PartnerAgreementTypeDescription,
                        PartnerAgreementScope = agreement.PartnerAgreementScope,
                        PartnerAgreementScopeDescription = agreement.PartnerAgreementScopeDescription,
                        StartDate = agreement.PartnerAgreementStartDate,
                        EndDate = agreement.PartnerAgreementEndDate,
                        SignedDate = agreement.PartnerAgreementSignedDate,
                        Source = "ERP" // From BigQuery
                    };
                    
                    // Check if agreement covers opportunity period
                    if (opportunityStartDate.HasValue && opportunityEndDate.HasValue &&
                        agreement.PartnerAgreementStartDate.HasValue && agreement.PartnerAgreementEndDate.HasValue)
                    {
                        agreementInfo.CoversOpportunityPeriod = 
                            agreement.PartnerAgreementStartDate <= opportunityStartDate &&
                            agreement.PartnerAgreementEndDate >= opportunityEndDate;
                            
                        agreementInfo.ExpiresBeforeOpportunityEnd = 
                            agreement.PartnerAgreementEndDate < opportunityEndDate;
                    }
                    
                    // Build service lines description
                    var serviceLines = new List<string>();
                    if (agreement.PartnerAgreementServiceLineInfrastructureFlag) serviceLines.Add("Infrastructure");
                    if (agreement.PartnerAgreementServiceLineProcurementFlag) serviceLines.Add("Procurement");
                    if (agreement.PartnerAgreementServiceLineProjectManagementFlag) serviceLines.Add("Project Management");
                    if (agreement.PartnerAgreementServiceLineFundManagementFlag) serviceLines.Add("Fund Management");
                    if (agreement.PartnerAgreementServiceLineHumanResourcesFlag) serviceLines.Add("Human Resources");
                    if (agreement.PartnerAgreementServiceLineOtherFlag) serviceLines.Add("Other");
                    
                    if (serviceLines.Any())
                    {
                        agreementInfo.ServiceLinesDescription = string.Join(", ", serviceLines);
                    }
                    
                    // Check geographic restrictions
                    if (!string.IsNullOrEmpty(agreement.PartnerAgreementCountries))
                    {
                        agreementInfo.HasGeographicRestrictions = true;
                        agreementInfo.GeographicRestrictions = agreement.PartnerAgreementCountries;
                        
                        // Check if opportunity countries match agreement restrictions
                        if (opportunityCountryIds != null && opportunityCountryIds.Any())
                        {
                            var agreementCountryCodes = agreement.PartnerAgreementCountries.Split(',')
                                .Select(c => c.Trim())
                                .ToList();
                                
                            var opportunityCountryCodes = await context.OpportunityCountries
                                .Where(oc => oc.OpportunityId == opportunityCountryIds.FirstOrDefault())
                                .Select(oc => oc.Country!.Iso2Code)
                                .ToListAsync();
                                
                            var hasMatchingCountry = opportunityCountryCodes.Any(oc => 
                                agreementCountryCodes.Contains(oc, StringComparer.OrdinalIgnoreCase));
                                
                            if (!hasMatchingCountry && opportunityCountryCodes.Any())
                            {
                                agreementInfo.WarningMessage = "This agreement has geographic restrictions that may not match the opportunity countries.";
                            }
                        }
                    }
                    else if (agreement.PartnerAgreementScope == "GLOBAL")
                    {
                        agreementInfo.HasGeographicRestrictions = false;
                        agreementInfo.GeographicRestrictions = "Global (no restrictions)";
                    }
                    
                    // Add expiry warning
                    if (agreementInfo.ExpiresBeforeOpportunityEnd)
                    {
                        agreementInfo.WarningMessage = agreementInfo.WarningMessage != null
                            ? agreementInfo.WarningMessage + " Agreement expires before opportunity end date."
                            : "Agreement expires before opportunity end date.";
                    }
                    
                    agreements.Add(agreementInfo);
                }
            }
            
            // ==========================================
            // SOURCE 2: Load Partnership Agreement documents from Partner record
            // ==========================================
            
            // Get the "Partnership Agreement" document type ID
            var partnershipAgreementDocType = await context.DocumentTypes
                .Where(dt => dt.EntityType == "Partner" && dt.Name == "Partnership Agreement")
                .Select(dt => dt.Id)
                .FirstOrDefaultAsync();
                
            if (partnershipAgreementDocType > 0)
            {
                // Load documents of type "Partnership Agreement" linked to this partner via DocumentRelationship
                var partnershipDocs = await context.Set<DocumentRelationship>()
                    .Include(dr => dr.Document)
                    .Where(dr => dr.EntityType == "Partner" 
                        && dr.EntityId == partnerId 
                        && dr.Document!.DocumentTypeId == partnershipAgreementDocType 
                        && !dr.Document.IsDeleted)
                    .Select(dr => dr.Document!)
                    .OrderByDescending(d => d.CreatedDate)
                    .ToListAsync();
                    
                foreach (var doc in partnershipDocs)
                {
                    var docAgreementInfo = new PartnerAgreementInfo
                    {
                        PartnerAgreementNumber = $"DOC-{doc.Id}", // Unique identifier for document-based agreements
                        Name = doc.Name ?? "Partnership Agreement",
                        PartnerAgreementType = "Uploaded Document",
                        PartnerAgreementTypeDescription = "Manually uploaded Partnership Agreement",
                        PartnerAgreementScope = "Unknown", // No scope info from uploaded docs
                        StartDate = doc.CreatedDate,
                        EndDate = null, // Unknown from uploaded docs
                        Source = "Document", // From partner record upload
                        DocumentId = doc.Id,
                        DocumentStoragePath = doc.StoragePath,
                        GeographicRestrictions = "Unknown (review document for details)",
                        HasGeographicRestrictions = false // Unknown, so assume no restrictions
                    };
                    
                    agreements.Add(docAgreementInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading partner agreements for partner {partnerId}: {ex.Message}");
            // Return empty list on error, don't fail the whole operation
        }
        
        return agreements;
    }
}


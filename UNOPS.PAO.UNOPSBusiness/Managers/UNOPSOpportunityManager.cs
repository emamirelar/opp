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
            .Include(o => o.Deliverables)
                .ThenInclude(d => d.Output)
                    .ThenInclude(o => o.Unit)
            .Include(o => o.Deliverables)
                .ThenInclude(d => d.Output)
                    .ThenInclude(o => o.ProjectCategory)
            .Include(o => o.Countries)
                .ThenInclude(c => c.Country)
            .Include(o => o.SDGs)
                .ThenInclude(s => s.SDG)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

        if (entity == null)
        {
            return null;
        }

        var model = mapper.Map<OpportunityModel>(entity, opt => opt.Items["Opportunity"] = entity);
        
        // Populate associated documents and DD fields for funding partners
        if (model.FundingPartners != null && model.FundingPartners.Any())
        {
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
                }
            }
        }
        
        // Populate associated documents and DD fields for client partners
        if (model.ClientPartners != null && model.ClientPartners.Any())
        {
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
                }
            }
        }
        
        // Enrich country models with organization unit hierarchy
        if (model.Countries != null && model.Countries.Any())
        {
            await EnrichCountriesWithOrgUnitHierarchyAsync(model.Countries);
        }

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
            nameof(Opportunity.SDGs)
        });

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update WHY section fields
        if (request.StrategicAlignment != null)
        {
            entity.StrategicAlignment = request.StrategicAlignment;
        }

        if (request.ExpectedBeneficiaries != null)
        {
            entity.ExpectedBeneficiaries = request.ExpectedBeneficiaries;
        }

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

        // Update SDG alignments
        if (request.SdGs != null)
        {
            // Remove existing SDGs
            if (entity.SDGs != null && entity.SDGs.Any())
            {
                context.Set<OpportunitySDG>().RemoveRange(entity.SDGs);
            }

            // Add new SDGs
            entity.SDGs = request.SdGs
                .Select(s => new OpportunitySDG
                {
                    OpportunityId = id,
                    SDGId = s.SDGId,
                    IsPrimary = s.IsPrimary,
                    Notes = s.Notes
                })
                .ToList();
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
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

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
                    DocumentId = fp.DocumentId
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
                    PartnerId = cp.PartnerId
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

        // Update Countries
        if (request.Countries != null)
        {
            // Remove existing countries
            if (opportunity.Countries != null && opportunity.Countries.Any())
            {
                context.OpportunityCountries.RemoveRange(opportunity.Countries);
            }

            // Add new countries
            opportunity.Countries = request.Countries
                .Select(c => new OpportunityCountry
                {
                    OpportunityId = id,
                    CountryId = c.CountryId,
                    SpecificAreas = c.SpecificAreas
                })
                .ToList();
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
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update dates
        opportunity.TargetSigningDate = request.TargetSigningDate;
        opportunity.TargetDeliveryDate = request.TargetDeliveryDate;

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
        if (request.StrategicAlignment != null)
        {
            entity.StrategicAlignment = request.StrategicAlignment;
        }

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

        if (request.PartnershipAgreementReference != null)
        {
            entity.PartnershipAgreementReference = request.PartnershipAgreementReference;
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
            ["strategicAlignment"] = opportunity.StrategicAlignment ?? "",
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
                    .ThenInclude(o => o.Unit)
            .Include(o => o.Deliverables)
                .ThenInclude(d => d.Output)
                    .ThenInclude(o => o.ProjectCategory)
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
}


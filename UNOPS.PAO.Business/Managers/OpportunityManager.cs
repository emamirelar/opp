using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;

namespace UNOPS.PAO.Business.Managers;

/// <summary>
/// Base OpportunityManager - Use UNOPSOpportunityManager for UNOPS-specific implementation
/// </summary>
public class OpportunityManager : IOpportunityManager
{
    private readonly IMapper mapper;
    private readonly AppDbContext context;
    private readonly DataRepository<Opportunity> opportunityRepository;

    public OpportunityManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.context = context;
        this.opportunityRepository = new DataRepository<Opportunity>(context);
    }

    public async Task<OpportunityModel> CreateOpportunityAsync(OpportunityRequest model)
    {
        var entity = mapper.Map<Opportunity>(model);

        // Handle child entities
        if (model.FundingPartners != null && model.FundingPartners.Any())
        {
            entity.FundingPartners = model.FundingPartners
                .Select(fp => mapper.Map<OpportunityFundingPartner>(fp))
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

        return mapper.Map<OpportunityModel>(entity);
    }

    public async Task<OpportunityModel?> GetOpportunityAsync(int id)
    {
        var includes = new[]
        {
            "WorkflowStage",
            "ResponsibleOrgUnit",
            "ProposedInitiativeType",
            "FundingPartners.Partner",
            "FundingPartners.Currency",
            "ClientPartners.Partner",
            "Stakeholders.User",
            "Stakeholders.Contact",
            "Stakeholders.EntityRole",
            "Deliverables.Output.Unit",
            "Deliverables.Output.ProjectCategory",
            "Countries.Country",
            "SDGs.SDG"
        };

        var entity = await opportunityRepository.GetByIdAsync(id, includes);

        if (entity == null)
        {
            return null;
        }

        return mapper.Map<OpportunityModel>(entity);
    }

    public async Task<IEnumerable<OpportunityModel>> GetAllOpportunitiesAsync()
    {
        var entities = await context.Opportunities
            .Include(o => o.WorkflowStage)
            .Include(o => o.ResponsibleOrgUnit)
            .Where(o => !o.IsDeleted)
            .ToListAsync();

        return entities.Select(e => mapper.Map<OpportunityModel>(e));
    }

    public async Task<OpportunityModel?> UpdateOpportunityAsync(UpdateOpportunityRequest model)
    {
        var includes = new[]
        {
            "FundingPartners",
            "ClientPartners",
            "Stakeholders",
            "Deliverables",
            "Countries",
            "SDGs"
        };

        var entity = await opportunityRepository.GetByIdAsync(model.Id, includes);

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

        return mapper.Map<OpportunityModel>(entity);
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
        return await GetOpportunityAsync(entity.Id) ?? throw new InvalidOperationException("Failed to reload opportunity after update");
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
        return await GetOpportunityAsync(entity.Id) ?? throw new InvalidOperationException("Failed to reload opportunity after update");
    }

    public async Task<OpportunityModel> UpdateWhoSectionAsync(int id, WhoSectionRequest request)
    {
        var entity = await opportunityRepository.GetByIdAsync(id, new[]
        {
            nameof(Opportunity.FundingPartners),
            nameof(Opportunity.ClientPartners),
            nameof(Opportunity.Stakeholders)
        });

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update Funding Partners
        if (request.FundingPartners != null)
        {
            // Remove existing funding partners
            if (entity.FundingPartners != null && entity.FundingPartners.Any())
            {
                context.Set<OpportunityFundingPartner>().RemoveRange(entity.FundingPartners);
            }

            // Get a valid currency ID (preferably USD, or the first available)
            var defaultCurrencyId = context.Set<Currency>()
                .Where(c => c.Code == "USD")
                .Select(c => c.Id)
                .FirstOrDefault();
            
            if (defaultCurrencyId == 0)
            {
                // Fallback to first available currency
                defaultCurrencyId = context.Set<Currency>()
                    .Select(c => c.Id)
                    .FirstOrDefault();
            }

            // Add new funding partners
            entity.FundingPartners = request.FundingPartners
                .Select(fp => new OpportunityFundingPartner
                {
                    OpportunityId = id,
                    PartnerId = fp.PartnerId,
                    Amount = fp.Amount,
                    CurrencyId = fp.CurrencyId ?? defaultCurrencyId, // Use provided or default currency
                    Percentage = fp.Percentage,
                    FeePercentage = fp.FeePercentage,
                    FeeAmount = fp.FeeAmount,
                    FeeAmountUSD = fp.FeeAmountUSD,
                    IsAmountBasedFee = fp.IsAmountBasedFee,
                    PartnershipAgreementReference = fp.PartnershipAgreementReference
                })
                .ToList();
        }

        // Update Client Partners
        if (request.ClientPartners != null)
        {
            // Remove existing client partners
            if (entity.ClientPartners != null && entity.ClientPartners.Any())
            {
                context.Set<OpportunityClientPartner>().RemoveRange(entity.ClientPartners);
            }

            // Add new client partners
            entity.ClientPartners = request.ClientPartners
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
            if (entity.Stakeholders != null && entity.Stakeholders.Any())
            {
                context.Set<OpportunityStakeholder>().RemoveRange(entity.Stakeholders);
            }

            // Add new stakeholders
            entity.Stakeholders = request.Stakeholders
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

        await opportunityRepository.UpdateAsync(entity);

        // Reload with all includes for complete response
        return await GetOpportunityAsync(entity.Id) ?? throw new InvalidOperationException("Failed to reload opportunity after update");
    }

    public async Task<OpportunityModel> UpdateWhereSectionAsync(int id, WhereSectionRequest request)
    {
        var entity = await opportunityRepository.GetByIdAsync(id, new[]
        {
            nameof(Opportunity.Countries)
        });

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update Countries
        if (request.Countries != null)
        {
            // Remove existing countries
            if (entity.Countries != null && entity.Countries.Any())
            {
                context.Set<OpportunityCountry>().RemoveRange(entity.Countries);
            }

            // Add new countries
            entity.Countries = request.Countries
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
        return await GetOpportunityAsync(entity.Id) ?? throw new InvalidOperationException("Failed to reload opportunity after update");
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
        var entity = await opportunityRepository.GetByIdAsync(id);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Update dates
        entity.TargetSigningDate = request.TargetSigningDate;
        entity.TargetDeliveryDate = request.TargetDeliveryDate;

        await context.SaveChangesAsync();

        // Reload with all includes
        return await GetOpportunityAsync(entity.Id) ?? throw new InvalidOperationException("Failed to reload opportunity after update");
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
}


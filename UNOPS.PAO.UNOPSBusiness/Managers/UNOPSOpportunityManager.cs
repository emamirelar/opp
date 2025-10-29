using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSOpportunityManager : IOpportunityManager
{
    private readonly IMapper mapper;
    private readonly AppDbContext context;
    private readonly DataRepository<Opportunity> opportunityRepository;

    public UNOPSOpportunityManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.context = context;
        this.opportunityRepository = new DataRepository<Opportunity>(context);
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
            .Include(o => o.Stakeholders)
                .ThenInclude(s => s.Contact)
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

        var model = mapper.Map<OpportunityModel>(entity);

        // Compute statistics
        model.Stats = ComputeOpportunityStats(entity);

        return model;
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

        // Calculate total funding
        if (opportunity.FundingPartners != null && opportunity.FundingPartners.Any())
        {
            stats.TotalFundingUSD = opportunity.InitiativeBudgetUSD ?? 0;
            stats.TotalFeeAmountUSD = opportunity.FundingPartners
                .Where(fp => fp.FeeAmountUSD.HasValue)
                .Sum(fp => fp.FeeAmountUSD.Value);
        }

        // Primary SDG
        stats.PrimarySDGId = opportunity.SDGs?.FirstOrDefault(s => s.IsPrimary)?.SDGId;

        return stats;
    }
}


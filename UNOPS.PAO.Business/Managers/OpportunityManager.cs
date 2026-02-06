using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Filters;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Search;

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

    #region Immutability

    /// <summary>
    /// Immutable stages - opportunities in these stages cannot be modified.
    /// GO is permanent, while NO GO and CANCELLED can be reopened (changing stage back to IDENTIFY &amp; PROFILE).
    /// </summary>
    protected static readonly string[] ImmutableStages = { "GO", "NO GO", "CANCELLED" };

    /// <summary>
    /// Determines if an opportunity is immutable based on its current stage.
    /// Immutable stages: GO, NO GO, CANCELLED
    /// </summary>
    /// <param name="opportunity">The opportunity entity to check</param>
    /// <returns>True if the opportunity is in an immutable stage</returns>
    protected bool IsOpportunityImmutable(Opportunity opportunity)
    {
        return IsOpportunityImmutable(opportunity?.Stage);
    }

    /// <summary>
    /// Determines if an opportunity is immutable based on its stage value.
    /// Immutable stages: GO, NO GO, CANCELLED
    /// </summary>
    /// <param name="stage">The stage value to check</param>
    /// <returns>True if the stage is an immutable stage</returns>
    protected bool IsOpportunityImmutable(string? stage)
    {
        if (string.IsNullOrEmpty(stage)) return false;
        return ImmutableStages.Contains(stage, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Throws a BusinessException if the opportunity is in an immutable stage.
    /// Call this at the start of any modification method.
    /// </summary>
    /// <param name="opportunity">The opportunity to validate</param>
    /// <exception cref="BusinessException">Thrown when opportunity is immutable</exception>
    protected void ThrowIfImmutable(Opportunity opportunity)
    {
        if (IsOpportunityImmutable(opportunity))
        {
            throw new BusinessException("This opportunity record is locked and cannot be modified after a decision has been made.");
        }
    }

    #endregion

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
            // "WorkflowStage" removed - now using Stage property instead
            "ResponsibleOrgUnit",
            "ProposedInitiativeType",
            "FundingPartners.Partner",
            "FundingPartners.Currency",
            "FundingPartners.Document",
            "ClientPartners.Partner",
            "ClientPartners.Document",
            "Stakeholders.User.UserProfile",
            "Stakeholders.Contact",
            "Stakeholders.EntityRole",
            "Stakeholders.OrganizationHierarchy",
            "Collaborators.User.UserProfile",
            "Collaborators.AddedByUser.UserProfile",
            "Deliverables.Output.Unit",
            "Deliverables.Output.ProjectCategory",
            "Countries.Country",
            "SDGs.SDG",
            "SDGs.Targets.SDGTarget",
            "SDGs.Targets.Indicators.SDGIndicator",
            "CreatedByUser.UserProfile",
            "LastModifiedByUser.UserProfile"
        };

        var entity = await opportunityRepository.GetByIdAsync(id, includes);

        if (entity == null)
        {
            return null;
        }

        var model = mapper.Map<OpportunityModel>(entity);
        
        // Enrich country models with organization unit hierarchy and UNCF outcome counts
        if (model.Countries != null && model.Countries.Any())
        {
            await EnrichCountriesWithOrgUnitHierarchyAsync(model.Countries);
            await EnrichCountriesWithActiveUNCFAsync(model.Countries);
        }

        return model;
    }
    
    /// <summary>
    /// Gets an opportunity by ID with user-specific permissions
    /// NOTE: This is a stub implementation. Use UNOPSOpportunityManager for full permission support.
    /// </summary>
    public virtual async Task<OpportunityModel?> GetOpportunityAsync(System.Security.Claims.ClaimsPrincipal user, int id)
    {
        // Base implementation just returns the opportunity without permissions
        return await GetOpportunityAsync(id);
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

    public async Task<IEnumerable<OpportunityModel>> GetAllOpportunitiesAsync()
    {
        var entities = await context.Opportunities
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

        // Check immutability before any modifications
        ThrowIfImmutable(entity);

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

    public async Task<OpportunityModel> UpdateOverviewSectionAsync(int id, OverviewSectionRequest request)
    {
        var entity = await opportunityRepository.GetByIdAsync(id);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Check immutability before any modifications
        ThrowIfImmutable(entity);

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
        return await GetOpportunityAsync(entity.Id) ?? throw new InvalidOperationException("Failed to reload opportunity after update");
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

        // Check immutability before any modifications
        ThrowIfImmutable(entity);

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

        // Check immutability before any modifications
        ThrowIfImmutable(entity);

        // Update WHY section fields

        if (request.ExpectedBeneficiaries != null)
        {
            entity.ExpectedBeneficiaries = request.ExpectedBeneficiaries;
        }

        if (request.ExpectedImpact != null)
        {
            entity.ExpectedImpact = request.ExpectedImpact;
        }

        if (request.ExpectedOutcomes != null)
        {
            entity.ExpectedOutcomes = request.ExpectedOutcomes;
        }

        if (request.Challenges != null)
        {
            entity.Challenges = request.Challenges;
        }

        // Update SDG alignments with differential update strategy
        if (request.SdGs != null)
        {
            // Load existing SDGs with their targets and indicators for comparison
            // CRITICAL: Filter out soft-deleted records to avoid re-selection issues
            var existingSDGs = await context.Set<OpportunitySDG>()
                .Where(sdg => sdg.OpportunityId == id && !sdg.IsDeleted)
                .Include(sdg => sdg.Targets.Where(t => !t.IsDeleted))
                    .ThenInclude(t => t.Indicators.Where(i => !i.IsDeleted))
                .ToListAsync();

            var requestedSDGIds = request.SdGs.Select(s => s.SDGId).ToHashSet();

            // Remove SDGs that are no longer in the request
            var sdgsToRemove = existingSDGs.Where(s => !requestedSDGIds.Contains(s.SDGId)).ToList();
            if (sdgsToRemove.Any())
            {
                context.Set<OpportunitySDG>().RemoveRange(sdgsToRemove);
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
                        Notes = sdgRequest.Notes
                    };

                    // Add targets
                    if (sdgRequest.Targets != null && sdgRequest.Targets.Any())
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

                    context.Set<OpportunitySDG>().Add(newSDG);
                }
                else
                {
                    // Update existing SDG properties
                    existingSDG.IsPrimary = sdgRequest.IsPrimary;
                    existingSDG.Notes = sdgRequest.Notes;

                    // Update targets with differential strategy
                    var requestedTargetIds = sdgRequest.Targets?.Select(t => t.SDGTargetDatabaseId).ToHashSet() ?? new HashSet<int>();

                    // Remove targets that are no longer in the request
                    var targetsToRemove = existingSDG.Targets.Where(t => !requestedTargetIds.Contains(t.SDGTargetId)).ToList();
                    if (targetsToRemove.Any())
                    {
                        foreach (var target in targetsToRemove)
                        {
                            existingSDG.Targets.Remove(target);
                            context.Set<OpportunitySDGTarget>().Remove(target);
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

                                // Remove indicators that are no longer in the request
                                var indicatorsToRemove = existingTarget.Indicators.Where(i => !requestedIndicatorIds.Contains(i.SDGIndicatorId)).ToList();
                                if (indicatorsToRemove.Any())
                                {
                                    foreach (var indicator in indicatorsToRemove)
                                    {
                                        existingTarget.Indicators.Remove(indicator);
                                        context.Set<OpportunitySDGIndicator>().Remove(indicator);
                                    }
                                }

                                // Add new indicators
                                if (targetRequest.SDGIndicatorDatabaseIds != null)
                                {
                                    foreach (var indicatorId in targetRequest.SDGIndicatorDatabaseIds)
                                    {
                                        if (!existingTarget.Indicators.Any(i => i.SDGIndicatorId == indicatorId))
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
                }
            }
        }

        // Update UNCF Outcome alignments with differential update strategy
        if (request.UncfOutcomes != null)
        {
            // Load existing UNCF outcomes with their indicators for comparison
            // CRITICAL: Filter out soft-deleted records to avoid re-selection issues
            var existingUNCFOutcomes = await context.Set<OpportunityUNCFOutcome>()
                .Where(uo => uo.OpportunityId == id && !uo.IsDeleted)
                .Include(uo => uo.Indicators.Where(i => !i.IsDeleted))
                .ToListAsync();

            // Group request by (OpportunityCountryId, UNCFOutcomeId) composite key
            var requestedKeys = request.UncfOutcomes
                .Select(u => (u.OpportunityCountryId, u.UNCFOutcomeId))
                .ToHashSet();

            // Remove UNCF outcomes that are no longer in the request
            var uncfOutcomesToRemove = existingUNCFOutcomes
                .Where(uo => !requestedKeys.Contains((uo.OpportunityCountryId, uo.UNCFOutcomeId)))
                .ToList();
            if (uncfOutcomesToRemove.Any())
            {
                context.Set<OpportunityUNCFOutcome>().RemoveRange(uncfOutcomesToRemove);
            }

            // Process each requested UNCF outcome
            foreach (var uncfOutcomeRequest in request.UncfOutcomes)
            {
                var existingUNCFOutcome = existingUNCFOutcomes.FirstOrDefault(uo => 
                    uo.OpportunityCountryId == uncfOutcomeRequest.OpportunityCountryId && 
                    uo.UNCFOutcomeId == uncfOutcomeRequest.UNCFOutcomeId);

                if (existingUNCFOutcome == null)
                {
                    // Add new UNCF outcome with its indicators
                    var newUNCFOutcome = new OpportunityUNCFOutcome
                    {
                        OpportunityId = id,
                        OpportunityCountryId = uncfOutcomeRequest.OpportunityCountryId,
                        UNCFOutcomeId = uncfOutcomeRequest.UNCFOutcomeId,
                        Notes = uncfOutcomeRequest.Notes
                    };

                    // Add indicators
                    if (uncfOutcomeRequest.UNCFIndicatorIds != null && uncfOutcomeRequest.UNCFIndicatorIds.Any())
                    {
                        foreach (var indicatorId in uncfOutcomeRequest.UNCFIndicatorIds)
                        {
                            newUNCFOutcome.Indicators.Add(new OpportunityUNCFIndicator
                            {
                                OpportunityId = id,
                                UNCFIndicatorId = indicatorId
                            });
                        }
                    }

                    context.Set<OpportunityUNCFOutcome>().Add(newUNCFOutcome);
                }
                else
                {
                    // Update existing UNCF outcome properties
                    existingUNCFOutcome.Notes = uncfOutcomeRequest.Notes;

                    // Update indicators with differential strategy
                    var requestedIndicatorIds = uncfOutcomeRequest.UNCFIndicatorIds?.ToHashSet() ?? new HashSet<int>();

                    // Remove indicators that are no longer in the request
                    var indicatorsToRemove = existingUNCFOutcome.Indicators
                        .Where(i => !requestedIndicatorIds.Contains(i.UNCFIndicatorId))
                        .ToList();
                    if (indicatorsToRemove.Any())
                    {
                        foreach (var indicator in indicatorsToRemove)
                        {
                            existingUNCFOutcome.Indicators.Remove(indicator);
                            context.Set<OpportunityUNCFIndicator>().Remove(indicator);
                        }
                    }

                    // Add new indicators
                    if (uncfOutcomeRequest.UNCFIndicatorIds != null)
                    {
                        foreach (var indicatorId in uncfOutcomeRequest.UNCFIndicatorIds)
                        {
                            if (!existingUNCFOutcome.Indicators.Any(i => i.UNCFIndicatorId == indicatorId))
                            {
                                existingUNCFOutcome.Indicators.Add(new OpportunityUNCFIndicator
                                {
                                    OpportunityId = id,
                                    OpportunityUNCFOutcomeId = existingUNCFOutcome.Id,
                                    UNCFIndicatorId = indicatorId
                                });
                            }
                        }
                    }
                }
            }
        }

        await opportunityRepository.UpdateAsync(entity);

        // Reload with all includes for complete response  
        var reloadedEntity = await opportunityRepository.GetByIdAsync(entity.Id, new[]
        {
            $"{nameof(Opportunity.SDGs)}.{nameof(OpportunitySDG.Targets)}.{nameof(OpportunitySDGTarget.Indicators)}",
            $"{nameof(Opportunity.UNCFOutcomes)}.{nameof(OpportunityUNCFOutcome.Indicators)}",
            nameof(Opportunity.FundingPartners),
            nameof(Opportunity.ClientPartners),
            nameof(Opportunity.Stakeholders),
            nameof(Opportunity.Deliverables),
            nameof(Opportunity.Countries)
        });
        
        if (reloadedEntity == null)
        {
            throw new InvalidOperationException("Failed to reload opportunity after update");
        }
        
        return mapper.Map<OpportunityModel>(reloadedEntity);
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

        // Check immutability before any modifications
        ThrowIfImmutable(entity);

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

        // Note: Internal stakeholders are now managed in the Team section (UpdateTeamSectionAsync)

        await opportunityRepository.UpdateAsync(entity);

        // Reload with all includes for complete response
        return await GetOpportunityAsync(entity.Id) ?? throw new InvalidOperationException("Failed to reload opportunity after update");
    }

    public async Task<OpportunityModel> UpdateTeamSectionAsync(int id, TeamSectionRequest request)
    {
        var entity = await opportunityRepository.GetByIdAsync(id, new[]
        {
            nameof(Opportunity.Stakeholders),
            nameof(Opportunity.Collaborators)
        });

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Check immutability before any modifications
        ThrowIfImmutable(entity);

        // Track if org unit changed
        var orgUnitChanged = request.ResponsibleOrgUnitId.HasValue && 
                            request.ResponsibleOrgUnitId.Value != entity.ResponsibleOrgUnitId;

        // Update Responsible Org Unit
        if (request.ResponsibleOrgUnitId.HasValue)
        {
            entity.ResponsibleOrgUnitId = request.ResponsibleOrgUnitId.Value;
        }

        // Update Initiative Type
        if (request.ProposedInitiativeTypeId.HasValue)
        {
            entity.ProposedInitiativeTypeId = request.ProposedInitiativeTypeId.Value;
        }

        // Update Internal Stakeholders (Team & Stakeholders) using differential update
        if (request.Stakeholders != null)
        {
            // Get Opportunity Manager role ID - Opportunity Manager is managed separately via request.OpportunityManagerId
            var opportunityManagerRoleId = await context.Set<EntityRole>()
                .Where(er => er.Name != null && er.Name.ToLower() == "opportunity manager" && er.EntityType == "Opportunity" && !er.IsDeleted)
                .Select(er => er.Id)
                .FirstOrDefaultAsync();

            // Filter out Opportunity Manager role from stakeholders (it's handled separately via OpportunityManagerId)
            var filteredStakeholders = request.Stakeholders
                .Where(s => s.EntityRoleId != opportunityManagerRoleId)
                .ToList();

            // Get entity roles to check AllowsMultiple property
            var entityRoleIds = filteredStakeholders.Select(s => s.EntityRoleId).Distinct().ToList();
            var entityRoles = await context.Set<EntityRole>()
                .Where(er => entityRoleIds.Contains(er.Id))
                .ToDictionaryAsync(er => er.Id);

            // Validate that single-assignment roles don't have duplicates for user-based stakeholders
            var userBasedStakeholders = filteredStakeholders.Where(s => s.UserId.HasValue).ToList();
            var roleGroups = userBasedStakeholders
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

            entity.Stakeholders ??= new List<OpportunityStakeholder>();

            // Separate user-based stakeholders from the request (exclude auto-populated ones and Opportunity Manager role)
            var requestedUserStakeholders = filteredStakeholders
                .Where(s => s.UserId.HasValue && !s.OrganizationHierarchyId.HasValue)
                .ToList();

            // Get existing user-based stakeholders (not auto-populated, not Opportunity Manager)
            var existingUserStakeholders = entity.Stakeholders
                .Where(s => s.UserId.HasValue && !s.OrganizationHierarchyId.HasValue && s.EntityRoleId != opportunityManagerRoleId)
                .ToList();

            // Find stakeholders to remove (exist in DB but not in request)
            var stakeholdersToRemove = existingUserStakeholders
                .Where(existing => !requestedUserStakeholders.Any(req => 
                    req.UserId == existing.UserId && req.EntityRoleId == existing.EntityRoleId))
                .ToList();

            // Find stakeholders to add (exist in request but not in DB)
            var stakeholdersToAdd = requestedUserStakeholders
                .Where(req => !existingUserStakeholders.Any(existing => 
                    existing.UserId == req.UserId && existing.EntityRoleId == req.EntityRoleId))
                .ToList();

            // Find stakeholders to update (exist in both - update notes if changed)
            var stakeholdersToUpdate = existingUserStakeholders
                .Where(existing => requestedUserStakeholders.Any(req => 
                    req.UserId == existing.UserId && req.EntityRoleId == existing.EntityRoleId))
                .ToList();

            // Remove stakeholders that are no longer in the request
            foreach (var stakeholder in stakeholdersToRemove)
            {
                entity.Stakeholders.Remove(stakeholder);
                context.Set<OpportunityStakeholder>().Remove(stakeholder);
            }

            // Add new stakeholders
            foreach (var req in stakeholdersToAdd)
            {
                entity.Stakeholders.Add(new OpportunityStakeholder
                {
                    OpportunityId = id,
                    UserId = req.UserId,
                    EntityRoleId = req.EntityRoleId,
                    OrganizationHierarchyId = null,
                    IsInternal = true,
                    StakeholderType = "Internal",
                    Notes = req.Notes
                });
            }

            // Update existing stakeholders (notes field)
            foreach (var existing in stakeholdersToUpdate)
            {
                var req = requestedUserStakeholders.First(r => 
                    r.UserId == existing.UserId && r.EntityRoleId == existing.EntityRoleId);
                if (existing.Notes != req.Notes)
                {
                    existing.Notes = req.Notes;
                }
            }
        }

        // Update Opportunity Manager (from stakeholders with "Opportunity Manager" role)
        if (request.OpportunityManagerId.HasValue)
        {
            // Get the Opportunity Manager role
            var opportunityManagerRole = await context.Set<EntityRole>()
                .FirstOrDefaultAsync(er => er.Name != null && er.Name.ToLower() == "opportunity manager" && er.EntityType == "Opportunity");
            
            if (opportunityManagerRole != null)
            {
                // Remove existing opportunity manager stakeholder
                var existingManager = entity.Stakeholders?
                    .FirstOrDefault(s => s.EntityRoleId == opportunityManagerRole.Id && s.UserId.HasValue);
                
                if (existingManager != null)
                {
                    entity.Stakeholders!.Remove(existingManager);
                    context.Set<OpportunityStakeholder>().Remove(existingManager);
                }
                
                // Add new opportunity manager stakeholder
                entity.Stakeholders ??= new List<OpportunityStakeholder>();
                entity.Stakeholders.Add(new OpportunityStakeholder
                {
                    OpportunityId = id,
                    UserId = request.OpportunityManagerId.Value,
                    EntityRoleId = opportunityManagerRole.Id,
                    IsInternal = true,
                    StakeholderType = "Internal",
                    OrganizationHierarchyId = null
                });
            }
        }

        // Auto-populate stakeholders from EntityUserRoles if org unit changed
        if (orgUnitChanged && request.ResponsibleOrgUnitId.HasValue)
        {
            await AutoPopulateStakeholdersFromOrgUnitAsync(entity, request.ResponsibleOrgUnitId.Value);
        }

        await opportunityRepository.UpdateAsync(entity);

        // Reload with all includes for complete response
        return await GetOpportunityAsync(entity.Id) ?? throw new InvalidOperationException("Failed to reload opportunity after update");
    }

    /// <summary>
    /// Auto-populates stakeholders from EntityUserRoles when the org unit is of type "OrgUnit".
    /// Uses differential update - only adds/removes what's necessary.
    /// Also removes old auto-populated stakeholders when switching to a non-OrgUnit type.
    /// </summary>
    protected virtual async Task AutoPopulateStakeholdersFromOrgUnitAsync(Opportunity entity, int orgUnitId)
    {
        // Get the org unit to check its type
        var orgUnit = await context.OrganizationHierarchies
            .Where(oh => oh.Id == orgUnitId && !oh.IsDeleted)
            .Select(oh => new { oh.Id, oh.Type })
            .FirstOrDefaultAsync();

        entity.Stakeholders ??= new List<OpportunityStakeholder>();

        // Get existing auto-populated stakeholders
        var existingAutoPopulated = entity.Stakeholders
            .Where(s => s.OrganizationHierarchyId.HasValue)
            .ToList();

        // If the new org unit is not of type "OrgUnit", remove all auto-populated stakeholders
        if (orgUnit == null || orgUnit.Type != Domain.Enums.OrganizationUnitType.OrgUnit)
        {
            foreach (var stakeholder in existingAutoPopulated)
            {
                entity.Stakeholders.Remove(stakeholder);
                context.Set<OpportunityStakeholder>().Remove(stakeholder);
            }
            return;
        }

        // Build list of org units to get EntityUserRoles from
        var orgUnitIdsForRoles = new List<int> { orgUnitId };

        // ALWAYS add normally responsible org units (if different from selected)
        // These are the org units normally responsible for implementation countries
        var normallyResponsibleOrgUnits = await GetNormallyResponsibleOrgUnitsAsync(entity.Id, orgUnitId);
        orgUnitIdsForRoles.AddRange(normallyResponsibleOrgUnits);

        // Get EntityUserRoles for this org unit and normally responsible org units
        var entityUserRoles = await context.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" 
                       && orgUnitIdsForRoles.Contains(eur.EntityId)
                       && eur.EntityRoleId.HasValue
                       && !eur.IsDeleted)
            .Select(eur => new { eur.EntityId, EntityRoleId = eur.EntityRoleId!.Value })
            .Distinct()
            .ToListAsync();

        // Create a set of valid (OrgUnitId, RoleId) combinations
        var validCombinations = entityUserRoles
            .Select(e => (e.EntityId, e.EntityRoleId))
            .ToHashSet();

        // Find auto-populated stakeholders to remove:
        // - Those not in the valid combinations
        var autoPopulatedToRemove = existingAutoPopulated
            .Where(existing => 
                !existing.OrganizationHierarchyId.HasValue ||
                !validCombinations.Contains((existing.OrganizationHierarchyId.Value, existing.EntityRoleId)))
            .ToList();

        // Find combinations to add (exist in EntityUserRoles but not in existing auto-populated)
        var existingCombinations = existingAutoPopulated
            .Where(s => s.OrganizationHierarchyId.HasValue)
            .Select(s => (s.OrganizationHierarchyId!.Value, s.EntityRoleId))
            .ToHashSet();

        var combinationsToAdd = validCombinations
            .Where(combo => !existingCombinations.Contains(combo))
            .ToList();

        // Remove stakeholders that are no longer needed
        foreach (var stakeholder in autoPopulatedToRemove)
        {
            entity.Stakeholders.Remove(stakeholder);
            context.Set<OpportunityStakeholder>().Remove(stakeholder);
        }

        // Add new auto-populated stakeholders
        foreach (var (targetOrgUnitId, roleId) in combinationsToAdd)
        {
            entity.Stakeholders.Add(new OpportunityStakeholder
            {
                OpportunityId = entity.Id,
                EntityRoleId = roleId,
                OrganizationHierarchyId = targetOrgUnitId,
                UserId = null, // No specific user - auto-populated
                IsInternal = true,
                StakeholderType = "Internal",
                Notes = null
            });
        }
    }

    /// <summary>
    /// Gets normally responsible org unit IDs for countries where the selected responsible org unit 
    /// is NOT normally responsible. Returns org units (Type = "OrgUnit", level 3) from country hierarchies
    /// that differ from the selected org unit.
    /// </summary>
    protected virtual async Task<List<int>> GetNormallyResponsibleOrgUnitsAsync(int opportunityId, int selectedOrgUnitId)
    {
        // Get implementation country IDs for this opportunity
        var countryIds = await context.Set<OpportunityCountry>()
            .Where(oc => oc.OpportunityId == opportunityId)
            .Select(oc => oc.CountryId)
            .ToListAsync();

        if (!countryIds.Any())
            return new List<int>();

        // Get org unit relationships for these countries
        var countryOrgUnitIds = await context.OrganizationUnitRelationships
            .Where(r => 
                r.EntityType == "Country" 
                && countryIds.Contains(r.EntityId)
                && !r.IsDeleted)
            .Select(r => r.OrganizationHierarchyId)
            .Distinct()
            .ToListAsync();

        if (!countryOrgUnitIds.Any())
            return new List<int>();

        // Get org units that are of type OrgUnit (level 3) and different from selected
        var normallyResponsibleOrgUnits = await context.OrganizationHierarchies
            .Where(oh => countryOrgUnitIds.Contains(oh.Id) 
                      && oh.Type == Domain.Enums.OrganizationUnitType.OrgUnit
                      && oh.Id != selectedOrgUnitId
                      && !oh.IsDeleted)
            .Select(oh => oh.Id)
            .Distinct()
            .ToListAsync();

        return normallyResponsibleOrgUnits;
    }

    public async Task<OpportunityModel> UpdateWhereSectionAsync(int id, WhereSectionRequest request)
    {
        var entity = await context.Opportunities
            .Include(o => o.Countries)
            .Include(o => o.Stakeholders)  // Include stakeholders for auto-population
            .FirstOrDefaultAsync(o => o.Id == id);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Check immutability before any modifications
        ThrowIfImmutable(entity);

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
                    SpecificAreas = c.SpecificAreas,
                    HumanitarianFrameworkAlignment = c.HumanitarianFrameworkAlignment,
                    NdcAlignment = c.NdcAlignment,
                    NapAlignment = c.NapAlignment,
                    OrgUnitStrategyAlignment = c.OrgUnitStrategyAlignment
                })
                .ToList();
        }

        await context.SaveChangesAsync();

        // Auto-populate stakeholders from normally responsible org units if responsible org unit is set
        // This ensures that when countries change, the normally responsible org units' role holders
        // are automatically added as internal stakeholders
        if (entity.ResponsibleOrgUnitId.HasValue)
        {
            await AutoPopulateStakeholdersFromOrgUnitAsync(entity, entity.ResponsibleOrgUnitId.Value);
            await context.SaveChangesAsync();
        }

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

        // Check immutability before any modifications
        ThrowIfImmutable(entity);

        // Validate date logic
        if (request.ImplementationStartDate.HasValue && request.TargetSigningDate.HasValue)
        {
            if (request.ImplementationStartDate.Value < request.TargetSigningDate.Value)
            {
                throw new BusinessException("Implementation Start Date cannot be before the Target Signing Date");
            }
        }

        if (request.TargetDeliveryDate.HasValue)
        {
            var effectiveStartDate = request.ImplementationStartDate ?? request.TargetSigningDate;
            if (effectiveStartDate.HasValue && request.TargetDeliveryDate.Value < effectiveStartDate.Value)
            {
                throw new BusinessException("Target Delivery Date must be after the Implementation Start Date (or Target Signing Date if no Implementation Start Date is set)");
            }
        }

        // Validate deliverable dates
        if (request.Deliverables != null && request.Deliverables.Any())
        {
            var effectiveImplementationStart = request.ImplementationStartDate ?? request.TargetSigningDate;
            
            foreach (var deliverable in request.Deliverables)
            {
                // Validate that deliverable start is not before implementation start
                if (deliverable.PlannedStartDate.HasValue && effectiveImplementationStart.HasValue)
                {
                    if (deliverable.PlannedStartDate.Value < effectiveImplementationStart.Value)
                    {
                        throw new BusinessException($"Deliverable Planned Start Date cannot be before the Implementation Start Date for deliverable ID: {deliverable.Id}");
                    }
                }
                
                // Validate that deliverable end is not before deliverable start
                if (deliverable.PlannedStartDate.HasValue && deliverable.PlannedEndDate.HasValue)
                {
                    if (deliverable.PlannedEndDate.Value < deliverable.PlannedStartDate.Value)
                    {
                        throw new BusinessException($"Deliverable Planned End Date cannot be before the Planned Start Date for deliverable ID: {deliverable.Id}");
                    }
                }
            }
        }

        // Update dates
        entity.TargetSigningDate = request.TargetSigningDate;
        entity.ImplementationStartDate = request.ImplementationStartDate;
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

        // Check immutability before any modifications
        ThrowIfImmutable(entity);

        await opportunityRepository.Delete(entity);
        return true;
    }

    /// <summary>
    /// Apply AI-extracted changes to an opportunity across multiple sections
    /// NOTE: This is a stub implementation. Use UNOPSOpportunityManager for full functionality.
    /// </summary>
    public virtual async Task<OpportunityModel> ApplyAiChangesAsync(int id, ApplyOpportunityAiChangesRequest request)
    {
        throw new NotImplementedException("ApplyAiChangesAsync is only implemented in UNOPSOpportunityManager");
    }

    /// <summary>
    /// Gets similar opportunities - UNOPS-specific implementation required
    /// </summary>
    public virtual async Task<SimilarOpportunitiesResponse> GetSimilarOpportunitiesAsync(int id, int maxResults = 6, System.Security.Claims.ClaimsPrincipal? user = null)
    {
        throw new NotImplementedException("GetSimilarOpportunitiesAsync is only implemented in UNOPSOpportunityManager");
    }

    public Task AssignCreatorAsOpportunityManagerAsync(int opportunityId, int userId)
    {
        throw new NotImplementedException("AssignCreatorAsOpportunityManagerAsync is only implemented in UNOPSOpportunityManager");
    }

    public Task<IEnumerable<OpportunityModel>> GetOpportunitiesByPartnerIdAsync(int partnerId)
    {
        throw new NotImplementedException("GetOpportunitiesByPartnerIdAsync is only implemented in UNOPSOpportunityManager");
    }

    public List<SearchFieldInfo> GetOpportunitySearchFields()
    {
        throw new NotImplementedException("GetOpportunitySearchFields is only implemented in UNOPSOpportunityManager");
    }

    public Task<bool> UpdateHighRiskAcknowledgementAsync(int opportunityId, bool acknowledged)     
    {
        throw new NotImplementedException("UpdateHighRiskAcknowledgementAsync is only implemented in UNOPSOpportunityManager");                                                                       
    }

    public Task<OpportunityModel> CreateOpportunityFromProposalAsync(CreateOpportunityFromInteractionsRequest request, int currentUserId)
    {
        throw new NotImplementedException("CreateOpportunityFromProposalAsync is only implemented in UNOPSOpportunityManager");
    }

    /// <summary>
    /// Assigns an Executive to an opportunity during Go decision approval.
    /// The Executive is typically the Director/Manager/OiC of the responsible org unit.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <param name="executiveId">The user ID of the assigned Executive</param>
    /// <exception cref="KeyNotFoundException">Thrown when opportunity is not found</exception>
    public virtual async Task AssignExecutiveAsync(int opportunityId, int executiveId)
    {
        var opportunity = await opportunityRepository.GetByIdAsync(opportunityId);
        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found");
        }

        opportunity.ExecutiveId = executiveId;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets personnel for an opportunity's responsible org unit.
    /// Used to populate the Executive dropdown in the Go Decision approval dialog.
    /// Returns all personnel with roles on the org unit, with Directors/Deputy Directors marked as "Suggested".
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <returns>List of personnel with display label and user ID</returns>
    /// <exception cref="KeyNotFoundException">Thrown when opportunity is not found</exception>
    public virtual async Task<IEnumerable<TypeaheadInput>> GetExecutivesForOpportunityAsync(int opportunityId)
    {
        // Get the opportunity to find the ResponsibleOrgUnitId
        var opportunity = await context.Opportunities
            .AsNoTracking()
            .Where(o => o.Id == opportunityId && !o.IsDeleted)
            .Select(o => new { o.Id, o.ResponsibleOrgUnitId })
            .FirstOrDefaultAsync();

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found");
        }

        if (!opportunity.ResponsibleOrgUnitId.HasValue)
        {
            return Enumerable.Empty<TypeaheadInput>();
        }

        return await GetExecutivesForOrgUnitAsync(opportunity.ResponsibleOrgUnitId.Value);
    }

    /// <summary>
    /// Gets all users in the system for executive selection.
    /// Users with Director/Deputy Director roles on the specified org unit are marked as "Suggested".
    /// </summary>
    /// <param name="orgUnitId">The organization unit ID</param>
    /// <returns>List of all users with suggested executives first</returns>
    protected virtual async Task<IEnumerable<TypeaheadInput>> GetExecutivesForOrgUnitAsync(int orgUnitId)
    {
        // Director/Deputy Director/OiC role codes that should be marked as "Suggested"
        var suggestedRoleCodes = new[]
        {
            "OrgUnit_Director_OrganizationHierarchy",
            "OrgUnit_Deputy_Director_OrganizationHierarchy",
            "OrgUnit_OiC_OrganizationHierarchy",
            "Regional_Director_OrganizationHierarchy",
            "Regional_Deputy_Director_OrganizationHierarchy",
            "MCO_Director_OrganizationHierarchy",
            "MCO_Deputy_Director_OrganizationHierarchy"
        };

        // Get users with executive roles on this org unit (to mark as "Suggested")
        var executiveRoles = await context.EntityUserRoles
            .AsNoTracking()
            .Include(e => e.EntityRole)
            .Where(e => !e.IsDeleted &&
                       e.EntityType == "OrganizationHierarchy" &&
                       e.EntityId == orgUnitId &&
                       e.EntityRole != null &&
                       e.EntityRole.Code != null &&
                       suggestedRoleCodes.Contains(e.EntityRole.Code))
            .ToListAsync();

        // Get the set of suggested user IDs and their roles
        var suggestedUserRoles = executiveRoles
            .GroupBy(e => e.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(e => e.EntityRole?.Code?.Contains("_Director_") == true && 
                                              !e.EntityRole?.Code?.Contains("Deputy") == true)
                      .First().EntityRole?.Code
            );

        // Get ALL users in the system
        var allUsers = await context.PAOUsers
            .AsNoTracking()
            .Include(u => u.UserProfile)
            .ToListAsync();

        // Map to TypeaheadInput - suggested users first, then 30 more non-suggested users
        var suggestedUsers = allUsers
            .Where(user => suggestedUserRoles.ContainsKey(user.Id))
            .Select(user => {
                var userName = user.UserProfile?.Name ?? user.Email ?? "Unknown User";
                var roleName = GetFriendlyRoleName(suggestedUserRoles[user.Id]);
                
                return new TypeaheadInput
                {
                    Label = $"{userName} ({roleName})",
                    Value = user.Id.ToString(),
                    Description = "Suggested"
                };
            })
            .OrderBy(e => e.Label)
            .ToList();

        var nonSuggestedUsers = allUsers
            .Where(user => !suggestedUserRoles.ContainsKey(user.Id))
            .Select(user => {
                var userName = user.UserProfile?.Name ?? user.Email ?? "Unknown User";
                
                return new TypeaheadInput
                {
                    Label = userName,
                    Value = user.Id.ToString(),
                    Description = null
                };
            })
            .OrderBy(e => e.Label)
            .Take(30) // Limit to 30 non-suggested users for performance
            .ToList();

        // Combine: all suggested users first, then up to 30 other users
        var result = suggestedUsers.Concat(nonSuggestedUsers).ToList();

        return result;
    }

    /// <summary>
    /// Converts role code to friendly display name.
    /// </summary>
    private static string GetFriendlyRoleName(string? roleCode)
    {
        if (string.IsNullOrEmpty(roleCode))
            return "Personnel";
            
        return roleCode switch
        {
            "OrgUnit_Director_OrganizationHierarchy" => "Director",
            "OrgUnit_Deputy_Director_OrganizationHierarchy" => "Deputy Director",
            "Regional_Director_OrganizationHierarchy" => "Regional Director",
            "Regional_Deputy_Director_OrganizationHierarchy" => "Regional Deputy Director",
            "MCO_Director_OrganizationHierarchy" => "MCO Director",
            "MCO_Deputy_Director_OrganizationHierarchy" => "MCO Deputy Director",
            "OrgUnit_Manager_OrganizationHierarchy" => "Manager",
            "OrgUnit_OiC_OrganizationHierarchy" => "OiC",
            "OrgUnit_Staff_OrganizationHierarchy" => "Staff",
            "OrgUnit_Member_OrganizationHierarchy" => "Member",
            _ => ExtractRoleNameFromCode(roleCode)
        };
    }
    
    /// <summary>
    /// Extracts a friendly role name from a role code by parsing the code structure.
    /// Example: "OrgUnit_Portfolio_Manager_OrganizationHierarchy" => "Portfolio Manager"
    /// </summary>
    private static string ExtractRoleNameFromCode(string roleCode)
    {
        // Remove common prefixes and suffixes
        var name = roleCode
            .Replace("_OrganizationHierarchy", "")
            .Replace("OrgUnit_", "")
            .Replace("Regional_", "Regional ")
            .Replace("MCO_", "MCO ")
            .Replace("_", " ");
            
        // Capitalize first letter of each word
        if (!string.IsNullOrEmpty(name))
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
        }
        
        return "Personnel";
    }
}


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
using UNOPS.PAO.Models.Filters;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Business.Mapping;
using UNOPS.PAO.UNOPSBusiness.Services;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSOpportunityManager : BaseUNOPSManager, IOpportunityManager
{
    private readonly IMapper mapper;
    private readonly AppDbContext context;
    private readonly UNOPSAppDbContext uNOPSAppDbContext;
    private readonly BaseRepository<Opportunity> opportunityRepository;
    private readonly IServiceProvider _serviceProvider; 
    private readonly IConfiguration configuration;
    private readonly IDbContextFactory<UNOPSAppDbContext> _dbContextFactory;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IHttpContextAccessor httpContextAccessor;

    public UNOPSOpportunityManager(
        IMapper mapper,
        AppDbContext context,
        IConfiguration configuration,
        IDbContextFactory<UNOPSAppDbContext> dbContextFactory,
        IExchangeRateService exchangeRateService,
        IPermissionService permissionService = null,
        IHttpContextAccessor httpContextAccessor = null,
        IServiceProvider serviceProvider = null)
        : base(mapper, context as UNOPSAppDbContext, configuration, null, "Opportunity", permissionService, httpContextAccessor)
    {
        this.mapper = mapper;
        this.context = context;
        this.uNOPSAppDbContext = context as UNOPSAppDbContext;
        this._serviceProvider = serviceProvider;
        this.configuration = configuration;
        this._dbContextFactory = dbContextFactory;
        this._exchangeRateService = exchangeRateService;
        this.httpContextAccessor = httpContextAccessor;
        this.opportunityRepository = new BaseRepository<Opportunity>(this.uNOPSAppDbContext, configuration, serviceProvider);
    }

    #region Immutability

    /// <summary>
    /// Immutable stages - opportunities in these stages cannot be modified.
    /// GO is permanent, while NO GO and CANCELLED can be reopened (changing stage back to IDENTIFY &amp; PROFILE).
    /// </summary>
    private static readonly string[] ImmutableStages = { "GO", "NO GO", "CANCELLED" };

    /// <summary>
    /// Determines if an opportunity is immutable based on its current stage.
    /// Immutable stages: GO, NO GO, CANCELLED
    /// </summary>
    /// <param name="opportunity">The opportunity entity to check</param>
    /// <returns>True if the opportunity is in an immutable stage</returns>
    private bool IsOpportunityImmutable(Opportunity opportunity)
    {
        return IsOpportunityImmutable(opportunity?.Stage);
    }

    /// <summary>
    /// Determines if an opportunity is immutable based on its stage value.
    /// Immutable stages: GO, NO GO, CANCELLED
    /// </summary>
    /// <param name="stage">The stage value to check</param>
    /// <returns>True if the stage is an immutable stage</returns>
    private bool IsOpportunityImmutable(string? stage)
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
    private void ThrowIfImmutable(Opportunity opportunity)
    {
        if (IsOpportunityImmutable(opportunity))
        {
            throw new BusinessException("This opportunity record is locked and cannot be modified after a decision has been made.");
        }
    }

    /// <summary>
    /// Throws a BusinessException if the opportunity is currently in an approval workflow.
    /// Call this at the start of any modification method.
    /// </summary>
    /// <param name="opportunity">The opportunity to validate</param>
    /// <exception cref="BusinessException">Thrown when opportunity is in approval workflow</exception>
    private void ThrowIfInApprovalWorkflow(Opportunity opportunity)
    {
        if (opportunity?.IsInWorkflow == true)
        {
            throw new BusinessException("This opportunity is pending approval and cannot be modified.");
        }
    }

    /// <summary>
    /// Throws a BusinessException if the opportunity cannot be modified.
    /// Checks both immutability (GO/NO GO/CANCELLED stages) and approval workflow status.
    /// Call this at the start of any modification method.
    /// </summary>
    /// <param name="opportunity">The opportunity to validate</param>
    /// <exception cref="BusinessException">Thrown when opportunity cannot be modified</exception>
    private void ThrowIfCannotModify(Opportunity opportunity)
    {
        ThrowIfImmutable(opportunity);
        ThrowIfInApprovalWorkflow(opportunity);
    }

    #endregion

    /// <summary>
    /// Gets the user name by user ID from UserProfile or falls back to PAOUser email
    /// </summary>
    private async Task<string> GetUserNameByIdAsync(int userId)
    {
        try
        {
            // Handle special case for Opportunity+ system user
            if (userId == -1)
            {
                return "Opportunity+ System";
            }
            
            // Handle unassigned/system default
            if (userId == 0)
            {
                return "System";
            }
            
            var userProfile = await uNOPSAppDbContext.UserProfile.FirstOrDefaultAsync(up => up.UserId == userId);
            if (userProfile != null && !string.IsNullOrEmpty(userProfile.Name))
            {
                return userProfile.Name;
            }
            
            // Fallback to PAOUser email if UserProfile not found or Name is empty
            var user = await uNOPSAppDbContext.PAOUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                return user.Email;
            }
        }
        catch (Exception)
        {
            // Log error if needed, but don't fail the entire operation
        }
        
        return $"User #{userId}";
    }

    public async Task<OpportunityModel> CreateOpportunityAsync(OpportunityRequest model)
    {
        var entity = mapper.Map<Opportunity>(model);

        // Set default workflow stage if not provided
        // Stage defaults to "IDENTIFY & PROFILE" in entity definition
        if (string.IsNullOrEmpty(entity.Stage))
        {
            entity.Stage = "IDENTIFY & PROFILE";
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

            // Use exchange rate service for currency conversion (same as ApplyAiChangesAsync)
            var fundingPartners = new List<OpportunityFundingPartner>();
            
            foreach (var fp in model.FundingPartners)
            {
                var mapped = mapper.Map<OpportunityFundingPartner>(fp);
                
                // Set default currency if not provided
                var currencyId = mapped.CurrencyId > 0 ? mapped.CurrencyId : defaultCurrencyId;
                mapped.CurrencyId = currencyId;
                
                var currency = await uNOPSAppDbContext.Currencies.FindAsync(currencyId);
                var amount = mapped.Amount;
                
                // Convert amount to USD if amount is provided (same logic as ApplyAiChangesAsync)
                if (amount.HasValue && amount.Value > 0 && currency != null)
                {
                    try
                    {
                        var conversionResult = await _exchangeRateService.ConvertToUSDAsync(
                            amount.Value, 
                            currency.Code ?? "USD"
                        );
                        
                        mapped.AmountUSD = conversionResult.AmountUSD;
                        mapped.ExchangeRate = conversionResult.ExchangeRate;
                        mapped.ExchangeRateDate = conversionResult.ExchangeRateDate;
                        mapped.ExchangeRateId = conversionResult.ExchangeRateId > 0 ? conversionResult.ExchangeRateId : null;
                    }
                    catch (Exception ex)
                    {
                        // Log warning but don't fail the operation
                        Console.WriteLine($"Warning: Could not convert amount to USD for partner {fp.PartnerId}: {ex.Message}");
                        // If conversion fails, just store the original amount as USD
                        mapped.AmountUSD = amount.Value;
                        mapped.ExchangeRate = 1.0m;
                        mapped.ExchangeRateDate = DateTime.UtcNow;
                    }
                }
                
                fundingPartners.Add(mapped);
            }
            
            entity.FundingPartners = fundingPartners;
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
            .AsNoTracking() // Performance: No entity tracking needed for read-only operations
            .Include(o => o.ResponsibleOrgUnit)
            .Include(o => o.ProposedInitiativeType)
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
                .ThenInclude(fp => fp.Partner)
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
                .ThenInclude(fp => fp.Currency)
            .Include(o => o.ClientPartners.Where(cp => !cp.IsDeleted))
                .ThenInclude(cp => cp.Partner)
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.EntityRole)
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.User)
                    .ThenInclude(u => u!.UserProfile)
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.OrganizationHierarchy)
            .Include(o => o.Collaborators.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.User)
                    .ThenInclude(u => u!.UserProfile)
            .Include(o => o.Collaborators.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.AddedByUser)
                    .ThenInclude(u => u!.UserProfile)
            .Include(o => o.Collaborators.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Expertises.Where(e => !e.IsDeleted))
                    .ThenInclude(e => e.CollaboratorExpertise)
            .Include(o => o.ExternalStakeholders.Where(es => !es.IsDeleted))
                .ThenInclude(es => es.Contact)
                    .ThenInclude(c => c!.Partner)
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
                .ThenInclude(d => d.Output)
            .Include(o => o.Countries.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Country)
            .Include(o => o.SDGs.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.SDG)
            .Include(o => o.SDGs.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Targets.Where(t => !t.IsDeleted))
                    .ThenInclude(t => t.SDGTarget)
            .Include(o => o.SDGs.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Targets.Where(t => !t.IsDeleted))
                    .ThenInclude(t => t.Indicators.Where(i => !i.IsDeleted))
                        .ThenInclude(i => i.SDGIndicator)
            .Include(o => o.UNCFOutcomes.Where(uo => !uo.IsDeleted))
                .ThenInclude(uo => uo.UNCFOutcome)
            .Include(o => o.UNCFOutcomes.Where(uo => !uo.IsDeleted))
                .ThenInclude(uo => uo.OpportunityCountry)
                    .ThenInclude(oc => oc.Country)
            .Include(o => o.UNCFOutcomes.Where(uo => !uo.IsDeleted))
                .ThenInclude(uo => uo.Indicators.Where(ui => !ui.IsDeleted))
                    .ThenInclude(ui => ui.UNCFIndicator)
            .Include(o => o.UNOPSMissions.Where(om => !om.IsDeleted))
                .ThenInclude(om => om.UNOPSMission)
            .Include(o => o.CreatedByUser)
                .ThenInclude(u => u!.UserProfile)
            .Include(o => o.LastModifiedByUser)
                .ThenInclude(u => u!.UserProfile)
            .AsSplitQuery() // Split into multiple queries to avoid Cartesian explosion
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

        if (entity == null)
        {
            return null;
        }

        var model = mapper.Map<OpportunityModel>(entity, opt => opt.Items["Opportunity"] = entity);
        
        // Resolve user names for audit fields (handles -1 for Opportunity+ System, 0 for System)
        model.CreatedByName = await GetUserNameByIdAsync(entity.CreatedBy);
        model.LastModifiedByName = await GetUserNameByIdAsync(entity.LastModifiedBy);
        
        // Populate EntityArtifacts for ResponsibleOrgUnit (resolver doesn't work for nested mappings)
        if (model.ResponsibleOrgUnit != null && entity.ResponsibleOrgUnit != null)
        {
            var now = DateTime.UtcNow;
            var orgUnitArtifacts = await context.EntityArtifacts
                .AsNoTracking() // Performance: No entity tracking needed for read-only operations
                .Where(a => a.EntityType == "OrganizationHierarchy"
                    && a.EntityId == entity.ResponsibleOrgUnit.Id
                    && !a.IsDeleted
                    && a.Status == EntityStatus.Active
                    && (a.EffectiveDate == null || a.EffectiveDate <= now))
                .Include(a => a.ArtifactType)
                    .ThenInclude(at => at!.ArtifactDataType)
                .OrderBy(a => a.ArtifactType!.Order)
                .ToListAsync();

            model.ResponsibleOrgUnit.Artifacts = EntityArtifactValueResolver.MapToModels(orgUnitArtifacts);
        }
        
        // Populate associated documents and DD fields for funding partners
        if (model.FundingPartners != null && model.FundingPartners.Any())
        {
            // Get opportunity country IDs for agreement matching
            var opportunityCountryIds = entity.Countries?.Select(c => c.CountryId).ToList() ?? new List<int>();
            
            // Initialize GoogleCloudStorageService for logo URL signing
            var googleCloudStorageService = new GoogleCloudStorageService(configuration);
            
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
                    
                    // Partner Logo URL - convert to signed URL
                    if (!string.IsNullOrEmpty(partner.LogoUrl))
                    {
                        fundingPartner.PartnerLogoUrl = await googleCloudStorageService.GenerateSignedUrlFromStorageUrl(partner.LogoUrl);
                    }
                    
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
            
            // Initialize GoogleCloudStorageService for logo URL signing (reuse if already created)
            var googleCloudStorageService = new GoogleCloudStorageService(configuration);
            
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
                    
                    // Partner Logo URL - convert to signed URL
                    if (!string.IsNullOrEmpty(partner.LogoUrl))
                    {
                        clientPartner.PartnerLogoUrl = await googleCloudStorageService.GenerateSignedUrlFromStorageUrl(partner.LogoUrl);
                    }
                    
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
        // Note: Sequential execution required - DbContext is not thread-safe for parallel operations
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
                    .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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

        // Load SME (Subject Matter Expert) selections from EntityUserRoles table
        model.SMESelections = await GetSMESelectionsAsync(id);

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
            .AsNoTracking() // Performance: No entity tracking needed for read-only operations
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        
        if (entity == null)
        {
            return null;
        }
        
        // Check if user is a team member (stakeholder or collaborator) on this opportunity
        var isTeamMember = await IsUserTeamMemberOnOpportunityAsync(user, id);
        
        // Add permissions with team member check
        model = await MapEntityToModelWithPermissionsAsync(model, user, entity);
        
        // If user is a team member (stakeholder or collaborator), they should be able to update the opportunity
        // even if they don't have global update permission
        if (isTeamMember && model.Permissions != null)
        {
            model.Permissions.CanUpdate = true;
            model.Permissions.Notes = "Team member on this opportunity";
        }
        
        // Check immutability and override permissions if the opportunity is in an immutable stage
        // This must be done AFTER all other permission checks as immutability takes precedence
        if (model.Permissions != null)
        {
            var isImmutable = IsOpportunityImmutable(entity);
            if (isImmutable)
            {
                model.Permissions.CanUpdate = false;
                model.Permissions.CanDelete = false;
                model.Permissions.IsImmutable = true;
                model.Permissions.Notes = "This opportunity is locked after a decision has been made.";
            }
            
            // Check if opportunity is in approval workflow (Approval Pending status)
            // When in workflow, the opportunity cannot be edited until approval completes
            if (entity.IsInWorkflow)
            {
                model.Permissions.CanUpdate = false;
                model.Permissions.CanDelete = false;
                model.Permissions.IsApprovalPending = true;
                model.Permissions.Notes = "This opportunity is pending approval and cannot be edited.";
            }
        }
        
        return model;
    }
    
    /// <summary>
    /// Checks if the current user is a team member (stakeholder or collaborator) on the given opportunity.
    /// Team members include:
    /// - Internal stakeholders (users assigned via OpportunityStakeholder)
    /// - Collaborators (users assigned via OpportunityCollaborator - Opportunity Development Team)
    /// </summary>
    /// <param name="user">Current user context</param>
    /// <param name="opportunityId">Opportunity ID</param>
    /// <returns>True if user is a team member, false otherwise</returns>
    private async Task<bool> IsUserTeamMemberOnOpportunityAsync(ClaimsPrincipal user, int opportunityId)
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
        
        // Check if user is a Collaborator (Opportunity Development Team member)
        // Collaborators have permissions to edit all fields of the opportunity
        var isCollaborator = await context.OpportunityCollaborators
            .AnyAsync(c => c.OpportunityId == opportunityId 
                        && c.UserId == userId
                        && !c.IsDeleted);
        
        if (isCollaborator)
        {
            return true;
        }
        
        // Check if this user is an internal stakeholder on the opportunity
        var isStakeholder = await context.OpportunityStakeholders
            .AnyAsync(s => s.OpportunityId == opportunityId 
                        && s.UserId == userId 
                        && s.IsInternal
                        && !s.IsDeleted);
        
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
                .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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
                .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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
            .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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
            .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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
            .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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
            .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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
            .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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
            .AsNoTracking() // Performance: No entity tracking needed for read-only operations
            .Where(r => 
                r.EntityType == "Country" 
                && countryIds.Contains(r.EntityId)
                && !r.IsDeleted)
            .Include(r => r.OrganizationHierarchy)
            .ToListAsync();
        
        // Get all org units with Strategy artifacts
        var orgUnitsWithStrategy = await context.EntityArtifacts
            .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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
                .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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
            .Include(o => o.ResponsibleOrgUnit)
            .Include(o => o.ProposedInitiativeType)
            .Where(o => !o.IsDeleted)
            .ToListAsync();

        return entities.Select(e => mapper.Map<OpportunityModel>(e));
    }

    public async Task<OpportunityModel?> UpdateOpportunityAsync(UpdateOpportunityRequest model)
    {
        var entity = await context.Opportunities
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
            .Include(o => o.ClientPartners.Where(cp => !cp.IsDeleted))
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
            .Include(o => o.Countries.Where(c => !c.IsDeleted))
            .Include(o => o.SDGs.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == model.Id && !o.IsDeleted);

        if (entity == null)
        {
            return null;
        }

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(entity);

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

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(entity);

        // Update Overview section fields
        if (request.Name != null)
        {
            entity.Name = request.Name;
        }

        if (request.Description != null)
        {
            entity.Description = request.Description;
        }

        // Update initiative budget (allow setting to null to clear)
        entity.InitiativeBudgetUSD = request.InitiativeBudgetUSD;

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

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(entity);

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

            // Deduplicate deliverables by OutputId (keep first occurrence)
            var uniqueDeliverables = request.Deliverables
                .GroupBy(d => d.OutputId)
                .Select(g => g.First())
                .ToList();

            // Add new deliverables
            entity.Deliverables = uniqueDeliverables
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

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(entity);

        // Update WHY section fields

        if (request.ExpectedBeneficiaries != null)
        {
            entity.ExpectedBeneficiaries = request.ExpectedBeneficiaries;
        }
        
        // Update beneficiary numbers
        entity.EstimatedDirectBeneficiaries = request.EstimatedDirectBeneficiaries;
        entity.EstimatedIndirectBeneficiaries = request.EstimatedIndirectBeneficiaries;
        entity.BeneficiariesToBeDetermined = request.BeneficiariesToBeDetermined;

        if (request.ExpectedImpact != null)
        {
            // Truncate to 510 characters (database column limit)
            entity.ExpectedImpact = request.ExpectedImpact.Length > 510 
                ? request.ExpectedImpact[..510] 
                : request.ExpectedImpact;
        }

        if (request.ExpectedOutcomes != null)
        {
            // Truncate to 510 characters (database column limit)
            entity.ExpectedOutcomes = request.ExpectedOutcomes.Length > 510 
                ? request.ExpectedOutcomes[..510] 
                : request.ExpectedOutcomes;
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
            // CRITICAL: Filter out soft-deleted records to avoid re-selection issues
            var existingSDGs = await context.OpportunitySDGs
                .Where(sdg => sdg.OpportunityId == id && !sdg.IsDeleted)
                .Include(sdg => sdg.Targets.Where(t => !t.IsDeleted))
                    .ThenInclude(t => t.Indicators.Where(i => !i.IsDeleted))
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
            // CRITICAL: Filter out soft-deleted records to avoid re-selection issues
            var existingUNCFOutcomes = await context.OpportunityUNCFOutcomes
                .Where(uo => uo.OpportunityId == id && !uo.IsDeleted)
                .Include(uo => uo.Indicators.Where(i => !i.IsDeleted))
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

        // Update UNOPS Missions Not Applicable flag
        // When true, it means the user explicitly indicated that mission alignment is not applicable
        entity.UNOPSMissionsNotApplicable = request.UNOPSMissionsNotApplicable;
        
        // Update UNOPS Mission alignments with differential update strategy
        if (request.UNOPSMissions != null)
        {
            // Load existing UNOPS mission alignments
            // CRITICAL: Filter out soft-deleted records to avoid re-selection issues
            var existingMissions = await context.Set<OpportunityUNOPSMission>()
                .Where(m => m.OpportunityId == id && !m.IsDeleted)
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
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
            .Include(o => o.ClientPartners.Where(cp => !cp.IsDeleted))
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.ExternalStakeholders.Where(es => !es.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(opportunity);

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

            // Deduplicate funding partners by PartnerId (keep first occurrence)
            var uniqueFundingPartners = request.FundingPartners
                .GroupBy(fp => fp.PartnerId)
                .Select(g => g.First())
                .ToList();

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
            var fundingPartners = new List<OpportunityFundingPartner>();
            
            foreach (var fp in uniqueFundingPartners)
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
                        var conversionResult = await _exchangeRateService.ConvertToUSDAsync(
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

            // Deduplicate client partners by PartnerId (keep first occurrence)
            var uniqueClientPartners = request.ClientPartners
                .GroupBy(cp => cp.PartnerId)
                .Select(g => g.First())
                .ToList();

            // Add new client partners
            opportunity.ClientPartners = uniqueClientPartners
                .Select(cp => new OpportunityClientPartner
                {
                    OpportunityId = id,
                    PartnerId = cp.PartnerId,
                    SelectedPartnerAgreementNumber = cp.SelectedPartnerAgreementNumber
                })
                .ToList();
        }

        // Note: Internal stakeholders are now managed in the Team section (UpdateTeamSectionAsync)
        
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
            
            // Deduplicate external stakeholders by ContactId (keep first occurrence)
            var uniqueExternalStakeholders = request.ExternalStakeholders
                .GroupBy(es => es.ContactId)
                .Select(g => g.First())
                .ToList();

            // Validate that all contacts belong to the opportunity's partners
            var contactIds = uniqueExternalStakeholders.Select(es => es.ContactId).Distinct().ToList();
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
            opportunity.ExternalStakeholders = uniqueExternalStakeholders
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

    public async Task<OpportunityModel> UpdateTeamSectionAsync(int id, TeamSectionRequest request)
    {
        var opportunity = await context.Opportunities
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.Collaborators.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Expertises.Where(e => !e.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(opportunity);

        // Track if org unit changed
        var orgUnitChanged = request.ResponsibleOrgUnitId.HasValue && 
                            request.ResponsibleOrgUnitId.Value != opportunity.ResponsibleOrgUnitId;

        // Update Responsible Org Unit
        if (request.ResponsibleOrgUnitId.HasValue)
        {
            opportunity.ResponsibleOrgUnitId = request.ResponsibleOrgUnitId.Value;
        }

        // Update Initiative Type
        if (request.ProposedInitiativeTypeId.HasValue)
        {
            opportunity.ProposedInitiativeTypeId = request.ProposedInitiativeTypeId.Value;
        }

        // Update Internal Stakeholders (Team & Stakeholders) using differential update
        if (request.Stakeholders != null)
        {
            // Get SME role IDs first - SME stakeholders should NOT be in request.Stakeholders
            // They are managed separately via request.SMESelections
            var smeRoleIds = await context.Set<EntityRole>()
                .Where(er => er.EntityType == "Opportunity" && er.Type == "SME" && !er.IsDeleted)
                .Select(er => er.Id)
                .ToListAsync();

            // Get Opportunity Manager role ID - Opportunity Manager is managed separately via request.OpportunityManagerId
            var opportunityManagerRoleId = await context.Set<EntityRole>()
                .Where(er => er.Name != null && er.Name.ToLower() == "opportunity manager" && er.EntityType == "Opportunity" && !er.IsDeleted)
                .Select(er => er.Id)
                .FirstOrDefaultAsync();

            // Deduplicate user-based stakeholders by UserId + EntityRoleId combination (keep first occurrence)
            // EXCLUDE SME roles - they should only be managed via SMESelections
            // EXCLUDE Opportunity Manager role - it is managed separately via OpportunityManagerId field
            var requestedUserStakeholders = request.Stakeholders
                .Where(s => s.UserId.HasValue 
                    && !s.OrganizationHierarchyId.HasValue 
                    && !smeRoleIds.Contains(s.EntityRoleId) // EXCLUDE SME roles
                    && s.EntityRoleId != opportunityManagerRoleId) // EXCLUDE Opportunity Manager role
                .GroupBy(s => new { s.UserId, s.EntityRoleId })
                .Select(g => g.First())
                .ToList();

            // Get entity roles to check AllowsMultiple property
            var entityRoleIds = requestedUserStakeholders.Select(s => s.EntityRoleId).Distinct().ToList();
            var entityRoles = await context.Set<EntityRole>()
                .Where(er => entityRoleIds.Contains(er.Id))
                .ToDictionaryAsync(er => er.Id);

            // Validate that single-assignment roles don't have duplicates for user-based stakeholders
            var roleGroups = requestedUserStakeholders
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

            opportunity.Stakeholders ??= new List<OpportunityStakeholder>();

            // Get existing user-based stakeholders (not auto-populated and NOT SME/Opportunity Manager roles)
            // SME stakeholders are managed separately via SMESelections
            // Opportunity Manager stakeholders are managed separately via OpportunityManagerId field
            var existingUserStakeholders = opportunity.Stakeholders
                .Where(s => s.UserId.HasValue 
                    && !s.OrganizationHierarchyId.HasValue 
                    && !smeRoleIds.Contains(s.EntityRoleId) // EXCLUDE SME roles
                    && s.EntityRoleId != opportunityManagerRoleId) // EXCLUDE Opportunity Manager role
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
                opportunity.Stakeholders.Remove(stakeholder);
                context.Set<OpportunityStakeholder>().Remove(stakeholder);
            }

            // Add new stakeholders
            foreach (var req in stakeholdersToAdd)
            {
                opportunity.Stakeholders.Add(new OpportunityStakeholder
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
                // Step 1: Soft-delete ALL existing Opportunity Manager stakeholders for this opportunity
                var allExistingManagers = await context.Set<OpportunityStakeholder>()
                    .Where(s => s.OpportunityId == id 
                        && s.EntityRoleId == opportunityManagerRole.Id)
                    .ToListAsync();
                
                foreach (var existingManager in allExistingManagers)
                {
                    existingManager.IsDeleted = true;
                    // Remove from in-memory collection if it's there
                    if (opportunity.Stakeholders?.Contains(existingManager) == true)
                    {
                        opportunity.Stakeholders.Remove(existingManager);
                    }
                }
                
                // Step 2: Check if the new manager already has a stakeholder record (possibly soft-deleted)
                var existingRecordForNewManager = allExistingManagers
                    .FirstOrDefault(s => s.UserId == request.OpportunityManagerId.Value);
                
                if (existingRecordForNewManager != null)
                {
                    // Reactivate the existing record
                    existingRecordForNewManager.IsDeleted = false;
                    // Add back to in-memory collection
                    opportunity.Stakeholders ??= new List<OpportunityStakeholder>();
                    if (!opportunity.Stakeholders.Contains(existingRecordForNewManager))
                    {
                        opportunity.Stakeholders.Add(existingRecordForNewManager);
                    }
                }
                else
                {
                    // Create new opportunity manager stakeholder
                    opportunity.Stakeholders ??= new List<OpportunityStakeholder>();
                    opportunity.Stakeholders.Add(new OpportunityStakeholder
                    {
                        OpportunityId = id,
                        UserId = request.OpportunityManagerId.Value,
                        EntityRoleId = opportunityManagerRole.Id,
                        IsInternal = true,
                        StakeholderType = "Internal",
                        OrganizationHierarchyId = null,
                        IsDeleted = false
                    });
                }
            }
        }

        // Update Collaborators (Opportunity Development Team) with Expertise assignments
        if (request.Collaborators != null)
        {
            opportunity.Collaborators ??= new List<OpportunityCollaborator>();
            
            // Get current user ID for AddedBy tracking
            int currentUserId = 0;
            if (httpContextAccessor?.HttpContext?.User != null)
            {
                var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    currentUserId = parsedUserId;
                }
            }
            
            // Get requested user IDs
            var requestedUserIds = request.Collaborators.Select(c => c.UserId).ToHashSet();
            
            // Get existing collaborators (need to load with expertises)
            // CRITICAL: Filter out soft-deleted records to avoid re-selection issues
            var existingCollaborators = await context.Set<OpportunityCollaborator>()
                .Include(c => c.Expertises.Where(e => !e.IsDeleted))
                .Where(c => c.OpportunityId == id && !c.IsDeleted)
                .ToListAsync();
            
            // Find collaborators to remove (exist in DB but not in request)
            var collaboratorsToRemove = existingCollaborators
                .Where(c => !requestedUserIds.Contains(c.UserId))
                .ToList();
            
            // Remove collaborators that are no longer in the request
            foreach (var collaborator in collaboratorsToRemove)
            {
                // Remove expertise assignments first
                if (collaborator.Expertises != null && collaborator.Expertises.Any())
                {
                    context.Set<OpportunityCollaboratorExpertise>().RemoveRange(collaborator.Expertises);
                }
                context.Set<OpportunityCollaborator>().Remove(collaborator);
            }
            
            // Process each requested collaborator
            foreach (var collaboratorRequest in request.Collaborators)
            {
                var existingCollaborator = existingCollaborators.FirstOrDefault(c => c.UserId == collaboratorRequest.UserId);
                
                if (existingCollaborator == null)
                {
                    // Add new collaborator
                    var newCollaborator = new OpportunityCollaborator
                    {
                        OpportunityId = id,
                        UserId = collaboratorRequest.UserId,
                        AddedDate = DateTime.UtcNow,
                        AddedBy = currentUserId > 0 ? currentUserId : null
                    };
                    
                    // Add expertises
                    if (collaboratorRequest.ExpertiseIds != null && collaboratorRequest.ExpertiseIds.Any())
                    {
                        newCollaborator.Expertises = collaboratorRequest.ExpertiseIds
                            .Select(expertiseId => new OpportunityCollaboratorExpertise
                            {
                                OpportunityId = id,
                                CollaboratorExpertiseId = expertiseId
                            })
                            .ToList();
                    }
                    
                    context.Set<OpportunityCollaborator>().Add(newCollaborator);
                }
                else
                {
                    // Update existing collaborator's expertises
                    var requestedExpertiseIds = collaboratorRequest.ExpertiseIds?.ToHashSet() ?? new HashSet<int>();
                    var existingExpertiseIds = existingCollaborator.Expertises?
                        .Select(e => e.CollaboratorExpertiseId).ToHashSet() ?? new HashSet<int>();
                    
                    // Remove expertises that are no longer in the request
                    if (existingCollaborator.Expertises != null)
                    {
                        var expertisesToRemove = existingCollaborator.Expertises
                            .Where(e => !requestedExpertiseIds.Contains(e.CollaboratorExpertiseId))
                            .ToList();
                        context.Set<OpportunityCollaboratorExpertise>().RemoveRange(expertisesToRemove);
                    }
                    
                    // Add new expertises
                    var expertiseIdsToAdd = requestedExpertiseIds.Where(eid => !existingExpertiseIds.Contains(eid));
                    foreach (var expertiseId in expertiseIdsToAdd)
                    {
                        context.Set<OpportunityCollaboratorExpertise>().Add(new OpportunityCollaboratorExpertise
                        {
                            OpportunityId = id,
                            OpportunityCollaboratorId = existingCollaborator.Id,
                            CollaboratorExpertiseId = expertiseId
                        });
                    }
                }
            }
        }

        // Auto-populate stakeholders from EntityUserRoles if org unit changed
        if (orgUnitChanged && request.ResponsibleOrgUnitId.HasValue)
        {
            await AutoPopulateStakeholdersFromOrgUnitAsync(opportunity, request.ResponsibleOrgUnitId.Value);
        }

        await context.SaveChangesAsync();

        // Reload with all includes
        var reloadedResult = await GetOpportunityAsync(id);
        return reloadedResult ?? throw new KeyNotFoundException($"Failed to reload opportunity {id}");
    }

    /// <summary>
    /// Auto-populates stakeholders from EntityUserRoles based on the org unit type.
    /// - OrgUnit: Uses EntityUserRoles directly from the selected org unit
    /// - GPO (name contains "GPO"): Gets org units for implementation countries (with parent/grandparent)
    /// - Hub/Region: Gets child org units that relate to implementation countries
    /// Uses differential update - only adds/removes what's necessary.
    /// </summary>
    private async Task AutoPopulateStakeholdersFromOrgUnitAsync(Opportunity entity, int orgUnitId)
    {
        // Get the org unit to check its type and name
        var orgUnit = await context.OrganizationHierarchies
            .Where(oh => oh.Id == orgUnitId && !oh.IsDeleted)
            .Select(oh => new { oh.Id, oh.Type, oh.Name })
            .FirstOrDefaultAsync();

        entity.Stakeholders ??= new List<OpportunityStakeholder>();

        // Get existing auto-populated stakeholders
        var existingAutoPopulated = entity.Stakeholders
            .Where(s => s.OrganizationHierarchyId.HasValue)
            .ToList();

        if (orgUnit == null)
        {
            // Remove all auto-populated stakeholders if org unit not found
            foreach (var stakeholder in existingAutoPopulated)
            {
                entity.Stakeholders.Remove(stakeholder);
                context.Set<OpportunityStakeholder>().Remove(stakeholder);
            }
            return;
        }

        // Determine which org units to get EntityUserRoles from
        var orgUnitIdsForRoles = new List<int>();
        var isGpo = orgUnit.Name?.Contains("GPO") ?? false;
        var isHubOrRegion = orgUnit.Type == Domain.Enums.OrganizationUnitType.Hub || 
                           orgUnit.Type == Domain.Enums.OrganizationUnitType.Region;

        if (isGpo)
        {
            // GPO: Get org units for implementation countries (with parent/grandparent)
            // AND include the GPO org unit itself
            orgUnitIdsForRoles = await GetOrgUnitIdsForCountriesWithHierarchyAsync(entity.Id);
            // Add the GPO org unit ID if not already included
            if (!orgUnitIdsForRoles.Contains(orgUnitId))
            {
                orgUnitIdsForRoles.Add(orgUnitId);
            }
        }
        else if (isHubOrRegion)
        {
            // Hub/Region: Get child org units that relate to implementation countries
            orgUnitIdsForRoles = await GetChildOrgUnitIdsForHubRegionAsync(orgUnitId, entity.Id);
        }
        else if (orgUnit.Type == Domain.Enums.OrganizationUnitType.OrgUnit)
        {
            // OrgUnit: Use the selected org unit directly
            orgUnitIdsForRoles.Add(orgUnitId);
        }
        else
        {
            // Other types: Remove all auto-populated stakeholders
            foreach (var stakeholder in existingAutoPopulated)
            {
                entity.Stakeholders.Remove(stakeholder);
                context.Set<OpportunityStakeholder>().Remove(stakeholder);
            }
            return;
        }

        // ALWAYS add normally responsible org units (if different from selected)
        // These are the org units normally responsible for implementation countries
        // NOTE: Must be done BEFORE the empty check below
        var normallyResponsibleOrgUnits = await GetNormallyResponsibleOrgUnitsAsync(entity.Id, orgUnitId);
        foreach (var normalOrgUnitId in normallyResponsibleOrgUnits)
        {
            if (!orgUnitIdsForRoles.Contains(normalOrgUnitId))
            {
                orgUnitIdsForRoles.Add(normalOrgUnitId);
            }
        }

        if (!orgUnitIdsForRoles.Any())
        {
            // No org units to populate from - remove existing auto-populated
            foreach (var stakeholder in existingAutoPopulated)
            {
                entity.Stakeholders.Remove(stakeholder);
                context.Set<OpportunityStakeholder>().Remove(stakeholder);
            }
            return;
        }

        // Get Opportunity Manager role ID to exclude from auto-population
        // Opportunity Manager is managed separately via the dedicated OpportunityManagerId field
        var opportunityManagerRoleId = await context.Set<EntityRole>()
            .Where(er => er.Name != null && er.Name.ToLower() == "opportunity manager" && er.EntityType == "Opportunity" && !er.IsDeleted)
            .Select(er => er.Id)
            .FirstOrDefaultAsync();

        // Get EntityUserRoles for all relevant org units (including normally responsible)
        // Returns tuples of (OrgUnitId, EntityRoleId, UserId) - includes UserId for storage
        // IMPORTANT: Excludes Opportunity Manager role - it is managed separately via OpportunityManagerId field
        var entityUserRoles = await context.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" 
                       && orgUnitIdsForRoles.Contains(eur.EntityId)
                       && eur.EntityRoleId.HasValue
                       && eur.EntityRoleId != opportunityManagerRoleId // Exclude Opportunity Manager role
                       && !eur.IsDeleted)
            .Select(eur => new { 
                OrgUnitId = eur.EntityId, 
                EntityRoleId = eur.EntityRoleId!.Value,
                UserId = eur.UserId // Include UserId for proper stakeholder storage
            })
            .ToListAsync();

        // Create a set of valid (OrgUnitId, RoleId, UserId) combinations
        // Use tuple to track all three values
        var validCombinations = entityUserRoles
            .Select(e => (e.OrgUnitId, e.EntityRoleId, e.UserId))
            .ToHashSet();
        
        // Also track just (OrgUnitId, RoleId) for comparison with existing
        var validOrgUnitRoleCombinations = entityUserRoles
            .Select(e => (e.OrgUnitId, e.EntityRoleId))
            .ToHashSet();

        // Find auto-populated stakeholders to remove:
        // - Those not in the valid combinations (by OrgUnit + Role)
        var autoPopulatedToRemove = existingAutoPopulated
            .Where(existing => 
                !existing.OrganizationHierarchyId.HasValue ||
                !validOrgUnitRoleCombinations.Contains((existing.OrganizationHierarchyId.Value, existing.EntityRoleId)))
            .ToList();

        // Find combinations to add (exist in EntityUserRoles but not in existing auto-populated)
        // Check by OrgUnit + Role + UserId to handle multiple users per role
        var existingCombinations = existingAutoPopulated
            .Where(s => s.OrganizationHierarchyId.HasValue)
            .Select(s => (s.OrganizationHierarchyId!.Value, s.EntityRoleId, s.UserId))
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

        // Add new auto-populated stakeholders with UserId
        foreach (var (targetOrgUnitId, roleId, userId) in combinationsToAdd)
        {
            entity.Stakeholders.Add(new OpportunityStakeholder
            {
                OpportunityId = entity.Id,
                EntityRoleId = roleId,
                OrganizationHierarchyId = targetOrgUnitId,
                UserId = null,
                IsInternal = true,
                StakeholderType = "Internal",
                Notes = null
            });
        }
    }

    /// <summary>
    /// Updates SME (Subject Matter Expert) selections for an opportunity in the OpportunityStakeholder table.
    /// Uses differential update - only adds/removes what's necessary.
    /// IMPORTANT: Deselected SME roles are HARD DELETED (permanently removed) from the database.
    /// Only currently selected SME assignments are retained.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <param name="smeSelections">List of SME selection requests</param>
    private async Task UpdateSMESelectionsAsync(int opportunityId, List<SMESelectionRequest> smeSelections)
    {
        // Get all SME roles (roles with Type = "SME")
        var smeRoleIds = await context.Set<EntityRole>()
            .Where(er => er.EntityType == "Opportunity" && er.Type == "SME" && !er.IsDeleted)
            .Select(er => er.Id)
            .ToListAsync();

        if (!smeRoleIds.Any())
            return;

        // Get existing SME OpportunityStakeholders for this opportunity
        // SMEs are OpportunityStakeholders with IsInternal=true and EntityRoleId in SME roles
        var existingSmeStakeholders = await context.Set<OpportunityStakeholder>()
            .Where(os => 
                os.OpportunityId == opportunityId 
                && os.IsInternal == true
                && smeRoleIds.Contains(os.EntityRoleId)
                && os.OrganizationHierarchyId == null) // Exclude auto-populated stakeholders
            .ToListAsync();

        // Get selected SME entries (IsSelected = true and UserId is provided)
        var selectedSmes = smeSelections
            .Where(s => s.IsSelected && s.UserId.HasValue && smeRoleIds.Contains(s.EntityRoleId))
            .ToList();

        // Find OpportunityStakeholders to remove (exist in DB but not in selected SMEs or deselected)
        var stakeholdersToRemove = existingSmeStakeholders
            .Where(existing => !selectedSmes.Any(req => 
                req.EntityRoleId == existing.EntityRoleId && req.UserId == existing.UserId))
            .ToList();

        // Find OpportunityStakeholders to add (exist in selected SMEs but not in DB)
        var stakeholdersToAdd = selectedSmes
            .Where(req => !existingSmeStakeholders.Any(existing => 
                existing.EntityRoleId == req.EntityRoleId && existing.UserId == req.UserId))
            .ToList();

        // HARD DELETE OpportunityStakeholders that are no longer selected (permanently removes from database)
        foreach (var stakeholderToRemove in stakeholdersToRemove)
        {
            context.Set<OpportunityStakeholder>().Remove(stakeholderToRemove);
        }

        // Add new OpportunityStakeholders
        foreach (var req in stakeholdersToAdd)
        {
            context.Set<OpportunityStakeholder>().Add(new OpportunityStakeholder
            {
                OpportunityId = opportunityId,
                EntityRoleId = req.EntityRoleId,
                UserId = req.UserId!.Value,
                IsInternal = true,
                StakeholderType = "Internal",
                OrganizationHierarchyId = null, // User-assigned SMEs don't have org hierarchy
                Notes = null
            });
        }
    }

    /// <summary>
    /// Gets SME (Subject Matter Expert) selections for an opportunity from the OpportunityStakeholder table.
    /// Returns all SME roles with their selection status and assigned user.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <returns>List of SME selection models</returns>
    private async Task<List<SMESelectionModel>> GetSMESelectionsAsync(int opportunityId)
    {
        // Get all SME roles (roles with Type = "SME")
        // PRIORITY 4 OPTIMIZATION: Add AsNoTracking() for read-only operation
        var smeRoles = await context.Set<EntityRole>()
            .AsNoTracking()
            .Where(er => er.EntityType == "Opportunity" && er.Type == "SME" && !er.IsDeleted)
            .OrderBy(er => er.SubType)
            .ThenBy(er => er.Name)
            .Select(er => new { er.Id, er.Name, er.SubType })
            .ToListAsync();

        if (!smeRoles.Any())
            return new List<SMESelectionModel>();

        var smeRoleIds = smeRoles.Select(r => r.Id).ToList();

        // Get existing SME OpportunityStakeholders for this opportunity
        // SMEs are OpportunityStakeholders with IsInternal=true and EntityRoleId in SME roles
        // PRIORITY 4 OPTIMIZATION: Add AsNoTracking() for read-only operation
        var existingSmeStakeholders = await context.Set<OpportunityStakeholder>()
            .AsNoTracking()
            .Include(os => os.User)
                .ThenInclude(u => u!.UserProfile)
            .Where(os => 
                os.OpportunityId == opportunityId 
                && os.IsInternal == true
                && smeRoleIds.Contains(os.EntityRoleId)
                && os.OrganizationHierarchyId == null) // Exclude auto-populated stakeholders
            .ToListAsync();

        // Build the result - all SME roles with their selection status
        var result = new List<SMESelectionModel>();
        foreach (var role in smeRoles)
        {
            var existingAssignment = existingSmeStakeholders.FirstOrDefault(s => s.EntityRoleId == role.Id);
            
            result.Add(new SMESelectionModel
            {
                EntityRoleId = role.Id,
                EntityRoleName = role.Name,
                IsSelected = existingAssignment != null,
                UserId = existingAssignment?.UserId,
                UserName = existingAssignment?.User?.UserProfile?.Name ?? existingAssignment?.User?.Email,
                UserEmail = existingAssignment?.User?.Email
            });
        }

        return result;
    }

    /// <summary>
    /// Gets org unit IDs for the opportunity's implementation countries, including parent and grandparent org units.
    /// Used when a GPO is selected as the responsible org unit.
    /// </summary>
    private async Task<List<int>> GetOrgUnitIdsForCountriesWithHierarchyAsync(int opportunityId)
    {
        // Get implementation country IDs for this opportunity
        // Filter out soft-deleted records
        var countryIds = await context.Set<OpportunityCountry>()
            .Where(oc => oc.OpportunityId == opportunityId && !oc.IsDeleted)
            .Select(oc => oc.CountryId)
            .ToListAsync();

        if (!countryIds.Any())
            return new List<int>();

        // Get org unit relationships for these countries
        var orgUnitRelationships = await context.OrganizationUnitRelationships
            .Where(r => 
                r.EntityType == "Country" 
                && countryIds.Contains(r.EntityId)
                && !r.IsDeleted)
            .Select(r => r.OrganizationHierarchyId)
            .Distinct()
            .ToListAsync();

        if (!orgUnitRelationships.Any())
            return new List<int>();

        // For each org unit, get itself plus parent and grandparent (only OrgUnit types)
        var allOrgUnitIds = new HashSet<int>();

        foreach (var orgUnitIdItem in orgUnitRelationships)
        {
            var currentId = orgUnitIdItem;
            var levelsToGet = 3; // Current + parent + grandparent

            for (int i = 0; i < levelsToGet && currentId != 0; i++)
            {
                var unit = await context.OrganizationHierarchies
                    .Where(oh => oh.Id == currentId && !oh.IsDeleted)
                    .Select(oh => new { oh.Id, oh.ParentId, oh.Type })
                    .FirstOrDefaultAsync();

                if (unit == null)
                    break;

                // Only add OrgUnit type
                if (unit.Type == Domain.Enums.OrganizationUnitType.OrgUnit)
                {
                    allOrgUnitIds.Add(unit.Id);
                }

                currentId = unit.ParentId ?? 0;
            }
        }

        return allOrgUnitIds.ToList();
    }

    /// <summary>
    /// Gets child org unit IDs under a Hub/Region that directly relate to the opportunity's implementation countries.
    /// Used when a Hub or Region is selected as the responsible org unit.
    /// </summary>
    private async Task<List<int>> GetChildOrgUnitIdsForHubRegionAsync(int parentOrgUnitId, int opportunityId)
    {
        // Get implementation country IDs for this opportunity
        // Filter out soft-deleted records
        var countryIds = await context.Set<OpportunityCountry>()
            .Where(oc => oc.OpportunityId == opportunityId && !oc.IsDeleted)
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

        // Filter to only get child org units under this parent that are OrgUnit type (level 3)
        var childOrgUnitIds = new List<int>();
        foreach (var orgUnitId in countryOrgUnitIds)
        {
            var orgUnit = await context.OrganizationHierarchies
                .Where(oh => oh.Id == orgUnitId && !oh.IsDeleted)
                .Select(oh => new { oh.Id, oh.ParentId, oh.Type })
                .FirstOrDefaultAsync();

            if (orgUnit == null)
                continue;

            // Check if this org unit is a child (direct or indirect) of the parent
            // and is of type OrgUnit
            if (orgUnit.Type == Domain.Enums.OrganizationUnitType.OrgUnit)
            {
                // Traverse up to check if parent matches
                var currentParentId = orgUnit.ParentId;
                var isChildOfParent = false;

                while (currentParentId.HasValue && currentParentId.Value != 0)
                {
                    if (currentParentId.Value == parentOrgUnitId)
                    {
                        isChildOfParent = true;
                        break;
                    }

                    var parentUnit = await context.OrganizationHierarchies
                        .Where(oh => oh.Id == currentParentId.Value && !oh.IsDeleted)
                        .Select(oh => new { oh.ParentId })
                        .FirstOrDefaultAsync();

                    currentParentId = parentUnit?.ParentId;
                }

                if (isChildOfParent)
                {
                    childOrgUnitIds.Add(orgUnit.Id);
                }
            }
        }

        return childOrgUnitIds.Distinct().ToList();
    }

    /// <summary>
    /// Gets normally responsible org unit IDs for countries where the selected responsible org unit 
    /// is NOT normally responsible. Returns org units (Type = "OrgUnit", level 3) from country hierarchies
    /// that differ from the selected org unit.
    /// </summary>
    private async Task<List<int>> GetNormallyResponsibleOrgUnitsAsync(int opportunityId, int selectedOrgUnitId)
    {
        // Get implementation country IDs for this opportunity
        // Filter out soft-deleted records
        var countryIds = await context.Set<OpportunityCountry>()
            .Where(oc => oc.OpportunityId == opportunityId && !oc.IsDeleted)
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

    /// <summary>
    /// Gets all descendant org unit IDs under a given parent org unit (recursive).
    /// </summary>
    private async Task<HashSet<int>> GetAllDescendantOrgUnitIdsAsync(int parentOrgUnitId)
    {
        var descendants = new HashSet<int>();
        var toProcess = new Queue<int>();
        toProcess.Enqueue(parentOrgUnitId);

        while (toProcess.Count > 0)
        {
            var currentParentId = toProcess.Dequeue();

            var children = await context.OrganizationHierarchies
                .Where(oh => oh.ParentId == currentParentId && !oh.IsDeleted)
                .Select(oh => oh.Id)
                .ToListAsync();

            foreach (var childId in children)
            {
                if (descendants.Add(childId))
                {
                    toProcess.Enqueue(childId);
                }
            }
        }

        return descendants;
    }

    public async Task<OpportunityModel> UpdateWhereSectionAsync(int id, WhereSectionRequest request)
    {
        var opportunity = await context.Opportunities
            .Include(o => o.Countries.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Country)
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))  // Include stakeholders for auto-population
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(opportunity);

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

        // Auto-populate stakeholders from normally responsible org units if responsible org unit is set
        // This ensures that when countries change, the normally responsible org units' role holders
        // are automatically added as internal stakeholders
        if (opportunity.ResponsibleOrgUnitId.HasValue)
        {
            await AutoPopulateStakeholdersFromOrgUnitAsync(opportunity, opportunity.ResponsibleOrgUnitId.Value);
            await context.SaveChangesAsync();
        }

        // Reload with all includes
        var result = await GetOpportunityAsync(id);
        return result ?? throw new KeyNotFoundException($"Failed to reload opportunity {id}");
    }

    public async Task<RelatedItemsModel> GetRelatedItemsAsync(int id)
    {
        var opportunity = await context.Opportunities
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
                .ThenInclude(fp => fp.Partner)
            .Include(o => o.ClientPartners.Where(cp => !cp.IsDeleted))
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
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(opportunity);

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

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(entity);

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

        // WHY Section - Update strategic properties

        if (request.Challenges != null)
        {
            entity.Challenges = request.Challenges;
        }

        if (request.ResultsFocus != null)
        {
            entity.ResultsFocus = request.ResultsFocus;
        }

        if (request.ExpectedImpact != null)
        {
            // Truncate to 510 characters (database column limit)
            entity.ExpectedImpact = request.ExpectedImpact.Length > 510 
                ? request.ExpectedImpact[..510] 
                : request.ExpectedImpact;
        }

        if (request.ExpectedOutcomes != null)
        {
            // Truncate to 510 characters (database column limit)
            entity.ExpectedOutcomes = request.ExpectedOutcomes.Length > 510 
                ? request.ExpectedOutcomes[..510] 
                : request.ExpectedOutcomes;
        }

        if (request.ExpectedBeneficiaries != null)
        {
            entity.ExpectedBeneficiaries = request.ExpectedBeneficiaries;
        }

        if (request.EstimatedDirectBeneficiaries.HasValue)
        {
            entity.EstimatedDirectBeneficiaries = request.EstimatedDirectBeneficiaries.Value;
        }

        if (request.EstimatedIndirectBeneficiaries.HasValue)
        {
            entity.EstimatedIndirectBeneficiaries = request.EstimatedIndirectBeneficiaries.Value;
        }

        if (request.BeneficiariesToBeDetermined.HasValue)
        {
            entity.BeneficiariesToBeDetermined = request.BeneficiariesToBeDetermined.Value;
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

            // Add new funding partners with amounts if provided - using exchange rate conversion
            var fundingPartners = new List<OpportunityFundingPartner>();
            
            foreach (var fp in request.FundingPartners)
            {
                var currencyId = fp.CurrencyId ?? defaultCurrencyId;
                var currency = await context.Currencies.FindAsync(currencyId);
                var amount = fp.Amount ?? fp.FundedAmount; // Use Amount or FundedAmount alias
                
                var fundingPartner = new OpportunityFundingPartner
                {
                    OpportunityId = id,
                    PartnerId = fp.PartnerId,
                    Amount = amount,
                    Percentage = fp.Percentage,
                    CurrencyId = currencyId,
                    FeePercentage = fp.FeePercentage,
                    FeeAmount = fp.FeeAmount,
                    FeeAmountUSD = fp.FeeAmountUSD,
                    IsAmountBasedFee = fp.IsAmountBasedFee,
                    PartnershipAgreementReference = fp.PartnershipAgreementReference,
                    DocumentId = fp.DocumentId,
                    IsPooledContribution = fp.IsPooledContribution,
                    SelectedPartnerAgreementNumber = fp.SelectedPartnerAgreementNumber
                };
                
                // Convert amount to USD if amount is provided (same logic as UpdateWhoSectionAsync)
                if (amount.HasValue && amount.Value > 0 && currency != null)
                {
                    try
                    {
                        var conversionResult = await _exchangeRateService.ConvertToUSDAsync(
                            amount.Value, 
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
                        fundingPartner.AmountUSD = amount.Value;
                        fundingPartner.ExchangeRate = 1.0m;
                        fundingPartner.ExchangeRateDate = DateTime.UtcNow;
                    }
                }
                
                fundingPartners.Add(fundingPartner);
            }
            
            entity.FundingPartners = fundingPartners;
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

        if (request.Stakeholders != null && request.Stakeholders.Any())
        {
            // Get the "Opportunity Manager" role ID to preserve it
            var opportunityManagerRole = await context.Set<EntityRole>()
                .Where(er => er.EntityType == "Opportunity" && er.Name == "Opportunity Manager" && !er.IsDeleted)
                .FirstOrDefaultAsync();
            
            var opportunityManagerRoleId = opportunityManagerRole?.Id;
            
            // Find existing Opportunity Manager (to preserve if AI doesn't provide one)
            OpportunityStakeholder? existingOpportunityManager = null;
            if (opportunityManagerRoleId.HasValue && entity.Stakeholders != null)
            {
                existingOpportunityManager = entity.Stakeholders
                    .FirstOrDefault(s => s.EntityRoleId == opportunityManagerRoleId.Value && s.UserId.HasValue);
            }
            
            // Check if AI-extracted stakeholders include an Opportunity Manager
            var aiHasOpportunityManager = opportunityManagerRoleId.HasValue && 
                request.Stakeholders.Any(s => s.EntityRoleId == opportunityManagerRoleId.Value && s.UserId.HasValue);
            
            // Remove existing stakeholders EXCEPT Opportunity Manager if AI doesn't provide one
            if (entity.Stakeholders != null && entity.Stakeholders.Any())
            {
                var stakeholdersToRemove = entity.Stakeholders
                    .Where(s => 
                        // Remove all if AI provides Opportunity Manager
                        aiHasOpportunityManager ||
                        // Otherwise, keep the existing Opportunity Manager
                        (opportunityManagerRoleId.HasValue && s.EntityRoleId != opportunityManagerRoleId.Value))
                    .ToList();
                
                if (stakeholdersToRemove.Any())
                {
                    context.Set<OpportunityStakeholder>().RemoveRange(stakeholdersToRemove);
                }
            }

            // Add new stakeholders from AI (with proper userId and entityRoleId)
            var newStakeholders = request.Stakeholders
                .Where(s => s.UserId.HasValue) // Only add stakeholders with valid user IDs
                .Select(s => new OpportunityStakeholder
                {
                    OpportunityId = id,
                    UserId = s.UserId,
                    EntityRoleId = s.EntityRoleId,
                    IsInternal = true,
                    StakeholderType = "Internal",
                    Notes = s.Notes
                })
                .ToList();

            // Initialize stakeholders list if null
            entity.Stakeholders ??= new List<OpportunityStakeholder>();
            
            // If AI doesn't have Opportunity Manager but we have one, keep it
            if (!aiHasOpportunityManager && existingOpportunityManager != null)
            {
                // Filter out any existing stakeholders that are the preserved Opportunity Manager
                entity.Stakeholders = entity.Stakeholders
                    .Where(s => s.Id == existingOpportunityManager.Id)
                    .ToList();
            }
            else
            {
                // Clear for fresh add
                entity.Stakeholders = new List<OpportunityStakeholder>();
            }
            
            // Add all new stakeholders
            foreach (var stakeholder in newStakeholders)
            {
                entity.Stakeholders.Add(stakeholder);
            }
        }

        if (request.MiscExternalStakeholders != null)
        {
            entity.MiscExternalStakeholders = request.MiscExternalStakeholders;
        }

        if (request.ExternalStakeholderNotes != null)
        {
            entity.ExternalStakeholderNotes = request.ExternalStakeholderNotes;
        }

        // WHERE Section - Update countries (same logic as UpdateWhereSectionAsync)
        if (request.Countries != null)
        {
            // Remove existing countries
            if (entity.Countries != null && entity.Countries.Any())
            {
                context.Set<OpportunityCountry>().RemoveRange(entity.Countries);
            }

            // Compute OrgUnitWithStrategyId for each country (same as UpdateWhereSectionAsync)
            var countryOrgUnitStrategyMap = await ComputeOrgUnitWithStrategyForCountriesAsync(request.Countries);

            // Add new countries with OrgUnitWithStrategyId computed
            entity.Countries = request.Countries
                .Select(countryId => new OpportunityCountry
                {
                    OpportunityId = id,
                    CountryId = countryId,
                    OrgUnitWithStrategyId = countryOrgUnitStrategyMap.ContainsKey(countryId) 
                        ? countryOrgUnitStrategyMap[countryId] 
                        : null
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

        if (request.ImplementationStartDate.HasValue)
        {
            entity.ImplementationStartDate = request.ImplementationStartDate.Value;
        }

        if (request.SubmissionDeadline.HasValue)
        {
            entity.SubmissionDeadline = request.SubmissionDeadline.Value;
        }

        if (request.IsTargetSigningDateFirm.HasValue)
        {
            entity.IsTargetSigningDateFirm = request.IsTargetSigningDateFirm.Value;
        }

        if (request.SigningDateNotes != null)
        {
            entity.SigningDateNotes = request.SigningDateNotes;
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

        if (!string.IsNullOrEmpty(request.Stage))
        {
            entity.Stage = request.Stage;
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

    /// <summary>
    /// Creates an opportunity from AI-generated proposal with user-accepted fields
    /// Handles deduplication, context partner inclusion, and exchange rate calculations
    /// </summary>
    /// <param name="request">Create request with accepted fields and resolved IDs</param>
    /// <param name="currentUserId">Current user ID for assignment as Opportunity Manager</param>
    /// <returns>Created opportunity model</returns>
    public async Task<OpportunityModel> CreateOpportunityFromProposalAsync(
        CreateOpportunityFromInteractionsRequest request,
        int currentUserId)
    {
        // Deduplicate SDGs by ID (plain integer array)
        var uniqueSdGs = request.SdGs?.Distinct().ToList() ?? new List<int>();
        
        // Deduplicate Countries by ID (plain integer array)
        var uniqueCountries = request.Countries?.Distinct().ToList() ?? new List<int>();
        
        // Deduplicate Stakeholders by UserId + EntityRoleId combination
        var uniqueStakeholders = request.Stakeholders?
            .GroupBy(s => new { s.UserId, s.EntityRoleId })
            .Select(g => g.First())
            .ToList() ?? new List<OpportunityStakeholderRequest>();
        
        // Build opportunity request from accepted proposal
        var opportunityRequest = new OpportunityRequest
        {
            Name = request.Name,
            Description = request.Description,
            PartnerReference = request.PartnerReference,
            ResponsibleOrgUnitId = request.ResponsibleOrgUnitId,
            ProposedInitiativeTypeId = request.ProposedInitiativeTypeId,
            DeliveryModality = request.DeliveryModality,
            InitiativeBudgetUSD = request.InitiativeBudgetUSD,
            TargetSigningDate = request.TargetSigningDate,
            TargetDeliveryDate = request.TargetDeliveryDate,
            Challenges = request.Challenges,
            ResultsFocus = request.ResultsFocus,
            // Truncate to 510 characters (database column limit)
            ExpectedImpact = request.ExpectedImpact?.Length > 510 
                ? request.ExpectedImpact[..510] 
                : request.ExpectedImpact,
            ExpectedOutcomes = request.ExpectedOutcomes?.Length > 510 
                ? request.ExpectedOutcomes[..510] 
                : request.ExpectedOutcomes,
            ExpectedBeneficiaries = request.ExpectedBeneficiaries,
            EstimatedDirectBeneficiaries = request.EstimatedDirectBeneficiaries,
            EstimatedIndirectBeneficiaries = request.EstimatedIndirectBeneficiaries,
            BeneficiariesToBeDetermined = request.BeneficiariesToBeDetermined ?? false,
            MiscExternalStakeholders = request.MiscExternalStakeholders,
            ExternalStakeholderNotes = request.ExternalStakeholderNotes,
            SDGs = uniqueSdGs.Select(sdgId => new OpportunitySDGRequest { SDGId = sdgId }).ToList(),
            Countries = uniqueCountries.Select(countryId => new OpportunityCountryRequest { CountryId = countryId }).ToList(),
            Deliverables = request.Deliverables ?? new List<OpportunityDeliverableRequest>(),
            Stakeholders = uniqueStakeholders,
            FundingPartners = new List<OpportunityFundingPartnerRequest>(),
            ClientPartners = new List<OpportunityClientPartnerRequest>()
        };

        // Deduplicate funding partners by PartnerId (keep first occurrence with all its properties)
        var uniqueFundingPartners = request.FundingPartners?
            .GroupBy(fp => fp.PartnerId)
            .Select(g => g.First())
            .ToList() ?? new List<OpportunityFundingPartnerRequest>();
        
        // Deduplicate client partners by PartnerId (keep first occurrence)
        var uniqueClientPartners = request.ClientPartners?
            .GroupBy(cp => cp.PartnerId)
            .Select(g => g.First())
            .ToList() ?? new List<OpportunityClientPartnerRequest>();

        // Add the context partner as funding/client based on user selection (only if partnerId provided)
        // This ensures the context partner is included even if not in the AI-proposed arrays
        if (request.PartnerId.HasValue && request.PartnerId > 0)
        {
            // Check if context partner is already in the deduplicated AI-proposed arrays
            var contextPartnerInFunding = uniqueFundingPartners.Any(fp => fp.PartnerId == request.PartnerId.Value);
            var contextPartnerInClient = uniqueClientPartners.Any(cp => cp.PartnerId == request.PartnerId.Value);
            
            // Add to funding partners if user selected funding role and not already in array
            if (request.IsFundingPartner && !contextPartnerInFunding)
            {
                opportunityRequest.FundingPartners.Add(new OpportunityFundingPartnerRequest
                {
                    PartnerId = request.PartnerId.Value,
                    Amount = null // User can set later
                });
            }
            
            // Add to client partners if user selected client role and not already in array
            if (request.IsClientPartner && !contextPartnerInClient)
            {
                opportunityRequest.ClientPartners.Add(new OpportunityClientPartnerRequest
                {
                    PartnerId = request.PartnerId.Value
                });
            }
        }

        // Add all deduplicated AI-proposed funding partners
        if (uniqueFundingPartners.Any())
        {
            opportunityRequest.FundingPartners.AddRange(uniqueFundingPartners);
        }

        // Add all deduplicated AI-proposed client partners  
        if (uniqueClientPartners.Any())
        {
            opportunityRequest.ClientPartners.AddRange(uniqueClientPartners);
        }

        // Create the opportunity using existing CreateOpportunityAsync
        var createdOpportunity = await CreateOpportunityAsync(opportunityRequest);

        // Assign the current user as Opportunity Manager
        try
        {
            await AssignCreatorAsOpportunityManagerAsync(createdOpportunity.Id, currentUserId);
        }
        catch (Exception)
        {
            // Don't fail if assignment fails - log handled by caller
        }

        return createdOpportunity;
    }

    public async Task<bool> DeleteOpportunityAsync(int id)
    {
        var entity = await opportunityRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return false;
        }

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(entity);

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
            ExternalStakeholderCount = opportunity.Stakeholders?.Count(s => !s.IsInternal) ?? 0,
            ServiceLines = new List<string>() // Initialize empty list
        };

        // Calculate total partner count (unique partners from both funding and client)
        var allPartnerIds = new HashSet<int>();
        if (opportunity.FundingPartners != null)
        {
            foreach (var fp in opportunity.FundingPartners)
            {
                allPartnerIds.Add(fp.PartnerId);
            }
        }
        if (opportunity.ClientPartners != null)
        {
            foreach (var cp in opportunity.ClientPartners)
            {
                allPartnerIds.Add(cp.PartnerId);
            }
        }
        stats.TotalPartnerCount = allPartnerIds.Count;

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

        // Calculate days to target signing date
        if (opportunity.TargetSigningDate.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            var signingDate = opportunity.TargetSigningDate.Value.Date;
            stats.DaysToTargetSigningDate = (int)(signingDate - today).TotalDays;
        }

        // Extract unique service lines from deliverables
        if (opportunity.Deliverables != null && opportunity.Deliverables.Any())
        {
            var serviceLines = opportunity.Deliverables
                .Where(d => d.Output != null && !string.IsNullOrEmpty(d.Output.ServiceLine))
                .Select(d => d.Output!.ServiceLine)
                .Distinct()
                .OrderBy(sl => sl)
                .ToList();
            
            // Only update if we found any service lines
            if (serviceLines.Any())
            {
                stats.ServiceLines = serviceLines!;
            }
        }

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
    /// Includes ALL data from implemented interfaces: Risks, DST Analysis, Insights, Suggestions, etc.
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    /// <returns>Dictionary containing all opportunity details formatted for AI prompt placeholders</returns>
    public async Task<Dictionary<string, object>> GetOpportunityDetailsForAIAsync(int id)
    {
        // ==========================================
        // PERFORMANCE OPTIMIZATION: Parallel query execution using DbContextFactory
        // PRIORITY 5: Task.WhenAll() with separate DbContext instances (20-30% additional improvement)
        // Using AsNoTracking() for read-only operations
        // ==========================================
        
        // ==========================================
        // QUERY 1: Main Opportunity with Simple Navigation Properties Only (MUST run first)
        // ==========================================
        var opportunity = await context.Set<Opportunity>()
            .AsNoTracking() // No change tracking needed for AI data processing
            .Include(o => o.ResponsibleOrgUnit)
            .Include(o => o.ProposedInitiativeType)
            .Include(o => o.CreatedByUser)
            .Include(o => o.LastModifiedByUser)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {id} not found");
        }

        // ==========================================
        // PARALLEL WAVE 1: Execute 10 independent queries concurrently
        // Each task uses its own DbContext instance for thread safety
        // ==========================================
        List<OpportunityFundingPartner> fundingPartners;
        List<OpportunityClientPartner> clientPartners;
        List<OpportunityStakeholder> stakeholders;
        List<OpportunityExternalStakeholder> externalStakeholders;
        List<OpportunityDeliverable> deliverables;
        List<OpportunityCountry> countries;
        List<OpportunitySDG> sdgs;
        List<OpportunityUNCFOutcome> uncfOutcomes;
        List<OpportunityUNOPSMission> unopsMissions;
        List<Domain.Entities.Risk> risks;

        // Execute all independent queries in parallel using separate DbContext instances
        // CRITICAL: Filter out soft-deleted records in all parallel queries
        var task1 = Task.Run(async () => 
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return await ctx.Set<OpportunityFundingPartner>()
                .AsNoTracking()
                .Where(fp => fp.OpportunityId == id && !fp.IsDeleted)
                .Include(fp => fp.Partner)
                .Include(fp => fp.Currency)
                .Include(fp => fp.Document)
                .ToListAsync();
        });

        var task2 = Task.Run(async () => 
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return await ctx.Set<OpportunityClientPartner>()
                .AsNoTracking()
                .Where(cp => cp.OpportunityId == id && !cp.IsDeleted)
                .Include(cp => cp.Partner)
                .Include(cp => cp.Document)
                .ToListAsync();
        });

        var task3 = Task.Run(async () => 
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return await ctx.Set<OpportunityStakeholder>()
                .AsNoTracking()
                .Where(s => s.OpportunityId == id && !s.IsDeleted)
                .Include(s => s.User).ThenInclude(u => u.UserProfile)
                .Include(s => s.EntityRole)
                .Include(s => s.OrganizationHierarchy)
                .ToListAsync();
        });

        var task4 = Task.Run(async () => 
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return await ctx.Set<OpportunityExternalStakeholder>()
                .AsNoTracking()
                .Where(es => es.OpportunityId == id && !es.IsDeleted)
                .Include(es => es.Contact).ThenInclude(c => c.Partner)
                .ToListAsync();
        });

        var task5 = Task.Run(async () => 
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return await ctx.Set<OpportunityDeliverable>()
                .AsNoTracking()
                .Where(d => d.OpportunityId == id && !d.IsDeleted)
                .Include(d => d.Output)
                .ToListAsync();
        });

        var task6 = Task.Run(async () => 
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return await ctx.Set<OpportunityCountry>()
                .AsNoTracking()
                .Where(c => c.OpportunityId == id && !c.IsDeleted)
                .Include(c => c.Country)
                .ToListAsync();
        });

        var task7 = Task.Run(async () => 
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return await ctx.Set<OpportunitySDG>()
                .AsNoTracking()
                .Where(s => s.OpportunityId == id && !s.IsDeleted)
                .Include(s => s.SDG)
                .ToListAsync();
        });

        var task8 = Task.Run(async () => 
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return await ctx.Set<OpportunityUNCFOutcome>()
                .AsNoTracking()
                .Where(u => u.OpportunityId == id && !u.IsDeleted)
                .Include(u => u.UNCFOutcome)
                .ToListAsync();
        });

        var task9 = Task.Run(async () => 
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return await ctx.Set<OpportunityUNOPSMission>()
                .AsNoTracking()
                .Where(m => m.OpportunityId == id && !m.IsDeleted)
                .Include(m => m.UNOPSMission)
                .ToListAsync();
        });

        var task10 = Task.Run(async () => 
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return await ctx.Set<Domain.Entities.Risk>()
                .AsNoTracking()
                .Include(r => r.RiskTypeEntity)
                .Include(r => r.RiskCategory)
                .Include(r => r.RiskProbabilityEntity)
                .Include(r => r.RiskProximityEntity)
                .Include(r => r.RiskImpactLevelEntity)
                .Include(r => r.RiskResponseTypeEntity)
                .Include(r => r.PreDefinedHighRisk)
                .Where(r => r.EntityType == "Opportunity" && r.EntityId == id && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        });

        // Wait for all parallel queries to complete
        await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10);
        
        // Assign results
        fundingPartners = await task1;
        clientPartners = await task2;
        stakeholders = await task3;
        externalStakeholders = await task4;
        deliverables = await task5;
        countries = await task6;
        sdgs = await task7;
        uncfOutcomes = await task8;
        unopsMissions = await task9;
        risks = await task10;

        // ==========================================
        // PARALLEL WAVE 2: Dependent queries (require results from Wave 1)
        // ==========================================
        var sdgIds = sdgs.Select(s => s.Id).ToList();
        var outcomeIds = uncfOutcomes.Select(u => u.Id).ToList();

        List<OpportunitySDGTarget> sdgTargets;
        List<OpportunitySDGIndicator> sdgIndicators;
        List<OpportunityUNCFIndicator> uncfIndicators;

        if (sdgIds.Any() || outcomeIds.Any())
        {
            var dependentTasks = new List<Task>();
            
            // Task 1: SDG Targets (if SDGs exist)
            Task<List<OpportunitySDGTarget>>? sdgTargetsTask = null;
            if (sdgIds.Any())
            {
                sdgTargetsTask = Task.Run(async () =>
                {
                    await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                    return await ctx.Set<OpportunitySDGTarget>()
                        .AsNoTracking()
                        .Where(t => sdgIds.Contains(t.OpportunitySDGId))
                        .Include(t => t.SDGTarget)
                        .ToListAsync();
                });
                dependentTasks.Add(sdgTargetsTask);
            }
            
            // Task 2: UNCF Indicators (if outcomes exist)
            Task<List<OpportunityUNCFIndicator>>? uncfIndicatorsTask = null;
            if (outcomeIds.Any())
            {
                uncfIndicatorsTask = Task.Run(async () =>
                {
                    await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                    return await ctx.Set<OpportunityUNCFIndicator>()
                        .AsNoTracking()
                        .Where(i => outcomeIds.Contains(i.OpportunityUNCFOutcomeId))
                        .Include(i => i.UNCFIndicator)
                        .ToListAsync();
                });
                dependentTasks.Add(uncfIndicatorsTask);
            }

            // Wait for dependent queries
            await Task.WhenAll(dependentTasks);

            // Get SDG Targets result
            sdgTargets = sdgTargetsTask != null ? await sdgTargetsTask : new List<OpportunitySDGTarget>();
            uncfIndicators = uncfIndicatorsTask != null ? await uncfIndicatorsTask : new List<OpportunityUNCFIndicator>();

            // Task 3: SDG Indicators (depends on SDG Targets)
            var targetIds = sdgTargets.Select(t => t.Id).ToList();
            sdgIndicators = targetIds.Any()
                ? await Task.Run(async () =>
                {
                    await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                    return await ctx.Set<OpportunitySDGIndicator>()
                        .AsNoTracking()
                        .Where(i => targetIds.Contains(i.OpportunitySDGTargetId))
                        .Include(i => i.SDGIndicator)
                        .ToListAsync();
                })
                : new List<OpportunitySDGIndicator>();
        }
        else
        {
            // No SDGs or UNCF outcomes, initialize empty collections
            sdgTargets = new List<OpportunitySDGTarget>();
            sdgIndicators = new List<OpportunitySDGIndicator>();
            uncfIndicators = new List<OpportunityUNCFIndicator>();
        }

        // Assign collections back to opportunity object for ComputeOpportunityStats
        opportunity.FundingPartners = fundingPartners;
        opportunity.ClientPartners = clientPartners;
        opportunity.Stakeholders = stakeholders;
        opportunity.ExternalStakeholders = externalStakeholders;
        opportunity.Deliverables = deliverables;
        opportunity.Countries = countries;
        opportunity.SDGs = sdgs;
        opportunity.SDGTargets = sdgTargets;
        opportunity.SDGIndicators = sdgIndicators;
        opportunity.UNCFOutcomes = uncfOutcomes;
        opportunity.UNCFIndicators = uncfIndicators;
        opportunity.UNOPSMissions = unopsMissions;

        var stats = ComputeOpportunityStats(opportunity);
        
        // ==========================================
        // PROCESS RISK REGISTER DATA (already loaded in parallel Wave 1)
        // ==========================================
        var risksDetails = risks.Select(r => new
        {
            Title = r.Title ?? "Untitled Risk",
            Description = r.Description ?? "",
            Recommendation = r.Recommendation ?? "",
            RiskType = r.RiskTypeEntity?.Name ?? "Unknown",
            RiskCategory = r.RiskCategory?.Name ?? "Unknown",
            RiskCategoryCode = r.RiskCategory?.Code ?? "Unknown",
            RiskCategoryShortCode = r.RiskCategory?.ShortCode ?? "Unknown",
            RiskCategoryLevel = r.RiskCategory?.Level ?? 0,
            Probability = r.RiskProbabilityEntity?.Name ?? "Unknown",
            ProbabilityValue = r.RiskProbabilityEntity?.NumericValue ?? 0,
            Proximity = r.RiskProximityEntity?.Name ?? "Unknown",
            ImpactLevel = r.RiskImpactLevelEntity?.Name ?? "Unknown",
            ImpactValue = r.RiskImpactLevelEntity?.NumericValue ?? 0,
            ResponseType = r.RiskResponseTypeEntity?.Name ?? "Not specified",
            IsPreDefinedHighRisk = r.PreDefinedHighRiskId.HasValue,
            PreDefinedHighRiskCode = r.PreDefinedHighRisk?.Code ?? "",
            PreDefinedHighRiskTitle = r.PreDefinedHighRisk?.ShortTitle ?? "",
            IdentifiedDate = r.IdentifiedDate?.ToString("yyyy-MM-dd") ?? "",
            IdentifiedBy = r.IdentifiedBy?.ToString() ?? ""
        }).ToList();
        
        var risksText = risksDetails.Any()
            ? string.Join("\n", risksDetails.Select(r =>
                $"- [{r.RiskType}] {r.Title}" +
                (string.IsNullOrEmpty(r.Description) ? "" : $"\n  Description: {r.Description}") +
                (string.IsNullOrEmpty(r.Recommendation) ? "" : $"\n  Recommendation: {r.Recommendation}") +
                $"\n  Category: {r.RiskCategory} ({r.RiskCategoryShortCode}, Level {r.RiskCategoryLevel})" +
                $"\n  Probability: {r.Probability} (Value: {r.ProbabilityValue})" +
                $"\n  Impact: {r.ImpactLevel} (Value: {r.ImpactValue})" +
                $"\n  Proximity: {r.Proximity}" +
                (string.IsNullOrEmpty(r.ResponseType) || r.ResponseType == "Not specified" ? "" : $"\n  Response: {r.ResponseType}") +
                (r.IsPreDefinedHighRisk ? $"\n  Pre-Defined High Risk: {r.PreDefinedHighRiskCode} - {r.PreDefinedHighRiskTitle}" : "") +
                (string.IsNullOrEmpty(r.IdentifiedDate) ? "" : $"\n  Identified: {r.IdentifiedDate} by {r.IdentifiedBy}")))
            : "No risks identified";
        
        // ==========================================
        // LOAD SME SELECTIONS
        // ==========================================
        var smeSelections = await GetSMESelectionsAsync(id);
        var smeSelectionsText = smeSelections.Any()
            ? string.Join("\n", smeSelections
                .Where(s => s.IsSelected && !string.IsNullOrEmpty(s.UserName))
                .Select(s => $"- {s.EntityRoleName}: {s.UserName} ({s.UserEmail})"))
            : "No SME selections made";
        
        // ==========================================
        // LOAD PARTNER AGREEMENTS (for context) - OPTIMIZED TO ELIMINATE N+1 QUERIES
        // PRIORITY 3 OPTIMIZATION: Batch query for all partner agreements
        // ==========================================
        var partnerAgreementsSummary = new List<string>();
        if (fundingPartners != null && fundingPartners.Any())
        {
            var opportunityCountryIds = countries?.Select(c => c.CountryId).ToList() ?? new List<int>();
            
            // Get all partner IDs upfront (limit to first 5 for brevity)
            var partnerIds = fundingPartners.Take(5).Select(fp => fp.PartnerId).ToList();
            
            // BATCH QUERY 1: Get ERP dimension values for ALL partners in ONE query
            var partnerErpValues = await context.Partners
                .AsNoTracking()
                .Where(p => partnerIds.Contains(p.Id) && p.ErpDimValue.HasValue)
                .Select(p => new { p.Id, ErpDimValueString = p.ErpDimValue.Value.ToString() })
                .ToListAsync();
            
            if (partnerErpValues.Any())
            {
                var partnerNumbers = partnerErpValues.Select(p => p.ErpDimValueString).ToList();
                
                // BATCH QUERY 2: Load ALL agreements for ALL partners in ONE query
                var allPartnerAgreements = await context.PartnerAgreements
                    .AsNoTracking()
                    .Where(pa => partnerNumbers.Contains(pa.PartnerAgreementPartner) && !pa.IsDeleted)
                    .OrderByDescending(pa => pa.PartnerAgreementStartDate)
                    .ToListAsync();
                
                // Group agreements by partner for processing
                var agreementsByPartner = allPartnerAgreements
                    .Where(pa => !string.IsNullOrEmpty(pa.PartnerAgreementPartner))
                    .GroupBy(pa => pa.PartnerAgreementPartner!)
                    .ToDictionary(g => g.Key, g => g.ToList());
                
                // Process each funding partner with their pre-loaded agreements
                foreach (var fp in fundingPartners.Take(5))
                {
                    var partnerErp = partnerErpValues.FirstOrDefault(p => p.Id == fp.PartnerId);
                    if (partnerErp != null && agreementsByPartner.TryGetValue(partnerErp.ErpDimValueString, out var partnerAgreements))
                    {
                        // Filter agreements based on dates (in-memory, already loaded)
                        var relevantAgreements = partnerAgreements
                            .Where(pa => 
                                pa.PartnerAgreementStartDate.HasValue && 
                                pa.PartnerAgreementEndDate.HasValue &&
                                opportunity.TargetDeliveryDate.HasValue &&
                                pa.PartnerAgreementStartDate <= opportunity.CreatedDate &&
                                pa.PartnerAgreementEndDate >= opportunity.TargetDeliveryDate.Value)
                            .ToList();
                        
                        if (relevantAgreements.Any())
                        {
                            var agreementInfo = string.Join("; ", relevantAgreements.Take(2).Select(a =>
                                $"{a.Name} ({a.PartnerAgreementType}, {a.PartnerAgreementStartDate?.ToString("yyyy-MM-dd") ?? "N/A"} to {a.PartnerAgreementEndDate?.ToString("yyyy-MM-dd") ?? "N/A"})"));
                            partnerAgreementsSummary.Add($"{fp.Partner?.Name ?? "Unknown"}: {agreementInfo}");
                        }
                        else if (partnerAgreements.Any())
                        {
                            // Include any agreements even if they don't match date criteria
                            var agreementInfo = string.Join("; ", partnerAgreements.Take(2).Select(a =>
                                $"{a.Name} ({a.PartnerAgreementType}, {a.PartnerAgreementStartDate?.ToString("yyyy-MM-dd") ?? "N/A"} to {a.PartnerAgreementEndDate?.ToString("yyyy-MM-dd") ?? "N/A"})"));
                            partnerAgreementsSummary.Add($"{fp.Partner?.Name ?? "Unknown"}: {agreementInfo}");
                        }
                    }
                }
            }
        }
        
        var partnerAgreementsText = partnerAgreementsSummary.Any()
            ? string.Join("\n", partnerAgreementsSummary)
            : "No partner agreements loaded";

        // Format funding partners with detailed information
        var fundingPartnersDetails = opportunity.FundingPartners?
            .Select(fp => new
            {
                PartnerName = fp.Partner?.Name ?? "Unknown",
                Amount = fp.Amount?.ToString("N2") ?? "Not specified",
                Currency = fp.Currency?.Code ?? "USD",
                AmountUSD = fp.AmountUSD?.ToString("N2"),
                Percentage = fp.Percentage?.ToString("N2") + "%",
                FeePercentage = fp.FeePercentage?.ToString("N2") + "%",
                FeeAmount = fp.FeeAmount?.ToString("N2"),
                FeeAmountUSD = fp.FeeAmountUSD?.ToString("N2"),
                CommitmentStatus = fp.CommitmentStatus ?? "Not specified",
                PartnershipAgreementReference = fp.PartnershipAgreementReference ?? "",
                IsPooledContribution = fp.IsPooledContribution ? "Yes" : "No",
                DocumentName = fp.Document?.Name ?? ""
            })
            .ToList();

        var fundingPartnersText = fundingPartnersDetails != null && fundingPartnersDetails.Any()
            ? string.Join("\n", fundingPartnersDetails.Select(fp =>
                $"- {fp.PartnerName}: {fp.Amount} {fp.Currency} (USD: {fp.AmountUSD ?? "Not converted"}), " +
                $"Commitment: {fp.CommitmentStatus}, Fee: {fp.FeePercentage}, Pooled: {fp.IsPooledContribution}"))
            : "No funding partners";

        // Format client partners with detailed information
        var clientPartnersDetails = opportunity.ClientPartners?
            .Select(cp => new
            {
                PartnerName = cp.Partner?.Name ?? "Unknown",
                PartnerStatus = cp.Partner?.Status.ToString() ?? "",
                DocumentName = cp.Document?.Name ?? ""
            })
            .ToList();

        var clientPartnersText = clientPartnersDetails != null && clientPartnersDetails.Any()
            ? string.Join("\n", clientPartnersDetails.Select(cp => $"- {cp.PartnerName} (Status: {cp.PartnerStatus})"))
            : "No client partners";

        // Format stakeholders with detailed information
        var stakeholdersDetails = opportunity.Stakeholders?
            .Select(s => new
            {
                UserName = s.User?.Name ?? "Unknown",
                UserEmail = s.User?.Email ?? "",
                RoleName = s.EntityRole?.Name ?? "No role",
                RoleCode = s.EntityRole?.Code ?? "",
                OrgUnitName = s.OrganizationHierarchy?.Name ?? "",
                IsAutoPopulated = s.IsAutoPopulated ? "Auto-assigned" : "Manually assigned",
                Notes = s.Notes ?? ""
            })
            .ToList();

        var stakeholdersText = stakeholdersDetails != null && stakeholdersDetails.Any()
            ? string.Join("\n", stakeholdersDetails.Select(s =>
                $"- {s.UserName} ({s.UserEmail}): {s.RoleName} [{s.IsAutoPopulated}]" +
                (string.IsNullOrEmpty(s.OrgUnitName) ? "" : $", Org Unit: {s.OrgUnitName}") +
                (string.IsNullOrEmpty(s.Notes) ? "" : $", Notes: {s.Notes}")))
            : "No internal stakeholders";

        // Format external stakeholders
        var externalStakeholdersDetails = opportunity.ExternalStakeholders?
            .Select(es => new
            {
                ContactName = es.Contact?.Name ?? "Unknown",
                ContactEmail = es.Contact?.Email ?? "",
                PartnerName = es.Contact?.Partner?.Name ?? ""
            })
            .ToList();

        var externalStakeholdersText = externalStakeholdersDetails != null && externalStakeholdersDetails.Any()
            ? string.Join("\n", externalStakeholdersDetails.Select(es =>
                $"- {es.ContactName} ({es.ContactEmail})" +
                (string.IsNullOrEmpty(es.PartnerName) ? "" : $" from {es.PartnerName}")))
            : "No external stakeholders";

        // Format deliverables with detailed information
        var deliverablesDetails = opportunity.Deliverables?
            .Select(d => new
            {
                OutputName = d.Output?.Name ?? "Not specified",
                Level0 = d.Output?.Level0 ?? "",
                Level1 = d.Output?.Level1 ?? "",
                Level2 = d.Output?.Level2 ?? "",
                Level3 = d.Output?.Level3 ?? "",
                Level4 = d.Output?.Level4 ?? "",
                ServiceLine = d.Output?.ServiceLine ?? "",
                Quantity = d.Quantity?.ToString() ?? "Not specified",
                PlannedStartDate = d.PlannedStartDate?.Date.ToString("yyyy-MM-dd") ?? "",
                PlannedEndDate = d.PlannedEndDate?.Date.ToString("yyyy-MM-dd") ?? "",
                Notes = d.Notes ?? ""
            })
            .ToList();

        var deliverablesText = deliverablesDetails != null && deliverablesDetails.Any()
            ? string.Join("\n", deliverablesDetails.Select(d =>
                $"- {d.OutputName}" +
                (string.IsNullOrEmpty(d.ServiceLine) ? "" : $" (Service Line: {d.ServiceLine})") +
                (d.Quantity != "Not specified" ? $", Quantity: {d.Quantity}" : "") +
                (string.IsNullOrEmpty(d.PlannedStartDate) ? "" : $", Start: {d.PlannedStartDate}") +
                (string.IsNullOrEmpty(d.PlannedEndDate) ? "" : $", End: {d.PlannedEndDate}") +
                (string.IsNullOrEmpty(d.Notes) ? "" : $", Notes: {d.Notes}")))
            : "No deliverables";

        // Format countries with detailed information
        var countriesDetails = opportunity.Countries?
            .Select(c => new
            {
                CountryName = c.Country?.Name ?? "Unknown",
                Iso2Code = c.Country?.Iso2Code ?? "",
                Continent = c.Country?.ContinentDescription ?? "",
                Region = c.Country?.RegionDescription ?? "",
                SpecificAreas = c.SpecificAreas ?? "",
                RiskScore = c.RiskScore?.ToString() ?? "Not assessed",
                HumanitarianFrameworkAlignment = c.HumanitarianFrameworkAlignment.HasValue
                    ? (c.HumanitarianFrameworkAlignment.Value ? "Aligned" : "Not aligned")
                    : "Not assessed",
                NdcAlignment = c.NdcAlignment.HasValue
                    ? (c.NdcAlignment.Value ? "Aligned" : "Not aligned")
                    : "Not assessed",
                NapAlignment = c.NapAlignment.HasValue
                    ? (c.NapAlignment.Value ? "Aligned" : "Not aligned")
                    : "Not assessed",
                OrgUnitStrategyAlignment = c.OrgUnitStrategyAlignment.HasValue
                    ? (c.OrgUnitStrategyAlignment.Value ? "Aligned" : "Not aligned")
                    : "Not assessed"
            })
            .ToList();

        var countriesText = countriesDetails != null && countriesDetails.Any()
            ? string.Join("\n", countriesDetails.Select(c =>
                $"- {c.CountryName} ({c.Iso2Code})" +
                (string.IsNullOrEmpty(c.Region) ? "" : $", Region: {c.Region}") +
                (string.IsNullOrEmpty(c.SpecificAreas) ? "" : $", Areas: {c.SpecificAreas}") +
                $", Risk Score: {c.RiskScore}" +
                $", Humanitarian Framework: {c.HumanitarianFrameworkAlignment}" +
                $", NDC: {c.NdcAlignment}" +
                $", NAP: {c.NapAlignment}" +
                $", Org Strategy: {c.OrgUnitStrategyAlignment}"))
            : "No countries";

        // ==========================================
        // NORMALLY RESPONSIBLE ORG UNITS ANALYSIS
        // Gets org units that are normally responsible for implementation countries
        // These may differ from the selected responsible org unit
        // ==========================================
        var normallyResponsibleOrgUnitsInfo = new List<(int OrgUnitId, string OrgUnitName, string OrgUnitCode, string CountryName)>();
        var selectedOrgUnitId = opportunity.ResponsibleOrgUnitId ?? 0;
        
        if (countries != null && countries.Any())
        {
            // Get org unit relationships for all implementation countries
            var countryIds = countries.Select(c => c.CountryId).ToList();
            
            var countryOrgUnitRelationships = await context.OrganizationUnitRelationships
                .AsNoTracking()
                .Where(r => 
                    r.EntityType == "Country" 
                    && countryIds.Contains(r.EntityId)
                    && !r.IsDeleted)
                .Include(r => r.OrganizationHierarchy)
                .ToListAsync();
            
            // Get the org unit (Type = OrgUnit, level 3) for each country
            var countryToOrgUnitMap = countryOrgUnitRelationships
                .Where(r => r.OrganizationHierarchy != null 
                         && r.OrganizationHierarchy.Type == Domain.Enums.OrganizationUnitType.OrgUnit)
                .ToDictionary(r => r.EntityId, r => r.OrganizationHierarchy!);
            
            foreach (var country in countries)
            {
                if (countryToOrgUnitMap.TryGetValue(country.CountryId, out var orgUnit))
                {
                    normallyResponsibleOrgUnitsInfo.Add((
                        OrgUnitId: orgUnit.Id,
                        OrgUnitName: orgUnit.Name,
                        OrgUnitCode: orgUnit.Code,
                        CountryName: country.Country?.Name ?? "Unknown"
                    ));
                }
            }
        }
        
        // Determine if there's a mismatch between selected org unit and normally responsible org units
        var normallyResponsibleOrgUnitIds = normallyResponsibleOrgUnitsInfo
            .Select(n => n.OrgUnitId)
            .Distinct()
            .ToList();
        
        var hasOrgUnitMismatch = selectedOrgUnitId > 0 && 
                                 normallyResponsibleOrgUnitIds.Any() && 
                                 normallyResponsibleOrgUnitIds.Any(id => id != selectedOrgUnitId);
        
        // Countries where selected org unit is NOT normally responsible
        var countriesWithDifferentOrgUnit = normallyResponsibleOrgUnitsInfo
            .Where(n => n.OrgUnitId != selectedOrgUnitId)
            .Select(n => $"{n.CountryName} (normally: {n.OrgUnitName})")
            .Distinct()
            .ToList();
        
        // Countries where selected org unit IS normally responsible
        var countriesWithMatchingOrgUnit = normallyResponsibleOrgUnitsInfo
            .Where(n => n.OrgUnitId == selectedOrgUnitId)
            .Select(n => n.CountryName)
            .Distinct()
            .ToList();
        
        var normallyResponsibleOrgUnitsText = normallyResponsibleOrgUnitsInfo.Any()
            ? string.Join("\n", normallyResponsibleOrgUnitsInfo
                .GroupBy(n => new { n.OrgUnitId, n.OrgUnitName, n.OrgUnitCode })
                .Select(g => $"- {g.Key.OrgUnitName} ({g.Key.OrgUnitCode}): {string.Join(", ", g.Select(n => n.CountryName))}"))
            : "No normally responsible org units identified";

        // Format SDGs with targets and indicators
        var sdgsDetails = opportunity.SDGs?
            .Select(s => new
            {
                SDGNumber = s.SDG?.SDGNumber ?? "",
                SDGName = s.SDG?.Name ?? "Unknown",
                IsPrimary = s.IsPrimary ? "Primary" : "Secondary",
                SkipTargets = (s.SkipTargetsAndIndicators ?? false) ? "Yes" : "No",
                Notes = s.Notes ?? "",
                Targets = opportunity.SDGTargets?
                    .Where(t => t.OpportunitySDGId == s.Id)
                    .Select(t => new
                    {
                        TargetId = t.SDGTarget?.SDGTargetId ?? "",
                        Description = t.SDGTarget?.TargetDescription ?? "",
                        Notes = t.Notes ?? "",
                        Indicators = opportunity.SDGIndicators?
                            .Where(i => i.OpportunitySDGTargetId == t.Id)
                            .Select(i => new
                            {
                                IndicatorId = i.SDGIndicator?.SDGIndicatorId ?? "",
                                Description = i.SDGIndicator?.SDGIndicatorLongDescription ?? "",
                                Notes = i.Notes ?? ""
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();

        var sdgsText = sdgsDetails != null && sdgsDetails.Any()
            ? string.Join("\n", sdgsDetails.Select(s =>
            {
                var baseText = $"- SDG {s.SDGNumber}: {s.SDGName} [{s.IsPrimary}]";
                if (s.SkipTargets == "Yes")
                {
                    return baseText + " (No specific targets/indicators)";
                }
                var targetsText = s.Targets != null && s.Targets.Any()
                    ? "\n  Targets:\n  " + string.Join("\n  ", s.Targets.Select(t =>
                    {
                        var targetText = $"• Target {t.TargetId}: {t.Description}";
                        var indicatorsText = t.Indicators != null && t.Indicators.Any()
                            ? "\n    Indicators:\n    " + string.Join("\n    ", t.Indicators.Select(i =>
                                $"○ Indicator {i.IndicatorId}: {i.Description}"))
                            : "";
                        return targetText + indicatorsText;
                    }))
                    : "";
                return baseText + targetsText;
            }))
            : "No SDGs";

        // Separate Primary and Secondary SDGs for clearer AI prompt usage
        var primarySdgsText = sdgsDetails != null && sdgsDetails.Any(s => s.IsPrimary == "Primary")
            ? string.Join("\n", sdgsDetails.Where(s => s.IsPrimary == "Primary").Select(s => $"- SDG {s.SDGNumber}: {s.SDGName}"))
            : "No primary SDGs selected";
        var primarySdgsCount = sdgsDetails?.Count(s => s.IsPrimary == "Primary") ?? 0;
        
        var secondarySdgsText = sdgsDetails != null && sdgsDetails.Any(s => s.IsPrimary == "Secondary")
            ? string.Join("\n", sdgsDetails.Where(s => s.IsPrimary == "Secondary").Select(s => $"- SDG {s.SDGNumber}: {s.SDGName}"))
            : "No secondary SDGs selected";
        var secondarySdgsCount = sdgsDetails?.Count(s => s.IsPrimary == "Secondary") ?? 0;

        // Simple country names list for Location section
        var countryNamesList = countriesDetails != null && countriesDetails.Any()
            ? string.Join(", ", countriesDetails.Select(c => c.CountryName))
            : "No countries specified";
        
        var countryRegionsList = countriesDetails != null && countriesDetails.Any()
            ? string.Join(", ", countriesDetails.Select(c => c.Region).Where(r => !string.IsNullOrEmpty(r)).Distinct())
            : "No regions specified";

        // Formatted budget display
        var budgetDisplay = stats.TotalFundingUSD > 0
            ? $"USD {stats.TotalFundingUSD:N2}"
            : (opportunity.InitiativeBudgetUSD.HasValue && opportunity.InitiativeBudgetUSD.Value > 0
                ? $"USD {opportunity.InitiativeBudgetUSD.Value:N2} (estimated initiative budget)"
                : "Budget not yet specified");

        // Formatted timeline display
        var timelineDisplay = new List<string>();
        if (opportunity.TargetSigningDate.HasValue)
            timelineDisplay.Add($"Target Signing Date: {opportunity.TargetSigningDate.Value:MMMM d, yyyy}");
        if (opportunity.ImplementationStartDate.HasValue)
            timelineDisplay.Add($"Implementation Start: {opportunity.ImplementationStartDate.Value:MMMM d, yyyy}");
        if (opportunity.TargetDeliveryDate.HasValue)
            timelineDisplay.Add($"Target Delivery Date: {opportunity.TargetDeliveryDate.Value:MMMM d, yyyy}");
        var formattedTimeline = timelineDisplay.Any() 
            ? string.Join(", ", timelineDisplay) 
            : "Timeline not yet specified";

        // Formatted beneficiaries display
        var beneficiariesDisplay = new List<string>();
        if (opportunity.EstimatedDirectBeneficiaries.HasValue && opportunity.EstimatedDirectBeneficiaries.Value > 0)
            beneficiariesDisplay.Add($"Direct Beneficiaries: {opportunity.EstimatedDirectBeneficiaries.Value:N0}");
        else if (opportunity.BeneficiariesToBeDetermined)
            beneficiariesDisplay.Add("Direct Beneficiaries: To be determined during development");
        else
            beneficiariesDisplay.Add("Direct Beneficiaries: Not specified");
            
        if (opportunity.EstimatedIndirectBeneficiaries.HasValue && opportunity.EstimatedIndirectBeneficiaries.Value > 0)
            beneficiariesDisplay.Add($"Indirect Beneficiaries: {opportunity.EstimatedIndirectBeneficiaries.Value:N0}");
        else if (opportunity.BeneficiariesToBeDetermined)
            beneficiariesDisplay.Add("Indirect Beneficiaries: To be determined during development");
        else
            beneficiariesDisplay.Add("Indirect Beneficiaries: Not specified");
            
        if (!string.IsNullOrEmpty(opportunity.ExpectedBeneficiaries))
            beneficiariesDisplay.Add($"Beneficiary Institutions: {opportunity.ExpectedBeneficiaries}");
        else
            beneficiariesDisplay.Add("Beneficiary Institutions: Not specified");
        
        var formattedBeneficiaries = string.Join("\n", beneficiariesDisplay);

        // Enhanced deliverables formatting with full hierarchy
        var deliverablesEnhanced = deliverablesDetails != null && deliverablesDetails.Any()
            ? string.Join("\n", deliverablesDetails.Select(d =>
            {
                var parts = new List<string> { d.OutputName };
                if (!string.IsNullOrEmpty(d.ServiceLine)) parts.Add($"Service Line: {d.ServiceLine}");
                if (!string.IsNullOrEmpty(d.Level1)) parts.Add($"Category: {d.Level1}");
                if (!string.IsNullOrEmpty(d.Level2)) parts.Add($"Sub-category: {d.Level2}");
                if (d.Quantity != "Not specified") parts.Add($"Quantity: {d.Quantity}");
                if (!string.IsNullOrEmpty(d.PlannedStartDate) && !string.IsNullOrEmpty(d.PlannedEndDate))
                    parts.Add($"Timeline: {d.PlannedStartDate} to {d.PlannedEndDate}");
                return $"- {string.Join(" | ", parts)}";
            }))
            : "No deliverables specified";

        // Format UNCF Outcomes
        var uncfOutcomesDetails = opportunity.UNCFOutcomes?
            .Select(u => new
            {
                OutcomeName = u.UNCFOutcome?.Name ?? "Unknown",
                ExternalId = u.UNCFOutcome?.UNCFOutcomeId ?? "",
                Country = u.UNCFOutcome?.Country ?? "",
                VersionNo = u.UNCFOutcome?.UNCooperationFrameworkVersionNo?.ToString() ?? "",
                Notes = u.Notes ?? "",
                Indicators = opportunity.UNCFIndicators?
                    .Where(i => i.OpportunityUNCFOutcomeId == u.Id)
                    .Select(i => new
                    {
                        IndicatorName = i.UNCFIndicator?.Name ?? "",
                        ExternalId = i.UNCFIndicator?.UNCFIndicatorId ?? "",
                        Notes = i.Notes ?? ""
                    })
                    .ToList()
            })
            .ToList();

        var uncfOutcomesText = uncfOutcomesDetails != null && uncfOutcomesDetails.Any()
            ? string.Join("\n", uncfOutcomesDetails.Select(u =>
            {
                var baseText = $"- {u.OutcomeName} (Country: {u.Country}, Version: {u.VersionNo})";
                var indicatorsText = u.Indicators != null && u.Indicators.Any()
                    ? "\n  Indicators:\n  " + string.Join("\n  ", u.Indicators.Select(i => $"• {i.IndicatorName}"))
                    : "";
                return baseText + indicatorsText;
            }))
            : "No UNCF Outcomes";

        // Format UNOPS Missions
        var unopsMissionsDetails = opportunity.UNOPSMissions?
            .Select(m => new
            {
                MissionCode = m.UNOPSMission?.Code ?? "",
                MissionName = m.UNOPSMission?.Name ?? "Unknown",
                Description = m.UNOPSMission?.Description ?? ""
            })
            .ToList();

        var unopsMissionsText = unopsMissionsDetails != null && unopsMissionsDetails.Any()
            ? string.Join("\n", unopsMissionsDetails.Select(m =>
                $"- {m.MissionCode}: {m.MissionName}" +
                (string.IsNullOrEmpty(m.Description) ? "" : $" - {m.Description}")))
            : "No UNOPS Mission alignments";

        // Return comprehensive dictionary with all opportunity details
        return new Dictionary<string, object>
        {
            // Basic Information
            ["id"] = opportunity.Id.ToString(),
            ["name"] = opportunity.Name ?? "",
            ["description"] = opportunity.Description ?? "",
            ["partnerReference"] = opportunity.PartnerReference ?? "",
            ["status"] = opportunity.Status.ToString(),
            ["stage"] = opportunity.Stage ?? "", // Use Stage property instead of WorkflowStage navigation
            
            // Organizational Information
            ["responsibleOrgUnitId"] = opportunity.ResponsibleOrgUnitId?.ToString() ?? "",
            ["responsibleOrgUnitName"] = opportunity.ResponsibleOrgUnit?.Name ?? "",
            ["responsibleOrgUnitCode"] = opportunity.ResponsibleOrgUnit?.Code ?? "",
            
            // Initiative Type
            ["proposedInitiativeTypeId"] = opportunity.ProposedInitiativeTypeId?.ToString() ?? "",
            ["proposedInitiativeTypeName"] = opportunity.ProposedInitiativeType?.Name ?? "",
            
            // Budget and Dates
            ["initiativeBudgetUSD"] = opportunity.InitiativeBudgetUSD?.ToString("N2") ?? "",
            ["targetSigningDate"] = opportunity.TargetSigningDate?.Date.ToString("yyyy-MM-dd") ?? "",
            ["implementationStartDate"] = opportunity.ImplementationStartDate?.Date.ToString("yyyy-MM-dd") ?? "",
            ["targetDeliveryDate"] = opportunity.TargetDeliveryDate?.Date.ToString("yyyy-MM-dd") ?? "",
            ["isTargetSigningDateFirm"] = opportunity.IsTargetSigningDateFirm ? "Yes" : "No",
            ["signingDateNotes"] = opportunity.SigningDateNotes ?? "",
            ["submissionDeadline"] = opportunity.SubmissionDeadline?.Date.ToString("yyyy-MM-dd") ?? "",
            
            // Strategic Information
            ["resultsFocus"] = opportunity.ResultsFocus ?? "",
            ["expectedImpact"] = opportunity.ExpectedImpact ?? "",
            ["expectedOutcomes"] = opportunity.ExpectedOutcomes ?? "",
            ["expectedBeneficiaries"] = opportunity.ExpectedBeneficiaries ?? "",
            ["estimatedDirectBeneficiaries"] = opportunity.EstimatedDirectBeneficiaries?.ToString() ?? "Not specified",
            ["estimatedIndirectBeneficiaries"] = opportunity.EstimatedIndirectBeneficiaries?.ToString() ?? "Not specified",
            ["beneficiariesToBeDetermined"] = opportunity.BeneficiariesToBeDetermined ? "Yes" : "No",
            ["challenges"] = opportunity.Challenges ?? "",
            
            // Marketing Content
            // LEAVE OUT GENERATED CONTENT LIKE OPPORTUNITY STATEMENT THAT RELIES ON STRUCTURED DATA ANYWAY AND THE BANNER AND THUMBNAIL ARE NOT NEEDED FOR AI GENERATION ANYWAY
            // ["opportunityStatementMarkdown"] = opportunity.OpportunityStatementMarkdown ?? "",
            // ["hasOpportunityBannerImage"] = !string.IsNullOrEmpty(opportunity.OpportunityBannerImage) ? "Yes" : "No",
            // ["hasOpportunityThumbnail"] = !string.IsNullOrEmpty(opportunity.OpportunityThumbnail) ? "Yes" : "No",
            
            // Funding and Risk Information
            ["isPooledFunding"] = opportunity.IsPooledFunding ? "Yes" : "No",
            ["highRisksAcknowledged"] = opportunity.HighRisksAcknowledged ? "Yes" : "No",
            ["deliveryModality"] = opportunity.DeliveryModality?.ToString() ?? "Not specified",
            
            // External Stakeholder Notes
            ["miscExternalStakeholders"] = opportunity.MiscExternalStakeholders ?? "",
            ["externalStakeholderNotes"] = opportunity.ExternalStakeholderNotes ?? "",
            
            // Arrays - Detailed Information
            ["fundingPartners"] = fundingPartnersText,
            ["fundingPartnersCount"] = (fundingPartnersDetails?.Count ?? 0).ToString(),
            ["clientPartners"] = clientPartnersText,
            ["clientPartnersCount"] = (clientPartnersDetails?.Count ?? 0).ToString(),
            ["stakeholders"] = stakeholdersText,
            ["stakeholdersCount"] = (stakeholdersDetails?.Count ?? 0).ToString(),
            ["externalStakeholders"] = externalStakeholdersText,
            ["externalStakeholdersCount"] = (externalStakeholdersDetails?.Count ?? 0).ToString(),
            ["deliverables"] = deliverablesText,
            ["deliverablesCount"] = (deliverablesDetails?.Count ?? 0).ToString(),
            ["countries"] = countriesText,
            ["countriesCount"] = (countriesDetails?.Count ?? 0).ToString(),
            ["sdGs"] = sdgsText,
            ["sdGsCount"] = (sdgsDetails?.Count ?? 0).ToString(),
            ["primarySdGs"] = primarySdgsText,
            ["primarySdGsCount"] = primarySdgsCount.ToString(),
            ["secondarySdGs"] = secondarySdgsText,
            ["secondarySdGsCount"] = secondarySdgsCount.ToString(),
            ["uncfOutcomes"] = uncfOutcomesText,
            ["uncfOutcomesCount"] = (uncfOutcomesDetails?.Count ?? 0).ToString(),
            ["unopsMissions"] = unopsMissionsText,
            ["unopsMissionsCount"] = (unopsMissionsDetails?.Count ?? 0).ToString(),
            
            // Statistics
            ["stats.totalFundingUSD"] = stats.TotalFundingUSD.ToString("N2"),
            ["stats.totalFeeAmountUSD"] = stats.TotalFeeAmountUSD.ToString("N2"),
            ["stats.totalFundingPartners"] = stats.FundingPartnerCount.ToString(),
            ["stats.totalClientPartners"] = stats.ClientPartnerCount.ToString(),
            ["stats.totalPartners"] = stats.TotalPartnerCount.ToString(),
            ["stats.totalStakeholders"] = stats.StakeholderCount.ToString(),
            ["stats.totalInternalStakeholders"] = stats.InternalStakeholderCount.ToString(),
            ["stats.totalExternalStakeholders"] = stats.ExternalStakeholderCount.ToString(),
            ["stats.totalDeliverables"] = stats.DeliverableCount.ToString(),
            ["stats.totalCountries"] = stats.CountryCount.ToString(),
            ["stats.totalSDGs"] = stats.SDGCount.ToString(),
            ["stats.primarySDGId"] = stats.PrimarySDGId?.ToString() ?? "",
            ["stats.daysToTargetSigningDate"] = stats.DaysToTargetSigningDate?.ToString() ?? "",
            ["stats.serviceLines"] = string.Join(", ", stats.ServiceLines ?? new List<string>()),
            
            // Audit Information
            ["createdDate"] = opportunity.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
            ["lastModifiedDate"] = opportunity.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            ["createdBy"] = opportunity.CreatedBy.ToString(),
            ["createdByName"] = opportunity.CreatedByUser?.Name ?? "",
            ["lastModifiedBy"] = opportunity.LastModifiedBy.ToString(),
            ["lastModifiedByName"] = opportunity.LastModifiedByUser?.Name ?? "",
            
            // ==========================================
            // RISK REGISTER DATA
            // ==========================================
            ["risks"] = risksText,
            ["risksCount"] = risksDetails.Count.ToString(),
            ["totalThreats"] = risksDetails.Count(r => r.RiskType.Equals("Threat", StringComparison.OrdinalIgnoreCase)).ToString(),
            ["totalOpportunityRisks"] = risksDetails.Count(r => r.RiskType.Equals("Opportunity", StringComparison.OrdinalIgnoreCase)).ToString(),
            ["highImpactRisks"] = risksDetails.Count(r => r.ImpactValue >= 4).ToString(),
            ["highProbabilityRisks"] = risksDetails.Count(r => r.ProbabilityValue >= 4).ToString(),
            ["preDefinedHighRisksCount"] = risksDetails.Count(r => r.IsPreDefinedHighRisk).ToString(),
            
            // ==========================================
            // SME SELECTIONS
            // ==========================================
            ["smeSelections"] = smeSelectionsText,
            ["smeSelectionsCount"] = smeSelections.Count(s => s.IsSelected).ToString(),
            
            // ==========================================
            // PARTNER AGREEMENTS SUMMARY
            // ==========================================
            ["partnerAgreements"] = partnerAgreementsText,
            ["partnerAgreementsCount"] = partnerAgreementsSummary.Count.ToString(),
            
            // ==========================================
            // ADDITIONAL COMPUTED FIELDS
            // ==========================================
            ["hasOpportunityStatement"] = !string.IsNullOrEmpty(opportunity.OpportunityStatementMarkdown) ? "Yes" : "No",
            ["opportunityStatementLength"] = opportunity.OpportunityStatementMarkdown?.Length.ToString() ?? "0",
            ["hasHighRiskAcknowledgement"] = opportunity.HighRisksAcknowledged ? "Yes" : "No",
            ["isMultiCountry"] = stats.CountryCount > 1 ? "Yes" : "No",
            ["isMultiFunder"] = stats.FundingPartnerCount > 1 ? "Yes" : "No",
            ["hasSDGTargets"] = opportunity.SDGTargets?.Any() == true ? "Yes" : "No",
            ["hasSDGIndicators"] = opportunity.SDGIndicators?.Any() == true ? "Yes" : "No",
            ["hasUNCFAlignment"] = opportunity.UNCFOutcomes?.Any() == true ? "Yes" : "No",
            ["hasUNOPSMissionAlignment"] = opportunity.UNOPSMissions?.Any() == true ? "Yes" : "No",
            ["hasExternalStakeholders"] = opportunity.ExternalStakeholders?.Any() == true ? "Yes" : "No",
            ["hasMiscExternalStakeholders"] = !string.IsNullOrEmpty(opportunity.MiscExternalStakeholders) ? "Yes" : "No",
            ["fundingToFeeRatio"] = stats.TotalFundingUSD > 0 && stats.TotalFeeAmountUSD > 0 
                ? (stats.TotalFeeAmountUSD / stats.TotalFundingUSD * 100).ToString("N2") + "%" 
                : "N/A",
            
            // ==========================================
            // TIMELINE ANALYSIS
            // ==========================================
            ["hasDefinedTimeline"] = opportunity.TargetSigningDate.HasValue && opportunity.TargetDeliveryDate.HasValue ? "Yes" : "No",
            ["estimatedDurationMonths"] = opportunity.TargetSigningDate.HasValue && opportunity.TargetDeliveryDate.HasValue
                ? Math.Round((opportunity.TargetDeliveryDate.Value - opportunity.TargetSigningDate.Value).TotalDays / 30.0).ToString()
                : "Not specified",
            ["hasSubmissionDeadline"] = opportunity.SubmissionDeadline.HasValue ? "Yes" : "No",
            ["daysUntilSubmissionDeadline"] = opportunity.SubmissionDeadline.HasValue
                ? ((int)(opportunity.SubmissionDeadline.Value.Date - DateTime.UtcNow.Date).TotalDays).ToString()
                : "N/A",
            
            // ==========================================
            // NORMALLY RESPONSIBLE ORG UNITS ANALYSIS
            // Helps AI understand org unit configuration for implementation countries
            // ==========================================
            ["normallyResponsibleOrgUnits"] = normallyResponsibleOrgUnitsText,
            ["normallyResponsibleOrgUnitsCount"] = normallyResponsibleOrgUnitIds.Count.ToString(),
            ["hasOrgUnitMismatch"] = hasOrgUnitMismatch ? "Yes" : "No",
            ["countriesWithDifferentOrgUnit"] = countriesWithDifferentOrgUnit.Any() 
                ? string.Join(", ", countriesWithDifferentOrgUnit) 
                : "None - selected org unit is normally responsible for all countries",
            ["countriesWithMatchingOrgUnit"] = countriesWithMatchingOrgUnit.Any()
                ? string.Join(", ", countriesWithMatchingOrgUnit)
                : "None",
            
            // ==========================================
            // ENHANCED FORMATTED FIELDS FOR AI PROMPTS
            // ==========================================
            ["countryNamesList"] = countryNamesList,
            ["countryRegionsList"] = countryRegionsList,
            ["budgetDisplay"] = budgetDisplay,
            ["formattedTimeline"] = formattedTimeline,
            ["formattedBeneficiaries"] = formattedBeneficiaries,
            ["deliverablesEnhanced"] = deliverablesEnhanced
        };
    }

    /// <summary>
    /// Gets opportunity details for statement validation. Same as GetOpportunityDetailsForAIAsync but includes
    /// opportunityStatementMarkdown so the validation AI receives the current statement alongside structured data.
    /// </summary>
    /// <param name="id">Opportunity ID</param>
    /// <returns>Dictionary of opportunity details including opportunityStatementMarkdown</returns>
    public async Task<Dictionary<string, object>> GetOpportunityDetailsForStatementValidationAsync(int id)
    {
        var result = await GetOpportunityDetailsForAIAsync(id);
        var statement = await context.Set<Opportunity>()
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => o.OpportunityStatementMarkdown)
            .FirstOrDefaultAsync();
        result["opportunityStatementMarkdown"] = statement ?? "";
        return result;
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
            .Include(o => o.ResponsibleOrgUnit)
            .Include(o => o.ProposedInitiativeType)
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
                .ThenInclude(fp => fp.Partner)
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
                .ThenInclude(fp => fp.Currency)
            .Include(o => o.ClientPartners.Where(cp => !cp.IsDeleted))
                .ThenInclude(cp => cp.Partner)
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.EntityRole)
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.User)
                    .ThenInclude(u => u!.UserProfile)
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
                .ThenInclude(d => d.Output)
            .Include(o => o.Countries.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Country)
            .Include(o => o.SDGs.Where(s => !s.IsDeleted))
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
                                WorkflowStage = opp.Stage // Use Stage property
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
                .FirstOrDefaultAsync(er => er.EntityType == "Opportunity" && er.Code == "Opportunity_Manager_Opportunity");

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
                .Include(o => o.ResponsibleOrgUnit)
                .Include(o => o.ProposedInitiativeType)
                .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted)).ThenInclude(fp => fp.Partner)
                .Include(o => o.ClientPartners.Where(cp => !cp.IsDeleted)).ThenInclude(cp => cp.Partner)
                .Include(o => o.Stakeholders.Where(s => !s.IsDeleted)).ThenInclude(s => s.EntityRole)
                .Include(o => o.Stakeholders.Where(s => !s.IsDeleted)).ThenInclude(s => s.OrganizationHierarchy)
                .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
                .Include(o => o.Countries.Where(c => !c.IsDeleted)).ThenInclude(c => c.Country)
                .Include(o => o.SDGs.Where(s => !s.IsDeleted)).ThenInclude(s => s.SDG)
                .Where(o => 
                    o.FundingPartners.Any(fp => !fp.IsDeleted && fp.PartnerId == partnerId) ||
                    o.ClientPartners.Any(cp => !cp.IsDeleted && cp.PartnerId == partnerId))
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
                // "WorkflowStage" removed - now using Stage property instead
                "ResponsibleOrgUnit",
                "ProposedInitiativeType",
                "FundingPartners",
                "FundingPartners.Partner",
                "ClientPartners",
                "ClientPartners.Partner",
                "Stakeholders",
                "Stakeholders.EntityRole",
                "Stakeholders.User",
                "Stakeholders.OrganizationHierarchy",
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
                new() { Field = "expectedImpact", DisplayName = "label.opportunity.expectedImpact", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "expectedOutcomes", DisplayName = "label.opportunity.expectedOutcomes", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "expectedBeneficiaries", DisplayName = "label.opportunity.expectedBeneficiaries", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },

                // Budget field
                new() { Field = "initiativeBudgetUSD", DisplayName = "label.opportunity.budgetUSD", FieldType = "number", AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq", "entityCards.operators.gt", "entityCards.operators.lt", "entityCards.operators.gte", "entityCards.operators.lte", "entityCards.operators.between" } },

                // Date fields
                new() { Field = "targetSigningDate", DisplayName = "label.opportunity.targetSigningDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "targetDeliveryDate", DisplayName = "label.opportunity.targetDeliveryDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "createdDate", DisplayName = "label.common.createdDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "lastModifiedDate", DisplayName = "label.common.lastModifiedDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },

                // Related entity fields - using dropdowns for enum-like lookups
                new() { 
                    Field = "stage", 
                    DisplayName = "label.opportunity.stage", 
                    FieldType = "enum", 
                    AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" },
                    DropdownOptions = new List<DropdownOption>() // Will be populated dynamically from API
                },
                new() { 
                    Field = "responsibleOrgUnitId", 
                    DisplayName = "label.opportunity.responsibleOrgUnit", 
                    FieldType = "enum", 
                    AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" },
                    DropdownOptions = new List<DropdownOption>() // Will be populated dynamically from API
                },
                new() { 
                    Field = "proposedInitiativeTypeId", 
                    DisplayName = "label.opportunity.proposedInitiativeType", 
                    FieldType = "enum", 
                    AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" },
                    DropdownOptions = new List<DropdownOption>() // Will be populated dynamically from API
                },
                
                // Partner relationship fields - using partner dropdown
                new() { Field = "fundingPartners.partnerId", DisplayName = "label.opportunity.fundingPartner", FieldType = "partner", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "clientPartners.partnerId", DisplayName = "label.opportunity.clientPartner", FieldType = "partner", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" } },
                
                // Audit fields - User dropdowns
                new() {
                    Field = "createdBy",
                    DisplayName = "label.common.createdBy",
                    FieldType = "user",
                    AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" }
                },
                new() {
                    Field = "lastModifiedBy",
                    DisplayName = "label.common.lastModifiedBy",
                    FieldType = "user",
                    AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" }
                },
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
                .AsNoTracking() // Performance: No entity tracking needed for read-only operations
                .Where(p => p.Id == partnerId)
                .Select(p => new { p.ErpDimValue })
                .FirstOrDefaultAsync();
                
            if (partner != null && partner.ErpDimValue.HasValue)
            {
                // Convert ErpDimValue to string for matching with PartnerAgreementPartner
                var partnerNumber = partner.ErpDimValue.Value.ToString();
                
                // Load all active agreements for this partner from BigQuery
                var partnerAgreements = await context.PartnerAgreements
                    .AsNoTracking() // Performance: No entity tracking needed for read-only operations
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

    /// <summary>
    /// Updates the high risk acknowledgement status for an opportunity
    /// AC1: User must acknowledge they've reviewed all applicable organizational high risks
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <param name="acknowledged">Whether the high risks have been acknowledged</param>
    /// <returns>True if updated successfully</returns>
    public async Task<bool> UpdateHighRiskAcknowledgementAsync(int opportunityId, bool acknowledged)
    {
        var opportunity = await context.Opportunities.FindAsync(opportunityId);
        if (opportunity == null)
        {
            return false;
        }

        // Check if opportunity can be modified (immutability and approval workflow status)
        ThrowIfCannotModify(opportunity);

        opportunity.HighRisksAcknowledged = acknowledged;
        // LastModifiedDate and LastModifiedBy are handled automatically by AuditableDbContext

        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets the entity artifact document by artifact type code
    /// Generic method that can be used for any entity type and artifact type code
    /// Returns the GCS path (ValueText) and metadata (ValueJson) if found
    /// </summary>
    /// <param name="entityType">Entity type (e.g., "OrganizationHierarchy", "Country", "Partner")</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="artifactTypeCode">Artifact type code (e.g., "High_Risk_Guidance", "Strategy", "NDC")</param>
    /// <returns>Tuple with GCS path (ValueText) and metadata (ValueJson), or null if not found</returns>
    public async Task<(string? GcsPath, string? MimeType, string? FileName)?> GetEntityArtifactDocumentAsync(
        string entityType, 
        int entityId, 
        string artifactTypeCode)
    {
        try
        {
            // Get the artifact type by code
            var artifactType = await context.Set<ArtifactType>()
                .Where(at => at.ArtifactTypeCode == artifactTypeCode && !at.IsDeleted)
                .Select(at => at.Id)
                .FirstOrDefaultAsync();

            if (artifactType == 0)
            {
                Console.WriteLine($"[WARNING] Artifact type with code '{artifactTypeCode}' not found");
                return null;
            }

            // Get the entity artifact
            var entityArtifact = await context.EntityArtifacts
                .Where(ea => 
                    ea.EntityType == entityType 
                    && ea.EntityId == entityId 
                    && ea.ArtifactTypeId == artifactType
                    && !ea.IsDeleted
                    && ea.Status == Domain.Entities.EntityStatus.Active
                    && !string.IsNullOrEmpty(ea.ValueText)
                    && ea.ValueText.StartsWith("gs://"))
                .OrderByDescending(ea => ea.CreatedDate) // Get most recent
                .Select(ea => new { ea.ValueText, ea.ValueJson })
                .FirstOrDefaultAsync();

            if (entityArtifact == null)
            {
                Console.WriteLine($"[INFO] No artifact found for EntityType='{entityType}', EntityId={entityId}, ArtifactTypeCode='{artifactTypeCode}'");
                return null;
            }

            // Extract MIME type and file name from ValueJson
            string? mimeType = null;
            string? fileName = null;

            if (!string.IsNullOrEmpty(entityArtifact.ValueJson))
            {
                try
                {
                    var metadata = System.Text.Json.JsonDocument.Parse(entityArtifact.ValueJson);
                    var root = metadata.RootElement;

                    if (root.TryGetProperty("mimeType", out var mimeTypeElement))
                    {
                        mimeType = mimeTypeElement.GetString();
                    }

                    if (root.TryGetProperty("fileName", out var fileNameElement))
                    {
                        fileName = fileNameElement.GetString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to parse ValueJson for artifact: {ex.Message}");
                    mimeType = "application/pdf"; // Default fallback
                }
            }

            // Default to PDF if no MIME type found
            mimeType ??= "application/pdf";

            Console.WriteLine($"[SUCCESS] Found artifact document: {fileName ?? entityArtifact.ValueText}");
            return (entityArtifact.ValueText, mimeType, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error getting entity artifact document: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the High Risk Guidance document from EntityArtifact table
    /// This is a global document with ArtifactTypeCode "High_Risk_Guidance"
    /// </summary>
    /// <returns>Tuple with GCS path, MIME type, and file name, or null if not found</returns>
    public async Task<(string? GcsPath, string? MimeType, string? FileName)?> GetHighRiskGuidanceDocumentAsync()
    {
        try
        {
            // Get the ArtifactType ID for "High_Risk_Guidance" (case-insensitive)
            var artifactType = await uNOPSAppDbContext.ArtifactTypes
                .Where(at => at.ArtifactTypeCode.ToLower() == "high_risk_guidance" && !at.IsDeleted)
                .FirstOrDefaultAsync();

            if (artifactType == null)
            {
                Console.WriteLine($"[WARNING] ArtifactType 'High_Risk_Guidance' not found");
                return null;
            }

            // Get the EntityArtifact with this type that has a GCS path (gs:// or https://storage.cloud.google.com/)
            var artifact = await uNOPSAppDbContext.EntityArtifacts
                .Where(ea => ea.ArtifactTypeId == artifactType.Id
                          && !ea.IsDeleted
                          && ea.Status == Domain.Entities.EntityStatus.Active
                          && !string.IsNullOrEmpty(ea.ValueText)
                          && (ea.ValueText.StartsWith("gs://") || ea.ValueText.StartsWith("https://storage.cloud.google.com/")))
                .OrderByDescending(ea => ea.CreatedDate) // Get most recent
                .FirstOrDefaultAsync();

            if (artifact == null)
            {
                Console.WriteLine($"[INFO] No High Risk Guidance document found in EntityArtifacts");
                return null;
            }

            // Convert HTTPS URL to gs:// format if needed (Gemini expects gs:// URI)
            var gcsPath = artifact.ValueText;
            if (gcsPath.StartsWith("https://storage.cloud.google.com/"))
            {
                // Convert: https://storage.cloud.google.com/bucket/path → gs://bucket/path
                gcsPath = "gs://" + gcsPath.Replace("https://storage.cloud.google.com/", "");
                Console.WriteLine($"[INFO] Converted HTTPS URL to gs:// format: {gcsPath}");
            }

            // Extract mime type and file name from ValueJson if available
            string? mimeType = "application/pdf";
            string? fileName = null;

            if (!string.IsNullOrEmpty(artifact.ValueJson))
            {
                try
                {
                    var metadata = Newtonsoft.Json.Linq.JObject.Parse(artifact.ValueJson);
                    mimeType = metadata["mimeType"]?.ToString() ?? "application/pdf";
                    fileName = metadata["fileName"]?.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to parse ValueJson: {ex.Message}");
                }
            }

            Console.WriteLine($"[SUCCESS] Found High Risk Guidance document: {gcsPath}");
            return (gcsPath, mimeType, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error getting High Risk Guidance document: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Assigns an Executive to an opportunity during Go decision approval.
    /// The Executive is typically the Director/Manager/OiC of the responsible org unit.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <param name="executiveId">The user ID of the assigned Executive</param>
    /// <exception cref="KeyNotFoundException">Thrown when opportunity is not found</exception>
    public async Task AssignExecutiveAsync(int opportunityId, int executiveId)
    {
        var opportunity = await opportunityRepository.GetByIdAsync(opportunityId);
        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with ID {opportunityId} not found");
        }

        opportunity.ExecutiveId = executiveId;
        await uNOPSAppDbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets personnel for an opportunity's responsible org unit.
    /// Used to populate the Executive dropdown in the Go Decision approval dialog.
    /// Returns all personnel with roles on the org unit, with Directors/Deputy Directors marked as "Suggested".
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <returns>List of personnel with display label and user ID</returns>
    /// <exception cref="KeyNotFoundException">Thrown when opportunity is not found</exception>
    public async Task<IEnumerable<TypeaheadInput>> GetExecutivesForOpportunityAsync(int opportunityId)
    {
        // Get the opportunity to find the ResponsibleOrgUnitId
        var opportunity = await uNOPSAppDbContext.Opportunities
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
    private async Task<IEnumerable<TypeaheadInput>> GetExecutivesForOrgUnitAsync(int orgUnitId)
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
        var executiveRoles = await uNOPSAppDbContext.EntityUserRoles
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
        var allUsers = await uNOPSAppDbContext.PAOUsers
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


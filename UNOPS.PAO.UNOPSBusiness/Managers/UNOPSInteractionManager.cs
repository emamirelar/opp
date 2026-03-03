namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System.Threading.Tasks;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Utilities.Helpers;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Business.Repositories.Generic;
using System.Security.Claims;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Extensions;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Models.Integrations;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.Models.Shared;

public class UNOPSInteractionManager : BaseUNOPSManager, IInteractionManager
{
    private readonly IMapper mapper;
    private readonly BaseRepository<UNOPSInteraction> interactionRepository;
    private readonly BaseRepository<UNOPSContact> contactRepository;
    private readonly BaseRepository<OrganizationHierarchy> OrganizationHierarchyRepository;
    private readonly UNOPSAppDbContext context;
    private GoogleCloudStorageService googleCloudStorageService;
    private readonly IUserProfileCacheService userProfileCacheService;
    private readonly PartnerTreeService partnerTreeService;
    private readonly GlobalFilterService _globalFilterService;
    private readonly IDbContextFactory<UNOPSAppDbContext>? _dbContextFactory; // ⚡ PERFORMANCE: Enable parallel query execution

    private InteractionModel MapEntityToModel(UNOPSInteraction entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSInteraction, InteractionModel>(entity);

        // For sync version, provide default user identifiers to avoid N+1 queries
        // User names will be resolved by batch methods when possible
        //Updated the condition to include Opportunity+ User that has Id of -1
        if (entity.CreatedBy != 0)
        {
            result.CreatedByName = $"User #{entity.CreatedBy}";
        }
        //Updated the condition to include Opportunity+ User that has Id of -1
        if (entity.LastModifiedBy != 0)
        {
            result.LastModifiedByName = $"User #{entity.LastModifiedBy}";
        }
        
        return result;
    }

    private async Task<InteractionModel> MapEntityToModelAsync(UNOPSInteraction entity, IMapper mapper, ClaimsPrincipal user = null)
    {
        // Use AutoMapper with the updated configuration
        var result = mapper.Map<UNOPSInteraction, InteractionModel>(entity);

        // Convert ProfilePictureUrl to signed URL for nested Contact objects
        if (result.Contacts != null && googleCloudStorageService != null)
        {
            foreach (var contact in result.Contacts)
            {
                if (!string.IsNullOrEmpty(contact.ProfilePictureUrl))
                {
                    contact.ProfilePictureUrl = await googleCloudStorageService.GenerateSignedUrlFromStorageUrl(contact.ProfilePictureUrl);
                }
            }
        }

        // Convert LogoUrl to signed URL for nested Partner objects
        if (result.Partners != null && googleCloudStorageService != null)
        {
            foreach (var partner in result.Partners)
            {
                if (!string.IsNullOrEmpty(partner.LogoUrl))
                {
                    partner.LogoUrl = await googleCloudStorageService.GenerateSignedUrlFromStorageUrl(partner.LogoUrl);
                }
            }
        }

        // Resolve user names for audit fields
        //Updated the condition to include Opportunity+ User that has Id of -1
        if (entity.CreatedBy != 0)
        {
            result.CreatedByName = await GetUserNameByIdAsync(entity.CreatedBy);
        }
        //Updated the condition to include Opportunity+ User that has Id of -1
        if (entity.LastModifiedBy != 0)
        {
            result.LastModifiedByName = await GetUserNameByIdAsync(entity.LastModifiedBy);
        }

        return await MapEntityToModelWithPermissionsAsync(result, user);
    }

    /// <summary>
    /// Maps entity to model with pre-loaded user names to avoid N+1 query problem
    /// </summary>
    private async Task<InteractionModel> MapEntityToModelAsync(UNOPSInteraction entity, IMapper mapper, Dictionary<int, string> userNames, ClaimsPrincipal user = null)
    {
        // Use AutoMapper with the updated configuration
        var result = mapper.Map<UNOPSInteraction, InteractionModel>(entity);

        // Convert ProfilePictureUrl to signed URL for nested Contact objects
        if (result.Contacts != null && googleCloudStorageService != null)
        {
            foreach (var contact in result.Contacts)
            {
                if (!string.IsNullOrEmpty(contact.ProfilePictureUrl))
                {
                    contact.ProfilePictureUrl = await googleCloudStorageService.GenerateSignedUrlFromStorageUrl(contact.ProfilePictureUrl);
                }
            }
        }

        // Convert LogoUrl to signed URL for nested Partner objects
        if (result.Partners != null && googleCloudStorageService != null)
        {
            foreach (var partner in result.Partners)
            {
                if (!string.IsNullOrEmpty(partner.LogoUrl))
                {
                    partner.LogoUrl = await googleCloudStorageService.GenerateSignedUrlFromStorageUrl(partner.LogoUrl);
                }
            }
        }

        // Resolve user names from pre-loaded dictionary
        //Updated the condition to include Opportunity+ User that has Id of -1
        if (entity.CreatedBy != 0 && userNames.TryGetValue(entity.CreatedBy, out var createdByName))
        {
            result.CreatedByName = createdByName;
        }
        //Updated the condition to include Opportunity+ User that has Id of -1
        else if (entity.CreatedBy != 0)
        {
            result.CreatedByName = $"User #{entity.CreatedBy}";
        }
        //Updated the condition to include Opportunity+ User that has Id of -1
        if (entity.LastModifiedBy != 0 && userNames.TryGetValue(entity.LastModifiedBy, out var lastModifiedByName))
        {
            result.LastModifiedByName = lastModifiedByName;
        }
        //Updated the condition to include Opportunity+ User that has Id of -1
        else if (entity.LastModifiedBy != 0)
        {
            result.LastModifiedByName = $"User #{entity.LastModifiedBy}";
        }

        return await MapEntityToModelWithPermissionsAsync(result, user);
    }

    private UNOPSInteraction MapModelToEntity(InteractionRequest model, UNOPSInteraction entity)
    {
        mapper.Map(model, entity);
        return entity;
    }

    private async Task<UNOPSInteraction> MapModelToEntity(InteractionRequest model)
    {
        // Contact relationships are now handled through InteractionContacts many-to-many table
        var contactId = model.ContactIds?.FirstOrDefault() ?? 0;
        
        return MapModelToEntity(model, new UNOPSInteraction() { 
            Name = contactId + " - " + model.Date,
            Subject = model.Subject ?? "No Subject"
        });
    }

    private UNOPSInteraction MapModelToEntity(UpdateInteractionRequest model, UNOPSInteraction entity)
    {
        mapper.Map(model, entity);
        return entity;
    }

    private async Task UpdateOrganizationUnitRelationshipsDifferentialAsync(int interactionId, IEnumerable<int>? newOrgUnitIds)
    {
        if (newOrgUnitIds == null) return;

        // Get current relationships
        var currentRelationships = await context.OrganizationUnitRelationships
            .Where(r => r.EntityId == interactionId && r.EntityType == "Interaction")
            .ToListAsync();

        var currentOrgUnitIds = currentRelationships.Select(r => r.OrganizationHierarchyId).ToHashSet();
        var newOrgUnitIdsSet = newOrgUnitIds.ToHashSet();

        // Find relationships to remove (in current but not in new)
        var relationshipsToRemove = currentRelationships
            .Where(r => !newOrgUnitIdsSet.Contains(r.OrganizationHierarchyId))
            .ToList();

        // Find relationships to add (in new but not in current)
        var orgUnitIdsToAdd = newOrgUnitIdsSet
            .Where(id => !currentOrgUnitIds.Contains(id))
            .ToList();

        // Remove old relationships
        if (relationshipsToRemove.Any())
        {
            await context.OrganizationUnitRelationships
                .Where(r => r.EntityId == interactionId && r.EntityType == "Interaction" && 
                           relationshipsToRemove.Select(rel => rel.OrganizationHierarchyId).Contains(r.OrganizationHierarchyId))
                .ExecuteDeleteAsync();
        }

        // Add new relationships
        if (orgUnitIdsToAdd.Any())
        {
            var relationshipsToAdd = new List<OrganizationUnitRelationship>();
            foreach (var orgUnitId in orgUnitIdsToAdd)
            {
                var orgUnit = await OrganizationHierarchyRepository.GetByIdAsync(orgUnitId);
                if (orgUnit != null && orgUnit.Type == Domain.Enums.OrganizationUnitType.OrgUnit)
                {
                    var newRelationship = new OrganizationUnitRelationship
                    {
                        OrganizationHierarchyId = orgUnitId,
                        EntityId = interactionId,
                        EntityType = nameof(Interaction),
                        Name = $"Interaction-{interactionId}-{orgUnit.Code}",
                        Status = EntityStatus.Active
                    };
                    relationshipsToAdd.Add(newRelationship);
                }
            }

            foreach (var relationship in relationshipsToAdd)
            {
                context.OrganizationUnitRelationships.Add(relationship);
            }
        }

        await context.SaveChangesAsync();
    }

    public UNOPSInteractionManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, PartnerTreeService partnerTreeService, IPermissionService permissionService, GlobalFilterService globalFilterService, IHttpContextAccessor httpContextAccessor = null, IServiceProvider serviceProvider = null, IUserProfileCacheService userProfileCacheService = null, IDbContextFactory<UNOPSAppDbContext>? dbContextFactory = null)
        : base(mapper, context, configuration, null, "Interaction", permissionService, httpContextAccessor)
    {
        this.mapper = mapper;
        this.context = context;
        this.partnerTreeService = partnerTreeService;
        _globalFilterService = globalFilterService;
        _dbContextFactory = dbContextFactory; // ⚡ PERFORMANCE: Store factory for parallel execution
        interactionRepository = new BaseRepository<UNOPSInteraction>(context, configuration, serviceProvider);
        contactRepository = new BaseRepository<UNOPSContact>(context, configuration, serviceProvider);
        OrganizationHierarchyRepository = new BaseRepository<OrganizationHierarchy>(context, configuration, serviceProvider);
        googleCloudStorageService = new GoogleCloudStorageService(configuration);
        this.userProfileCacheService = userProfileCacheService;
    }

    private async Task<string> GetUserNameByIdAsync(int userId)
    {
        try
        {
            var userProfile = await context.UserProfile.FirstOrDefaultAsync(up => up.UserId == userId);
            if (userProfile != null && !string.IsNullOrEmpty(userProfile.Name))
            {
                return userProfile.Name;
            }
            
            // Fallback to PAOUser email if UserProfile not found or Name is empty
            var user = await context.PAOUsers.FirstOrDefaultAsync(u => u.Id == userId);
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

    /// <summary>
    /// Bulk loads user names for multiple user IDs to avoid N+1 query problem
    /// Uses cache for performance optimization when available
    /// </summary>
    private async Task<Dictionary<int, string>> GetUserNamesBatchAsync(IEnumerable<int> userIds)
    {
        var result = new Dictionary<int, string>();
        if (!userIds.Any()) return result;

        var distinctUserIds = userIds.Where(id => id > 0).Distinct().ToList();
        if (!distinctUserIds.Any()) return result;

        var uncachedUserIds = distinctUserIds;

        try
        {
            // Try to get cached user names first if cache service is available
            if (userProfileCacheService != null)
            {
                var cachedUserNames = await userProfileCacheService.GetCachedUserNamesBatchAsync(distinctUserIds);
                foreach (var kvp in cachedUserNames)
                {
                    result[kvp.Key] = kvp.Value;
                }
                uncachedUserIds = distinctUserIds.Where(id => !result.ContainsKey(id)).ToList();
            }

            // Load uncached user names from database
            if (uncachedUserIds.Any())
            {
                var newUserNames = new Dictionary<int, string>();

                // Bulk load user profiles
                var userProfiles = await context.UserProfile
                    .Where(up => uncachedUserIds.Contains(up.UserId))
                    .ToDictionaryAsync(up => up.UserId, up => up.Name);

                // Bulk load PAO users for fallback
                var missingUserIds = uncachedUserIds.Where(id => !userProfiles.ContainsKey(id) || string.IsNullOrEmpty(userProfiles[id])).ToList();
                var paoUsers = new Dictionary<int, string>();
                
                if (missingUserIds.Any())
                {
                    paoUsers = await context.PAOUsers
                        .Where(u => missingUserIds.Contains(u.Id))
                        .ToDictionaryAsync(u => u.Id, u => u.Email ?? string.Empty);
                }

                // Combine results with fallback logic
                foreach (var userId in uncachedUserIds)
                {
                    if (userProfiles.ContainsKey(userId) && !string.IsNullOrEmpty(userProfiles[userId]))
                    {
                        newUserNames[userId] = userProfiles[userId];
                    }
                    else if (paoUsers.ContainsKey(userId) && !string.IsNullOrEmpty(paoUsers[userId]))
                    {
                        newUserNames[userId] = paoUsers[userId];
                    }
                    else
                    {
                        newUserNames[userId] = $"User #{userId}";
                    }
                    result[userId] = newUserNames[userId];
                }

                // Cache the newly loaded user names
                if (userProfileCacheService != null && newUserNames.Any())
                {
                    await userProfileCacheService.SetCachedUserNamesBatchAsync(newUserNames);
                }
            }
        }
        catch (Exception)
        {
            // Log error if needed, but don't fail the entire operation
            // Fallback to default names for all users
            foreach (var userId in distinctUserIds)
            {
                result[userId] = $"User #{userId}";
            }
        }

        return result;
    }

    public async Task<InteractionModel> CreateInteractionAsync(InteractionRequest model)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var entity = await MapModelToEntity(model);

        try
        {
            // Use first contact ID for naming, or default to interaction date
            var contactId = model.ContactIds?.FirstOrDefault() ?? 0;
            entity.Name = contactId + " - " + model.Date;

            await interactionRepository.AddAsync(entity);
            await context.SaveChangesAsync();

            // Handle OrganizationHierarchyIds
            if (model.OrganizationHierarchyIds != null && model.OrganizationHierarchyIds.Any())
            {
                var relationshipsToAdd = new List<OrganizationUnitRelationship>();
                foreach (var orgUnitId in model.OrganizationHierarchyIds)
                {
                    var orgUnit = await OrganizationHierarchyRepository.GetByIdAsync(orgUnitId);
                    if (orgUnit == null || orgUnit.Type != Domain.Enums.OrganizationUnitType.OrgUnit)
                    {
                        throw new BusinessException($"Organization unit with ID {orgUnitId} must be of type OrgUnit");
                    }
                    
                    var newRelationship = new OrganizationUnitRelationship
                    {
                        OrganizationHierarchyId = orgUnitId,
                        EntityId = entity.Id,
                        EntityType = nameof(Interaction),
                        Name = $"Interaction-{entity.Id}-{orgUnit.Code}",
                        Status = EntityStatus.Active
                    };
                    relationshipsToAdd.Add(newRelationship);
                }

                foreach (var relationship in relationshipsToAdd)
                {
                    context.OrganizationUnitRelationships.Add(relationship);
                }
                await context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await ProcessJunctionTables(entity, model);
        return mapper.Map<InteractionModel>(entity);
    }

    private async Task ProcessJunctionTables(Interaction interaction, InteractionRequest model)
    {
        await using var jtTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Process InteractionContacts
            if (model.ContactIds?.Any() == true)
            {
                var existingContacts = await context.InteractionContacts
                   .Where(ic => ic.InteractionId == interaction.Id)
                   .ToListAsync();

                // Remove contacts not in the new list
                foreach (var contact in existingContacts.Where(ec => !model.ContactIds.Contains(ec.ContactId)))
                {
                    context.InteractionContacts.Remove(contact);
                }

                // Add new contacts
                foreach (var contactId in model.ContactIds.Except(existingContacts.Select(ec => ec.ContactId)))
                {
                    await context.InteractionContacts.AddAsync(new InteractionContact
                    {
                        InteractionId = interaction.Id,
                        ContactId = contactId
                    });
                }
            }

            // Process InteractionPartners
            if (model.PartnerIds?.Any() == true)
            {
                var existingPartners = await context.InteractionPartners
                    .Where(ip => ip.InteractionId == interaction.Id)
                    .ToListAsync();

                foreach (var partner in existingPartners.Where(ep => !model.PartnerIds.Contains(ep.PartnerId)))
                {
                    context.InteractionPartners.Remove(partner);
                }

                foreach (var partnerId in model.PartnerIds.Except(existingPartners.Select(ep => ep.PartnerId)))
                {
                    await context.InteractionPartners.AddAsync(new InteractionPartner
                    {
                        InteractionId = interaction.Id,
                        PartnerId = partnerId
                    });
                }
            }

            // Process InteractionUsers
            if (model.UserIds?.Any() == true)
            {
                var existingUsers = await context.InteractionUsers
                    .Where(iu => iu.InteractionId == interaction.Id)
                    .ToListAsync();

                foreach (var user in existingUsers.Where(eu => !model.UserIds.Contains(eu.UserId)))
                {
                    context.InteractionUsers.Remove(user);
                }

                foreach (var userId in model.UserIds.Except(existingUsers.Select(eu => eu.UserId)))
                {
                    await context.InteractionUsers.AddAsync(new InteractionUser
                    {
                        InteractionId = interaction.Id,
                        UserId = userId
                    });
                }
            }
            await context.SaveChangesAsync();
            await jtTransaction.CommitAsync();
        }
        catch
        {
            await jtTransaction.RollbackAsync();
            throw;
        }
    }

    public PaginationResponse<InteractionModel> GetInteractions(int userId, PaginationRequest request)
    {
        var query = interactionRepository
            .GetAll()
            .AsQueryable()
            .Include(i => i.InteractionContacts).ThenInclude(ic => ic.Contact)
            .Include(i => i.InteractionPartners).ThenInclude(ip => ip.Partner)
            .Include(i => i.InteractionUsers).ThenInclude(iu => iu.User)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrEmpty(request.OrderBy))
        {
            switch (request.OrderBy.ToLower())
            {
                case "type":
                    query = request.Ascending ?? true 
                        ? query.OrderBy(x => x.Type)
                        : query.OrderByDescending(x => x.Type);
                    break;
                case "contactid":
                    // Order by first contact name through InteractionContacts
                    query = request.Ascending ?? true
                        ? query.OrderBy(x => x.InteractionContacts.FirstOrDefault().Contact.Name)
                        : query.OrderByDescending(x => x.InteractionContacts.FirstOrDefault().Contact.Name);
                    break;
                case "date":
                    query = request.Ascending ?? true
                        ? query.OrderBy(x => x.Date)
                        : query.OrderByDescending(x => x.Date);
                    break;
                case "description":
                    query = request.Ascending ?? true
                        ? query.OrderBy(x => x.Description)
                        : query.OrderByDescending(x => x.Description);
                    break;
                default:
                    query = query.OrderByDescending(x => x.Date);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.Date);
        }

        return query.Paginate(
            x => mapper.Map<InteractionModel>(x),
            request
        );
    }

    public async Task<InteractionModel?> GetInteraction(int userId, int id)
    {
        var item = await interactionRepository.GetByIdAsync(id,
            includes: new[]
            {
                nameof(Interaction.InteractionContacts),
                nameof(Interaction.InteractionPartners),
                nameof(Interaction.InteractionUsers),
                $"{nameof(Interaction.InteractionContacts)}.{nameof(InteractionContact.Contact)}",
                $"{nameof(Interaction.InteractionPartners)}.{nameof(InteractionPartner.Partner)}",
                $"{nameof(Interaction.InteractionUsers)}.{nameof(InteractionUser.User)}",
                $"{nameof(Interaction.InteractionUsers)}.{nameof(InteractionUser.User)}.{nameof(PAOUser.UserProfile)}"
            });

        if (item == null)
        {
            return default;
        }

        // Load organization unit relationships for single interaction
        await item.LoadOrganizationUnitRelationshipsAsync(context);

        InteractionModel retVal = MapEntityToModel(item, mapper);

        foreach (var contact in item.InteractionContacts)
        {
            retVal.ContactIds.Add(contact.ContactId);
        }

        foreach (var partner in item.InteractionPartners)
        {
            retVal.PartnerIds.Add(partner.PartnerId);
        }

        return retVal;
    }

    public async Task<InteractionModel?> UpdateInteractionAsync(int userId, UpdateInteractionRequest model)
    {
        var entity = await interactionRepository.GetByIdAsync(model.Id,
            includes: new[]
            {
                nameof(Interaction.InteractionContacts),
                nameof(Interaction.InteractionPartners),
                nameof(Interaction.InteractionUsers),
                $"{nameof(Interaction.InteractionUsers)}.{nameof(InteractionUser.User)}",
                $"{nameof(Interaction.InteractionUsers)}.{nameof(InteractionUser.User)}.{nameof(PAOUser.UserProfile)}"
            });

        if (entity == null) return null;

        PatchNonNullProperties(model, entity);

        // Update emails
        entity.EmailAddresses = model.EmailAddresses?.ToList() ?? new List<string>();
        //Update CreatedBy value selected by the User on the Interaction edit page
        if (model.CreatedBy.HasValue)
        {
            entity.CreatedBy = model.CreatedBy.Value;
        }

        // Handle OrganizationHierarchyIds if provided
        if (model.OrganizationHierarchyIds != null)
        {
            await UpdateOrganizationUnitRelationshipsDifferentialAsync(entity.Id, model.OrganizationHierarchyIds);
        }

        // Update junction tables
        await ProcessJunctionTables(entity, model);

        await interactionRepository.UpdateAsync(entity);
        //return mapper.Map<InteractionModel>(entity);
        return MapEntityToModel(entity, mapper);
    }

    public async Task DeleteInteractionAsync(int userId, int id)
    {
        var entity = await interactionRepository.GetByIdAsync(id);

        if (entity != null)
        {
            // Soft delete associated OrganizationUnitRelationship records
            await SoftDeleteOrganizationUnitRelationshipsAsync(id, "Interaction");
            
            await interactionRepository.Delete(entity);
        }
    }
    
    public async Task<PaginationResponse<InteractionModel>> GetContactInteractionsAsync(int contactId, PaginationRequest request)
    {
        var query = interactionRepository
            .GetAll(["InteractionContacts"])
            .Where(x => x.InteractionContacts.Any(ic => ic.ContactId == contactId) && !x.IsDeleted)
            .AsQueryable();

        // Apply access control filters
        var filteredData = await ApplyAccessControlFilters(query, GetCurrentUserOrSystemContext(), "read");
        
        // ⚡ PERFORMANCE OPTIMIZATION: Handle both IQueryable (optimized path) and IEnumerable (column filtering path)
        IEnumerable<UNOPSInteraction> interactionCollection;
        int totalCount;
        int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        int excludedRows = (pageIndex - 1) * request.PageSize;
        
        if (filteredData is IQueryable<UNOPSInteraction> queryableData)
        {
            // Optimized path: No column filtering, can paginate at database level
            var orderedQuery = queryableData.OrderByDescending(x => x.Date);
            totalCount = await orderedQuery.CountAsync();
            
            // Execute pagination at database level
            interactionCollection = await orderedQuery
                .Skip(excludedRows)
                .Take(request.PageSize)
                .ToListAsync();
        }
        else
        {
            // Column filtering path: Data is already materialized
            var interactionArray = filteredData.OfType<UNOPSInteraction>().OrderByDescending(x => x.Date).ToArray();
            totalCount = interactionArray.Length;
            
            interactionCollection = interactionArray
                .Skip(excludedRows)
                .Take(request.PageSize);
        }
        
        var pagedItems = interactionCollection.ToArray();

        // ⚡ PERFORMANCE FIX: Load organization unit relationships ONLY for paginated items
        await pagedItems.LoadOrganizationUnitRelationshipsAsync(context);

        // Collect all user IDs from paginated items to bulk load user names
        var allUserIds = pagedItems
            .SelectMany(x => new[] { x.CreatedBy, x.LastModifiedBy })
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        // Bulk load user names to avoid N+1 query problem
        var userNames = await GetUserNamesBatchAsync(allUserIds);

        var results = new List<InteractionModel>();
        foreach (var item in pagedItems)
        {
            var mapped = await MapEntityToModelAsync(item, mapper, userNames, null);
            results.Add(mapped);
        }

        return new PaginationResponse<InteractionModel>
        {
            Records = results,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = request.PageSize
        };
    }

    public async Task<PaginationResponse<InteractionModel>> GetInteractionsWithSpecification(int userId, ISpecification<Domain.Entities.Interaction> specification, PaginationRequest pagination)
    {
        // Apply the specification to the query
        var query = interactionRepository.GetAll().AsQueryable()
            .Where(x => !x.IsDeleted);
        
        // Cast to base type to apply specification, then cast back to derived type
        var baseQuery = query.Cast<Interaction>();
        var filteredBaseQuery = baseQuery.ApplySpecification(specification);
        var filteredQuery = filteredBaseQuery.OfType<UNOPSInteraction>();

        // Apply global filters using the centralized GlobalFilterService
        filteredQuery = await _globalFilterService.ApplyGlobalFiltersAsync(filteredQuery, GetCurrentUserOrSystemContext());
        
        // Apply access control filters (role-based permissions only)
        var filteredData = await ApplyAccessControlFilters(filteredQuery, GetCurrentUserOrSystemContext(), "read");
        
        // ⚡ PERFORMANCE OPTIMIZATION: Handle both IQueryable (optimized path) and IEnumerable (column filtering path)
        IEnumerable<UNOPSInteraction> interactionCollection;
        int totalCount;
        int pageIndex = pagination.PageIndex < 1 ? 1 : pagination.PageIndex;
        int excludedRows = (pageIndex - 1) * pagination.PageSize;
        
        if (filteredData is IQueryable<UNOPSInteraction> queryableData)
        {
            // Optimized path: No column filtering, can paginate at database level
            totalCount = await queryableData.CountAsync();
            
            // Execute pagination at database level
            interactionCollection = await queryableData
                .Skip(excludedRows)
                .Take(pagination.PageSize)
                .ToListAsync();
        }
        else
        {
            // Column filtering path: Data is already materialized
            var interactionArray = filteredData.OfType<UNOPSInteraction>().ToArray();
            totalCount = interactionArray.Length;
            
            interactionCollection = interactionArray
                .Skip(excludedRows)
                .Take(pagination.PageSize);
        }
        
        var pagedItems = interactionCollection.ToArray();

        // ⚡ PERFORMANCE FIX: Load organization unit relationships ONLY for paginated items
        // This avoids loading relationships for ALL interactions in the database
        await pagedItems.LoadOrganizationUnitRelationshipsAsync(context);

        // Collect all user IDs from paginated items to bulk load user names
        var allUserIds = pagedItems
            .SelectMany(x => new[] { x.CreatedBy, x.LastModifiedBy })
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        // Bulk load user names to avoid N+1 query problem
        var userNames = await GetUserNamesBatchAsync(allUserIds);

        var results = new List<InteractionModel>();
        foreach (var item in pagedItems)
        {
            var mapped = await MapEntityToModelAsync(item, mapper, userNames, null);
            results.Add(mapped);
        }

        return new PaginationResponse<InteractionModel>
        {
            Records = results,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pagination.PageSize
        };
    }

    public IEnumerable<ExternalInteractionModel> GetPostedInteractions()
    {
        // Implementation for external interactions if needed
        return new List<ExternalInteractionModel>();
    }

    public async Task<ExternalInteractionModel?> GetPostedInteraction(int id)
    {
        // Implementation for external interaction by id if needed
        return null;
    }

    public async Task<InteractionModel> UpdateInteractionAsync(int id, InteractionRequest request)
    {
        var entity = await interactionRepository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new BusinessException($"Interaction {id} does not exist.");
        }

        PatchNonNullProperties(request, entity);
        await interactionRepository.UpdateAsync(entity);

        return MapEntityToModel(entity, mapper);
    }

    /// <summary>
    /// Gets all interactions with row-level security applied
    /// ⚡ OPTIMIZED: Split queries + AsNoTracking + Batch loading for optimal performance
    /// </summary>
    public async Task<PaginationResponse<InteractionModel>> GetInteractionsAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;

        // ==========================================
        // QUERY 1: Main interactions only (no collections to avoid Cartesian product)
        // ⚡ OPTIMIZATION: AsNoTracking for read-only operation
        // ==========================================
        var query = context.Set<UNOPSInteraction>()
            .AsNoTracking()
            .Where(i => !i.IsDeleted);

        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }
        else
        {
            query = query.OrderByDescending(i => i.Date);
        }

        // Get total count first
        var totalCount = await query.CountAsync();
        
        // Get paginated entities
        var pagedEntities = await query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToListAsync();

        if (!pagedEntities.Any())
        {
            return new PaginationResponse<InteractionModel>
            {
                Records = new List<InteractionModel>(),
                TotalCount = 0,
                PageIndex = pageIndex,
                PageSize = request.PageSize,
                TotalPages = 0
            };
        }

        var interactionIds = pagedEntities.Select(i => i.Id).ToList();

        // ==========================================
        // QUERY 2-4: Batch load collections for ALL paginated interactions
        // ⚡ OPTIMIZATION: Load in 3 separate queries instead of N+1 pattern
        // ==========================================
        
        // Batch load InteractionContacts with Contact and Partner
        var allInteractionContacts = await context.Set<InteractionContact>()
            .AsNoTracking()
            .Where(ic => interactionIds.Contains(ic.InteractionId))
            .Include(ic => ic.Contact)
                .ThenInclude(c => c.Partner)
            .ToListAsync();
        
        // Batch load InteractionPartners with Partner
        var allInteractionPartners = await context.Set<InteractionPartner>()
            .AsNoTracking()
            .Where(ip => interactionIds.Contains(ip.InteractionId))
            .Include(ip => ip.Partner)
            .ToListAsync();
        
        // Batch load InteractionUsers with User and UserProfile
        var allInteractionUsers = await context.Set<InteractionUser>()
            .AsNoTracking()
            .Where(iu => interactionIds.Contains(iu.InteractionId))
            .Include(iu => iu.User)
                .ThenInclude(u => u.UserProfile)
            .ToListAsync();

        // Group collections by interaction ID for fast assignment
        var contactsByInteraction = allInteractionContacts.GroupBy(ic => ic.InteractionId).ToDictionary(g => g.Key, g => g.ToList());
        var partnersByInteraction = allInteractionPartners.GroupBy(ip => ip.InteractionId).ToDictionary(g => g.Key, g => g.ToList());
        var usersByInteraction = allInteractionUsers.GroupBy(iu => iu.InteractionId).ToDictionary(g => g.Key, g => g.ToList());

        // Assign collections to entities
        foreach (var entity in pagedEntities)
        {
            entity.InteractionContacts = contactsByInteraction.TryGetValue(entity.Id, out var contacts) ? contacts : new List<InteractionContact>();
            entity.InteractionPartners = partnersByInteraction.TryGetValue(entity.Id, out var partners) ? partners : new List<InteractionPartner>();
            entity.InteractionUsers = usersByInteraction.TryGetValue(entity.Id, out var users) ? users : new List<InteractionUser>();
        }

        // ⚡ PERFORMANCE FIX: Load organization unit relationships ONLY for paginated items
        await pagedEntities.LoadOrganizationUnitRelationshipsAsync(context);

        // Collect all user IDs from the paginated results to bulk load user names
        var allUserIds = pagedEntities
            .SelectMany(x => new[] { x.CreatedBy, x.LastModifiedBy })
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        // Bulk load user names to avoid N+1 query problem
        var userNames = await GetUserNamesBatchAsync(allUserIds);

        // Map entities to models using pre-loaded user names
        var results = new List<InteractionModel>();
        foreach (var entity in pagedEntities)
        {
            var mapped = await MapEntityToModelAsync(entity, mapper, userNames, user);
            results.Add(mapped);
        }

        return new PaginationResponse<InteractionModel>
        {
            Records = results,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = request.PageSize,
            TotalPages = request.PageSize > 0 ? (int)Math.Ceiling((double)totalCount / request.PageSize) : 0
        };
    }

    /// <summary>
    /// Gets a specific interaction with row-level security applied
    /// ⚡ OPTIMIZED: Split queries + AsNoTracking for better performance
    /// </summary>
    public async Task<InteractionModel?> GetInteractionAsync(ClaimsPrincipal user, int id)
    {
        // ==========================================
        // QUERY 1: Main interaction entity only (no collections)
        // ⚡ OPTIMIZATION: AsNoTracking for read-only operation
        // ==========================================
        var item = await context.Set<UNOPSInteraction>()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
            
        if (item == null) return null;

        // ==========================================
        // QUERY 2-4: Load collections independently to avoid Cartesian product
        // Each query loads only what it needs with AsNoTracking
        // ==========================================
        
        // Load InteractionContacts with Contact and Partner
        var interactionContacts = await context.Set<InteractionContact>()
            .AsNoTracking()
            .Where(ic => ic.InteractionId == id)
            .Include(ic => ic.Contact)
                .ThenInclude(c => c.Partner)
            .ToListAsync();
        
        // Load InteractionPartners with Partner
        var interactionPartners = await context.Set<InteractionPartner>()
            .AsNoTracking()
            .Where(ip => ip.InteractionId == id)
            .Include(ip => ip.Partner)
            .ToListAsync();
        
        // Load InteractionUsers with User and UserProfile
        var interactionUsers = await context.Set<InteractionUser>()
            .AsNoTracking()
            .Where(iu => iu.InteractionId == id)
            .Include(iu => iu.User)
                .ThenInclude(u => u.UserProfile)
            .ToListAsync();

        // Assign collections to entity
        item.InteractionContacts = interactionContacts;
        item.InteractionPartners = interactionPartners;
        item.InteractionUsers = interactionUsers;

        // Load organization unit relationships for single interaction
        await item.LoadOrganizationUnitRelationshipsAsync(context);

        return await MapEntityToModelAsync(item, mapper, user);
    }

    /// <summary>
    /// Updates an interaction with permission validation
    /// ⚡ OPTIMIZED: Split queries for loading, tracked entity for updates
    /// </summary>
    public async Task<InteractionModel?> UpdateInteractionAsync(ClaimsPrincipal user, UpdateInteractionRequest model)
    {
        // ==========================================
        // QUERY 1: Load main interaction WITH TRACKING (needed for update)
        // NOTE: No AsNoTracking here since we're updating the entity
        // ==========================================
        var entity = await context.Set<UNOPSInteraction>()
            .FirstOrDefaultAsync(i => i.Id == model.Id && !i.IsDeleted);
            
        if (entity == null)
        {
            throw new BusinessException($"Interaction {model.Id} does not exist.");
        }

        // ==========================================
        // QUERY 2-4: Load collections separately for reference
        // ⚡ OPTIMIZATION: Split queries to avoid Cartesian product
        // ==========================================
        
        var interactionContacts = await context.Set<InteractionContact>()
            .Where(ic => ic.InteractionId == model.Id)
            .Include(ic => ic.Contact)
                .ThenInclude(c => c.Partner)
            .ToListAsync();
        
        var interactionPartners = await context.Set<InteractionPartner>()
            .Where(ip => ip.InteractionId == model.Id)
            .Include(ip => ip.Partner)
            .ToListAsync();
        
        var interactionUsers = await context.Set<InteractionUser>()
            .Where(iu => iu.InteractionId == model.Id)
            .Include(iu => iu.User)
                .ThenInclude(u => u.UserProfile)
            .ToListAsync();

        // Assign collections to entity
        entity.InteractionContacts = interactionContacts;
        entity.InteractionPartners = interactionPartners;
        entity.InteractionUsers = interactionUsers;

        // Load organization unit relationships for single interaction
        await entity.LoadOrganizationUnitRelationshipsAsync(context);

        PatchNonNullProperties(model, entity);

        // Update emails
        entity.EmailAddresses = model.EmailAddresses?.ToList() ?? new List<string>();

        // Handle OrganizationHierarchyIds if provided
        if (model.OrganizationHierarchyIds != null)
        {
            await UpdateOrganizationUnitRelationshipsDifferentialAsync(entity.Id, model.OrganizationHierarchyIds);
        }

        // Update junction tables
        await ProcessJunctionTables(entity, model);

        await interactionRepository.UpdateAsync(entity);

        return await MapEntityToModelAsync(entity, mapper, user);
    }

    /// <summary>
    /// Deletes an interaction with permission validation
    /// ⚡ OPTIMIZED: Minimal query for delete operation (no unnecessary includes)
    /// </summary>
    public async Task DeleteInteractionAsync(ClaimsPrincipal user, int id)
    {
        // ==========================================
        // QUERY 1: Load only main entity for delete (no collections needed)
        // ⚡ OPTIMIZATION: Don't load unnecessary related data for delete operation
        // ==========================================
        var entity = await context.Set<UNOPSInteraction>()
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
            
        if (entity == null) return;

        // Soft delete associated OrganizationUnitRelationship records
        await SoftDeleteOrganizationUnitRelationshipsAsync(id, "Interaction");

        await interactionRepository.Delete(entity);
    }

    /// <summary>
    /// Gets comprehensive interaction details with security checks for AI prompts
    /// ⚡ OPTIMIZED: Split queries + AsNoTracking + Parallel execution
    /// </summary>
    public async Task<InteractionModel?> GetInteractionDetailsAsync(ClaimsPrincipal user, int id)
    {
        // ==========================================
        // QUERY 1: Main interaction with Documents navigation property
        // ⚡ OPTIMIZATION: AsNoTracking for read-only operation
        // ==========================================
        var item = await context.Set<UNOPSInteraction>()
            .AsNoTracking()
            .Include(i => i.Documents)
                .ThenInclude(d => d.DocumentType)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        if (item == null) return null;

        // ==========================================
        // PARALLEL WAVE 1: Load junction table collections concurrently if DbContextFactory available
        // ⚡ OPTIMIZATION: Parallel execution for better performance
        // ==========================================
        
        List<InteractionContact> interactionContacts;
        List<InteractionPartner> interactionPartners;
        List<InteractionUser> interactionUsers;

        if (_dbContextFactory != null)
        {
            // Execute queries in parallel using separate DbContext instances
            var task1 = Task.Run(async () =>
            {
                await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                return await ctx.Set<InteractionContact>()
                    .AsNoTracking()
                    .Where(ic => ic.InteractionId == id)
                    .Include(ic => ic.Contact)
                        .ThenInclude(c => c.Partner)
                    .ToListAsync();
            });

            var task2 = Task.Run(async () =>
            {
                await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                return await ctx.Set<InteractionPartner>()
                    .AsNoTracking()
                    .Where(ip => ip.InteractionId == id)
                    .Include(ip => ip.Partner)
                    .ToListAsync();
            });

            var task3 = Task.Run(async () =>
            {
                await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                return await ctx.Set<InteractionUser>()
                    .AsNoTracking()
                    .Where(iu => iu.InteractionId == id)
                    .Include(iu => iu.User)
                        .ThenInclude(u => u.UserProfile)
                    .ToListAsync();
            });

            // Wait for all parallel tasks to complete
            await Task.WhenAll(task1, task2, task3);

            interactionContacts = await task1;
            interactionPartners = await task2;
            interactionUsers = await task3;
        }
        else
        {
            // Fallback to sequential execution if DbContextFactory not available
            interactionContacts = await context.Set<InteractionContact>()
                .AsNoTracking()
                .Where(ic => ic.InteractionId == id)
                .Include(ic => ic.Contact)
                    .ThenInclude(c => c.Partner)
                .ToListAsync();

            interactionPartners = await context.Set<InteractionPartner>()
                .AsNoTracking()
                .Where(ip => ip.InteractionId == id)
                .Include(ip => ip.Partner)
                .ToListAsync();

            interactionUsers = await context.Set<InteractionUser>()
                .AsNoTracking()
                .Where(iu => iu.InteractionId == id)
                .Include(iu => iu.User)
                    .ThenInclude(u => u.UserProfile)
                .ToListAsync();
        }

        // Assign collections to entity
        item.InteractionContacts = interactionContacts;
        item.InteractionPartners = interactionPartners;
        item.InteractionUsers = interactionUsers;

        // Load organization unit relationships for single interaction
        await item.LoadOrganizationUnitRelationshipsAsync(context);

        var result = await MapEntityToModelAsync(item, mapper, user);
        
        // Populate junction table IDs
        if (interactionContacts != null)
        {
            result.ContactIds = interactionContacts.Select(ic => ic.ContactId).ToList();
        }

        if (interactionPartners != null)
        {
            result.PartnerIds = interactionPartners.Select(ip => ip.PartnerId).ToList();
        }

        return result;
    }

    /// <summary>
    /// Gets interaction with comprehensive details formatted for AI prompt processing
    /// ⚡ OPTIMIZED: Split queries + AsNoTracking + Parallel execution for AI operations
    /// </summary>
    public async Task<object> GetInteractionDetailsForAIAsync(ClaimsPrincipal user, int id)
    {
        // ==========================================
        // QUERY 1: Main interaction with Documents navigation property
        // ⚡ OPTIMIZATION: AsNoTracking for read-only AI operation
        // ==========================================
        var entity = await context.Set<UNOPSInteraction>()
            .AsNoTracking()
            .Include(i => i.Documents)
                .ThenInclude(d => d.DocumentType)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        if (entity == null) return new { error = "Interaction not found" };

        // ==========================================
        // PARALLEL WAVE 1: Load all junction table collections concurrently if DbContextFactory available
        // Otherwise execute sequentially
        // ⚡ OPTIMIZATION: Parallel execution for maximum performance
        // ==========================================
        
        List<InteractionContact> interactionContacts;
        List<InteractionPartner> interactionPartners;
        List<InteractionUser> interactionUsers;

        if (_dbContextFactory != null)
        {
            // Execute queries in parallel using separate DbContext instances
            var task1 = Task.Run(async () =>
            {
                await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                return await ctx.Set<InteractionContact>()
                    .AsNoTracking()
                    .Where(ic => ic.InteractionId == id)
                    .Include(ic => ic.Contact)
                        .ThenInclude(c => c.Partner)
                    .ToListAsync();
            });

            var task2 = Task.Run(async () =>
            {
                await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                return await ctx.Set<InteractionPartner>()
                    .AsNoTracking()
                    .Where(ip => ip.InteractionId == id)
                    .Include(ip => ip.Partner)
                    .ToListAsync();
            });

            var task3 = Task.Run(async () =>
            {
                await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                return await ctx.Set<InteractionUser>()
                    .AsNoTracking()
                    .Where(iu => iu.InteractionId == id)
                    .Include(iu => iu.User)
                        .ThenInclude(u => u.UserProfile)
                    .ToListAsync();
            });

            // Wait for all parallel tasks to complete
            await Task.WhenAll(task1, task2, task3);

            interactionContacts = await task1;
            interactionPartners = await task2;
            interactionUsers = await task3;
        }
        else
        {
            // Fallback to sequential execution if DbContextFactory not available
            interactionContacts = await context.Set<InteractionContact>()
                .AsNoTracking()
                .Where(ic => ic.InteractionId == id)
                .Include(ic => ic.Contact)
                    .ThenInclude(c => c.Partner)
                .ToListAsync();

            interactionPartners = await context.Set<InteractionPartner>()
                .AsNoTracking()
                .Where(ip => ip.InteractionId == id)
                .Include(ip => ip.Partner)
                .ToListAsync();

            interactionUsers = await context.Set<InteractionUser>()
                .AsNoTracking()
                .Where(iu => iu.InteractionId == id)
                .Include(iu => iu.User)
                    .ThenInclude(u => u.UserProfile)
                .ToListAsync();
        }

        // Assign collections to entity for organization unit loading
        entity.InteractionContacts = interactionContacts;
        entity.InteractionPartners = interactionPartners;
        entity.InteractionUsers = interactionUsers;

        // Load organization unit relationships
        await entity.LoadOrganizationUnitRelationshipsAsync(context);

        // Create structured JSON for AI prompt placeholders
        var result = new
        {
            id = entity.Id,
            subject = entity.Subject,
            description = entity.Description,
            date = entity.Date.ToString("yyyy-MM-dd"),
            time = entity.Date.ToString("HH:mm"),
            type = entity.Type.ToString(),
            location = entity.Location,
            status = "Active", // Default status for interactions

            // Contact information
            contacts = interactionContacts?.Select(ic => new
            {
                id = ic.Contact.Id,
                name = $"{ic.Contact.FirstName} {ic.Contact.LastName}".Trim(),
                firstName = ic.Contact.FirstName,
                lastName = ic.Contact.LastName,
                email = ic.Contact.Email,
                title = ic.Contact.Title,
                phone = ic.Contact.Phone,
                mobile = ic.Contact.Mobile,
                partner = ic.Contact.Partner != null ? new
                {
                    id = ic.Contact.Partner.Id,
                    name = ic.Contact.Partner.Name
                } : null
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),

            // Partner information
            partners = interactionPartners?.Select(ip => new
            {
                id = ip.Partner.Id,
                name = ip.Partner.Name,
                status = ip.Partner.Status.ToString()
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),

            // User information (UNOPS staff)
            users = interactionUsers?.Select(iu => new
            {
                id = iu.User.Id,
                name = iu.User.Name,
                email = iu.User.UserProfile?.UserEmail,
                title = iu.User.UserProfile?.Position,
                office = iu.User.UserProfile?.OrgUnit
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),

            // Organization unit relationships
            organizationUnits = entity.OrganizationUnitRelationships?.Where(r => r.Status == (Domain.Entities.EntityStatus)1 && !r.IsDeleted)
                .Select(r => new
                {
                    id = r.OrganizationHierarchy.Id,
                    name = r.OrganizationHierarchy.Name,
                    code = r.OrganizationHierarchy.Code,
                    type = r.OrganizationHierarchy.Type.ToString()
                }).Cast<dynamic>().ToList() ?? new List<dynamic>(),

            // Documents and attachments
            documents = entity.Documents?.Where(d => d != null).Select(d => new
            {
                id = d.Id,
                link = d.Link,
                type = d.Type,
                documentType = d.DocumentType?.Name,
                uploadDate = d.CreatedDate.ToString("yyyy-MM-dd")
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),

            // Email information
            emailAddresses = entity.EmailAddresses ?? new List<string>(),

            // Computed names for easy access
            contactNames = string.Join(", ", interactionContacts?.Select(ic => $"{ic.Contact.FirstName} {ic.Contact.LastName}".Trim()) ?? new List<string>()),
            partnerNames = string.Join(", ", interactionPartners?.Select(ip => ip.Partner.Name) ?? new List<string>()),
            userNames = string.Join(", ", interactionUsers?.Select(iu => iu.User.Name) ?? new List<string>()),

            // Summary statistics
            summary = new
            {
                totalContacts = interactionContacts?.Count ?? 0,
                totalPartners = interactionPartners?.Count ?? 0,
                totalUsers = interactionUsers?.Count ?? 0,
                totalDocuments = entity.Documents?.Count ?? 0,
                hasDocuments = entity.Documents?.Any() ?? false,
                hasEmailAddresses = entity.EmailAddresses?.Any() ?? false
            },

            // Audit information
            auditInfo = new
            {
                createdDate = entity.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                lastModifiedDate = entity.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Not modified",
                createdBy = entity.CreatedBy,
                lastModifiedBy = entity.LastModifiedBy
            }
        };

        return result;
    }

    /// <summary>
    /// Gets comprehensive interaction details for AI prompts including all related entities (legacy method without security)
    /// </summary>
    public async Task<InteractionModel?> GetInteractionDetailsLegacyAsync(int id)
    {
        var item = await interactionRepository.GetByIdAsync(id,
            includes: new[]
            {
                "InteractionContacts",
                "InteractionPartners",
                "InteractionUsers",
                "InteractionContacts.Contact",
                "InteractionContacts.Contact.Partner",
                "InteractionPartners.Partner",
                "InteractionUsers.User",
                "InteractionUsers.User.UserProfile",
                "Documents"
            });

        if (item == null)
        {
            return null;
        }

        // Load organization unit relationships for single interaction
        await item.LoadOrganizationUnitRelationshipsAsync(context);

        var result = await MapEntityToModelAsync(item, mapper, null);
        
        // Populate junction table IDs
        if (item.InteractionContacts != null)
        {
            result.ContactIds = item.InteractionContacts.Select(ic => ic.ContactId).ToList();
        }

        if (item.InteractionPartners != null)
        {
            result.PartnerIds = item.InteractionPartners.Select(ip => ip.PartnerId).ToList();
        }
        

        return result;
    }

    /// <summary>
    /// Implementation of abstract method from BaseUNOPSManager
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        if (user != null)
        {
            return await GetInteractionDetailsAsync(user, entityId);
        }
        else
        {
            return await GetInteractionDetailsLegacyAsync(entityId);
        }
    }

    /// <summary>
    /// Gets basic interaction data by ID without nested entities
    /// </summary>
    public override async Task<object> GetBasicEntityDataAsync(int id)
    {
        var interaction = await _context.Interactions.FirstOrDefaultAsync(e => e.Id == id);
        if (interaction != null)
        {
            return mapper.Map<UNOPSInteraction, InteractionModel>(interaction);
        }
        return null;
    }


    public virtual async Task<InteractionModel?> FindGmailInteractionAsync(GmailInteractionRequest model)
    {
        var entity = await context.Interactions
                            .FirstOrDefaultAsync(x => x.GmailThreadId == model.GmailThreadId && x.GmailMessageId == model.GmailMessageId && !x.IsDeleted);

        return entity == null ? null : mapper.Map<InteractionModel>(entity);
    }

    public virtual async Task<InteractionModel?> CreateGmailInteractionAsync(InteractionRequest model)
    {
        // Check for existing interaction with same Gmail thread and message IDs
        if (!string.IsNullOrWhiteSpace(model.GmailThreadId) || !string.IsNullOrWhiteSpace(model.GmailMessageId))
        {
            var existingInteraction = await context.Interactions
                .FirstOrDefaultAsync(x => x.GmailThreadId == model.GmailThreadId && 
                                        x.GmailMessageId == model.GmailMessageId && 
                                        !x.IsDeleted);
            
            if (existingInteraction != null)
            {
                // Return the existing interaction instead of creating a duplicate
                return mapper.Map<InteractionModel>(existingInteraction);
            }
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        if(model.EmailAddresses != null && model.EmailAddresses.Count > 0)
        {
            model.EmailAddresses = model.EmailAddresses.Distinct().ToList();
        }

        var entity = await MapModelToEntity(model);

        try
        {
            entity.Name = model.Subject.Substring(0, Math.Min(model.Subject.Length, 20)) + " - " + model.Date;

            await context.Interactions.AddAsync(entity);
            await context.SaveChangesAsync();

            // Get current user and their organization unit
            var currentUser = GetCurrentUserOrSystemContext();
            var userIdClaim = currentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out var id) ? id : null;

            if (userId != null && userId.HasValue)
            {
                // Get user profile by email to find their org unit
                var userProfile = await context.UserProfile
                    .FirstOrDefaultAsync(up => up.UserId == userId.Value);

                if (userProfile?.OrgUnit != null)
                {
                    // Find the organization hierarchy by org unit code
                    var orgHierarchy = await context.OrganizationHierarchies
                        .FirstOrDefaultAsync(oh => oh.Code == userProfile.OrgUnit && 
                                                    oh.Type == Domain.Enums.OrganizationUnitType.OrgUnit);

                    if (orgHierarchy != null)
                    {
                        // Create organization unit relationship for the interaction
                        var newRelationship = new OrganizationUnitRelationship
                        {
                            OrganizationHierarchyId = orgHierarchy.Id,
                            EntityId = entity.Id,
                            EntityType = nameof(Interaction),
                            Name = $"Interaction-{entity.Id}-{orgHierarchy.Code}",
                            Status = EntityStatus.Active
                        };
                            
                        context.OrganizationUnitRelationships.Add(newRelationship);
                        await context.SaveChangesAsync();
                    }
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await ProcessGmailInteractionJunctionTables(entity, model);
        return mapper.Map<InteractionModel>(entity);
    }

    private async Task ProcessGmailInteractionJunctionTables(Interaction interaction, InteractionRequest model)
    {
        await using var jtTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Get existing relationships in parallel for better performance
            var existingContacts = await context.InteractionContacts
                .Where(ic => ic.InteractionId == interaction.Id)
                .Select(ic => ic.ContactId)
                .ToListAsync();

            var existingUsers = await context.InteractionUsers
                .Where(iu => iu.InteractionId == interaction.Id)
                .Select(iu => iu.UserId)
                .ToListAsync();

            var existingPartners = await context.InteractionPartners
                .Where(ip => ip.InteractionId == interaction.Id)
                .Select(ip => ip.PartnerId)
                .ToListAsync();

            var existingContactIds = existingContacts.ToHashSet();
            var existingUserIds = existingUsers.ToHashSet();
            var existingPartnerIds = existingPartners.ToHashSet();

            // Prepare bulk insert lists
            var contactsToAdd = new List<InteractionContact>();
            var partnersToAdd = new List<InteractionPartner>();
            var usersToAdd = new List<InteractionUser>();

            // Process ContactIds - bulk prepare
            if (model.ContactIds?.Any() == true)
            {
                var newContactIds = model.ContactIds.Where(id => !existingContactIds.Contains(id)).ToList();
                if (newContactIds.Any())
                {
                    // Load contacts into context to ensure EF can track them
                    var contacts = await context.Contacts.Where(c => newContactIds.Contains(c.Id)).ToListAsync();
                    
                    foreach (var contact in contacts)
                    {
                        contactsToAdd.Add(new InteractionContact
                        {
                            InteractionId = interaction.Id,
                            ContactId = contact.Id,
                            Contact = contact
                        });
                    }
                }
            }

            // Process PartnerIds - bulk prepare
            if (model.PartnerIds?.Any() == true)
            {
                var newPartnerIds = model.PartnerIds.Where(id => !existingPartnerIds.Contains(id)).ToList();
                if (newPartnerIds.Any())
                {
                    // Load partners into context to ensure EF can track them
                    var partners = await context.Partners.Where(p => newPartnerIds.Contains(p.Id)).ToListAsync();
                    
                    foreach (var partner in partners)
                    {
                        partnersToAdd.Add(new InteractionPartner
                        {
                            InteractionId = interaction.Id,
                            PartnerId = partner.Id,
                            Partner = partner
                        });
                    }
                }
            }

            // Process UserIds - bulk prepare
            if (model.UserIds?.Any() == true)
            {
                var newUserIds = model.UserIds.Where(id => !existingUserIds.Contains(id)).ToList();
                if (newUserIds.Any())
                {
                    // Load users into context to ensure EF can track them
                    var users = await context.PAOUsers.Where(u => newUserIds.Contains(u.Id)).ToListAsync();
                    
                    foreach (var user in users)
                    {
                        usersToAdd.Add(new InteractionUser
                        {
                            InteractionId = interaction.Id,
                            UserId = user.Id,
                            User = user
                        });
                    }
                }
            }

            // Bulk insert all relationships
            if (contactsToAdd.Any())
            {
                await context.InteractionContacts.AddRangeAsync(contactsToAdd);
            }

            if (partnersToAdd.Any())
            {
                await context.InteractionPartners.AddRangeAsync(partnersToAdd);
            }

            if (usersToAdd.Any())
            {
                await context.InteractionUsers.AddRangeAsync(usersToAdd);
            }

            await context.SaveChangesAsync();
            await jtTransaction.CommitAsync();
        }
        catch
        {
            await jtTransaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Gets multiple interactions by their IDs for search results
    /// ⚡ OPTIMIZED: Split queries + AsNoTracking + Batch loading
    /// </summary>
    public override async Task<List<object>> GetByIdsAsync(int[] ids, ClaimsPrincipal user = null)
    {
        if (ids == null || ids.Length == 0)
            return new List<object>();

        // ==========================================
        // QUERY 1: Main interactions only (no collections to avoid Cartesian product)
        // ⚡ OPTIMIZATION: AsNoTracking for read-only operation
        // ==========================================
        var interactions = await context.Set<UNOPSInteraction>()
            .AsNoTracking()
            .Where(i => ids.Contains(i.Id) && !i.IsDeleted)
            .ToListAsync();

        if (!interactions.Any())
            return new List<object>();

        var interactionIds = interactions.Select(i => i.Id).ToList();

        // ==========================================
        // QUERY 2-4: Batch load collections for ALL interactions
        // ⚡ OPTIMIZATION: Load in 3 separate queries instead of N+1 pattern
        // ==========================================
        
        // Batch load InteractionContacts with Contact and Partner
        var allInteractionContacts = await context.Set<InteractionContact>()
            .AsNoTracking()
            .Where(ic => interactionIds.Contains(ic.InteractionId))
            .Include(ic => ic.Contact)
                .ThenInclude(c => c.Partner)
            .ToListAsync();
        
        // Batch load InteractionPartners with Partner
        var allInteractionPartners = await context.Set<InteractionPartner>()
            .AsNoTracking()
            .Where(ip => interactionIds.Contains(ip.InteractionId))
            .Include(ip => ip.Partner)
            .ToListAsync();
        
        // Batch load InteractionUsers with User and UserProfile
        var allInteractionUsers = await context.Set<InteractionUser>()
            .AsNoTracking()
            .Where(iu => interactionIds.Contains(iu.InteractionId))
            .Include(iu => iu.User)
                .ThenInclude(u => u.UserProfile)
            .ToListAsync();

        // Group collections by interaction ID for fast assignment
        var contactsByInteraction = allInteractionContacts.GroupBy(ic => ic.InteractionId).ToDictionary(g => g.Key, g => g.ToList());
        var partnersByInteraction = allInteractionPartners.GroupBy(ip => ip.InteractionId).ToDictionary(g => g.Key, g => g.ToList());
        var usersByInteraction = allInteractionUsers.GroupBy(iu => iu.InteractionId).ToDictionary(g => g.Key, g => g.ToList());

        // Assign collections to entities
        foreach (var interaction in interactions)
        {
            interaction.InteractionContacts = contactsByInteraction.TryGetValue(interaction.Id, out var contacts) ? contacts : new List<InteractionContact>();
            interaction.InteractionPartners = partnersByInteraction.TryGetValue(interaction.Id, out var partners) ? partners : new List<InteractionPartner>();
            interaction.InteractionUsers = usersByInteraction.TryGetValue(interaction.Id, out var users) ? users : new List<InteractionUser>();
        }

        // Load organization unit relationships
        await interactions.LoadOrganizationUnitRelationshipsAsync(context);

        // Apply access control if user context is provided
        if (user != null)
        {
            var filteredData = await ApplyAccessControlFilters(interactions.AsQueryable(), user, "read");
            if (filteredData is IEnumerable<UNOPSInteraction> interactionList)
            {
                interactions = interactionList.ToList();
            }
        }

        // Collect all user IDs to bulk load user names
        var allUserIds = interactions
            .SelectMany(x => new[] { x.CreatedBy, x.LastModifiedBy })
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        // Bulk load user names to avoid N+1 query problem
        var userNames = await GetUserNamesBatchAsync(allUserIds);

        // Process interactions sequentially to avoid DbContext threading issues
        var results = new List<InteractionModel>();
        foreach (var interaction in interactions)
        {
            var mappedInteraction = await MapEntityToModelAsync(interaction, mapper, userNames, user);
            results.Add(mappedInteraction);
        }
        
        return results.Cast<object>().ToList();
    }

    
    /// <summary>
    /// Get supported search fields for interactions - helps frontend build dynamic search forms
    /// </summary>
    /// <returns>List of all supported search fields with their metadata</returns>
    public List<SearchFieldInfo> GetInteractionSearchFields()
    {
        try
        {
            var fields = new List<SearchFieldInfo>
            {
                // Direct Interaction fields - using translation keys
                new() { 
                    Field = "type", 
                    DisplayName = "label.interaction.type", 
                    FieldType = "enum", 
                    AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" },
                    DropdownOptions = new List<DropdownOption>
                    {
                        new() { Value = "Email", Label = "enums.interactionType.email" },
                        new() { Value = "Chat", Label = "enums.interactionType.chat" },
                        new() { Value = "Call", Label = "enums.interactionType.call" },
                        new() { Value = "VirtualMeeting", Label = "enums.interactionType.virtualMeeting" },
                        new() { Value = "InPersonMeeting", Label = "enums.interactionType.inPersonMeeting" },
                        new() { Value = "Other", Label = "enums.interactionType.other" }
                    }
                },
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
                new() { Field = "subject", DisplayName = "label.interaction.subject", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "description", DisplayName = "label.interaction.description", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "date", DisplayName = "label.interaction.date", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "fromDate", DisplayName = "label.interaction.fromDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "toDate", DisplayName = "label.interaction.toDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "createdDate", DisplayName = "label.common.createdDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },

                // Contact relationship fields through InteractionContacts junction table - using translation keys
                new() { Field = "interactioncontacts.contact.fullName", DisplayName = "label.contact.fullName", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "interactioncontacts.contact.firstName", DisplayName = "label.contact.firstName", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "interactioncontacts.contact.lastName", DisplayName = "label.contact.lastName", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "interactioncontacts.contact.email", DisplayName = "label.contact.email", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },

                // Partner relationship fields through InteractionPartners junction table - using translation keys  
                new() { Field = "interactionpartners.partner.name", DisplayName = "label.partner.name", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                
                // User relationship fields through InteractionUsers junction table - using translation keys
                new() { Field = "interactionusers.user.name", DisplayName = "label.user.name", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                
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
                new() { Field = "createdDate", DisplayName = "label.common.createdDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "lastModifiedDate", DisplayName = "label.common.lastModifiedDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
            };
            
            return fields;
        }
        catch (Exception ex)
        {
            // Log error - no logger available in this manager
            Console.WriteLine($"Error retrieving interaction search fields: {ex.Message}");
            return new List<SearchFieldInfo>();
        }
    }

    /// <summary>
    /// Soft deletes OrganizationUnitRelationship records for a given entity
    /// </summary>
    private async Task SoftDeleteOrganizationUnitRelationshipsAsync(int entityId, string entityType)
    {
        var relationships = await context.OrganizationUnitRelationships
            .Where(r => r.EntityId == entityId && r.EntityType == entityType && !r.IsDeleted)
            .ToListAsync();

        // Get current user ID from claims
        var currentUser = GetCurrentUserOrSystemContext();
        var userIdClaim = currentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var id) ? id : 0;

        foreach (var relationship in relationships)
        {
            relationship.SetDeleteAuditData(userId);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Data retrieval method for AI prompts - Gets comprehensive interaction details for opportunity creation
    /// This method is called via reflection by the Gemini Manager
    /// ⚡ OPTIMIZED: Split queries + AsNoTracking + Parallel execution for AI operations
    /// </summary>
    /// <param name="id">Interaction ID</param>
    /// <returns>Dictionary containing all interaction details formatted for AI prompt placeholders</returns>
    public async Task<Dictionary<string, object>> GetInteractionDetailsForOpportunityCreationAsync(int id)
    {
        // ==========================================
        // QUERY 1: Main interaction with Documents navigation property
        // ⚡ OPTIMIZATION: AsNoTracking for read-only AI operation
        // ==========================================
        var interaction = await context.Set<Interaction>()
            .AsNoTracking()
            .Include(i => i.Documents)
                .ThenInclude(d => d.DocumentType)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        if (interaction == null)
        {
            return null;
        }

        // ==========================================
        // PARALLEL WAVE 1: Load junction table collections concurrently if DbContextFactory available
        // ⚡ OPTIMIZATION: Parallel execution for maximum AI processing performance
        // ==========================================
        
        List<InteractionContact> interactionContacts;
        List<InteractionPartner> interactionPartners;
        List<InteractionUser> interactionUsers;

        if (_dbContextFactory != null)
        {
            // Execute queries in parallel using separate DbContext instances
            var task1 = Task.Run(async () =>
            {
                await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                return await ctx.Set<InteractionContact>()
                    .AsNoTracking()
                    .Where(ic => ic.InteractionId == id)
                    .Include(ic => ic.Contact)
                    .ToListAsync();
            });

            var task2 = Task.Run(async () =>
            {
                await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                return await ctx.Set<InteractionPartner>()
                    .AsNoTracking()
                    .Where(ip => ip.InteractionId == id)
                    .Include(ip => ip.Partner)
                    .ToListAsync();
            });

            var task3 = Task.Run(async () =>
            {
                await using var ctx = await _dbContextFactory.CreateDbContextAsync();
                return await ctx.Set<InteractionUser>()
                    .AsNoTracking()
                    .Where(iu => iu.InteractionId == id)
                    .Include(iu => iu.User)
                        .ThenInclude(u => u.UserProfile)
                    .ToListAsync();
            });

            // Wait for all parallel tasks to complete
            await Task.WhenAll(task1, task2, task3);

            interactionContacts = await task1;
            interactionPartners = await task2;
            interactionUsers = await task3;
        }
        else
        {
            // Fallback to sequential execution if DbContextFactory not available
            interactionContacts = await context.Set<InteractionContact>()
                .AsNoTracking()
                .Where(ic => ic.InteractionId == id)
                .Include(ic => ic.Contact)
                .ToListAsync();

            interactionPartners = await context.Set<InteractionPartner>()
                .AsNoTracking()
                .Where(ip => ip.InteractionId == id)
                .Include(ip => ip.Partner)
                .ToListAsync();

            interactionUsers = await context.Set<InteractionUser>()
                .AsNoTracking()
                .Where(iu => iu.InteractionId == id)
                .Include(iu => iu.User)
                    .ThenInclude(u => u.UserProfile)
                .ToListAsync();
        }

        // Build comprehensive interaction details
        var details = new Dictionary<string, object>
        {
            ["id"] = interaction.Id,
            ["subject"] = interaction.Subject ?? string.Empty,
            ["description"] = interaction.Description ?? string.Empty,
            ["date"] = interaction.Date.ToString("yyyy-MM-dd"),
            ["type"] = interaction.Type.ToString(),
            ["location"] = interaction.Location ?? string.Empty,
            ["status"] = interaction.Status.ToString(),
            
            // UNOPS participants with org units
            ["users"] = interactionUsers?.Select(iu => new
            {
                id = iu.User?.Id ?? 0,
                name = iu.User?.Name ?? string.Empty,
                position = iu.User?.UserProfile?.Position ?? string.Empty,
                orgUnit = iu.User?.UserProfile?.OrgUnit ?? string.Empty
            }).ToList() ?? (object)new List<object>(),
            
            // Partner contacts
            ["contacts"] = interactionContacts?.Select(ic => new
            {
                id = ic.Contact?.Id ?? 0,
                name = $"{ic.Contact?.FirstName ?? string.Empty} {ic.Contact?.LastName ?? string.Empty}".Trim(),
                email = ic.Contact?.Email ?? string.Empty,
                phone = ic.Contact?.Phone ?? string.Empty
            }).ToList() ?? (object)new List<object>(),
            
            // Partner organizations
            ["partners"] = interactionPartners?.Select(ip => new
            {
                id = ip.Partner?.Id ?? 0,
                name = ip.Partner?.Name ?? string.Empty
            }).ToList() ?? (object)new List<object>(),
            
            // Documents
            ["documents"] = interaction.Documents?.Where(d => d != null).Select(d => new
            {
                id = d.Id,
                name = d.Name ?? string.Empty,
                documentType = d.DocumentType?.Name ?? string.Empty
            }).ToList() ?? (object)new List<object>(),
            
            // Email addresses
            ["emailAddresses"] = interaction.EmailAddresses ?? new List<string>()
        };

        return details;
    }
}
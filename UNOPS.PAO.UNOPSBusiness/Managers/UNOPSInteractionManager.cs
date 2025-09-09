namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System.Threading.Tasks;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
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

public class UNOPSInteractionManager : BaseUNOPSManager, IInteractionManager
{
    private readonly IMapper mapper;
    private readonly BaseRepository<UNOPSInteraction> interactionRepository;
    private readonly BaseRepository<UNOPSContact> contactRepository;
    private readonly BaseRepository<OrganizationHierarchy> OrganizationHierarchyRepository;
    private readonly UNOPSAppDbContext context;
    private GoogleCloudStorageService googleCloudStorageService;
    private readonly IUserProfileCacheService userProfileCacheService;

    private InteractionModel MapEntityToModel(UNOPSInteraction entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSInteraction, InteractionModel>(entity);
        
        // For sync version, provide default user identifiers to avoid N+1 queries
        // User names will be resolved by batch methods when possible
        if (entity.CreatedBy > 0)
        {
            result.CreatedByName = $"User #{entity.CreatedBy}";
        }
        
        if (entity.LastModifiedBy > 0)
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
        if (entity.CreatedBy > 0)
        {
            result.CreatedByName = await GetUserNameByIdAsync(entity.CreatedBy);
        }
        
        if (entity.LastModifiedBy > 0)
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
        if (entity.CreatedBy > 0 && userNames.TryGetValue(entity.CreatedBy, out var createdByName))
        {
            result.CreatedByName = createdByName;
        }
        else if (entity.CreatedBy > 0)
        {
            result.CreatedByName = $"User #{entity.CreatedBy}";
        }
        
        if (entity.LastModifiedBy > 0 && userNames.TryGetValue(entity.LastModifiedBy, out var lastModifiedByName))
        {
            result.LastModifiedByName = lastModifiedByName;
        }
        else if (entity.LastModifiedBy > 0)
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
            Name = contactId + " - " + model.Date
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

    public UNOPSInteractionManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, IPermissionService permissionService = null, IHttpContextAccessor httpContextAccessor = null, IServiceProvider serviceProvider = null, IUserProfileCacheService userProfileCacheService = null)
        : base(mapper, context, configuration, null, "Interaction", permissionService, httpContextAccessor)
    {
        this.mapper = mapper;
        this.context = context;
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

        foreach (var user in item.InteractionUsers)
        {
            retVal.UserIds.Add(user.UserId);
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

        // Update emails/phones
        entity.EmailAddresses = model.EmailAddresses?.ToList() ?? new List<string>();
        entity.PhoneNumbers = model.PhoneNumbers?.ToList() ?? new List<string>();

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
            await interactionRepository.Delete(entity);
        }
    }
    
    public async Task<PaginationResponse<InteractionModel>> GetContactInteractionsAsync(int contactId, PaginationRequest request)
    {
        var query = interactionRepository
            .GetAll(["InteractionContacts"])
            .Where(x => x.InteractionContacts.Any(ic => ic.ContactId == contactId) && !x.IsDeleted)
            .AsQueryable();

        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(context);

        // Apply access control filters
        var filteredData = await ApplyAccessControlFilters(query, GetCurrentUserOrSystemContext(), "read");
        
        // Filter to ensure we only have UNOPSInteraction instances and handle pagination manually
        var interactionArray = filteredData.OfType<UNOPSInteraction>().OrderByDescending(x => x.Date).ToArray();
        var totalCount = interactionArray.Length;
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        var pagedItems = interactionArray
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToArray();

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

        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(context);
        
        // Cast to base type to apply specification, then cast back to derived type
        var baseQuery = query.Cast<Interaction>();
        var filteredBaseQuery = baseQuery.ApplySpecification(specification);
        var filteredQuery = filteredBaseQuery.OfType<UNOPSInteraction>();

        // Apply org unit filtering if the specification supports it
        filteredQuery = ApplyOrgUnitFilterIfSupported(filteredQuery, specification);
        
        // Apply access control filters (row and column filtering) BEFORE pagination
        var filteredData = await ApplyAccessControlFilters(filteredQuery, GetCurrentUserOrSystemContext(), "read");
        
        // Filter to ensure we only have UNOPSInteraction instances and handle pagination manually
        var interactionArray = filteredData.OfType<UNOPSInteraction>().ToArray();
        var totalCount = interactionArray.Length;
        var pageIndex = pagination.PageIndex < 1 ? 1 : pagination.PageIndex;
        var excludedRows = (pageIndex - 1) * pagination.PageSize;
        
        var pagedItems = interactionArray
            .Skip(excludedRows)
            .Take(pagination.PageSize)
            .ToArray();

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
    /// </summary>
    public async Task<PaginationResponse<InteractionModel>> GetInteractionsAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        // RBAC interceptor handles security enforcement
        var query = interactionRepository
            .GetAll([
                "InteractionContacts", 
                "InteractionContacts.Contact", 
                "InteractionContacts.Contact.Partner",
                "InteractionPartners",
                "InteractionPartners.Partner",
                "InteractionUsers",
                "InteractionUsers.User",
                "InteractionUsers.User.UserProfile"
            ])
            .AsQueryable();

        // Load organization unit relationships
        await query.LoadOrganizationUnitRelationshipsAsync(context);

        // First get the paginated entities without mapping to avoid N+1 queries
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;

        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }

        // Get total count first
        var totalCount = await query.CountAsync();
        
        // Get paginated entities
        var pagedEntities = await query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToListAsync();

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
    /// </summary>
    public async Task<InteractionModel?> GetInteractionAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var item = await interactionRepository.GetByIdAsync(id, [
            "InteractionContacts", 
            "InteractionContacts.Contact", 
            "InteractionContacts.Contact.Partner",
            "InteractionPartners",
            "InteractionPartners.Partner", 
            "InteractionUsers",
            "InteractionUsers.User",
            "InteractionUsers.User.UserProfile"
        ]);
        if (item == null) return null;

        // Load organization unit relationships for single interaction
        await item.LoadOrganizationUnitRelationshipsAsync(context);

        return await MapEntityToModelAsync(item, mapper, user);
    }

    /// <summary>
    /// Updates an interaction with permission validation
    /// </summary>
    public async Task<InteractionModel?> UpdateInteractionAsync(ClaimsPrincipal user, UpdateInteractionRequest model)
    {
        // RBAC interceptor handles security enforcement
        var entity = await interactionRepository.GetByIdAsync(model.Id, [
            "InteractionContacts", 
            "InteractionContacts.Contact", 
            "InteractionContacts.Contact.Partner",
            "InteractionPartners",
            "InteractionPartners.Partner",
            "InteractionUsers",
            "InteractionUsers.User",
            "InteractionUsers.User.UserProfile"
        ]);
        if (entity == null)
        {
            throw new BusinessException($"Interaction {model.Id} does not exist.");
        }

        // Load organization unit relationships for single interaction
        await entity.LoadOrganizationUnitRelationshipsAsync(context);

        PatchNonNullProperties(model, entity);

        // Update emails/phones
        entity.EmailAddresses = model.EmailAddresses?.ToList() ?? new List<string>();
        entity.PhoneNumbers = model.PhoneNumbers?.ToList() ?? new List<string>();

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
    /// </summary>
    public async Task DeleteInteractionAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
        var entity = await interactionRepository.GetByIdAsync(id, [
            "InteractionContacts", 
            "InteractionContacts.Contact", 
            "InteractionContacts.Contact.Partner",
            "InteractionPartners",
            "InteractionPartners.Partner",
            "InteractionUsers",
            "InteractionUsers.User",
            "InteractionUsers.User.UserProfile"
        ]);
        if (entity == null) return;

        // Load organization unit relationships for single interaction
        await entity.LoadOrganizationUnitRelationshipsAsync(context);

        await interactionRepository.Delete(entity);
    }

    /// <summary>
    /// Gets comprehensive interaction details with security checks for AI prompts
    /// </summary>
    public async Task<InteractionModel?> GetInteractionDetailsAsync(ClaimsPrincipal user, int id)
    {
        // RBAC interceptor handles security enforcement
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

        if (item == null) return null;

        // Load organization unit relationships for single interaction
        await item.LoadOrganizationUnitRelationshipsAsync(context);

        var result = await MapEntityToModelAsync(item, mapper, user);
        
        // Populate junction table IDs
        if (item.InteractionContacts != null)
        {
            result.ContactIds = item.InteractionContacts.Select(ic => ic.ContactId).ToList();
        }

        if (item.InteractionPartners != null)
        {
            result.PartnerIds = item.InteractionPartners.Select(ip => ip.PartnerId).ToList();
        }

        if (item.InteractionUsers != null)
        {
            result.UserIds = item.InteractionUsers.Select(iu => iu.UserId).ToList();
        }

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

        if (item.InteractionUsers != null)
        {
            result.UserIds = item.InteractionUsers.Select(iu => iu.UserId).ToList();
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

    public virtual async Task<InteractionModel> FindGmailInteractionAsync(GmailInteractionRequest model)
    {
        var entity = await context.Interactions
                            .FirstOrDefaultAsync(x => x.GmailThreadId == model.GmailThreadId && x.GmailMessageId == model.GmailMessageId && !x.IsDeleted);

        return mapper.Map<InteractionModel>(entity);
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

    public virtual async Task<InteractionModel?> UpdateGmailInteractionAsync(UpdateInteractionRequest model)
    {
        var includes = new[]
        {
            "InteractionContacts",
            "InteractionContacts.Contact",
            "InteractionContacts.Contact.Partner",
            "InteractionPartners.Partner",
            "InteractionUsers.User",
            "Documents"
        };

        var query = context.Interactions.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        var entity = await query.FirstOrDefaultAsync(i => i.Id == model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Interaction {model.Id} does not exist.");
        }

        // Load organization unit relationships for single interaction
        await entity.LoadOrganizationUnitRelationshipsAsync(context);

        if (model.EmailAddresses != null && model.EmailAddresses.Count > 0)
        {
            model.EmailAddresses = model.EmailAddresses.Distinct().ToList();
        }

        entity = MapModelToEntity(model, entity);

        // Update emails/phones
        entity.EmailAddresses = model.EmailAddresses?.ToList() ?? new List<string>();
        entity.PhoneNumbers = model.PhoneNumbers?.ToList() ?? new List<string>();

        // Update junction tables
        await ProcessGmailInteractionJunctionTables(entity, model);

        context.Interactions.Update(entity);
        await context.SaveChangesAsync();

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

            var existingOrgUnitRelationships = await context.OrganizationUnitRelationships
                .Where(r => r.EntityId == interaction.Id && r.EntityType == "Interaction")
                .Select(r => r.OrganizationHierarchyId)
                .ToListAsync();

            var existingContactIds = existingContacts.ToHashSet();
            var existingUserIds = existingUsers.ToHashSet();
            var existingPartnerIds = existingPartners.ToHashSet();
            var existingOrgUnitIds = existingOrgUnitRelationships.ToHashSet();

            // Prepare bulk insert lists
            var contactsToAdd = new List<InteractionContact>();
            var partnersToAdd = new List<InteractionPartner>();
            var usersToAdd = new List<InteractionUser>();
            var orgUnitRelationshipsToAdd = new List<OrganizationUnitRelationship>();

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

            // Process OrganizationUnitRelationships from related partners - bulk prepare
            if (model.PartnerIds?.Any() == true)
            {
                // Get organization unit relationships for all related partners
                var partnerOrgUnitRelationships = await context.OrganizationUnitRelationships
                    .Where(r => model.PartnerIds.Contains(r.EntityId) && r.EntityType == "Partner")
                    .ToListAsync();

                if (partnerOrgUnitRelationships.Any())
                {
                    var uniqueOrgUnitIds = partnerOrgUnitRelationships
                        .Select(r => r.OrganizationHierarchyId)
                        .Distinct()
                        .Where(id => !existingOrgUnitIds.Contains(id))
                        .ToList();

                    if (uniqueOrgUnitIds.Any())
                    {
                        // Load organization units into context to ensure EF can track them
                        var orgUnits = await context.OrganizationHierarchies
                            .Where(ou => uniqueOrgUnitIds.Contains(ou.Id) && ou.Type == Domain.Enums.OrganizationUnitType.OrgUnit)
                            .ToListAsync();

                        foreach (var orgUnit in orgUnits)
                        {
                            orgUnitRelationshipsToAdd.Add(new OrganizationUnitRelationship
                            {
                                OrganizationHierarchyId = orgUnit.Id,
                                EntityId = interaction.Id,
                                EntityType = nameof(Interaction),
                                Name = $"Interaction-{interaction.Id}-{orgUnit.Code}",
                                Status = EntityStatus.Active
                            });
                        }
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

            if (orgUnitRelationshipsToAdd.Any())
            {
                await context.OrganizationUnitRelationships.AddRangeAsync(orgUnitRelationshipsToAdd);
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
    /// </summary>
    public override async Task<List<object>> GetByIdsAsync(int[] ids, ClaimsPrincipal user = null)
    {
        if (ids == null || ids.Length == 0)
            return new List<object>();

        var interactions = interactionRepository
            .GetAll([
                "InteractionContacts", 
                "InteractionContacts.Contact", 
                "InteractionContacts.Contact.Partner",
                "InteractionPartners",
                "InteractionPartners.Partner",
                "InteractionUsers",
                "InteractionUsers.User",
                "InteractionUsers.User.UserProfile"
            ])
            .Where(i => ids.Contains(i.Id))
            .ToList();

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
    /// Applies org unit filtering if the specification supports it using manual joins
    /// </summary>
    private IQueryable<UNOPSInteraction> ApplyOrgUnitFilterIfSupported(IQueryable<UNOPSInteraction> query, ISpecification<Interaction> specification)
    {
        // Check if specification has ApplyOrgUnitFilter method and call it
        var specType = specification.GetType();
        var filterMethod = specType.GetMethod("ApplyOrgUnitFilter", new[] { typeof(IQueryable<Interaction>), typeof(DbContext) });
        
        if (filterMethod != null)
        {
            try
            {
                // Cast to base type for the ApplyOrgUnitFilter method
                var baseQuery = query.Cast<Interaction>();
                var result = filterMethod.Invoke(specification, new object[] { baseQuery, context });
                if (result is IQueryable<Interaction> filteredBaseQuery)
                {
                    // Cast back to UNOPSInteraction using OfType for safety
                    return filteredBaseQuery.OfType<UNOPSInteraction>();
                }
            }
            catch (Exception ex)
            {
                // Log error but continue without org unit filtering
                Console.WriteLine($"Error applying org unit filter: {ex.Message}");
            }
        }
        
        // If no ApplyOrgUnitFilter method found, return original query
        return query;
    }
}
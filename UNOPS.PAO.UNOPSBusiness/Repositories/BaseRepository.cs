using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Utilities.Helpers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace UNOPS.PAO.UNOPSBusiness.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Microsoft.Extensions.Configuration;
using Humanizer;
using Newtonsoft.Json;

public class BaseRepository<TEntity>  where TEntity : class, IBaseBusinessEntity<int>
{
    protected readonly UNOPSAppDbContext _dataDbContext;
    protected DbSet<TEntity> _dbSet;
    protected readonly IConfiguration _configuration;
    protected readonly AiContextualService _aiService;
    private readonly IServiceProvider? _serviceProvider;
    private readonly GlobalFilterService? _globalFilterService;

    private IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> set, string[] includes)
    {
        return includes.Aggregate(set, (current, include) => current.Include(include));
    }

    public BaseRepository(UNOPSAppDbContext context, IConfiguration configuration, IServiceProvider? serviceProvider = null)
    {
        _dataDbContext = context;
        _dbSet = context.Set<TEntity>();
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _aiService = new AiContextualService(configuration, context, null!);
        _globalFilterService = _serviceProvider?.GetService<GlobalFilterService>();
    }

    /// <summary>
    /// Gets all descendant organization unit IDs for a given organization unit ID
    /// </summary>
    private async Task<List<int>> GetDescendantOrgUnitIdsAsync(int orgUnitId)
    {
        var allOrgUnits = await _dataDbContext.OrganizationHierarchies
            .Where(x => !x.IsDeleted && x.Status == EntityStatus.Active)
            .ToListAsync();

        var descendantIds = new List<int> { orgUnitId };
        var queue = new Queue<int>();
        queue.Enqueue(orgUnitId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = allOrgUnits.Where(x => x.ParentId == currentId).ToList();
            
            foreach (var child in children)
            {
                if (!descendantIds.Contains(child.Id))
                {
                    descendantIds.Add(child.Id);
                    queue.Enqueue(child.Id);
                }
            }
        }

        return descendantIds;
    }

    /// <summary>
    /// Gets all descendant organization unit IDs for a given organization unit ID (Synchronous version)
    /// </summary>
    private List<int> GetDescendantOrgUnitIds(int orgUnitId)
    {
        var allOrgUnits = _dataDbContext.OrganizationHierarchies
            .Where(x => !x.IsDeleted && x.Status == EntityStatus.Active)
            .ToList();

        var descendantIds = new List<int> { orgUnitId };
        var queue = new Queue<int>();
        queue.Enqueue(orgUnitId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = allOrgUnits.Where(x => x.ParentId == currentId).ToList();
            
            foreach (var child in children)
            {
                if (!descendantIds.Contains(child.Id))
                {
                    descendantIds.Add(child.Id);
                    queue.Enqueue(child.Id);
                }
            }
        }

        return descendantIds;
    }

    /// <summary>
    /// Gets the current user's ID from the HTTP context
    /// </summary>
    private string? GetCurrentUserId()
    {
        if (_serviceProvider == null)
            return null;
            
        var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
        return httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Gets the current user's integer ID from the HTTP context
    /// </summary>
    private int? GetCurrentUserIdAsInt()
    {
        var userIdString = GetCurrentUserId();
        if (string.IsNullOrEmpty(userIdString))
            return null;

        if (int.TryParse(userIdString, out int userId))
            return userId;

        // If it's not a direct integer, we can't find the user in PAOUsers table since it uses int IDs
        // This means the user authentication is using string IDs but our PAOUser table uses int IDs
        // Return null in this case
        return null;
    }

    /// <summary>
    /// Gets the Id property safely, handling ambiguous matches in inheritance hierarchies
    /// </summary>
    private PropertyInfo GetIdProperty(Type entityType)
    {
        try
        {
            // First try without DeclaredOnly to include inherited properties
            var idProperty = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (idProperty != null)
            {
                System.Diagnostics.Debug.WriteLine($"BaseRepository: Found Id property for {entityType.Name} of type {idProperty.PropertyType.Name} declared in {idProperty.DeclaringType.Name}");
            }
            return idProperty;
        }
        catch (AmbiguousMatchException)
        {
            System.Diagnostics.Debug.WriteLine($"BaseRepository: Ambiguous Id property found for {entityType.Name}, resolving...");
            
            // If ambiguous, get all properties named "Id" and pick the most specific int one
            var idProperties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name == "Id" && p.PropertyType == typeof(int))
                .ToArray();
            
            if (idProperties.Length > 0)
            {
                // Prefer properties declared in the current type over inherited ones
                var declaredProperty = idProperties.FirstOrDefault(p => p.DeclaringType == entityType);
                if (declaredProperty != null)
                {
                    System.Diagnostics.Debug.WriteLine($"BaseRepository: Using declared Id property from {declaredProperty.DeclaringType.Name}");
                    return declaredProperty;
                }
                
                // Otherwise, use the first one found
                System.Diagnostics.Debug.WriteLine($"BaseRepository: Using first Id property from {idProperties[0].DeclaringType.Name}");
                return idProperties[0];
            }
            
            System.Diagnostics.Debug.WriteLine($"BaseRepository: No int Id property found for {entityType.Name}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BaseRepository: Error getting Id property for {entityType.Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the correct entity type name for OrganizationUnitRelationship queries
    /// Handles inheritance (e.g., UNOPSPartner -> Partner)
    /// </summary>
    private string GetEntityTypeNameForRelationship(Type entityType)
    {
        // Handle UNOPS inheritance - they are stored with their base type names
        if (entityType == typeof(UNOPSPartner))
            return "Partner";
        if (entityType == typeof(UNOPSContact))
            return "Contact";
        if (entityType == typeof(UNOPSInteraction))
            return "Interaction";
        
        // For all other types, use the actual type name
        return entityType.Name;
    }

    /// <summary>
    /// Smart organization unit filter that handles Partner, Contact, and Interaction with specific logic
    /// 1. Partner: Direct lookup in OrganizationUnitRelationship table
    /// 2. Contact: Get partners in org unit, then filter contacts by those partners
    /// 3. Interaction: Direct lookup + contacts with partners in org unit
    /// </summary>
    private async Task<IQueryable<TEntity>> ApplySmartOrgUnitFilterAsync(IQueryable<TEntity> queryable, int orgUnitId, Type entityType)
    {
        // Check if the organization unit has code "OPS" - if so, return original queryable (no filtering)
        var orgUnit = await _dataDbContext.OrganizationHierarchies
            .FirstOrDefaultAsync(x => x.Id == orgUnitId && !x.IsDeleted && x.Status == EntityStatus.Active);
        
        if (orgUnit != null && orgUnit.Code == "OPS")
        {
            System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Organization unit {orgUnitId} has code 'OPS' - skipping org unit filter for {entityType.Name}");
            return queryable; // Return original queryable without filtering
        }

        var orgUnitIds = await GetDescendantOrgUnitIdsAsync(orgUnitId);
        System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Smart org unit filter for {entityType.Name} with {orgUnitIds.Count} org units");

        var entityTypeName = GetEntityTypeNameForRelationship(entityType);

        if (entityType == typeof(Partner) || entityType == typeof(UNOPSPartner))
        {
            // 1. Partner: Simple direct lookup in OrganizationUnitRelationship table
            return await ApplyDirectOrgUnitFilterAsync(queryable, orgUnitIds, "Partner", entityType);
        }
        else if (entityType == typeof(Contact) || entityType == typeof(UNOPSContact))
        {
            // 2. Contact: Get partners in org unit, then filter contacts by those partners
            return await ApplyContactOrgUnitFilterAsync(queryable, orgUnitIds, entityType);
        }
        else if (entityType == typeof(Interaction) || entityType == typeof(UNOPSInteraction))
        {
            // 3. Interaction: Direct lookup + contacts with partners in org unit
            return await ApplyInteractionOrgUnitFilterAsync(queryable, orgUnitIds, entityType);
        }
        else
        {
            // For other entity types (like Engagement), try direct lookup first
            return await ApplyDirectOrgUnitFilterAsync(queryable, orgUnitIds, entityTypeName, entityType);
        }
    }

    /// <summary>
    /// Applies direct organization unit filtering by looking up entity IDs in OrganizationUnitRelationship table
    /// </summary>
    private async Task<IQueryable<TEntity>> ApplyDirectOrgUnitFilterAsync(IQueryable<TEntity> queryable, List<int> orgUnitIds, string entityTypeName, Type entityType)
    {
        var validEntityIds = await _dataDbContext.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == entityTypeName && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToListAsync();

        System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Direct filter found {validEntityIds.Count} {entityTypeName} IDs");

        if (validEntityIds.Any())
        {
            var parameter = Expression.Parameter(entityType, "e");
            var idProperty = GetIdProperty(entityType);
            if (idProperty != null)
            {
                var idAccess = Expression.Property(parameter, idProperty);
                var idsConstant = Expression.Constant(validEntityIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                var containsCall = Expression.Call(idsConstant, containsMethod, idAccess);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameter);
                return queryable.Where(lambda);
            }
            else
            {
                // If no ID property found, we can't filter properly, so return empty result
                System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: No ID property found for {entityType.Name} - returning empty result for org unit filter");
                return queryable.Where(e => false); // Returns empty result
            }
        }
        else
        {
            // If no valid entity IDs found in org unit, return empty result
            System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: No entities found in organization unit for {entityTypeName} - returning empty result");
            return queryable.Where(e => false); // Returns empty result
        }
    }

    /// <summary>
    /// Applies organization unit filtering for Contact entities by finding partners in org unit first
    /// </summary>
    private async Task<IQueryable<TEntity>> ApplyContactOrgUnitFilterAsync(IQueryable<TEntity> queryable, List<int> orgUnitIds, Type entityType)
    {
        // Get all partners that belong to the specified organization units
                var validPartnerIds = await _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();
                
        System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Contact filter found {validPartnerIds.Count} partner IDs in org units");

        if (validPartnerIds.Any())
        {
            // Filter contacts by PartnerId
            var parameter = Expression.Parameter(entityType, "e");
            var partnerIdProperty = entityType.GetProperty("PartnerId");
            
            if (partnerIdProperty != null)
            {
                var partnerIdAccess = Expression.Property(parameter, partnerIdProperty);
                var partnerIdsConstant = Expression.Constant(validPartnerIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                var containsCall = Expression.Call(partnerIdsConstant, containsMethod, partnerIdAccess);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameter);
                return queryable.Where(lambda);
            }
        }

        return queryable;
    }

    /// <summary>
    /// Applies organization unit filtering for Interaction entities using both direct and partner-based filtering
    /// </summary>
    private async Task<IQueryable<TEntity>> ApplyInteractionOrgUnitFilterAsync(IQueryable<TEntity> queryable, List<int> orgUnitIds, Type entityType)
    {
        // Get direct interaction IDs from OrganizationUnitRelationship table
        var validInteractionIds = await _dataDbContext.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Interaction" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToListAsync();

        // Get partner IDs from OrganizationUnitRelationship table
                var validPartnerIds = await _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();
                
        System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Interaction filter found {validInteractionIds.Count} interaction IDs + {validPartnerIds.Count} partner IDs");

        if (validInteractionIds.Any() || validPartnerIds.Any())
        {
            var parameter = Expression.Parameter(entityType, "e");
            var idProperty = GetIdProperty(entityType);
            Expression combinedFilter = null;

            // Direct interaction filter
            if (validInteractionIds.Any() && idProperty != null)
            {
                var idAccess = Expression.Property(parameter, idProperty);
                var interactionIdsConstant = Expression.Constant(validInteractionIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                var directFilter = Expression.Call(interactionIdsConstant, containsMethod, idAccess);
                combinedFilter = directFilter;
            }

            // Partner-based filter (interactions with contacts that have partners in org unit)
            if (validPartnerIds.Any())
            {
                // This requires navigation properties - check if Interaction has InteractionContacts
                var interactionContactsProperty = entityType.GetProperty("InteractionContacts");
                if (interactionContactsProperty != null)
                {
                    // Create complex expression: i.InteractionContacts.Any(ic => ic.Contact.PartnerId in validPartnerIds)
                    var interactionContactsAccess = Expression.Property(parameter, interactionContactsProperty);
                    
                    // For simplicity, let's use a more direct approach with LINQ
                    // This might need to be adjusted based on your actual navigation structure
                    System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Interaction navigation filtering not implemented - using direct IDs only");
                }
            }

            if (combinedFilter != null)
            {
                var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedFilter, parameter);
                return queryable.Where(lambda);
            }
            else
            {
                // If we have valid IDs but can't create filter (no ID property), return empty result
                System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: No valid filter could be created for {entityType.Name} - returning empty result");
                return queryable.Where(e => false); // Returns empty result
            }
        }
        else
        {
            // If no valid interaction or partner IDs found in org unit, return empty result
            System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: No interactions or partners found in organization unit for {entityType.Name} - returning empty result");
            return queryable.Where(e => false); // Returns empty result
        }
    }

    /// <summary>
    /// Handles organization unit filtering for Contact entities through Partner relationships
    /// </summary>
    private async Task<IQueryable<TEntity>> HandleContactOrgUnitFilter(IQueryable<TEntity> queryable, List<int> orgUnitIds, Type entityType)
    {
        System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Applying Partner-based org unit filter for {entityType.Name}");
        
                var validPartnerIds = await _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();
                
        if (validPartnerIds.Any())
        {
            System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Found {validPartnerIds.Count} valid Partner IDs for Contact filtering");
            var parameter = Expression.Parameter(entityType, "e");
            var partnerProperty = entityType.GetProperty("PartnerId") ?? entityType.GetProperty("Partner");
            
            if (partnerProperty != null)
            {
                Expression filterExpression;
                if (partnerProperty.PropertyType == typeof(int) || partnerProperty.PropertyType == typeof(int?))
                {
                    // PartnerId property
                    var partnerIdAccess = Expression.Property(parameter, partnerProperty);
                    var partnerIdsConstant = Expression.Constant(validPartnerIds);
                    var containsMethod = typeof(List<int>).GetMethod("Contains");
                    
                    if (partnerProperty.PropertyType == typeof(int?))
                    {
                        var hasValue = Expression.Property(partnerIdAccess, "HasValue");
                        var value = Expression.Property(partnerIdAccess, "Value");
                        var partnerIdInList = Expression.Call(partnerIdsConstant, containsMethod, value);
                        filterExpression = Expression.AndAlso(hasValue, partnerIdInList);
                    }
                    else
                    {
                        filterExpression = Expression.Call(partnerIdsConstant, containsMethod, partnerIdAccess);
                    }
                }
                else
                {
                    // Partner navigation property
                    var partnerAccess = Expression.Property(parameter, partnerProperty);
                    var partnerIdAccess = Expression.Property(partnerAccess, "Id");
                    var partnerIdsConstant = Expression.Constant(validPartnerIds);
                    var containsMethod = typeof(List<int>).GetMethod("Contains");
                    var partnerNotNull = Expression.NotEqual(partnerAccess, Expression.Constant(null));
                    var partnerIdInList = Expression.Call(partnerIdsConstant, containsMethod, partnerIdAccess);
                    filterExpression = Expression.AndAlso(partnerNotNull, partnerIdInList);
                }
                
                var lambda = Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
                queryable = queryable.Where(lambda);
                System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Applied Partner-based organization unit filter for {entityType.Name}");
            }
        }
        
        return queryable;
    }

    /// <summary>
    /// Handles organization unit filtering for Engagement entities through Partner relationships
    /// </summary>
    private async Task<IQueryable<TEntity>> HandleEngagementOrgUnitFilter(IQueryable<TEntity> queryable, List<int> orgUnitIds, Type entityType)
    {
        System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Applying Partner-based org unit filter for Engagement");
        
        var validPartnerIds = await _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                orgRel.EntityType == "Partner" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();
                
        if (validPartnerIds.Any())
        {
            var parameter = Expression.Parameter(entityType, "e");
            var partnerIdProperty = entityType.GetProperty("PartnerId");
            if (partnerIdProperty != null)
            {
                var partnerIdAccess = Expression.Property(parameter, partnerIdProperty);
                var partnerIdsConstant = Expression.Constant(validPartnerIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                
                // Handle nullable PartnerId
                Expression filterExpression;
                if (partnerIdProperty.PropertyType == typeof(int?))
                {
                    var hasValue = Expression.Property(partnerIdAccess, "HasValue");
                    var value = Expression.Property(partnerIdAccess, "Value");
                    var partnerIdInList = Expression.Call(partnerIdsConstant, containsMethod, value);
                    filterExpression = Expression.AndAlso(hasValue, partnerIdInList);
            }
            else
            {
                    filterExpression = Expression.Call(partnerIdsConstant, containsMethod, partnerIdAccess);
                }
                
                var lambda = Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
                queryable = queryable.Where(lambda);
                System.Diagnostics.Debug.WriteLine($"BaseRepository ASYNC: Applied Partner-based organization unit filter for Engagement");
            }
        }
        
        return queryable;
    }

    /// <summary>
    /// Smart organization unit filter (Sync version)
    /// </summary>
    private IQueryable<TEntity> ApplySmartOrgUnitFilterSync(IQueryable<TEntity> queryable, int orgUnitId, Type entityType)
    {
        // Check if the organization unit has code "OPS" - if so, return original queryable (no filtering)
        var orgUnit = _dataDbContext.OrganizationHierarchies
            .FirstOrDefault(x => x.Id == orgUnitId && !x.IsDeleted && x.Status == EntityStatus.Active);
        
        if (orgUnit != null && orgUnit.Code == "OPS")
        {
            System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: Organization unit {orgUnitId} has code 'OPS' - skipping org unit filter for {entityType.Name}");
            return queryable; // Return original queryable without filtering
        }

        var orgUnitIds = GetDescendantOrgUnitIds(orgUnitId);
        System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: Smart org unit filter for {entityType.Name} with {orgUnitIds.Count} org units");

        if (entityType == typeof(Partner) || entityType == typeof(UNOPSPartner))
        {
            // 1. Partner: Simple direct lookup in OrganizationUnitRelationship table
            return ApplyDirectOrgUnitFilterSync(queryable, orgUnitIds, "Partner", entityType);
        }
        else if (entityType == typeof(Contact) || entityType == typeof(UNOPSContact))
        {
            // 2. Contact: Get partners in org unit, then filter contacts by those partners
            return ApplyContactOrgUnitFilterSync(queryable, orgUnitIds, entityType);
        }
        else if (entityType == typeof(Interaction) || entityType == typeof(UNOPSInteraction))
        {
            // 3. Interaction: Direct lookup + contacts with partners in org unit
            return ApplyInteractionOrgUnitFilterSync(queryable, orgUnitIds, entityType);
        }
        else
        {
            // For other entity types (like Engagement), try direct lookup first
            var entityTypeName = GetEntityTypeNameForRelationship(entityType);
            return ApplyDirectOrgUnitFilterSync(queryable, orgUnitIds, entityTypeName, entityType);
        }
    }

    /// <summary>
    /// Applies direct organization unit filtering (Sync version)
    /// </summary>
    private IQueryable<TEntity> ApplyDirectOrgUnitFilterSync(IQueryable<TEntity> queryable, List<int> orgUnitIds, string entityTypeName, Type entityType)
    {
        var validEntityIds = _dataDbContext.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == entityTypeName && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: Direct filter found {validEntityIds.Count} {entityTypeName} IDs");

        if (validEntityIds.Any())
        {
            var parameter = Expression.Parameter(entityType, "e");
            var idProperty = GetIdProperty(entityType);
            if (idProperty != null)
            {
                var idAccess = Expression.Property(parameter, idProperty);
                var idsConstant = Expression.Constant(validEntityIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                var containsCall = Expression.Call(idsConstant, containsMethod, idAccess);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameter);
                return queryable.Where(lambda);
            }
            else
            {
                // If no ID property found, we can't filter properly, so return empty result
                System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: No ID property found for {entityType.Name} - returning empty result for org unit filter");
                return queryable.Where(e => false); // Returns empty result
            }
        }
        else
        {
            // If no valid entity IDs found in org unit, return empty result
            System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: No entities found in organization unit for {entityTypeName} - returning empty result");
            return queryable.Where(e => false); // Returns empty result
        }
    }

    /// <summary>
    /// Applies organization unit filtering for Contact entities (Sync version)
    /// </summary>
    private IQueryable<TEntity> ApplyContactOrgUnitFilterSync(IQueryable<TEntity> queryable, List<int> orgUnitIds, Type entityType)
    {
        // Get all partners that belong to the specified organization units
        var validPartnerIds = _dataDbContext.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Partner" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: Contact filter found {validPartnerIds.Count} partner IDs in org units");

        if (validPartnerIds.Any())
        {
            // Filter contacts by PartnerId
            var parameter = Expression.Parameter(entityType, "e");
            var partnerIdProperty = entityType.GetProperty("PartnerId");
            
            if (partnerIdProperty != null)
            {
                var partnerIdAccess = Expression.Property(parameter, partnerIdProperty);
                var partnerIdsConstant = Expression.Constant(validPartnerIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                var containsCall = Expression.Call(partnerIdsConstant, containsMethod, partnerIdAccess);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameter);
                return queryable.Where(lambda);
            }
        }

        return queryable;
    }

    /// <summary>
    /// Applies organization unit filtering for Interaction entities (Sync version)
    /// </summary>
    private IQueryable<TEntity> ApplyInteractionOrgUnitFilterSync(IQueryable<TEntity> queryable, List<int> orgUnitIds, Type entityType)
    {
        // Get direct interaction IDs from OrganizationUnitRelationship table
        var validInteractionIds = _dataDbContext.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Interaction" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: Interaction filter found {validInteractionIds.Count} interaction IDs");

        if (validInteractionIds.Any())
        {
            var parameter = Expression.Parameter(entityType, "e");
            var idProperty = GetIdProperty(entityType);
            if (idProperty != null)
            {
                var idAccess = Expression.Property(parameter, idProperty);
                var interactionIdsConstant = Expression.Constant(validInteractionIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                var directFilter = Expression.Call(interactionIdsConstant, containsMethod, idAccess);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(directFilter, parameter);
                return queryable.Where(lambda);
            }
            else
            {
                // If no ID property found, we can't filter properly, so return empty result
                System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: No ID property found for {entityType.Name} - returning empty result for interaction filter");
                return queryable.Where(e => false); // Returns empty result
            }
        }
        else
        {
            // If no valid interaction IDs found in org unit, return empty result
            System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: No interactions found in organization unit for {entityType.Name} - returning empty result");
            return queryable.Where(e => false); // Returns empty result
        }
    }

    /// <summary>
    /// Handles organization unit filtering for Contact entities through Partner relationships (Sync version)
    /// </summary>
    private IQueryable<TEntity> HandleContactOrgUnitFilterSync(IQueryable<TEntity> queryable, List<int> orgUnitIds, Type entityType)
    {
        System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: Applying Partner-based org unit filter for {entityType.Name}");
        
        var validPartnerIds = _dataDbContext.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Partner" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToList();
        
        if (validPartnerIds.Any())
        {
            System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: Found {validPartnerIds.Count} valid Partner IDs for Contact filtering");
            var parameter = Expression.Parameter(entityType, "e");
            var partnerProperty = entityType.GetProperty("PartnerId") ?? entityType.GetProperty("Partner");
            
            if (partnerProperty != null)
            {
                Expression filterExpression;
                if (partnerProperty.PropertyType == typeof(int) || partnerProperty.PropertyType == typeof(int?))
                {
                    // PartnerId property
                    var partnerIdAccess = Expression.Property(parameter, partnerProperty);
                    var partnerIdsConstant = Expression.Constant(validPartnerIds);
                    var containsMethod = typeof(List<int>).GetMethod("Contains");
                    
                    if (partnerProperty.PropertyType == typeof(int?))
                    {
                        var hasValue = Expression.Property(partnerIdAccess, "HasValue");
                        var value = Expression.Property(partnerIdAccess, "Value");
                        var partnerIdInList = Expression.Call(partnerIdsConstant, containsMethod, value);
                        filterExpression = Expression.AndAlso(hasValue, partnerIdInList);
                    }
                    else
                    {
                        filterExpression = Expression.Call(partnerIdsConstant, containsMethod, partnerIdAccess);
                    }
                }
                else
                {
                    // Partner navigation property
                    var partnerAccess = Expression.Property(parameter, partnerProperty);
                    var partnerIdAccess = Expression.Property(partnerAccess, "Id");
                    var partnerIdsConstant = Expression.Constant(validPartnerIds);
                    var containsMethod = typeof(List<int>).GetMethod("Contains");
                    var partnerNotNull = Expression.NotEqual(partnerAccess, Expression.Constant(null));
                    var partnerIdInList = Expression.Call(partnerIdsConstant, containsMethod, partnerIdAccess);
                    filterExpression = Expression.AndAlso(partnerNotNull, partnerIdInList);
                }
                
                var lambda = Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
                    queryable = queryable.Where(lambda);
                System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: Applied Partner-based organization unit filter for {entityType.Name}");
            }
        }
        
        return queryable;
    }

    /// <summary>
    /// Handles organization unit filtering for Engagement entities through Partner relationships (Sync version)
    /// </summary>
    private IQueryable<TEntity> HandleEngagementOrgUnitFilterSync(IQueryable<TEntity> queryable, List<int> orgUnitIds, Type entityType)
    {
        System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: Applying Partner-based org unit filter for Engagement");
        
        var validPartnerIds = _dataDbContext.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Partner" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToList();
        
        if (validPartnerIds.Any())
        {
            var parameter = Expression.Parameter(entityType, "e");
            var partnerIdProperty = entityType.GetProperty("PartnerId");
            if (partnerIdProperty != null)
            {
                var partnerIdAccess = Expression.Property(parameter, partnerIdProperty);
                var partnerIdsConstant = Expression.Constant(validPartnerIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                
                // Handle nullable PartnerId
                Expression filterExpression;
                if (partnerIdProperty.PropertyType == typeof(int?))
                {
                    var hasValue = Expression.Property(partnerIdAccess, "HasValue");
                    var value = Expression.Property(partnerIdAccess, "Value");
                    var partnerIdInList = Expression.Call(partnerIdsConstant, containsMethod, value);
                    filterExpression = Expression.AndAlso(hasValue, partnerIdInList);
                }
                else
                {
                    filterExpression = Expression.Call(partnerIdsConstant, containsMethod, partnerIdAccess);
                }
                
                var lambda = Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
                queryable = queryable.Where(lambda);
                System.Diagnostics.Debug.WriteLine($"BaseRepository SYNC: Applied Partner-based organization unit filter for Engagement");
            }
        }
        
        return queryable;
    }

    /// <summary>
    /// Applies global filters to a queryable using the centralized GlobalFilterService
    /// </summary>
    protected async Task<IQueryable<TEntity>> ApplyGlobalFiltersAsync(IQueryable<TEntity> queryable)
    {
        // Use centralized GlobalFilterService if available
        if (_globalFilterService != null)
        {
            var user = GetCurrentClaimsPrincipal();
            return await _globalFilterService.ApplyGlobalFiltersAsync(queryable, user);
        }
        
        // Fallback: return queryable unchanged if service is not available
        return queryable;
    }

    /// <summary>
    /// Applies global filters to a queryable based on user preferences (Synchronous version)
    /// </summary>
    protected IQueryable<TEntity> ApplyGlobalFilters(IQueryable<TEntity> queryable)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
            return queryable;

        // Check if service provider is available before attempting to resolve services
        if (_serviceProvider == null)
            return queryable;

        var userPreferenceService = _serviceProvider.GetService<IUserPreferenceService>();
        if (userPreferenceService == null)
            return queryable;

        // Get global filters synchronously by querying database directly
        GlobalFilters? globalFilters = null;
        if (int.TryParse(currentUserId, out int userIdInt))
        {
            var userPreferences = _dataDbContext.UserPreferences
                .FirstOrDefault(up => up.UserId == userIdInt);
            globalFilters = userPreferences?.GlobalFilters;
        }
        
        if (globalFilters == null)
            return queryable;

        var entityType = typeof(TEntity);

        // Apply organization unit filter
        if (globalFilters.OrgUnitId.HasValue)
        {
            queryable = ApplySmartOrgUnitFilterSync(queryable, globalFilters.OrgUnitId.Value, entityType);
        }

        /*if (globalFilters.OrgUnitId.HasValue)
        {
            var orgUnitIds = GetDescendantOrgUnitIds(globalFilters.OrgUnitId.Value);
            
            if (entityType == typeof(Partner))
            {
                // Pre-materialize the partner IDs that match the org unit criteria to avoid nested query issues
                var validPartnerIds = _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToList();
                
                var partnerQuery = queryable as IQueryable<Partner>;
                queryable = partnerQuery.Where(p => validPartnerIds.Contains(p.Id)) as IQueryable<TEntity>;
            }
            else if (entityType == typeof(Contact))
            {
                // Pre-materialize the partner IDs that match the org unit criteria
                var validPartnerIds = _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToList();
                
                var contactQuery = queryable as IQueryable<Contact>;
                queryable = contactQuery.Where(c => c.Partner != null && validPartnerIds.Contains(c.Partner.Id)) as IQueryable<TEntity>;
            }
            else if (entityType == typeof(Interaction))
            {
                // Pre-materialize the partner IDs that match the org unit criteria
                var validPartnerIds = _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToList();
                
                // Pre-materialize the interaction IDs that match the org unit criteria
                var validInteractionIds = _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Interaction" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToList();
                
                var interactionQuery = queryable as IQueryable<Interaction>;
                queryable = interactionQuery.Where(i => 
                    validInteractionIds.Contains(i.Id) ||
                    (i.InteractionContacts != null && i.InteractionContacts.Any(ic => ic.Contact != null && ic.Contact.Partner != null && validPartnerIds.Contains(ic.Contact.Partner.Id))) ||
                    (i.InteractionPartners != null && i.InteractionPartners.Any(ip => ip.Partner != null && validPartnerIds.Contains(ip.Partner.Id)))
                ) as IQueryable<TEntity>;
            }
            else
            {
                // For other entities, try to find OrgUnitId property using reflection
                var orgUnitIdProperty = entityType.GetProperty("OrgUnitId");
                if (orgUnitIdProperty != null && orgUnitIdProperty.PropertyType == typeof(int?))
                {
                    var parameter = Expression.Parameter(entityType, "x");
                    var property = Expression.Property(parameter, orgUnitIdProperty);
                    var hasValue = Expression.Property(property, "HasValue");
                    var value = Expression.Property(property, "Value");
                    
                    var orgUnitIdsConstant = Expression.Constant(orgUnitIds);
                    var containsMethod = typeof(List<int>).GetMethod("Contains", new[] { typeof(int) });
                    var containsCall = Expression.Call(orgUnitIdsConstant, containsMethod, value);
                    
                    var condition = Expression.AndAlso(hasValue, containsCall);
                    var lambda = Expression.Lambda<Func<TEntity, bool>>(condition, parameter);
                    
                    queryable = queryable.Where(lambda);
                }
            }
        }*/

        // Apply user-based filters
        var currentUserIdAsInt = GetCurrentUserIdAsInt();
        if (currentUserIdAsInt.HasValue && globalFilters.RelatedToMe)
        {
            // RelatedToMe filter: check both CreatedBy AND LastModifiedBy
            var createdByProperty = entityType.GetProperty("CreatedBy");
            var lastModifiedByProperty = entityType.GetProperty("LastModifiedBy");
            
            Expression? combinedUserExpression = null;
            var parameter = Expression.Parameter(entityType, "x");
            
            // Check CreatedBy
            if (createdByProperty != null && (createdByProperty.PropertyType == typeof(int) || createdByProperty.PropertyType == typeof(int?)))
            {
                var createdByPropertyAccess = Expression.Property(parameter, createdByProperty);
                var createdByConstant = Expression.Constant(currentUserIdAsInt.Value, createdByProperty.PropertyType);
                var createdByEquals = Expression.Equal(createdByPropertyAccess, createdByConstant);
                combinedUserExpression = createdByEquals;
            }
            
            // Check LastModifiedBy
            if (lastModifiedByProperty != null && (lastModifiedByProperty.PropertyType == typeof(int) || lastModifiedByProperty.PropertyType == typeof(int?)))
            {
                var lastModifiedByPropertyAccess = Expression.Property(parameter, lastModifiedByProperty);
                var lastModifiedByConstant = Expression.Constant(currentUserIdAsInt.Value, lastModifiedByProperty.PropertyType);
                var lastModifiedByEquals = Expression.Equal(lastModifiedByPropertyAccess, lastModifiedByConstant);
                
                if (combinedUserExpression != null)
                {
                    // Combine with OR: (CreatedBy == userId) OR (LastModifiedBy == userId)
                    combinedUserExpression = Expression.OrElse(combinedUserExpression, lastModifiedByEquals);
                }
                else
                {
                    combinedUserExpression = lastModifiedByEquals;
                }
            }
            
            // Apply the combined user filter
            if (combinedUserExpression != null)
            {
                var userLambda = Expression.Lambda<Func<TEntity, bool>>(combinedUserExpression, parameter);
                queryable = queryable.Where(userLambda);
            }
        }

        // Apply date filters (applies to both CreatedDate AND LastModified)
        // Single date mode - prioritize single date over range
        if (globalFilters.DateOn.HasValue)
        {
            // Single date mode - filter for this specific date on both CreatedDate and LastModified
            var startOfDay = globalFilters.DateOn.Value.Date;
            var endOfDay = startOfDay.AddDays(1);
            
            var parameter = Expression.Parameter(entityType, "x");
            Expression? combinedDateExpression = null;
            
            // Check CreatedDate
            var createdDateProperty = entityType.GetProperty("CreatedDate");
            if (createdDateProperty != null && createdDateProperty.PropertyType == typeof(DateTime))
            {
                var createdDatePropertyAccess = Expression.Property(parameter, createdDateProperty);
                var startConstant = Expression.Constant(startOfDay);
                var endConstant = Expression.Constant(endOfDay);
                
                var createdGreaterThanOrEqual = Expression.GreaterThanOrEqual(createdDatePropertyAccess, startConstant);
                var createdLessThan = Expression.LessThan(createdDatePropertyAccess, endConstant);
                var createdDateCondition = Expression.AndAlso(createdGreaterThanOrEqual, createdLessThan);
                
                combinedDateExpression = createdDateCondition;
            }
            
            // Check LastModified
            var lastModifiedDateProperty = entityType.GetProperty("LastModified");
            if (lastModifiedDateProperty != null && lastModifiedDateProperty.PropertyType == typeof(DateTime?))
            {
                var lastModifiedDatePropertyAccess = Expression.Property(parameter, lastModifiedDateProperty);
                var startConstant = Expression.Constant(startOfDay, typeof(DateTime?));
                var endConstant = Expression.Constant(endOfDay, typeof(DateTime?));
                
                var lastModifiedGreaterThanOrEqual = Expression.GreaterThanOrEqual(lastModifiedDatePropertyAccess, startConstant);
                var lastModifiedLessThan = Expression.LessThan(lastModifiedDatePropertyAccess, endConstant);
                var lastModifiedDateCondition = Expression.AndAlso(lastModifiedGreaterThanOrEqual, lastModifiedLessThan);
                
                if (combinedDateExpression != null)
                {
                    // Combine with OR: (CreatedDate in range) OR (LastModified in range)
                    combinedDateExpression = Expression.OrElse(combinedDateExpression, lastModifiedDateCondition);
                }
                else
                {
                    combinedDateExpression = lastModifiedDateCondition;
                }
            }
            
            // Apply the combined date filter
            if (combinedDateExpression != null)
            {
                var dateLambda = Expression.Lambda<Func<TEntity, bool>>(combinedDateExpression, parameter);
                queryable = queryable.Where(dateLambda);
            }
        }
        else
        {
            // Range mode - use DateFrom and DateTo if available (applies to both CreatedDate and LastModified)
            var parameter = Expression.Parameter(entityType, "x");
            Expression? combinedRangeExpression = null;
            
            if (globalFilters.DateFrom.HasValue || globalFilters.DateTo.HasValue)
            {
                // Check CreatedDate
                var createdDateProperty = entityType.GetProperty("CreatedDate");
                if (createdDateProperty != null && createdDateProperty.PropertyType == typeof(DateTime))
                {
                    var createdDatePropertyAccess = Expression.Property(parameter, createdDateProperty);
                    Expression? createdDateRangeExpression = null;
                    
                    if (globalFilters.DateFrom.HasValue)
                    {
                        var fromConstant = Expression.Constant(globalFilters.DateFrom.Value);
                        var createdFromCondition = Expression.GreaterThanOrEqual(createdDatePropertyAccess, fromConstant);
                        createdDateRangeExpression = createdFromCondition;
                    }
                    
                    if (globalFilters.DateTo.HasValue)
                    {
                        var toConstant = Expression.Constant(globalFilters.DateTo.Value.AddDays(1)); // Include the entire day
                        var createdToCondition = Expression.LessThan(createdDatePropertyAccess, toConstant);
                        
                        if (createdDateRangeExpression != null)
                        {
                            createdDateRangeExpression = Expression.AndAlso(createdDateRangeExpression, createdToCondition);
                        }
                        else
                        {
                            createdDateRangeExpression = createdToCondition;
                        }
                    }
                    
                    combinedRangeExpression = createdDateRangeExpression;
                }
                
                // Check LastModified
                var lastModifiedDateProperty = entityType.GetProperty("LastModified");
                if (lastModifiedDateProperty != null && lastModifiedDateProperty.PropertyType == typeof(DateTime?))
                {
                    var lastModifiedDatePropertyAccess = Expression.Property(parameter, lastModifiedDateProperty);
                    Expression? lastModifiedDateRangeExpression = null;
                    
                    if (globalFilters.DateFrom.HasValue)
                    {
                        var fromConstant = Expression.Constant(globalFilters.DateFrom.Value, typeof(DateTime?));
                        var lastModifiedFromCondition = Expression.GreaterThanOrEqual(lastModifiedDatePropertyAccess, fromConstant);
                        lastModifiedDateRangeExpression = lastModifiedFromCondition;
                    }
                    
                    if (globalFilters.DateTo.HasValue)
                    {
                        var toConstant = Expression.Constant(globalFilters.DateTo.Value.AddDays(1), typeof(DateTime?)); // Include the entire day
                        var lastModifiedToCondition = Expression.LessThan(lastModifiedDatePropertyAccess, toConstant);
                        
                        if (lastModifiedDateRangeExpression != null)
                        {
                            lastModifiedDateRangeExpression = Expression.AndAlso(lastModifiedDateRangeExpression, lastModifiedToCondition);
                        }
                        else
                        {
                            lastModifiedDateRangeExpression = lastModifiedToCondition;
                        }
                    }
                    
                    if (combinedRangeExpression != null && lastModifiedDateRangeExpression != null)
                    {
                        // Combine with OR: (CreatedDate in range) OR (LastModified in range)
                        combinedRangeExpression = Expression.OrElse(combinedRangeExpression, lastModifiedDateRangeExpression);
                    }
                    else if (lastModifiedDateRangeExpression != null)
                    {
                        combinedRangeExpression = lastModifiedDateRangeExpression;
                    }
                }
                
                // Apply the combined range filter
                if (combinedRangeExpression != null)
                {
                    var rangeLambda = Expression.Lambda<Func<TEntity, bool>>(combinedRangeExpression, parameter);
                    queryable = queryable.Where(rangeLambda);
                }
            }
        }

        return queryable;
    }

    public async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        await _dataDbContext.SaveChangesAsync();

        await PublishMessageToPubSub(entity);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(string[] includes)
    {
        var set = ApplyIncludes(_dbSet, includes);
        
        // Apply soft delete filtering if the entity supports it
        var isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");
        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var isDeletedProp = Expression.Property(parameter, "IsDeleted");
            var notDeleted = Expression.Not(isDeletedProp);
            var isDeletedLambda = Expression.Lambda<Func<TEntity, bool>>(notDeleted, parameter);
            set = set.Where(isDeletedLambda);
        }
        
        return await set.ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync() => await GetAllAsync(Array.Empty<string>());

    public IEnumerable<TEntity> GetAll(string[] includes)
    {
        var set = ApplyIncludes(_dbSet, includes);
        
        // Apply soft delete filtering if the entity supports it
        var isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");
        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var isDeletedProp = Expression.Property(parameter, "IsDeleted");
            var notDeleted = Expression.Not(isDeletedProp);
            var isDeletedLambda = Expression.Lambda<Func<TEntity, bool>>(notDeleted, parameter);
            set = set.Where(isDeletedLambda);
        }
        
        return set.AsEnumerable();
    }

    public IEnumerable<TEntity> GetAll() => GetAll(Array.Empty<string>());

    public async Task<TEntity?> GetByIdAsync(int id, string[] includes)
    {
        var set = ApplyIncludes(_dbSet, includes);
        
        // Apply soft delete filtering if the entity supports it
        var isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");
        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var idProperty = GetIdProperty(typeof(TEntity));
            if (idProperty != null)
            {
                var idAccess = Expression.Property(parameter, idProperty);
                var idEquals = Expression.Equal(idAccess, Expression.Constant(id));
                var isDeletedProp = Expression.Property(parameter, "IsDeleted");
                var notDeleted = Expression.Not(isDeletedProp);
                var combined = Expression.AndAlso(idEquals, notDeleted);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
                return await set.SingleOrDefaultAsync(lambda);
            }
        }
        
        return await set.SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TEntity?> GetByIdAsync(int id) => await GetByIdAsync(id, Array.Empty<string>());

    public async Task UpdateAsync(TEntity entity)
    {
        await _dataDbContext.SingleUpdateAsync<TEntity>(entity);
        await _dataDbContext.SaveChangesAsync();

        await PublishMessageToPubSub(entity);
    }

    public async Task Delete(TEntity entity)
    {
        _dataDbContext.Remove(entity);
        await _dataDbContext.SaveChangesAsync();

        await PublishMessageToPubSub(entity);
    }
    
    public async Task<IEnumerable<TEntity>> GetAllSortedAsync(string sortBy, bool ascending = true)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), parameter);

        IQueryable<TEntity> query = _dbSet;

        // Apply soft delete filtering if the entity supports it
        var isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");
        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            var isDeletedParam = Expression.Parameter(typeof(TEntity), "x");
            var isDeletedProp = Expression.Property(isDeletedParam, "IsDeleted");
            var notDeleted = Expression.Not(isDeletedProp);
            var isDeletedLambda = Expression.Lambda<Func<TEntity, bool>>(notDeleted, isDeletedParam);
            query = query.Where(isDeletedLambda);
        }

        if (ascending)
        {
            query = query.OrderBy(lambda);
        }
        else
        {
            query = query.OrderByDescending(lambda);
        }

        return await query.ToListAsync();
    }

    public async Task PublishMessageToPubSub(TEntity entity)
    {
        var entityName = typeof(TEntity).Name.Replace("UNOPS", "").Pluralize();
        
        // Get all potential ID properties with case-insensitive match
        var idProperties = entity.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
            .Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && p.PropertyType == typeof(int))
            .ToList();
        
        // Try to find a non-zero ID value
        int entityId = idProperties
            .Select(p => (int)p.GetValue(entity))
            .FirstOrDefault(value => value != 0);
        
        // Skip publishing if the ID is 0 or invalid
        if (entityId <= 0)
        {
            return;
        }
        
        var message = new MyPubSubMessage
        {
            EntityName = entityName,
            EntityId = entityId,
            MessageType = "EntityProcessing"
        };

        await _aiService.PublishMessageToPubSub(message);
    }

    /// <summary>
    /// Get the current user's ClaimsPrincipal for global filter application
    /// </summary>
    private ClaimsPrincipal GetCurrentClaimsPrincipal()
    {
        if (_serviceProvider == null)
            return null;

        var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
        var context = httpContextAccessor?.HttpContext;
        return context?.User;
    }
}
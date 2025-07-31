using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Utilities.Helpers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace UNOPS.PAO.UNOPSBusiness.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Linq.Expressions;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Humanizer;
using Newtonsoft.Json;

public class BaseRepository<TEntity>  where TEntity : class, IBaseBusinessEntity<int>
{
    protected readonly UNOPSAppDbContext _dataDbContext;
    protected DbSet<TEntity> _dbSet;
    protected readonly IConfiguration _configuration;
    protected readonly AiContextualService _aiService;
    private readonly IServiceProvider _serviceProvider;

    private IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> set, string[] includes)
    {
        return includes.Aggregate(set, (current, include) => current.Include(include));
    }

    public BaseRepository(UNOPSAppDbContext context, IConfiguration configuration, IServiceProvider serviceProvider = null)
    {
        _dataDbContext = context;
        _dbSet = context.Set<TEntity>();
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _aiService = new AiContextualService(configuration, context, null);
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
    /// Applies global filters to a queryable based on user preferences
    /// </summary>
    protected async Task<IQueryable<TEntity>> ApplyGlobalFiltersAsync(IQueryable<TEntity> queryable)
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

        var globalFilters = await userPreferenceService.GetGlobalFiltersAsync(currentUserId);
        if (globalFilters == null)
            return queryable;

        var entityType = typeof(TEntity);

        // Apply organization unit filter
        if (globalFilters.OrgUnitId.HasValue)
        {
            var orgUnitIds = await GetDescendantOrgUnitIdsAsync(globalFilters.OrgUnitId.Value);
            
            if (entityType == typeof(Partner))
            {
                // Pre-materialize the partner IDs that match the org unit criteria to avoid nested query issues
                var validPartnerIds = await _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();
                
                var partnerQuery = queryable as IQueryable<Partner>;
                queryable = partnerQuery.Where(p => validPartnerIds.Contains(p.Id)) as IQueryable<TEntity>;
            }
            else if (entityType == typeof(Contact))
            {
                // Pre-materialize the partner IDs that match the org unit criteria
                var validPartnerIds = await _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();
                
                var contactQuery = queryable as IQueryable<Contact>;
                queryable = contactQuery.Where(c => c.Partner != null && validPartnerIds.Contains(c.Partner.Id)) as IQueryable<TEntity>;
            }
            else if (entityType == typeof(Interaction))
            {
                // Pre-materialize the partner IDs that match the org unit criteria
                var validPartnerIds = await _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();
                
                // Pre-materialize the interaction IDs that match the org unit criteria
                var validInteractionIds = await _dataDbContext.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Interaction" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();
                
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
        }

        // Apply user-based filters
        var currentUserIdAsInt = GetCurrentUserIdAsInt();
        if (currentUserIdAsInt.HasValue && globalFilters.RelatedToMe)
        {
            // RelatedToMe filter: check both CreatedBy AND LastUpdatedBy
            var createdByProperty = entityType.GetProperty("CreatedBy");
            var lastUpdatedByProperty = entityType.GetProperty("LastUpdatedBy");
            
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
            
            // Check LastUpdatedBy
            if (lastUpdatedByProperty != null && (lastUpdatedByProperty.PropertyType == typeof(int) || lastUpdatedByProperty.PropertyType == typeof(int?)))
            {
                var lastUpdatedByPropertyAccess = Expression.Property(parameter, lastUpdatedByProperty);
                var lastUpdatedByConstant = Expression.Constant(currentUserIdAsInt.Value, lastUpdatedByProperty.PropertyType);
                var lastUpdatedByEquals = Expression.Equal(lastUpdatedByPropertyAccess, lastUpdatedByConstant);
                
                if (combinedUserExpression != null)
                {
                    // Combine with OR: (CreatedBy == userId) OR (LastUpdatedBy == userId)
                    combinedUserExpression = Expression.OrElse(combinedUserExpression, lastUpdatedByEquals);
                }
                else
                {
                    combinedUserExpression = lastUpdatedByEquals;
                }
            }
            
            // Apply the combined user filter
            if (combinedUserExpression != null)
            {
                var userLambda = Expression.Lambda<Func<TEntity, bool>>(combinedUserExpression, parameter);
                queryable = queryable.Where(userLambda);
            }
        }

        // Apply date filters (applies to both CreatedDate AND LastUpdatedDate)
        // Single date mode - prioritize single date over range
        if (globalFilters.DateOn.HasValue)
        {
            // Single date mode - filter for this specific date on both CreatedDate and LastUpdatedDate
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
            
            // Check LastUpdatedDate
            var lastUpdatedDateProperty = entityType.GetProperty("LastUpdatedDate");
            if (lastUpdatedDateProperty != null && lastUpdatedDateProperty.PropertyType == typeof(DateTime?))
            {
                var lastUpdatedDatePropertyAccess = Expression.Property(parameter, lastUpdatedDateProperty);
                var startConstant = Expression.Constant(startOfDay, typeof(DateTime?));
                var endConstant = Expression.Constant(endOfDay, typeof(DateTime?));
                
                var lastUpdatedGreaterThanOrEqual = Expression.GreaterThanOrEqual(lastUpdatedDatePropertyAccess, startConstant);
                var lastUpdatedLessThan = Expression.LessThan(lastUpdatedDatePropertyAccess, endConstant);
                var lastUpdatedDateCondition = Expression.AndAlso(lastUpdatedGreaterThanOrEqual, lastUpdatedLessThan);
                
                if (combinedDateExpression != null)
                {
                    // Combine with OR: (CreatedDate in range) OR (LastUpdatedDate in range)
                    combinedDateExpression = Expression.OrElse(combinedDateExpression, lastUpdatedDateCondition);
                }
                else
                {
                    combinedDateExpression = lastUpdatedDateCondition;
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
            // Range mode - use DateFrom and DateTo if available (applies to both CreatedDate and LastUpdatedDate)
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
                
                // Check LastUpdatedDate
                var lastUpdatedDateProperty = entityType.GetProperty("LastUpdatedDate");
                if (lastUpdatedDateProperty != null && lastUpdatedDateProperty.PropertyType == typeof(DateTime?))
                {
                    var lastUpdatedDatePropertyAccess = Expression.Property(parameter, lastUpdatedDateProperty);
                    Expression? lastUpdatedDateRangeExpression = null;
                    
                    if (globalFilters.DateFrom.HasValue)
                    {
                        var fromConstant = Expression.Constant(globalFilters.DateFrom.Value, typeof(DateTime?));
                        var lastUpdatedFromCondition = Expression.GreaterThanOrEqual(lastUpdatedDatePropertyAccess, fromConstant);
                        lastUpdatedDateRangeExpression = lastUpdatedFromCondition;
                    }
                    
                    if (globalFilters.DateTo.HasValue)
                    {
                        var toConstant = Expression.Constant(globalFilters.DateTo.Value.AddDays(1), typeof(DateTime?)); // Include the entire day
                        var lastUpdatedToCondition = Expression.LessThan(lastUpdatedDatePropertyAccess, toConstant);
                        
                        if (lastUpdatedDateRangeExpression != null)
                        {
                            lastUpdatedDateRangeExpression = Expression.AndAlso(lastUpdatedDateRangeExpression, lastUpdatedToCondition);
                        }
                        else
                        {
                            lastUpdatedDateRangeExpression = lastUpdatedToCondition;
                        }
                    }
                    
                    if (combinedRangeExpression != null && lastUpdatedDateRangeExpression != null)
                    {
                        // Combine with OR: (CreatedDate in range) OR (LastUpdatedDate in range)
                        combinedRangeExpression = Expression.OrElse(combinedRangeExpression, lastUpdatedDateRangeExpression);
                    }
                    else if (lastUpdatedDateRangeExpression != null)
                    {
                        combinedRangeExpression = lastUpdatedDateRangeExpression;
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
        }

        // Apply user-based filters
        var currentUserIdAsInt = GetCurrentUserIdAsInt();
        if (currentUserIdAsInt.HasValue && globalFilters.RelatedToMe)
        {
            // RelatedToMe filter: check both CreatedBy AND LastUpdatedBy
            var createdByProperty = entityType.GetProperty("CreatedBy");
            var lastUpdatedByProperty = entityType.GetProperty("LastUpdatedBy");
            
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
            
            // Check LastUpdatedBy
            if (lastUpdatedByProperty != null && (lastUpdatedByProperty.PropertyType == typeof(int) || lastUpdatedByProperty.PropertyType == typeof(int?)))
            {
                var lastUpdatedByPropertyAccess = Expression.Property(parameter, lastUpdatedByProperty);
                var lastUpdatedByConstant = Expression.Constant(currentUserIdAsInt.Value, lastUpdatedByProperty.PropertyType);
                var lastUpdatedByEquals = Expression.Equal(lastUpdatedByPropertyAccess, lastUpdatedByConstant);
                
                if (combinedUserExpression != null)
                {
                    // Combine with OR: (CreatedBy == userId) OR (LastUpdatedBy == userId)
                    combinedUserExpression = Expression.OrElse(combinedUserExpression, lastUpdatedByEquals);
                }
                else
                {
                    combinedUserExpression = lastUpdatedByEquals;
                }
            }
            
            // Apply the combined user filter
            if (combinedUserExpression != null)
            {
                var userLambda = Expression.Lambda<Func<TEntity, bool>>(combinedUserExpression, parameter);
                queryable = queryable.Where(userLambda);
            }
        }

        // Apply date filters (applies to both CreatedDate AND LastUpdatedDate)
        // Single date mode - prioritize single date over range
        if (globalFilters.DateOn.HasValue)
        {
            // Single date mode - filter for this specific date on both CreatedDate and LastUpdatedDate
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
            
            // Check LastUpdatedDate
            var lastUpdatedDateProperty = entityType.GetProperty("LastUpdatedDate");
            if (lastUpdatedDateProperty != null && lastUpdatedDateProperty.PropertyType == typeof(DateTime?))
            {
                var lastUpdatedDatePropertyAccess = Expression.Property(parameter, lastUpdatedDateProperty);
                var startConstant = Expression.Constant(startOfDay, typeof(DateTime?));
                var endConstant = Expression.Constant(endOfDay, typeof(DateTime?));
                
                var lastUpdatedGreaterThanOrEqual = Expression.GreaterThanOrEqual(lastUpdatedDatePropertyAccess, startConstant);
                var lastUpdatedLessThan = Expression.LessThan(lastUpdatedDatePropertyAccess, endConstant);
                var lastUpdatedDateCondition = Expression.AndAlso(lastUpdatedGreaterThanOrEqual, lastUpdatedLessThan);
                
                if (combinedDateExpression != null)
                {
                    // Combine with OR: (CreatedDate in range) OR (LastUpdatedDate in range)
                    combinedDateExpression = Expression.OrElse(combinedDateExpression, lastUpdatedDateCondition);
                }
                else
                {
                    combinedDateExpression = lastUpdatedDateCondition;
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
            // Range mode - use DateFrom and DateTo if available (applies to both CreatedDate and LastUpdatedDate)
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
                
                // Check LastUpdatedDate
                var lastUpdatedDateProperty = entityType.GetProperty("LastUpdatedDate");
                if (lastUpdatedDateProperty != null && lastUpdatedDateProperty.PropertyType == typeof(DateTime?))
                {
                    var lastUpdatedDatePropertyAccess = Expression.Property(parameter, lastUpdatedDateProperty);
                    Expression? lastUpdatedDateRangeExpression = null;
                    
                    if (globalFilters.DateFrom.HasValue)
                    {
                        var fromConstant = Expression.Constant(globalFilters.DateFrom.Value, typeof(DateTime?));
                        var lastUpdatedFromCondition = Expression.GreaterThanOrEqual(lastUpdatedDatePropertyAccess, fromConstant);
                        lastUpdatedDateRangeExpression = lastUpdatedFromCondition;
                    }
                    
                    if (globalFilters.DateTo.HasValue)
                    {
                        var toConstant = Expression.Constant(globalFilters.DateTo.Value.AddDays(1), typeof(DateTime?)); // Include the entire day
                        var lastUpdatedToCondition = Expression.LessThan(lastUpdatedDatePropertyAccess, toConstant);
                        
                        if (lastUpdatedDateRangeExpression != null)
                        {
                            lastUpdatedDateRangeExpression = Expression.AndAlso(lastUpdatedDateRangeExpression, lastUpdatedToCondition);
                        }
                        else
                        {
                            lastUpdatedDateRangeExpression = lastUpdatedToCondition;
                        }
                    }
                    
                    if (combinedRangeExpression != null && lastUpdatedDateRangeExpression != null)
                    {
                        // Combine with OR: (CreatedDate in range) OR (LastUpdatedDate in range)
                        combinedRangeExpression = Expression.OrElse(combinedRangeExpression, lastUpdatedDateRangeExpression);
                    }
                    else if (lastUpdatedDateRangeExpression != null)
                    {
                        combinedRangeExpression = lastUpdatedDateRangeExpression;
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
        var filteredSet = await ApplyGlobalFiltersAsync(set);
        return await filteredSet.ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync() => await GetAllAsync(Array.Empty<string>());

    public IEnumerable<TEntity> GetAll(string[] includes)
    {
        var set = ApplyIncludes(_dbSet, includes);
        var filteredSet = ApplyGlobalFilters(set);
        return filteredSet.AsEnumerable();
    }

    public IEnumerable<TEntity> GetAll() => GetAll(Array.Empty<string>());

    public async Task<TEntity?> GetByIdAsync(int id, string[] includes)
    {
        var set = ApplyIncludes(_dbSet, includes);
        return await set.SingleOrDefaultAsync(x => x.Id == id);

       /* var filteredSet = await ApplyGlobalFiltersAsync(set);
        return await filteredSet.SingleOrDefaultAsync(x => x.Id == id);*/
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
        query = await ApplyGlobalFiltersAsync(query);

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
}
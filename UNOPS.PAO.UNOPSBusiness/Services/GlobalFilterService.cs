using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Services;

/// <summary>
/// Centralized service for applying global filters across all queries
/// This ensures consistent global filter behavior across BaseRepository, AdvancedSearchService, and other services
/// </summary>
public class GlobalFilterService
{
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly ILogger<GlobalFilterService> _logger;
    private readonly UNOPSAppDbContext _context;
    private readonly IOrgUnitHierarchyService _orgUnitHierarchyService;

    public GlobalFilterService(
        IUserPreferenceService userPreferenceService,
        ILogger<GlobalFilterService> logger,
        UNOPSAppDbContext context,
        IOrgUnitHierarchyService orgUnitHierarchyService)
    {
        _userPreferenceService = userPreferenceService;
        _logger = logger;
        _context = context;
        _orgUnitHierarchyService = orgUnitHierarchyService;
    }

    /// <summary>
    /// Apply all global filters to a queryable based on user preferences
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="query">Query to filter</param>
    /// <param name="user">Current user for preferences</param>
    /// <returns>Filtered query</returns>
    public async Task<IQueryable<TEntity>> ApplyGlobalFiltersAsync<TEntity>(IQueryable<TEntity> query, ClaimsPrincipal user) 
        where TEntity : class
    {
        if (user?.Identity?.IsAuthenticated != true)
            return query;

        var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
            return query;

        try
        {
            // Get global filters from user preferences
            var globalFilters = await _userPreferenceService.GetGlobalFiltersAsync(currentUserId);
            if (globalFilters == null)
                return query;

            var entityType = typeof(TEntity);
            var parameter = Expression.Parameter(entityType, "x");
            Expression? combinedExpression = null;

            // Apply organization unit filter
            if (globalFilters.OrgUnitId.HasValue)
            {
                combinedExpression = await ApplyOrgUnitFilterAsync(parameter, entityType, globalFilters.OrgUnitId.Value, combinedExpression);
            }

            // Apply "Related to Me" filter
            if (globalFilters.RelatedToMe && int.TryParse(currentUserId, out var userIdInt))
            {
                combinedExpression = ApplyRelatedToMeFilter(parameter, entityType, userIdInt, combinedExpression);
            }

            // Apply date filters
            combinedExpression = ApplyDateFilters(parameter, entityType, globalFilters, combinedExpression);

            // Apply the combined filter expression if any filters were applied
            if (combinedExpression != null)
            {
                var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
                query = query.Where(lambda);
                _logger.LogDebug("Applied global filters for user {UserId}", currentUserId);
            }

            return query;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply global filters for user {UserId}, continuing without filters", currentUserId);
            return query;
        }
    }

    #region Filter Implementation Methods

    /// <summary>
    /// Apply RelatedToMe filter - checks both CreatedBy and LastModifiedBy
    /// </summary>
    private Expression? ApplyRelatedToMeFilter(ParameterExpression parameter, Type entityType, int userId, Expression? existingExpression)
    {
        var createdByProperty = entityType.GetProperty("CreatedBy");
        var lastModifiedByProperty = entityType.GetProperty("LastModifiedBy");
        
        Expression? userExpression = null;
        
        // Check CreatedBy
        if (createdByProperty != null && (createdByProperty.PropertyType == typeof(int) || createdByProperty.PropertyType == typeof(int?)))
        {
            var createdByAccess = Expression.Property(parameter, createdByProperty);
            var createdByConstant = Expression.Constant(userId, createdByProperty.PropertyType);
            var createdByEquals = Expression.Equal(createdByAccess, createdByConstant);
            userExpression = createdByEquals;
        }
        
        // Check LastModifiedBy
        if (lastModifiedByProperty != null && (lastModifiedByProperty.PropertyType == typeof(int) || lastModifiedByProperty.PropertyType == typeof(int?)))
        {
            var lastModifiedByAccess = Expression.Property(parameter, lastModifiedByProperty);
            var lastModifiedByConstant = Expression.Constant(userId, lastModifiedByProperty.PropertyType);
            var lastModifiedByEquals = Expression.Equal(lastModifiedByAccess, lastModifiedByConstant);
            
            if (userExpression != null)
            {
                // Combine with OR: (CreatedBy == userId) OR (LastModifiedBy == userId)
                userExpression = Expression.OrElse(userExpression, lastModifiedByEquals);
            }
            else
            {
                userExpression = lastModifiedByEquals;
            }
        }
        
        // Combine with existing expression using AND
        if (userExpression != null && existingExpression != null)
        {
            return Expression.AndAlso(existingExpression, userExpression);
        }
        
        return userExpression ?? existingExpression;
    }

    /// <summary>
    /// Apply date filters (DateOn, DateFrom, DateTo) to CreatedDate and LastModifiedDate
    /// </summary>
    private Expression? ApplyDateFilters(ParameterExpression parameter, Type entityType, dynamic globalFilters, Expression? existingExpression)
    {
        try
        {
            Expression? dateExpression = null;

            // Check if DateOn is set (single date mode)
            if (HasProperty(globalFilters, "DateOn") && globalFilters.DateOn != null)
            {
                var dateOn = (DateTime)globalFilters.DateOn;
                var startOfDay = dateOn.Date;
                var endOfDay = startOfDay.AddDays(1);

                dateExpression = CreateDateRangeExpression(parameter, entityType, startOfDay, endOfDay);
                _logger.LogDebug("Applied DateOn filter: {Date}", dateOn.ToString("yyyy-MM-dd"));
            }
            // Otherwise check DateFrom/DateTo range mode
            else
            {
                DateTime? dateFrom = HasProperty(globalFilters, "DateFrom") ? globalFilters.DateFrom : null;
                DateTime? dateTo = HasProperty(globalFilters, "DateTo") ? globalFilters.DateTo : null;

                if (dateFrom.HasValue || dateTo.HasValue)
                {
                    var fromDate = dateFrom?.Date;
                    var toDate = dateTo?.Date.AddDays(1); // Include entire day

                    dateExpression = CreateDateRangeExpression(parameter, entityType, fromDate, toDate);
                    _logger.LogDebug("Applied DateFrom/DateTo filter: {From} - {To}", 
                        fromDate?.ToString("yyyy-MM-dd") ?? "no limit", 
                        dateTo?.ToString("yyyy-MM-dd") ?? "no limit");
                }
            }

            // Combine with existing expression using AND
            if (dateExpression != null && existingExpression != null)
            {
                return Expression.AndAlso(existingExpression, dateExpression);
            }

            return dateExpression ?? existingExpression;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error applying date filters, skipping date filtering");
            return existingExpression;
        }
    }

    /// <summary>
    /// Apply OrgUnit filter with smart entity-specific logic
    /// 1. Partner: Direct lookup in OrganizationUnitRelationship table
    /// 2. Contact: Get partners in org unit, then filter contacts by those partners
    /// 3. Interaction: Direct lookup + contacts with partners in org unit
    /// </summary>
    private async Task<Expression?> ApplyOrgUnitFilterAsync(ParameterExpression parameter, Type entityType, int orgUnitId, Expression? existingExpression)
    {
        try
        {
            // Check if the organization unit has code "OPS" - if so, skip filtering
            var orgUnit = await _context.OrganizationHierarchies
                .FirstOrDefaultAsync(x => x.Id == orgUnitId && !x.IsDeleted && x.Status == EntityStatus.Active);
            
            if (orgUnit != null && orgUnit.Code == "OPS")
            {
                _logger.LogDebug("Organization unit {OrgUnitId} has code 'OPS' - skipping org unit filter for {EntityType}", 
                    orgUnitId, entityType.Name);
                return existingExpression; // No filtering for OPS org unit
            }

            // Get all descendant org unit IDs (including the root)
            var orgUnitIds = await _orgUnitHierarchyService.GetDescendantIdsAsync(orgUnitId);
            _logger.LogDebug("Smart org unit filter for {EntityType} with {Count} org units", entityType.Name, orgUnitIds.Count);

            var entityTypeName = GetEntityTypeNameForRelationship(entityType);

            if (entityType == typeof(Partner) || entityType == typeof(UNOPSPartner))
            {
                // 1. Partner: Simple direct lookup in OrganizationUnitRelationship table
                return await ApplyDirectOrgUnitFilterAsync(parameter, entityType, orgUnitIds, "Partner", existingExpression);
            }
            else if (entityType == typeof(Contact) || entityType == typeof(UNOPSContact))
            {
                // 2. Contact: Get partners in org unit, then filter contacts by those partners
                return await ApplyContactOrgUnitFilterAsync(parameter, entityType, orgUnitIds, existingExpression);
            }
            else if (entityType == typeof(Interaction) || entityType == typeof(UNOPSInteraction))
            {
                // 3. Interaction: Direct lookup + contacts with partners in org unit
                return await ApplyInteractionOrgUnitFilterAsync(parameter, entityType, orgUnitIds, existingExpression);
            }
            else
            {
                // For other entity types, try direct lookup first
                return await ApplyDirectOrgUnitFilterAsync(parameter, entityType, orgUnitIds, entityTypeName, existingExpression);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply OrgUnit filter for {EntityType}, continuing without org unit filtering", entityType.Name);
            return existingExpression;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Create date range expression for CreatedDate and/or LastModifiedDate
    /// </summary>
    private Expression? CreateDateRangeExpression(ParameterExpression parameter, Type entityType, DateTime? fromDate, DateTime? toDate)
    {
        var createdDateProperty = entityType.GetProperty("CreatedDate");
        var lastModifiedDateProperty = entityType.GetProperty("LastModifiedDate");

        Expression? combinedDateExpression = null;

        // Check CreatedDate
        if (createdDateProperty != null && createdDateProperty.PropertyType == typeof(DateTime))
        {
            var createdDateAccess = Expression.Property(parameter, createdDateProperty);
            var createdDateCondition = CreatePropertyDateRangeCondition(createdDateAccess, fromDate, toDate, typeof(DateTime));
            
            if (createdDateCondition != null)
            {
                combinedDateExpression = createdDateCondition;
            }
        }

        // Check LastModifiedDate (usually nullable DateTime)
        if (lastModifiedDateProperty != null && 
            (lastModifiedDateProperty.PropertyType == typeof(DateTime?) || lastModifiedDateProperty.PropertyType == typeof(DateTime)))
        {
            var lastModifiedDateAccess = Expression.Property(parameter, lastModifiedDateProperty);
            var lastModifiedDateCondition = CreatePropertyDateRangeCondition(lastModifiedDateAccess, fromDate, toDate, lastModifiedDateProperty.PropertyType);

            if (lastModifiedDateCondition != null)
            {
                if (combinedDateExpression != null)
                {
                    // Combine with OR: (CreatedDate in range) OR (LastModifiedDate in range)
                    combinedDateExpression = Expression.OrElse(combinedDateExpression, lastModifiedDateCondition);
                }
                else
                {
                    combinedDateExpression = lastModifiedDateCondition;
                }
            }
        }

        return combinedDateExpression;
    }

    /// <summary>
    /// Create date range condition for a specific property
    /// </summary>
    private Expression? CreatePropertyDateRangeCondition(MemberExpression propertyAccess, DateTime? fromDate, DateTime? toDate, Type propertyType)
    {
        Expression? condition = null;

        // From date condition
        if (fromDate.HasValue)
        {
            var fromConstant = Expression.Constant(fromDate.Value, propertyType);
            var fromCondition = Expression.GreaterThanOrEqual(propertyAccess, fromConstant);
            condition = fromCondition;
        }

        // To date condition
        if (toDate.HasValue)
        {
            var toConstant = Expression.Constant(toDate.Value, propertyType);
            var toCondition = Expression.LessThan(propertyAccess, toConstant);

            if (condition != null)
            {
                condition = Expression.AndAlso(condition, toCondition);
            }
            else
            {
                condition = toCondition;
            }
        }

        return condition;
    }

    /// <summary>
    /// Check if dynamic object has a property
    /// </summary>
    private bool HasProperty(dynamic obj, string propertyName)
    {
        try
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get entity type name for OrganizationUnitRelationship lookup
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
    /// Apply direct organization unit filtering by looking up entity IDs in OrganizationUnitRelationship table
    /// </summary>
    private async Task<Expression?> ApplyDirectOrgUnitFilterAsync(ParameterExpression parameter, Type entityType, List<int> orgUnitIds, string entityTypeName, Expression? existingExpression)
    {
        var validEntityIds = await _context.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == entityTypeName && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToListAsync();

        _logger.LogDebug("Direct filter found {Count} {EntityType} IDs", validEntityIds.Count, entityTypeName);

        if (validEntityIds.Any())
        {
            var idProperty = GetIdProperty(entityType);
            if (idProperty != null)
            {
                var idAccess = Expression.Property(parameter, idProperty);
                var idsConstant = Expression.Constant(validEntityIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                var containsCall = Expression.Call(idsConstant, containsMethod, idAccess);
                
                // Combine with existing expression using AND
                if (existingExpression != null)
                {
                    return Expression.AndAlso(existingExpression, containsCall);
                }
                return containsCall;
            }
            else
            {
                // If no ID property found, return false (empty result)
                _logger.LogWarning("No ID property found for {EntityType} - returning empty result for org unit filter", entityType.Name);
                return Expression.Constant(false);
            }
        }
        else
        {
            // If no valid entity IDs found in org unit, return false (empty result)
            _logger.LogDebug("No entities found in organization unit for {EntityType} - returning empty result", entityTypeName);
            return Expression.Constant(false);
        }
    }

    /// <summary>
    /// Apply organization unit filtering for Contact entities by finding partners in org unit first
    /// </summary>
    private async Task<Expression?> ApplyContactOrgUnitFilterAsync(ParameterExpression parameter, Type entityType, List<int> orgUnitIds, Expression? existingExpression)
    {
        // Get all partners that belong to the specified organization units
        var validPartnerIds = await _context.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Partner" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToListAsync();
            
        _logger.LogDebug("Contact filter found {Count} partner IDs in org units", validPartnerIds.Count);

        if (validPartnerIds.Any())
        {
            // Filter contacts by PartnerId
            var partnerIdProperty = entityType.GetProperty("PartnerId");
            
            if (partnerIdProperty != null)
            {
                var partnerIdAccess = Expression.Property(parameter, partnerIdProperty);
                var partnerIdsConstant = Expression.Constant(validPartnerIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                var containsCall = Expression.Call(partnerIdsConstant, containsMethod, partnerIdAccess);
                
                // Combine with existing expression using AND
                if (existingExpression != null)
                {
                    return Expression.AndAlso(existingExpression, containsCall);
                }
                return containsCall;
            }
        }

        // If no valid partners found or no PartnerId property, return empty result
        return Expression.Constant(false);
    }

    /// <summary>
    /// Apply organization unit filtering for Interaction entities using both direct and partner-based filtering
    /// </summary>
    private async Task<Expression?> ApplyInteractionOrgUnitFilterAsync(ParameterExpression parameter, Type entityType, List<int> orgUnitIds, Expression? existingExpression)
    {
        // Get direct interaction IDs from OrganizationUnitRelationship table
        var validInteractionIds = await _context.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Interaction" && 
                !orgRel.IsDeleted &&
                orgRel.Status == EntityStatus.Active &&
                orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToListAsync();

        _logger.LogDebug("Interaction filter found {Count} interaction IDs in org units", validInteractionIds.Count);

        if (validInteractionIds.Any())
        {
            var idProperty = GetIdProperty(entityType);
            if (idProperty != null)
            {
                var idAccess = Expression.Property(parameter, idProperty);
                var interactionIdsConstant = Expression.Constant(validInteractionIds);
                var containsMethod = typeof(List<int>).GetMethod("Contains");
                var containsCall = Expression.Call(interactionIdsConstant, containsMethod, idAccess);
                
                // Combine with existing expression using AND
                if (existingExpression != null)
                {
                    return Expression.AndAlso(existingExpression, containsCall);
                }
                return containsCall;
            }
        }

        // If no valid interaction IDs found, return empty result
        return Expression.Constant(false);
    }

    /// <summary>
    /// Get the Id property from an entity type, handling inheritance scenarios
    /// </summary>
    private System.Reflection.PropertyInfo? GetIdProperty(Type entityType)
    {
        // Try to get Id property with DeclaredOnly first to avoid ambiguity
        var idProperty = entityType.GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
        
        if (idProperty == null)
        {
            // If not found with DeclaredOnly, try all properties and get the first Id property
            var allIdProperties = entityType.GetProperties().Where(p => p.Name == "Id").ToArray();
            if (allIdProperties.Length > 0)
            {
                idProperty = allIdProperties[0]; // Take the first one
            }
        }
        
        return idProperty;
    }

    #endregion
}

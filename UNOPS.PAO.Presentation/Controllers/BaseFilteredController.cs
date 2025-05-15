using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using UNOPS.PAO.UNOPSBusiness.Authorization;

namespace UNOPS.PAO.Presentation.Controllers;

/// <summary>
/// Base controller that implements row-level security filtering
/// </summary>
public abstract class BaseFilteredController : ControllerBase
{
    protected readonly IPermissionService _permissionService;

    protected BaseFilteredController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Apply row-level security filters to a query - USE THIS FOR LIST/COLLECTION OPERATIONS
    /// This applies the FilterExpression from EntityPermissions to filter records at the query level
    /// </summary>
    protected async Task<IQueryable<T>> ApplySecurityFiltersAsync<T>(IQueryable<T> query) where T : class
    {
        return await _permissionService.ApplySecurityFiltersAsync(query, User);
    }

    /// <summary>
    /// Check if user has permission to perform action on an entity - USE THIS FOR SINGLE ENTITY OPERATIONS
    /// This evaluates the FilterExpression from EntityPermissions against a specific entity instance
    /// The entity parameter is REQUIRED for row-level filtering to work
    /// </summary>
    protected async Task<bool> CanPerformActionAsync<T>(string action, T entity) where T : class
    {
        // IMPORTANT: This method needs the entity parameter to apply row-level filtering
        // Without it, CheckPermissionWithEntityUncachedAsync never gets called
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), 
                "Entity parameter is required for row-level filtering to work");
        }
        
        return await _permissionService.CanPerformActionAsync(
            entityName: typeof(T).Name,
            action: action,
            user: User,
            entity: entity);
    }
    
    /// <summary>
    /// Check if user has permission to perform action without an entity - NO ROW LEVEL FILTERING
    /// This will only check basic permissions without any row-level filtering
    /// Use this only when you don't have an entity instance yet (e.g., before creation)
    /// </summary>
    protected async Task<bool> CanPerformActionAsync<T>(string action) where T : class
    {
        // WARNING: This will NOT apply row-level filtering since there's no entity
        return await _permissionService.CanPerformActionAsync(
            entityName: typeof(T).Name,
            action: action,
            user: User,
            entity: null);
    }

    /// <summary>
    /// Handle errors in a consistent way when performing database operations
    /// </summary>
    protected async Task<IActionResult> HandleOperationAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            var result = await operation();
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get paginated, filtered results with row-level security applied
    /// </summary>
    protected async Task<IActionResult> GetPaginatedResultsAsync<T>(
        IQueryable<T> query,
        int pageIndex = 1,
        int pageSize = 10,
        string orderBy = null,
        bool ascending = true) where T : class
    {
        // Apply security filters - THIS APPLIES ROW-LEVEL FILTERING
        var filteredQuery = await ApplySecurityFiltersAsync(query);

        // Apply ordering if specified
        if (!string.IsNullOrEmpty(orderBy))
        {
            // This is a simplistic approach - in practice you'd need dynamic ordering
            // Implement dynamic ordering based on the orderBy parameter
        }

        // Calculate pagination
        var totalCount = await filteredQuery.CountAsync();
        var items = await filteredQuery
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize,
            Items = items
        });
    }
} 
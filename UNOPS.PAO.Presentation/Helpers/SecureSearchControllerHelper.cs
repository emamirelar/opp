using System.Security.Claims;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSBusiness.Services;

namespace UNOPS.PAO.Presentation.Helpers;

/// <summary>
/// Helper class for secure search operations in controllers with RBAC integrated at database level
/// This replaces post-query filtering with pre-query filtering for proper pagination
/// </summary>
public static class SecureSearchControllerHelper
{
    /// <summary>
    /// Processes advanced search with integrated RBAC filtering
    /// Security is applied at the database level, ensuring correct pagination and performance
    /// </summary>
    public static async Task<TResult> ProcessSecureAdvancedSearchAsync<TEntity, TFilterRequest, TResult>(
        string searchCriteria,
        string? searchText,
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool? ascending,
        TFilterRequest filterRequest,
        string entityType,
        ClaimsPrincipal user,
        Func<TFilterRequest, ClaimsPrincipal, Task<ISpecification<TEntity>>> secureSpecificationFactory,
        Func<int, ISpecification<TEntity>, PaginationRequest, Task<TResult>> searchExecutor,
        int currentUserId,
        ILogger logger) where TFilterRequest : PaginationRequest
    {
        // Decode and validate search criteria
        var criteriaToUse = AdvancedSearchHelper.DecodeSearchCriteria(searchCriteria);
        var allowedFields = AdvancedSearchHelper.GetAllowedFieldsForEntity(entityType);
        var parsedCriteria = AdvancedSearchHelper.ValidateAndParseSearchCriteria(criteriaToUse, allowedFields);
        
        // Update filter request with search parameters
        UpdateFilterRequest(filterRequest, true, criteriaToUse, searchText, pageIndex, pageSize, orderBy, ascending);
        
        // Create secure specification with RBAC integrated
        var specification = await secureSpecificationFactory(filterRequest, user);
        
        logger.LogInformation("Executing secure advanced search for {EntityType} with criteria: {SearchCriteria}", 
            entityType, criteriaToUse);
        
        return await searchExecutor(currentUserId, specification, filterRequest);
    }
    
    /// <summary>
    /// Processes simple text search with integrated RBAC filtering
    /// Security is applied at the database level, ensuring correct pagination and performance
    /// </summary>
    public static async Task<TResult> ProcessSecureSimpleTextSearchAsync<TEntity, TFilterRequest, TResult>(
        string searchText,
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool? ascending,
        TFilterRequest filterRequest,
        string entityType,
        ClaimsPrincipal user,
        Func<TFilterRequest, ClaimsPrincipal, Task<ISpecification<TEntity>>> secureSpecificationFactory,
        Func<int, ISpecification<TEntity>, PaginationRequest, Task<TResult>> searchExecutor,
        int currentUserId,
        ILogger logger) where TFilterRequest : PaginationRequest
    {
        // Update filter request with search parameters
        UpdateFilterRequest(filterRequest, false, null, searchText.Trim(), pageIndex, pageSize, orderBy, ascending);
        
        // Create secure specification with RBAC integrated
        var specification = await secureSpecificationFactory(filterRequest, user);
        
        logger.LogInformation("Executing secure simple text search for {EntityType}: {SearchText}", entityType, searchText);
        return await searchExecutor(currentUserId, specification, filterRequest);
    }
    
    /// <summary>
    /// Processes standard listing with integrated RBAC filtering
    /// Security is applied at the database level, ensuring correct pagination and performance
    /// </summary>
    public static async Task<TResult> ProcessSecureListingAsync<TEntity, TFilterRequest, TResult>(
        TFilterRequest filterRequest,
        string entityType,
        ClaimsPrincipal user,
        Func<TFilterRequest, ClaimsPrincipal, Task<ISpecification<TEntity>>> secureSpecificationFactory,
        Func<int, ISpecification<TEntity>, PaginationRequest, Task<TResult>> searchExecutor,
        int currentUserId,
        ILogger logger) where TFilterRequest : PaginationRequest
    {
        // Create secure specification with RBAC integrated
        var specification = await secureSpecificationFactory(filterRequest, user);
        
        logger.LogInformation("Executing secure listing for {EntityType} with pagination", entityType);
        return await searchExecutor(currentUserId, specification, filterRequest);
    }
    
    /// <summary>
    /// Updates filter request properties using reflection
    /// </summary>
    private static void UpdateFilterRequest<TFilterRequest>(
        TFilterRequest filterRequest,
        bool advancedSearch,
        string? searchCriteria,
        string? searchText,
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool? ascending) where TFilterRequest : PaginationRequest
    {
        // Set pagination properties directly
        filterRequest.PageIndex = pageIndex;
        filterRequest.PageSize = pageSize;
        filterRequest.OrderBy = orderBy;
        filterRequest.Ascending = ascending;
        
        // Set search-specific properties using reflection for flexibility
        SetPropertyIfExists(filterRequest, "AdvancedSearch", advancedSearch);
        SetPropertyIfExists(filterRequest, "SearchCriteria", searchCriteria);
        SetPropertyIfExists(filterRequest, "SearchText", searchText);
    }
    
    /// <summary>
    /// Helper method to set property value if the property exists
    /// </summary>
    private static void SetPropertyIfExists<T>(T obj, string propertyName, object? value)
    {
        var property = typeof(T).GetProperty(propertyName);
        if (property != null && property.CanWrite && value != null)
        {
            property.SetValue(obj, value);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Presentation.Helpers;

/// <summary>
/// Helper class for search operations in controllers
/// </summary>
public static class SearchControllerHelper
{
    /// <summary>
    /// Creates a filter request object for simple or advanced search
    /// </summary>
    /// <typeparam name="TFilterRequest">Type of filter request</typeparam>
    /// <param name="advancedSearch">Whether this is an advanced search</param>
    /// <param name="searchCriteria">Search criteria JSON (for advanced search)</param>
    /// <param name="searchText">Simple search text</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="orderBy">Order by field</param>
    /// <param name="ascending">Sort direction</param>
    /// <returns>Configured filter request</returns>
    public static TFilterRequest CreateFilterRequest<TFilterRequest>(
        bool advancedSearch,
        string? searchCriteria,
        string? searchText,
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool? ascending) where TFilterRequest : new()
    {
        var filterRequest = new TFilterRequest();
        
        // Set common properties using reflection
        SetPropertyIfExists(filterRequest, "AdvancedSearch", advancedSearch);
        SetPropertyIfExists(filterRequest, "SearchCriteria", searchCriteria);
        SetPropertyIfExists(filterRequest, "SearchText", searchText);
        SetPropertyIfExists(filterRequest, "PageIndex", pageIndex);
        SetPropertyIfExists(filterRequest, "PageSize", pageSize);
        SetPropertyIfExists(filterRequest, "OrderBy", orderBy);
        SetPropertyIfExists(filterRequest, "Ascending", ascending);
        
        return filterRequest;
    }

    /// <summary>
    /// Processes advanced search for any entity type
    /// </summary>
    /// <typeparam name="TFilterRequest">Type of filter request</typeparam>
    /// <typeparam name="TSpecification">Type of specification</typeparam>
    /// <typeparam name="TResult">Type of search result</typeparam>
    /// <param name="searchCriteria">Search criteria JSON</param>
    /// <param name="searchText">Additional search text</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="orderBy">Order by field</param>
    /// <param name="ascending">Sort direction</param>
    /// <param name="paginationRequest">Pagination request</param>
    /// <param name="entityType">Entity type for validation</param>
    /// <param name="specificationFactory">Factory function to create specification</param>
    /// <param name="searchExecutor">Function to execute the search</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>Search result</returns>
    public static async Task<TResult> ProcessAdvancedSearch<TFilterRequest, TSpecification, TResult>(
        string searchCriteria,
        string? searchText,
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool? ascending,
        PaginationRequest paginationRequest,
        string entityType,
        Func<TFilterRequest, TSpecification> specificationFactory,
        Func<int, TSpecification, PaginationRequest, Task<TResult>> searchExecutor,
        int currentUserId,
        ILogger logger) where TFilterRequest : new()
    {
        // Decode and validate search criteria
        var criteriaToUse = AdvancedSearchHelper.DecodeSearchCriteria(searchCriteria);
        var allowedFields = AdvancedSearchHelper.GetAllowedFieldsForEntity(entityType);
        var parsedCriteria = AdvancedSearchHelper.ValidateAndParseSearchCriteria(criteriaToUse, allowedFields);
        
        // Create filter request for advanced search
        var filterRequest = CreateFilterRequest<TFilterRequest>(
            true, criteriaToUse, searchText, pageIndex, pageSize, orderBy, ascending);
        
        // Execute search with specification
        var specification = specificationFactory(filterRequest);
        logger.LogInformation("Executing advanced search for {EntityType} with criteria: {SearchCriteria}", 
            entityType, criteriaToUse);
        
        return await searchExecutor(currentUserId, specification, paginationRequest);
    }

    /// <summary>
    /// Processes simple text search for any entity type
    /// </summary>
    /// <typeparam name="TFilterRequest">Type of filter request</typeparam>
    /// <typeparam name="TSpecification">Type of specification</typeparam>
    /// <typeparam name="TResult">Type of search result</typeparam>
    /// <param name="searchText">Search text</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="orderBy">Order by field</param>
    /// <param name="ascending">Sort direction</param>
    /// <param name="paginationRequest">Pagination request</param>
    /// <param name="entityType">Entity type for logging</param>
    /// <param name="specificationFactory">Factory function to create specification</param>
    /// <param name="searchExecutor">Function to execute the search</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>Search result</returns>
    public static async Task<TResult> ProcessSimpleTextSearch<TFilterRequest, TSpecification, TResult>(
        string searchText,
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool? ascending,
        PaginationRequest paginationRequest,
        string entityType,
        Func<TFilterRequest, TSpecification> specificationFactory,
        Func<int, TSpecification, PaginationRequest, Task<TResult>> searchExecutor,
        int currentUserId,
        ILogger logger) where TFilterRequest : new()
    {
        var filterRequest = CreateFilterRequest<TFilterRequest>(
            false, null, searchText.Trim(), pageIndex, pageSize, orderBy, ascending);
        var specification = specificationFactory(filterRequest);
        
        logger.LogInformation("Executing simple text search for {EntityType}: {SearchText}", entityType, searchText);
        return await searchExecutor(currentUserId, specification, paginationRequest);
    }

    /// <summary>
    /// Processes advanced search for any entity type (synchronous version)
    /// </summary>
    /// <typeparam name="TFilterRequest">Type of filter request</typeparam>
    /// <typeparam name="TSpecification">Type of specification</typeparam>
    /// <typeparam name="TResult">Type of search result</typeparam>
    /// <param name="searchCriteria">Search criteria JSON</param>
    /// <param name="searchText">Additional search text</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="orderBy">Order by field</param>
    /// <param name="ascending">Sort direction</param>
    /// <param name="paginationRequest">Pagination request</param>
    /// <param name="entityType">Entity type for validation</param>
    /// <param name="specificationFactory">Factory function to create specification</param>
    /// <param name="searchExecutor">Function to execute the search (synchronous)</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>Search result</returns>
    public static TResult ProcessAdvancedSearchSync<TFilterRequest, TSpecification, TResult>(
        string searchCriteria,
        string? searchText,
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool? ascending,
        PaginationRequest paginationRequest,
        string entityType,
        Func<TFilterRequest, TSpecification> specificationFactory,
        Func<int, TSpecification, PaginationRequest, TResult> searchExecutor,
        int currentUserId,
        ILogger logger) where TFilterRequest : new()
    {
        // Decode and validate search criteria
        var criteriaToUse = AdvancedSearchHelper.DecodeSearchCriteria(searchCriteria);
        var allowedFields = AdvancedSearchHelper.GetAllowedFieldsForEntity(entityType);
        var parsedCriteria = AdvancedSearchHelper.ValidateAndParseSearchCriteria(criteriaToUse, allowedFields);
        
        // Create filter request for advanced search
        var filterRequest = CreateFilterRequest<TFilterRequest>(
            true, criteriaToUse, searchText, pageIndex, pageSize, orderBy, ascending);
        
        // Execute search with specification
        var specification = specificationFactory(filterRequest);
        logger.LogInformation("Executing advanced search for {EntityType} with criteria: {SearchCriteria}", 
            entityType, criteriaToUse);
        
        return searchExecutor(currentUserId, specification, paginationRequest);
    }

    /// <summary>
    /// Processes simple text search for any entity type (synchronous version)
    /// </summary>
    /// <typeparam name="TFilterRequest">Type of filter request</typeparam>
    /// <typeparam name="TSpecification">Type of specification</typeparam>
    /// <typeparam name="TResult">Type of search result</typeparam>
    /// <param name="searchText">Search text</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="orderBy">Order by field</param>
    /// <param name="ascending">Sort direction</param>
    /// <param name="paginationRequest">Pagination request</param>
    /// <param name="entityType">Entity type for logging</param>
    /// <param name="specificationFactory">Factory function to create specification</param>
    /// <param name="searchExecutor">Function to execute the search (synchronous)</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>Search result</returns>
    public static TResult ProcessSimpleTextSearchSync<TFilterRequest, TSpecification, TResult>(
        string searchText,
        int pageIndex,
        int pageSize,
        string? orderBy,
        bool? ascending,
        PaginationRequest paginationRequest,
        string entityType,
        Func<TFilterRequest, TSpecification> specificationFactory,
        Func<int, TSpecification, PaginationRequest, TResult> searchExecutor,
        int currentUserId,
        ILogger logger) where TFilterRequest : new()
    {
        var filterRequest = CreateFilterRequest<TFilterRequest>(
            false, null, searchText.Trim(), pageIndex, pageSize, orderBy, ascending);
        var specification = specificationFactory(filterRequest);
        
        logger.LogInformation("Executing simple text search for {EntityType}: {SearchText}", entityType, searchText);
        return searchExecutor(currentUserId, specification, paginationRequest);
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
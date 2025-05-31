using System.Text.Json;
using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Presentation.Helpers;

/// <summary>
/// Helper class for processing advanced search criteria
/// </summary>
public static class AdvancedSearchHelper
{
    /// <summary>
    /// Maps advanced search criteria JSON to a filter request object
    /// </summary>
    /// <typeparam name="T">The type of filter request</typeparam>
    /// <param name="searchCriteriaJson">JSON string containing search criteria</param>
    /// <returns>A new instance of T with mapped properties</returns>
    public static T MapAdvancedSearchCriteria<T>(string searchCriteriaJson) where T : new()
    {
        if (string.IsNullOrWhiteSpace(searchCriteriaJson))
        {
            throw new ArgumentException("Search criteria JSON cannot be null or empty", nameof(searchCriteriaJson));
        }

        try
        {
            var searchCriteria = JsonSerializer.Deserialize<List<SearchCriteria>>(searchCriteriaJson);
            if (searchCriteria == null || !searchCriteria.Any())
            {
                throw new ArgumentException("Search criteria JSON is invalid or empty");
            }

            var result = new T();
            
            // Set AdvancedSearch flag if the type supports it
            var advancedSearchProperty = typeof(T).GetProperty("AdvancedSearch");
            if (advancedSearchProperty != null && advancedSearchProperty.CanWrite)
            {
                advancedSearchProperty.SetValue(result, true);
            }
            
            // Set SearchCriteria property if the type supports it
            var searchCriteriaProperty = typeof(T).GetProperty("SearchCriteria");
            if (searchCriteriaProperty != null && searchCriteriaProperty.CanWrite)
            {
                searchCriteriaProperty.SetValue(result, searchCriteriaJson);
            }
            
            // Set ParsedSearchCriteria property if the type supports it
            var parsedSearchCriteriaProperty = typeof(T).GetProperty("ParsedSearchCriteria");
            if (parsedSearchCriteriaProperty != null && parsedSearchCriteriaProperty.CanWrite)
            {
                parsedSearchCriteriaProperty.SetValue(result, searchCriteria);
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format in search criteria: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Error processing advanced search criteria: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates search criteria for security and correctness
    /// </summary>
    /// <param name="criteria">The search criteria to validate</param>
    /// <param name="allowedFields">List of allowed field names for security</param>
    public static void ValidateSearchCriteria(List<SearchCriteria> criteria, HashSet<string> allowedFields)
    {
        if (criteria == null || !criteria.Any())
        {
            return;
        }

        var validOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "is", "is not", "like", "not like", ">", "<", ">=", "<=", "after", "before", "between"
        };

        var validLogicalOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AND", "OR"
        };

        foreach (var criterion in criteria)
        {
            // Validate field name
            if (string.IsNullOrWhiteSpace(criterion.Field))
            {
                throw new ArgumentException("Field name cannot be empty");
            }

            if (allowedFields.Any() && !allowedFields.Contains(criterion.Field, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Field '{criterion.Field}' is not allowed for search");
            }

            // Validate operator
            if (string.IsNullOrWhiteSpace(criterion.Operator) || !validOperators.Contains(criterion.Operator))
            {
                throw new ArgumentException($"Invalid operator '{criterion.Operator}'. Allowed operators: {string.Join(", ", validOperators)}");
            }

            // Validate value
            if (string.IsNullOrWhiteSpace(criterion.Value))
            {
                throw new ArgumentException($"Value cannot be empty for field '{criterion.Field}'");
            }

            // Validate logical operator
            if (!string.IsNullOrWhiteSpace(criterion.LogicalOperator) && 
                !validLogicalOperators.Contains(criterion.LogicalOperator))
            {
                throw new ArgumentException($"Invalid logical operator '{criterion.LogicalOperator}'. Allowed operators: {string.Join(", ", validLogicalOperators)}");
            }
        }
    }

    /// <summary>
    /// Decodes URL-encoded search criteria if needed
    /// </summary>
    /// <param name="searchCriteria">The search criteria to decode</param>
    /// <returns>Decoded search criteria</returns>
    public static string DecodeSearchCriteria(string searchCriteria)
    {
        if (string.IsNullOrEmpty(searchCriteria))
        {
            return searchCriteria;
        }
        
        var decoded = System.Net.WebUtility.UrlDecode(searchCriteria);
        return decoded ?? searchCriteria;
    }

    /// <summary>
    /// Validates and parses search criteria from JSON string
    /// </summary>
    /// <param name="criteriaJson">JSON string containing search criteria</param>
    /// <param name="allowedFields">Set of allowed field names for validation</param>
    /// <returns>Parsed and validated search criteria</returns>
    public static List<SearchCriteria> ValidateAndParseSearchCriteria(string criteriaJson, HashSet<string> allowedFields)
    {
        var parsedCriteria = JsonSerializer.Deserialize<List<SearchCriteria>>(criteriaJson);
        
        if (parsedCriteria == null || parsedCriteria.Count == 0)
        {
            throw new ArgumentException("Search criteria cannot be empty");
        }
        
        ValidateSearchCriteria(parsedCriteria, allowedFields);
        return parsedCriteria;
    }

    /// <summary>
    /// Gets the allowed search fields for Contact entity
    /// </summary>
    /// <returns>HashSet of allowed field names</returns>
    public static HashSet<string> GetContactAllowedFields()
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Contact direct fields
            "id", "salutation", "firstName", "middleName", "lastName", "suffix",
            "title", "department", "description", "email", "phone", "mobile",
            "assistant", "assistantPhone", "assistantEmail", "status",
            "mailingStreet", "mailingStreet2", "mailingCity", "mailingStateProvince",
            "mailingPostalCode", "mailingCountry", "profilePictureUrl",
            
            // Partner related fields
            "partner.name", "partner.status", "partner.shortName", "partner.phone",
            "partner.website", "partner.address1City", "partner.address1Country",
            "partnerId", "partnerName", "partnerStatus", "partnerShortName"
        };
    }

    /// <summary>
    /// Gets the allowed search fields for Partner entity
    /// </summary>
    /// <returns>HashSet of allowed field names</returns>
    public static HashSet<string> GetPartnerAllowedFields()
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Partner direct fields
            "id", "name", "status", "newEngagement", "phone", "website", "shortName",
            "partnerOfficeId", "partnerOfficeName", "partnerCategoryId", "partnerCategoryName",
            "addressCity", "addressStateProvince", "addressPostalCode", "addressCountry",
            
            // Add other partner-specific fields as needed
            "description", "email", "createdDate", "modifiedDate"
        };
    }

    /// <summary>
    /// Gets the allowed search fields for Interaction entity
    /// </summary>
    /// <returns>HashSet of allowed field names</returns>
    public static HashSet<string> GetInteractionAllowedFields()
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Interaction direct fields
            "id", "contactId", "type", "date", "fromDate", "toDate", "description", "subject",
            
            // Contact related fields
            "contact.firstName", "contact.lastName", "contact.email", "contact.title",
            "contact.department", "contact.phone", "contact.mobile",
            "contactName", "contactFirstName", "contactLastName", "contactEmail",
            
            // Partner related fields (through contact)
            "contact.partner.name", "contact.partner.status", "contact.partner.shortName",
            "partnerName", "partnerStatus"
        };
    }

    /// <summary>
    /// Gets allowed search fields for a given entity type
    /// </summary>
    /// <param name="entityType">The entity type (e.g., "Contact", "Partner", "Interaction")</param>
    /// <returns>HashSet of allowed field names</returns>
    public static HashSet<string> GetAllowedFieldsForEntity(string entityType)
    {
        return entityType.ToLowerInvariant() switch
        {
            "contact" => GetContactAllowedFields(),
            "partner" => GetPartnerAllowedFields(),
            "interaction" => GetInteractionAllowedFields(),
            _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        };
    }
} 
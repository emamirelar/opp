using System;
using System.Collections.Generic;
using System.Linq;
using UNOPS.PAO.Models.Artifacts;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.Locations;

public class CountryModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Iso2Code { get; set; }

    public string? Continent { get; set; }
    public string? Region { get; set; }
    
    // Computed properties
    public int PartnerCount { get; set; }
    public int LiaisonOfficeCount { get; set; }
    
    // RBAC permissions
    public EntityPermissionsModel? Permissions { get; set; }

    /// <summary>
    /// Collection of artifacts associated with this country
    /// Automatically loaded via AutoMapper when Country entity is mapped
    /// </summary>
    public List<EntityArtifactModel> Artifacts { get; set; } = new List<EntityArtifactModel>();
    
    /// <summary>
    /// Organization unit hierarchy chain from root to the country's org unit
    /// Ordered from most general (root, e.g., OPS) to most specific (country's direct org unit, e.g., B5101)
    /// </summary>
    public List<OrganizationUnitHierarchyNode>? OrganizationUnitHierarchy { get; set; }
    
    /// <summary>
    /// Indicates if country has active UNCF (UN Cooperation Framework) metadata
    /// Populated by the mapper/service when loading country data
    /// </summary>
    public bool HasActiveUNCF { get; set; }
    
    /// <summary>
    /// Conditional tags based on country's current state for frontend display
    /// </summary>
    public List<EntityTagModel>? Tags => CalculateConditionalTags();
        
    /// <summary>
    /// Calculate conditional tags based on country's artifacts and documents for frontend display
    /// </summary>
    public List<EntityTagModel> CalculateConditionalTags()
    {
        var tags = new List<EntityTagModel>();
        
        // 1. Check for "World_Bank_Fragile_Situation" artifact
        var fragileSituationArtifact = Artifacts?.FirstOrDefault(a => 
            a.ArtifactTypeCode == "World_Bank_Fragile_Situation");
        
        if (IsBooleanArtifactTrue(fragileSituationArtifact))
        {
            tags.Add(new EntityTagModel 
            { 
                Tag = "Fragile State", 
                Color = "bg-red-100 text-red-800" 
            });
        }
        
        // 2. Check for "SIDS" artifact
        var sidsArtifact = Artifacts?.FirstOrDefault(a => 
            a.ArtifactTypeCode == "SIDS");
        
        if (IsBooleanArtifactTrue(sidsArtifact))
        {
            tags.Add(new EntityTagModel 
            { 
                Tag = "SIDS", 
                Color = "bg-yellow-100 text-yellow-800" 
            });
        }
        
        // 3. Check for "Host_Agreement" artifact (document type artifact)
        var hostAgreementArtifact = Artifacts?.FirstOrDefault(a => 
            a.ArtifactTypeCode == "Host_Agreement");
        
        if (hostAgreementArtifact != null)
        {
            tags.Add(new EntityTagModel 
            { 
                Tag = "HCA Present", 
                Color = "bg-green-100 text-green-800" 
            });
        }
        else
        {
            tags.Add(new EntityTagModel 
            { 
                Tag = "HCA Not Present", 
                Color = "bg-yellow-100 text-yellow-800" 
            });
        }
        
        return tags;
    }
    
    /// <summary>
    /// Helper method to check if an artifact's value is boolean true
    /// Handles both boolean objects and string representations
    /// </summary>
    private bool IsBooleanArtifactTrue(EntityArtifactModel? artifact)
    {
        if (artifact?.Value == null)
            return false;
        
        // Handle direct boolean value
        if (artifact.Value is bool boolValue)
            return boolValue;
        
        // Handle string representation (for backward compatibility)
        return artifact.Value.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}

/// <summary>
/// Represents a single node in the organization unit hierarchy chain
/// </summary>
public class OrganizationUnitHierarchyNode
{
    /// <summary>
    /// Organization unit ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Organization unit code
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization unit name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization unit type (e.g., "OrgUnit", "Region", "Global")
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization unit description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Parent organization unit ID (null for root)
    /// </summary>
    public int? ParentId { get; set; }
    
    /// <summary>
    /// Level in the hierarchy (0 = root, higher = deeper)
    /// </summary>
    public int Level { get; set; }
}

public class CountryFilterRequest : PaginationRequest
{
    public string? Name { get; set; }
    public string? Iso2Code { get; set; }
    public string? Status { get; set; }
    public bool IncludeCounts { get; set; } = true;
}

public class CountrySearchRequest
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public int? MinPartnerCount { get; set; }
    public int? MaxPartnerCount { get; set; }
    public int PageSize { get; set; } = 20;
    public int PageIndex { get; set; } = 1;
    public string? OrderBy { get; set; } = "Name";
    public bool Ascending { get; set; } = true;
}

/// <summary>
/// Lightweight country info for search results (only essential fields)
/// </summary>
public class CountrySearchInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Iso2Code { get; set; }
    public string? Continent { get; set; }
    public string? Region { get; set; }
}

/// <summary>
/// Represents a country search result with match context
/// </summary>
public class CountrySearchResultModel
{
    /// <summary>
    /// The country details (lightweight - only essential fields)
    /// </summary>
    public required CountrySearchInfo Country { get; set; }
    
    /// <summary>
    /// How this country matched the search criteria
    /// </summary>
    public required List<SearchMatchReason> MatchReasons { get; set; }
    
    /// <summary>
    /// Overall relevance score (higher = more relevant)
    /// Used for sorting results
    /// </summary>
    public decimal RelevanceScore { get; set; }
}

/// <summary>
/// Describes why a country matched the search
/// </summary>
public class SearchMatchReason
{
    /// <summary>
    /// Type of match (e.g., "CountryName", "ArtifactValue")
    /// </summary>
    public required string MatchType { get; set; }
    
    /// <summary>
    /// The artifact type that matched (if applicable)
    /// </summary>
    public string? ArtifactTypeCode { get; set; }
    
    /// <summary>
    /// Display name of the artifact type
    /// </summary>
    public string? ArtifactTypeName { get; set; }
    
    /// <summary>
    /// The specific value that matched
    /// </summary>
    public required string MatchedValue { get; set; }
    
    /// <summary>
    /// Highlighted version of the matched value (with search term emphasized)
    /// </summary>
    public string? HighlightedValue { get; set; }
    
    /// <summary>
    /// Category of the artifact (e.g., "Strategy", "Assessment", "Metric")
    /// </summary>
    public string? Category { get; set; }
}

/// <summary>
/// Request model for dynamic country search
/// </summary>
public class CountryDynamicSearchRequest
{
    /// <summary>
    /// Search term to match against country names and artifact values
    /// </summary>
    public required string SearchTerm { get; set; }
    
    /// <summary>
    /// Whether to include artifact-based matches
    /// </summary>
    public bool IncludeArtifacts { get; set; } = true;
    
    /// <summary>
    /// Specific artifact type codes to search (null = search all searchable artifacts)
    /// </summary>
    public List<string>? ArtifactTypeCodes { get; set; }
    
    /// <summary>
    /// Whether to use case-sensitive search
    /// </summary>
    public bool CaseSensitive { get; set; } = false;
    
    /// <summary>
    /// Whether to use exact match or partial match
    /// </summary>
    public bool ExactMatch { get; set; } = false;
    
    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int MaxResults { get; set; } = 50;
    
    /// <summary>
    /// Whether to highlight matched portions in results
    /// </summary>
    public bool HighlightMatches { get; set; } = true;
}

/// <summary>
/// Response model for dynamic country search with grouping
/// </summary>
public class CountryDynamicSearchResponse
{
    /// <summary>
    /// Total number of countries matched
    /// </summary>
    public int TotalMatches { get; set; }
    
    /// <summary>
    /// Countries grouped by match type
    /// </summary>
    public required CountrySearchGroups Groups { get; set; }
    
    /// <summary>
    /// All results flattened (for ungrouped display)
    /// </summary>
    public List<CountrySearchResultModel> AllResults { get; set; } = new List<CountrySearchResultModel>();
    
    /// <summary>
    /// Search metadata
    /// </summary>
    public required SearchMetadata Metadata { get; set; }
}

/// <summary>
/// Grouped search results by match type
/// </summary>
public class CountrySearchGroups
{
    /// <summary>
    /// Countries matched by name
    /// </summary>
    public List<CountrySearchResultModel> NameMatches { get; set; } = new List<CountrySearchResultModel>();
    
    /// <summary>
    /// Countries matched by region description
    /// </summary>
    public List<CountrySearchResultModel> RegionMatches { get; set; } = new List<CountrySearchResultModel>();
    
    /// <summary>
    /// Countries matched by continent description
    /// </summary>
    public List<CountrySearchResultModel> ContinentMatches { get; set; } = new List<CountrySearchResultModel>();
    
    /// <summary>
    /// Countries matched by artifact values, grouped by artifact type
    /// Key: Artifact type name, Value: List of matching countries
    /// </summary>
    public Dictionary<string, List<CountrySearchResultModel>> ArtifactMatches { get; set; } 
        = new Dictionary<string, List<CountrySearchResultModel>>();
}

/// <summary>
/// Search operation metadata
/// </summary>
public class SearchMetadata
{
    /// <summary>
    /// Search term used
    /// </summary>
    public required string SearchTerm { get; set; }
    
    /// <summary>
    /// Number of artifact types searched
    /// </summary>
    public int ArtifactTypesSearched { get; set; }
    
    /// <summary>
    /// Search execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }
    
    /// <summary>
    /// Whether results were cached
    /// </summary>
    public bool FromCache { get; set; }
}
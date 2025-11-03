using System;
using System.Collections.Generic;
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
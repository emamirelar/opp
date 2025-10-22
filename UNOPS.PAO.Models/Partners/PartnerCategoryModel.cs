using System;
using System.Collections.Generic;
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.Partners;

public class PartnerCategoryModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public string? Parent { get; set; }
    public string PartnerCategoryCode { get; set; }
    public string Status { get; set; }
    
    // Computed properties
    public int PartnerGroupCount { get; set; }
    public int TotalPartnerCount { get; set; }
    
    // Related data
    public List<PartnerGroupModel>? PartnerGroups { get; set; }
    
    // RBAC permissions
    public EntityPermissionsModel? Permissions { get; set; }
}

public class PartnerCategoryFilterRequest : PaginationRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
    public string? Parent { get; set; }
    public string? PartnerCategoryCode { get; set; }
    public string? Status { get; set; }
    public bool IncludePartnerGroups { get; set; } = false;
    public bool IncludePartnerCounts { get; set; } = true;
}

public class PartnerCategorySearchRequest
{
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public int? MinPartnerCount { get; set; }
    public int? MaxPartnerCount { get; set; }
    public bool IncludePartnerGroups { get; set; } = false;
    public int PageSize { get; set; } = 20;
    public int PageIndex { get; set; } = 1;
    public string? OrderBy { get; set; } = "Name";
    public bool Ascending { get; set; } = true;
}

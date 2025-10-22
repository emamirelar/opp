using System;
using System.Collections.Generic;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.PartnerTrees;

public class PartnerGroupModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public string? Parent { get; set; }
    public string PartnerGroupCode { get; set; }
    public string Status { get; set; }
    
    // Category relationship
    public int? PartnerCategoryId { get; set; }
    public string? PartnerCategoryCode { get; set; }
    public string? PartnerCategoryName { get; set; }
    
    // Computed properties
    public int PartnerCount { get; set; }
    public int TotalPartnerCount { get; set; }
    
    // Related data
    public PartnerCategoryModel? PartnerCategory { get; set; }
    public List<PartnerModel>? Partners { get; set; }
    
    // RBAC permissions
    public EntityPermissionsModel? Permissions { get; set; }
}

public class PartnerGroupFilterRequest : PaginationRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
    public string? Parent { get; set; }
    public string? PartnerGroupCode { get; set; }
    public int? PartnerCategoryId { get; set; }
    public string? PartnerCategoryCode { get; set; }
    public string? Status { get; set; }
    public bool IncludePartnerCategory { get; set; } = true;
    public bool IncludePartners { get; set; } = false;
    public bool IncludePartnerCounts { get; set; } = true;
}

public class PartnerGroupSearchRequest
{
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public int? PartnerCategoryId { get; set; }
    public string? PartnerCategoryCode { get; set; }
    public string? Status { get; set; }
    public int? MinPartnerCount { get; set; }
    public int? MaxPartnerCount { get; set; }
    public bool IncludePartnerCategory { get; set; } = true;
    public bool IncludePartners { get; set; } = false;
    public int PageSize { get; set; } = 20;
    public int PageIndex { get; set; } = 1;
    public string? OrderBy { get; set; } = "Name";
    public bool Ascending { get; set; } = true;
}

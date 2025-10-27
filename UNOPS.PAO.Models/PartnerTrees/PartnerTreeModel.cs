using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.PartnerTrees;

public class PartnerTreeModel
{
    public PartnerTreeDataModel Data { get; set; } = new PartnerTreeDataModel();
    public List<PartnerTreeModel> Children { get; set; } = new List<PartnerTreeModel>();
    
    // RBAC permissions
    public EntityPermissionsModel? Permissions { get; set; }
}

public class PartnerTreeDataModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public string? Parent { get; set; }
    public string Status { get; set; } // Add Status property
    public bool PartnerCategoryEditable { get; set; }
    public bool PartnerGroupEditable { get; set; }

    public int? PartnerCategoryId { get; set; }
    public string? PartnerCategoryCode { get; set; }
    public string? PartnerCategoryName { get; set; }
    public int? PartnerGroupId { get; set; }
    public string? PartnerGroupCode { get; set; }
    public string? PartnerGroupName { get; set; }
}
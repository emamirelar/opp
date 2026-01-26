namespace UNOPS.PAO.Models.OrganizationUnits;

/// <summary>
/// Model representing an entity user role assignment for an organization hierarchy.
/// Used to display users assigned to specific roles within an org unit.
/// </summary>
public class EntityUserRoleModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public int EntityRoleId { get; set; }
    public string? EntityRoleName { get; set; }
    public int OrganizationHierarchyId { get; set; }
    public string? OrganizationHierarchyName { get; set; }
}

/// <summary>
/// Response model for entity user roles grouped by entity role.
/// </summary>
public class EntityUserRolesByOrgUnitResponse
{
    public int OrganizationHierarchyId { get; set; }
    public string? OrganizationHierarchyName { get; set; }
    public string? OrganizationHierarchyType { get; set; }
    public List<EntityUserRoleGroupModel> RoleGroups { get; set; } = new();
}

/// <summary>
/// Represents a group of users assigned to a specific entity role.
/// </summary>
public class EntityUserRoleGroupModel
{
    public int EntityRoleId { get; set; }
    public string? EntityRoleName { get; set; }
    public string? EntityRoleCode { get; set; }
    public List<UserBasicModel> Users { get; set; } = new();
}

/// <summary>
/// Basic user information model.
/// </summary>
public class UserBasicModel
{
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }  // Standardized position title from personnel record
}


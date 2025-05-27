namespace UNOPS.PAO.Models;

/// <summary>
/// Generic entity permissions model that can be used across all entities
/// </summary>
public class EntityPermissionsModel
{
    /// <summary>
    /// Whether the user can read/view this entity
    /// </summary>
    public bool CanRead { get; set; }

    /// <summary>
    /// Whether the user can create new instances of this entity
    /// </summary>
    public bool CanCreate { get; set; }

    /// <summary>
    /// Whether the user can update/edit this entity
    /// </summary>
    public bool CanUpdate { get; set; }

    /// <summary>
    /// Whether the user can delete this entity
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// Additional metadata about permissions (optional)
    /// </summary>
    public string? PermissionSource { get; set; }

    /// <summary>
    /// Any additional permission-related notes or constraints (optional)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Creates a default permission set with all permissions disabled
    /// </summary>
    public static EntityPermissionsModel None => new()
    {
        CanRead = false,
        CanCreate = false,
        CanUpdate = false,
        CanDelete = false
    };

    /// <summary>
    /// Creates a permission set with all permissions enabled
    /// </summary>
    public static EntityPermissionsModel All => new()
    {
        CanRead = true,
        CanCreate = true,
        CanUpdate = true,
        CanDelete = true
    };

    /// <summary>
    /// Creates a read-only permission set
    /// </summary>
    public static EntityPermissionsModel ReadOnly => new()
    {
        CanRead = true,
        CanCreate = false,
        CanUpdate = false,
        CanDelete = false
    };
} 
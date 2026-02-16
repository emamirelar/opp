namespace UNOPS.PAO.Models.Shared;

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
    /// List of field names that the user can edit (based on PropertyFilter CanUpdate)
    /// Empty list means user cannot edit any fields, null means no field-level restrictions
    /// </summary>
    public List<string>? CanEditFields { get; set; }

    /// <summary>
    /// Whether the user can activate this entity (based on mandatory fields completion and permissions)
    /// Only applicable to certain entities like Partner
    /// </summary>
    public bool? CanActivate { get; set; }

    /// <summary>
    /// Whether the user can close this entity
    /// Only applicable to certain entities like Partner
    /// </summary>
    public bool? CanClose { get; set; }

    /// <summary>
    /// Whether the user can archive this entity
    /// Only applicable to certain entities like Partner
    /// </summary>
    public bool? CanArchive { get; set; }

    /// <summary>
    /// Whether the user can approve this entity
    /// Only applicable to certain entities like Partner
    /// </summary>
    public bool? CanApprove { get; set; }

    /// <summary>
    /// Whether the user can unapprove this entity
    /// Only applicable to certain entities like Partner
    /// </summary>
    public bool? CanUnapprove { get; set; }


    /// <summary>
    /// Whether the user can export data (PARTNER_GLOB_ADMIN only)
    /// </summary>
    public bool CanExport { get; set; }

    /// <summary>
    /// Whether the user can import data (PARTNER_GLOB_ADMIN only)
    /// </summary>
    public bool CanImport { get; set; }

    /// <summary>
    /// Additional metadata about permissions (optional)
    /// </summary>
    public string? PermissionSource { get; set; }

    /// <summary>
    /// Any additional permission-related notes or constraints (optional)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether the entity is in an immutable state (e.g., after Go/No-Go decision for opportunities).
    /// When true, all modification operations are blocked regardless of other permissions.
    /// Frontend uses this to show "Historic Artifact" badge or disable edit controls.
    /// </summary>
    public bool? IsImmutable { get; set; }

    /// <summary>
    /// Whether the entity is currently in an approval workflow (Approval Pending status).
    /// When true, the entity cannot be edited until the approval process completes.
    /// Frontend uses this to show "Approval Pending" indicator and disable edit controls.
    /// </summary>
    public bool? IsApprovalPending { get; set; }

    /// <summary>
    /// Creates a default permission set with all permissions disabled
    /// </summary>
    public static EntityPermissionsModel None => new()
    {
        CanRead = false,
        CanCreate = false,
        CanUpdate = false,
        CanDelete = false,
        CanActivate = false,
        CanClose = false,
        CanArchive = false,
        CanApprove = false
    };

    /// <summary>
    /// Creates a permission set with all permissions enabled
    /// </summary>
    public static EntityPermissionsModel All => new()
    {
        CanRead = true,
        CanCreate = true,
        CanUpdate = true,
        CanDelete = true,
        CanActivate = true,
        CanClose = true,
        CanArchive = true,
        CanApprove = true
    };

    /// <summary>
    /// Creates a read-only permission set
    /// </summary>
    public static EntityPermissionsModel ReadOnly => new()
    {
        CanRead = true,
        CanCreate = false,
        CanUpdate = false,
        CanDelete = false,
        CanActivate = false,
        CanClose = false,
        CanArchive = false,
        CanApprove = false
    };
} 
using System;

namespace UNOPS.PAO.RBAC.Attributes;

/// <summary>
/// Unified RBAC attribute for declarative security enforcement
/// Supports all CRUD operations and custom actions with flexible configuration
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RBACAttribute : Attribute
{
    /// <summary>
    /// The security action to check (read, create, update, delete, or custom action)
    /// </summary>
    public string Action { get; set; }
    
    /// <summary>
    /// Entity name for permission checks. If not specified, auto-detected from manager class name
    /// </summary>
    public string? Entity { get; set; }
    
    /// <summary>
    /// Whether to apply row-level filtering to queries and results
    /// </summary>
    public bool ApplyRowFiltering { get; set; } = true;
    
    /// <summary>
    /// Whether to apply column-level filtering to returned entities
    /// </summary>
    public bool ApplyColumnFiltering { get; set; } = true;
    
    /// <summary>
    /// Whether to check access to specific entity instances (for update/delete operations)
    /// </summary>
    public bool RequireEntityAccess { get; set; } = false;
    
    /// <summary>
    /// Name of the method parameter that contains the entity object
    /// </summary>
    public string? EntityParameterName { get; set; }
    
    /// <summary>
    /// Name of the method parameter that contains the entity ID
    /// </summary>
    public string? EntityIdParameterName { get; set; }
    
    /// <summary>
    /// Optional reason for debugging/documentation purposes
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Creates an RBAC attribute with the specified action
    /// </summary>
    /// <param name="action">The security action (read, create, update, delete, or custom)</param>
    public RBACAttribute(string action)
    {
        Action = action;
        
        // Set sensible defaults based on action
        switch (action.ToLower())
        {
            case "create":
                ApplyRowFiltering = false; // Not applicable for create
                RequireEntityAccess = false;
                break;
                
            case "update":
            case "delete":
                RequireEntityAccess = true; // Usually need to check specific entity access
                break;
                
            case "read":
            default:
                // Use default values
                break;
        }
    }
}

/// <summary>
/// Attribute to skip RBAC security checks for internal system operations
/// Use with caution - only for methods that shouldn't be subject to user permissions
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SkipRBACAttribute : Attribute
{
    /// <summary>
    /// Reason for skipping RBAC (for documentation/debugging)
    /// </summary>
    public string? Reason { get; set; }

    public SkipRBACAttribute(string? reason = null)
    {
        Reason = reason;
    }
} 
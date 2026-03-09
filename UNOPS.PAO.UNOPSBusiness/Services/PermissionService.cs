using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Authorization;
using System.Text.Json;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace UNOPS.PAO.UNOPSBusiness.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly UNOPSAppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionService(UNOPSAppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string entity, string action)
        {
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            // Get user roles from claims
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!userRoles.Any())
            {
                return false;
            }

            // Query EntityPermissions table for matching entity and user roles
            var permissions = await _context.EntityPermissions
                .Where(ep => ep.Entity.ToLower() == entity.ToLower() && userRoles.Contains(ep.Role))
                .ToListAsync();

            if (!permissions.Any())
            {
                return false;
            }

            // Get the permission from the highest priority role
            var highestPriorityPermission = GetHighestPriorityPermission(permissions, userRoles);
            
            if (highestPriorityPermission == null)
            {
                return false;
            }

            // Check the specific action permission using the highest priority role
            bool hasPermission = action.ToLower() switch
            {
                "read" => highestPriorityPermission.CanRead,
                "create" => highestPriorityPermission.CanCreate,
                "update" => highestPriorityPermission.CanUpdate,
                "delete" => highestPriorityPermission.CanDelete,
                _ => false
            };

            return hasPermission;
        }

        public async Task<object> ApplyAccessControlFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string action, string entityName) where T : class
        {
            if (user == null || !user.Identity.IsAuthenticated)
                return new List<T>(); // No access for unauthenticated users

            // Get entity permissions for this user
            var permissions = await GetEntityPermissionsAsync(user, entityName);
            
            if (!permissions.Any())
                return new List<T>(); // No permissions found

            // Get user roles from claims
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Get the permission from the highest priority role
            var highestPriorityPermission = GetHighestPriorityPermission(permissions, userRoles);
            
            if (highestPriorityPermission == null)
                return new List<T>(); // No permissions found

            // Check if user has permission for this action using the highest priority role
            bool hasPermission = action.ToLower() switch
            {
                "read" => highestPriorityPermission.CanRead,
                "create" => highestPriorityPermission.CanCreate,
                "update" => highestPriorityPermission.CanUpdate,
                "delete" => highestPriorityPermission.CanDelete,
                _ => false
            };

            string rowFilterConditions = null;
            var permittedColumns = new HashSet<string>();

            if (hasPermission)
            {
                // Get row filter conditions if available from the highest priority permission
                if (!string.IsNullOrEmpty(highestPriorityPermission.RowFilter))
                {
                    try
                    {
                        var rowFilterJson = JsonSerializer.Deserialize<Dictionary<string, string>>(highestPriorityPermission.RowFilter);
                        if (rowFilterJson != null && rowFilterJson.TryGetValue($"Can{char.ToUpper(action[0])}{action.Substring(1).ToLower()}", out var filter))
                        {
                            if (!string.IsNullOrEmpty(filter))
                            {
                                rowFilterConditions = filter;
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Invalid JSON, skip row filtering for this permission
                    }
                }

                // Get column filter restrictions if available from the highest priority permission
                if (!string.IsNullOrEmpty(highestPriorityPermission.PropertyFilter))
                {
                    try
                    {
                        var propertyFilterJson = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(highestPriorityPermission.PropertyFilter);
                        if (propertyFilterJson != null && propertyFilterJson.TryGetValue($"Can{char.ToUpper(action[0])}{action.Substring(1).ToLower()}", out var columns))
                        {
                            foreach (var column in columns)
                            {
                                permittedColumns.Add(column);
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Invalid JSON, skip column filtering for this permission
                    }
                }
            }

            if (!hasPermission)
                return new List<T>(); // No permission for this action

            // Apply row filtering if conditions exist
            if (!string.IsNullOrEmpty(rowFilterConditions))
            {
                try
                {
                    // Get current user information for parameter substitution
                    var currentUserId = GetCurrentUserId(user);
                    var userOrgUnit = await GetUserOrgUnitAsync(user);

                    // Replace parameter placeholders with actual values
                    var processedFilter = rowFilterConditions
                        .Replace("@currentUserId", currentUserId.ToString())
                        .Replace("@userOrgUnit", $"\"{userOrgUnit}\""); // Wrap in quotes for string comparison

                    // Apply dynamic LINQ where clause
                    query = query.Where(processedFilter);
                }
                catch (Exception ex)
                {
                    // If row filtering fails, log the error and proceed without row filtering
                    // In production, you might want to log this error
                    // For now, we'll proceed with the original query
                }
            }

            // ⚡ PERFORMANCE OPTIMIZATION: Only materialize the query if column filtering is needed
            // This allows pagination to happen at the database level when no column filtering is required
            if (!permittedColumns.Any())
            {
                // No column filtering - return the IQueryable to allow database-level pagination
                return query;
            }

            // Column filtering is needed - must materialize the query
            List<T> data;
            try
            {
                // Try async first (for EF queries)
                data = await query.ToListAsync();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("IAsyncEnumerable") || ex.Message.Contains("Unable to cast"))
            {
                // Fallback to synchronous execution (for LINQ to Objects after dynamic filtering)
                data = query.ToList();
            }
            catch (InvalidCastException)
            {
                // Another type of exception that can occur with non-EF queries
                data = query.ToList();
            }

            // Apply column filtering
            // Always ensure Id and permissions are included, even if not in PropertyFilter
            permittedColumns.Add("Id");
            permittedColumns.Add("permissions");
            
            return await ApplyColumnFilteringToDataGeneric(data, permittedColumns);
        }

        /// <summary>
        /// Gets entity permissions for the current entity and user roles from database
        /// </summary>
        private async Task<List<EntityPermission>> GetEntityPermissionsAsync(ClaimsPrincipal user, string entityName)
        {
            if (user == null || !user.Identity.IsAuthenticated)
                return new List<EntityPermission>();

            // Get user roles from claims
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!userRoles.Any())
                return new List<EntityPermission>();

            // Query EntityPermissions table for matching entity and user roles
            var permissions = await _context.EntityPermissions
                .Where(ep => ep.Entity.ToLower() == entityName.ToLower() && userRoles.Contains(ep.Role))
                .ToListAsync();

            return permissions;
        }

        /// <summary>
        /// Gets the current user ID from claims for row filtering
        /// </summary>
        private int GetCurrentUserId(ClaimsPrincipal user)
        {
            if (user == null) return 0;
            
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             user.FindFirst("sub")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Gets the user's organization unit for row filtering
        /// </summary>
        public async Task<string> GetUserOrgUnitAsync(ClaimsPrincipal user)
        {
            if (user == null) return string.Empty;

            // Try multiple ways to get the current user's email from claims as fallback
            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ??
                            user.FindFirst("email")?.Value ??
                            user.Identity?.Name;

            // Extract email from identity provider format if needed
            // Format: "securetoken.google.com/unops-opportunityplus-dev:email@domain.com"
            if (!string.IsNullOrEmpty(userEmail) && userEmail.Contains(':'))
            {
                var emailParts = userEmail.Split(':');
                if (emailParts.Length > 1)
                {
                    userEmail = emailParts[emailParts.Length - 1]; // Take the last part after colon
                }
            }

            if (string.IsNullOrEmpty(userEmail))
            {
                return string.Empty;
            }

            try
            {   
                // Look up user's assigned org unit from database
                var userInfo = await _context.UserProfile
                    .Where(u => u.UserEmail.ToLower() == userEmail.ToLower())
                    .Select(u => u.OrgUnit)
                    .FirstOrDefaultAsync();
                    
                return userInfo ?? string.Empty;
            }
            catch (Exception)
            {
                // If any error occurs, return empty string
                return string.Empty;
            }
        }

        /// <summary>
        /// Applies column filtering to data by keeping only permitted columns (generic version)
        /// </summary>
        /// <param name="data">The data to filter</param>
        /// <param name="permittedColumns">Set of column names to keep</param>
        /// <returns>Data with only permitted columns, maintaining original type</returns>
        private async Task<List<T>> ApplyColumnFilteringToDataGeneric<T>(List<T> data, HashSet<string> permittedColumns)
        {
            if (data == null)
                return data;

            // If no permitted columns specified, return all data (backward compatibility)
            if (!permittedColumns.Any())
                return data;

            try
            {
                // Configure JsonSerializer to handle circular references and respect JsonIgnore attributes
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = false
                };

                // Serialize to JSON, filter properties, then deserialize back to List<T>
                var jsonString = JsonSerializer.Serialize(data, options);
                var jsonDocument = JsonDocument.Parse(jsonString);
                
                var filteredJson = FilterJsonProperties(jsonDocument.RootElement, permittedColumns);
                
                var filteredData = JsonSerializer.Deserialize<List<T>>(filteredJson, options);
                return filteredData ?? data;
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON processing failed: {ex.Message}");
                // If JSON processing fails, return original data
                return data;
            }
        }

        /// <summary>
        /// Helper method to filter JSON properties to keep only permitted columns
        /// </summary>
        private string FilterJsonProperties(JsonElement element, HashSet<string> permittedColumns)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var filteredObject = new Dictionary<string, object>();
                
                foreach (var property in element.EnumerateObject())
                {
                    // Check both exact match and case-insensitive match to handle PascalCase vs camelCase
                    if (permittedColumns.Contains(property.Name) || 
                        permittedColumns.Any(col => string.Equals(col, property.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        filteredObject[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                    }
                }
                
                // Configure JsonSerializer options for consistency
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                
                return JsonSerializer.Serialize(filteredObject, options);
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                var filteredArray = new List<object>();
                
                foreach (var item in element.EnumerateArray())
                {
                    var filteredItem = FilterJsonProperties(item, permittedColumns);
                    filteredArray.Add(JsonSerializer.Deserialize<object>(filteredItem));
                }
                
                // Configure JsonSerializer options for consistency
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                
                return JsonSerializer.Serialize(filteredArray, options);
            }
            
            return element.GetRawText();
        }

        /// <summary>
        /// Checks if user can perform the specified action on the entity
        /// For Opportunity entities with "update" action, also checks if user is a team member (stakeholder)
        /// </summary>
        public async Task<bool> CanPerformActionAsync(string entityName, string action, ClaimsPrincipal user, object? entity = null)
        {
            // First check entity-level permissions
            var hasEntityPermission = await HasPermissionAsync(user, entityName, action);
            
            if (hasEntityPermission)
            {
                return true;
            }
            
            // For Opportunity entities with "update" action, also check if user is a team member (stakeholder)
            // Team members can update the opportunity even if their role doesn't have global update permission
            if (entityName.Equals("Opportunity", StringComparison.OrdinalIgnoreCase) && action.ToLower() == "update")
            {
                // Try to get the opportunity ID from the HTTP context route values
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    // Get the opportunity ID from the route
                    if (httpContext.Request.RouteValues.TryGetValue("id", out var idValue) && 
                        int.TryParse(idValue?.ToString(), out int opportunityId) && 
                        opportunityId > 0)
                    {
                        // Check if user is a team member (stakeholder) on this opportunity
                        var isTeamMember = await IsOpportunityTeamMemberAsync(opportunityId);
                        if (isTeamMember)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets all permissions the current user has for a given entity
        /// </summary>
        public async Task<object> GetEntityPermissionsAsync(string entityName, object? entity = null)
        {
            // Get the current user from HttpContext
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;
            
            if (user == null)
            {
                return new
                {
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false,
                    CanExport = false,
                    CanImport = false
                };
            }
            
            return new
            {
                CanRead = await HasPermissionAsync(user, entityName, "read"),
                CanCreate = await HasPermissionAsync(user, entityName, "create"),
                CanUpdate = await HasPermissionAsync(user, entityName, "update"),
                CanDelete = await HasPermissionAsync(user, entityName, "delete"),
                CanExport = CanExport(user),
                CanImport = CanImport(user)
            };
        }

        /// <summary>
        /// Gets entity permissions for a specific instance, including team-based permissions for Opportunity
        /// For Opportunity entities, also checks if user is a team member (stakeholder)
        /// </summary>
        public async Task<object> GetEntityInstancePermissionsAsync(string entityName, int entityId)
        {
            // Get the current user from HttpContext
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;
            
            if (user == null)
            {
                return new
                {
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false,
                    CanExport = false,
                    CanImport = false,
                    IsTeamMember = false
                };
            }
            
            // Get base permissions from role
            var canRead = await HasPermissionAsync(user, entityName, "read");
            var canCreate = await HasPermissionAsync(user, entityName, "create");
            var canUpdate = await HasPermissionAsync(user, entityName, "update");
            var canDelete = await HasPermissionAsync(user, entityName, "delete");
            var isTeamMember = false;
            
            // For Opportunity, check if user is a team member (stakeholder)
            // Team members get edit access even if their role doesn't have update permission
            if (entityName.Equals("Opportunity", StringComparison.OrdinalIgnoreCase) && entityId > 0)
            {
                isTeamMember = await IsOpportunityTeamMemberAsync(entityId);
                
                // If user is a team member, grant update permission
                if (isTeamMember && !canUpdate)
                {
                    canUpdate = true;
                }
            }
            
            return new
            {
                CanRead = canRead,
                CanCreate = canCreate,
                CanUpdate = canUpdate,
                CanDelete = canDelete,
                CanExport = CanExport(user),
                CanImport = CanImport(user),
                IsTeamMember = isTeamMember
            };
        }
        
        /// <summary>
        /// Checks if the current user is a stakeholder (team member) of an Opportunity.
        /// This includes:
        /// 1. Opportunity Collaborators (Opportunity Development Team - always have edit access)
        /// 2. Directly assigned stakeholders
        /// 3. Users related through OrgUnit role assignments (auto-populated)
        /// </summary>
        /// <param name="opportunityId">The opportunity ID to check</param>
        /// <returns>True if the user is a team member</returns>
        public async Task<bool> IsOpportunityTeamMemberAsync(int opportunityId)
        {
            // Get the current user from HttpContext
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;
            
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }
            
            // Get current user ID from claims
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             user.FindFirst("sub")?.Value;
            
            if (!int.TryParse(userIdClaim, out var currentUserId) || currentUserId <= 0)
            {
                return false;
            }
            
            try
            {
                // 1. Check if user is an Opportunity Collaborator (Opportunity Development Team)
                // Collaborators always have edit access to all opportunity fields
                var isCollaborator = await _context.Set<UNOPS.PAO.Domain.Entities.OpportunityCollaborator>()
                    .AnyAsync(oc => oc.OpportunityId == opportunityId && oc.UserId == currentUserId);
                
                if (isCollaborator)
                {
                    return true;
                }
                
                // 2. Check if user is directly assigned as a stakeholder (UserId is set, OrganizationHierarchyId is null)
                var isDirectStakeholder = await _context.Set<UNOPS.PAO.Domain.Entities.OpportunityStakeholder>()
                    .AnyAsync(os => os.OpportunityId == opportunityId 
                        && os.UserId == currentUserId 
                        && os.OrganizationHierarchyId == null);
                
                if (isDirectStakeholder)
                {
                    return true;
                }
                
                // 3. Check if user is related through OrgUnit role assignments (auto-populated stakeholders)
                // Get all auto-populated stakeholders for this opportunity (OrganizationHierarchyId is set)
                var autoPopulatedStakeholders = await _context.Set<UNOPS.PAO.Domain.Entities.OpportunityStakeholder>()
                    .Where(os => os.OpportunityId == opportunityId && os.OrganizationHierarchyId.HasValue)
                    .Select(os => new { os.EntityRoleId, os.OrganizationHierarchyId })
                    .ToListAsync();
                
                if (autoPopulatedStakeholders.Any())
                {
                    // Check if current user has an EntityUserRole that matches any auto-populated stakeholder
                    // EntityUserRole links users to roles for specific entities (in this case, OrganizationHierarchy)
                    foreach (var autoStakeholder in autoPopulatedStakeholders)
                    {
                        var hasMatchingRole = await _context.Set<UNOPS.PAO.Domain.Entities.EntityUserRole>()
                            .AnyAsync(eur => 
                                eur.UserId == currentUserId 
                                && eur.EntityRoleId == autoStakeholder.EntityRoleId
                                && eur.EntityType == "OrganizationHierarchy"
                                && eur.EntityId == autoStakeholder.OrganizationHierarchyId.Value
                                && !eur.IsDeleted);
                        
                        if (hasMatchingRole)
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking opportunity team membership: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if the user has access to a specific entity instance using row filtering for the specified action
        /// </summary>
        public async Task<bool> HasInstanceAccessAsync(string entityName, object entity, ClaimsPrincipal user, string action)
        {
            if (user == null || !user.Identity.IsAuthenticated || entity == null)
            {
                return false;
            }

            // Get entity permissions for this user (use the private method that returns List<EntityPermission>)
            var permissions = await GetEntityPermissionsAsync(user, entityName);
            
            if (!permissions.Any())
            {
                return false;
            }

            // Get user roles from claims
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Get the permission from the highest priority role
            var highestPriorityPermission = GetHighestPriorityPermission(permissions, userRoles);
            
            if (highestPriorityPermission == null)
            {
                return false;
            }

            // Check if user has the basic permission for this action using the highest priority role
            bool hasBasicPermission = action.ToLower() switch
            {
                "read" => highestPriorityPermission.CanRead,
                "create" => highestPriorityPermission.CanCreate,
                "update" => highestPriorityPermission.CanUpdate,
                "delete" => highestPriorityPermission.CanDelete,
                _ => false
            };

            // For Opportunity entities with "update" action, also check if user is a stakeholder (team member)
            // Team members can update the opportunity even if their role doesn't have global update permission
            if (!hasBasicPermission && entityName.Equals("Opportunity", StringComparison.OrdinalIgnoreCase) && action.ToLower() == "update")
            {
                // Try to get the opportunity ID from the entity
                var idProperty = entity.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(entity);
                    if (idValue != null && int.TryParse(idValue.ToString(), out int opportunityId) && opportunityId > 0)
                    {
                        // Check if user is a team member (stakeholder) on this opportunity
                        var isTeamMember = await IsOpportunityTeamMemberAsync(opportunityId);
                        if (isTeamMember)
                        {
                            hasBasicPermission = true;
                        }
                    }
                }
            }

            if (!hasBasicPermission)
                return false;

            // If there's no row filter, user has access to all instances
            if (string.IsNullOrEmpty(highestPriorityPermission.RowFilter))
            {
                return true;
            }

            // Determine which permission property to check based on action
            var actionKey = $"Can{char.ToUpper(action[0])}{action.Substring(1).ToLower()}";

            try
            {
                // Parse row filter conditions from the highest priority permission
                var rowFilterJson = JsonSerializer.Deserialize<Dictionary<string, string>>(highestPriorityPermission.RowFilter);
                if (rowFilterJson != null && rowFilterJson.TryGetValue(actionKey, out var filter))
                {
                    if (string.IsNullOrEmpty(filter))
                    {
                        // Empty filter means access to all instances
                        return true;
                    }

                    // Apply row filter to check if this specific instance is accessible
                    var hasAccess = await CheckRowFilterCondition(entity, filter, user);
                    return hasAccess;
                }
                else
                {
                    // No specific filter for this action, assume access granted
                    return true;
                }
            }
            catch (JsonException)
            {
                // Invalid JSON, assume access granted to avoid breaking functionality
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if an entity instance matches the row filter condition
        /// </summary>
        private async Task<bool> CheckRowFilterCondition(object entity, string filterCondition, ClaimsPrincipal user)
        {
            try
            {
                // Get current user information for parameter substitution
                var currentUserId = GetCurrentUserId(user);
                var userOrgUnit = await GetUserOrgUnitAsync(user);

                // Replace parameter placeholders with actual values
                var processedFilter = filterCondition
                    .Replace("@currentUserId", currentUserId.ToString())
                    .Replace("@userOrgUnit", $"\"{userOrgUnit}\""); // Wrap in quotes for string comparison

                // Instead of converting to dictionary, work directly with the entity type
                // This preserves navigation properties and allows proper LINQ evaluation
                var entityType = entity.GetType();

                // Create a generic method to handle the dynamic LINQ evaluation
                var method = typeof(PermissionService).GetMethod(nameof(EvaluateFilterOnEntity), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var genericMethod = method.MakeGenericMethod(entityType);
                
                var result = (bool)genericMethod.Invoke(this, new object[] { entity, processedFilter });
                return result;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Row filter evaluation failed: {ex.Message}");
                // If filtering fails, assume no access for security
                return false;
            }
        }

        /// <summary>
        /// Evaluates a filter condition on a specific entity type using Dynamic LINQ
        /// </summary>
        private bool EvaluateFilterOnEntity<T>(T entity, string filterCondition)
        {
            try
            {
                // Create a queryable from the single entity with proper type
                var queryable = new[] { entity }.AsQueryable();
                
                // Apply the filter using Dynamic LINQ
                var filteredResults = queryable.Where(filterCondition).ToList();
                
                // If the entity passes the filter, it will be in the results
                return filteredResults.Any();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Dynamic LINQ evaluation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the permission from the highest priority role when user has multiple roles.
        /// Role hierarchy (highest to lowest priority):
        /// 1. PARTNER_GLOB_ADMIN - Full access
        /// 2. ORG_UNIT_ADMIN - Admin access for specific org units
        /// 3. PARTNER_USER - Standard partnership user
        /// 4. UNOPS_GEN_USER - Limited general user access
        /// </summary>
        /// <param name="permissions">All permissions for the user's roles</param>
        /// <param name="userRoles">User's roles</param>
        /// <returns>The permission from the highest priority role</returns>
        private EntityPermission GetHighestPriorityPermission(List<EntityPermission> permissions, List<string> userRoles)
        {
            // Define role hierarchy (order matters - first = highest priority)
            var roleHierarchy = new List<string>
            {
                "PARTNER_GLOB_ADMIN",   // Highest priority - full access
                "ORG_UNIT_ADMIN",       // Second - admin access for org units
                "PARTNER_USER",         // Third - standard partnership user
                "UNOPS_GEN_USER"        // Lowest priority - limited access
            };

            // Find the highest priority role that the user has permissions for
            foreach (var role in roleHierarchy)
            {
                if (userRoles.Contains(role))
                {
                    var permission = permissions.FirstOrDefault(p => p.Role == role);
                    if (permission != null)
                    {
                        return permission;
                    }
                }
            }

            // Fallback: return the first permission if no role matches hierarchy
            return permissions.FirstOrDefault();
        }

        /// <summary>
        /// Gets the highest priority role from user's roles based on the role hierarchy
        /// </summary>
        /// <param name="userRoles">User's roles</param>
        /// <returns>The highest priority role name</returns>
        private string GetHighestPriorityRole(List<string> userRoles)
        {
            // Define role hierarchy (order matters - first = highest priority)
            var roleHierarchy = new List<string>
            {
                "PARTNER_GLOB_ADMIN",   // Highest priority - full access
                "ORG_UNIT_ADMIN",       // Second - admin access for org units  
                "PARTNER_USER",         // Third - standard partnership user
                "UNOPS_GEN_USER"        // Lowest priority - limited access
            };

            // Return the first role in hierarchy that the user has
            foreach (var role in roleHierarchy)
            {
                if (userRoles.Contains(role))
                {
                    return role;
                }
            }

            // Fallback to first user role if none match hierarchy
            return userRoles.FirstOrDefault();
        }

        /// <summary>
        /// Gets the effective role for a user based on role hierarchy.
        /// This is useful for debugging and logging which role is being used for permission decisions.
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>The effective role name</returns>
        public string GetEffectiveRole(ClaimsPrincipal user)
        {
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            // Get user roles from claims
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!userRoles.Any())
            {
                return null;
            }

            return GetHighestPriorityRole(userRoles);
        }

        public bool CanExport(ClaimsPrincipal user)
        {
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            // Only PARTNER_GLOB_ADMIN can export
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return userRoles.Contains("PARTNER_GLOB_ADMIN");
        }

        public bool CanImport(ClaimsPrincipal user)
        {
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            // Only PARTNER_GLOB_ADMIN can import
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return userRoles.Contains("PARTNER_GLOB_ADMIN");
        }
    }
}
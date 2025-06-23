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

            // Check the specific action permission
            foreach (var permission in permissions)
            {
                bool hasPermission = action.ToLower() switch
                {
                    "read" => permission.CanRead,
                    "create" => permission.CanCreate,
                    "update" => permission.CanUpdate,
                    "delete" => permission.CanDelete,
                    _ => false
                };

                if (hasPermission)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<object> ApplyAccessControlFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string action, string entityName) where T : class
        {
            if (user == null || !user.Identity.IsAuthenticated)
                return new List<T>(); // No access for unauthenticated users

            // Get entity permissions for this user
            var permissions = await GetEntityPermissionsAsync(user, entityName);
            
            if (!permissions.Any())
                return new List<T>(); // No permissions found

            // Check if user has permission for this action
            bool hasPermission = false;
            string rowFilterConditions = null;
            var permittedColumns = new HashSet<string>();

            foreach (var permission in permissions)
            {
                bool actionAllowed = action.ToLower() switch
                {
                    "read" => permission.CanRead,
                    "create" => permission.CanCreate,
                    "update" => permission.CanUpdate,
                    "delete" => permission.CanDelete,
                    _ => false
                };

                if (actionAllowed)
                {
                    hasPermission = true;
                    
                    // Get row filter conditions if available
                    if (!string.IsNullOrEmpty(permission.RowFilter))
                    {
                        try
                        {
                            var rowFilterJson = JsonSerializer.Deserialize<Dictionary<string, string>>(permission.RowFilter);
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

                    // Get column filter restrictions if available
                    if (!string.IsNullOrEmpty(permission.PropertyFilter))
                    {
                        try
                        {
                            var propertyFilterJson = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(permission.PropertyFilter);
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

            // Execute the query to get data
            List<T> data;
            try
            {
                // Try async first (for EF queries)
                data = await query.ToListAsync();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("IAsyncEnumerable"))
            {
                // Fallback to synchronous execution (for LINQ to Objects after dynamic filtering)
                data = query.ToList();
            }

            // Apply column filtering if there are permitted columns
            if (permittedColumns.Any())
            {
                // Always ensure Id and permissions are included, even if not in PropertyFilter
                permittedColumns.Add("Id");
                permittedColumns.Add("permissions");
                
                return await ApplyColumnFilteringToDataGeneric(data, permittedColumns);
            }

            return data;
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

            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? 
                           user.FindFirst("email")?.Value;
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return string.Empty;
            }

            try
            {   
                // Look up user's assigned org unit from database
                var userInfo = await _context.UserInfos
                    .Where(u => u.UserEmail.ToLower() == userEmail.ToLower() && !u.IsDeleted)
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
        /// </summary>
        public async Task<bool> CanPerformActionAsync(string entityName, string action, ClaimsPrincipal user, object entity = null)
        {
            return await HasPermissionAsync(user, entityName, action);
        }
        
        /// <summary>
        /// Gets all permissions the current user has for a given entity
        /// </summary>
        public async Task<object> GetEntityPermissionsAsync(string entityName, object entity = null)
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
                    CanDelete = false
                };
            }
            
            return new
            {
                CanRead = await HasPermissionAsync(user, entityName, "read"),
                CanCreate = await HasPermissionAsync(user, entityName, "create"),
                CanUpdate = await HasPermissionAsync(user, entityName, "update"),
                CanDelete = await HasPermissionAsync(user, entityName, "delete")
            };
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

            // Determine which permission property to check based on action
            var actionKey = $"Can{char.ToUpper(action[0])}{action.Substring(1).ToLower()}";

            // Check if any role has row filtering conditions for the specified action
            foreach (var permission in permissions)
            {
                // Check if user has the basic permission for this action
                bool hasBasicPermission = action.ToLower() switch
                {
                    "read" => permission.CanRead,
                    "create" => permission.CanCreate,
                    "update" => permission.CanUpdate,
                    "delete" => permission.CanDelete,
                    _ => false
                };

                if (!hasBasicPermission)
                    continue;

                // If there's no row filter, user has access to all instances
                if (string.IsNullOrEmpty(permission.RowFilter))
                {
                    return true;
                }

                try
                {
                    // Parse row filter conditions
                    var rowFilterJson = JsonSerializer.Deserialize<Dictionary<string, string>>(permission.RowFilter);
                    if (rowFilterJson != null && rowFilterJson.TryGetValue(actionKey, out var filter))
                    {
                        if (string.IsNullOrEmpty(filter))
                        {
                            // Empty filter means access to all instances
                            return true;
                        }

                        // Apply row filter to check if this specific instance is accessible
                        var hasAccess = await CheckRowFilterCondition(entity, filter, user);
                        if (hasAccess)
                        {
                            return true;
                        }
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
    }
}
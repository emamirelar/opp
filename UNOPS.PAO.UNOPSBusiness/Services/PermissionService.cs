using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Authorization;
using System.Text.Json;
using System.Linq.Dynamic.Core;

namespace UNOPS.PAO.UNOPSBusiness.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly UNOPSAppDbContext _context;

        public PermissionService(UNOPSAppDbContext context)
        {
            _context = context;
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
                .Where(ep => ep.Entity == entity && userRoles.Contains(ep.Role))
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
            var restrictedColumns = new HashSet<string>();

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
                                    restrictedColumns.Add(column);
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
            var data = await query.ToListAsync();

            // Apply column filtering if there are restricted columns
            if (restrictedColumns.Any())
            {
                return await ApplyColumnFilteringToData(data, restrictedColumns);
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
                .Where(ep => ep.Entity == entityName && userRoles.Contains(ep.Role))
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
        private async Task<string> GetUserOrgUnitAsync(ClaimsPrincipal user)
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
        /// Applies column filtering to data by removing restricted columns
        /// </summary>
        /// <param name="data">The data to filter</param>
        /// <param name="restrictedColumns">Set of column names to remove</param>
        /// <returns>Data with restricted columns removed</returns>
        private async Task<object> ApplyColumnFilteringToData(object data, HashSet<string> restrictedColumns)
        {
            if (data == null || !restrictedColumns.Any())
                return data;

            try
            {
                // Serialize to JSON, filter properties, then deserialize back
                var jsonString = JsonSerializer.Serialize(data);
                var jsonDocument = JsonDocument.Parse(jsonString);
                
                var filteredJson = FilterJsonProperties(jsonDocument.RootElement, restrictedColumns);
                
                return JsonSerializer.Deserialize<object>(filteredJson);
            }
            catch (JsonException)
            {
                // If JSON processing fails, return original data
                return data;
            }
        }

        /// <summary>
        /// Helper method to filter JSON properties
        /// </summary>
        private string FilterJsonProperties(JsonElement element, HashSet<string> restrictedColumns)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var filteredObject = new Dictionary<string, object>();
                
                foreach (var property in element.EnumerateObject())
                {
                    if (!restrictedColumns.Contains(property.Name))
                    {
                        filteredObject[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                    }
                }
                
                return JsonSerializer.Serialize(filteredObject);
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                var filteredArray = new List<object>();
                
                foreach (var item in element.EnumerateArray())
                {
                    var filteredItem = FilterJsonProperties(item, restrictedColumns);
                    filteredArray.Add(JsonSerializer.Deserialize<object>(filteredItem));
                }
                
                return JsonSerializer.Serialize(filteredArray);
            }
            
            return element.GetRawText();
        }
    }
}
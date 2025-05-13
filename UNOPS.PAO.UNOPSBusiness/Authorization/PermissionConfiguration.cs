using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UNOPS.PAO.UNOPSBusiness.Authorization
{
    public class PermissionConfiguration
    {
        private readonly ILogger<PermissionConfiguration> _logger;
        
        public List<RoutePermission> Routes { get; private set; } = new();
        public List<EntityPermission> Entities { get; private set; } = new();

        public PermissionConfiguration(ILogger<PermissionConfiguration> logger)
        {
            _logger = logger;
        }

        public async Task LoadFromFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("Permissions file not found: {FilePath}", filePath);
                    return;
                }

                var jsonString = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var config = JsonSerializer.Deserialize<PermissionConfigRoot>(jsonString, options);
                if (config != null)
                {
                    Routes = config.Routes ?? new List<RoutePermission>();
                    Entities = config.Entities ?? new List<EntityPermission>();
                    _logger.LogInformation("Successfully loaded permission configuration. Routes: {RouteCount}, Entities: {EntityCount}", 
                        Routes.Count, Entities.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize permission configuration file");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permission configuration from file: {FilePath}", filePath);
                throw;
            }
        }

        // Find allowed roles for a specific API endpoint
        public IEnumerable<string> GetAllowedRolesForApiEndpoint(string path, string method)
        {
            // Normalize the path to lowercase for consistent matching
            path = path.ToLowerInvariant();
            method = method.ToUpperInvariant(); // HTTP methods are usually uppercase
            
            foreach (var route in Routes)
            {
                foreach (var endpoint in route.ApiEndpoints ?? Enumerable.Empty<ApiEndpoint>())
                {
                    // Normalize the endpoint path to lowercase
                    var endpointPath = endpoint.Path.ToLowerInvariant();
                    
                    // Check if the endpoint matches (we check for exact match or pattern with parameter)
                    bool pathMatches = endpointPath.Equals(path, StringComparison.OrdinalIgnoreCase) ||
                                       (endpointPath.Contains("{") && MatchPathPattern(endpointPath, path));

                    if (pathMatches && endpoint.Methods.Contains(method, StringComparer.OrdinalIgnoreCase))
                    {
                        return endpoint.AllowedRoles;
                    }
                }

                // Check in children
                if (route.Children != null)
                {
                    foreach (var child in route.Children)
                    {
                        foreach (var endpoint in child.ApiEndpoints ?? Enumerable.Empty<ApiEndpoint>())
                        {
                            // Normalize the endpoint path to lowercase
                            var endpointPath = endpoint.Path.ToLowerInvariant();
                            
                            bool pathMatches = endpointPath.Equals(path, StringComparison.OrdinalIgnoreCase) ||
                                               (endpointPath.Contains("{") && MatchPathPattern(endpointPath, path));

                            if (pathMatches && endpoint.Methods.Contains(method, StringComparer.OrdinalIgnoreCase))
                            {
                                return endpoint.AllowedRoles;
                            }
                        }
                    }
                }
            }

            // If no match found, return empty list
            return Enumerable.Empty<string>();
        }

        // Find allowed roles for a specific route
        public IEnumerable<string> GetAllowedRolesForRoute(string path)
        {
            path = path.TrimStart('/').ToLowerInvariant();
            
            _logger.LogDebug("Looking for route permissions for: {Path}", path);
            
            // Root path should be accessible to all
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                return new[] { "ALL" };
            }

            foreach (var route in Routes)
            {
                var routePath = route.Path.TrimStart('/').ToLowerInvariant();
                
                _logger.LogDebug("Checking route: {RoutePath} against {Path}", routePath, path);
                
                // Exact match for parent route
                if (string.Equals(routePath, path, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Found exact match for parent route: {RoutePath}", routePath);
                    return route.AllowedRoles;
                }

                // Check if this is a child of a parent route
                if (route.Children != null && path.StartsWith(routePath, StringComparison.OrdinalIgnoreCase))
                {
                    // Remove parent path plus slash to get child path
                    // e.g. "partnerships/contacts" -> "contacts"
                    var childPath = path;
                    if (!string.IsNullOrEmpty(routePath))
                    {
                        childPath = path.Substring(routePath.Length).TrimStart('/');
                    }
                    
                    _logger.LogDebug("Checking for child route: {ChildPath} in parent: {RoutePath}", childPath, routePath);
                    
                    // Special case - if child path is empty, use parent permissions
                    if (string.IsNullOrEmpty(childPath))
                    {
                        _logger.LogDebug("Child path is empty, using parent permissions");
                        return route.AllowedRoles;
                    }
                    
                    // Check if a "/" exists in the remaining path, which could indicate nested paths
                    // like "partnerships/contacts/details"
                    if (childPath.Contains('/'))
                    {
                        var firstSegment = childPath.Split('/')[0].Trim().ToLowerInvariant();
                        
                        _logger.LogDebug("Found nested path, checking first segment: {FirstSegment}", firstSegment);
                        
                        foreach (var child in route.Children)
                        {
                            var childRoutePath = child.Path.ToLowerInvariant();
                            if (string.Equals(childRoutePath, firstSegment, StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.LogDebug("Found child route for first segment: {ChildPath}", childRoutePath);
                                return child.AllowedRoles;
                            }
                        }
                    }
                    else
                    {
                        // Direct child like "partnerships/contacts"
                        foreach (var child in route.Children)
                        {
                            var childRoutePath = child.Path.ToLowerInvariant();
                            _logger.LogDebug("Comparing child route: {ChildPath} with {ChildPathFromUrl}", childRoutePath, childPath);
                            
                            if (string.Equals(childRoutePath, childPath, StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.LogDebug("Found exact match for child route: {ChildPath}", childRoutePath);
                                return child.AllowedRoles;
                            }
                        }
                    }
                    
                    // If no child match found, use parent permissions
                    _logger.LogDebug("No matching child found, using parent permissions for: {RoutePath}", routePath);
                    return route.AllowedRoles;
                }
            }

            // If no match found, return empty list
            _logger.LogWarning("No matching route found for path: {Path}", path);
            return Enumerable.Empty<string>();
        }

        // Find entity permissions for a specific entity and action
        public IEnumerable<string> GetAllowedRolesForEntityAction(string entityName, string action)
        {
            var entity = Entities.FirstOrDefault(e => 
                string.Equals(e.Name, entityName, StringComparison.OrdinalIgnoreCase));

            if (entity != null && entity.Permissions.TryGetValue(action, out var roles))
            {
                return roles;
            }

            return Enumerable.Empty<string>();
        }

        // Helper to match paths with parameters
        private bool MatchPathPattern(string pattern, string actualPath)
        {
            // Normalize both paths to lowercase
            pattern = pattern.ToLowerInvariant();
            actualPath = actualPath.ToLowerInvariant();
            
            // Split both into segments
            var patternSegments = pattern.Split('/');
            var actualSegments = actualPath.Split('/');

            // Must have same number of segments
            if (patternSegments.Length != actualSegments.Length)
            {
                return false;
            }

            // Check each segment
            for (int i = 0; i < patternSegments.Length; i++)
            {
                var patternSegment = patternSegments[i];
                var actualSegment = actualSegments[i];

                // If not a parameter segment, must match exactly
                if (!patternSegment.Contains("{") && !string.Equals(patternSegment, actualSegment, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class PermissionConfigRoot
    {
        public List<RoutePermission> Routes { get; set; } = new();
        public List<EntityPermission> Entities { get; set; } = new();
    }

    public class RoutePermission
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> AllowedRoles { get; set; } = new();
        public List<ApiEndpoint>? ApiEndpoints { get; set; }
        public List<RoutePermission>? Children { get; set; }
    }

    public class ApiEndpoint
    {
        public string Path { get; set; } = string.Empty;
        public List<string> Methods { get; set; } = new();
        public List<string> AllowedRoles { get; set; } = new();
    }

    public class EntityPermission
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, List<string>> Permissions { get; set; } = new();
    }
} 
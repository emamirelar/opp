using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Presentation.Security;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Authorization;

namespace UNOPS.PAO.Presentation.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "IAP")]
    public abstract class BaseController : ControllerBase
    {   
        protected readonly ILogger _logger;
        protected readonly IAuthorizationService _authorizationService;
        protected readonly UserResolverService<int> _userResolverService;
        protected readonly IPermissionService _permissionService;

        protected int CurrentUserId => _userResolverService.GetCurrentUserId();

        protected BaseController(
            ILogger logger,
            IAuthorizationService authorizationService,
            UserResolverService<int> userResolverService,
            IPermissionService permissionService = null)
        {
            _logger = logger;
            _authorizationService = authorizationService;
            _userResolverService = userResolverService;
            _permissionService = permissionService;
        }
        
        /// <summary>
        /// Automatically authorize a request based on HTTP method and standard conventions
        /// </summary>
        /// <param name="context">Action executing context from action filter</param>
        /// <returns>True if authorized, false otherwise</returns>
        [NonAction]
        public async Task<bool> AutoAuthorizeRequest(ActionExecutingContext context)
        {
            try
            {
                // Get controller and action names
                string controllerName = context.RouteData.Values["controller"]?.ToString();
                string actionName = context.RouteData.Values["action"]?.ToString();
                string httpMethod = context.HttpContext.Request.Method;
                
                if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(actionName))
                {
                    _logger.LogWarning("Unable to determine controller or action name for auto-authorization");
                    return false;
                }
                
                // Remove "Controller" suffix if present
                if (controllerName.EndsWith("Controller"))
                {
                    controllerName = controllerName.Substring(0, controllerName.Length - 10);
                }
                
                // Determine entity type from controller name (e.g., "Partner" from "PartnerController")
                string entityType = controllerName;
                
                // Determine required operation based on HTTP method
                string operation = GetOperationFromHttpMethod(httpMethod);
                
                // Determine roles allowed based on entity and operation
                string[] allowedRoles = new string[] { };

                // Check role-based authorization
                var roleAuthResult = await CheckRoleAuthorizationAsync(allowedRoles);
                if (roleAuthResult != null)
                {
                    // Not authorized by role
                    context.Result = roleAuthResult;
                    return false;
                }
                
                // All checks passed - individual controllers should handle entity-level permissions using IPermissionService
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutoAuthorizeRequest");
                return false;
            }
        }

        /// <summary>
        /// Checks if the current user has any of the specified roles
        /// </summary>
        /// <param name="allowedRoles">Array of allowed roles</param>
        /// <returns>ActionResult with 403 Forbidden if user doesn't have any of the roles, null otherwise</returns>
        protected async Task<ActionResult> CheckRoleAuthorizationAsync(params string[] allowedRoles)
        {
            // Special case: if "ALL" is specified as a role, allow access
            if (allowedRoles.Contains("ALL"))
            {
                return null;
            }
            
            // Fallback to the standard User.IsInRole for role checks
            _logger.LogDebug("Checking role authorization for user {UserId}", CurrentUserId);
            
            // Log all claims to help debug role issues
            _logger.LogInformation("User claims for {UserId}:", CurrentUserId);
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("  Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
            
            _logger.LogInformation("Checking if user {UserId} has any of these roles: {Roles}", 
                CurrentUserId, string.Join(", ", allowedRoles));
            
            foreach (var role in allowedRoles)
            {
                bool hasRole = User.IsInRole(role);
                _logger.LogInformation("  Role check: {Role} = {HasRole}", role, hasRole);
                
                if (hasRole)
                {
                    return null; // User has the role, allow access
                }
            }
            
            // No matching role found, log warning and return 403
            _logger.LogWarning("User {UserId} attempted to access endpoint without required roles {Roles}",
                CurrentUserId, string.Join(", ", allowedRoles));
                
            return StatusCode(403, new { error = "You don't have permission to access this resource" });
        }
        
        /// <summary>
        /// Maps HTTP method to corresponding operation name
        /// </summary>
        private string GetOperationFromHttpMethod(string httpMethod)
        {
            return httpMethod.ToUpper() switch
            {
                "GET" => "Read",
                "POST" => "Create",
                "PUT" => "Update",
                "PATCH" => "Update",
                "DELETE" => "Delete",
                _ => "Read" // Default to Read for unknown methods
            };
        }
        
        /// <summary>
        /// Checks if the user has permission to perform the specified operation on the entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="entity">The entity to check permissions for</param>
        /// <param name="operation">The operation to check (Create, Read, Update, Delete)</param>
        /// <returns>True if the user has permission, false otherwise</returns>
        protected async Task<bool> UserHasPermissionAsync<T>(T entity, OperationAuthorizationRequirement operation)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, entity, operation);
            return authResult.Succeeded;
        }

        /// <summary>
        /// Handles an operation with proper error handling and logging
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="successStatusCode">HTTP status code to return on success (defaults to 200 OK)</param>
        /// <returns>An ActionResult containing the operation result or an error response</returns>
        protected async Task<ActionResult> HandleOperationAsync<T>(
            Func<Task<T>> operation,
            int successStatusCode = 200)
        {
            try
            {
                var result = await operation();
                
                // If result is already an ActionResult, return it directly
                if (result is ActionResult actionResult)
                {
                    return actionResult;
                }
                
                // Otherwise, wrap it in a StatusCodeResult
                return StatusCode(successStatusCode, result);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception occurred: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request");
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Handles an operation with proper error handling and logging (for void operations)
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="successStatusCode">HTTP status code to return on success (defaults to 204 No Content)</param>
        /// <returns>An ActionResult or an error response</returns>
        protected async Task<ActionResult> HandleOperationAsync(
            Func<Task> operation,
            int successStatusCode = 204)
        {
            try
            {
                await operation();
                return StatusCode(successStatusCode);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception occurred: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request");
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Checks if the user has permission to perform an operation on an entity and returns Forbid if not
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="entity">The entity to check permissions for</param>
        /// <param name="operation">The operation to check (Create, Read, Update, Delete)</param>
        /// <returns>Forbid result if the user doesn't have permission, null otherwise</returns>
        protected async Task<ActionResult> CheckPermissionAsync<T>(T entity, OperationAuthorizationRequirement operation)
        {
            if (!await UserHasPermissionAsync(entity, operation))
            {
                _logger.LogWarning("User {UserId} attempted to perform {Operation} on {EntityType} without permission",
                    CurrentUserId, operation.Name, typeof(T).Name);
                
                // Return a proper 403 Forbidden status with a clear error message
                return StatusCode(403, new { error = $"You don't have permission to {operation.Name} this {typeof(T).Name}" });
            }

            return null;
        }

        /// <summary>
        /// Validates if user has the required permission. Use this method in lambda expressions.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">The entity to validate permissions against</param>
        /// <param name="operation">The operation to check</param>
        /// <returns>True if permission is granted, otherwise throws UnauthorizedAccessException</returns>
        protected async Task<bool> ValidatePermissionAsync<T>(T entity, OperationAuthorizationRequirement operation)
        {
            if (!await UserHasPermissionAsync(entity, operation))
            {
                _logger.LogWarning("User {UserId} attempted to perform {Operation} on {EntityType} without permission",
                    CurrentUserId, operation.Name, typeof(T).Name);
                throw new UnauthorizedAccessException($"You don't have permission to {operation.Name} this {typeof(T).Name}");
            }

            return true;
        }

        /// <summary>
        /// Checks if the current user has permission to perform the specified action on the entity
        /// </summary>
        /// <param name="entityName">Name of the entity (e.g., "Contact", "Partner")</param>
        /// <param name="action">Action to perform (e.g., "read", "create", "update", "delete")</param>
        /// <param name="entity">Optional entity instance for entity-specific checks</param>
        /// <returns>ActionResult with Forbid if permission denied, null if allowed</returns>
        protected async Task<ActionResult> CheckEntityPermissionAsync(string entityName, string action, object entity = null)
        {
            if (_permissionService == null)
            {
                _logger.LogWarning("IPermissionService not available in controller {ControllerName}. Permission check skipped.", GetType().Name);
                return null; // Allow access if permission service is not available
            }
            
            if (!await _permissionService.CanPerformActionAsync(entityName, action, User, entity))
            {
                _logger.LogWarning("User {UserId} attempted to perform {Action} on {EntityName} without permission",
                    CurrentUserId, action, entityName);
                return Forbid();
            }
            
            return null;
        }

        /// <summary>
        /// Validates if user has the required permission. Throws exception if not authorized.
        /// </summary>
        /// <param name="entityName">Name of the entity (e.g., "Contact", "Partner")</param>
        /// <param name="action">Action to perform (e.g., "read", "create", "update", "delete")</param>
        /// <param name="entity">Optional entity instance for entity-specific checks</param>
        /// <returns>True if permission is granted, otherwise throws UnauthorizedAccessException</returns>
        protected async Task<bool> ValidateEntityPermissionAsync(string entityName, string action, object entity = null)
        {
            if (_permissionService == null)
            {
                _logger.LogWarning("IPermissionService not available in controller {ControllerName}. Permission check skipped.", GetType().Name);
                return true; // Allow access if permission service is not available
            }
            
            if (!await _permissionService.CanPerformActionAsync(entityName, action, User, entity))
            {
                _logger.LogWarning("User {UserId} attempted to perform {Action} on {EntityName} without permission",
                    CurrentUserId, action, entityName);
                throw new UnauthorizedAccessException($"You don't have permission to {action} {entityName}");
            }
            
            return true;
        }

        /// <summary>
        /// Gets all permissions the current user has for a given entity
        /// </summary>
        /// <param name="entityName">Name of the entity (e.g., "Contact", "Partner")</param>
        /// <param name="entity">Optional entity instance for entity-specific checks</param>
        /// <returns>An object containing permission flags</returns>
        protected async Task<object> GetEntityPermissionsAsync(string entityName, object entity = null)
        {
            if (_permissionService == null)
            {
                _logger.LogWarning("IPermissionService not available in controller {ControllerName}. Returning default permissions.", GetType().Name);
                return new
                {
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                };
            }
            
            return new
            {
                CanRead = await _permissionService.CanPerformActionAsync(entityName, "read", User, entity),
                CanCreate = await _permissionService.CanPerformActionAsync(entityName, "create", User, entity),
                CanUpdate = await _permissionService.CanPerformActionAsync(entityName, "update", User, entity),
                CanDelete = await _permissionService.CanPerformActionAsync(entityName, "delete", User, entity)
            };
        }

        /// <summary>
        /// Handles an operation with proper error handling, logging, and role-based authorization
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="allowedRoles">Roles allowed to access this endpoint</param>
        /// <param name="successStatusCode">HTTP status code to return on success (defaults to 200 OK)</param>
        /// <returns>An ActionResult containing the operation result or an error response</returns>
        protected async Task<ActionResult> HandleOperationWithAuthAsync<T>(
            Func<Task<T>> operation, 
            string[] allowedRoles,
            int successStatusCode = 200)
        {
            // Check role-based authorization first
            var authResult = await CheckRoleAuthorizationAsync(allowedRoles);
            if (authResult != null)
            {
                return authResult;
            }
            
            // If authorized, proceed with normal operation handling
            return await HandleOperationAsync(operation, successStatusCode);
        }

        /// <summary>
        /// Handles an operation with proper error handling, logging, and role-based authorization (for void operations)
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="allowedRoles">Roles allowed to access this endpoint</param>
        /// <param name="successStatusCode">HTTP status code to return on success (defaults to 204 No Content)</param>
        /// <returns>An ActionResult or an error response</returns>
        protected async Task<ActionResult> HandleOperationWithAuthAsync(
            Func<Task> operation,
            string[] allowedRoles,
            int successStatusCode = 204)
        {
            // Check role-based authorization first
            var authResult = await CheckRoleAuthorizationAsync(allowedRoles);
            if (authResult != null)
            {
                return authResult;
            }
            
            // If authorized, proceed with normal operation handling
            return await HandleOperationAsync(operation, successStatusCode);
        }

        /// <summary>
        /// Handles an operation with proper error handling, logging, and both role-based and entity-level authorization
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <typeparam name="TEntity">Type of the entity to check permissions for</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="entity">The entity to check permissions for</param>
        /// <param name="operationRequirement">The operation requirement (Create, Read, Update, Delete)</param>
        /// <param name="allowedRoles">Roles allowed to access this endpoint</param>
        /// <param name="successStatusCode">HTTP status code to return on success (defaults to 200 OK)</param>
        /// <returns>An ActionResult containing the operation result or an error response</returns>
        protected async Task<ActionResult> HandleOperationWithPermissionAsync<T, TEntity>(
            Func<Task<T>> operation,
            TEntity entity,
            OperationAuthorizationRequirement operationRequirement,
            string[] allowedRoles,
            int successStatusCode = 200)
        {
            // Check role-based authorization first
            var roleAuthResult = await CheckRoleAuthorizationAsync(allowedRoles);
            if (roleAuthResult != null)
            {
                return roleAuthResult;
            }
            
            // Check entity-level permission
            var permissionResult = await CheckPermissionAsync(entity, operationRequirement);
            if (permissionResult != null)
            {
                return permissionResult;
            }
            
            // If both authorization checks pass, proceed with normal operation handling
            return await HandleOperationAsync(operation, successStatusCode);
        }

        /// <summary>
        /// Handles an operation with proper error handling, logging, and both role-based and entity-level authorization (for void operations)
        /// </summary>
        /// <typeparam name="TEntity">Type of the entity to check permissions for</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="entity">The entity to check permissions for</param>
        /// <param name="operationRequirement">The operation requirement (Create, Read, Update, Delete)</param>
        /// <param name="allowedRoles">Roles allowed to access this endpoint</param>
        /// <param name="successStatusCode">HTTP status code to return on success (defaults to 204 No Content)</param>
        /// <returns>An ActionResult or an error response</returns>
        protected async Task<ActionResult> HandleOperationWithPermissionAsync<TEntity>(
            Func<Task> operation,
            TEntity entity,
            OperationAuthorizationRequirement operationRequirement,
            string[] allowedRoles,
            int successStatusCode = 204)
        {
            // Check role-based authorization first
            var roleAuthResult = await CheckRoleAuthorizationAsync(allowedRoles);
            if (roleAuthResult != null)
            {
                return roleAuthResult;
            }
            
            // Check entity-level permission
            var permissionResult = await CheckPermissionAsync(entity, operationRequirement);
            if (permissionResult != null)
            {
                return permissionResult;
            }
            
            // If both authorization checks pass, proceed with normal operation handling
            return await HandleOperationAsync(operation, successStatusCode);
        }
    }
} 
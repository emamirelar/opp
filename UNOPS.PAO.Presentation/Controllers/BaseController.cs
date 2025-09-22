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
using System.Text.Json;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSBusiness.Managers;

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
            IPermissionService permissionService = null,
            UNOPSAppDbContext context = null,
            AiContextualService aiService = null)
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

        /// <summary>
        /// Validates pagination parameters and returns a BadRequest result if invalid
        /// </summary>
        /// <param name="pageIndex">The page index to validate</param>
        /// <param name="pageSize">The page size to validate</param>
        /// <param name="maxPageSize">Maximum allowed page size (default: 2000)</param>
        /// <returns>BadRequest ActionResult if invalid, null if valid</returns>
        protected ActionResult? ValidatePaginationParameters(int pageIndex, int pageSize, int maxPageSize = 2000)
        {
            var errors = new Dictionary<string, string[]>();

            if (pageIndex < 1)
            {
                errors["pageIndex"] = new[] { "Page index must be greater than 0" };
            }
            
            if (pageSize < 1)
            {
                errors["pageSize"] = new[] { "Page size must be greater than 0" };
            }
            else if (pageSize > maxPageSize)
            {
                errors["pageSize"] = new[] { $"Page size cannot exceed {maxPageSize}" };
            }
            
            if (errors.Any())
            {
                return BadRequest(new ValidationProblemDetails(errors)
                {
                    Title = "Invalid pagination parameters"
                });
            }
            
            return null;
        }

        /// <summary>
        /// Validates model state and returns ValidationProblemDetails if invalid
        /// </summary>
        /// <returns>BadRequest with validation details if invalid, null if valid</returns>
        protected ActionResult? ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Title = "One or more validation errors occurred"
                });
            }
            return null;
        }

        /// <summary>
        /// Creates a standardized validation error response
        /// </summary>
        /// <param name="field">Field name</param>
        /// <param name="error">Error message</param>
        /// <returns>BadRequest with validation details</returns>
        protected ActionResult CreateValidationError(string field, string error)
        {
            var errors = new Dictionary<string, string[]>
            {
                [field] = new[] { error }
            };
            
            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "Validation failed"
            });
        }

        /// <summary>
        /// Creates a standardized validation error response with multiple errors
        /// </summary>
        /// <param name="errors">Dictionary of field names and error messages</param>
        /// <returns>BadRequest with validation details</returns>
        protected ActionResult CreateValidationErrors(Dictionary<string, string[]> errors)
        {
            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "One or more validation errors occurred"
            });
        }

        /// <summary>
        /// Handles search operations with proper error handling and logging
        /// </summary>
        /// <typeparam name="T">Return type of the search operation</typeparam>
        /// <param name="searchOperation">The search operation to execute</param>
        /// <param name="searchDescription">Description of the search for logging</param>
        /// <returns>An ActionResult containing the search result or an error response</returns>
        protected async Task<ActionResult> HandleSearchOperationAsync<T>(
            Func<Task<T>> searchOperation,
            string searchDescription = "search operation")
        {
            try
            {
                _logger.LogInformation("Executing {SearchDescription}", searchDescription);
                var result = await searchOperation();
                return Ok(result);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON format in {SearchDescription}", searchDescription);
                return BadRequest(new { error = "Invalid search criteria format", details = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid search criteria in {SearchDescription}", searchDescription);
                return BadRequest(new { error = ex.Message });
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception in {SearchDescription}: {Message}", searchDescription, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during {SearchDescription}", searchDescription);
                return StatusCode(500, new { error = $"An error occurred during {searchDescription}" });
            }
        }

        protected async Task<ActionResult> GetEntityPermissionsAsync(string entityType, object entity = null)
        {
            var permissions = await _permissionService.GetEntityPermissionsAsync(entityType, entity);
            return Ok(permissions);
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
        /// Applies intelligent field value matching to handle typos and similar values for AI agents
        /// </summary>
        /// <param name="searchCriteria">JSON search criteria to process</param>
        /// <param name="entityType">Entity type (Partner, Contact, Interaction)</param>
        /// <returns>Processed search criteria with corrected field values</returns>
        protected async Task<string> ApplySmartFieldMatching(string searchCriteria, string entityType)
        {
            try
            {
                const float defaultSimilarityThreshold = 0.7f;
                _logger.LogInformation("Applying smart field matching for {EntityType} with threshold {Threshold}", entityType, defaultSimilarityThreshold);

                // Parse the search criteria
                var criteriaList = JsonSerializer.Deserialize<JsonElement[]>(searchCriteria);
                var processedCriteria = new List<object>();

                foreach (var criterion in criteriaList)
                {
                    var processedCriterion = await ProcessSingleCriterion(criterion, entityType, defaultSimilarityThreshold);
                    processedCriteria.Add(processedCriterion);
                }

                // Serialize back to JSON
                var result = JsonSerializer.Serialize(processedCriteria, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                _logger.LogInformation("Smart field matching completed. Original: {Original}, Processed: {Processed}", 
                    searchCriteria, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Smart field matching failed, returning original criteria");
                return searchCriteria; // Return original if processing fails
            }
        }

        /// <summary>
        /// Processes a single search criterion to apply smart field value matching
        /// </summary>
        private async Task<object> ProcessSingleCriterion(JsonElement criterion, string entityType, float similarityThreshold)
        {
            try
            {
                var field = criterion.GetProperty("field").GetString();
                var value = criterion.GetProperty("value").GetString();
                var operatorValue = criterion.GetProperty("operator").GetString();

                // Only apply smart matching for text-based "like" operations
                if (string.IsNullOrWhiteSpace(value) || operatorValue != "like")
                {
                    return ConvertJsonElementToObject(criterion);
                }

                // Apply field-specific smart matching
                var correctedValue = await ApplyFieldSpecificMatching(field, value, entityType, similarityThreshold);

                // Create new criterion with corrected value
                var result = new
                {
                    field = field,
                    value = correctedValue,
                    label = criterion.TryGetProperty("label", out var labelProp) ? labelProp.GetString() : "",
                    @operator = operatorValue,
                    logicalOperator = criterion.TryGetProperty("logicalOperator", out var logicalProp) ? logicalProp.GetString() : "AND",
                    fieldType = criterion.TryGetProperty("fieldType", out var typeProp) ? typeProp.GetString() : "text"
                };

                if (correctedValue != value)
                {
                    _logger.LogInformation("Smart field matching: '{Original}' -> '{Corrected}' for field '{Field}'", 
                        value, correctedValue, field);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process single criterion, returning original");
                return ConvertJsonElementToObject(criterion);
            }
        }

        /// <summary>
        /// Applies field-specific smart matching based on field type and entity
        /// </summary>
        private async Task<string> ApplyFieldSpecificMatching(string field, string value, string entityType, float similarityThreshold)
        {
            try
            {
                // For partner group, liaison office, organization unit fields - use fuzzy matching
                if (IsEntityLookupField(field))
                {
                    return await FindSimilarEntityValue(field, value, entityType, similarityThreshold);
                }

                // For name fields - apply typo correction
                if (IsNameField(field))
                {
                    return ApplyTypoCorrection(value);
                }

                // For other text fields - apply basic normalization
                return NormalizeTextValue(value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Field-specific matching failed for {Field}, returning original value", field);
                return value;
            }
        }

        /// <summary>
        /// Determines if a field is an entity lookup field that should use fuzzy matching
        /// </summary>
        private bool IsEntityLookupField(string field)
        {
            var lookupFields = new[]
            {
                "partnerGroup.name", "partnerGroup.code",
                "liaisonOffice.name", "liaisonOffice.code",
                "organizationUnitRelationships.organizationHierarchy.name",
                "partner.partnerGroup.name", "partner.liaisonOffice.name",
                "contact.partner.partnerGroup.name"
            };

            return lookupFields.Any(f => field.Equals(f, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines if a field is a name field that should use typo correction
        /// </summary>
        private bool IsNameField(string field)
        {
            var nameFields = new[]
            {
                "name", "firstName", "lastName", "title", "subject", "description",
                "contacts.firstName", "contacts.lastName", "contact.firstName", "contact.lastName",
                "partner.name", "contact.partner.name"
            };

            return nameFields.Any(f => field.Equals(f, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Finds similar entity values using database lookup with fuzzy matching
        /// </summary>
        private async Task<string> FindSimilarEntityValue(string field, string value, string entityType, float similarityThreshold)
        {
            try
            {
                // This is a simplified implementation - in a real scenario, you'd query the database
                // for similar values based on the field type (PartnerGroup, LiaisonOffice, etc.)
                
                // For now, apply basic typo correction
                return ApplyTypoCorrection(value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Entity value lookup failed for {Field}", field);
                return value;
            }
        }

        /// <summary>
        /// Applies basic typo correction to text values
        /// </summary>
        private string ApplyTypoCorrection(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            // Common typo corrections for AI agents
            var corrections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Common misspellings
                {"privat", "private"},
                {"goverment", "government"},
                {"publick", "public"},
                {"internatinal", "international"},
                {"organizaton", "organization"},
                {"infrastucture", "infrastructure"},
                {"devlopment", "development"},
                {"parner", "partner"},
                {"contac", "contact"},
                {"meetng", "meeting"},
                {"discusion", "discussion"},
                {"presentaton", "presentation"},
                
                // AI common mistakes
                {"NGO", "NGO"},
                {"UN", "UN"},
                {"WHO", "WHO"},
                {"UNICEF", "UNICEF"},
                {"UNDP", "UNDP"}
            };

            // Check for exact matches first
            foreach (var correction in corrections)
            {
                if (value.Equals(correction.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return correction.Value;
                }
            }

            // Check for partial matches
            foreach (var correction in corrections)
            {
                if (value.Contains(correction.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return value.Replace(correction.Key, correction.Value, StringComparison.OrdinalIgnoreCase);
                }
            }

            return value;
        }

        /// <summary>
        /// Normalizes text values for better matching
        /// </summary>
        private string NormalizeTextValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            // Trim whitespace and normalize spacing
            return value.Trim().Replace("  ", " ");
        }

        /// <summary>
        /// Converts JsonElement to object for serialization
        /// </summary>
        private object ConvertJsonElementToObject(JsonElement element)
        {
            try
            {
                return new
                {
                    field = element.GetProperty("field").GetString(),
                    value = element.GetProperty("value").GetString(),
                    label = element.TryGetProperty("label", out var labelProp) ? labelProp.GetString() : "",
                    @operator = element.GetProperty("operator").GetString(),
                    logicalOperator = element.TryGetProperty("logicalOperator", out var logicalProp) ? logicalProp.GetString() : "AND",
                    fieldType = element.TryGetProperty("fieldType", out var typeProp) ? typeProp.GetString() : "text"
                };
            }
            catch
            {
                return element;
            }
        }

    }
} 
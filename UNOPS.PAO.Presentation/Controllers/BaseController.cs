using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Presentation.Security;

namespace UNOPS.PAO.Presentation.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "IAP")]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger _logger;
        protected readonly IAuthorizationService _authorizationService;
        protected readonly UserResolverService<int> _userResolverService;

        protected int CurrentUserId => _userResolverService.GetCurrentUserId();

        protected BaseController(
            ILogger logger,
            IAuthorizationService authorizationService,
            UserResolverService<int> userResolverService)
        {
            _logger = logger;
            _authorizationService = authorizationService;
            _userResolverService = userResolverService;
        }

        /// <summary>
        /// Checks if the current user has permission to perform the specified operation on the entity.
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
                return Forbid();
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
        /// Gets all permissions the current user has for a given entity
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="entity">The entity to check permissions for</param>
        /// <returns>An object containing permission flags</returns>
        protected async Task<object> GetEntityPermissionsAsync<T>(T entity)
        {
            var canReadResult = await _authorizationService.AuthorizeAsync(User, entity, Operations.Read);
            var canUpdateResult = await _authorizationService.AuthorizeAsync(User, entity, Operations.Update);
            var canCreateResult = await _authorizationService.AuthorizeAsync(User, entity, Operations.Create);
            var canDeleteResult = await _authorizationService.AuthorizeAsync(User, entity, Operations.Delete);

            return new
            {
                CanRead = canReadResult.Succeeded,
                CanUpdate = canUpdateResult.Succeeded,
                CanCreate = canCreateResult.Succeeded,
                CanDelete = canDeleteResult.Succeeded
            };
        }
    }
} 
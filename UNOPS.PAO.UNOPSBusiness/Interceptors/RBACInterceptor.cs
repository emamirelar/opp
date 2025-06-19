using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Security.Claims;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Services;

namespace UNOPS.PAO.UNOPSBusiness.Interceptors;

public class RBACInterceptor : IInterceptor
{
    private readonly IBusinessSecurityService _securityService;
    private readonly ILogger<RBACInterceptor> _logger;

    public RBACInterceptor(IBusinessSecurityService securityService, ILogger<RBACInterceptor> logger)
    {
        _securityService = securityService;
        _logger = logger;
    }

    public void Intercept(IInvocation invocation)
    {
        var method = invocation.Method;
        
        // Check if method should skip RBAC
        var skipRBACAttribute = method.GetCustomAttribute<SkipRBACAttribute>();
        if (skipRBACAttribute != null)
        {
            _logger.LogDebug("Skipping RBAC for method {MethodName}. Reason: {Reason}", 
                method.Name, skipRBACAttribute.Reason ?? "Not specified");
            invocation.Proceed();
            return;
        }

        // Get RBAC attribute
        var rbacAttribute = method.GetCustomAttribute<RBACAttribute>();
        if (rbacAttribute == null)
        {
            // No RBAC attribute, proceed normally
            invocation.Proceed();
            return;
        }

        try
        {
            // Extract ClaimsPrincipal from method parameters
            var user = ExtractUserFromParameters(invocation);
            if (user == null)
            {
                _logger.LogWarning("No ClaimsPrincipal found in method parameters for {MethodName}", method.Name);
                invocation.Proceed();
                return;
            }

            // Determine entity name
            var entityName = DetermineEntityName(rbacAttribute, method);

            _logger.LogDebug("Applying RBAC security for method {MethodName}, Entity: {EntityName}, Action: {Action}", 
                method.Name, entityName, rbacAttribute.Action);

            // Pre-execution security checks
            Task.Run(async () => await PerformPreExecutionChecks(invocation, rbacAttribute, user, entityName)).Wait();

            // Execute the method
            invocation.Proceed();

            // Post-execution security processing (for return values)
            if (invocation.ReturnValue != null)
            {
                Task.Run(async () => await PerformPostExecutionProcessing(invocation, rbacAttribute, user, entityName)).Wait();
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Re-throw authorization exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RBAC interceptor for method {MethodName}", method.Name);
            throw;
        }
    }

    private async Task PerformPreExecutionChecks(IInvocation invocation, RBACAttribute rbacAttribute, ClaimsPrincipal user, string entityName)
    {
        // Check basic entity permissions
        var entityPermissions = await _securityService.GetEntityPermissionsAsync(user, entityName);
        
        var hasPermission = rbacAttribute.Action.ToLower() switch
        {
            "read" => entityPermissions.CanRead,
            "create" => entityPermissions.CanCreate,
            "update" => entityPermissions.CanUpdate,
            "delete" => entityPermissions.CanDelete,
            _ => false
        };

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException($"User does not have {rbacAttribute.Action} permission for {entityName}");
        }

        // Check specific entity access if required
        if (rbacAttribute.RequireEntityAccess)
        {
            await CheckSpecificEntityAccess(invocation, rbacAttribute, user, entityName);
        }
    }

    private async Task CheckSpecificEntityAccess(IInvocation invocation, RBACAttribute rbacAttribute, ClaimsPrincipal user, string entityName)
    {
        object? entity = null;
        int? entityId = null;

        // Try to get entity from parameters
        if (!string.IsNullOrEmpty(rbacAttribute.EntityParameterName))
        {
            entity = GetParameterValue(invocation, rbacAttribute.EntityParameterName);
        }

        // Try to get entity ID from parameters
        if (!string.IsNullOrEmpty(rbacAttribute.EntityIdParameterName))
        {
            var idValue = GetParameterValue(invocation, rbacAttribute.EntityIdParameterName);
            if (idValue != null && int.TryParse(idValue.ToString(), out var id))
            {
                entityId = id;
            }
        }

        // Check access based on what we found
        bool hasAccess = false;
        if (entityId.HasValue)
        {
            hasAccess = await _securityService.CanUserAccessEntityAsync(user, entityName, entityId.Value, rbacAttribute.Action);
        }
        else if (entity != null)
        {
            // For entity objects, we'll need to use reflection to get the generic method
            var entityType = entity.GetType();
            var method = typeof(IBusinessSecurityService).GetMethods()
                .FirstOrDefault(m => m.Name == nameof(IBusinessSecurityService.CanUserAccessEntityAsync) &&
                                   m.IsGenericMethodDefinition);
            
            if (method != null)
            {
                var genericMethod = method.MakeGenericMethod(entityType);
                var task = (Task<bool>)genericMethod.Invoke(_securityService, new[] { entity, user, rbacAttribute.Action })!;
                hasAccess = await task;
            }
        }

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException($"User does not have {rbacAttribute.Action} access to this {entityName}");
        }
    }

    private async Task PerformPostExecutionProcessing(IInvocation invocation, RBACAttribute rbacAttribute, ClaimsPrincipal user, string entityName)
    {
        if (!rbacAttribute.ApplyRowFiltering && !rbacAttribute.ApplyColumnFiltering)
            return;

        var returnValue = invocation.ReturnValue;
        if (returnValue == null)
            return;

        try
        {
            // Handle Task<T> return types
            if (returnValue.GetType().IsGenericType && 
                returnValue.GetType().GetGenericTypeDefinition() == typeof(Task<>))
            {
                var taskResult = await GetTaskResult(returnValue);
                if (taskResult != null)
                {
                    var filteredResult = await ApplySecurityFiltering(taskResult, rbacAttribute, user, entityName);
                    SetTaskResult(invocation, filteredResult);
                }
            }
            // Handle IQueryable<T> return types (apply row filtering)
            else if (IsQueryableType(returnValue.GetType()) && rbacAttribute.ApplyRowFiltering)
            {
                var filteredResult = await ApplyRowFiltering(returnValue, rbacAttribute, user);
                invocation.ReturnValue = filteredResult;
            }
            // Handle direct entity/collection returns
            else
            {
                var filteredResult = await ApplySecurityFiltering(returnValue, rbacAttribute, user, entityName);
                invocation.ReturnValue = filteredResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying security filtering to return value in method {MethodName}", invocation.Method.Name);
            // Don't throw here - let the original return value pass through
        }
    }

    private async Task<object?> ApplySecurityFiltering(object returnValue, RBACAttribute rbacAttribute, ClaimsPrincipal user, string entityName)
    {
        if (returnValue == null || !rbacAttribute.ApplyColumnFiltering) 
            return returnValue;

        var returnType = returnValue.GetType();

        try
        {
            // Handle collections
            if (IsCollectionType(returnType))
            {
                var elementType = GetCollectionElementType(returnType);
                if (elementType != null)
                {
                    var method = typeof(IBusinessSecurityService).GetMethod(nameof(IBusinessSecurityService.FilterEntityColumnsForList));
                    var genericMethod = method?.MakeGenericMethod(elementType);
                    var task = (Task<object>)genericMethod?.Invoke(_securityService, new[] { returnValue, user, rbacAttribute.Action })!;
                    return await task;
                }
            }
            // Handle single entities
            else if (IsEntityType(returnType))
            {
                var method = typeof(IBusinessSecurityService).GetMethod(nameof(IBusinessSecurityService.FilterEntityColumns));
                var genericMethod = method?.MakeGenericMethod(returnType);
                var task = (Task<object>)genericMethod?.Invoke(_securityService, new[] { returnValue, user, rbacAttribute.Action })!;
                return await task;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying column filtering to {ReturnType}", returnType.Name);
        }

        return returnValue;
    }

    private async Task<object?> ApplyRowFiltering(object queryable, RBACAttribute rbacAttribute, ClaimsPrincipal user)
    {
        if (!rbacAttribute.ApplyRowFiltering) return queryable;

        var queryableType = queryable.GetType();
        if (queryableType.IsGenericType)
        {
            var entityType = queryableType.GetGenericArguments().FirstOrDefault();
            if (entityType != null)
            {
                try
                {
                    var method = typeof(IBusinessSecurityService).GetMethod(nameof(IBusinessSecurityService.ApplyRowFiltersAsync));
                    var genericMethod = method?.MakeGenericMethod(entityType);
                    var task = (Task<object>)genericMethod?.Invoke(_securityService, new[] { queryable, user, rbacAttribute.Action })!;
                    return await task;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying row filtering to {EntityType}", entityType.Name);
                }
            }
        }

        return queryable;
    }

    #region Helper Methods

    private ClaimsPrincipal? ExtractUserFromParameters(IInvocation invocation)
    {
        var parameters = invocation.Method.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType == typeof(ClaimsPrincipal))
            {
                return invocation.Arguments[i] as ClaimsPrincipal;
            }
        }
        return null;
    }

    private string DetermineEntityName(RBACAttribute rbacAttribute, MethodInfo method)
    {
        if (!string.IsNullOrEmpty(rbacAttribute.Entity))
        {
            return rbacAttribute.Entity;
        }

        // Try to infer from class name
        var className = method.DeclaringType?.Name ?? "";
        if (className.StartsWith("UNOPS") && className.EndsWith("Manager"))
        {
            var entityName = className.Substring(5, className.Length - 12); // Remove "UNOPS" and "Manager"
            return entityName;
        }

        return "Unknown";
    }

    private object? GetParameterValue(IInvocation invocation, string parameterName)
    {
        var parameters = invocation.Method.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].Name?.Equals(parameterName, StringComparison.OrdinalIgnoreCase) == true)
            {
                return invocation.Arguments[i];
            }
        }
        return null;
    }

    private async Task<object?> GetTaskResult(object task)
    {
        try
        {
            dynamic dynamicTask = task;
            await dynamicTask;
            return dynamicTask.Result;
        }
        catch
        {
            return null;
        }
    }

    private void SetTaskResult(IInvocation invocation, object? result)
    {
        var returnType = invocation.Method.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = returnType.GetGenericArguments()[0];
            var taskFromResult = typeof(Task).GetMethod(nameof(Task.FromResult))?.MakeGenericMethod(resultType);
            invocation.ReturnValue = taskFromResult?.Invoke(null, new[] { result });
        }
    }

    private bool IsQueryableType(Type type)
    {
        return type.IsGenericType && 
               (type.GetGenericTypeDefinition() == typeof(IQueryable<>) ||
                type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>)));
    }

    private bool IsCollectionType(Type type)
    {
        return type != typeof(string) && 
               (type.IsArray || 
                (type.IsGenericType && 
                 (type.GetGenericTypeDefinition() == typeof(List<>) ||
                  type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                  type.GetGenericTypeDefinition() == typeof(ICollection<>))));
    }

    private bool IsEntityType(Type type)
    {
        // Simple heuristic - consider it an entity if it's a class with properties
        return type.IsClass && 
               type != typeof(string) && 
               type.GetProperties().Any();
    }

    private Type? GetCollectionElementType(Type collectionType)
    {
        if (collectionType.IsArray)
            return collectionType.GetElementType();
        
        if (collectionType.IsGenericType)
            return collectionType.GetGenericArguments().FirstOrDefault();
        
        return null;
    }

    #endregion
} 
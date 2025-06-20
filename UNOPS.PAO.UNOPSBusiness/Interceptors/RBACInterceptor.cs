using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Security.Claims;
using System.Collections;
using UNOPS.PAO.RBAC.Attributes;
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
            // Handle collections - use the entity name instead of reflection
            if (IsCollectionType(returnType))
            {
                // Use non-generic overload with entity name to avoid AsyncStateMachine issues
                return await ApplyColumnFilteringToCollection(returnValue, user, entityName, rbacAttribute.Action);
            }
            // Handle single entities - use the entity name instead of reflection
            else if (IsEntityType(returnType))
            {
                // Use non-generic overload with entity name to avoid AsyncStateMachine issues
                return await ApplyColumnFilteringToEntity(returnValue, user, entityName, rbacAttribute.Action);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying column filtering to {ReturnType}", returnType.Name);
        }

        return returnValue;
    }

    private async Task<object> ApplyColumnFilteringToEntity(object entity, ClaimsPrincipal user, string entityName, string action)
    {
        // Get allowed columns using entity name (not generic type)
        var allowedColumns = await _securityService.GetAllowedColumnsAsync(user, entityName, action);
        return FilterEntityByColumns(entity, allowedColumns);
    }

    private async Task<object> ApplyColumnFilteringToCollection(object collection, ClaimsPrincipal user, string entityName, string action)
    {
        // Get allowed columns using entity name (not generic type)
        var allowedColumns = await _securityService.GetAllowedColumnsAsync(user, entityName, action);
        
        if (collection is IEnumerable enumerable)
        {
            var filteredList = new List<object>();
            foreach (var item in enumerable)
            {
                if (item != null)
                {
                    var filteredItem = FilterEntityByColumns(item, allowedColumns);
                    filteredList.Add(filteredItem);
                }
            }
            return filteredList;
        }
        
        return collection;
    }

    private object FilterEntityByColumns(object entity, IEnumerable<string> allowedColumns)
    {
        if (entity == null) return entity;

        var allowedColumnsSet = new HashSet<string>(allowedColumns, StringComparer.OrdinalIgnoreCase);
        var entityType = GetActualEntityType(entity);
        
        if (entityType == null)
        {
            _logger.LogWarning("Could not determine actual entity type for {ObjectType}, skipping column filtering", entity.GetType().Name);
            return entity;
        }
        
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        _logger.LogDebug("Filtering entity {EntityType} with allowed columns: {AllowedColumns}", 
            entityType.Name, string.Join(", ", allowedColumns));

        // Instead of creating a new instance, modify the existing entity by setting restricted properties to default
        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                try
                {
                    if (!allowedColumnsSet.Contains(property.Name))
                    {
                        // Set restricted properties to default values
                        if (property.PropertyType.IsValueType)
                        {
                            var defaultValue = Activator.CreateInstance(property.PropertyType);
                            property.SetValue(entity, defaultValue);
                            _logger.LogDebug("Set restricted property {PropertyName} to default value", property.Name);
                        }
                        else
                        {
                            property.SetValue(entity, null);
                            _logger.LogDebug("Set restricted property {PropertyName} to null", property.Name);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Keeping allowed property {PropertyName}", property.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error filtering property {PropertyName} for entity {EntityType}", property.Name, entityType.Name);
                }
            }
        }

        return entity;
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

    private Type? GetActualEntityType(object entity)
    {
        var entityType = entity.GetType();
        
        // Handle AsyncStateMachine and other compiler-generated types
        if (entityType.Name.Contains("AsyncStateMachine") || entityType.Name.Contains("`"))
        {
            _logger.LogDebug("Detected async state machine type: {TypeName}", entityType.FullName);
            
            // Try to extract the actual entity type from the FullName
            // Example: "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1+AsyncStateMachineBox`1[[UNOPS.PAO.Models.ContactModel, ..."
            var fullName = entityType.FullName ?? "";
            
            // Look for the pattern [[ActualType, Assembly]]
            var match = System.Text.RegularExpressions.Regex.Match(fullName, @"\[\[([^,]+),");
            if (match.Success)
            {
                var actualTypeName = match.Groups[1].Value;
                _logger.LogDebug("Extracted entity type name: {ActualTypeName}", actualTypeName);
                
                // Try to load the type
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var actualType = assembly.GetType(actualTypeName);
                        if (actualType != null)
                        {
                            _logger.LogDebug("Successfully resolved entity type: {ActualType}", actualType.Name);
                            return actualType;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error trying to load type {TypeName} from assembly {AssemblyName}", actualTypeName, assembly.FullName);
                    }
                }
            }
            
            // Fallback: Try to get from generic arguments
            if (entityType.IsGenericType)
            {
                var genericArgs = entityType.GetGenericArguments();
                if (genericArgs.Length > 0)
                {
                    var firstArg = genericArgs[0];
                    if (!firstArg.Name.Contains("AsyncStateMachine") && !firstArg.Name.Contains("`"))
                    {
                        _logger.LogDebug("Using first generic argument as entity type: {EntityType}", firstArg.Name);
                        return firstArg;
                    }
                }
            }
            
            _logger.LogWarning("Could not extract actual entity type from async state machine: {TypeName}", entityType.FullName);
            return null;
        }
        
        // For normal types, return as-is
        return entityType;
    }

    #endregion
} 
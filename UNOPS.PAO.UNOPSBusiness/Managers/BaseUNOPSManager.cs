namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.UNOPSDataAccess.Context;

/// <summary>
/// Base class for all UNOPS managers that provides common AI prompt functionality
/// </summary>
public abstract class BaseUNOPSManager
{
    protected readonly IMapper _mapper;
    protected readonly UNOPSAppDbContext _context;
    protected readonly IConfiguration _configuration;

    protected BaseUNOPSManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration)
    {
        _mapper = mapper;
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Calls a specific function on this manager by name with the entity ID
    /// This method uses reflection to call the function specified in AiPrompt.PromptFunction
    /// </summary>
    /// <param name="functionName">Name of the function to call (e.g., "GetPartnerAsync", "GetContactWithInteractionsAsync")</param>
    /// <param name="entityId">ID of the entity to retrieve</param>
    /// <param name="user">Optional user context for permission checking</param>
    /// <returns>Result of the function call</returns>
    public virtual async Task<object> CallFunctionByNameAsync(string functionName, int entityId, ClaimsPrincipal user = null)
    {
        if (string.IsNullOrEmpty(functionName))
        {
            throw new ArgumentException("Function name cannot be null or empty", nameof(functionName));
        }

        try
        {
            // Get the method by name
            var method = GetType().GetMethod(functionName, BindingFlags.Public | BindingFlags.Instance);
            
            if (method == null)
            {
                throw new ArgumentException($"Method '{functionName}' not found on {GetType().Name}");
            }

            // Get method parameters to determine the correct overload
            var parameters = method.GetParameters();
            
            // Call the method with appropriate parameters based on its signature
            object result;
            
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
            {
                // Method signature: Method(int id)
                result = method.Invoke(this, new object[] { entityId });
            }
            else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(ClaimsPrincipal) && parameters[1].ParameterType == typeof(int))
            {
                // Method signature: Method(ClaimsPrincipal user, int id)
                result = method.Invoke(this, new object[] { user, entityId });
            }
            else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(int) && parameters[1].ParameterType == typeof(int))
            {
                // Method signature: Method(int userId, int id) - for legacy methods
                var userId = GetUserIdFromClaims(user);
                result = method.Invoke(this, new object[] { userId, entityId });
            }
            else
            {
                // Try calling with just entityId as fallback
                result = method.Invoke(this, new object[] { entityId });
            }

            // Handle async methods
            if (result is Task task)
            {
                await task;
                
                // Get the result if it's Task<T>
                if (task.GetType().IsGenericType)
                {
                    var resultProperty = task.GetType().GetProperty("Result");
                    return resultProperty?.GetValue(task);
                }
                
                return null; // Task without return value
            }

            return result;
        }
        catch (TargetInvocationException ex)
        {
            // Unwrap the inner exception for better error messages
            throw ex.InnerException ?? ex;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error calling function '{functionName}' on {GetType().Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Helper method to extract user ID from claims
    /// </summary>
    private int GetUserIdFromClaims(ClaimsPrincipal user)
    {
        if (user == null) return 0;
        
        var userIdClaim = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        
        return 0;
    }

    /// <summary>
    /// Gets basic entity data - must be implemented by derived managers
    /// </summary>
    public abstract Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null);

    /// <summary>
    /// Gets the entity type name for this manager (used for logging/error messages)
    /// </summary>
    protected virtual string GetEntityTypeName()
    {
        var typeName = GetType().Name;
        // Remove "UNOPS" prefix and "Manager" suffix
        return typeName.Replace("UNOPS", "").Replace("Manager", "");
    }
} 
namespace UNOPS.PAO.UNOPSPresentation.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

public class EntityPermissionHandler : AuthorizationHandler<EntityPermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public EntityPermissionHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EntityPermissionRequirement requirement)
    {
        // Get the entity from the request if available (for row-level checks)
        var httpContext = context.Resource as HttpContext;
        object? entity = null;
        
        // If we're in an endpoint with an entity parameter, try to extract it
        // This is a simplified implementation, you might need to adapt it for your API patterns
        if (httpContext != null)
        {
            // Try to get the entity from route data or request body
            if (httpContext.Request.RouteValues.TryGetValue("id", out var id))
            {
                // Ideally this would use a repository to load the entity
                // This is just a placeholder for demonstration
                // entity = await _repository.GetByIdAsync(Convert.ToInt32(id));
            }
        }
        
        if (await _permissionService.CanPerformActionAsync(
            requirement.EntityName, 
            requirement.Action, 
            context.User, 
            entity ?? new object()))
        {
            context.Succeed(requirement);
        }
    }
} 
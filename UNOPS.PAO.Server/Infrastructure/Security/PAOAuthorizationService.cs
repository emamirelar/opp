namespace UNOPS.PAO.Server.Infrastructure.Security;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UNOPS.PAO.Identity.Context;
using UNOPS.PAO.Presentation.ContextPermissionHandlers;


public class PAOAuthorizationService : IAuthorizationService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IPAOExecutionContext executionContext;

    public PAOAuthorizationService(IServiceProvider serviceProvider, IPAOExecutionContext executionContext)
    {
        this.serviceProvider = serviceProvider;
        this.executionContext = executionContext;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
    {
        if (user == null)
        {
            return AuthorizationResult.Failed();
        }

        var permissions = executionContext.UserPermissions.Select(s => new Claim("permission", s.Name));

        // Convert claimsPrincipal.Claims to a list and add permissions
        var claims = user.Claims.ToList();
        claims.AddRange(permissions);
        var identity = new ClaimsIdentity(claims, "IAP-Header");
        user = new ClaimsPrincipal(identity);

        var context = new AuthorizationHandlerContext(requirements, user, resource);

        foreach (var requirement in requirements)
        {
            var handlers = serviceProvider.GetServices<IAuthorizationHandler>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(context);
            }
        }

        var handlerWrappers = serviceProvider.GetServices<IAuthorizationHandlerWrapper>();

        foreach (var handlerWrapper in handlerWrappers)
        {
            var handlers = handlerWrapper.GetType().GetProperties()
                .Where(p => typeof(IAuthorizationHandler).IsAssignableFrom(p.PropertyType) &&
                            p.PropertyType.GenericTypeArguments.Length == 2 &&
                            p.PropertyType.GenericTypeArguments[1].IsInstanceOfType(resource))
                .Select(p => (IAuthorizationHandler)p.GetValue(handlerWrapper));

            foreach (var handler in handlers)
            {
                await handler.HandleAsync(context!);
            }
        }

        return context.HasSucceeded ? AuthorizationResult.Success() : AuthorizationResult.Failed();
    }

    Task<AuthorizationResult> IAuthorizationService.AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
    {
        if (user == null)
        {
            return Task.FromResult(AuthorizationResult.Failed());
        }

        var policy = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>()
            .GetPolicyAsync(policyName).GetAwaiter().GetResult();

        if (policy == null)
        {
            return Task.FromResult(AuthorizationResult.Failed());
        }

        return AuthorizeAsync(user, resource, policy.Requirements);
    }
}

namespace UNOPS.PAO.UNOPSPresentation.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

public class EntityPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public EntityPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => 
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => 
        _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("EntityPermission:"))
        {
            var parts = policyName.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3)
            {
                var entityName = parts[1];
                var action = parts[2];
                
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new EntityPermissionRequirement(entityName, action))
                    .Build();
                    
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }
        
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
} 
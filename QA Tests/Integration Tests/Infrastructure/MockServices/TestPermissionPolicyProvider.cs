using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace UNOPS.PAO.IntegrationTests.Infrastructure.MockServices;

/// <summary>
/// Test implementation of IAuthorizationPolicyProvider that uses the "IAP" scheme
/// instead of IdentityConstants.ApplicationScheme (cookie auth).
/// 
/// In production, PermissionPolicyProvider creates policies requiring the
/// "Identity.Application" cookie scheme. Since integration tests authenticate
/// via the TestAuthHandler registered on the "IAP" scheme, the cookie scheme
/// has no valid ticket and the authorization middleware returns 401 before
/// the permission handler is ever invoked.
/// 
/// This provider fixes that by building all policies against the "IAP" scheme.
/// </summary>
public sealed class TestPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PermissionPrefix = "Permission";
    private const string EntityPermissionPrefix = "EntityPermission:";
    private const string TestScheme = "IAP";

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        var policy = new AuthorizationPolicyBuilder(TestScheme)
            .RequireAuthenticatedUser()
            .Build();
        return Task.FromResult(policy);
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return Task.FromResult<AuthorizationPolicy?>(null);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // Permission-based policies: always succeed in tests because
            // TestPAOExecutionContext returns all permissions
            var policy = new AuthorizationPolicyBuilder(TestScheme)
                .RequireAuthenticatedUser()
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        if (policyName.StartsWith(EntityPermissionPrefix, StringComparison.Ordinal))
        {
            // Entity permission policies: always succeed in tests because
            // TestPermissionService.CanPerformActionAsync returns true
            var policy = new AuthorizationPolicyBuilder(TestScheme)
                .RequireAuthenticatedUser()
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Unknown policy: require authentication on IAP scheme
        var defaultPolicy = new AuthorizationPolicyBuilder(TestScheme)
            .RequireAuthenticatedUser()
            .Build();
        return Task.FromResult<AuthorizationPolicy?>(defaultPolicy);
    }
}

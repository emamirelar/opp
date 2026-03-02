using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace UNOPS.PAO.IntegrationTests.Infrastructure.MockServices;

/// <summary>
/// Test implementation of IAuthorizationService that succeeds for all
/// authenticated users and fails for unauthenticated ones.
///
/// The production PAOAuthorizationService manually resolves IAuthorizationHandler
/// instances and only handles PermissionRequirement / EntityPermissionRequirement.
/// Standard requirements like DenyAnonymousAuthorizationRequirement have no
/// handler, causing all requests to get 403 Forbidden in tests.
///
/// This replacement implements a simple authenticated-or-not check, which is
/// correct for integration tests where TestPermissionService already returns
/// true for all permission queries.
/// </summary>
public sealed class TestAuthorizationService : IAuthorizationService
{
    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
    {
        if (user?.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }

        return Task.FromResult(AuthorizationResult.Failed());
    }

    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user, object? resource, string policyName)
    {
        if (user?.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }

        return Task.FromResult(AuthorizationResult.Failed());
    }
}

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UNOPS.PAO.IntegrationTests.Infrastructure;

/// <summary>
/// Test authentication handler for integration tests
/// Creates authenticated users with configurable claims
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Allow tests to simulate unauthenticated requests by sending
        // the "Test-NoAuth: true" header. CreateUnauthenticatedClient()
        // helpers add this header so that security tests still get 401.
        if (Request.Headers.TryGetValue("Test-NoAuth", out var noAuth) &&
            string.Equals(noAuth.FirstOrDefault(), "true", StringComparison.OrdinalIgnoreCase))
        {
            // QA-075 / DEF-063: Honor [AllowAnonymous] on the endpoint.
            // Currently blocked by DEF-063 (IAPVerificationMiddleware runs first),
            // but this is correct defense-in-depth for when DEF-063 is resolved.
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                var anonIdentity = new ClaimsIdentity();
                var anonPrincipal = new ClaimsPrincipal(anonIdentity);
                var anonTicket = new AuthenticationTicket(anonPrincipal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(anonTicket));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }
        
        var claims = new List<Claim>();

        // Extract claims from request headers (set by test client)
        foreach (var header in Request.Headers)
        {
            if (header.Key.StartsWith("Test-"))
            {
                var claimType = header.Key.Substring(5); // Remove "Test-" prefix
                var claimValue = header.Value.FirstOrDefault();
                
                if (!string.IsNullOrEmpty(claimValue))
                {
                    // Use ClaimTypes.Role for Test-Role so User.IsInRole() works (e.g. PARTNER_GLOB_ADMIN)
                    if (string.Equals(claimType, "Role", StringComparison.OrdinalIgnoreCase))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, claimValue));
                    }
                    else
                    {
                        claims.Add(new Claim(claimType, claimValue));
                    }
                }
            }
        }

        // Ensure we have basic claims
        if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "123"));
        }

        if (!claims.Any(c => c.Type == ClaimTypes.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, "Test User"));
        }
        
        if (!claims.Any(c => c.Type == ClaimTypes.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, "testuser@unops.org"));
        }
        
        if (!claims.Any(c => c.Type == ClaimTypes.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, "User"));
        }

        var identity = new ClaimsIdentity(claims, "IAP");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "IAP");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
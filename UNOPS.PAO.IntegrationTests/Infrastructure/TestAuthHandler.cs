using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
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
                    claims.Add(new Claim(claimType, claimValue));
                }
            }
        }

        // Ensure we have basic claims
        if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "test-user"));
        }

        if (!claims.Any(c => c.Type == ClaimTypes.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, "Test User"));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
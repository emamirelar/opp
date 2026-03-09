using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class AuthenticationBypassTest
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationBypassTest(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]

    [Trait("Defect", "DEF-107")]
    public async Task TestAuthenticationBypass_ReturnsNotFound_NotUnauthorized()
    {
        // Act - Try to access a non-existent endpoint
        var response = await _client.GetAsync("/api/nonexistent");
        
        // Assert - Should get 404 (not found) instead of 401 (unauthorized)
        // This proves authentication is bypassed
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    
    [Trait("Defect", "DEF-107")]
    public async Task TestRootEndpoint_Works()
    {
        // Act
        var response = await _client.GetAsync("/");
        
        // Assert - Should get some response (not 401)
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
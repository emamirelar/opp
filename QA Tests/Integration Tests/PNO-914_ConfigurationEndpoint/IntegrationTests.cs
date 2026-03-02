/**
 * @fileoverview PNO-914 ConfigurationController integration tests.
 * Full config flow, serialization, and environment-specific behavior.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Text.Json;
using FluentAssertions;
using UNOPS.PAO.Models.Shared;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.Configuration;

[Collection("PNO914Config_Configuration")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914-Configuration")]
public class ConfigurationIntegrationTests : ConfigurationTestFixtureBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    [Trait("TestId", "PNO914-CFG-INT-001")]
    public void Get_FullConfigFlow_ReturnsValidResponse()
    {
        var config = BuildFullConfig();

        var response = BuildResponse(config);

        response.Should().NotBeNull();
        response.Should().BeOfType<ConfigurationResponse>();
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-INT-002")]
    public void Get_ResponseSerializesCorrectly()
    {
        var config = BuildFullConfig(googleClientId: "cid", environment: "Test");

        var response = BuildResponse(config, "Test");

        var json = JsonSerializer.Serialize(response, JsonOptions);
        json.Should().Contain("cid");
        json.Should().Contain("Test");
        var deserialized = JsonSerializer.Deserialize<ConfigurationResponse>(json, JsonOptions);
        deserialized!.GoogleClientId.Should().Be("cid");
        deserialized.Environment.Should().Be("Test");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-INT-003")]
    public void Get_MultipleCalls_ReturnsSameConfig()
    {
        var config = BuildFullConfig();

        var r1 = BuildResponse(config);
        var r2 = BuildResponse(config);

        r1.Environment.Should().Be(r2.Environment);
        r1.GoogleClientId.Should().Be(r2.GoogleClientId);
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-INT-004")]
    public void Get_DifferentConfigs_ReflectedInResponse()
    {
        var config1 = BuildFullConfig(environment: "Env1");
        var config2 = BuildFullConfig(environment: "Env2");

        var r1 = BuildResponse(config1);
        var r2 = BuildResponse(config2);

        r1.Environment.Should().Be("Env1");
        r2.Environment.Should().Be("Env2");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-INT-005")]
    public void Get_EnvironmentSpecificSettings_ReturnsCorrectValues()
    {
        var config = BuildFullConfig(environment: "Production", defaultModel: "gemini-pro");

        var response = BuildResponse(config);

        response.Environment.Should().Be("Production");
        response.DefaultModel.Should().Be("gemini-pro");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-INT-006")]
    public void Get_DevEnvironmentConfig()
    {
        var config = BuildFullConfig(environment: "Development");

        var response = BuildResponse(config, "Development");

        response.Environment.Should().Be("Development");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-INT-007")]
    public void Get_StagingEnvironmentConfig()
    {
        var config = BuildFullConfig(environment: "Staging");

        var response = BuildResponse(config);

        response.Environment.Should().Be("Staging");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-INT-008")]
    public void Get_ProductionEnvironmentConfig()
    {
        var config = BuildFullConfig(environment: "Production");

        var response = BuildResponse(config);

        response.Environment.Should().Be("Production");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-INT-009")]
    public void Get_ConfigWithMissingOptionalFields_ReturnsPartialResponse()
    {
        var config = BuildConfigRoot(new Dictionary<string, string?>
        {
            ["GoogleAuthSettings:clientId"] = "cid",
            ["AppConfig:Environment"] = "Test"
        });

        var response = BuildResponse(config, "Test");

        response.GoogleClientId.Should().Be("cid");
        response.Environment.Should().Be("Test");
        response.ProjectId.Should().BeNull();
        response.Location.Should().BeNull();
        response.DefaultModel.Should().BeNull();
    }
}

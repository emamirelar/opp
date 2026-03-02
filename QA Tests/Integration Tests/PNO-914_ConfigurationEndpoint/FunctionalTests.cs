/**
 * @fileoverview PNO-914 ConfigurationController functional tests.
 * Business rules: fallback behavior, config precedence, section mapping.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models.Shared;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.Configuration;

[Collection("PNO914Config_Configuration")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914-Configuration")]
public class ConfigurationFunctionalTests : ConfigurationTestFixtureBase
{
    [Fact]
    [Trait("TestId", "PNO914-CFG-FUN-001")]
    public void Get_WithConfigValues_UsesConfigValues()
    {
        var config = BuildFullConfig(googleClientId: "config-client-id", googleApiKey: "config-api-key");

        var response = BuildResponse(config);

        response.GoogleClientId.Should().Be("config-client-id");
        response.GoogleApiKey.Should().Be("config-api-key");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-FUN-002")]
    public void Get_EnvironmentDefaultsToHostEnv()
    {
        var config = BuildConfigRoot(new Dictionary<string, string?>
        {
            ["GoogleAuthSettings:clientId"] = "cid",
            ["GoogleAuthSettings:apiKey"] = "key",
            ["AISettings:ProjectId"] = "p",
            ["AISettings:Location"] = "loc",
            ["AISettings:GeminiModelName"] = "model"
        });

        var response = BuildResponse(config, "CustomEnv");

        response.Environment.Should().Be("CustomEnv");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-FUN-003")]
    public void Get_ResponseHasAllRequiredProperties()
    {
        var config = BuildFullConfig();

        var response = BuildResponse(config);

        response.Should().NotBeNull();
        typeof(ConfigurationResponse).GetProperty("GoogleClientId")!.Should().NotBeNull();
        typeof(ConfigurationResponse).GetProperty("GoogleApiKey")!.Should().NotBeNull();
        typeof(ConfigurationResponse).GetProperty("Environment")!.Should().NotBeNull();
        typeof(ConfigurationResponse).GetProperty("ProjectId")!.Should().NotBeNull();
        typeof(ConfigurationResponse).GetProperty("Location")!.Should().NotBeNull();
        typeof(ConfigurationResponse).GetProperty("DefaultModel")!.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-FUN-004")]
    public void Get_ProjectIdFromAISettings()
    {
        var config = BuildFullConfig(projectId: "ai-project-only");

        var response = BuildResponse(config);

        response.ProjectId.Should().Be("ai-project-only");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-FUN-005")]
    public void Get_LocationFromAISettings()
    {
        var config = BuildFullConfig(location: "asia-northeast1");

        var response = BuildResponse(config);

        response.Location.Should().Be("asia-northeast1");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-FUN-006")]
    public void Get_DefaultModelFromAISettings()
    {
        var config = BuildFullConfig(defaultModel: "gemini-1.5-flash");

        var response = BuildResponse(config);

        response.DefaultModel.Should().Be("gemini-1.5-flash");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-FUN-007")]
    public void Get_AppConfigEnvironmentOverridesHostEnv()
    {
        var config = BuildFullConfig(environment: "Staging");

        var response = BuildResponse(config, "Development");

        response.Environment.Should().Be("Staging");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-FUN-008")]
    public void Get_ConfigurationResponseIsNotNull()
    {
        var config = BuildFullConfig();

        var response = BuildResponse(config);

        response.Should().NotBeNull();
        response.Should().BeOfType<ConfigurationResponse>();
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-FUN-009")]
    public void Get_GoogleClientIdFromConfig()
    {
        var config = BuildFullConfig(googleClientId: "my-google-client-id");

        var response = BuildResponse(config);

        response.GoogleClientId.Should().Be("my-google-client-id");
    }
}

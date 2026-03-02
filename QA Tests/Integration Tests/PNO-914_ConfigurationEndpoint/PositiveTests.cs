/**
 * @fileoverview PNO-914 ConfigurationController positive tests.
 * Happy path scenarios for configuration-to-response mapping.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models.Shared;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.Configuration;

[Collection("PNO914Config_Configuration")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914-Configuration")]
public class ConfigurationPositiveTests : ConfigurationTestFixtureBase
{
    [Fact]
    [Trait("TestId", "PNO914-CFG-POS-001")]
    public void Get_WithFullConfig_ReturnsConfigurationResponse()
    {
        var config = BuildFullConfig();

        var response = BuildResponse(config);

        response.Should().NotBeNull();
        response.Should().BeOfType<ConfigurationResponse>();
        response.GoogleClientId.Should().NotBeNullOrEmpty();
        response.Environment.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-POS-002")]
    public void Get_WithGoogleSettings_ReturnsPopulatedGoogleCredentials()
    {
        var config = BuildFullConfig(googleClientId: "my-client-id", googleApiKey: "my-api-key");

        var response = BuildResponse(config);

        response.GoogleClientId.Should().Be("my-client-id");
        response.GoogleApiKey.Should().Be("my-api-key");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-POS-003")]
    public void Get_WithAISettings_ReturnsPopulatedAIProperties()
    {
        var config = BuildFullConfig(
            projectId: "ai-project-123",
            location: "europe-west1",
            defaultModel: "gemini-2.0");

        var response = BuildResponse(config);

        response.ProjectId.Should().Be("ai-project-123");
        response.Location.Should().Be("europe-west1");
        response.DefaultModel.Should().Be("gemini-2.0");
    }
}

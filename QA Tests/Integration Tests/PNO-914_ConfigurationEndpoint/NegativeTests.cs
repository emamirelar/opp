/**
 * @fileoverview PNO-914 ConfigurationController negative tests.
 * Missing sections, null values, and expected fallback scenarios.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models.Shared;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.Configuration;

[Collection("PNO914Config_Configuration")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914-Configuration")]
public class ConfigurationNegativeTests : ConfigurationTestFixtureBase
{
    [Fact]
    [Trait("TestId", "PNO914-CFG-NEG-001")]
    public void Get_MissingGoogleAuthSettings_FallsBackToEmptyOrNull()
    {
        var config = BuildConfigRoot(new Dictionary<string, string?>
        {
            ["AppConfig:ProjectId"] = "proj",
            ["AppConfig:Environment"] = "Test",
            ["AISettings:ProjectId"] = "proj",
            ["AISettings:Location"] = "us",
            ["AISettings:GeminiModelName"] = "gemini"
        });

        var response = BuildResponse(config, "Test");

        response.Should().NotBeNull();
        (response.GoogleClientId == null || response.GoogleClientId == string.Empty).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-NEG-002")]
    public void Get_NullProjectId_DoesNotThrow()
    {
        var config = BuildFullConfig(projectId: null);

        var act = () => BuildResponse(config);

        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-NEG-003")]
    public void Get_EmptyClientId_ReturnsEmptyOrNull()
    {
        var config = BuildFullConfig(googleClientId: "");

        var response = BuildResponse(config);

        (response.GoogleClientId == null || response.GoogleClientId == "").Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-NEG-004")]
    public void Get_NullApiKey_ReturnsNull()
    {
        var config = BuildFullConfig(googleApiKey: null);

        var response = BuildResponse(config);

        response.GoogleApiKey.Should().BeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-NEG-005")]
    public void Get_MissingAppConfig_EnvironmentFallsBackToHostEnv()
    {
        var config = BuildConfigRoot(new Dictionary<string, string?>
        {
            ["GoogleAuthSettings:clientId"] = "cid",
            ["GoogleAuthSettings:apiKey"] = "key",
            ["AISettings:ProjectId"] = "p",
            ["AISettings:Location"] = "loc",
            ["AISettings:GeminiModelName"] = "model"
        });

        var response = BuildResponse(config, "Staging");

        response.Environment.Should().Be("Staging");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-NEG-006")]
    public void Get_MissingAISettings_ReturnsNullForAIProperties()
    {
        var config = BuildConfigRoot(new Dictionary<string, string?>
        {
            ["GoogleAuthSettings:clientId"] = "cid",
            ["GoogleAuthSettings:apiKey"] = "key",
            ["AppConfig:ProjectId"] = "p",
            ["AppConfig:Environment"] = "Test"
        });

        var response = BuildResponse(config, "Test");

        response.ProjectId.Should().BeNull();
        response.Location.Should().BeNull();
        response.DefaultModel.Should().BeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-051")]
    [Trait("TestId", "PNO914-CFG-NEG-007")]
    public void Get_EmptyEnvironmentName_UsesHostEnvironment()
    {
        var config = BuildFullConfig(environment: "");

        var response = BuildResponse(config, "Production");

        response.Environment.Should().Be("Production");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-NEG-008")]
    public void Get_AllOptionalSectionsNull_ReturnsMinimalResponse()
    {
        var config = BuildConfigRoot(new Dictionary<string, string?>());

        var response = BuildResponse(config, "Test");

        response.Should().NotBeNull();
        response.Environment.Should().Be("Test");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-NEG-009")]
    public void Get_NoConfigSections_FallsBackToHostEnvironment()
    {
        var config = BuildConfigRoot(new Dictionary<string, string?>
        {
            ["SomeUnrelated:Key"] = "value"
        });

        var response = BuildResponse(config, "FallbackEnv");

        response.Environment.Should().Be("FallbackEnv");
        response.GoogleClientId.Should().BeNull();
        response.GoogleApiKey.Should().BeNull();
    }
}

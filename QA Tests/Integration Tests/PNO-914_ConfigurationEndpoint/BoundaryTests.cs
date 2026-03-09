/**
 * @fileoverview PNO-914 ConfigurationController boundary tests.
 * Edge cases: empty strings, special chars, max lengths, whitespace.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.Configuration;

[Collection("PNO914Config_Configuration")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914-Configuration")]
public class ConfigurationBoundaryTests : ConfigurationTestFixtureBase
{
    [Fact]
    [Trait("TestId", "PNO914-CFG-BND-001")]
    public void Get_EmptyStringValues_ReturnsAsProvided()
    {
        var config = BuildFullConfig(googleClientId: "", googleApiKey: "", projectId: "", location: "", defaultModel: "");

        var response = BuildResponse(config);

        response.GoogleClientId.Should().BeEmpty();
        response.ProjectId.Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-BND-002")]
    public void Get_SpecialCharsInKeys_HandlesCorrectly()
    {
        var config = BuildFullConfig(projectId: "proj-with-dash_123", defaultModel: "gemini/1.5-pro");

        var response = BuildResponse(config);

        response.ProjectId.Should().Be("proj-with-dash_123");
        response.DefaultModel.Should().Be("gemini/1.5-pro");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-BND-003")]
    public void Get_VeryLongValues_ReturnsWithoutTruncation()
    {
        var longClientId = new string('a', 5000);
        var config = BuildFullConfig(googleClientId: longClientId);

        var response = BuildResponse(config);

        response.GoogleClientId.Should().HaveLength(5000);
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-BND-004")]
    public void Get_WhitespaceOnlyValues_ReturnsAsProvided()
    {
        var config = BuildFullConfig(googleClientId: "   ", environment: "\t");

        var response = BuildResponse(config);

        response.GoogleClientId.Should().Be("   ");
        response.Environment.Should().Be("\t");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-BND-005")]
    public void Get_UnicodeInEnvironmentName_ReturnsCorrectly()
    {
        var config = BuildFullConfig(environment: "Production-日本");

        var response = BuildResponse(config);

        response.Environment.Should().Be("Production-日本");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-BND-006")]
    public void Get_NestedNullSections_DoesNotThrow()
    {
        var config = BuildConfigRoot(new Dictionary<string, string?>
        {
            ["GoogleAuthSettings:clientId"] = "cid",
            ["AppConfig:Environment"] = "Test"
        });

        var act = () => BuildResponse(config, "Test");

        act.Should().NotThrow();
    }

    [Fact]

    [Trait("Defect", "DEF-051")]
    [Trait("TestId", "PNO914-CFG-BND-007")]
    public void Get_AllSectionsEmpty_ReturnsMinimalResponse()
    {
        var config = BuildConfigRoot(new Dictionary<string, string?>
        {
            ["GoogleAuthSettings:clientId"] = "",
            ["GoogleAuthSettings:apiKey"] = "",
            ["AppConfig:ProjectId"] = "",
            ["AppConfig:Environment"] = "",
            ["AISettings:ProjectId"] = "",
            ["AISettings:Location"] = "",
            ["AISettings:GeminiModelName"] = ""
        });

        var response = BuildResponse(config, "Dev");

        response.Environment.Should().Be("Dev");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-BND-008")]
    public void Get_SingleSectionMissing_OtherSectionsStillReturned()
    {
        var config = BuildConfigRoot(new Dictionary<string, string?>
        {
            ["GoogleAuthSettings:clientId"] = "cid",
            ["GoogleAuthSettings:apiKey"] = "key",
            ["AISettings:ProjectId"] = "p",
            ["AISettings:Location"] = "loc",
            ["AISettings:GeminiModelName"] = "model"
        });

        var response = BuildResponse(config, "Test");

        response.GoogleClientId.Should().Be("cid");
        response.ProjectId.Should().Be("p");
        response.Environment.Should().Be("Test");
    }

    [Fact]
    [Trait("TestId", "PNO914-CFG-BND-009")]
    public void Get_MaxLengthValues_HandlesCorrectly()
    {
        var longValue = new string('s', 256);
        var config = BuildFullConfig(googleClientId: longValue, googleApiKey: longValue);

        var act = () => BuildResponse(config);

        act.Should().NotThrow();
        var response = BuildResponse(config);
        response.GoogleClientId.Should().HaveLength(256);
    }
}

/**
 * @fileoverview PNO-914 ConfigurationController test fixture base.
 * Tests configuration-to-response mapping logic used by ConfigurationController.Get().
 * Since SystemConfigurationManager.GetConfiguration() is not virtual and cannot be mocked,
 * this fixture tests the configuration binding logic directly using IConfigurationRoot.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.IntegrationTests.PNO914.Configuration;

[CollectionDefinition("PNO914Config_Configuration")]
public class PNO914ConfigurationCollection { }

/// <summary>
/// Shared fixture base for PNO-914 ConfigurationController tests.
/// Replicates the controller's Get() config-binding logic for isolated testing.
/// </summary>
public abstract class ConfigurationTestFixtureBase
{
    /// <summary>Builds IConfigurationRoot from in-memory dictionary for testing.</summary>
    protected static IConfigurationRoot BuildConfigRoot(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.Where(x => x.Value != null).Select(x => new KeyValuePair<string, string?>(x.Key, x.Value)))
            .Build();
    }

    /// <summary>Builds full configuration with GoogleAuthSettings, AppConfig, AISettings.</summary>
    protected static IConfigurationRoot BuildFullConfig(
        string? googleClientId = "test-client-id",
        string? googleApiKey = "test-api-key",
        string? projectId = "test-project",
        string? environment = "Development",
        string? location = "us-central1",
        string? defaultModel = "gemini-1.5-pro")
    {
        var values = new Dictionary<string, string?>();
        if (googleClientId != null) values["GoogleAuthSettings:clientId"] = googleClientId;
        if (googleApiKey != null) values["GoogleAuthSettings:apiKey"] = googleApiKey;
        if (projectId != null) values["AppConfig:ProjectId"] = projectId;
        if (environment != null) values["AppConfig:Environment"] = environment;
        if (projectId != null) values["AISettings:ProjectId"] = projectId;
        if (location != null) values["AISettings:Location"] = location;
        if (defaultModel != null) values["AISettings:GeminiModelName"] = defaultModel;
        return BuildConfigRoot(values);
    }

    /// <summary>
    /// Replicates ConfigurationController.Get() config-to-response mapping.
    /// This is the exact logic from the production controller without needing
    /// SystemConfigurationManager (which has a non-virtual GetConfiguration()).
    /// </summary>
    protected static ConfigurationResponse BuildResponse(
        IConfigurationRoot config,
        string hostEnvironmentName = "Development")
    {
        var googleSettings = config.GetSection("GoogleAuthSettings");
        var appConfig = config.GetSection("AppConfig");
        var aiSettings = config.GetSection("AISettings");

        var googleClientId = googleSettings.GetSection("clientId").Value;
        var googleApiKey = googleSettings.GetSection("apiKey").Value;

        return new ConfigurationResponse
        {
            GoogleClientId = googleClientId,
            GoogleApiKey = googleApiKey,
            Environment = appConfig.GetSection("Environment").Value ?? hostEnvironmentName,
            ProjectId = aiSettings.GetSection("ProjectId").Value,
            Location = aiSettings.GetSection("Location").Value,
            DefaultModel = aiSettings.GetSection("GeminiModelName").Value
        };
    }
}

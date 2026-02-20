namespace UNOPS.PAO.Presentation.Controllers.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Models.Shared;

[Route("/")]
public class ConfigurationController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    
    public ConfigurationController(
        SystemConfigurationManager manager,
        ILogger<ConfigurationController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService,
        IWebHostEnvironment environment)
        : base(logger, authorizationService, userResolverService)
    {
        _configuration = manager.GetConfiguration();
        _environment = environment;
    }

    [HttpGet(APIDictionary.Configuration)]
    public ActionResult Get()
    {
        return HandleOperationAsync(async () => 
        {
            var googleSettings = _configuration.GetSection("GoogleAuthSettings");
            var appConfig = _configuration.GetSection("AppConfig");
            var aiSettings = _configuration.GetSection("AISettings");
            
            // Get Google credentials from Secret Manager
            string? googleClientId = null;
            string? googleApiKey = null;
            
            try
            {
                var clientIdSecretName = googleSettings.GetSection("ClientIdSecretName").Value;
                var apiKeySecretName = googleSettings.GetSection("ApiSecretName").Value;
                var projectId = appConfig.GetSection("ProjectId").Value;
                
                if (!string.IsNullOrEmpty(projectId))
                {
                    var secretProvider = new GoogleSecretManagerConfigurationProvider(projectId);
                    
                    // Get Client ID from secret
                    if (!string.IsNullOrEmpty(clientIdSecretName))
                    {
                        googleClientId = secretProvider.GetSecretVersion(clientIdSecretName, "latest");
                    }
                    
                    // Get API Key from secret
                    if (!string.IsNullOrEmpty(apiKeySecretName))
                    {
                        googleApiKey = secretProvider.GetSecretVersion(apiKeySecretName, "latest");
                    }
                }
                
                // Fallback to direct configuration if secret retrieval fails
                if (string.IsNullOrEmpty(googleClientId))
                {
                    googleClientId = googleSettings.GetSection("clientId").Value;
                }
                
                if (string.IsNullOrEmpty(googleApiKey))
                {
                    googleApiKey = googleSettings.GetSection("apiKey").Value;
                }
            }
            catch (Exception ex)
            {
                // Log the error and fallback to direct configuration
                _logger.LogWarning(ex, "Failed to retrieve Google credentials from Secret Manager, falling back to configuration");
                googleClientId = googleSettings.GetSection("clientId").Value;
                googleApiKey = googleSettings.GetSection("apiKey").Value;
            }
            
            var googleAnalyticsEnabled = bool.TryParse(
                appConfig.GetSection("GoogleAnalyticsEnabled").Value,
                out var gaEnabled) && gaEnabled;

            return await Task.FromResult(new ConfigurationResponse()
            {
                GoogleClientId = googleClientId,
                GoogleApiKey = googleApiKey,
                Environment = appConfig.GetSection("Environment").Value ?? _environment.EnvironmentName,
                ProjectId = aiSettings.GetSection("ProjectId").Value,
                Location = aiSettings.GetSection("Location").Value,
                DefaultModel = aiSettings.GetSection("GeminiModelName").Value,
                GoogleAnalyticsMeasurementId = appConfig.GetSection("GoogleAnalyticsMeasurementId").Value,
                GoogleAnalyticsEnabled = googleAnalyticsEnabled
            });
        }).Result;
    }
}
